namespace zmachine.Library.Enumerations;

public enum VariableOperandOpcode
{
    op_call = 0x00, // 224/00 
    op_storew = 0x01, // 225/01 
    op_storeb = 0x02, // 226/02
    op_put_prop = 0x03, // 227/03
    op_sread = 0x04, // 228/04
    op_print_char = 0x05, // 229/05
    op_print_num = 0x06, // 230/06
    op_random = 0x07, // 231/07
    op_push = 0x08, // 232/08
    op_pull = 0x09, // 233/09
    op_split_window = 0x0A, // 234/0A
    op_set_window = 0x0B, // 235/0B
    Unknown236 = 0x0C, // 236/0C
    Unknown237 = 0x0D, // 237/0D
    Unknown238 = 0x0E, // 238/0E
    Unknown239 = 0x0F, // 239/0F
    Unknown240 = 0x10, // 240/10
    Unknown241 = 0x11, // 241/11
    Unknown242 = 0x12, // 242/12
    Unknown243 = 0x13, // 243/13
    Unknown244 = 0x13 // 244/14
}