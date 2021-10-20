namespace zmachine.Library.Enumerations
{
    public enum SingleOperandOpcodes : byte
    {
        op_jz = 128,               // 128/00
        op_get_sibling = 129,      // 129/01 	
        op_get_child = 130,        // 130/02 
        op_get_parent = 131,       // 131/03
        op_get_prop_len = 132,     // 132/04
        op_inc = 133,              // 133/05
        op_dec = 134,              // 134/06
        op_print_addr = 135,       // 135/07
        op_unknown_1op = 136,      // 136/08
        op_remove_obj = 137,       // 137/09
        op_print_obj = 138,        // 138/0A
        op_ret = 139,              // 139/0B
        op_jump = 140,             // 140/0C
        op_print_paddr = 141,      // 141/0D
        op_load = 142,             // 142/0E
        Unknown143 = 143,          // 143/0F ?? -JM
        Unknown144 = 144,          // 144/10 ?? -JM
    }
}
