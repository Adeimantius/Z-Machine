using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zmachine
{
    class ObjectTable
    {
        public Memory memory = new Memory(1024 * 128);
        public int tp = 0;                                 // pointer to move through tables
        public int objectId = 0;                           // Object ID
        public int[] defaultTable;



        public ObjectTable (Memory mem)
        {
            memory = mem;

        }

        public int getDefaultProperty(int property)     // Rewrite this so that it only reads the property I want instead of all of them. Set pointer to start and read word @ (tp - 1) * 2
        {
            tp = getObjectTable();
            tp += (property - 1) * 2;
            int defaultProperty = tp_getWord();
            return defaultProperty;
        }        
        public int getObjectTable()
        {
            tp = memory.getWord((uint)Memory.ADDR_OBJECTS + (31 * 2));
            return tp;
        }
        public int getPropertyTableAddress(int objectId)
        {
            getObjectTable();

            tp = getObjectAddress(objectId) + 7;
            int propertyTableAddress = tp_getWord();
          return propertyTableAddress;
        }
        public int getObjectAddress(int objectId) // take an objectId and consult the objectTable
        {
            
            getObjectTable();
            tp += 9 * (objectId - 1);             // tp will already be set at the start of the object table
            int objectAddress = tp;              // each object is 9 bytes above the previous    

            return objectAddress;
        }
        public int getObjectPropertyAddress(int objectId, int property)
        {
            int propertyAddress; 
            tp = getPropertyTableAddress(objectId);
            int text_length = tp_getByte(); // Read the first byte of the property address, figure out the text-length, then skip forward to the first property address.
            tp += (text_length * 2);   // skip past header

            int sizeByte = tp_getByte();        // get initial sizeByte (property 1)
            while (sizeByte > 0)
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
        public int getNextObjectPropertyIdAfter(int objectId, int property)
        {                        
            int propLen;
            int nextPropertyId;
            byte sizeByte;
            // Since we are not at the start of the property we want...
            tp = getObjectPropertyAddress(objectId, property);              // Set pointer to start of given property
            sizeByte = memory.getByte((uint)tp - 1);       // Find length of property
            propLen = getObjectPropertyLengthFromAddress(tp);
             
                if (sizeByte == 0)                    // No next property
                    return 0;

            tp += propLen;                            // Move pointer to start next property and skip header   
            nextPropertyId = tp_getByte() & 31;       // Read next property number
            
            return nextPropertyId;
        }
        public int getObjectPropertyLengthFromAddress(int propertyAddress)
        {
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
                else
                    propertyData = tp_getWord();

            return propertyData;
        }
        public int getParent(int objectId)
        {
            tp = getObjectAddress(objectId);
            tp += 4;                           // move past object header (4 attribute bytes)
            return tp_getByte();
        }
        public int getSibling(int objectId)
        {
            tp = getObjectAddress(objectId);
            tp += 4 + 1;                           // move past object header (4 attribute bytes + parent byte)
            return tp_getByte();
        }
        public int getChild(int objectId)
        {
            tp = getObjectAddress(objectId);
            tp += 4 + 2;                           // move past object header (4 attribute bytes + parent byte + sibling byte)
            return tp_getByte();
        }
        public bool getObjectAttribute(int objectId, int attributeId)
        {
            tp = getObjectAddress(objectId);
            bool [] attributes = new bool[32];
            byte attributeSegment;

            // Why not just make a boolean array of the attributes?!
            for (int i = 0; i < 4; i++)
            {
                attributeSegment = tp_getByte();
                for (int j = 0; j < 8; j++)
                {
                    attributes[i * 8 + j] = Convert.ToBoolean((attributeSegment >> (7 - j)) & 0x01);// Take the next byte and cut the first (MSB) value off the top. Add to boolean array attribute[]
                }
            }
            
            return attributes[attributeId];
        }
        public void setObjectAttribute(int objectId, int attributeId, bool value)
        {
            byte a;                     // If can find the right byte in the memory, I can bitwise AND or OR to clear or fill it.
            uint address = (uint)getObjectAddress(objectId);
            uint attributeSegment = (uint)(attributeId / 4);           // The byte of the attribute header that we are working in
            int attributeShift = attributeId % 8;

            if (value == true)
            { 
                a = memory.getByte(address + attributeSegment);        // If 
                a |= (byte)(1 << attributeShift);
            }
            else 
            {
                a = memory.getByte(address + attributeSegment);
                a &= (byte)(1 << attributeShift);
            }
            memory.setByte(address, a);

        }
        public void setObjectProperty(int objectId, int property, int value)
        {
            getObjectPropertyAddress(objectId, property);
            
        }
        public void setParent(int objectId, int parentId)
        {
            tp = getObjectAddress(objectId);
            tp += 4;                           // move past object header (4 attribute bytes)
            memory.setByte((uint)tp, (byte)parentId);
        }
        public void setSibling(int objectId, int siblingId)
        {
            tp = getObjectAddress(objectId);
            tp += 4 + 1;                           // move past object header (4 attribute bytes)
            memory.setByte((uint)tp, (byte)siblingId);
        }
        public void setChild(int objectId, int childId)
        {
            tp = getObjectAddress(objectId);
            tp += 4 + 2;                           // move past object header (4 attribute bytes + parent byte + sibling byte)
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

        public String objectName(int objectId)
        {
            Machine.StringAndReadLength str = new Machine.StringAndReadLength();
            getPropertyTableAddress(objectId);                 // An object's name is stored in the header of its property table, and is given in the text-length.
            int textLength = tp_getByte();                     // The first byte is the text-length number of words in the short name
            str = Machine.getZSCII((uint)getPropertyTableAddress(objectId), (uint)textLength);
            String objectName = str.str;
            return objectName;
        }


    }
}
