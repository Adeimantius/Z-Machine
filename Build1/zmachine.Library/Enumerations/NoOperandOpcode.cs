namespace zmachine.Library.Enumerations
{
    public enum NoOperandOpcode
    {
        op_rtrue = 0x00,            // 176/00 
        op_rfalse = 0x01,           // 177/01 
        op_print = 0x02,            // 178/02 
        op_print_ret = 0x03,        // 179/03
        op_nop = 0x04,              // 180/04
        op_save = 0x05,             // 181/05
        op_restore = 0x06,          // 182/06
        op_restart = 0x07,          // 183/07
        op_ret_popped = 0x08,       // 184/08
        op_pop = 0x09,              // 185/09
        op_quit = 0x0A,             // 186/0A
        op_new_line = 0x0B,         // 187/0B
        op_show_status = 0x0C,      // 188/0C
        op_verify = 0x0D,           // 189/0D
    }
}
