namespace zmachine.Library.Enumerations;

public enum BreakpointType
{
    None,
    DivisionByZero,
    StackOverflow,
    StackUnderrun,
    Error,
    InputRequired,
    Opcode,
    Unimplemented,
    Terminate,
    Complete
}