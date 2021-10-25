﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using zmachine.Library.Models;
using zmachine.Library.Models.IO;

namespace zmachine;

internal class Program
{
    private static void Main(string[] args)
    {
        string selectedFile;
        string userSelection;

        Console.WriteLine("Welcome to Mark's implementation of the Infocom Z-machine. ");
        while (true)
        {
            Console.WriteLine(
                "Please select a file:\n\n\t 1) Zork 1: The Final Underground\n\t 2) Hitchhiker's Guide to the Galaxy\n\t 3) Custom File");
            userSelection = Console.ReadLine();
            if (userSelection == "1")
            {
                selectedFile = "ZORK1.DAT";
            }
            else if (userSelection == "2")
            {
                selectedFile = "hhgg.z5";
            }
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
                Thread.Sleep(3000);
                Console.Clear();
                break;
            }

            Console.WriteLine(
                "\n\n ==================================\n File Not Found.\n==================================\n\n");
        }
        //ReadData("ZORK1.DAT");

        //Memory memory = new Memory(128 * 1024); //128k main memory block
        //memory.load("ZORK1.DAT");
        //memory.dumpHeader();
        ConsoleIO io = new ConsoleIO();
        Machine machine = new Machine(
            io,
            selectedFile);

        while (!machine.Finished)
        {
            if (machine.DebugEnabled)
            {
                Debug.Write("" + machine.InstructionCounter + " : ");
            }

            machine.processInstruction();
        }

        Debug.WriteLine("Instructions processed: " + machine.InstructionCounter);
    }
}