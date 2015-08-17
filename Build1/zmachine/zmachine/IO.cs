using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zmachine
{
    public class IO
    {
        public string ReadLine()
        {
            return Console.ReadLine();
        }
        public void Write(String str)
        {
           Console.WriteLine(str);
        }
        public void WriteLine(String str)
        {
            Console.Write(str); 
        }
        public System.ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey();
        }
    }
}
