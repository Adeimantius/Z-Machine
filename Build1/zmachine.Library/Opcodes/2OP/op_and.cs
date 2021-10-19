namespace zmachine.Library.Opcodes._2OP
{
    public class op_and : OpcodeHandler_2OP
    {
        public static void run(Machine machine, ushort v1, ushort v2) { machine.setVar(machine.pc_getByte(), (ushort)(v1 & v2)); }
    }
}
