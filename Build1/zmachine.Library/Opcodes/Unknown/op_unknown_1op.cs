namespace zmachine.Library.Opcodes.Unknown
{
    public class op_unknown_1op : OpcodeHandler_1OP
    {
        public static readonly string ClassName = "UNKNOWN 1OP";

        public static void run(Machine machine, ushort v1) { fail_unimplemented(machine); }
    }
}
