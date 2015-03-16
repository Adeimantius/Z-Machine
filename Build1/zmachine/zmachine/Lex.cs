using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zmachine
{

        public class Lex
        {
            Memory memory;
            uint dictionaryAddress;
            List<ushort> separators = new List<ushort>();
            List<String> dictionary = new List<String>();
            List<int> dictionaryIndex = new List<int>();
            int mp = 0;                                 // Memory Pointer

            public Lex(Memory mem)
            {
                memory = mem;
                dictionaryAddress = memory.getByte(Memory.ADDR_DICT);
            }

            public void read(int textBufferAddress, uint parseBufferAddress)
            {

                // NOTE: These is a String.Contains function that may be of great value with the resulting zstring..!
                int maxInputLength = memory.getByte((uint)textBufferAddress) - 1; //byte 0 of the text-buffer should initially contain the maximum number of letters which can be typed, minus 1
                int maxWordLength = memory.getByte((uint) parseBufferAddress);

                int[] wordBuffer = new int[maxWordLength];           
                String input = Console.ReadLine();                    // Get initial input from console terminal

                if (input.Length > maxInputLength)
                    input.Remove(maxInputLength);                   // Limit input to size of text-buffer
                input.TrimEnd('\n');                            // Remove carriage return from end of string  
                input.ToLower();                                // Convert to lowercase
                writeToBuffer(input, textBufferAddress);        //stored in bytes 1 onward, with a zero terminator (but without any other terminator, such as a carriage return code

                String[] wordArray = parseWord(input);

                buildDict();
                for (int i = 0; i < wordArray.Length; i++) 
                {
                    String word = wordArray[i];
                    wordBuffer[i] = compare(word);
                }

                memory.setByte(parseBufferAddress + 1, (byte)(wordBuffer.Length));

                for (int i = 0; i < maxWordLength; i++)
                {
                    memory.setWord((uint)(parseBufferAddress + 2 + (4 * i)), (byte)wordBuffer[i]);
                    memory.setByte((uint)(parseBufferAddress + 4 + (4 * i)), (byte)dictionary[i].Length);
                    memory.setByte((uint)(parseBufferAddress + 5 + (4 * i)), (byte)dictionaryIndex[i]);
                }
                    
            }

            public void buildDict()
            {
                // First build the dictionary
                uint separatorLength = memory.getByte(dictionaryAddress);
                uint entryLength = memory.getByte(dictionaryAddress + separatorLength + 1);
                uint dictionaryLength = memory.getWord(dictionaryAddress + separatorLength + 2);

                for (uint i = dictionaryAddress; i < separatorLength; i++)
                {
                    separators.Add(memory.getByte(i + 1));      // Find 'n' different word separators and add to list
                }
                for (uint i = dictionaryAddress; i < dictionaryLength; i += entryLength)
                {
                    dictionaryIndex.Add(memory.getWord(i + separatorLength + 1));         // Record dictionary entry address
                    Memory.StringAndReadLength dictEntry = memory.getZSCII(i + separatorLength + 1, 0);
                    dictionary.Add(dictEntry.str);                                           // Find 'n' different dictionary entries and add words to list
                }


                // search Dictionary for comparisons

            }

            public int compare(String word, int dictionaryFlag = 0)
            {
                for (int i = 0; i < dictionary.Count; i++)
                {
                    if (dictionary[i] == word)
                    {
                        return memory.getByte((uint)dictionaryIndex[i]);
                    }
                }
                Console.WriteLine("Could not identify keyword:" + word);
                return 0;
            }

            public String[] parseWord(String input)
            {
                                // --------------------------------------------------------- What do I do with this stuff?                
                int wordindex = 0;

                String[] wordArray = input.Split(' ');        // Tokenize into words
                int[] wordStartIndex = new int[wordArray.Length];                      

                for (int i = 0; i < wordArray.Length; i++)
                {
                    wordStartIndex[i] = wordindex;                                // take index of word
                                                                 
                    for (int j = 0; j < wordArray[i].Length; j++)
                    {
                        wordindex++;                                              // Add 1 for each char
                    }
                    wordindex++;                                                  // Add 1 for each space
                }

                return wordArray;
            }

            // Store string (in ASCII) at address in byte 1 onward with a zero terminator. 
            public void writeToBuffer(String input, int address) 
            {
                int[] c = new int[3];        // Store three zchars across 16 bits (two bytes). May have to make a dynamic list and pad it once every 3 reads.
                int i = 0;
                mp = address + 1;
                ushort word = 0;
                bool stringComplete = false;

                while (stringComplete != true)
                {
                    for (int j = 0; j < 3; j++)
                    {   
                        if (i + j > input.Length - 1)
                        {
                            c[j] = Convert.ToInt32('5');            // Pad word with 5's.
                            stringComplete = true;
                        }
                        else                                        // Write next char from input into 3-char array
                        {
                            c[j] = input[i + j];                    
                        }
                        word += (ushort)(c[j] << 10 - (5 * j));     // Write 3 chars into word
                    }
                    //word += (ushort)(c[0] << 10);
                    //word += (ushort)(c[1] << 5);
                    //word += (ushort)(c[2]);
                    memory.setWord((uint)(address + mp), word);      // Write word to memory
                    i += 3;
                    mp += 2;
                }
                memory.setWord((uint)(address + mp), 0);       // Write empty word to terminate after read is complete.
                
            }

            // Get console input and fix it to ZSpec standards.




    //            List<String> zstringArray = new List<String>();                                            // Make array to hold onto zstrings
    //            getZSCII(null, null);                                             // Convert to Zchars



    //            int padLength = 0; 

    //            String last = zstringArray[zstringArray.Count - 1];
    //            zstringArray = zstringArray.RemoveAt(zstringArray.Count - 1);  // pop the last zstring in the array
    //                          // Figure out size in memory  
    //            padLength = last.Length % 3;                   //--------------------------------- Get length of zstring and check if last word needs padding
    //            last.PadRight(padLength, '5');                 // Pad with 5s
    //            zstringArray = zstringArray.Add(last);

    //            for (int i = 0; i < zstringArray.Count; i += 3)    // Pack into 3-char pieces as a Zstring
    //            {
    //                Concatenate 3 consecutive chars into a word;
    //                memory.setWord((uint)(textBufferAddress + (2 + 4 * i)), (ushort)zstringArray[i]);    // Write number of words in byte 1, write words from byte 2 onward (stopping at maxWordLength;
    //            }
    //                                                            //

    ////          tokenize(input)                                 // Tokenize input using the main dictionary
    //            setVar(firstoperand, zstringArray[i]);                                    // Store string in buffer in first operand 



            public char readChar()
            {
                memory.getZChar(Convert.ToChar(Console.ReadKey()));// read keypress and pass as a char into getZchar
                return '0';
            }

        }
}
