namespace zmachine.Library.Opcodes._2OP
{
    public class op_jin : OpcodeHandler_2OP
    {
        public static void run(Machine machine, ushort v1, ushort v2) { machine.branch(machine.ObjectTable.getParent(v1) == v2); }
    }
}
