namespace zmachine.Library
{
    using zmachine.Library.Enumerations;

    public partial class Machine
    {
        public bool ShouldBreak => (this.InstructionCounter > this.BreakAfter) && this.breakpointsReached.Any();

        public bool ShouldBreakFor(BreakpointType breakpointType)
        {
            if (EndProgramBreakpoints.Contains(breakpointType))
            {
                // always break for termination
                return true;
            }
            return (this.InstructionCounter > this.BreakAfter) &&
                this.BreakFor.ContainsKey(breakpointType);
        }

        public bool ShouldContinueFor(BreakpointType breakpointType)
        {
            return this.BreakFor.ContainsKey(breakpointType) && this.BreakFor[breakpointType] == BreakpointAction.Continue;
        }

        public bool BreakpointReached(BreakpointType breakpointType)
        {
            return this.breakpointsReached
.Select(t => t.breakpointType)
.Contains(breakpointType);
        }

        public Machine ClearBreakpointsReached()
        {
            this.breakpointsReached.Clear();
            return this;
        }

        public Machine AddBreakpoint(BreakpointType breakpointType, BreakpointAction breakpointAction, ulong? afterInstruction = null)
        {
            if (this.BreakFor.ContainsKey(breakpointType))
            {
                this.BreakFor[breakpointType] = breakpointAction;
            } 
            else
            {
                this.BreakFor.Add(breakpointType, breakpointAction);
            }
            if (afterInstruction is not null && (this.BreakAfter < afterInstruction.Value))
            {
                this.BreakAfter = afterInstruction.Value;
            }
            return this;
        }

        public Machine RemoveBreakpoint(BreakpointType breakpointType)
        {
            if (this.BreakFor.ContainsKey(breakpointType))
            {
                this.BreakFor.Remove(breakpointType);
            }
            return this;
        }

        public bool Break(BreakpointType breakpointType)
        {
            if (!this.ShouldBreakFor(breakpointType))
            {
                return false;
            }
            this.breakpointsReached.Add((
                instruction: this.InstructionCounter,
                breakpointType: breakpointType));
            return true;
        }
    }
}
