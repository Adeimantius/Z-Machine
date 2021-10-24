using zmachine.Library.Enumerations;

namespace zmachine.Library.Models
{

    public record OpcodeEnumBox
    {
        public readonly Type OpcodeType;
        public readonly NoOperandOpcode? NoOperandOpcode;
        public readonly SingleOperandOpcodes? SingleOperandOpcode;
        public readonly TwoOperandOpcode? TwoOperandOpcode;
        public readonly VariableOperandOpcode? VariableOperandOpcode;

        public OpcodeEnumBox(object opcodeType)
        {
            this.OpcodeType = opcodeType.GetType();
            if (opcodeType is NoOperandOpcode noOperandOpcode)
            {
                this.NoOperandOpcode = noOperandOpcode;
                this.OpcodeType = noOperandOpcode.GetType();
            }
            else if (opcodeType is SingleOperandOpcodes singleOperandOpcode)
            {
                this.SingleOperandOpcode = singleOperandOpcode;
                this.OpcodeType = singleOperandOpcode.GetType();
            }
            else if (opcodeType is TwoOperandOpcode twoOperandOpcode)
            {
                this.TwoOperandOpcode = twoOperandOpcode;
                this.OpcodeType = twoOperandOpcode.GetType();
            }
            else if (opcodeType is VariableOperandOpcode variableOperandOpcode)
            {
                this.VariableOperandOpcode = variableOperandOpcode;
                this.OpcodeType = variableOperandOpcode.GetType();
            }
            else
            {
                throw new Exception();
            }
        }
    }
}