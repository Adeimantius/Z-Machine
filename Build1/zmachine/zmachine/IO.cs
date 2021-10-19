using System;

namespace zmachine
{
    public class IO : IIO
    {
        public string ReadLine()
        {
            return Console.ReadLine();
        }
        public void Write(string str)
        {
            Console.WriteLine(str);
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
