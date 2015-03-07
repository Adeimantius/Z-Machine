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

        void ObjectTable (Memory mem)
        {
            memory = mem;
        }

        //public int getDefaultProperty(int property) 
        //{
        //    int[] defaultTable = new int[31];

        //    for (int i = 0, i < 31, i++)
        //    {
        //        defaultTable[i] = tp_getWord();
        //    }
            
        //    return defaultTable[property];
        //}
        
        public int getObjectTable()
        {
            tp = memory.getWord((uint)Memory.ADDR_OBJECTS + (31 * 2));
            // Set table pointer at end of default properties
            // Every 9 bytes from here, the table pointer will encounter the start of a new objectId
            // Once we get an objectId, we can pass it into getObjectAddress
            // With the objectAddress, we can access the object table anywhere by simply setting the pointer to objectAddress.
            // So why are we passing objectId to our methods, instead of objectAddress? 
            // (unless I'm just going to call getObjectAddress(objectId) everywhere, which is fine with me)
            return tp;
        }
//        public int getPropertyTable(int propertyAddress)
//        {
//            int[] propertyTable;
//            while (size != 0)
//            {
//                // Let's build an array of objectproperties with their values.
//            }
//            /* Each object has its own property table. Each of these can be anywhere in dynamic memory 
// * (indeed, a game can legally change an object's properties table address in play, provided the new address points to another valid properties table). 
// * The header of a property table is as follows:
// *
// *   text-length     text of short name of object
// *  -----byte----   --some even number of bytes---
// * where the text-length is the number of 2-byte words making up the text, which is stored in the usual format. 
// * (This means that an object's short name is limited to 765 Z-characters.) After the header, the properties are listed in descending numerical order. 
// * (This order is essential and is not a matter of convention.)

//In Versions 1 to 3, each property is stored as a block:
// * size byte     the actual property data
// *              ---between 1 and 8 bytes--
// *              where the size byte is arranged as 32 times the number of data bytes minus one, plus the property number.
// *              A property list is terminated by a size byte of 0. (It is otherwise illegal for a size byte to be a multiple of 32.)            */
//          return propertyTable[propertyAddress];
//        }

        public int getObjectAddress(int objectId)
        {
            
            getObjectTable();
            tp += 9 * objectId;             // tp will already be set at the start of the object table
            int objectAddress = tp;
            // each object is 9 bytes above the previous
            // take an objectId and use it with the objectTable

            return objectAddress;
        }

        public int getPropertyTableAddress(int objectId)
        {
            tp = memory.getWord((uint)getObjectAddress(objectId) + 7);    // The table pointer moves to the beginning of the objectId and forward 7 bytes
            int propertyAddress = tp_getWord();    

            return  propertyAddress;
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
        //public String getName(objectId)
        //{
        //    Machine.StringAndReadLength str = new Machine.StringAndReadLength();
        //    int byteNumber = text-length from header in getPropertyTableAddress(objectId);
        //    return Machine.getZSCII(objectId, byteNumber);
        //}


    }
}
