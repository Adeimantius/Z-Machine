namespace zmachine.Library;

public partial class Machine
{
    public CPUState State
    {
        get => new(this.Memory.Contents, this.stack.Contents, this.Lex.MemoryPointer, this.ProgramCounter, this.pcStart, this.stackPointer, this.callDepth,
            this.callStack, this.Finished, this.InstructionCounter);
        set
        {
            this.Memory.load(value.memory);
            this.stack.load(value.stack);
            this.Lex.MemoryPointer = value.lexMemoryPointer;
            this.ProgramCounter = value.programCounter;
            this.pcStart = value.pcStart;
            this.stackPointer = value.stackPointer;
            this.callDepth = value.callDepth;
            Array.Copy(
                value.callStack,
                this.callStack,
                StackDepth);
            this.Finished = value.finish;
            this.InstructionCounter = value.instructionCounter;
        }
    }

    public CPUState Save()
    {
        if (this.CPUStates.Count >= MaximumRestoreStates)
        {
            this.CPUStates.RemoveFirst();
        }

        CPUState state = this.State;
        this.CPUStates.AddLast(state);
        return state;
    }

    public bool Restore(bool removeAfterRestore = false)
    {
        if (!this.CPUStates.Any())
        {
            return false;
        }

        this.State = this.CPUStates.Last();

        if (removeAfterRestore)
        {
            this.CPUStates.RemoveLast();
        }

        return true;
    }

    public string stateString()
    {
        string s = "M: " + this.Memory.getCrc32() + " S: " + this.stack.getCrc32();
        //            for (ushort i = 1; i < 256; ++i)
        //                s += " " + getVar(i);
        return s;
    }
}