using zmachine.Library.Enumerations;
using zmachine.Library.Interfaces;

namespace zmachine.Library.Models;

/// <summary>
///     This class moves through the input file and extracts bytes to deconstruct instructions in the code
/// </summary>
public partial class Machine
{
    public static bool DEBUG_ASSERT_DISABLED = false;
    public const int StackDepth = 128;

    /// <summary>
    ///     Stack of size 32768 (can be larger, but this should be fine)
    /// </summary>
    public const int StackSize = 1024 * 32;

    /// <summary>
    ///     Z-Machine implementation level
    /// </summary>
    public const int CurrentVersion = 3;

    private const int MaximumRestoreStates = 10;

    public static readonly int[] MemorySizeByVersion =
    {
        0, // V0    - does not exist 
        1024 * 128, // V1-V3 - 128k
        1024 * 128, // V1-V3 - 128k
        1024 * 128, // V1-V3 - 128k
        1024 * 256, // V4-V5 - 256K
        1024 * 256, // V4-V5 - 256K
        1024 * 576, // V6-V7 - 576K
        1024 * 576, // V6-V7 - 576K
        1024 * 512 // V8    - 512K
    };

    /// <summary>
    ///     Fixed list of breakpoint types that must be allowed to break and are not filtered.
    /// </summary>
    public static readonly BreakpointType[] EndProgramBreakpoints =
    {
        BreakpointType.Complete,
        BreakpointType.DivisionByZero,
        BreakpointType.StackUnderrun,
        BreakpointType.StackOverflow,
        BreakpointType.Error,
        BreakpointType.Terminate,
        BreakpointType.Unimplemented
    };

    /// <summary>
    ///     Breakpoint types besides complete/terminate to break for
    /// </summary>
    private readonly Dictionary<BreakpointType, BreakpointAction> BreakFor;

    /// <summary>
    ///     Breakpoints reached and their instruction number
    /// </summary>
    private readonly List<(ulong instruction, BreakpointType breakpointType)> breakpointsReached;

    /// <summary>
    ///     We could use a "Stack" here, as well, but we have lots of memory. According to the spec, there could be up to 90.
    ///     But 128 is nice.
    /// </summary>
    private readonly RoutineCallState[] callStack = new RoutineCallState[StackDepth];

    /// <summary>
    /// Index 0 is special. Depth >= 1 is the actual stack.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public RoutineCallState CallStackAt(int index) => callStack.Length > index ? this.CallStack.ElementAt(index) : throw new ArgumentException(nameof(index));
    public IEnumerable<RoutineCallState> CallStack => (RoutineCallState[])callStack.Clone();

    private readonly LinkedList<CPUState> CPUStates;

    public readonly IIO IO;
    private readonly Lex Lex;


    public readonly Memory Memory;
    public readonly ObjectTable ObjectTable;

    public readonly List<Enum> OpcodeBreakpoints;
    private readonly Memory Stack;

    /// <summary>
    ///     Do not perform non-terminating breakpoints unless after the given instruction number
    /// </summary>
    public ulong BreakAfter;

    /// <summary>
    ///     Current call depth
    /// </summary>
    private uint callDepth;

    public uint CallDepth => callDepth;

    /// <summary>
    ///     Number of instructions since power on
    /// </summary>
    public ulong InstructionCounter;

    /// <summary>
    ///     Program Counter at the beginning of executing the instruction
    /// </summary>
    private uint pcStart;

    /// <summary>
    ///     Program Counter to step through memory
    /// </summary>
    public uint ProgramCounter = 0;

    /// <summary>
    ///     Index to current stack record
    /// </summary>
    public uint StackPointer;

    /// <summary>
    ///     Class constructor : Loads in data from file and sets Program Counter
    /// </summary>
    /// <param name="io"></param>
    /// <param name="programFilename"></param>
    public Machine(IIO io, string programFilename, Dictionary<BreakpointType, BreakpointAction>? breakpointTypes = null)
    {
        this.Memory = new Memory(size: MemorySizeByVersion[CurrentVersion], contentsFilename: programFilename);
        this.Stack = new Memory(size: StackSize, contents: null);
        this.CPUStates = new LinkedList<CPUState>();
        this.OpcodeBreakpoints = new List<Enum>();
        this.BreakAfter = 0;
        this.BreakFor = breakpointTypes is not null
            ? new Dictionary<BreakpointType, BreakpointAction>(breakpointTypes)
            : new Dictionary<BreakpointType, BreakpointAction>();
        this.breakpointsReached = new List<(ulong, BreakpointType)>();
        this.IO = io;
        this.Finished = false;
        this.initializeProgramCounter();

        for (int i = 0; i < StackDepth; ++i)
        {
            this.callStack[i] = new RoutineCallState();
        }

        this.ObjectTable = new ObjectTable(this);
        this.Lex = new Lex(
            this);
        if (this.IO is null)
        {
            throw new Exception(nameof(this.IO));
        }
    }

    /// <summary>
    ///     Class constructor : Initialized memory
    /// </summary>
    /// <param name="io"></param>
    /// <param name="initialState"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Machine(IIO io, CPUState initialState, Dictionary<BreakpointType, BreakpointAction>? breakpointTypes = null)
    {
        if (initialState is null)
        {
            throw new ArgumentNullException(nameof(initialState));
        }

        this.Memory = new Memory(size: MemorySizeByVersion[CurrentVersion], contents: null);
        this.Stack = new Memory(size: StackSize, contents: null);
        this.CPUStates = new LinkedList<CPUState>();
        this.OpcodeBreakpoints = new List<Enum>();
        this.BreakAfter = 0;
        this.BreakFor = breakpointTypes is not null
            ? new Dictionary<BreakpointType, BreakpointAction>(breakpointTypes)
            : new Dictionary<BreakpointType, BreakpointAction>();
        this.breakpointsReached = new List<(ulong, BreakpointType)>();
        this.IO = io;
        this.Finished = false;
        this.ObjectTable = new ObjectTable(this);
        this.Lex = new Lex(
            this,
            initialState.lexMemoryPointer);
        this.State = initialState;
        if (this.IO is null)
        {
            throw new Exception(nameof(this.IO));
        }
    }

    /// <summary>
    ///     readonly handle to access pcStart
    /// </summary>
    public uint ProgramCounterStart => this.pcStart;


    public IEnumerable<(ulong instruction, BreakpointType breakpointType)> BreakpointsReached =>
        this.breakpointsReached.ToArray();

    private bool finished;

    /// <summary>
    ///     Flag to say "finish processing. We're done".
    /// </summary>
    public bool Finished
    {
        get
        {
            return this.finished;
        }

        private set
        {
            if (this.finished == false)
            {
                this.finished = value;
            } else
            {
                throw new Exception();
            }
        }
    }

    public bool DebugEnabled { get; } = false;
} // end Machine
  // end namespace