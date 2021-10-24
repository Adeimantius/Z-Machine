namespace zmachine.Library;

public class StaticIO : IIO
{
    private StringReader inputReader;
    private StringWriter outputWriter;

    public StaticIO(string? initialInput = null)
    {
        this.inputReader = new StringReader(initialInput is not null ? initialInput : "");
        this.outputWriter = new StringWriter();
    }

    public string? ReadLine()
    {
        return this.inputReader.ReadLine();
    }

    public void Write(string str)
    {
        this.outputWriter.Write(str);
    }

    public void WriteLine(string str)
    {
        this.outputWriter.WriteLine(str);
    }

    /// <summary>
    ///     Translate stored byte into key code/key press
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ConsoleKeyInfo ReadKey()
    {
        char[] keyArray = new char[1];
        this.inputReader.Read(keyArray, 0, 1);
        char key = keyArray[0];
        char keyUcase = Char.ToUpperInvariant(key);

        if (key >= 'a' && key <= 'z' || key >= '0' && key <= '9')
        {
            ConsoleKey console = (ConsoleKey)Enum.Parse(
                typeof(ConsoleKey),
                new ReadOnlySpan<char>(
                    new char[] { keyUcase },
                    0,
                    1),
                true);
            return new ConsoleKeyInfo(
                keyUcase,
                console,
                false,
                false,
                false);
        }

        if (key >= 'A' && key <= 'Z')
        {
            ConsoleKey console = (ConsoleKey)Enum.Parse(
                typeof(ConsoleKey),
                new ReadOnlySpan<char>(
                    new char[] { keyUcase },
                    0,
                    1),
                false);
            return new ConsoleKeyInfo(
                key,
                console,
                true,
                false,
                false);
        }

        if (key == ' ')
        {
            return new ConsoleKeyInfo(
                key,
                ConsoleKey.Spacebar,
                false,
                false,
                false);
        }
        //TODO: punctuation

        throw new NotImplementedException();
    }

    public string GetOutput(bool keepContents = false)
    {
        this.outputWriter.Flush();
        string output = this.outputWriter.ToString();
        if (!keepContents)
        {
            this.outputWriter.Dispose();
            this.outputWriter = new StringWriter();
        }

        return output;
    }

    public void SetInput(string value)
    {
        this.inputReader = new StringReader(value);
    }
}