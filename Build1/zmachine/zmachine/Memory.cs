using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zmachine
{
    class Memory
    {
        byte[] memory;

        public Memory (int size)
        {
            // Class constructor
           memory = new byte[size];    
        }
 
        //input byte array [] from file, output size specified byte array []
        public void load(string filename)
        {
            //Load data into temp variable, copy into specified byte array

            byte[] src = File.ReadAllBytes(filename);

            if (src.Length <= memory.Length)
            {
            
                int i = 0;
                foreach (byte b in src)
                {
                    //read through bytes and write in new byte array []
                    memory[i] = src[i];
                    i++;
                }
            }
     
        }
        //assign given byte value @ hex address location
        public void setByte(uint address, byte val)
        {
            memory[address] = val;
        }


        //access given byte location & return byte stored in data
        public byte getByte(uint address)
        { 
            return (byte) memory[address];
        }


        //assign 16-bit value from given 16-bit memory address
        public void setWord(uint address, ushort val)
        {
            byte a = (byte) ((val >> 8) & 0xff);
            byte b = (byte) (val & 0xff);

            // byte[] a = BitConverter.GetBytes(val);
            // byte first = (byte) a[0];
            // byte second = (byte) a[1];
            // first <<= 8;                             // shift first byte up 8 bits
            memory[address] = a;
            memory[address + 1] = b;     
        }


        public ushort getWord(uint address)
        { 
            //access 16-bit value and return "word" stored in data
            //read in two bytes, shift first byte left (<<) 8 bits
            byte a = memory[address];
            byte b = memory[address + 1];
            ushort val = (ushort) ((a << 8) + b);

            return val;
        }

        public short getSignedWord(uint address)
        {
            return (short)getWord(address);
        }

        public static ushort ADDR_VERSION = 0x00;
        public static ushort ADDR_HIGH = 0x04;
        public static ushort ADDR_INITIALPC = 0x06;
        public static ushort ADDR_GLOBALS = 0x0c;
        public static ushort ADDR_DICT = 0x08;
        public static ushort ADDR_OBJECTS = 0x0a;
        public static ushort ADDR_ABBREVS = 0x18;

        // Print the file header
        public void dumpHeader()
        {
            Debug.WriteLine("Type : " + getByte(ADDR_VERSION));
            Debug.WriteLine("Base : " + getWord(ADDR_HIGH));
            Debug.WriteLine("PC   : " + getWord(ADDR_INITIALPC));
            Debug.WriteLine("Dict : " + getWord(ADDR_DICT));
            Debug.WriteLine("Obj  : " + getWord(ADDR_OBJECTS));

        }


    }
}
