namespace zmachine.Library.Extensions
{
    using zmachine.Library.Enumerations;

    public static partial class MachineOpcodeExtensions
    {
        public static void op_and(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(machine.pc_getByte(), (ushort)(v1 & v2));
        }

        public static void op_dec_chk(this Machine machine, ushort v1, ushort v2)
        {
            int value = ((short)machine.getVar(v1)) - 1;
            machine
                .setVar(v1, (ushort)value)
                .branch(value < v2);
        }

        public static void op_inc_chk(this Machine machine, ushort v1, ushort v2)
        {
            int value = ((short)machine.getVar(v1)) + 1;
            machine
                .setVar(v1, (ushort)value)
                .branch(value > v2);
        }

        public static void op_je(this Machine machine, ushort v1, ushort v2)
        {
            machine.branch(v1 == v2);
        }

        public static void op_je(this Machine machine, List<ushort> operands)
        {
            bool branchOn = false;
            for (int i = 1; i < operands.Count; i++)
            {
                if (operands[0] == operands[i])
                {
                    branchOn = true;
                }
            }
            machine.branch(branchOn);
        }

        public static void op_jg(this Machine machine, ushort v1, ushort v2)
        {
            machine.branch((short)v1 > (short)v2);
        }

        public static void op_jin(this Machine machine, ushort v1, ushort v2)
        {
            machine.branch(machine.ObjectTable.getParent(v1) == v2);
        }

        public static void op_jl(this Machine machine, ushort v1, ushort v2)
        {
            machine.branch((short)v1 < (short)v2);
        }

        public static void op_or(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(machine.pc_getByte(), (ushort)(v1 | v2));
        }

        public static void op_set_attr(this Machine machine, ushort v1, ushort v2)
        {
            machine.ObjectTable.setObjectAttribute(v1, v2, true);
        }

        public static void op_test(this Machine machine, ushort v1, ushort v2)
        {
            machine.branch((v1 & v2) == v2);
        }

        public static void op_test_attr(this Machine machine, ushort v1, ushort v2)
        {
            machine
.DebugWrite("Looking for attribute in obj " + v1 + " attribute:" + machine.ObjectTable.getObjectAttribute(v1, v2))
.branch(machine.ObjectTable.getObjectAttribute(v1, v2) == true);
        }

        public static void op_clear_attr(this Machine machine, ushort v1, ushort v2)
        {
            machine.ObjectTable.setObjectAttribute(v1, v2, false);
        }

        public static void op_store(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(v1, v2);
        }

        public static void op_insert_obj(this Machine machine, ushort v1, ushort v2)
        {
            int newSibling = machine.ObjectTable.getChild(v2); // 
            machine.ObjectTable
                .unlinkObject(v1)
                .setChild(v2, v1)
                .setParent(v1, v2)
                .setSibling(v1, newSibling);
            // after the operation the child of v2 is v1
            // and the sibling of v1 is whatever was previously the child of v2.
            // All children of v1 move with it. (Initially O can be at any point in the object tree; it may legally have parent zero.)    
        }

        public static void op_loadw(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(machine.pc_getByte(), Convert.ToUInt16(machine.Memory.getWord(v1 + (uint)(2 * v2))));
        }

        public static void op_loadb(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(machine.pc_getByte(), Convert.ToUInt16(machine.Memory.getByte(v1 + (uint)v2)));
        }

        public static void op_get_prop(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getObjectProperty(v1, v2));
        }

        public static void op_get_prop_addr(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getObjectPropertyAddress(v1, v2));
        }

        public static void op_get_next_addr(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getNextObjectPropertyIdAfter(v1, v2));
        }

        public static void op_add(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(machine.pc_getByte(), (ushort)((short)v1 + (short)v2));
        }

        public static void op_sub(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(machine.pc_getByte(), (ushort)Convert.ToInt32((short)v1 - (short)v2));
        }

        public static void op_div(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(machine.pc_getByte(), (ushort)Convert.ToInt32((short)v1 / (short)v2));
        }

        public static void op_mod(this Machine machine, ushort v1, ushort v2)
        {
            if (v2 == 0)
            {
                // Interpreter cannot div by 0
                machine.Terminate("Division by zero");
                return;
            }

            int result = (short)v1 % (short)v2;
            machine.setVar(machine.pc_getByte(), (ushort)result);
        }

        public static void op_mul(this Machine machine, ushort v1, ushort v2)
        {
            machine.setVar(machine.pc_getByte(), (ushort)Convert.ToInt32((short)v1 * (short)v2));
        }

        public static TwoOperandOpcode process2OP(this Machine machine, int opcode, List<ushort> operands)
        {
            if (!System.Enum.IsDefined(typeof(TwoOperandOpcode), opcode))
            {
                throw new InvalidCastException(opcode + " is not a defined value for enum type " +
                  typeof(TwoOperandOpcode).FullName);
            }

            TwoOperandOpcode twoOperandOpcode = (TwoOperandOpcode)opcode;
            if (machine.ShouldBreakFor(BreakpointType.Opcode) && machine.OpcodeBreakpoints.Contains(twoOperandOpcode))
            {
                machine.Break(breakpointType: BreakpointType.Opcode);
                if (!machine.ShouldContinueFor(BreakpointType.Opcode))
                {
                    return twoOperandOpcode;
                }
            }
            switch (twoOperandOpcode)
            {
                case TwoOperandOpcode.op_add:
                    op_add(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_and:
                    op_and(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_clear_attr:
                    op_clear_attr(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_dec_chk:
                    op_dec_chk(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_div:
                    op_div(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_get_next_addr:
                    op_get_next_addr(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_get_prop:
                    op_get_prop(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_get_prop_addr:
                    op_get_prop_addr(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_inc_chk:
                    op_inc_chk(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_insert_obj:
                    op_insert_obj(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_je:
                    op_je(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_jg:
                    op_jg(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_jin:
                    op_jin(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_jl:
                    op_jl(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_loadb:
                    op_loadb(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_loadw:
                    op_loadw(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_mod:
                    op_mod(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_mul:
                    op_mul(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_or:
                    op_or(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_set_attr:
                    op_set_attr(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_store:
                    op_store(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_sub:
                    op_sub(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_test:
                    op_test(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                case TwoOperandOpcode.op_test_attr:
                    op_test_attr(machine: machine, v1: operands[0], v2: operands[1]);
                    break;
                default:
                    fail_unimplemented(machine: machine);
                    break;
            }
            return twoOperandOpcode;
            /*
            string? opcodeName = twoOperandOpcode.ToString();
            MethodInfo opcodeMethod = typeof(Machine).GetMethod(opcodeName);

            if (machine.DebugEnabled)
            {
                Debug.Write(machine.ProgramCounterStart.ToString("X4") + "  " + machine.stateString() + " : [2OP/" + opcode.ToString("X2") + "] " + opcodeName);
                foreach (ushort v in operands)
                {
                    Debug.Write(" " + v);
                }

                Debug.WriteLine("");
            }
            opcodeMethod.Invoke(machine, new object[] { operands[0], operands[1] });
            */
        }
    }
}
