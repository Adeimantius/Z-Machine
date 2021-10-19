﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace zmachine
{

    public partial class Machine
    {
        public const int StackDepth = 128;
        public const int MemorySize = 1024 * 128;
        public const int StackSize = 1024 * 32;

        // This class moves through the input file and extracts bytes to deconstruct instructions in the code
        private Memory memory = new Memory(size: MemorySize);         // Initialize memory
        private Memory stack = new Memory(size: StackSize);           // Stack of size 32768 (can be larger, but this should be fine)
        private ObjectTable objectTable;
        private readonly IIO io;
        private readonly Lex lex;

        private readonly bool debug = false;

        //  StateOfPlay stateofplay = new StateOfPlay();    // Will contain (1) Local variable table, (2) contents of the stack, (3) value of PC, (4) current routine call state.
        private uint programCounter = 0;                                        // Program Counter to step through memory
        private uint pcStart = 0;                                   // Program Counter at the beginning of executing the instruction
        private bool finish = false;                                // Flag to say "finish processing. We're done".
        private uint stackPointer = 0;
        private uint callDepth = 0;
        RoutineCallState[] callStack = new RoutineCallState[StackDepth];  // We could use a "Stack" here, as well, but we have lots of memory. According to the spec, there could be up to 90. But 128 is nice.

        // Class constructor : Loads in data from file and sets Program Counter
        public Machine(IIO io, string programFilename)
        {
            this.io = io;
            this.memory.load(programFilename);
            this.setProgramCounter();

            for (int i = 0; i < StackDepth; ++i)
                this.callStack[i] = new RoutineCallState();

            this.objectTable = new ObjectTable(memory);
            this.lex = new Lex(
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
            this.objectTable = new ObjectTable(memory);
            this.lex = new Lex(
                io: this.io,
                mem: memory,
                mp: initialState.lexMemoryPointer);
            this.State = initialState;
        }

        public bool Finished => finish;

        public bool DebugEnabled
        {
            get => debug;
        }

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

        // Find the PC start point in the header file and set PC 
        public void setProgramCounter()
        {
            programCounter = memory.getWord(Memory.ADDR_INITIALPC);               // Set PC by looking at pc start byte in header file
        }

        //In a V3 machine, there are 4 types of opcodes, as per http://inform-fiction.org/zmachine/standards/z1point1/sect14.html
        //      - 2OP
        //      - 1OP
        //      - 0OP
        //      - VAR


        // Looks at pointer and returns instruction
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
                        int opTypeA = (int)(instruction >> 6) & 1;//6th bit
                        int opTypeB = (int)(instruction >> 5) & 1;//5th bit
                                                                  // Value of 0 means variable, value of 1 means small

                        // load both operands of long form instruction
                        operandList.Add(loadOperand(opTypeA == 0 ? OperandType.Small : OperandType.Var));
                        operandList.Add(loadOperand(opTypeB == 0 ? OperandType.Small : OperandType.Var));
                        process2OP(opcode, operandList);
                    }
                    break;

                case 0x02:                              // Define short-form instruction
                    {
                        //                    Debug.WriteLine("\tInstruction Form : Short");
                        opcode = (ushort)(instruction & 0xf);
                        int opType = (byte)((instruction >> 4) & 3);        // Grab bits 4 and 5.

                        if (opType == (int)OperandType.Omit)
                        {
                            process0OP(opcode);
                        }
                        else
                        {
                            ushort op1 = loadOperand((OperandType)opType);
                            process1OP(opcode, op1);
                        }
                    }
                    break;


                case 0x03:                             // Define Variable form instruction
                    {
                        //                        Debug.WriteLine("\tInstruction Form : Variable");
                        opcode = (ushort)(instruction & 0x1f);          // Grab bottom 5 bits. 
                        int type = ((instruction >> 5) & 1);            // Store type of opcode (2OP, VAR)          
                        byte operandTypes = (byte)pc_getByte();

                        for (int i = 0; i < 4; i++)
                        {
                            OperandType opType = (OperandType)((operandTypes >> 6) & 3);  // Grab top 2 bits

                            ushort op = loadOperand(opType);
                            if (opType != OperandType.Omit)
                                operandList.Add(op);

                            operandTypes <<= 2;   // Shift two to the left
                        }


                        if (type == 0)
                        {
                            process2OP(opcode, operandList);           // Need to pass in two operands?
                        }
                        else
                        {
                            processVAR(opcode, operandList);

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
            return (byte)next;
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

        void process2OP(int opcode, List<ushort> operands)
        {
            OpcodeHandler_2OP op = OP2opcodes[opcode];
            if (debug)
            {
                Debug.Write(pcStart.ToString("X4") + "  " + stateString() + " : [2OP/" + opcode.ToString("X2") + "] " + op.name());
                foreach (ushort v in operands)
                    Debug.Write(" " + v);
                Debug.WriteLine("");
            }
            op.run(this, operands);
        }
        void process1OP(int opcode, ushort operand1)
        {
            OpcodeHandler_1OP op = OP1opcodes[opcode];
            if (debug)
                Debug.WriteLine(pcStart.ToString("X4") + "  " + stateString() + " : [1OP/" + opcode.ToString("X2") + "] " + op.name() + " " + operand1.ToString());
            op.run(this, operand1);
        }
        void process0OP(int opcode)
        {
            OpcodeHandler_0OP op = OP0opcodes[opcode];
            if (debug)
                Debug.WriteLine(pcStart.ToString("X4") + "  " + stateString() + " : [0OP/" + opcode.ToString("X2") + "] " + op.name());
            op.run(this);
        }
        void processVAR(int opcode, List<ushort> operands)
        {
            OpcodeHandler_OPVAR op = VARopcodes[opcode];
            if (debug)
            {
                Debug.Write(pcStart.ToString("X4") + "  " + stateString() + " : [VAR/" + opcode.ToString("X2") + "] " + op.name());
                foreach (ushort v in operands)
                    Debug.Write(" " + v);
                Debug.WriteLine("");
            }
            op.run(this, operands);
        }

        private readonly OpcodeHandler_2OP[] OP2opcodes = {
        new  op_unknown_2op(),      // OPCODE/HEX
        new  op_je(),               // 01/01 a b ?(label)
        new  op_jl(),               // 02/02 a b ?(label)	
        new  op_jg(),               // 03/03
        new  op_dec_chk(),          // 04/04
        new  op_inc_chk(),          // 05/05
        new  op_jin(),              // 06/06
        new  op_test(),             // 07/07
        new  op_or(),               // 08/08
        new  op_and(),              // 09/09
        new  op_test_attr(),        // 10/0A
        new  op_set_attr(),         // 11/0B
        new  op_clear_attr(),       // 12/0C
        new  op_store(),            // 13/0D
        new  op_insert_obj(),       // 14/0E
        new  op_loadw(),            // 15/0F
        new  op_loadb(),            // 16/10
        new  op_get_prop(),         // 17/11
        new  op_get_prop_addr(),    // 18/12
        new  op_get_next_addr(),    // 19/13
        new  op_add(),              // 20/14
        new  op_sub(),              // 21/15
        new  op_mul(),              // 22/15
        new  op_div(),              // 23/16
        new  op_mod(),              // 24/17
        new  op_unknown_2op(),      // 25/19 
        new  op_unknown_2op(),      // 26/1A
        new  op_unknown_2op(),      // 27/1B 
        new  op_unknown_2op(),      // 28/1C 
        new  op_unknown_2op(),      // 29/1D 
        new  op_unknown_2op(),      // 30/1E
        new  op_unknown_2op()       // 31/1F 
     };
        private readonly OpcodeHandler_1OP[] OP1opcodes = {
                                    // OPCODE/HEX
        new  op_jz(),               // 128/01
        new  op_get_sibling(),      // 129/01 	
        new  op_get_child(),        // 130/02 
        new  op_get_parent(),       // 131/03
        new  op_get_prop_len(),     // 132/04
        new  op_inc(),              // 133/05
        new  op_dec(),              // 134/06
        new  op_print_addr(),       // 135/07
        new  op_unknown_1op(),      // 136/08
        new  op_remove_obj(),       // 137/09
        new  op_print_obj(),        // 138/0A
        new  op_ret(),              // 139/0B
        new  op_jump(),             // 140/0C
        new  op_print_paddr(),      // 141/0D
        new  op_load(),             // 142/0E
        new  op_unknown_1op(),      // 141/0F
        new  op_unknown_1op()       // 142/0F
        };
        private readonly OpcodeHandler_0OP[] OP0opcodes = {
                                    // OPCODE/HEX
        new  op_rtrue(),            // 176/00 
        new  op_rfalse(),           // 177/01 
        new  op_print (),           // 178/02 
        new  op_print_ret(),        // 179/03
        new  op_nop(),              // 180/04
        new  op_save (),            // 181/05
        new  op_restore (),         // 182/06
        new  op_restart(),          // 183/07
        new  op_ret_popped(),       // 184/08
        new  op_pop(),              // 185/09
        new  op_quit(),             // 186/0A
        new  op_new_line(),         // 187/0B
        new  op_show_status(),      // 188/0C
        new  op_verify ()           // 189/0C
        };
        private readonly OpcodeHandler_OPVAR[] VARopcodes = {
                                    // OPCODE/HEX
        new  op_call(),             // 224/00 
        new  op_storew(),           // 225/01 
        new  op_storeb(),           // 226/02
        new  op_put_prop(),         // 227/03
        new  op_sread(),            // 228/04
        new  op_print_char(),       // 229/05
        new  op_print_num(),        // 230/06
        new  op_random(),           // 231/07
        new  op_push(),             // 232/08
        new  op_pull(),             // 233/09
        new  op_split_window(),     // 234/0A
        new  op_set_window(),       // 235/0B
        new  op_unknown_op_var(),   // 236/1E
        new  op_unknown_op_var(),   // 237/1E
        new  op_unknown_op_var(),   // 238/1E
        new  op_unknown_op_var(),   // 239/1E
        new  op_unknown_op_var(),   // 240/1E
        new  op_unknown_op_var(),   // 241/1E
        new  op_unknown_op_var(),   // 242/1E
        new  op_output_stream(),    // 243/13
        new  op_input_stream()      // 244/14
        };
        // --------------------------------------------------------------------------------------------

        public void branch(bool condition)        // Check branch instructions
        {
            //             Debug.WriteLine("BRANCH: " + (condition ? "1" : "0"));

            byte branchInfo = pc_getByte();
            int branchOn = ((branchInfo >> 7) & 0x01);
            int branchSmall = ((branchInfo >> 6) & 0x1);
            uint offset = (uint)(branchInfo & 0x3f); // the "offset" is in the range 0 to 63, given in the bottom 6 bits.

            if (branchOn == 0)
                condition = !condition;

            if (branchSmall == 0)
            {                            // If bit 6 is clear the offset is a signed 14-bit number given
                if (offset > 31)
                    offset -= 64;          // Convert offset to signed


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
                    programCounter += offset - 2;
            }

        }


        // Put a value onto the top of the stack.
        public void setVar(ushort variable, ushort value)
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

        public CPUState State
        {
            get
            {
                return new CPUState(
                    memory: this.memory.Contents,
                    stack: this.stack.Contents,
                    lexMemoryPointer: this.lex.MemoryPointer,
                    pc: this.programCounter,
                    pcStart: this.pcStart,
                    sp: this.stackPointer,
                    callDepth: this.callDepth,
                    callStack: this.callStack,
                    finish: this.finish);
            }
            set
            {
                this.memory.load(value.memory);
                this.stack.load(value.stack);
                this.lex.MemoryPointer = value.lexMemoryPointer;
                this.programCounter = value.programCounter;
                this.pcStart = value.pcStart;
                this.stackPointer = value.stackPointer;
                this.callDepth = value.callDepth;
                this.callStack = value.callStack;
                this.finish = value.finish;
            }
        }
        // --------------------------------------------------------------------------------------------




        public string stateString()
        {
            string s = "M: " + memory.getCrc32() + " S: " + stack.getCrc32();
            //            for (ushort i = 1; i < 256; ++i)
            //                s += " " + getVar(i);
            return s;
        }
        //        --------------------------------------------------------------------------------------------


    } // end Machine
} // end namespace
