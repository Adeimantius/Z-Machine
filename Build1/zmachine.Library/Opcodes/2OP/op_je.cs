namespace zmachine.Library.Opcodes._2OP
{
    public class op_je : OpcodeHandler_2OP
    {
        public static new void run(Machine machine, List<ushort> operands)
        {
            bool branchOn = false;
            for (int i = 1; i < operands.Count; i++)
            {
                if (operands[0] == operands[i])
                    branchOn = true;
            }
            machine.branch(branchOn);
        }
    }
}
