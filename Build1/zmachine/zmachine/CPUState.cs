namespace zmachine
{
    public record CPUState
    {
        public byte[] memory;
        public byte[] stack;
        public uint pc;
    }
}
