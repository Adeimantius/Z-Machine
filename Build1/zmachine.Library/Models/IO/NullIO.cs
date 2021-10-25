using zmachine.Library.Interfaces;

namespace zmachine.Library.Models.IO;

public class NullIO : IIO
{
    public string? ReadLine()
    {
        return null;
    }

    public void Write(string str)
    {
        return;
    }

    public void WriteLine(string str)
    {
        return;
    }

    public ConsoleKeyInfo ReadKey()
    {
        return default;
    }
}