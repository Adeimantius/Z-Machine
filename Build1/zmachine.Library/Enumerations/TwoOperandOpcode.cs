namespace zmachine.Library.Enumerations;

public enum TwoOperandOpcode
{
    Unknown = 0x00, // 00/00
    op_je = 0x01, // 01/01 a b ?(label)
    op_jl = 0x02, // 02/02 a b ?(label)	
    op_jg = 0x03, // 03/03
    op_dec_chk = 0x04, // 04/04
    op_inc_chk = 0x05, // 05/05
    op_jin = 0x06, // 06/06
    op_test = 0x07, // 07/07
    op_or = 0x08, // 08/08
    op_and = 0x09, // 09/09
    op_test_attr = 0x0A, // 10/0A
    op_set_attr = 0x0B, // 11/0B
    op_clear_attr = 0x0C, // 12/0C
    op_store = 0x0D, // 13/0D
    op_insert_obj = 0x0E, // 14/0E
    op_loadw = 0x0F, // 15/0F
    op_loadb = 0x10, // 16/10
    op_get_prop = 0x11, // 17/11
    op_get_prop_addr = 0x12, // 18/12
    op_get_next_addr = 0x13, // 19/13
    op_add = 0x14, // 20/14
    op_sub = 0x15, // 21/15
    op_mul = 0x16, // 22/15
    op_div = 0x17, // 23/16
    op_mod = 0x18, // 24/17
    Unknown25 = 0x19, // 25/19 
    Unknown26 = 0x1A, // 26/1A
    Unknown27 = 0x1B, // 27/1B 
    Unknown28 = 0x1C, // 28/1C 
    Unknown29 = 0x1D, // 29/1D 
    Unknown30 = 0x1E, // 30/1E
    Unknown31 = 0x1F // 31/1F 
}