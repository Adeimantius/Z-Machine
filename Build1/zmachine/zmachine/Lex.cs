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
            List<uint> dictionaryIndex = new List<uint>();
            int[] wordStartIndex;                      

            int mp = 0;                                 // Memory Pointer

            public Lex(Memory mem)
            {
                memory = mem;
                dictionaryAddress = memory.getByte(Memory.ADDR_DICT);
            }

            public void read(int textBufferAddress, uint parseBufferAddress)
            {
                int maxInputLength = memory.getByte((uint)textBufferAddress) - 1;    // byte 0 of the text-buffer should initially contain the maximum number of letters which can be typed, minus 1
                int maxWordLength = memory.getByte((uint)parseBufferAddress);

                String input = Console.ReadLine();                                   // Get initial input from console terminal

                if (input.Length > maxInputLength)
                    input = input.Remove(maxInputLength);                            // Limit input to size of text-buffer
                input.TrimEnd('\n');                                                 // Remove carriage return from end of string  
                input = input.ToLower();                                             // Convert to lowercase

                writeToBuffer(input, textBufferAddress);                             // stored in bytes 1 onward, with a zero terminator (but without any other terminator, such as a carriage return code

                buildDict();                                                         // Build Dictionary into class variable
                String[] wordArray = parseString(input);         // Separate string by spaces and build list of word indices
                int[] wordBuffer = new int[maxWordLength];

                for (int i = 0; i < wordArray.Length; i++)
                {
                    wordBuffer[i] = compare(wordArray[i]);
                }
                // Record dictionary addresses after comparing words


                memory.setByte(parseBufferAddress + 1, (byte)(wordBuffer.Length));  // Write number of parsed words

                for (int i = 0; i < wordArray.Length; i++)
                {
                    int wordLength = wordStartIndex[i] > 0 ? wordStartIndex[i + 1] - wordStartIndex[i] : input.Length - wordStartIndex[i];
                    memory.setWord((uint)(parseBufferAddress + 2 + (4 * i)), (byte)wordBuffer[i]);      // Address in dictionary
                    memory.setByte((uint)(parseBufferAddress + 4 + (4 * i)), (byte)wordLength); // # of letters in parsed word (either from dictionary or 0)
                    memory.setByte((uint)(parseBufferAddress + 5 + (4 * i)), (byte)wordStartIndex[i]); // Corresponding word position in text buffer 
                }

            }

            public void buildDict()
            {
                // build the dictionary into class variable
                uint separatorLength = memory.getByte(dictionaryAddress);                           // Number of separators
                uint entryLength = memory.getByte(dictionaryAddress + separatorLength + 1);         // Size of each entry (default 7 bytes)
                uint dictionaryLength = memory.getWord(dictionaryAddress + separatorLength + 2);    // Number of 2-byte entries
                uint entryAddress = dictionaryAddress + separatorLength + 4;                        // Start of dictionary entries

                for (uint i = entryAddress; i < dictionaryAddress + separatorLength; i++)
                {
                    separators.Add(memory.getByte(i + 1));      // Find 'n' different word separators and add to list
                }

                for (uint i = entryAddress; i < entryAddress + dictionaryLength * 2; i += entryLength)
                {
                    dictionaryIndex.Add(i);         // Record dictionary entry address
                    Memory.StringAndReadLength dictEntry = memory.getZSCII(i, 4);
//                    Console.WriteLine(dictEntry.str);
                    dictionary.Add(dictEntry.str);                                           // Find 'n' different dictionary entries and add words to list
                }
            }

            public int compare(String word, int dictionaryFlag = 0)
            {
                // search dictionary for comparisons
                for (int i = 0; i < dictionary.Count; i++)
                {
                        if (dictionary[i] == word)
                            return memory.getByte((uint)dictionaryIndex[i]);
                }
                Console.WriteLine("Could not identify keyword:" + word);
                return 0;
            }

            public String[] parseString(String input)
            {
                int wordindex = 0;

                String[] wordArray = input.Split(' ');        // Tokenize into words
                wordStartIndex = new int[wordArray.Length + 1];

                // Record start index of each word in input string
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

                bool stringComplete = false;

                while (stringComplete != true)
                {
                    ushort word = 0;

                    for (int j = 0; j < 3; j++)
                    {   
                        if (i + j > input.Length - 1)
                        {
                            c[j] = Convert.ToInt32('5');            // Pad word with 5's.
                            stringComplete = true;
                        }
                        else                                        // Write next char from input into 3-char array
                        {
                            c[j] = convertASCIIToZSCII(input[i + j]);                    
                        }
                        word += (ushort)(c[j] << 10 - (5 * j));     // Write 3 chars into word
                    }
                    //word += (ushort)(c[0] << 10);
                    //word += (ushort)(c[1] << 5);
                    //word += (ushort)(c[2]);
//                    Console.WriteLine("Converted ZSCII string: " + word);

                    memory.setWord((uint)(address + mp), word);      // Write word to memory
                    i += 3;
                    mp += 2;
                }
                memory.setWord((uint)(address + mp), 0);       // Write empty word to terminate after read is complete.
                
            }

            public int convertASCIIToZSCII(char ascii)
            {
                // Convert into ZSCII from ASCII char
                if (ascii == ' ')
                    return 0;
                else if (
                    ascii >= 32 && ascii <= 126 ||      // standard ascii
                    ascii >= 155 && ascii <= 251)       // Take in ascii and return 10-bit zscii
                    return (int)ascii;
                else
                {
                    Console.WriteLine("Invalid character: " + ascii);
                    return ' ';
                }
            }





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
