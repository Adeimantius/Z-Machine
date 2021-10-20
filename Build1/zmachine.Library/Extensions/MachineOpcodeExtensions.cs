namespace zmachine.Library.Extensions
{
    using System.Diagnostics;

    public static partial class MachineOpcodeExtensions
    {
        public static void fail_unimplemented(this Machine machine)
        {
            string? callingFunctionName = new StackTrace().GetFrame(1)!.GetMethod()!.Name;
            machine.Terminate(error: "Unimplemented function: " + callingFunctionName);
        }
    }
}
