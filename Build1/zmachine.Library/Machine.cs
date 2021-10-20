
namespace zmachine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using static zmachine.Library.Extensions.MachineOpcodeExtensions;

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
        private bool finish = false;
        private uint stackPointer = 0;
        private uint callDepth = 0;
        /// <summary>
        /// We could use a "Stack" here, as well, but we have lots of memory. According to the spec, there could be up to 90. But 128 is nice.
        /// </summary>
        private readonly RoutineCallState[] callStack = new RoutineCallState[StackDepth];
        // Class constructor : Loads in data from file and sets Program Counter
        public Machine(IIO io, string programFilename)
        {
            this.io = io;
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

        public Machine(IIO io, CPUState initialState)
        {
            if (initialState is null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }
            this.io = io;
            objectTable = new ObjectTable(memory);
            lex = new Lex(
                io: this.io,
                mem: memory,
                mp: initialState.lexMemoryPointer);
            State = initialState;
        }

        public void Terminate(string? error = null)
        {
            DebugWrite("Terminate called");
            if (error is not null)
            {
                DebugWrite("Error: " + error);
            }
            finish = true;
        }

        public ObjectTable ObjectTable => objectTable;

        public bool Finished => finish;

        public bool DebugEnabled => debug;

        public Machine DebugWrite(params object[] args)
        {
            if (DebugEnabled)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    Debug.Write(args[i] + " ");
                }
                Debug.WriteLine("");
            }
            return this;
        }

        public IIO IO => io;

        public uint unpackedAddress(ushort address)
        {
            return (uint)2 * address;
        }

        public void pushRoutineData(List<ushort> operands)
        {

            // First check if we've gone too deep into our call stack:
            if (callDepth >= StackDepth)
            {
                Debug.Assert(true, "Error: Call Stack Overflow"); //alert the user 
                finish = true;
                return;
            }

            ++callDepth;
            callStack[callDepth].returnAddress = programCounter;            // Store return address @ current position of pc upon entering routine
            programCounter = unpackedAddress(operands[0]);                  // Set the PC to the routine address.
            callStack[callDepth].stackFrameAddress = stackPointer;                       // Store stack pointer address.

            //SPEC: 6.4.4 When a routine is called, its local variables are created with initial values taken from the routine header (Versions 1 to 4). 
            //            Next, the arguments are written into the local variables (argument 1 into local 1 and so on).
            byte numLocals = pc_getByte();
            for (int i = 0; i < numLocals; i++)
            {
                callStack[callDepth].localVars[i] = pc_getWord();   // Local variables and the stack are conserved as the routine executes.
            }

            //Those remaining (up to three) operands will stomp over the first (up to three) values in localVars
            for (int i = 1; i < operands.Count; i++)
            {
                callStack[callDepth].localVars[i - 1] = operands[i]; ////Loop over the remaining operands, putting them into the appropriate spots in localVars
            }

            numLocals = (byte)System.Math.Max(operands.Count - 1, numLocals);
            callStack[callDepth].numLocalVars = numLocals;
        }

        public void popRoutineData(ushort returnVal)
        {
            if (callDepth == 0)
            {

                Debug.Assert(false, "Error: Call Stack Underrun"); // alert the user to the error.
                finish = true;
            }
            // Restore the stack to the previous value in callstack[callDepth]
            programCounter = callStack[callDepth].returnAddress;                       //Restore the PC to the previous value in callstack[callDepth]
            stackPointer = callStack[callDepth].stackFrameAddress;                   //Restore the sp to the previous value in callstack[callDepth]
            --callDepth;

            setVar(pc_getByte(), returnVal);                                // Set the return value. Calling a routine is a "store" function, so the next byte contains where to store the result.
        }

        /// <summary>
        /// Find the PC start point in the header file and set PC 
        /// Set PC by looking at pc start byte in header file
        /// </summary>
        private void initializeProgramCounter()
        {
            ProgramCounter = memory.getWord(Memory.ADDR_INITIALPC);
        }

        public void ReadLex(List<ushort> operands)
        {
            if (operands.Count < 2)
            {
                throw new Exception("insufficient operands");
            }
            lex.read(operands[0], operands[1]);
        }

        //In a V3 machine, there are 4 types of opcodes, as per http://inform-fiction.org/zmachine/standards/z1point1/sect14.html
        //      - 2OP
        //      - 1OP
        //      - 0OP
        //      - VAR

        /// <summary>
        /// Looks at pointer and returns instruction
        /// </summary>
        public void processInstruction()
        {
            pcStart = programCounter;
            ushort opcode;                              // Initialize variables
            int instruction = pc_getByte();
            //            Debug.WriteLine(pcStart.ToString("X4") + " : 0x" + instruction.ToString("X2") + " (" + instruction + ")");

            List<ushort> operandList = new List<ushort>();

            // This switch statement looks at the PC and reads in two bits to see the instruction form
            int form = (byte)(instruction >> 6);
            switch (form)
            {
                case 0x00:                              // Define long-form instruction
                case 0x01:
                    {
                        //                    Debug.WriteLine("\tInstruction Form : Long");
                        opcode = (ushort)(instruction & 0x1f);
                        int opTypeA = instruction >> 6 & 1;//6th bit
                        int opTypeB = instruction >> 5 & 1;//5th bit
                                                           // Value of 0 means variable, value of 1 means small

                        // load both operands of long form instruction
                        operandList.Add(loadOperand(opTypeA == 0 ? OperandType.Small : OperandType.Var));
                        operandList.Add(loadOperand(opTypeB == 0 ? OperandType.Small : OperandType.Var));
                        this.process2OP(opcode, operandList);
                    }
                    break;

                case 0x02:                              // Define short-form instruction
                    {
                        //                    Debug.WriteLine("\tInstruction Form : Short");
                        opcode = (ushort)(instruction & 0xf);
                        int opType = (byte)((instruction >> 4) & 3);        // Grab bits 4 and 5.

                        if (opType == (int)OperandType.Omit)
                        {
                            this.process0OP(opcode);
                        }
                        else
                        {
                            ushort op1 = loadOperand((OperandType)opType);
                            this.process1OP(opcode, op1);
                        }
                    }
                    break;


                case 0x03:                             // Define Variable form instruction
                    {
                        //                        Debug.WriteLine("\tInstruction Form : Variable");
                        opcode = (ushort)(instruction & 0x1f);          // Grab bottom 5 bits. 
                        int type = ((instruction >> 5) & 1);            // Store type of opcode (2OP, VAR)          
                        byte operandTypes = pc_getByte();

                        for (int i = 0; i < 4; i++)
                        {
                            OperandType opType = (OperandType)((operandTypes >> 6) & 3);  // Grab top 2 bits

                            ushort op = loadOperand(opType);
                            if (opType != OperandType.Omit)
                            {
                                operandList.Add(op);
                            }

                            operandTypes <<= 2;   // Shift two to the left
                        }


                        if (type == 0)
                        {
                            this.process2OP(opcode, operandList);           // Need to pass in two operands?
                        }
                        else
                        {
                            this.processVAR(opcode, operandList);

                        }
                    }
                    break;
                default:
                    break;

                    //return (byte) instruction;                 
            }
        }// end processInstruction

        public byte pc_getByte()
        {
            byte next = memory.getByte(programCounter);
            programCounter++;
            return next;
        }
        public ushort pc_getWord()
        {
            ushort next = memory.getWord(programCounter);
            programCounter += 2;
            return next;
        }

        public ushort loadOperand(OperandType optype)
        {
            ushort operand;

            switch (optype)
            {
                case OperandType.Large:
                    {
                        operand = pc_getWord();
                        return operand;
                    }

                case OperandType.Small:
                    {
                        operand = pc_getByte();
                        return operand;
                    }
                case OperandType.Var:
                    {
                        operand = getVar(pc_getByte());
                        return operand;
                    }
                default:                    // OperandType.Omit:
                    {
                        return 0;
                    }
            }
        }

        /*
        private void process2OP(int opcode, List<ushort> operands)
        {
            string? callingFunctionName = new StackTrace().GetFrame(1)!.GetMethod()!.Name;

            Action op = OP2opcodes[opcode];
            if (debug)
            {
                Debug.Write(pcStart.ToString("X4") + "  " + stateString() + " : [2OP/" + opcode.ToString("X2") + "] " + callingFunctionName);
                foreach (ushort v in operands)
                {
                    Debug.Write(" " + v);
                }

                Debug.WriteLine("");
            }
            op.DynamicInvoke(operands);
        }

        void process1OP(int opcode, ushort operand1)
        {
            string? callingFunctionName = new StackTrace().GetFrame(1)!.GetMethod()!.Name;
            Action op = OP1opcodes[opcode];
            if (debug)
            {
                Debug.WriteLine(pcStart.ToString("X4") + "  " + stateString() + " : [1OP/" + opcode.ToString("X2") + "] " + callingFunctionName + " " + operand1.ToString());
            }

            op.DynamicInvoke(operand1);
        }
        void process0OP(int opcode)
        {
            string? callingFunctionName = new StackTrace().GetFrame(1)!.GetMethod()!.Name;
            Action op = OP0opcodes[opcode];
            if (debug)
            {
                Debug.WriteLine(pcStart.ToString("X4") + "  " + stateString() + " : [0OP/" + opcode.ToString("X2") + "] " + callingFunctionName);
            }

            op.Invoke();
        }
        void processVAR(int opcode, List<ushort> operands)
        {
            string? callingFunctionName = new StackTrace().GetFrame(1)!.GetMethod()!.Name;
            Action op = VARopcodes[opcode];
            if (debug)
            {
                Debug.Write(pcStart.ToString("X4") + "  " + stateString() + " : [VAR/" + opcode.ToString("X2") + "] " + callingFunctionName);
                foreach (ushort v in operands)
                {
                    Debug.Write(" " + v);
                }

                Debug.WriteLine("");
            }
            op.DynamicInvoke(operands);
        }
        */

        public void branch(bool condition)        // Check branch instructions
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
