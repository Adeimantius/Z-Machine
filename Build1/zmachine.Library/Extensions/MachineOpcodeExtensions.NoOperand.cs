namespace zmachine.Library.Extensions
{
    using System.Reflection;
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
            fail_unimplemented(machine: machine);
        }

        public static void op_restore(this Machine machine)
        {
            fail_unimplemented(machine);
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
            machine.Terminate(nameof(op_quit));
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

        public static void process0OP(this Machine machine, int opcode)
        {
            if (!System.Enum.IsDefined(typeof(NoOperandOpcode), opcode))
            {
                throw new InvalidCastException(opcode + " is not a defined value for enum type " +
                  typeof(NoOperandOpcode).FullName);
            }

            NoOperandOpcode noOperandOpcode = (NoOperandOpcode)opcode;
            string? opcodeName = noOperandOpcode.ToString();

            MethodInfo opcodeMethod = typeof(Machine).GetMethod(opcodeName);

            if (machine.DebugEnabled)
            {
                machine.DebugWrite(machine.ProgramCounterStart.ToString("X4") + "  " + machine.stateString() + " : [0OP/" + opcode.ToString("X2") + "] " + opcodeName);
            }
            opcodeMethod.Invoke(machine, new object[] { });
        }
    }
}
