namespace zmachine.Library.Interfaces;

public interface IIO
{
    public string? ReadLine();
    public void Write(string str);
    public void WriteLine(string str);
    public ConsoleKeyInfo ReadKey();
}