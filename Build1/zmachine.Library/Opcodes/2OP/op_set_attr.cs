namespace zmachine.Library.Opcodes._2OP
{
    public class op_set_attr : OpcodeHandler_2OP
    {
        public static void run(Machine machine, ushort v1, ushort v2) { machine.ObjectTable.setObjectAttribute(v1, v2, true); }
    }
}
