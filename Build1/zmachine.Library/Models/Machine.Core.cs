using System.Diagnostics;
using zmachine.Library.Enumerations;
using zmachine.Library.Extensions;

namespace zmachine.Library.Models;

public partial class Machine
{
    public Machine DebugWrite(params object[] args)
    {
        if (this.DebugEnabled)
        {
            for (int i = 0; i < args.Length; i++)
            {
                Debug.Write(args[i] + " ");
            }

            Debug.WriteLine("");
        }

        return this;
    }

    public virtual BreakpointType QuitNicely()
    {
        return this.Terminate("user quit", finalBreakpointType: BreakpointType.Complete, forceAddBreak: true);
    }

    /// <summary>
    ///     Terminates execution of the program.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public BreakpointType Terminate(string? error = null, BreakpointType finalBreakpointType = BreakpointType.Terminate, bool forceAddBreak = false)
    {
        this.DebugWrite("Terminate called");
        if (error is not null)
        {
            this.DebugWrite("Error: " + error);
        }

        this.Finished = true;
        this.Break(
            breakpointType: finalBreakpointType,
            force: forceAddBreak);
        return finalBreakpointType;
    }

    public uint unpackedAddress(ushort address)
    {
        return (uint)2 * address;
    }


    public Machine branch(bool condition) // Check branch instructions
    {
        //             Debug.WriteLine("BRANCH: " + (condition ? "1" : "0"));

        byte branchInfo = this.pc_getByte();
        int branchOn = (branchInfo >> 7) & 0x01;
        int branchSmall = (branchInfo >> 6) & 0x1;
        uint offset = (uint)(branchInfo & 0x3f); // the "offset" is in the range 0 to 63, given in the bottom 6 bits.

        if (branchOn == 0)
        {
            condition = !condition;
        }

        if (branchSmall == 0)
        {
            // If bit 6 is clear the offset is a signed 14-bit number given
            if (offset > 31)
            {
                offset -= 64; // Convert offset to signed
            }

            offset <<= 8; // Shift bits 8 left before adding next 6 to make a 14-bit number.

            offset += this.pc_getByte(); // in bits 0 to 5 of the first byte followed by all 8 of the second.
        }

        if (condition)
        {
            if (offset == 0 || offset == 1)
            {
                this.popRoutineData(
                    returnValue: (ushort)offset);
            }
            else
            {
                this.ProgramCounter += offset - 2;
            }
        }

        return this;
    }


    public const int VAR_TOP_OF_STACK = 0;

    // Put a value onto the top of the stack.
    public virtual Machine setVar(ushort variable, ushort value)
    {
        if (variable == VAR_TOP_OF_STACK) // Variable number $00 refers to the top of the stack
        {
            this.Stack.setWord(
                address: this.StackPointer,
                value: value); // Set value in stack
            this.StackPointer += 2; // Increment stack pointer by 2 (size of word)
        }
        else if (variable < 16) //$01 to $0f mean the local variables of the current routine
        {
            this.callStack[this.callDepth].localVars[variable - 1] = value; // Set value in local variable table
        }
        else
        {
            //and $10 to $ff mean the global variables.
            uint address =
                (uint)(this.Memory.getWord(
                    address:
                        Memory.ADDR_GLOBALS) + (variable - 16) * 2); // Set value in global variable table (variable 16 -> index 0)
            this.Memory.setWord(
                address: address,
                value: value);
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
            this.StackPointer -= 2;
            value = this.Stack.getWord(
                address: this.StackPointer); // get value from stack;
        }
        else if (variable < 16)
        {
            value = this.callStack[this.callDepth].localVars[variable - 1]; // get value from local variable table
        }
        else
        {
            uint address =
                (uint)(this.Memory.getWord(Memory.ADDR_GLOBALS) +
                        (variable - 16) *
                        2); // Move by a variable number of steps through the table of global variables.
            value = this.Memory.getWord(address);
            // Debug.WriteLine("VAR[" + variable + "] = " + value);
        }


        return value;
    }

    public virtual void pushRoutineData(List<ushort> operands)
    {
        // First check if we've gone too deep into our call stack:
        if (this.callDepth >= StackDepth)
        {
            Debug.Assert(Machine.DEBUG_ASSERT_DISABLED, "Error: Call Stack Overflow"); //alert the user
            this.Terminate("Error: Call Stack Overflow", BreakpointType.StackOverflow, forceAddBreak: true);
            this.Finished = true;
        }

        ++this.callDepth;
        this.callStack[this.callDepth].returnAddress =
            this.ProgramCounter; // Store return address @ current position of pc upon entering routine
        this.ProgramCounter = this.unpackedAddress(operands[0]); // Set the PC to the routine address.
        this.callStack[this.callDepth].stackFrameAddress = this.StackPointer; // Store stack pointer address.

        //SPEC: 6.4.4 When a routine is called, its local variables are created with initial values taken from the routine header (Versions 1 to 4). 
        //            Next, the arguments are written into the local variables (argument 1 into local 1 and so on).
        byte numLocals = this.pc_getByte();
        for (int i = 0; i < numLocals; i++)
        {
            this.callStack[this.callDepth].localVars[i] =
                this.pc_getWord(); // Local variables and the stack are conserved as the routine executes.
        }

        //Those remaining (up to three) operands will stomp over the first (up to three) values in localVars
        for (int i = 1; i < operands.Count; i++)
        {
            this.callStack[this.callDepth].localVars[i - 1] =
                operands[i]; ////Loop over the remaining operands, putting them into the appropriate spots in localVars
        }

        numLocals = (byte)Math.Max(operands.Count - 1, numLocals);
        this.callStack[this.callDepth].numLocalVars = numLocals;
    }

    /// <summary>
    /// Gets function/routine info from top of call stack and loads the return address on to the program counter and the stack frame address onto the stack pointer before setting the return value
    /// </summary>
    /// <param name="returnValue"></param>
    /// <returns></returns>
    public virtual void popRoutineData(ushort returnValue)
    {
        if (this.callDepth == 0)
        {
            Debug.Assert(Machine.DEBUG_ASSERT_DISABLED, "Error: Call Stack Underrun"); // alert the user to the error.
            this.Terminate("Error: Call Stack Underrun", BreakpointType.StackUnderrun, forceAddBreak: true);
            this.Finished = true;
        }

        // Restore the stack to the previous value in callstack[callDepth]
        this.ProgramCounter =
            this.callStack[this.callDepth].returnAddress; //Restore the PC to the previous value in callstack[callDepth]
        this.StackPointer =
            this.callStack[this.callDepth].stackFrameAddress; //Restore the sp to the previous value in callstack[callDepth]
        --this.callDepth;

        this.setVar(this.pc_getByte(),
            returnValue); // Set the return value. Calling a routine is a "store" function, so the next byte contains where to store the result.
    }

    /// <summary>
    ///     Find the PC start point in the header file and set PC
    ///     Set PC by looking at pc start byte in header file
    /// </summary>
    private Machine initializeProgramCounter()
    {
        this.ProgramCounter = this.Memory.getWord(Memory.ADDR_INITIALPC);
        this.Finished = false;
        return this;
    }

    public BreakpointType ReadLex(List<ushort> operands)
    {
        if (operands.Count < 2)
        {
            throw new Exception("insufficient operands");
        }

        return this.Lex.read(operands[0], operands[1]);
    }

    //In a V3 machine, there are 4 types of opcodes, as per http://inform-fiction.org/zmachine/standards/z1point1/sect14.html
    //      - 2OP
    //      - 1OP
    //      - 0OP
    //      - VAR

    /// <summary>
    ///     Looks at pointer and returns instruction
    ///     Returns whether input is required before proceeding
    /// </summary>
    public InstructionInfo processInstruction(ulong? instructionNumber = null)
    {
        var startingBreakCount = this.BreakpointsReached.Count();

        if (this.InstructionCounter >= 380)
        {
            this.DebugWrite(this.InstructionCounter.ToString());
        }

        if (instructionNumber is not null)
        {
            this.InstructionCounter = instructionNumber.Value;
        }

        if (this.Finished)
        {
            this.DebugWrite("Warning: processInstruction called after termination.");
            return new InstructionInfo(BreakpointType.Terminate, 0, null, new List<OperandInfo>());
        }

        this.pcStart = this.ProgramCounter;
        ushort opcode; // Initialize variables
        byte originalInstruction = this.pc_getByte();
        Enum? opcodeType = null;
        int instruction = originalInstruction;
        //            Debug.WriteLine(pcStart.ToString("X4") + " : 0x" + instruction.ToString("X2") + " (" + instruction + ")");

        List<ushort> operandList = new List<ushort>();
        List<OperandInfo> operandInfo = new List<OperandInfo>();
        // This switch statement looks at the PC and reads in two bits to see the instruction form
        int form = (byte)(instruction >> 6);
        switch (form)
        {
            case 0x00: // Define long-form instruction
            case 0x01:
                {
                    //                    Debug.WriteLine("\tInstruction Form : Long");
                    opcode = (ushort)(instruction & 0x1f);
                    int opTypeA = (instruction >> 6) & 1; //6th bit
                    int opTypeB = (instruction >> 5) & 1; //5th bit
                                                          // Value of 0 means variable, value of 1 means small
                    OperandType operandTypeA = opTypeA == 0 ? OperandType.Small : OperandType.Var;
                    OperandType operandTypeB = opTypeB == 0 ? OperandType.Small : OperandType.Var;

                    ushort operandA = this.loadOperand(operandTypeA);
                    ushort operandB = this.loadOperand(operandTypeB);

                    // load both operands of long form instruction
                    operandList.Add(operandA);
                    operandInfo.Add(new OperandInfo(
                        operandTypeA,
                        operandA));

                    operandList.Add(operandB);
                    operandInfo.Add(new OperandInfo(
                        operandTypeB,
                        operandB));

                    opcodeType = this.process2OP(opcode, operandList);
                }
                break;

            case 0x02: // Define short-form instruction
                {
                    //                    Debug.WriteLine("\tInstruction Form : Short");
                    opcode = (ushort)(instruction & 0xf);
                    int opType = (byte)((instruction >> 4) & 3); // Grab bits 4 and 5.

                    if (opType == (int)OperandType.Omit)
                    {
                        opcodeType = this.process0OP(opcode);
                    }
                    else
                    {
                        ushort op1 = this.loadOperand((OperandType)opType);
                        operandInfo.Add(new OperandInfo((OperandType)opType, op1));
                        opcodeType = this.process1OP(opcode, op1);
                    }
                }
                break;


            case 0x03: // Define Variable form instruction
                {
                    //                        Debug.WriteLine("\tInstruction Form : Variable");
                    opcode = (ushort)(instruction & 0x1f); // Grab bottom 5 bits. 
                    int type = (instruction >> 5) & 1; // Store type of opcode (2OP, VAR)          
                    byte operandTypes = this.pc_getByte();

                    for (int i = 0; i < 4; i++)
                    {
                        OperandType opType = (OperandType)((operandTypes >> 6) & 3); // Grab top 2 bits

                        ushort op = this.loadOperand(opType);
                        if (opType != OperandType.Omit)
                        {
                            operandList.Add(op);
                            operandInfo.Add(new OperandInfo(opType, op));
                        }

                        operandTypes <<= 2; // Shift two to the left
                    }


                    if (type == 0)
                    {
                        opcodeType = this.process2OP(opcode, operandList); // Need to pass in two operands?
                    }
                    else
                    {
                        opcodeType = this.processVAR(opcode, operandList);
                    }
                }
                break;

                //return (byte) instruction;                 
        }

        if (this.ShouldBreak)
        {
            BreakpointType lastBreak = this.BreakpointsReached.Last().breakpointType;
            // Completion is the only type of breakpoint we increment after
            if (lastBreak == BreakpointType.Complete)
            {
                this.InstructionCounter++;
            }

            return new InstructionInfo(
                lastBreak,
                originalInstruction,
                opcodeType,
                operandInfo);
        }

        // increment the completed instruction counter
        this.InstructionCounter++;

        return new InstructionInfo(
            BreakpointType.None,
            originalInstruction,
            opcodeType,
            operandInfo);
    } // end processInstruction

    public virtual byte pc_getByte()
    {
        byte next = this.Memory.getByte(this.ProgramCounter);
        this.ProgramCounter++;
        return next;
    }

    public ushort pc_getWord()
    {
        ushort next = this.Memory.getWord(this.ProgramCounter);
        this.ProgramCounter += 2;
        return next;
    }

    public ushort loadOperand(OperandType optype)
    {
        ushort operand;

        switch (optype)
        {
            case OperandType.Large:
                {
                    operand = this.pc_getWord();
                    return operand;
                }

            case OperandType.Small:
                {
                    operand = this.pc_getByte();
                    return operand;
                }
            case OperandType.Var:
                {
                    operand = this.getVar(this.pc_getByte());
                    return operand;
                }
            default: // OperandType.Omit:
                {
                    return 0;
                }
        }
    }
}