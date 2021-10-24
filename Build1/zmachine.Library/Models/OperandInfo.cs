using zmachine.Library.Enumerations;

namespace zmachine.Library.Models;

public record OperandInfo
{
    public readonly ushort Operand;
    public readonly OperandType OperandType;

    public OperandInfo(OperandType operandType, ushort operand)
    {
        this.OperandType = operandType;
        this.Operand = operand;
    }
}