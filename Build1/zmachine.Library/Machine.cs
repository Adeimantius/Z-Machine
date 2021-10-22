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
        public const int MemorySize = 1024 * 128;
        /// <summary>
        /// Stack of size 32768 (can be larger, but this should be fine)
        /// </summary>
        public const int StackSize = 1024 * 32;

        public readonly Memory Memory = new Memory(size: MemorySize);
        private readonly Memory stack = new Memory(size: StackSize);
        public readonly ObjectTable ObjectTable;
        private readonly IIO io;
        private readonly Lex Lex;

        private readonly bool debug = false;

        /// <summary>
        /// Program Counter to step through memory
        /// </summary>
        private uint programCounter = 0;

        public uint ProgramCounter
        {
            get => programCounter;
            set => programCounter = value;
        }

        /// <summary>
        /// Number of instructions since power on
        /// </summary>
        public ulong InstructionCounter;

        /// <summary>
        /// Program Counter at the beginning of executing the instruction
        /// </summary>
        private uint pcStart = 0;

        public uint ProgramCounterStart => pcStart;

        /// <summary>
        /// Flag to say "finish processing. We're done".
        /// </summary>
        private bool finishProcessing = false;
        private uint stackPointer = 0;
        private uint callDepth = 0;
        /// <summary>
        /// We could use a "Stack" here, as well, but we have lots of memory. According to the spec, there could be up to 90. But 128 is nice.
        /// </summary>
        private readonly RoutineCallState[] callStack = new RoutineCallState[StackDepth];

        private List<Enumerations.BreakpointType> BreakFor;

        private List<(ulong instruction, Enumerations.BreakpointType breakpointType)> breakpointsReached;

        public IEnumerable<(ulong instruction, Enumerations.BreakpointType breakpointType)> BreakpointsReached => breakpointsReached.ToArray();

        public ulong BreakAfter = 0;

        public bool ShouldBreak
        {
            get
            {
                return (this.InstructionCounter > this.BreakAfter) && this.breakpointsReached.Any();
            }
        }

        public bool ShouldBreakFor(BreakpointType breakpointType)
        {
            if ((breakpointType == BreakpointType.Complete) || (breakpointType == BreakpointType.Terminate))
            {
                // always break for termination
                return true;
            }
            return (this.InstructionCounter > this.BreakAfter) && 
                this.BreakFor.Contains(breakpointType);
        }

        public bool BreakpointReached(BreakpointType breakpointType) =>
            this.breakpointsReached
                .Select(t => t.breakpointType)
                .Contains(breakpointType);
        
        public Machine AddBreakpoint(BreakpointType breakpointType, ulong? afterInstruction = null)
        {
            if (!this.BreakFor.Contains(breakpointType))
            {
                this.BreakFor.Add(breakpointType);
            }
            if (afterInstruction is not null && (this.BreakAfter < afterInstruction.Value))
            {
                this.BreakAfter = afterInstruction.Value;
            }
            return this;
        }

        public Machine RemoveBreakpoint(BreakpointType breakpointType)
        {
            if (this.BreakFor.Contains(breakpointType))
            {
                this.BreakFor.Remove(breakpointType);
            }
            return this;
        }

        public bool Break(BreakpointType breakpointType)
        {
            if (!this.ShouldBreakFor(breakpointType))
            {
                return false;
            }
            this.breakpointsReached.Add((
                instruction: this.InstructionCounter,
                breakpointType: breakpointType));
            return true;
        }

        /// <summary>
        /// Class constructor : Loads in data from file and sets Program Counter
        /// </summary>
        /// <param name="io"></param>
        /// <param name="programFilename"></param>
        public Machine(IIO io, string programFilename, IEnumerable<BreakpointType>? breakpointTypes = null)
        {
            this.BreakFor = breakpointTypes is not null ? new List<Enumerations.BreakpointType>(breakpointTypes) : new List<Enumerations.BreakpointType> {  };
            this.breakpointsReached = new List<(ulong, Enumerations.BreakpointType)> { };
            this.io = io;
            this.finishProcessing = false;
            Memory.load(programFilename);
            initializeProgramCounter();

            for (int i = 0; i < StackDepth; ++i)
            {
                callStack[i] = new RoutineCallState();
            }

            this.ObjectTable = new ObjectTable(ref Memory);
            this.Lex = new Lex(
                io: this.io,
                mem: ref Memory);
        }

        /// <summary>
        /// Class constructor : Initialized memory
        /// </summary>
        /// <param name="io"></param>
        /// <param name="initialState"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Machine(IIO io, CPUState initialState, IEnumerable<BreakpointType>? breakpointTypes = null)
        {
            if (initialState is null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }
            this.BreakFor = breakpointTypes is not null ? new List<Enumerations.BreakpointType>(breakpointTypes) : new List<Enumerations.BreakpointType> { };
            this.breakpointsReached = new List<(ulong, Enumerations.BreakpointType)> { };
            this.io = io;
            this.finishProcessing = false;
            this.ObjectTable = new ObjectTable(ref Memory);
            this.Lex = new Lex(
                io: this.io,
                mem: ref Memory,
                mp: initialState.lexMemoryPointer);
            this.State = initialState;
        }

        public bool Finished => finishProcessing;

        public bool DebugEnabled => debug;

        public IIO IO => io;

    } // end Machine
} // end namespace
