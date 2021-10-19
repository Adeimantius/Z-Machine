namespace zmachine.Library.Opcodes._2OP
{
    public class op_test : OpcodeHandler_2OP
    {
        public static void run(Machine machine, ushort v1, ushort v2) { machine.branch((v1 & v2) == v2); }
    }
}
