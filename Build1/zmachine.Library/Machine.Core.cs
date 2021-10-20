namespace zmachine.Library
{
    using System.Diagnostics;
    using zmachine.Library.Enumerations;
    using zmachine.Library.Extensions;

    public partial class Machine
    {
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

        /// <summary>
        /// Terminates execution of the program.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public Machine Terminate(string? error = null)
        {
            DebugWrite("Terminate called");
            if (error is not null)
            {
                DebugWrite("Error: " + error);
            }
            finishProcessing = true;
            return this;
        }

        public uint unpackedAddress(ushort address)
        {
            return (uint)2 * address;
        }

        public Machine pushRoutineData(List<ushort> operands)
        {

            // First check if we've gone too deep into our call stack:
            if (callDepth >= StackDepth)
            {
                Debug.Assert(true, "Error: Call Stack Overflow"); //alert the user 
                finishProcessing = true;
                return this;
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
            return this;
        }

        public Machine popRoutineData(ushort returnVal)
        {
            if (callDepth == 0)
            {

                Debug.Assert(false, "Error: Call Stack Underrun"); // alert the user to the error.
                finishProcessing = true;
            }
            // Restore the stack to the previous value in callstack[callDepth]
            programCounter = callStack[callDepth].returnAddress;                       //Restore the PC to the previous value in callstack[callDepth]
            stackPointer = callStack[callDepth].stackFrameAddress;                   //Restore the sp to the previous value in callstack[callDepth]
            --callDepth;

            setVar(pc_getByte(), returnVal);                                // Set the return value. Calling a routine is a "store" function, so the next byte contains where to store the result.
            return this;
        }

        /// <summary>
        /// Find the PC start point in the header file and set PC 
        /// Set PC by looking at pc start byte in header file
        /// </summary>
        private Machine initializeProgramCounter()
        {
            ProgramCounter = memory.getWord(Memory.ADDR_INITIALPC);
            finishProcessing = false;
            return this;
        }

        public Machine ReadLex(List<ushort> operands)
        {
            if (operands.Count < 2)
            {
                throw new Exception("insufficient operands");
            }
            lex.read(operands[0], operands[1]);
            return this;
        }

        //In a V3 machine, there are 4 types of opcodes, as per http://inform-fiction.org/zmachine/standards/z1point1/sect14.html
        //      - 2OP
        //      - 1OP
        //      - 0OP
        //      - VAR

        /// <summary>
        /// Looks at pointer and returns instruction
        /// </summary>
        public Machine processInstruction()
        {
            if (Finished)
            {
                DebugWrite("Warning: processInstruction called after termination.");
                // throw new Exception();
            }

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
            return this;
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
    }
}
