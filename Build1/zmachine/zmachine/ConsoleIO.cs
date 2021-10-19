using System;

namespace zmachine
{
    public class ConsoleIO : IIO
    {
        public string ReadLine()
        {
            return Console.ReadLine();
        }
        public void Write(string str)
        {
            Console.WriteLine(str); // write/writeline came reversed in the fork.... -JM
        }
        public void WriteLine(string str)
        {
            Console.Write(str);
        }
        public System.ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey();
        }
    }
}
