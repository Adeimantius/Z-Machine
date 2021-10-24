namespace zmachine.Library;

public class ConsoleIO : IIO
{
    public string? ReadLine()
    {
        return Console.ReadLine();
    }

    public void Write(string str)
    {
        Console.Write(str);
    }

    public void WriteLine(string str)
    {
        Console.WriteLine(str);
    }

    public ConsoleKeyInfo ReadKey()
    {
        return Console.ReadKey();
    }
}