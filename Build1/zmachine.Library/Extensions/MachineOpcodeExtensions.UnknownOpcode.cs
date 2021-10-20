namespace zmachine.Library.Extensions
{
    public static partial class MachineOpcodeExtensions
    {
        public static void op_unknown_0op(this Machine machine) => fail_unimplemented(machine);

        public static void op_unknown_1op(this Machine machine, ushort v1) => fail_unimplemented(machine);

        public static void op_unknown_2op(this Machine machine, List<ushort> operands) => fail_unimplemented(machine);

        public static void op_unknown_op_var(this Machine machine, List<ushort> operands) => fail_unimplemented(machine);
    }
}
