namespace zmachine.Library
{
    using System;
    using zmachine.Library.Enumerations;

    /// <summary>
    /// This class moves through the input file and extracts bytes to deconstruct instructions in the code
    /// </summary>
    public partial class Machine
    {
        public const int StackDepth = 128;
        /// <summary>
        /// Stack of size 32768 (can be larger, but this should be fine)
        /// </summary>
        public const int StackSize = 1024 * 32;

        /// <summary>
        /// Z-Machine implementation level
        /// </summary>
        public const int CurrentVersion = 3;

        public static readonly int[] MemorySizeByVersion = new int[]
        {
            0,        // V0    - does not exist 
            1024*128, // V1-V3 - 128k
            1024*128, // V1-V3 - 128k
            1024*128, // V1-V3 - 128k
            1024*256, // V4-V5 - 256K
            1024*256, // V4-V5 - 256K
            1024*576, // V6-V7 - 576K
            1024*576, // V6-V7 - 576K
            1024*512, // V8    - 512K
        };


        public readonly Memory Memory;
        private readonly Memory stack;
        public readonly ObjectTable ObjectTable;
        private readonly IIO io;
        private readonly Lex Lex;

        private readonly bool debug = false;

        /// <summary>
        /// Program Counter to step through memory
        /// </summary>
        public uint ProgramCounter = 0;

        /// <summary>
        /// Number of instructions since power on
        /// </summary>
        public ulong InstructionCounter;

        /// <summary>
        /// Program Counter at the beginning of executing the instruction
        /// </summary>
        private uint pcStart = 0;

        /// <summary>
        /// Flag to say "finish processing. We're done".
        /// </summary>
        private bool finishProcessing = false;

        /// <summary>
        /// Index to current stack record
        /// </summary>
        private uint stackPointer = 0;

        /// <summary>
        /// Current call depth
        /// </summary>
        private uint callDepth = 0;

        private const int MaximumRestoreStates = 10;
        private readonly LinkedList<CPUState> CPUStates;

        /// <summary>
        /// We could use a "Stack" here, as well, but we have lots of memory. According to the spec, there could be up to 90. But 128 is nice.
        /// </summary>
        private readonly RoutineCallState[] callStack = new RoutineCallState[StackDepth];

        /// <summary>
        /// Breakpoint types besides complete/terminate to break for
        /// </summary>
        private readonly Dictionary<Enumerations.BreakpointType, BreakpointAction> BreakFor;

        /// <summary>
        /// Breakpoints reached and their instruction number
        /// </summary>
        private readonly List<(ulong instruction, Enumerations.BreakpointType breakpointType)> breakpointsReached;

        /// <summary>
        /// Do not perform non-terminating breakpoints unless after the given instruction number
        /// </summary>
        public ulong BreakAfter;

        /// <summary>
        /// Fixed list of breakpoint types that must be allowed to break and are not filtered.
        /// </summary>
        public static readonly BreakpointType[] EndProgramBreakpoints = { BreakpointType.Complete, BreakpointType.Terminate };

        public readonly List<Enum> OpcodeBreakpoints; 

        /// <summary>
        /// Class constructor : Loads in data from file and sets Program Counter
        /// </summary>
        /// <param name="io"></param>
        /// <param name="programFilename"></param>
        public Machine(IIO io, string programFilename, Dictionary<BreakpointType, BreakpointAction>? breakpointTypes = null)
        {
            this.Memory = new Memory(size: MemorySizeByVersion[CurrentVersion]);
            this.stack = new Memory(size: StackSize);
            this.CPUStates = new LinkedList<CPUState>();
            this.OpcodeBreakpoints = new List<Enum>();
            this.BreakAfter = 0;
            this.BreakFor = breakpointTypes is not null ? new Dictionary<Enumerations.BreakpointType, BreakpointAction>(breakpointTypes) : new Dictionary<BreakpointType, BreakpointAction> { };
            this.breakpointsReached = new List<(ulong, Enumerations.BreakpointType)> { };
            this.io = io;
            this.finishProcessing = false;
            this.Memory.load(programFilename);
            this.initializeProgramCounter();

            for (int i = 0; i < StackDepth; ++i)
            {
                this.callStack[i] = new RoutineCallState();
            }

            this.ObjectTable = new ObjectTable(ref this.Memory);
            this.Lex = new Lex(
                io: this.io,
                mem: ref this.Memory);
        }

        /// <summary>
        /// Class constructor : Initialized memory
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
            this.Memory = new Memory(size: MemorySizeByVersion[CurrentVersion]);
            this.stack = new Memory(size: StackSize);
            this.CPUStates = new LinkedList<CPUState>();
            this.OpcodeBreakpoints = new List<Enum>();
            this.BreakAfter = 0;
            this.BreakFor = breakpointTypes is not null ? new Dictionary<Enumerations.BreakpointType, BreakpointAction>(breakpointTypes) : new Dictionary<BreakpointType, BreakpointAction> { };
            this.breakpointsReached = new List<(ulong, Enumerations.BreakpointType)> { };
            this.io = io;
            this.finishProcessing = false;
            this.ObjectTable = new ObjectTable(ref this.Memory);
            this.Lex = new Lex(
                io: this.io,
                mem: ref this.Memory,
                mp: initialState.lexMemoryPointer);
            this.State = initialState;
        }

        /// <summary>
        /// readonly handle to access pcStart
        /// </summary>
        public uint ProgramCounterStart => this.pcStart;



        public IEnumerable<(ulong instruction, Enumerations.BreakpointType breakpointType)> BreakpointsReached => this.breakpointsReached.ToArray();

        public bool Finished => this.finishProcessing;

        public bool DebugEnabled => this.debug;

        public IIO IO => this.io;

    } // end Machine
} // end namespace
