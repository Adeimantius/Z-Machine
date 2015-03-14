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
            int mp = 0;                                 // Memory Pointer

            public Lex(Memory mem)
            {
                memory = mem;
                dictionaryAddress = memory.getByte(Memory.ADDR_DICT);
            }

            public void tokenize(List<String> input)
            {
                uint separatorLength = memory.getByte(dictionaryAddress);
                uint entryLength = memory.getByte(dictionaryAddress + separatorLength + 1);
                uint dictionaryLength = memory.getWord(dictionaryAddress + separatorLength + 2);

                for (uint i = dictionaryAddress; i < separatorLength; i++)
                {
                    separators.Add(memory.getByte(i + 1));      // Find 'n' different word separators and add to list

                }
                for (uint i = dictionaryAddress; i < dictionaryLength; i += entryLength)
                {
                    String word = getZSCII(i + separatorLength + 1, 0).str;         // Take dictionary entries and convert to zchars
                    dictionary.Add(word);                                           // Find 'n' different dictionary entries and add words to list
                }

                for (int i = 0; i < dictionary.Count; i++)
                {
                    // convert dictionary from words to zchars
                }

            }

            public String searchDict(String word)
            {
                for (int i = 0; i < dictionary.Count; i++)
                {
                    if (dictionary[i] == word)
                    {
                        return dictionary[i];
                    }
                }
                return "Could not identify keyword:" + word;
            }

            public void parseWord(String word)
            {
                for (int i = 0; i < wordArray.Length; i++)    // Look up in dictionary 
                {
                    int j = 0;
                    while (wordArray[i] == dictionary[j])
                        j++;                                // figure out what to do when a match is found between input and the dictionary.
                }    

            }

            // Store string (in ASCII) at address in byte 1 onward with a zero terminator. 
            public void readToMemory(String input, int address) 
            {
                int[] c = new int[3];        // Store three zchars across 16 bits (two bytes). May have to make a dynamic list and pad it once every 3 reads.
                int i;
                mp = address + 1;
                ushort word = 0;
                bool stringComplete = false;

                while (stringComplete != true)
                {
                    for (int j; j < 3; j++)
                    {   
                        if (i + j > input.Length)
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
            public void read(int textBufferAddress, int parseBuffer)
            {

                // NOTE: These is a String.Contains function that may be of great value with the resulting zstring..!
                int maxInputLength = memory.getByte((uint)textBufferAddress) - 1; //byte 0 of the text-buffer should initially contain the maximum number of letters which can be typed, minus 1
                int maxWordLength = memory.getByte((uint) parseBuffer);

                int zstringLength;
                String input = "";
            
                input += Console.ReadLine();                    // Get initial input from console terminal
                input.Remove(maxInputLength);                   // Limit input to size of text-buffer
                input.TrimEnd('\n');                            // Remove carriage return from end of string  
                input.ToLower();                                // Convert to lowercase

                input = readToMemory(input, textBufferAddress); //stored in bytes 1 onward, with a zero terminator (but without any other terminator, such as a carriage return code
            }


                int wordindex = 0;

                int[] wordStartIndex;                      
                String[] wordArray = input.Split(' ');        // Tokenize into words
                for (int i = 0; i < wordArray.Length; i++)
                {
                    wordStartIndex[i] = wordindex;                                // take index of word
                                                                 
                    for (int j = 0; j < wordArray[i].Length; j++)
                    {
                        wordindex++;                                              // Add 1 for each char
                    }
                    wordindex++;                                                  // Add 1 for each space
                }

                List<String> zstringArray = new List<String>();                                            // Make array to hold onto zstrings
                getZSCII(null, null);                                             // Convert to Zchars



                int padLength = 0; 

                String last = zstringArray[zstringArray.Count - 1];
                zstringArray = zstringArray.RemoveAt(zstringArray.Count - 1);  // pop the last zstring in the array
                              // Figure out size in memory  
                padLength = last.Length % 3;                   //--------------------------------- Get length of zstring and check if last word needs padding
                last.PadRight(padLength, '5');                 // Pad with 5s
                zstringArray = zstringArray.Add(last);

                for (int i = 0; i < zstringArray.Count; i += 3)    // Pack into 3-char pieces as a Zstring
                {
                    Concatenate 3 consecutive chars into a word;
                    memory.setWord((uint)(textBufferAddress + (2 + 4 * i)), (ushort)zstringArray[i]);    // Write number of words in byte 1, write words from byte 2 onward (stopping at maxWordLength;
                }
                                                                //

    //          tokenize(input)                                 // Tokenize input using the main dictionary
                setVar(firstoperand, zstringArray[i]);                                    // Store string in buffer in first operand 

                /*  sequence of characters is read in from the current input stream until a carriage return
             *   
             *  The text typed is reduced to lower case (so that it can tidily be printed back by the program if need be) and stored in bytes 1 onward, with a zero terminator (not a carriage return!)
             *  There are two input streams - the file containing commands (ZORK1) and the keyboard. Op_input_stream will swap them.
                 *  
                 *  if byte 1 contains a positive value at the start of the input, then read assumes that number of characters are left over from an interrupted previous input, 
                 *  and writes the new characters after those already there.
                 *  ^ I'm going to hate this.
                 *  
                 *  byte 0 of the parse-buffer should hold the maximum number of textual words which can be parsed. 
                 *  (If this is n, the buffer must be at least 2 + 4*n bytes long to hold the results of the analysis.)
                 *  
                 * The interpreter divides the text into words and looks them up in the dictionary, as described in S 13.
                 * The number of words is written in byte 1 and one 4-byte block is written for each word, from byte 2 onwards 
                 * (except that it should stop before going beyond the maximum number of words specified). 
                 * Each block consists of the byte address of the word in the dictionary, if it is in the dictionary, or 0 if it isn't; 
                 * followed by a byte giving the number of letters in the word; and finally a byte giving the position in the text-buffer of the first letter of the word.
            */
                return input;
                }

            public char readChar()
            {
                // read keypress and pass as a char into getZchar
                return '0';
            }

        }
}
