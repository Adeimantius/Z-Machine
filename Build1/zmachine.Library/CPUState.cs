namespace zmachine.Library
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
        public ulong instructionCounter;

        public CPUState() : this(
            memory: new byte[Machine.MemorySize],
            stack: new byte[Machine.StackSize],
            lexMemoryPointer: 0,
            pc: 0,
            pcStart: 0,
            sp: 0,
            callDepth: 0,
            callStack: new RoutineCallState[Machine.StackDepth],
            finish: false,
            instructionCounter: 0)
        {
        }

        public CPUState(
            ReadOnlyMemory<byte> memory,
            ReadOnlyMemory<byte> stack,
            uint lexMemoryPointer,
            uint pc,
            uint pcStart,
            uint sp,
            uint callDepth,
            RoutineCallState[] callStack,
            bool finish,
            ulong instructionCounter = 0)
        {
            this.memory = memory.ToArray();
            this.lexMemoryPointer = lexMemoryPointer;
            this.stack = stack.ToArray();
            programCounter = pc;
            this.pcStart = pcStart;
            stackPointer = sp;
            this.callDepth = callDepth;
            this.callStack = callStack;
            this.finish = finish;
            this.instructionCounter = instructionCounter;
        }
    }
}
