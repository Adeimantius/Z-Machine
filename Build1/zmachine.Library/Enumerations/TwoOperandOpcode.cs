namespace zmachine.Library.Enumerations
{
    public enum TwoOperandOpcode : byte
    {
        Unknown = 0,              // 00/00
        op_je = 1,                // 01/01 a b ?(label)
        op_jl = 2,                // 02/02 a b ?(label)	
        op_jg = 3,                // 03/03
        op_dec_chk = 4,           // 04/04
        op_inc_chk = 5,           // 05/05
        op_jin = 6,               // 06/06
        op_test = 7,              // 07/07
        op_or = 8,                // 08/08
        op_and = 9,               // 09/09
        op_test_attr = 10,        // 10/0A
        op_set_attr = 11,         // 11/0B
        op_clear_attr = 12,       // 12/0C
        op_store = 13,            // 13/0D
        op_insert_obj = 14,       // 14/0E
        op_loadw = 15,            // 15/0F
        op_loadb = 16,            // 16/10
        op_get_prop = 17,         // 17/11
        op_get_prop_addr = 18,    // 18/12
        op_get_next_addr = 19,    // 19/13
        op_add = 20,              // 20/14
        op_sub = 21,              // 21/15
        op_mul = 22,              // 22/15
        op_div = 23,              // 23/16
        op_mod = 24,              // 24/17
        Unknown25 = 25,           // 25/19 
        Unknown26 = 26,           // 26/1A
        Unknown27 = 27,           // 27/1B 
        Unknown28 = 28,           // 28/1C 
        Unknown29 = 29,           // 29/1D 
        Unknown30 = 30,           // 30/1E
        Unknown31 = 31            // 31/1F 
    }
}
