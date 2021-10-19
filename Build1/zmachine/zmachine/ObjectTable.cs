namespace zmachine
{
    public class ObjectTable
    {
        private readonly Memory memory;
        private int tp = 0;                                 // pointer to move through tables
        private int objectId = 0;                           // Object ID



        public ObjectTable(Memory mem)
        {
            memory = mem;
        }

        public int getDefaultProperty(int property)     // Start reading from the Program Defaults Table (before list of objects)
        {
            tp = memory.getWord((uint)Memory.ADDR_OBJECTS);
            tp += (property - 1) * 2;
            int defaultProperty = tp_getWord();
            return defaultProperty;
        }
        public int getObjectTable()                     // Set Table Pointer to the beginning of the object table. (After Default Properties)
        {
            tp = memory.getWord((uint)Memory.ADDR_OBJECTS) + (31 * 2);
            return tp;
        }
        public int getPropertyTableAddress(int objectId)// Return property table address for given object. Leaves Table Pointer at beginning of next property.
        {
            getObjectTable();

            tp = getObjectAddress(objectId) + 7;
            int propertyTableAddress = tp_getWord();
            return propertyTableAddress;
        }
        public int getObjectAddress(int objectId)       // take an objectId and consult the objectTable
        {

            getObjectTable();
            tp += 9 * (objectId - 1);             // tp will already be set at the start of the object table
            int objectAddress = tp;              // each object is 9 bytes above the previous    

            return objectAddress;
        }
        public int getObjectPropertyAddress(int objectId, int property)  // Get property address for an object's property (if it exists)
        {
            int propertyAddress;
            tp = getPropertyTableAddress(objectId);
            int text_length = tp_getByte();  // Read the first byte of the property address, figure out the text-length, then skip forward to the first property address.
            tp += (text_length * 2);         // skip past header

            int sizeByte = tp_getByte();     // get initial sizeByte (property 1)
            while (sizeByte > 0)             // Let the search begin! Move through properties looking for a propertyId matching the property given.
            {
                int propertyId = sizeByte & 31;
                int propLen = (sizeByte / 32) + 1;
                if (property == propertyId)
                {
                    propertyAddress = tp;
                    return propertyAddress;
                }
                tp += propLen;
                sizeByte = tp_getByte();
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
                tp = getPropertyTableAddress(objectId);
                int text_length = tp_getByte();  // Read the first byte of the property address, figure out the text-length, then skip forward to the first property address.
                tp += (text_length * 2);         // skip past header
                sizeByte = tp_getByte();

                return sizeByte & 31;       // read property id
            }

            // Since we are not at the start of the property we want...
            tp = getObjectPropertyAddress(objectId, property);              // Set pointer to start of given property
            sizeByte = memory.getByte((uint)tp - 1);       // Find length of property
            propLen = getObjectPropertyLengthFromAddress(tp);

            if (sizeByte == 0)                    // No next property
                return 0;

            tp += propLen;                            // Move pointer to start next property   
            nextPropertyId = tp_getByte() & 31;       // Read next property number

            return nextPropertyId;
        }
        public int getObjectPropertyLengthFromAddress(int propertyAddress)
        {
            if (propertyAddress == 0)
            {
                return 0;
            }
            int sizeByte = memory.getByte((uint)propertyAddress - 1);  // arranged as 32 times the number of data bytes minus one, plus the property number
            int propLen = (sizeByte / 32) + 1;
            return propLen;
        }
        public int getObjectProperty(int objectId, int property)
        {
            // each property is stored as a block
            //size byte     the actual property data
            //---between 1 and 8 bytes--

            int propertyAddress = getObjectPropertyAddress(objectId, property);

            if (propertyAddress == 0)
            {
                return getDefaultProperty(property);
            }

            int propertyData;
            int propLen = getObjectPropertyLengthFromAddress(propertyAddress);  // get size of property

            if (propLen == 1)                                   // Return size of property (byte or word)
                propertyData = tp_getByte();
            else if (propLen == 2)
                propertyData = tp_getWord();
            else
            {
                propertyData = 0;
                // Debug.WriteLine("Property Length " + propLen + " is an unspecified length call for opcodes.");
            }

            return propertyData;
        }
        public int getParent(int objectId)
        {
            tp = getObjectAddress(objectId) + 4;        // move past object header (4 attribute bytes)
            return tp_getByte();
        }
        public int getSibling(int objectId)
        {
            tp = getObjectAddress(objectId) + 5;                           // move past object header (4 attribute bytes + parent byte)
            return tp_getByte();
        }
        public int getChild(int objectId)
        {
            tp = getObjectAddress(objectId) + 6;                           // move past object header (4 attribute bytes + parent byte + sibling byte)
            return tp_getByte();
        }
        public bool getObjectAttribute(int objectId, int attributeId)
        {
            tp = getObjectAddress(objectId);
            bool[] attributes = new bool[32];
            byte attributeByte;

            // Why not just make a boolean array of the attributes?!
            for (int i = 0; i < 4; i++)
            {
                attributeByte = tp_getByte();
                for (int j = 0; j < 8; j++)
                {
                    attributes[i * 8 + j] = ((attributeByte >> (7 - j)) & 0x01) > 0;// Take the next byte and cut the first (MSB) value off the top. Add to boolean array attribute[] and voila!
                }
            }

            return attributes[attributeId];
        }
        public void setObjectAttribute(int objectId, int attributeId, bool value)
        {
            byte a;                     // If can find the right byte in the memory, I can bitwise AND or NOT to fill or clear it.
            uint address = (uint)getObjectAddress(objectId);
            //            Debug.WriteLine("BEFORE address: " + address + " " + memory.getByte(address) + "," + memory.getByte(address + 1) + "," + memory.getByte(address + 2) + "," + memory.getByte(address + 3));
            byte attributeByte = (byte)(attributeId / 8);           // The byte of the attribute header that we are working in
            int attributeShift = 7 - (attributeId % 8);

            if (value == true)
            {
                a = memory.getByte(address + (uint)attributeByte);        // Read the byte given by the attribute segment our attribute Id is in.
                a |= (byte)(1 << attributeShift);                      // Fill a bit at the given shift in that byte.
            }
            else
            {
                a = memory.getByte(address + (uint)attributeByte);
                a &= (byte)~(1 << attributeShift);
            }
            //            Debug.WriteLine("AFTER address: " + address + " " + memory.getByte(address) + "," + memory.getByte(address + 1) + "," + memory.getByte(address + 2) + "," + memory.getByte(address + 3));
            memory.setByte(address + attributeByte, a);
        }

        public void setObjectProperty(int objectId, int property, ushort value)
        {
            int propertyAddress = getObjectPropertyAddress(objectId, property);

            if (propertyAddress == 0)
            {
                property = getDefaultProperty(property);
            }
            int propLen = getObjectPropertyLengthFromAddress(propertyAddress);  // get size of property

            if (propLen == 1)                                   // Check size of property
                memory.setWord((uint)propertyAddress, (ushort)(value & 0xff));
            else if (propLen == 2)
                memory.setWord((uint)propertyAddress, value);
        }
        public void setParent(int objectId, int parentId)
        {
            tp = getObjectAddress(objectId) + 4;                           // move past object header (4 attribute bytes)
            memory.setByte((uint)tp, (byte)parentId);
        }
        public void setSibling(int objectId, int siblingId)
        {
            tp = getObjectAddress(objectId) + 5;                           // move past object header (4 attribute bytes)
            memory.setByte((uint)tp, (byte)siblingId);
        }
        public void setChild(int objectId, int childId)
        {
            tp = getObjectAddress(objectId) + 6;                           // move past object header (4 attribute bytes + parent byte + sibling byte)
            memory.setByte((uint)tp, (byte)childId);
        }

        // -------------------------- 
        public byte tp_getByte()
        {
            byte next = memory.getByte((uint)tp);
            tp++;
            return (byte)next;
        }

        public ushort tp_getWord()
        {
            ushort next = memory.getWord((uint)tp);
            tp += 2;
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
            tp = getPropertyTableAddress(objectId);            // An object's name is stored in the header of its property table, and is given in the text-length.
            int textLength = tp_getByte();                     // The first byte is the text-length number of words in the short name
            if (textLength == 0)
            {
                return "Name not found";
            }
            name = memory.getZSCII((uint)tp, (uint)textLength * 2).str;
            return name;
        }
        public void unlinkObject(int objectId)
        {
            // Get parent of object. If no parent, no need to unlink it.
            int parentId = getParent(objectId);
            if (parentId == 0)
                return;

            // Get next sibling
            int nextSibling = getSibling(objectId);

            // Get very first sibling in list
            int firstSibling = getChild(parentId);

            // Clear out the parent/sibling fields since they'll no longer be valid when this is done.
            setParent(objectId, 0);
            setSibling(objectId, 0);

            // Remove object from the list of siblings
            if (firstSibling == objectId)   // If this object is the first child, just set it to the next child.
                setChild(parentId, nextSibling);
            else
            {
                int sibId = firstSibling;
                int lastSibId = sibId;
                do
                {
                    lastSibId = sibId;
                    sibId = getSibling(sibId);
                } while (sibId != objectId);
                setSibling(lastSibId, nextSibling);
            }
        }

    }
}
