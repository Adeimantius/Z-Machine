using zmachine.Library.Enumerations;

namespace zmachine.Library;

public class Lex
{
    private readonly List<string> dictionary;
    private readonly uint dictionaryAddress;
    private readonly List<uint> dictionaryIndex;
    private readonly Machine Machine;
    private readonly List<ushort> separators;

    private int[] wordStartIndex;

    public Lex(Machine machine, uint mp = 0)
    {
        this.separators = new List<ushort>();
        this.dictionary = new List<string>();
        this.dictionaryIndex = new List<uint>();
        this.wordStartIndex = new int[] { };
        this.Machine = machine;
        this.MemoryPointer = mp;
        this.dictionaryAddress = this.Machine.Memory.getWord(Memory.ADDR_DICT);
    }

    public Memory Memory => this.Machine.Memory;

    /// <summary>
    ///     memory pointer
    /// </summary>
    public uint MemoryPointer { get; set; }

    public BreakpointType read(int textBufferAddress, uint parseBufferAddress)
    {
        int maxInputLength =
            this.Memory.getByte((uint)textBufferAddress) -
            1; // byte 0 of the text-buffer should initially contain the maximum number of letters which can be typed, minus 1
        int parseBufferLength = this.Memory.getByte(parseBufferAddress);
        this.MemoryPointer = parseBufferAddress + 2;
        string? input = this.Machine.IO.ReadLine(); // Get initial input from io terminal
        if (input is null)
        {
            return this.Machine.Terminate("Input required, but none available", BreakpointType.InputRequired);
        }

        if (input.Length > maxInputLength)
        {
            input = input.Remove(maxInputLength); // Limit input to size of text-buffer
        }

        input.TrimEnd('\n'); // Remove carriage return from end of string  
        input = input.ToLower(); // Convert to lowercase

        this.writeToBuffer(input,
            textBufferAddress); // stored in bytes 1 onward, with a zero terminator (but without any other terminator, such as a carriage return code

        if (parseBufferLength > 0) // Check to see if lexical analysis is called for
        {
            this.buildDict(); // Build Dictionary into class variable
            string[]? wordArray = this.parseString(input); // Separate string by spaces and build list of word indices
            uint[]? matchedWords = new uint[parseBufferLength];

            for (int i = 0; i < wordArray.Length; i++)
            {
                matchedWords[i] =
                    this.compare(wordArray[i]); // Stores the dictionary address of matched words (or 0 if no match)
            }
            //                       Debug.WriteLine("Byte address of matched word: " + matchedWords[i]);
            // Record dictionary addresses after comparing words


            this.Memory.setByte(this.MemoryPointer - 1, (byte)wordArray.Length); // Write number of parsed words

            for (int i = 0; i < wordArray.Length; i++)
            {
                //                    if (4 * (i + 1) < parseBufferLength)
                //                    {                            
                int wordLength = wordArray[i].Length;
                this.Memory
                    .setWord(this.MemoryPointer,
                        (ushort)matchedWords[i]) // Address in dictionary of matches (either from dictionary or 0)
                    .setByte(this.MemoryPointer + 2, (byte)wordLength) // # of letters in parsed word 
                    .setByte(this.MemoryPointer + 3,
                        (byte)this.wordStartIndex[i]); // Corresponding word position in text buffer 
                //                     }
                this.MemoryPointer += 4;
                this.Memory.setByte(this.MemoryPointer, 0);
            }
        }

        return BreakpointType.None;
    }

    // Store string (in ZSCII) at address in byte 1 onward with a zero terminator. 
    public Lex writeToBuffer(string input, int address)
    {
        int i;
        for (i = 0; i < input.Length; i++)
        {
            char ch = input[i];
            // Convert these to ZSCII here...     
            this.Memory.setByte((uint)(address + i + 1), (byte)ch);
        }
        // Write next char from input into 3-char array (unimplemented)

        this.Memory.setByte((uint)(address + i + 1), 0); // Write empty byte to terminate after read is complete.
        // io.WriteLine("Converted ZSCII string: " + memory.getZSCII((uint)(address + 1), 0).str);
        return this;
    }

    public Lex buildDict()
    {
        // build the dictionary into class variable
        uint separatorLength = this.Memory.getByte(this.dictionaryAddress); // Number of separators
        uint entryLength =
            this.Memory.getByte(this.dictionaryAddress + separatorLength + 1); // Size of each entry (default 7 bytes)
        uint dictionaryLength = this.Memory.getWord(this.dictionaryAddress + separatorLength + 2); // Number of 2-byte entries
        uint entryAddress = this.dictionaryAddress + separatorLength + 4; // Start of dictionary entries

        for (uint i = entryAddress; i < this.dictionaryAddress + separatorLength; i++)
        {
            this.separators.Add(this.Memory.getByte(i + 1)); // Find 'n' different word separators and add to list
        }

        for (uint i = entryAddress; i < entryAddress + dictionaryLength * entryLength; i += entryLength)
        {
            this.dictionaryIndex.Add(i); // Record dictionary entry address
            Memory.StringAndReadLength? dictEntry = this.Memory.getZSCII(i, 0);
            //                  Debug.WriteLine(dictEntry.str);
            this.dictionary.Add(dictEntry.str); // Find 'n' different dictionary entries and add words to list
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

        string[]? wordArray = input.Split(' '); // Tokenize into words
        this.wordStartIndex = new int[wordArray.Length];

        // Record start index of each word in input string
        for (int i = 0; i < wordArray.Length; i++)
        {
            this.wordStartIndex[i] = wordindex; // take index of word

            for (int j = 0; j < wordArray[i].Length; j++)
            {
                wordindex++; // Add 1 for each char
            }

            wordindex++; // Add 1 for each space
        }

        return wordArray;
    }


    public int convertZSCIIToZchar(char letter) // (unimplemented)
    {
        string[] zalphabets =
            {"abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", " \n0123456789.,!?_#'\"/\\-:()"};
        // Convert into Zchar from ZSCII char
        // Note: ASCII 'a' = int 97
        if (letter == ' ')
        {
            return 0;
        }

        if (zalphabets[0].IndexOf(letter) != -1) // Take in ZSCII letter and return 5-bit Zchar
        {
            // io.WriteLine("Recognized character: " + (int)letter);
            return zalphabets[0].IndexOf(letter) + 6;
        }

        if (zalphabets[2].IndexOf(letter) != -1)
        {
            return zalphabets[2].IndexOf(letter) + 6; // not working properly
        }

        return 0;
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
        this.Memory.getZChar(Convert.ToChar(this.Machine.IO.ReadKey())); // read keypress and pass as a char into getZchar
        return '0';
    }
}