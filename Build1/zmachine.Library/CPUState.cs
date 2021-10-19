namespace zmachine
{
    using System;

    public record CPUState
    {
        public byte[] memory;
        public byte[] stack;
        public uint lexMemoryPointer;
        public uint programCounter;
        public uint pcStart;
        public uint stackPointer;
        public uint callDepth;
        public RoutineCallState[] callStack;
        public bool finish;

        public CPUState(
            ReadOnlyMemory<byte> memory,
            ReadOnlyMemory<byte> stack,
            uint lexMemoryPointer,
            uint pc,
            uint pcStart,
            uint sp,
            uint callDepth,
            RoutineCallState[] callStack,
            bool finish)
        {
            this.memory = memory.ToArray();
            this.lexMemoryPointer = lexMemoryPointer;
            this.stack = stack.ToArray();
            this.programCounter = pc;
            this.pcStart = pcStart;
            this.stackPointer = sp;
            this.callDepth = callDepth;
            this.callStack = callStack;
            this.finish = finish;
        }
    }
}
