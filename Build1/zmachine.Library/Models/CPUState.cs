namespace zmachine.Library.Models;

public record CPUState
{
    public uint callDepth;
    public RoutineCallState[] callStack;
    public bool finish;
    public ulong instructionCounter;
    public uint lexMemoryPointer;
    public byte[] memory;
    public uint pcStart;
    public uint programCounter;
    public byte[] stack;
    public uint stackPointer;

    public CPUState() : this(
        new byte[Machine.MemorySizeByVersion[Machine.CurrentVersion]],
        new byte[Machine.StackSize],
        0,
        0,
        0,
        0,
        0,
        new RoutineCallState[Machine.StackDepth],
        false,
        0)
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
        ulong instructionCounter)
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
        this.instructionCounter = instructionCounter;
    }
}