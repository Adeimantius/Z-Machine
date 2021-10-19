namespace zmachine.Library.Opcodes._2OP
{
    public class op_jl : OpcodeHandler_2OP
    {
        public static void run(Machine machine, ushort v1, ushort v2) { machine.branch((short)v1 < (short)v2); }
    }
}
