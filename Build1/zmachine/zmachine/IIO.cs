using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zmachine
{
    public interface IIO
    {
        public string ReadLine();
        public void Write(string str);
        public void WriteLine(string str);
        public System.ConsoleKeyInfo ReadKey();
    }
}
