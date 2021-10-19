namespace zmachine
{
    using System;

    public partial class Machine
    {

        public CPUState State
        {
            get
            {
                return new CPUState(
                    memory: this.memory.Contents,
                    stack: this.stack.Contents,
                    lexMemoryPointer: this.lex.MemoryPointer,
                    pc: this.programCounter,
                    pcStart: this.pcStart,
                    sp: this.stackPointer,
                    callDepth: this.callDepth,
                    callStack: this.callStack,
                    finish: this.finish);
            }
            set
            {
                this.memory.load(value.memory);
                this.stack.load(value.stack);
                this.lex.MemoryPointer = value.lexMemoryPointer;
                this.programCounter = value.programCounter;
                this.pcStart = value.pcStart;
                this.stackPointer = value.stackPointer;
                this.callDepth = value.callDepth;
                Array.Copy(
                    sourceArray: value.callStack,
                    destinationArray: this.callStack,
                    length: StackDepth);
                this.finish = value.finish;
            }
        }

        public string stateString()
        {
            string s = "M: " + memory.getCrc32() + " S: " + stack.getCrc32();
            //            for (ushort i = 1; i < 256; ++i)
            //                s += " " + getVar(i);
            return s;
        }
    }
}
