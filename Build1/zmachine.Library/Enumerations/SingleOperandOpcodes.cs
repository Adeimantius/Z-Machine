namespace zmachine.Library.Enumerations;

public enum SingleOperandOpcodes
{
    op_jz = 0x00, // 128/00
    op_get_sibling = 0x01, // 129/01 	
    op_get_child = 0x02, // 130/02 
    op_get_parent = 0x03, // 131/03
    op_get_prop_len = 0x04, // 132/04
    op_inc = 0x05, // 133/05
    op_dec = 0x06, // 134/06
    op_print_addr = 0x07, // 135/07
    op_unknown_1op = 0x08, // 136/08
    op_remove_obj = 0x09, // 137/09
    op_print_obj = 0x0A, // 138/0A
    op_ret = 0x0B, // 139/0B
    op_jump = 0x0C, // 140/0C
    op_print_paddr = 0x0D, // 141/0D
    op_load = 0x0E, // 142/0E
    Unknown143 = 0x0F, // 143/0F ?? -JM
    Unknown144 = 0x10 // 144/10 ?? -JM
}