using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading;

namespace zmachine
{
    class Program:MonoBehaviour 
    {

        static void Start(string[] args)
        {
            //ReadData("ZORK1.DAT");

            //Memory memory = new Memory(128 * 1024); //128k main memory block
            //memory.load("ZORK1.DAT");
            //memory.dumpHeader();
            Machine machine = new Machine("ZORK1.DAT");

            int numInstructionsProcessed = 0;
            while (!machine.isFinished())
            {
                if (machine.debug)
                    Debug.Log("" + numInstructionsProcessed + " : ");
                machine.processInstruction();
                ++numInstructionsProcessed;
            }
            Debug.Log("Instructions processed: " + numInstructionsProcessed);
        }

    }
}
