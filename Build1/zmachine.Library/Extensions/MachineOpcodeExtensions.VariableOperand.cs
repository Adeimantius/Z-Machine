namespace zmachine.Library.Extensions
{
    using zmachine.Library.Enumerations;

    /// <summary>
    /// Variable Operand Classes
    /// </summary>
    public static partial class MachineOpcodeExtensions
    {
        public static void op_call(this Machine machine, List<ushort> operands)
        {
            if (operands[0] == 0)
            {
                machine.setVar(machine.pc_getByte(), 0); //set return value to zero
            }
            else
            {
                machine.pushRoutineData(operands);
            }
        }

        public static void op_storew(this Machine machine, List<ushort> operands)
        {
            machine.Memory.setWord((uint)(operands[0] + 2 * operands[1]), operands[2]);
        }

        public static void op_storeb(this Machine machine, List<ushort> operands)
        {
            machine.Memory.setByte((uint)(operands[0] + operands[1]), (byte)operands[2]);
        }

        public static void op_put_prop(this Machine machine, List<ushort> operands)
        {
            machine.ObjectTable.setObjectProperty(operands[0], operands[1], operands[2]);
        }

        public static void op_sread(this Machine machine, List<ushort> operands)
        {
            machine.ReadLex(operands);
        }

        public static void op_print_char(this Machine machine, List<ushort> operands)
        {
            machine.IO.Write("" + machine.Memory.getZChar(operands[0]));
        }

        public static void op_print_num(this Machine machine, List<ushort> operands)
        {
            machine.IO.Write(Convert.ToString(value: (short)operands[0], toBase: 10));
        }

        public static void op_random(this Machine machine, List<ushort> operands)
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

        public static void op_push(this Machine machine, List<ushort> operands)
        {
            machine.setVar(0, operands[0]);
        }

        public static void op_pull(this Machine machine, List<ushort> operands)
        {
            machine.setVar(operands[0], machine.getVar(0));
        }

        public static void op_split_window(this Machine machine, List<ushort> operands)
        {
            fail_unimplemented(machine);
        }

        public static void op_set_window(this Machine machine, List<ushort> operands)
        {
            fail_unimplemented(machine);
        }

        public static void op_output_stream(this Machine machine, List<ushort> operands)
        {
            if (operands[0] != 0)
            {
                if (operands[0] < 0)
                {
                    // deselect output stream
                }
                else if (operands[0] == 3)
                {
                    Memory.StringAndReadLength str = machine.Memory.getZSCII((uint)operands[1] + 2, machine.Memory.getWord(operands[1]));
                }
                else
                {
                    // select output stream
                }
            }
        }

        public static void op_input_stream(this Machine machine, List<ushort> operands)
        {
            fail_unimplemented(machine);
        }

        public static VariableOperandOpcode processVAR(this Machine machine, int opcode, List<ushort> operands)
        {
            if (!System.Enum.IsDefined(typeof(TwoOperandOpcode), opcode))
            {
                throw new InvalidCastException(opcode + " is not a defined value for enum type " +
                  typeof(TwoOperandOpcode).FullName);
            }

            VariableOperandOpcode variableOperandOpcode = (VariableOperandOpcode)opcode;
            if (machine.ShouldBreakFor(BreakpointType.Opcode) && machine.OpcodeBreakpoints.Contains(variableOperandOpcode))
            {
                machine.Break(breakpointType: BreakpointType.Opcode);
                if (!machine.ShouldContinueFor(BreakpointType.Opcode))
                {
                    return variableOperandOpcode;
                }
            }
            switch (variableOperandOpcode)
            {
                case VariableOperandOpcode.op_call:
                    op_call(machine: machine, operands: operands);
                    break;
                case VariableOperandOpcode.op_print_char:
                    op_print_char(machine: machine, operands: operands);
                    break;
                case VariableOperandOpcode.op_print_num:
                    op_print_num(machine: machine, operands: operands);
                    break;
                case VariableOperandOpcode.op_pull:
                    op_pull(machine: machine, operands: operands);
                    break;
                case VariableOperandOpcode.op_push:
                    op_push(machine: machine, operands: operands);
                    break;
                case VariableOperandOpcode.op_put_prop:
                    op_put_prop(machine: machine, operands: operands);
                    break;
                case VariableOperandOpcode.op_random:
                    op_random(machine: machine, operands: operands);
                    break;
                case VariableOperandOpcode.op_set_window:
                    op_set_window(machine: machine, operands: operands);
                    break;
                case VariableOperandOpcode.op_split_window:
                    op_split_window(machine: machine, operands: operands);
                    break;
                case VariableOperandOpcode.op_sread:
                    if (machine.ShouldBreakFor(BreakpointType.InputRequired))
                    {
                        machine.Break(BreakpointType.InputRequired);
                    }
                    if (machine.ShouldContinueFor(BreakpointType.InputRequired))
                    {
                        op_sread(machine: machine, operands: operands);
                    }
                    break;
                case VariableOperandOpcode.op_storeb:
                    op_storeb(machine: machine, operands: operands);
                    break;
                case VariableOperandOpcode.op_storew:
                    op_storew(machine: machine, operands: operands);
                    break;
                default:
                    fail_unimplemented(machine: machine);
                    break;
            }
            return variableOperandOpcode;
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
            opcodeMethod.Invoke(machine, new object[] { operands });
            */
        }
    }
}
