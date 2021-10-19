namespace zmachine.Library.Opcodes._2OP
{
    public class op_dec_chk : OpcodeHandler_2OP
    {
        public static void run(Machine machine, ushort v1, ushort v2)
        {
            int value = ((short)machine.getVar(v1)) - 1;
            machine.setVar(v1, (ushort)value);
            machine.branch(value < v2);
        }
    }
}
