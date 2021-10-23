namespace zmachine.Library
{
    public class ObjectTable
    {
        private readonly Memory memory;
        private int tp = 0;                                 // pointer to move through tables
        private readonly int objectId = 0;                           // Object ID

        private ObjectTable() { throw new NotImplementedException(); }

        public ObjectTable(ref Memory mem)
        {
            this.memory = mem;
        }

        public int getDefaultProperty(int property)     // Start reading from the Program Defaults Table (before list of objects)
        {
            this.tp = this.memory.getWord(Memory.ADDR_OBJECTS);
            this.tp += (property - 1) * 2;
            int defaultProperty = this.tp_getWord();
            return defaultProperty;
        }
        public int getObjectTable()                     // Set Table Pointer to the beginning of the object table. (After Default Properties)
        {
            this.tp = this.memory.getWord(Memory.ADDR_OBJECTS) + (31 * 2);
            return this.tp;
        }
        public int getPropertyTableAddress(int objectId)// Return property table address for given object. Leaves Table Pointer at beginning of next property.
        {
            this.getObjectTable();

            this.tp = this.getObjectAddress(objectId) + 7;
            int propertyTableAddress = this.tp_getWord();
            return propertyTableAddress;
        }
        public int getObjectAddress(int objectId)       // take an objectId and consult the objectTable
        {

            this.getObjectTable();
            this.tp += 9 * (objectId - 1);             // tp will already be set at the start of the object table
            int objectAddress = this.tp;              // each object is 9 bytes above the previous    

            return objectAddress;
        }
        public int getObjectPropertyAddress(int objectId, int property)  // Get property address for an object's property (if it exists)
        {
            int propertyAddress;
            this.tp = this.getPropertyTableAddress(objectId);
            int text_length = this.tp_getByte();  // Read the first byte of the property address, figure out the text-length, then skip forward to the first property address.
            this.tp += (text_length * 2);         // skip past header

            int sizeByte = this.tp_getByte();     // get initial sizeByte (property 1)
            while (sizeByte > 0)             // Let the search begin! Move through properties looking for a propertyId matching the property given.
            {
                int propertyId = sizeByte & 31;
                int propLen = (sizeByte / 32) + 1;
                if (property == propertyId)
                {
                    propertyAddress = this.tp;
                    return propertyAddress;
                }
                this.tp += propLen;
                sizeByte = this.tp_getByte();
            }
            return 0;
        }
        public int getNextObjectPropertyIdAfter(int objectId, int property) // Get property address down the chain given a certain object.
        {
            int propLen;
            int nextPropertyId;
            byte sizeByte;

            if (property == 0)      // Return first property
            {
                this.tp = this.getPropertyTableAddress(objectId);
                int text_length = this.tp_getByte();  // Read the first byte of the property address, figure out the text-length, then skip forward to the first property address.
                this.tp += (text_length * 2);         // skip past header
                sizeByte = this.tp_getByte();

                return sizeByte & 31;       // read property id
            }

            // Since we are not at the start of the property we want...
            this.tp = this.getObjectPropertyAddress(objectId, property);              // Set pointer to start of given property
            sizeByte = this.memory.getByte((uint)this.tp - 1);       // Find length of property
            propLen = this.getObjectPropertyLengthFromAddress(this.tp);

            if (sizeByte == 0)                    // No next property
            {
                return 0;
            }

            this.tp += propLen;                            // Move pointer to start next property   
            nextPropertyId = this.tp_getByte() & 31;       // Read next property number

            return nextPropertyId;
        }
        public int getObjectPropertyLengthFromAddress(int propertyAddress)
        {
            if (propertyAddress == 0)
            {
                return 0;
            }
            int sizeByte = this.memory.getByte((uint)propertyAddress - 1);  // arranged as 32 times the number of data bytes minus one, plus the property number
            int propLen = (sizeByte / 32) + 1;
            return propLen;
        }
        public int getObjectProperty(int objectId, int property)
        {
            // each property is stored as a block
            //size byte     the actual property data
            //---between 1 and 8 bytes--

            int propertyAddress = this.getObjectPropertyAddress(objectId, property);

            if (propertyAddress == 0)
            {
                return this.getDefaultProperty(property);
            }

            int propertyData;
            int propLen = this.getObjectPropertyLengthFromAddress(propertyAddress);  // get size of property

            if (propLen == 1)                                   // Return size of property (byte or word)
            {
                propertyData = this.tp_getByte();
            }
            else if (propLen == 2)
            {
                propertyData = this.tp_getWord();
            }
            else
            {
                propertyData = 0;
                // Debug.WriteLine("Property Length " + propLen + " is an unspecified length call for opcodes.");
            }

            return propertyData;
        }
        public int getParent(int objectId)
        {
            this.tp = this.getObjectAddress(objectId) + 4;        // move past object header (4 attribute bytes)
            return this.tp_getByte();
        }
        public int getSibling(int objectId)
        {
            this.tp = this.getObjectAddress(objectId) + 5;                           // move past object header (4 attribute bytes + parent byte)
            return this.tp_getByte();
        }
        public int getChild(int objectId)
        {
            this.tp = this.getObjectAddress(objectId) + 6;                           // move past object header (4 attribute bytes + parent byte + sibling byte)
            return this.tp_getByte();
        }
        public bool getObjectAttribute(int objectId, int attributeId)
        {
            this.tp = this.getObjectAddress(objectId);
            bool[] attributes = new bool[32];
            byte attributeByte;

            // Why not just make a boolean array of the attributes?!
            for (int i = 0; i < 4; i++)
            {
                attributeByte = this.tp_getByte();
                for (int j = 0; j < 8; j++)
                {
                    attributes[i * 8 + j] = ((attributeByte >> (7 - j)) & 0x01) > 0;// Take the next byte and cut the first (MSB) value off the top. Add to boolean array attribute[] and voila!
                }
            }

            return attributes[attributeId];
        }
        public ObjectTable setObjectAttribute(int objectId, int attributeId, bool value)
        {
            byte a;                     // If can find the right byte in the memory, I can bitwise AND or NOT to fill or clear it.
            uint address = (uint)this.getObjectAddress(objectId);
            //            Debug.WriteLine("BEFORE address: " + address + " " + memory.getByte(address) + "," + memory.getByte(address + 1) + "," + memory.getByte(address + 2) + "," + memory.getByte(address + 3));
            byte attributeByte = (byte)(attributeId / 8);           // The byte of the attribute header that we are working in
            int attributeShift = 7 - (attributeId % 8);

            if (value == true)
            {
                a = this.memory.getByte(address + attributeByte);        // Read the byte given by the attribute segment our attribute Id is in.
                a |= (byte)(1 << attributeShift);                      // Fill a bit at the given shift in that byte.
            }
            else
            {
                a = this.memory.getByte(address + attributeByte);
                a &= (byte)~(1 << attributeShift);
            }
            //            Debug.WriteLine("AFTER address: " + address + " " + memory.getByte(address) + "," + memory.getByte(address + 1) + "," + memory.getByte(address + 2) + "," + memory.getByte(address + 3));
            this.memory.setByte(address + attributeByte, a);
            return this;
        }

        public ObjectTable setObjectProperty(int objectId, int property, ushort value)
        {
            int propertyAddress = this.getObjectPropertyAddress(objectId, property);

            if (propertyAddress == 0)
            {
                property = this.getDefaultProperty(property);
            }
            int propLen = this.getObjectPropertyLengthFromAddress(propertyAddress);  // get size of property

            if (propLen == 1)                                   // Check size of property
            {
                this.memory.setWord((uint)propertyAddress, (ushort)(value & 0xff));
            }
            else if (propLen == 2)
            {
                this.memory.setWord((uint)propertyAddress, value);
            }
            return this;
        }
        public ObjectTable setParent(int objectId, int parentId)
        {
            this.tp = this.getObjectAddress(objectId) + 4;                           // move past object header (4 attribute bytes)
            this.memory.setByte((uint)this.tp, (byte)parentId);
            return this;
        }
        public ObjectTable setSibling(int objectId, int siblingId)
        {
            this.tp = this.getObjectAddress(objectId) + 5;                           // move past object header (4 attribute bytes)
            this.memory.setByte((uint)this.tp, (byte)siblingId);
            return this;
        }
        public ObjectTable setChild(int objectId, int childId)
        {
            this.tp = this.getObjectAddress(objectId) + 6;                           // move past object header (4 attribute bytes + parent byte + sibling byte)
            this.memory.setByte((uint)this.tp, (byte)childId);
            return this;
        }

        // -------------------------- 
        public byte tp_getByte()
        {
            byte next = this.memory.getByte((uint)this.tp);
            this.tp++;
            return next;
        }

        public ushort tp_getWord()
        {
            ushort next = this.memory.getWord((uint)this.tp);
            this.tp += 2;
            return next;
        }


        public string objectName(int objectId)
        {
            string name;
            if (objectId == 0)
            {
                return "Unable to find Object";
            }
            Memory.StringAndReadLength str = new Memory.StringAndReadLength();
            this.tp = this.getPropertyTableAddress(objectId);            // An object's name is stored in the header of its property table, and is given in the text-length.
            int textLength = this.tp_getByte();                     // The first byte is the text-length number of words in the short name
            if (textLength == 0)
            {
                return "Name not found";
            }
            name = this.memory.getZSCII((uint)this.tp, (uint)textLength * 2).str;
            return name;
        }
        public ObjectTable unlinkObject(int objectId)
        {
            // Get parent of object. If no parent, no need to unlink it.
            int parentId = this.getParent(objectId);
            if (parentId == 0)
            {
                return this;
            }

            // Get next sibling
            int nextSibling = this.getSibling(objectId);

            // Get very first sibling in list
            int firstSibling = this.getChild(parentId);

            // Clear out the parent/sibling fields since they'll no longer be valid when this is done.
            this.setParent(objectId, 0);
            this.setSibling(objectId, 0);

            // Remove object from the list of siblings
            if (firstSibling == objectId)   // If this object is the first child, just set it to the next child.
            {
                this.setChild(parentId, nextSibling);
            }
            else
            {
                int sibId = firstSibling;
                int lastSibId = sibId;
                do
                {
                    lastSibId = sibId;
                    sibId = this.getSibling(sibId);
                } while (sibId != objectId);
                this.setSibling(lastSibId, nextSibling);
            }
            return this;
        }

    }
}
