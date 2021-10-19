namespace zmachine.Library.Opcodes
{
    /// <summary>
    /// Branch Opcodes 1 - 7, 10 
    /// Store Opcodes 8, 9, 15 - 25
    /// </summary>
    public abstract class OpcodeHandler_2OP : OpcodeHandler
    {
        // Implement one or the other of these:
        public static void run(Machine machine, ushort v1, ushort v2) { fail_unimplemented(machine); }
        public static void run(Machine machine, List<ushort> operands) { run(machine, operands[0], operands[1]); }
    }
}
