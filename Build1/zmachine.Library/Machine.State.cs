namespace zmachine.Library
{
    using System;

    public partial class Machine
    {
        public CPUState State
        {
            get => new CPUState(
                    memory: Memory.Contents,
                    stack: stack.Contents,
                    lexMemoryPointer: lex.MemoryPointer,
                    pc: programCounter,
                    pcStart: pcStart,
                    sp: stackPointer,
                    callDepth: callDepth,
                    callStack: callStack,
                    finish: finishProcessing,
                    instructionCounter: InstructionCounter);
            set
            {
                Memory.load(value.memory);
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
                finishProcessing = value.finish;
                InstructionCounter = value.instructionCounter;
            }
        }

        public string stateString()
        {
            string s = "M: " + Memory.getCrc32() + " S: " + stack.getCrc32();
            //            for (ushort i = 1; i < 256; ++i)
            //                s += " " + getVar(i);
            return s;
        }
    }
}
