namespace zmachine.Library.Models
{
    using zmachine.Library.Enumerations;

    public record OperandInfo
    {
        public readonly Enumerations.OperandType OperandType;
        public readonly ushort Operand;

        public OperandInfo(OperandType operandType, ushort operand)
        {
            this.OperandType = operandType;
            this.Operand = operand;
        }
    }
}
