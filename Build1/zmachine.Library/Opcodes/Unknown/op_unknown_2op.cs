namespace zmachine.Library.Opcodes.Unknown
{
    public class op_unknown_2op : OpcodeHandler_2OP
    {
        public static readonly string ClassName = "UNKNOWN 2OP";

        public static void run(Machine machine, List<ushort> operands) { fail_unimplemented(machine); }
    }
}
