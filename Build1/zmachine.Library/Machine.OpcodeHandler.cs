using zmachine.Library.Opcodes;

namespace zmachine
{
    public partial class Machine
    {









        public class op_clear_attr : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2) { machine.ObjectTable.setObjectAttribute(v1, v2, false); }
        }
        public class op_store : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2) { machine.setVar(v1, v2); }
        }
        public class op_insert_obj : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2)
            {
                int newSibling = machine.ObjectTable.getChild(v2); // 
                machine.ObjectTable.unlinkObject(v1);
                machine.ObjectTable.setChild(v2, v1);
                machine.ObjectTable.setParent(v1, v2);
                machine.ObjectTable.setSibling(v1, newSibling);
                // after the operation the child of v2 is v1
                // and the sibling of v1 is whatever was previously the child of v2.
                // All children of v1 move with it. (Initially O can be at any point in the object tree; it may legally have parent zero.)    
            }
        }
        public class op_loadw : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2)
            {
                ushort value = machine.memory.getWord((uint)v1 + (uint)(2 * v2));
                machine.setVar((ushort)machine.pc_getByte(), value);
            }
        }
        public class op_loadb : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2)
            {
                {
                    ushort value = machine.memory.getByte((uint)v1 + (uint)v2);
                    machine.setVar((ushort)machine.pc_getByte(), value);
                }
            }
        }
        public class op_get_prop : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2) { machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getObjectProperty(v1, v2)); }
        }
        public class op_get_prop_addr : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2) { machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getObjectPropertyAddress(v1, v2)); }
        }
        public class op_get_next_addr : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2) { machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getNextObjectPropertyIdAfter(v1, v2)); }
        }
        public class op_add : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2) { machine.setVar(machine.pc_getByte(), (ushort)((short)v1 + (short)v2)); }
        }
        public class op_sub : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2)
            {
                int result = (short)v1 - (short)v2;
                machine.setVar(machine.pc_getByte(), (ushort)result);
            }
        }
        public class op_div : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2)
            {
                int result = (short)v1 / (short)v2;
                machine.setVar(machine.pc_getByte(), (ushort)result);
            }
        }
        public class op_mod : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2)
            {
                if (v2 == 0) machine.finish = true;         // Interpreter cannot div by 0

                int result = (short)v1 % (short)v2;
                machine.setVar(machine.pc_getByte(), (ushort)result);
            }
        }
        public class op_mul : OpcodeHandler_2OP
        {
            public static void run(Machine machine, ushort v1, ushort v2)
            {
                int result = (short)v1 * (short)v2;
                machine.setVar(machine.pc_getByte(), (ushort)result);
            }
        }
        // 1OP Classes
        // Branch Opcodes 128 - 130 
        // Store Opcodes 129 - 132, 136, 142 - 143
        public class op_jz : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1) { machine.branch(v1 == 0); }
        }
        public class op_get_sibling : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1)
            {
                machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getSibling(v1));
                machine.branch(machine.ObjectTable.getSibling(v1) != 0);
            }
        }
        public class op_get_child : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1)
            {
                machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getChild(v1));
                machine.branch(machine.ObjectTable.getChild(v1) != 0);
            }
        }
        public class op_get_parent : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1) { machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getParent(v1)); }
        }
        public class op_get_prop_len : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1) { machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getObjectPropertyLengthFromAddress(v1)); }
        }
        public class op_inc : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1)
            {
                int value = ((short)machine.getVar(v1)) + 1;
                machine.setVar(v1, (ushort)value);
            }
        }
        public class op_dec : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1)
            {
                int value = ((short)machine.getVar(v1)) - 1;
                machine.setVar(v1, (ushort)value);
            }
        }
        public class op_not : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1) { machine.setVar(machine.pc_getByte(), (ushort)(~v1)); }
        }
        public class op_print_addr : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1) { Console.WriteLine(machine.memory.getZSCII(v1, 0).str); }
        }
        public class op_remove_obj : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1) { machine.ObjectTable.setParent(v1, 0); }
        }
        public class op_print_obj : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1) { Console.Write(machine.ObjectTable.objectName(v1)); }
        }
        public class op_ret : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1) { machine.popRoutineData(v1); }
        }
        public class op_jump : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1)
            {
                int offset = (short)v1;
                machine.programCounter += (uint)(offset - 2);
            }
        }
        public class op_print_paddr : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1) { Console.Write(machine.memory.getZSCII((uint)v1 * 2, 0).str); }
        }
        public class op_load : OpcodeHandler_1OP
        {
            public static void run(Machine machine, ushort v1) { machine.setVar(machine.pc_getWord(), v1); }
        }
        // 0OP Classes
        // Branch Opcodes 181, 182 , 189, 191
        public class op_rtrue : OpcodeHandler_0OP
        {
            public static void run(Machine machine) { machine.popRoutineData(1); }
        }
        public class op_rfalse : OpcodeHandler_0OP
        {
            public static void run(Machine machine) { machine.popRoutineData(0); }
        }
        public class op_print : OpcodeHandler_0OP
        {
            public static void run(Machine machine)
            {
                //                Debug.WriteLine("Getting string at " + machine.pc);
                Memory.StringAndReadLength str = machine.memory.getZSCII(machine.programCounter, 0);
                Console.Write(str.str);
                machine.programCounter += (uint)str.bytesRead;
                //                Debug.WriteLine("New pc location: " + machine.pc);

            }
        }
        public class op_print_ret : OpcodeHandler_0OP
        {
            public static void run(Machine machine)
            {
                Memory.StringAndReadLength str = machine.memory.getZSCII(machine.programCounter, 0);
                Console.Write(str.str);
                machine.programCounter += (uint)str.bytesRead;
                machine.popRoutineData(1);
            }
        }
        public class op_nop : OpcodeHandler_0OP
        {
            public static void run(Machine machine) { return; }
        }
        public class op_save : OpcodeHandler_0OP
        {
            public static void run(Machine machine) { fail_unimplemented(machine); }
        }
        public class op_restore : OpcodeHandler_0OP
        {
            public static void run(Machine machine) { fail_unimplemented(machine); }
        }
        public class op_restart : OpcodeHandler_0OP
        {
            public static void run(Machine machine) { fail_unimplemented(machine); }
        }
        public class op_ret_popped : OpcodeHandler_0OP
        {
            public static void run(Machine machine) { machine.popRoutineData(machine.getVar(0)); }
        }
        public class op_pop : OpcodeHandler_0OP
        {
            public static void run(Machine machine)
            {
                machine.getVar(0);
            }
        }
        public class op_quit : OpcodeHandler_0OP
        {
            public static void run(Machine machine) { machine.finish = true; }
        }
        public class op_new_line : OpcodeHandler_0OP
        {
            public static void run(Machine machine) { Console.Write("\n"); }
        }
        public class op_show_status : OpcodeHandler_0OP
        {
            public static void run(Machine machine) { fail_unimplemented(machine); }
        }
        public class op_verify : OpcodeHandler_0OP
        {
            public static void run(Machine machine) { fail_unimplemented(machine); }
        }
        // VAR OP Classes
        public class op_call : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands)
            {
                if (operands[0] == 0)
                    machine.setVar(machine.pc_getByte(), 0); //set return value to zero
                else
                    machine.pushRoutineData(operands);
            }
        }
        public class op_storew : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands)
            {
                machine.memory.setWord((uint)(operands[0] + 2 * operands[1]), operands[2]);
            }
        }
        public class op_storeb : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands)
            {
                machine.memory.setByte((uint)(operands[0] + operands[1]), (byte)operands[2]);
            }
        }
        public class op_put_prop : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands) { machine.ObjectTable.setObjectProperty(operands[0], operands[1], operands[2]); }
        }
        public class op_sread : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands)
            {

                machine.lex.read(operands[0], operands[1]);

            }
        }
        public class op_print_char : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands) { Console.Write(machine.memory.getZChar(operands[0])); }
        }
        public class op_print_num : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands)
            {
                Console.Write((short)operands[0]);
            }
        }
        public class op_random : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands)
            {
                int value = 0;
                if (operands[0] > 0)
                {
                    Random random = new Random();
                    value = (ushort)random.Next(1, operands[0]);
                }
                else
                {
                    Random random = new Random(operands[0]);
                    value = (ushort)random.Next(1, operands[0]);
                }
                machine.setVar(machine.pc_getByte(), (ushort)value);  // If range is negative, the random number generator is seeded to that value and the return value is 0
            }
        }
        public class op_push : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands) { machine.setVar(0, operands[0]); }
        }
        public class op_pull : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands) { machine.setVar(operands[0], machine.getVar(0)); }
        }
        public class op_split_window : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands) { fail_unimplemented(machine); }
        }
        public class op_set_window : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands) { fail_unimplemented(machine); }
        }
        public class op_output_stream : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands)
            {
                if (operands[0] != 0)
                {
                    if (operands[0] < 0)
                    {
                        // deselect output stream
                    }
                    else if (operands[0] == 3)
                    {
                        Memory.StringAndReadLength str = machine.memory.getZSCII((uint)operands[1] + 2, machine.memory.getWord((uint)operands[1]));
                    }
                    else
                    {
                        // select output stream
                    }
                }
            }
        }
        public class op_input_stream : OpcodeHandler_OPVAR
        {
            public static void run(Machine machine, List<ushort> operands) { fail_unimplemented(machine); }
        }
    }
}
