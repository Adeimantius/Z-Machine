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
        public BreakpointType Terminate(string? error = null)
        {
            DebugWrite("Terminate called");
            if (error is not null)
            {
                DebugWrite("Error: " + error);
            }
            finishProcessing = true;
            this.Break(BreakpointType.Terminate);
            return BreakpointType.Terminate;
        }

        public uint unpackedAddress(ushort address)
        {
            return (uint)2 * address;
        }


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
                uint address = (uint)(Memory.getWord(Memory.ADDR_GLOBALS) + ((variable - 16) * 2));                 // Set value in global variable table (variable 16 -> index 0)
                Memory.setWord(address, value);
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
                uint address = (uint)(Memory.getWord(Memory.ADDR_GLOBALS) + ((variable - 16) * 2));        // Move by a variable number of steps through the table of global variables.
                value = Memory.getWord(address);
                // Debug.WriteLine("VAR[" + variable + "] = " + value);

            }


            return value;

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
            ProgramCounter = Memory.getWord(Memory.ADDR_INITIALPC);
            finishProcessing = false;
            return this;
        }

        public Machine ReadLex(List<ushort> operands)
        {
            if (operands.Count < 2)
            {
                throw new Exception("insufficient operands");
            }
            Lex.read(operands[0], operands[1]);
            return this;
        }

        //In a V3 machine, there are 4 types of opcodes, as per http://inform-fiction.org/zmachine/standards/z1point1/sect14.html
        //      - 2OP
        //      - 1OP
        //      - 0OP
        //      - VAR

        /// <summary>
        /// Looks at pointer and returns instruction
        /// Returns whether input is required before proceeding
        /// </summary>
        public BreakpointType processInstruction(ulong? instructionNumber = null)
        {
            if (instructionNumber is not null)
            {
                this.InstructionCounter = instructionNumber.Value;
            }   

            if (Finished)
            {
                DebugWrite("Warning: processInstruction called after termination.");
                if (this.BreakFor.Contains(BreakpointType.Terminate))
                {
                    // we are breaking before executing, instruction counter does not increment
                    return BreakpointType.Terminate;
                }
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

            if (this.ShouldBreak)
            {
                return BreakpointsReached.Last().breakpointType;
            }

            // increment the completed instruction counter
            this.InstructionCounter++;

            return BreakpointType.None;
        }// end processInstruction

        public byte pc_getByte()
        {
            byte next = Memory.getByte(programCounter);
            programCounter++;
            return next;
        }
        public ushort pc_getWord()
        {
            ushort next = Memory.getWord(programCounter);
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
