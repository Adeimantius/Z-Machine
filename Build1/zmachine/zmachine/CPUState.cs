using System;
using static zmachine.Machine;

namespace zmachine
{
    public record CPUState
    {
        public byte[] memory;
        public byte[] stack;
        public uint pc;
        public uint pcStart;
        public uint sp;
        public uint callDepth;
        public RoutineCallState[] callStack;
        public bool finish;

        public CPUState(ReadOnlyMemory<byte> memory, ReadOnlyMemory<byte> stack, uint pc, uint pcStart, uint sp, uint callDepth, RoutineCallState[] callStack, bool finish)
        {
            this.memory = memory.ToArray();
            this.stack = stack.ToArray();
            this.pc = pc;
            this.pcStart = pcStart;
            this.sp = sp;
            this.callDepth = callDepth;
            this.callStack = callStack;
            this.finish = finish;
        }
    }
}
