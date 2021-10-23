namespace zmachine.Library.Models
{
    public record InstructionInfo
    {
        public readonly Enumerations.BreakpointType BreakpointType;
        public readonly byte Instruction;
        public readonly Type OpcodeType;
        public readonly Enumerations.NoOperandOpcode? NoOperandOpcode;
        public readonly Enumerations.SingleOperandOpcodes? SingleOperandOpcode;
        public readonly Enumerations.TwoOperandOpcode? TwoOperandOpcode;
        public readonly Enumerations.VariableOperandOpcode? VariableOperandOpcode;
        public readonly IEnumerable<Models.OperandInfo> Operands;

        public InstructionInfo(Enumerations.BreakpointType breakpointType, byte instruction, Enum? opcodeType, IEnumerable<Models.OperandInfo> operands)
        {
            this.BreakpointType = breakpointType;
            this.Instruction = instruction;

            if (opcodeType is Enumerations.NoOperandOpcode noOperandOpcode)
            {
                this.NoOperandOpcode = noOperandOpcode;
                this.OpcodeType = noOperandOpcode.GetType();
            }
            else if (opcodeType is Enumerations.SingleOperandOpcodes singleOperandOpcode)
            {
                this.SingleOperandOpcode = singleOperandOpcode;
                this.OpcodeType = singleOperandOpcode.GetType();
            }
            else if (opcodeType is Enumerations.TwoOperandOpcode twoOperandOpcode)
            {
                this.TwoOperandOpcode = twoOperandOpcode;
                this.OpcodeType = twoOperandOpcode.GetType();
            }
            else if (opcodeType is Enumerations.VariableOperandOpcode variableOperandOpcode)
            {
                this.VariableOperandOpcode = variableOperandOpcode;
                this.OpcodeType = variableOperandOpcode.GetType();
            }
            else
            {
                throw new Exception();
            }
            
            this.Operands = operands;
        }
    }
}
