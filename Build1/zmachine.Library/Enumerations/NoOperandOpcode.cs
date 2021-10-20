namespace zmachine.Library.Enumerations
{
    public enum NoOperandOpcode : byte
    {
        op_rtrue = 176,            // 176/00 
        op_rfalse = 177,           // 177/01 
        op_print = 178,            // 178/02 
        op_print_ret = 179,        // 179/03
        op_nop = 180,              // 180/04
        op_save = 181,             // 181/05
        op_restore = 182,          // 182/06
        op_restart = 183,          // 183/07
        op_ret_popped = 184,       // 184/08
        op_pop = 185,              // 185/09
        op_quit = 186,             // 186/0A
        op_new_line = 187,         // 187/0B
        op_show_status = 188,      // 188/0C
        op_verify = 189,           // 189/0D
    }
}
