namespace zmachine.Library
{
    using System;

    public partial class Machine
    {
        public CPUState State
        {
            get => new CPUState(
                    memory: this.Memory.Contents,
                    stack: this.stack.Contents,
                    lexMemoryPointer: this.Lex.MemoryPointer,
                    pc: this.ProgramCounter,
                    pcStart: this.pcStart,
                    sp: this.stackPointer,
                    callDepth: this.callDepth,
                    callStack: this.callStack,
                    finish: this.finishProcessing,
                    instructionCounter: this.InstructionCounter);
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
                    sourceArray: value.callStack,
                    destinationArray: this.callStack,
                    length: StackDepth);
                this.finishProcessing = value.finish;
                this.InstructionCounter = value.instructionCounter;
            }
        }

        public CPUState Save()
        {
            if (this.CPUStates.Count >= Machine.MaximumRestoreStates)
            {
                this.CPUStates.RemoveFirst();
            }
            var state = this.State;
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
}
