﻿namespace zmachine
{
    using System;
    using System.Diagnostics;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            string selectedFile;
            string userSelection;

            Console.WriteLine("Welcome to Mark's implementation of the Infocom Z-machine. ");
            while (true)
            {
                Console.WriteLine("Please select a file:\n\n\t 1) Zork 1: The Final Underground\n\t 2) Hitchhiker's Guide to the Galaxy\n\t 3) Custom File");
                userSelection = Console.ReadLine();
                if (userSelection == "1") { selectedFile = "ZORK1.DAT"; }
                else if (userSelection == "2") { selectedFile = "hhgg2.z5"; }
                else if (userSelection == "3")
                {
                    Console.WriteLine("Please Enter the destination filename:");
                    selectedFile = Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Please make a valid selection.");
                    continue;
                }
                if (File.Exists(selectedFile))
                {
                    Console.WriteLine("\n\n\nFile Found. Loading...");
                    System.Threading.Thread.Sleep(3000);
                    Console.Clear();
                    break;
                }
                else
                {
                    Console.WriteLine("\n\n ==================================\n File Not Found.\n==================================\n\n");
                }
            }
            //ReadData("ZORK1.DAT");

            //Memory memory = new Memory(128 * 1024); //128k main memory block
            //memory.load("ZORK1.DAT");
            //memory.dumpHeader();
            ConsoleIO io = new ConsoleIO();
            Machine machine = new Machine(
                io: io,
                programFilename: selectedFile);

            int numInstructionsProcessed = 0;
            while (!machine.Finished)
            {
                if (machine.DebugEnabled)
                    Debug.Write("" + numInstructionsProcessed + " : ");
                machine.processInstruction();
                ++numInstructionsProcessed;
            }
            Debug.WriteLine("Instructions processed: " + numInstructionsProcessed);
        }

    }
}
