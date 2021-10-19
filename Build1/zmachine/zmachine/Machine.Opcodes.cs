namespace zmachine
{
    public partial class Machine
    {
        private readonly OpcodeHandler_2OP[] OP2opcodes = {
        new  op_unknown_2op(),      // OPCODE/HEX
        new  op_je(),               // 01/01 a b ?(label)
        new  op_jl(),               // 02/02 a b ?(label)	
        new  op_jg(),               // 03/03
        new  op_dec_chk(),          // 04/04
        new  op_inc_chk(),          // 05/05
        new  op_jin(),              // 06/06
        new  op_test(),             // 07/07
        new  op_or(),               // 08/08
        new  op_and(),              // 09/09
        new  op_test_attr(),        // 10/0A
        new  op_set_attr(),         // 11/0B
        new  op_clear_attr(),       // 12/0C
        new  op_store(),            // 13/0D
        new  op_insert_obj(),       // 14/0E
        new  op_loadw(),            // 15/0F
        new  op_loadb(),            // 16/10
        new  op_get_prop(),         // 17/11
        new  op_get_prop_addr(),    // 18/12
        new  op_get_next_addr(),    // 19/13
        new  op_add(),              // 20/14
        new  op_sub(),              // 21/15
        new  op_mul(),              // 22/15
        new  op_div(),              // 23/16
        new  op_mod(),              // 24/17
        new  op_unknown_2op(),      // 25/19 
        new  op_unknown_2op(),      // 26/1A
        new  op_unknown_2op(),      // 27/1B 
        new  op_unknown_2op(),      // 28/1C 
        new  op_unknown_2op(),      // 29/1D 
        new  op_unknown_2op(),      // 30/1E
        new  op_unknown_2op()       // 31/1F 
     };
        private readonly OpcodeHandler_1OP[] OP1opcodes = {
                                    // OPCODE/HEX
        new  op_jz(),               // 128/01
        new  op_get_sibling(),      // 129/01 	
        new  op_get_child(),        // 130/02 
        new  op_get_parent(),       // 131/03
        new  op_get_prop_len(),     // 132/04
        new  op_inc(),              // 133/05
        new  op_dec(),              // 134/06
        new  op_print_addr(),       // 135/07
        new  op_unknown_1op(),      // 136/08
        new  op_remove_obj(),       // 137/09
        new  op_print_obj(),        // 138/0A
        new  op_ret(),              // 139/0B
        new  op_jump(),             // 140/0C
        new  op_print_paddr(),      // 141/0D
        new  op_load(),             // 142/0E
        new  op_unknown_1op(),      // 141/0F
        new  op_unknown_1op()       // 142/0F
        };
        private readonly OpcodeHandler_0OP[] OP0opcodes = {
                                    // OPCODE/HEX
        new  op_rtrue(),            // 176/00 
        new  op_rfalse(),           // 177/01 
        new  op_print (),           // 178/02 
        new  op_print_ret(),        // 179/03
        new  op_nop(),              // 180/04
        new  op_save (),            // 181/05
        new  op_restore (),         // 182/06
        new  op_restart(),          // 183/07
        new  op_ret_popped(),       // 184/08
        new  op_pop(),              // 185/09
        new  op_quit(),             // 186/0A
        new  op_new_line(),         // 187/0B
        new  op_show_status(),      // 188/0C
        new  op_verify ()           // 189/0C
        };
        private readonly OpcodeHandler_OPVAR[] VARopcodes = {
                                    // OPCODE/HEX
        new  op_call(),             // 224/00 
        new  op_storew(),           // 225/01 
        new  op_storeb(),           // 226/02
        new  op_put_prop(),         // 227/03
        new  op_sread(),            // 228/04
        new  op_print_char(),       // 229/05
        new  op_print_num(),        // 230/06
        new  op_random(),           // 231/07
        new  op_push(),             // 232/08
        new  op_pull(),             // 233/09
        new  op_split_window(),     // 234/0A
        new  op_set_window(),       // 235/0B
        new  op_unknown_op_var(),   // 236/1E
        new  op_unknown_op_var(),   // 237/1E
        new  op_unknown_op_var(),   // 238/1E
        new  op_unknown_op_var(),   // 239/1E
        new  op_unknown_op_var(),   // 240/1E
        new  op_unknown_op_var(),   // 241/1E
        new  op_unknown_op_var(),   // 242/1E
        new  op_output_stream(),    // 243/13
        new  op_input_stream()      // 244/14
        };
    }
}
