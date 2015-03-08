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
            /* Each object has its own property table. Each of these can be anywhere in dynamic memory 
 * (indeed, a game can legally change an object's properties table address in play, provided the new address points to another valid properties table). 
 * The header of a property table is as follows:
 *
 *   text-length     text of short name of object
 *  -----byte----   --some even number of bytes---
 * where the text-length is the number of 2-byte words making up the text, which is stored in the usual format. 
 * (This means that an object's short name is limited to 765 Z-characters.) After the header, the properties are listed in descending numerical order. 
 * (This order is essential and is not a matter of convention.)

In Versions 1 to 3, each property is stored as a block:
 * size byte     the actual property data
 *              ---between 1 and 8 bytes--
 *              where the size byte is arranged as 32 times the number of data bytes minus one, plus the property number.
 *              A property list is terminated by a size byte of 0. (It is otherwise illegal for a size byte to be a multiple of 32.)            */
          return propertyTableAddress;
        }

        public int getObjectAddress(int objectId)
        {
            
            getObjectTable();
            tp += 9 * (objectId - 1);             // tp will already be set at the start of the object table
            int objectAddress = tp;
            // each object is 9 bytes above the previous
            // take an objectId and consult the objectTable

            return objectAddress;
        }

        public int getObjectPropertyAddress(int objectId, int property)
        {
            int propertyAddress; 
            tp = getPropertyTableAddress(objectId);

            for (int i = 1; i < property; i++)
            {
                int text_length = tp_getByte(); // Read the first byte of the property address, figure out the text-length, then skip forward to the next property address. Do this i times.
                tp += tp * (text_length * 2);
            }
            propertyAddress = tp;
                return propertyAddress;
        }
        public int getObjectPropertyLengthFromAddress(int propertyAddress)
        {
            tp = memory.getWord((uint)propertyAddress);
            int sizeByte = memory.getByte((uint)propertyAddress - 1);  // arranged as 32 times the number of data bytes minus one, plus the property number
            return sizeByte;

        }

        public int getObjectProperty(int objectId, int property)
        {
            int propertyAddress = getObjectPropertyAddress(objectId, property);
            int[] propertyList = new int[8];

            for (int i = 0; i < getObjectPropertyLengthFromAddress(propertyAddress); i++)
            {
                propertyList[i] = tp_getByte();
            }
            // From here, we read past the header based on the length in getObjectPropertyLengthFromAddress(int propertyAddress)
            // After the header, the properties are listed in descending numerical order. 
            // each property is stored as a block
   //size byte     the actual property data
                //---between 1 and 8 bytes--
            return propertyList[property];          // When we refer to the "property", is it a list of values? Is it the whole thing? If so, how do I return 8 bytes of data nicely?
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
            getObjectAddress(objectId);
            int byteNumber = tp_getByte();
            str = Machine.getZSCII((uint)objectId, (uint)byteNumber);
            String objectName = str.str;
            return objectName;
        }


    }
}
