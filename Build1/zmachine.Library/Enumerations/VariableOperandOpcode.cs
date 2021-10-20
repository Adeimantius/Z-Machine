namespace zmachine.Library.Enumerations
{
    public enum VariableOperandOpcode : byte
    {
        op_call = 224,             // 224/00 
        op_storew = 225,           // 225/01 
        op_storeb = 226,           // 226/02
        op_put_prop = 227,         // 227/03
        op_sread = 228,            // 228/04
        op_print_char = 229,       // 229/05
        op_print_num = 230,        // 230/06
        op_random = 231,           // 231/07
        op_push = 232,             // 232/08
        op_pull = 233,             // 233/09
        op_split_window = 234,     // 234/0A
        op_set_window = 235,       // 235/0B
        Unknown236 = 236,          // 236/0C
        Unknown237 = 237,          // 237/0D
        Unknown238 = 238,          // 238/0E
        Unknown239 = 239,          // 239/0F
        Unknown240 = 240,          // 240/10
        Unknown241 = 241,          // 241/11
        Unknown242 = 242,          // 242/12
        Unknown243 = 243,          // 243/13
        Unknown244 = 244           // 244/14
    }
}
