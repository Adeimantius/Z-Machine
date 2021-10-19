namespace zmachine.Library.Opcodes.Unknown
{
    public class op_unknown_op_var : OpcodeHandler_OPVAR
    {
        public static readonly string ClassName = "UNKNOWN OPVAR";

        public static void run(Machine machine, List<ushort> operands) { fail_unimplemented(machine); }
    }
}
