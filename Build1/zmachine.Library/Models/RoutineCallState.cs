namespace zmachine.Library.Models;

/// <summary>
///     This class stores the chain of routines that have been called, in sequence, and the values in the local variable
///     table.
/// </summary>
public struct RoutineCallState
{
    public ushort[] localVars = new ushort[15];
    public uint stackFrameAddress = 0; // Store the stack pointer whenever we call a return
    public uint returnAddress = 0; // Store where we need to return to
    public uint numLocalVars = 0; // We have an array of localVars, but we don't know how long it is.

    // A stack frame is an index to the routine call state (aka the stack of return addresses for routines already running, and the local variables they carry).
    // The interpreter should be able to produce the current value and set a value further down the call-stack than the current one, throwing away all others.
    // We want to be able to call a RoutineCallState for every time a routine is called.
    // We return values when routine is complete - but routine needs access to local variable table until it returns.
}