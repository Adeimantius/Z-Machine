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

        private readonly Memory memory = new Memory(size: MemorySize);
        public Memory Memory => memory;
        private readonly Memory stack = new Memory(size: StackSize);
        private readonly ObjectTable objectTable;
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
            memory.load(programFilename);
            initializeProgramCounter();

            for (int i = 0; i < StackDepth; ++i)
            {
                callStack[i] = new RoutineCallState();
            }

            objectTable = new ObjectTable(memory);
            lex = new Lex(
                io: this.io,
                mem: memory);
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
            objectTable = new ObjectTable(memory);
            lex = new Lex(
                io: this.io,
                mem: memory,
                mp: initialState.lexMemoryPointer);
            State = initialState;
        }

        public ObjectTable ObjectTable => objectTable;

        public bool Finished => finishProcessing;

        public bool DebugEnabled => debug;

        public IIO IO => io;

        public Machine branch(bool condition)        // Check branch instructions
        {
            //             Debug.WriteLine("BRANCH: " + (condition ? "1" : "0"));

            byte branchInfo = pc_getByte();
            int branchOn = ((branchInfo >> 7) & 0x01);
            int branchSmall = ((branchInfo >> 6) & 0x1);
            uint offset = (uint)(branchInfo & 0x3f); // the "offset" is in the range 0 to 63, given in the bottom 6 bits.

            if (branchOn == 0)
            {
                condition = !condition;
            }

            if (branchSmall == 0)
            {                            // If bit 6 is clear the offset is a signed 14-bit number given
                if (offset > 31)
                {
                    offset -= 64;          // Convert offset to signed
                }

                offset <<= 8;              // Shift bits 8 left before adding next 6 to make a 14-bit number.

                offset += pc_getByte();  // in bits 0 to 5 of the first byte followed by all 8 of the second.
            }

            if (condition)
            {
                if (offset == 0 || offset == 1)
                {
                    popRoutineData((ushort)offset);
                }
                else
                {
                    programCounter += offset - 2;
                }
            }
            return this;
        }


        // Put a value onto the top of the stack.
        public Machine setVar(ushort variable, ushort value)
        {
            if (variable == 0)                   // Variable number $00 refers to the top of the stack
            {
                stack.setWord(stackPointer, value);        // Set value in stack
                stackPointer += 2;                         // Increment stack pointer by 2 (size of word)
            }
            else if (variable < 16)              //$01 to $0f mean the local variables of the current routine
            {
                callStack[callDepth].localVars[variable - 1] = value; // Set value in local variable table
            }
            else
            {
                //and $10 to $ff mean the global variables.
                uint address = (uint)(memory.getWord(Memory.ADDR_GLOBALS) + ((variable - 16) * 2));                 // Set value in global variable table (variable 16 -> index 0)
                memory.setWord(address, value);
            }

            return this;
        }

        // Get a value from the top of the stack.
        public ushort getVar(ushort variable)
        {
            //            Debug.WriteLine("VAR[" + variable + "]");

            ushort value;

            if (variable == 0)
            {
                stackPointer -= 2;
                value = stack.getWord(stackPointer);                     // get value from stack;
            }
            else if (variable < 16)
            {
                value = callStack[callDepth].localVars[variable - 1];        // get value from local variable table
            }
            else
            {
                uint address = (uint)(memory.getWord(Memory.ADDR_GLOBALS) + ((variable - 16) * 2));        // Move by a variable number of steps through the table of global variables.
                value = memory.getWord(address);
                // Debug.WriteLine("VAR[" + variable + "] = " + value);

            }


            return value;

        }
    } // end Machine
} // end namespace
