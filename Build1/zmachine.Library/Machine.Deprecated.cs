namespace zmachine.Library
{
    public partial class Machine
    {
        /*
        private void process2OP(int opcode, List<ushort> operands)
        {
            string? callingFunctionName = new StackTrace().GetFrame(1)!.GetMethod()!.Name;

            Action op = OP2opcodes[opcode];
            if (debug)
            {
                Debug.Write(pcStart.ToString("X4") + "  " + stateString() + " : [2OP/" + opcode.ToString("X2") + "] " + callingFunctionName);
                foreach (ushort v in operands)
                {
                    Debug.Write(" " + v);
                }

                Debug.WriteLine("");
            }
            op.DynamicInvoke(operands);
        }

        void process1OP(int opcode, ushort operand1)
        {
            string? callingFunctionName = new StackTrace().GetFrame(1)!.GetMethod()!.Name;
            Action op = OP1opcodes[opcode];
            if (debug)
            {
                Debug.WriteLine(pcStart.ToString("X4") + "  " + stateString() + " : [1OP/" + opcode.ToString("X2") + "] " + callingFunctionName + " " + operand1.ToString());
            }

            op.DynamicInvoke(operand1);
        }
        void process0OP(int opcode)
        {
            string? callingFunctionName = new StackTrace().GetFrame(1)!.GetMethod()!.Name;
            Action op = OP0opcodes[opcode];
            if (debug)
            {
                Debug.WriteLine(pcStart.ToString("X4") + "  " + stateString() + " : [0OP/" + opcode.ToString("X2") + "] " + callingFunctionName);
            }

            op.Invoke();
        }
        void processVAR(int opcode, List<ushort> operands)
        {
            string? callingFunctionName = new StackTrace().GetFrame(1)!.GetMethod()!.Name;
            Action op = VARopcodes[opcode];
            if (debug)
            {
                Debug.Write(pcStart.ToString("X4") + "  " + stateString() + " : [VAR/" + opcode.ToString("X2") + "] " + callingFunctionName);
                foreach (ushort v in operands)
                {
                    Debug.Write(" " + v);
                }

                Debug.WriteLine("");
            }
            op.DynamicInvoke(operands);
        }
        */

    }
}
