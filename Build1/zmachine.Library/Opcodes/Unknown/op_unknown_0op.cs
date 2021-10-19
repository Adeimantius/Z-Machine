namespace zmachine.Library.Opcodes.Unknown
{
    public class op_unknown_0op : OpcodeHandler_0OP
    {
        public static readonly string ClassName = "UNKNOWN 0OP";

        public static void run(Machine machine) { fail_unimplemented(machine); }
    }
}
