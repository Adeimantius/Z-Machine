using zmachine.Library.Enumerations;

namespace zmachine.Library.Models;

public record InstructionInfo
{
    public readonly BreakpointType BreakpointType;
    public readonly byte Instruction;
    public readonly OpcodeEnumBox? OpcodeType;
    public readonly IEnumerable<OperandInfo> Operands;

    public InstructionInfo(BreakpointType breakpointType, byte instruction, object? opcodeType,
        IEnumerable<OperandInfo> operands)
    {
        this.BreakpointType = breakpointType;
        this.Instruction = instruction;
        this.OpcodeType = opcodeType is not null ? new OpcodeEnumBox(opcodeType) : null;
        this.Operands = operands;
    }
}