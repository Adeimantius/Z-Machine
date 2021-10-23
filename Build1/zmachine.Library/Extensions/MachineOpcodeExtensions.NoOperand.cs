namespace zmachine.Library.Extensions
{
    using zmachine.Library.Enumerations;

    /// <summary>
    /// 0OP Classes
    /// Branch Opcodes 181, 182 , 189, 191
    /// </summary>
    public static partial class MachineOpcodeExtensions
    {
        public static void op_rtrue(this Machine machine)
        {
            machine.popRoutineData(1);
        }

        public static void op_rfalse(this Machine machine)
        {
            machine.popRoutineData(0);
        }

        public static void op_print(this Machine machine)
        {
            //                Debug.WriteLine("Getting string at " + machine.pc);
            Memory.StringAndReadLength str = machine.Memory.getZSCII(machine.ProgramCounter, 0);
            machine.IO.Write(str.str);
            machine.ProgramCounter += (uint)str.bytesRead;
            //                Debug.WriteLine("New pc location: " + machine.pc);
        }

        public static void op_print_ret(this Machine machine)
        {
            Memory.StringAndReadLength str = machine.Memory.getZSCII(machine.ProgramCounter, 0);
            machine.IO.Write(str.str);
            machine.ProgramCounter += (uint)str.bytesRead;
            machine.popRoutineData(1);
        }

        public static void op_nop(this Machine machine) { return; }

        public static void op_save(this Machine machine)
        {
            machine.Save();
        }

        public static void op_restore(this Machine machine)
        {
            machine.Restore();
        }

        public static void op_restart(this Machine machine)
        {
            fail_unimplemented(machine);
        }

        public static void op_ret_popped(this Machine machine)
        {
            machine.popRoutineData(machine.getVar(0));
        }

        public static void op_pop(this Machine machine)
        {
            machine.getVar(0);
        }

        public static void op_quit(this Machine machine)
        {
            machine.QuitNicely();
        }

        public static void op_new_line(this Machine machine)
        {
            machine.IO.WriteLine("");
        }

        public static void op_show_status(this Machine machine)
        {
            fail_unimplemented(machine);
        }

        public static void op_verifyun(this Machine machine)
        {
            fail_unimplemented(machine);
        }

        public static NoOperandOpcode process0OP(this Machine machine, int opcode)
        {
            if (!System.Enum.IsDefined(typeof(NoOperandOpcode), opcode))
            {
                throw new InvalidCastException(opcode + " is not a defined value for enum type " +
                  typeof(NoOperandOpcode).FullName);
            }

            NoOperandOpcode noOperandOpcode = (NoOperandOpcode)opcode;
            if (machine.ShouldBreakFor(BreakpointType.Opcode) && machine.OpcodeBreakpoints.Contains(noOperandOpcode))
            {
                machine.Break(breakpointType: BreakpointType.Opcode);
                if (!machine.ShouldContinueFor(BreakpointType.Opcode))
                {
                    return noOperandOpcode;
                }
            }
            switch (noOperandOpcode)
            {
                case NoOperandOpcode.op_new_line:
                    op_new_line(machine: machine);
                    break;
                case NoOperandOpcode.op_nop:
                    op_nop(machine: machine);
                    break;
                case NoOperandOpcode.op_pop:
                    op_pop(machine: machine);
                    break;
                case NoOperandOpcode.op_print:
                    op_print(machine: machine);
                    break;
                case NoOperandOpcode.op_print_ret:
                    op_print_ret(machine: machine);
                    break;
                case NoOperandOpcode.op_quit:
                    op_quit(machine: machine);
                    break;
                case NoOperandOpcode.op_restart:
                    op_restart(machine: machine);
                    break;
                case NoOperandOpcode.op_restore:
                    op_restore(machine: machine);
                    break;
                case NoOperandOpcode.op_ret_popped:
                    op_ret_popped(machine: machine);
                    break;
                case NoOperandOpcode.op_rfalse:
                    op_rfalse(machine: machine);
                    break;
                case NoOperandOpcode.op_rtrue:
                    op_rtrue(machine: machine);
                    break;
                case NoOperandOpcode.op_save:
                    op_save(machine: machine);
                    break;
                case NoOperandOpcode.op_show_status:
                    op_show_status(machine: machine);
                    break;
                case NoOperandOpcode.op_verify:
                    break;
                default:
                    fail_unimplemented(machine: machine);
                    break;
            }
            return noOperandOpcode;
            /*
            string? opcodeName = noOperandOpcode.ToString();

            MethodInfo opcodeMethod = typeof(Machine).GetMethod(opcodeName);

            if (machine.DebugEnabled)
            {
                machine.DebugWrite(machine.ProgramCounterStart.ToString("X4") + "  " + machine.stateString() + " : [0OP/" + opcode.ToString("X2") + "] " + opcodeName);
            }
            opcodeMethod.Invoke(machine, new object[] { });
            */
        }
    }
}
