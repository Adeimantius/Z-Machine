namespace zmachine.Library;

public class Crc32
{
    private readonly uint[] table;

    public Crc32()
    {
        uint poly = 0xedb88320;
        this.table = new uint[256];
        uint temp = 0;
        for (uint i = 0; i < this.table.Length; ++i)
        {
            temp = i;
            for (int j = 8; j > 0; --j)
            {
                if ((temp & 1) == 1)
                {
                    temp = (temp >> 1) ^ poly;
                }
                else
                {
                    temp >>= 1;
                }
            }

            this.table[i] = temp;
        }
    }

    public uint ComputeChecksum(byte[] bytes)
    {
        uint crc = 0xffffffff;
        for (int i = 0; i < bytes.Length; ++i)
        {
            byte index = (byte)((crc & 0xff) ^ bytes[i]);
            crc = (crc >> 8) ^ this.table[index];
        }

        return ~crc;
    }

    public byte[] ComputeChecksumBytes(byte[] bytes)
    {
        return BitConverter.GetBytes(this.ComputeChecksum(bytes));
    }
}