using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace zmachine
{
    partial class Machine
    {

        public abstract class OpcodeHandler
        {
            abstract public String name();
            public void fail_unimplemented(Machine machine)
            {
                machine.finish = true;
                Debug.WriteLine("Unimplemented function: " + name());
            }
        }
        public abstract class OpcodeHandler_0OP : OpcodeHandler
        {
            abstract public void run(Machine machine);
        }
        public abstract class OpcodeHandler_1OP : OpcodeHandler
        {
            abstract public void run(Machine machine,ushort v1);
        }
        public abstract class OpcodeHandler_2OP : OpcodeHandler
        {
            // Implement one or the other of these:
            public virtual void run(Machine machine, ushort v1, ushort v2) { fail_unimplemented(machine); }
            public virtual void run(Machine machine, List<ushort> operands) { run(machine, operands[0], operands[1]); }
        }
        public abstract class OpcodeHandler_OPVAR : OpcodeHandler
        {
            abstract public void run(Machine machine, List<ushort> operands);
        }

        // Unknown OP Classes
        public class op_unknown_2op : OpcodeHandler_2OP
        {
            public override String name() { return "UNKNOWN 2OP"; }
            public override void run(Machine machine, List<ushort> operands) { fail_unimplemented(machine); }
        }
        public class op_unknown_1op : OpcodeHandler_1OP
        {
            public override String name() { return "UNKNOWN 1OP"; }
            public override void run(Machine machine,ushort v1) { fail_unimplemented(machine); }
        }
        public class op_unknown_0op : OpcodeHandler_0OP
        {
            public override String name() { return "UNKNOWN 0OP"; }
            public override void run(Machine machine) { fail_unimplemented(machine); }
        }
        public class op_unknown_op_var : OpcodeHandler_OPVAR
        {
            public override String name() { return "UNKNOWN OPVAR"; }
            public override void run(Machine machine,List<ushort> operands) { fail_unimplemented(machine); }
        }

        // 2OP Classes
        // Branch Opcodes 1 - 7, 10 
        // Store Opcodes 8, 9, 15 - 25

        public class op_je : OpcodeHandler_2OP
        {
            public override String name() { return "op_je"; }
            public override void run(Machine machine, List<ushort> operands) 
            {
                bool branchOn = false;
                for (int i = 1; i < operands.Count; i++)
                {
                    if (operands[0] == operands[i])
                        branchOn = true;
                }
                    machine.branch(branchOn); 
            }
        }
        public class op_jl : OpcodeHandler_2OP
        {
            public override String name() { return "op_jl"; }

            public override void run(Machine machine, ushort v1, ushort v2) { machine.branch((short)v1 < (short)v2); }
        }
        public class op_jg : OpcodeHandler_2OP
        {
            public override String name() { return "op_jg"; }
            public override void run(Machine machine, ushort v1, ushort v2) { machine.branch((short)v1 > (short)v2); }
        }
        public class op_dec_chk : OpcodeHandler_2OP
        {
            public override String name() { return "op_dec_chk"; }
            public override void run(Machine machine, ushort v1, ushort v2)
            {
                int value = ((short)machine.getVar(v1)) - 1;
                machine.setVar(v1, (ushort)value);
                machine.branch(value < v2);
            }
        }
        public class op_inc_chk : OpcodeHandler_2OP
        {
            public override String name() { return "op_inc_chk"; }
            public override void run(Machine machine, ushort v1, ushort v2)
            {
                int value = ((short)machine.getVar(v1)) + 1;
                machine.setVar(v1, (ushort)value);
                machine.branch(value > v2);
            }
        }
        public class op_jin : OpcodeHandler_2OP
        {
            public override String name() { return "op_jin"; }
            public override void run(Machine machine,ushort v1, ushort v2) { machine.branch(machine.objectTable.getParent(v1) == v2); }
        }
        public class op_test : OpcodeHandler_2OP
        {
            public override String name() { return "op_test"; }
            public override void run(Machine machine, ushort v1, ushort v2) { machine.branch((v1 & v2) == v2); }
        }
        public class op_or : OpcodeHandler_2OP
        {
            public override String name() { return "op_or"; }
            public override void run(Machine machine, ushort v1, ushort v2) { machine.setVar(machine.pc_getByte(), (ushort)(v1 | v2)); }
        }
        public class op_and : OpcodeHandler_2OP
        {
            public override String name() { return "op_and"; }
            public override void run(Machine machine, ushort v1, ushort v2) { machine.setVar(machine.pc_getByte(), (ushort)(v1 & v2)); }
        }
        public class op_test_attr : OpcodeHandler_2OP
        {
            public override String name() { return "op_test_attr"; }
            public override void run(Machine machine, ushort v1, ushort v2) 
            { 
//                Debug.WriteLine("Looking for attribute in obj " + v1 + " attribute:" + machine.objectTable.getObjectAttribute(v1, v2));
                machine.branch(machine.objectTable.getObjectAttribute(v1, v2) == true); 
            }
        }
        public class op_set_attr : OpcodeHandler_2OP
        {
            public override String name() { return "op_set_attr"; }
            public override void run(Machine machine, ushort v1, ushort v2) { machine.objectTable.setObjectAttribute(v1, v2, true); }
        }
        public class op_clear_attr : OpcodeHandler_2OP
        {
            public override String name() { return "op_clear_attr"; }
            public override void run(Machine machine, ushort v1, ushort v2) { machine.objectTable.setObjectAttribute(v1, v2, false); }
        }
        public class op_store : OpcodeHandler_2OP
        {
            public override String name() { return "op_store"; }
            public override void run(Machine machine,ushort v1, ushort v2) { machine.setVar(v1, v2); }
        }
        public class op_insert_obj : OpcodeHandler_2OP
        {
            public override String name() { return "op_insert_obj"; }
            public override void run(Machine machine,ushort v1, ushort v2) 
            {
                int newSibling = machine.objectTable.getChild(v2); // 
                machine.objectTable.unlinkObject(v1);
                machine.objectTable.setChild(v2, v1);
                machine.objectTable.setParent(v1, v2);
                machine.objectTable.setSibling(v1, newSibling);
                    // after the operation the child of v2 is v1
                    // and the sibling of v1 is whatever was previously the child of v2.
                    // All children of v1 move with it. (Initially O can be at any point in the object tree; it may legally have parent zero.)    
            }
        }
        public class op_loadw : OpcodeHandler_2OP
        {
            public override String name() { return "op_loadw"; }
            public override void run(Machine machine, ushort v1, ushort v2) 
            { 
                ushort value = machine.memory.getWord((uint)v1 + (uint)(2 * v2));
                machine.setVar((ushort)machine.pc_getByte(), value);
            }
        }
        public class op_loadb : OpcodeHandler_2OP
        {
            public override String name() { return "op_loadb"; }
            public override void run(Machine machine, ushort v1, ushort v2)
            {
                {
                    ushort value = machine.memory.getByte((uint)v1 + (uint)v2);
                    machine.setVar((ushort)machine.pc_getByte(), value);
                }
            }
        }
        public class op_get_prop : OpcodeHandler_2OP
        {
            public override String name() { return "op_get_prop"; }
            public override void run(Machine machine, ushort v1, ushort v2) { machine.setVar(machine.pc_getByte(), (ushort)machine.objectTable.getObjectProperty(v1, v2)); }
        }
        public class op_get_prop_addr : OpcodeHandler_2OP
        {
            public override String name() { return "op_get_prop_addr"; }
            public override void run(Machine machine, ushort v1, ushort v2) { machine.setVar(machine.pc_getByte(), (ushort)machine.objectTable.getObjectPropertyAddress(v1, v2)); }
        }
        public class op_get_next_addr : OpcodeHandler_2OP
        {
            public override String name() { return "op_get_next_addr"; }
            public override void run(Machine machine, ushort v1, ushort v2) { machine.setVar(machine.pc_getByte(), (ushort)machine.objectTable.getNextObjectPropertyIdAfter(v1, v2)); }
        }
        public class op_add : OpcodeHandler_2OP
        {
            public override String name() { return "op_add"; }
            public override void run(Machine machine, ushort v1, ushort v2) { machine.setVar(machine.pc_getByte(), (ushort)((short)v1 + (short)v2)); }
        }
        public class op_sub : OpcodeHandler_2OP
        {
            public override String name() { return "op_sub"; }
            public override void run(Machine machine, ushort v1, ushort v2) 
            {
                int result = (short)v1 - (short)v2;
                machine.setVar(machine.pc_getByte(), (ushort)result);
            } 
        }
        public class op_div : OpcodeHandler_2OP
        {
            public override String name() { return "op_div"; }
            public override void run(Machine machine, ushort v1, ushort v2) 
            {
                int result = (short)v1 / (short)v2;
                machine.setVar(machine.pc_getByte(), (ushort)result); 
            } 
        }
        public class op_mod : OpcodeHandler_2OP
        {
            public override String name() { return "op_mod"; }
            public override void run(Machine machine, ushort v1, ushort v2)
            {
                if (v2 == 0) machine.finish = true;         // Interpreter cannot div by 0

                int result = (short)v1 % (short)v2;
                machine.setVar(machine.pc_getByte(), (ushort)result);
            } 
        }
        public class op_mul : OpcodeHandler_2OP
        {
            public override String name() { return "op_mul"; }
            public override void run(Machine machine, ushort v1, ushort v2)
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
            public override String name() { return "op_jz"; }
            public override void run(Machine machine, ushort v1) { machine.branch(v1 == 0); }
        }
        public class op_get_sibling : OpcodeHandler_1OP
        {
            public override String name() { return "op_get_sibling"; }
            public override void run(Machine machine, ushort v1) 
            { 
                machine.setVar(machine.pc_getByte(), (ushort)machine.objectTable.getSibling(v1));
                machine.branch(machine.objectTable.getSibling(v1) != 0);
            }
        }
        public class op_get_child : OpcodeHandler_1OP
        {
            public override String name() { return "op_get_child"; }
            public override void run(Machine machine, ushort v1) 
            {
                machine.setVar(machine.pc_getByte(), (ushort)machine.objectTable.getChild(v1));
                machine.branch(machine.objectTable.getChild(v1) != 0);
            }
        }
        public class op_get_parent : OpcodeHandler_1OP
        {
            public override String name() { return "op_get_parent"; }
            public override void run(Machine machine, ushort v1) { machine.setVar(machine.pc_getByte(), (ushort)machine.objectTable.getParent(v1)); }
        }
        public class op_get_prop_len : OpcodeHandler_1OP
        {
            public override String name() { return "op_get_prop_len"; }
            public override void run(Machine machine, ushort v1) { machine.setVar(machine.pc_getByte(), (ushort)machine.objectTable.getObjectPropertyLengthFromAddress(v1)); }
        }
        public class op_inc : OpcodeHandler_1OP
        {
            public override String name() { return "op_inc"; }
            public override void run(Machine machine,ushort v1) 
            {
                int value = ((short) machine.getVar(v1)) + 1;
                machine.setVar(v1, (ushort) value );
            }
        }
        public class op_dec : OpcodeHandler_1OP
        {
            public override String name() { return "op_dec"; }
            public override void run(Machine machine, ushort v1) 
            {
                int value = ((short)machine.getVar(v1)) - 1;
                machine.setVar(v1, (ushort)value);
            }
        }
        public class op_not : OpcodeHandler_1OP
        {
            public override String name() { return "op_not"; }
            public override void run(Machine machine, ushort v1) { machine.setVar(machine.pc_getByte(), (ushort)(~v1)); }
        }
        public class op_print_addr : OpcodeHandler_1OP
        {
            public override String name() { return "op_print_addr"; }
            public override void run(Machine machine,ushort v1) { Console.WriteLine(machine.memory.getZSCII(v1, 0).str); }
        }
        public class op_remove_obj : OpcodeHandler_1OP
        {
            public override String name() { return "op_remove_obj"; }
            public override void run(Machine machine,ushort v1) { machine.objectTable.setParent(v1, 0); }
        }
        public class op_print_obj : OpcodeHandler_1OP
        {
            public override String name() { return "op_print_obj"; }
            public override void run(Machine machine,ushort v1) { Console.Write(machine.objectTable.objectName(v1)); }
        }
        public class op_ret : OpcodeHandler_1OP
        {
            public override String name() { return "op_ret"; }
            public override void run(Machine machine,ushort v1) {machine.popRoutineData(v1);}
        }
        public class op_jump : OpcodeHandler_1OP
        {
            public override String name() { return "op_jump"; }
            public override void run(Machine machine, ushort v1) 
            {
                int offset = (short)v1;
                machine.pc += (uint) (offset - 2); 
            }
        }
        public class op_print_paddr : OpcodeHandler_1OP
        {
            public override String name() { return "op_print_paddr"; }
            public override void run(Machine machine, ushort v1) { Console.Write(machine.memory.getZSCII((uint)v1 * 2, 0).str); }
        }
        public class op_load : OpcodeHandler_1OP
        {
            public override String name() { return "op_load"; }
            public override void run(Machine machine,ushort v1) { machine.setVar(machine.pc_getWord(), v1); }
        }
        // 0OP Classes
        // Branch Opcodes 181, 182 , 189, 191
        public class op_rtrue : OpcodeHandler_0OP
        {
            public override String name() { return "op_rtrue"; }
            public override void run(Machine machine) { machine.popRoutineData(1); }
        }
        public class op_rfalse : OpcodeHandler_0OP
        {
            public override String name() { return "op_rfalse"; }
            public override void run(Machine machine) { machine.popRoutineData(0); }
        }
        public class op_print : OpcodeHandler_0OP
        {
            public override String name() { return "op_print"; }
            public override void run(Machine machine) 
            {
//                Debug.WriteLine("Getting string at " + machine.pc);
                Memory.StringAndReadLength str = machine.memory.getZSCII(machine.pc, 0);
                Console.Write(str.str); 
                machine.pc += (uint)str.bytesRead;
//                Debug.WriteLine("New pc location: " + machine.pc);

            }
        }
        public class op_print_ret : OpcodeHandler_0OP
        {
            public override String name() { return "op_print_ret"; }
            public override void run(Machine machine) 
            { 
                Memory.StringAndReadLength str = machine.memory.getZSCII(machine.pc, 0);
                Console.Write(str.str);
                machine.pc += (uint)str.bytesRead;
                machine.popRoutineData(1);
            }
        }
        public class op_nop : OpcodeHandler_0OP
        {
            public override String name() { return "op_nop"; }
            public override void run(Machine machine) { return; }
        }
        public class op_save : OpcodeHandler_0OP
        {
            public override String name() { return "op_save"; }
            public override void run(Machine machine) { fail_unimplemented(machine); }
        }
        public class op_restore : OpcodeHandler_0OP
        {
            public override String name() { return "op_restore"; }
            public override void run(Machine machine) { fail_unimplemented(machine); }
        }
        public class op_restart : OpcodeHandler_0OP
        {
            public override String name() { return "op_restart"; }
            public override void run(Machine machine) { fail_unimplemented(machine); }
        }
        public class op_ret_popped : OpcodeHandler_0OP
        {
            public override String name() { return "op_ret_popped"; }
            public override void run(Machine machine) {machine.popRoutineData(machine.getVar(0));}
        }
        public class op_pop : OpcodeHandler_0OP
        {
            public override String name() { return "op_pop"; }
            public override void run(Machine machine) 
            {
                machine.getVar(0);
            }
        }
        public class op_quit : OpcodeHandler_0OP
        {
            public override String name() { return "op_quit"; }
            public override void run(Machine machine) { machine.finish = true; }
        }
        public class op_new_line : OpcodeHandler_0OP
        {
            public override String name() { return "op_new_line"; }
            public override void run(Machine machine) { Console.Write("\n"); }
        }
        public class op_show_status : OpcodeHandler_0OP
        {
            public override String name() { return "op_show_status"; }
            public override void run(Machine machine) { fail_unimplemented(machine); }
        }
        public class op_verify : OpcodeHandler_0OP
        {
            public override String name() { return "op_verify"; }
            public override void run(Machine machine) { fail_unimplemented(machine); }
        }
        // VAR OP Classes
        public class op_call : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_call"; }
            public override void run(Machine machine,List<ushort> operands)
            {
                if (operands[0] == 0)
                    machine.setVar(machine.pc_getByte(), 0); //set return value to zero
                else 
                    machine.pushRoutineData(operands);
            }
        }
        public class op_storew : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_storew"; }
            public override void run(Machine machine, List<ushort> operands) 
            {
                machine.memory.setWord((uint)(operands[0] + 2 * operands[1]), operands[2]);
            }
        }
        public class op_storeb : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_storeb"; }
            public override void run(Machine machine,List<ushort> operands) 
            {
                machine.memory.setByte((uint)(operands[0] + operands[1]), (byte)operands[2]);
            }
        }
        public class op_put_prop : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_put_prop"; }
            public override void run(Machine machine, List<ushort> operands) { machine.objectTable.setObjectProperty(operands[0], operands[1], operands[2]); }
        }
        public class op_sread : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_sread"; }
            public override void run(Machine machine,List<ushort> operands) 
            { 

                machine.lex.read(operands[0], operands[1]);

            }
        }
        public class op_print_char : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_print_char"; }
            public override void run(Machine machine, List<ushort> operands) { Console.Write(machine.memory.getZChar(operands[0])); }
        }
        public class op_print_num : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_print_num"; }
            public override void run(Machine machine,List<ushort> operands) 
            {
                Console.Write((short)operands[0]); 
            }
        }
        public class op_random : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_random"; }
            public override void run(Machine machine,List<ushort> operands) 
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
                machine.setVar(machine.pc_getByte(), (ushort) value);  // If range is negative, the random number generator is seeded to that value and the return value is 0
            }
        }
        public class op_push : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_push"; }
            public override void run(Machine machine,List<ushort> operands)             { machine.setVar(0, operands[0]); }
        }
        public class op_pull : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_pull"; }
            public override void run(Machine machine,List<ushort> operands)             { machine.setVar(operands[0], machine.getVar(0)); }
        }
        public class op_split_window : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_split_window"; }
            public override void run(Machine machine,List<ushort> operands) { fail_unimplemented(machine); }
        }
        public class op_set_window : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_set_window"; }
            public override void run(Machine machine,List<ushort> operands) { fail_unimplemented(machine); }
        }
        public class op_output_stream : OpcodeHandler_OPVAR
        {
            public override String name() { return "op_output_stream"; }
            public override void run(Machine machine,List<ushort> operands) 
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
            public override String name() { return "op_input_stream"; }
            public override void run(Machine machine,List<ushort> operands) { fail_unimplemented(machine); }
        }
    }
}
