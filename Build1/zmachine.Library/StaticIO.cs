namespace zmachine.Library
{
    using System;
    using System.IO;

    public class StaticIO : IIO
    {
        private StringReader inputReader;
        private StringWriter outputWriter;

        public string GetOutput(bool keepContents = false)
        {
            this.outputWriter.Flush();
            var output = this.outputWriter.ToString();
            if (!keepContents)
            {
                this.outputWriter.Dispose();
                this.outputWriter = new StringWriter();
            }
            return output;
        }

        public StaticIO(string? initialInput = null)
        {
            inputReader = new StringReader(s: initialInput is not null ? initialInput : "");
            outputWriter = new StringWriter();
        }

        public void SetInput(string value)
        {
            this.inputReader = new StringReader(value);
        }

        public string? ReadLine()
        {
            return inputReader.ReadLine();
        }

        public void Write(string str)
        {
            outputWriter.Write(str);
        }

        public void WriteLine(string str)
        {
            outputWriter.WriteLine(str);
        }

        /// <summary>
        /// Translate stored byte into key code/key press
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public System.ConsoleKeyInfo ReadKey()
        {
            char[] key = new char[1];
            inputReader.Read(key, 0, 1);

            if ((key[0] >= 'a' && key[0] <= 'z') || (key[0] >= '0' && key[0] <= '9'))
            {
                char[]? ucase = key.ToString().ToUpperInvariant().ToCharArray();
                ConsoleKey console = (ConsoleKey)Enum.Parse(
                    enumType: typeof(ConsoleKey),
                    value: new ReadOnlySpan<char>(
                        array: ucase,
                        start: 0,
                        length: 1),
                    ignoreCase: true);
                return new ConsoleKeyInfo(
                    keyChar: ucase[0],
                    key: console,
                    shift: false,
                    alt: false,
                    control: false);
            }
            else if (key[0] >= 'A' && key[0] <= 'Z')
            {
                ConsoleKey console = (ConsoleKey)Enum.Parse(
                    enumType: typeof(ConsoleKey),
                    value: new ReadOnlySpan<char>(
                        array: key,
                        start: 0,
                        length: 1),
                    ignoreCase: false);
                return new ConsoleKeyInfo(
                    keyChar: key[0],
                    key: console,
                    shift: true,
                    alt: false,
                    control: false);
            }
            else if (key[0] == ' ')
            {
                return new ConsoleKeyInfo(
                    keyChar: key[0],
                    key: ConsoleKey.Spacebar,
                    shift: false,
                    alt: false,
                    control: false);
            }

            throw new NotImplementedException();
        }

    }
}
