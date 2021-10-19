﻿namespace zmachine
{
    using System;
    using System.IO;

    public class StaticIO : IIO
    {
        private readonly StringReader inputReader;
        private readonly StringWriter outputWriter;

        public StaticIO(string initialInput)
        {
            this.inputReader = new StringReader(s: initialInput);
            this.outputWriter = new StringWriter();
        }

        public string ReadLine()
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
        /// Translate stored byte into key code/key press
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public System.ConsoleKeyInfo ReadKey()
        {
            char[] key = new char[1];
            this.inputReader.Read(key, 0, 1);

            if ((key[0] >= 'a' && key[0] <= 'z') || (key[0] >= '0' && key[0] <= '9'))
            {
                var ucase = key.ToString().ToUpperInvariant().ToCharArray();
                var console = (ConsoleKey)Enum.Parse(
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
                var console = (ConsoleKey)Enum.Parse(
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
