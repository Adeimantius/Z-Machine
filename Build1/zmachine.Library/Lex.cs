﻿namespace zmachine.Library
{
    using System;
    using System.Collections.Generic;

    public class Lex
    {
        private readonly Memory memory;
        private readonly uint dictionaryAddress;
        private readonly List<ushort> separators = new List<ushort>();
        private readonly List<string> dictionary = new List<string>();
        private readonly List<uint> dictionaryIndex = new List<uint>();
        private int[] wordStartIndex;
        private readonly IIO io;
        /// <summary>
        /// memory pointer
        /// </summary>
        private uint memoryPointer;

        public Lex(IIO io, ref Memory mem, uint mp = 0)
        {
            this.io = io;
            this.memory = mem;
            this.memoryPointer = mp;
            this.dictionaryAddress = this.memory.getWord(Memory.ADDR_DICT);
        }

        public uint MemoryPointer
        {
            get => this.memoryPointer;
            set => this.memoryPointer = value;
        }

        public void read(int textBufferAddress, uint parseBufferAddress)
        {
            int maxInputLength = this.memory.getByte((uint)textBufferAddress) - 1;    // byte 0 of the text-buffer should initially contain the maximum number of letters which can be typed, minus 1
            int parseBufferLength = this.memory.getByte(parseBufferAddress);
            this.memoryPointer = parseBufferAddress + 2;
            string input = this.io.ReadLine();                                   // Get initial input from io terminal

            if (input.Length > maxInputLength)
            {
                input = input.Remove(maxInputLength);                            // Limit input to size of text-buffer
            }

            input.TrimEnd('\n');                                                 // Remove carriage return from end of string  
            input = input.ToLower();                                             // Convert to lowercase

            this.writeToBuffer(input, textBufferAddress);                             // stored in bytes 1 onward, with a zero terminator (but without any other terminator, such as a carriage return code

            if (parseBufferLength > 0)                                               // Check to see if lexical analysis is called for
            {
                this.buildDict();                                                     // Build Dictionary into class variable
                string[] wordArray = this.parseString(input);                         // Separate string by spaces and build list of word indices
                uint[] matchedWords = new uint[parseBufferLength];

                for (int i = 0; i < wordArray.Length; i++)
                {
                    matchedWords[i] = this.compare(wordArray[i]);                       // Stores the dictionary address of matched words (or 0 if no match)
                                                                                        //                       Debug.WriteLine("Byte address of matched word: " + matchedWords[i]);
                }
                // Record dictionary addresses after comparing words


                this.memory.setByte(this.memoryPointer - 1, (byte)(wordArray.Length));  // Write number of parsed words

                for (int i = 0; i < wordArray.Length; i++)
                {

                    //                    if (4 * (i + 1) < parseBufferLength)
                    //                    {                            
                    int wordLength = wordArray[i].Length;
                    this.memory
                        .setWord(this.memoryPointer, (ushort)matchedWords[i])      // Address in dictionary of matches (either from dictionary or 0)
                        .setByte(this.memoryPointer + 2, (byte)wordLength)         // # of letters in parsed word 
                        .setByte(this.memoryPointer + 3, (byte)this.wordStartIndex[i]); // Corresponding word position in text buffer 
                    //                     }
                    this.memoryPointer += 4;
                    this.memory.setByte(this.memoryPointer, 0);
                }
            }

        }

        // Store string (in ZSCII) at address in byte 1 onward with a zero terminator. 
        public Lex writeToBuffer(string input, int address)
        {
            int i;
            for (i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                // Convert these to ZSCII here...     
                this.memory.setByte((uint)(address + i + 1), (byte)ch);
            }
            // Write next char from input into 3-char array (unimplemented)

            this.memory.setByte((uint)(address + i + 1), 0);       // Write empty byte to terminate after read is complete.
                                                                   // io.WriteLine("Converted ZSCII string: " + memory.getZSCII((uint)(address + 1), 0).str);
            return this;
        }

        public Lex buildDict()
        {
            // build the dictionary into class variable
            uint separatorLength = this.memory.getByte(this.dictionaryAddress);                           // Number of separators
            uint entryLength = this.memory.getByte(this.dictionaryAddress + separatorLength + 1);         // Size of each entry (default 7 bytes)
            uint dictionaryLength = this.memory.getWord(this.dictionaryAddress + separatorLength + 2);    // Number of 2-byte entries
            uint entryAddress = this.dictionaryAddress + separatorLength + 4;                        // Start of dictionary entries

            for (uint i = entryAddress; i < this.dictionaryAddress + separatorLength; i++)
            {
                this.separators.Add(this.memory.getByte(i + 1));      // Find 'n' different word separators and add to list
            }

            for (uint i = entryAddress; i < entryAddress + dictionaryLength * entryLength; i += entryLength)
            {
                this.dictionaryIndex.Add(i);         // Record dictionary entry address
                Memory.StringAndReadLength dictEntry = this.memory.getZSCII(address: i, numBytes: 0);
                //                  Debug.WriteLine(dictEntry.str);
                this.dictionary.Add(dictEntry.str);                                           // Find 'n' different dictionary entries and add words to list
            }
            return this;
        }

        public uint compare(string word, int dictionaryFlag = 0)
        {
            if (word.Length > 6)
            {
                word = word.Remove(6);
            }
            // search dictionary for comparisons
            for (int i = 0; i < this.dictionary.Count; i++)
            {
                if (this.dictionary[i] == word)
                {
                    // io.WriteLine("Matched word: " + word + " at dictionary entry: " + memory.getByte((uint)dictionaryIndex[i]) + " // " + dictionary[i] );
                    return this.dictionaryIndex[i];
                }
            }
            // io.WriteLine("Could not identify keyword: " + word);    // Game will have its own readout
            return 0;
        }

        public string[] parseString(string input)
        {
            int wordindex = 1;

            string[] wordArray = input.Split(' ');        // Tokenize into words
            this.wordStartIndex = new int[wordArray.Length];

            // Record start index of each word in input string
            for (int i = 0; i < wordArray.Length; i++)
            {
                this.wordStartIndex[i] = wordindex;                                // take index of word

                for (int j = 0; j < wordArray[i].Length; j++)
                {
                    wordindex++;                                              // Add 1 for each char
                }
                wordindex++;                                                  // Add 1 for each space
            }

            return wordArray;
        }



        public int convertZSCIIToZchar(char letter) // (unimplemented)
        {
            string[] zalphabets = { "abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", " \n0123456789.,!?_#'\"/\\-:()" };
            // Convert into Zchar from ZSCII char
            // Note: ASCII 'a' = int 97
            if (letter == ' ')
            {
                return 0;
            }
            else if (zalphabets[0].IndexOf(letter) != -1)       // Take in ZSCII letter and return 5-bit Zchar
            {
                // io.WriteLine("Recognized character: " + (int)letter);
                return zalphabets[0].IndexOf(letter) + 6;
            }
            else if (zalphabets[2].IndexOf(letter) != -1)
            {
                return zalphabets[2].IndexOf(letter) + 6;           // not working properly
            }
            else
            {
                // io.WriteLine("Invalid character: " + letter);
                return 0;
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
        //                memory.setWord((uint)(textBufferAddress + (2 + 4 * i)), (ushort)zstringArray[i]);    // Write number of words in byte 1, write words from byte 2 onward (stopping at parseBufferLength;
        //            }
        //                                                            //

        ////          tokenize(input)                                 // Tokenize input using the main dictionary
        //            setVar(firstoperand, zstringArray[i]);                                    // Store string in buffer in first operand 



        public char readChar()
        {
            this.memory.getZChar(Convert.ToChar(this.io.ReadKey()));// read keypress and pass as a char into getZchar
            return '0';
        }

    }
}
