namespace zmachine
{
    using System;

    public partial class Machine
    {

        public CPUState State
        {
            get => new CPUState(
                    memory: memory.Contents,
                    stack: stack.Contents,
                    lexMemoryPointer: lex.MemoryPointer,
                    pc: programCounter,
                    pcStart: pcStart,
                    sp: stackPointer,
                    callDepth: callDepth,
                    callStack: callStack,
                    finish: finish);
            set
            {
                memory.load(value.memory);
                stack.load(value.stack);
                lex.MemoryPointer = value.lexMemoryPointer;
                programCounter = value.programCounter;
                pcStart = value.pcStart;
                stackPointer = value.stackPointer;
                callDepth = value.callDepth;
                Array.Copy(
                    sourceArray: value.callStack,
                    destinationArray: callStack,
                    length: StackDepth);
                finish = value.finish;
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
