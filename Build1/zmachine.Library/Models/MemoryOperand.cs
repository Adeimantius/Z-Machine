namespace zmachine.Library.Models;

public class MemoryOperand
{
    private readonly Memory memory;
    private readonly uint offset;

    public MemoryOperand(Memory memory, uint offset)
    {
        this.memory = memory;
        this.offset = offset;
    }
}