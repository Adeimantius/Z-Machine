using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zmachine
{
    class Program
    {
        static void Main(string[] args)
        {
            //ReadData("ZORK1.DAT");

            //Memory memory = new Memory(128 * 1024); //128k main memory block
            //memory.load("ZORK1.DAT");
            //memory.dumpHeader();
            Machine machine = new Machine("ZORK1.DAT");

            int numInstructionsProcessed = 0;
            while (!machine.isFinished())
            {
                Debug.Write("" + numInstructionsProcessed + " : ");
                machine.processInstruction();
                ++numInstructionsProcessed;
                Debug.WriteLine("Object name: " + ObjectTable.objectName(1));
            }
            Debug.WriteLine("Instructions processed: " + numInstructionsProcessed);
        }

    }
}
