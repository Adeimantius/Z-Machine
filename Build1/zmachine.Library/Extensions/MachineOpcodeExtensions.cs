using System.Diagnostics;
using zmachine.Library.Enumerations;

namespace zmachine.Library.Extensions;

public static partial class MachineOpcodeExtensions
{
    public static BreakpointType fail_unimplemented(this Machine machine)
    {
        string callingFunctionName = new StackTrace().GetFrame(1)!.GetMethod()!.Name;
        return machine.Terminate("Unimplemented function: " + callingFunctionName, BreakpointType.Unimplemented);
    }
}