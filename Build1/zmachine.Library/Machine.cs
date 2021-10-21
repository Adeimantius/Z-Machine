namespace zmachine.Library
{
    using System;

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
        private readonly Lex lex;

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

        /// <summary>
        /// Class constructor : Loads in data from file and sets Program Counter
        /// </summary>
        /// <param name="io"></param>
        /// <param name="programFilename"></param>
        public Machine(IIO io, string programFilename)
        {
            this.io = io;
            finishProcessing = false;
            Memory.load(programFilename);
            initializeProgramCounter();

            for (int i = 0; i < StackDepth; ++i)
            {
                callStack[i] = new RoutineCallState();
            }

            ObjectTable = new ObjectTable(ref Memory);
            lex = new Lex(
                io: this.io,
                mem: ref Memory);
        }

        /// <summary>
        /// Class constructor : Initialized memory
        /// </summary>
        /// <param name="io"></param>
        /// <param name="initialState"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Machine(IIO io, CPUState initialState)
        {
            if (initialState is null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }
            this.io = io;
            finishProcessing = false;
            ObjectTable = new ObjectTable(ref Memory);
            lex = new Lex(
                io: this.io,
                mem: ref Memory,
                mp: initialState.lexMemoryPointer);
            State = initialState;
        }

        public bool Finished => finishProcessing;

        public bool DebugEnabled => debug;

        public IIO IO => io;

    } // end Machine
} // end namespace
