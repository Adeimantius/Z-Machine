using zmachine.Library.Enumerations;

namespace zmachine.Library.Extensions;

/// <summary>
///     1OP Classes
///     Branch Opcodes 128 - 130
///     Store Opcodes 129 - 132, 136, 142 - 143
/// </summary>
public static partial class MachineOpcodeExtensions
{
    public static void op_jz(this Machine machine, ushort v1)
    {
        machine.branch(v1 == 0);
    }

    public static void op_get_sibling(this Machine machine, ushort v1)
    {
        machine
            .setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getSibling(v1))
            .branch(machine.ObjectTable.getSibling(v1) != 0);
    }

    public static void op_get_child(this Machine machine, ushort v1)
    {
        machine
            .setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getChild(v1))
            .branch(machine.ObjectTable.getChild(v1) != 0);
    }

    public static void op_get_parent(this Machine machine, ushort v1)
    {
        machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getParent(v1));
    }

    public static void op_get_prop_len(this Machine machine, ushort v1)
    {
        machine.setVar(machine.pc_getByte(), (ushort)machine.ObjectTable.getObjectPropertyLengthFromAddress(v1));
    }

    public static void op_inc(this Machine machine, ushort v1)
    {
        machine.setVar(v1, (ushort)Convert.ToInt32((short)machine.getVar(v1) + 1));
    }

    public static void op_dec(this Machine machine, ushort v1)
    {
        machine.setVar(v1, (ushort)Convert.ToInt32((short)machine.getVar(v1) - 1));
    }

    public static void op_not(this Machine machine, ushort v1)
    {
        machine.setVar(machine.pc_getByte(), (ushort)~v1);
    }

    public static void op_print_addr(Machine machine, ushort v1)
    {
        machine.IO.WriteLine(machine.Memory.getZSCII(v1, 0).str);
    }

    public static void op_remove_obj(this Machine machine, ushort v1)
    {
        machine.ObjectTable.setParent(v1, 0);
    }

    /// <summary>
    ///     original code has this as write instead of writeline
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="v1"></param>
    public static void op_print_obj(this Machine machine, ushort v1)
    {
        machine.IO.Write(machine.ObjectTable.objectName(v1));
    }

    public static void op_ret(this Machine machine, ushort v1)
    {
        machine.popRoutineData(v1);
    }

    public static void op_jump(this Machine machine, ushort v1)
    {
        machine.ProgramCounter += (uint)(Convert.ToInt32((short)v1) - 2);
    }

    /// <summary>
    ///     original code has this as write instead of writeline
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="v1"></param>
    public static void op_print_paddr(this Machine machine, ushort v1)
    {
        machine.IO.Write(machine.Memory.getZSCII((uint)v1 * 2, 0).str);
    }

    public static void op_load(this Machine machine, ushort v1)
    {
        machine.setVar(machine.pc_getWord(), v1);
    }

    public static SingleOperandOpcodes process1OP(this Machine machine, int opcode, ushort operand1)
    {
        if (!Enum.IsDefined(typeof(SingleOperandOpcodes), opcode))
        {
            throw new InvalidCastException(opcode + " is not a defined value for enum type " +
                                           typeof(SingleOperandOpcodes).FullName);
        }

        SingleOperandOpcodes singleOperandOpcode = (SingleOperandOpcodes)opcode;
        if (machine.ShouldBreakFor(BreakpointType.Opcode) && machine.OpcodeBreakpoints.Contains(singleOperandOpcode))
        {
            machine.Break(BreakpointType.Opcode);
            if (!machine.ShouldContinueFor(BreakpointType.Opcode))
            {
                return singleOperandOpcode;
            }
        }

        switch (singleOperandOpcode)
        {
            case SingleOperandOpcodes.op_dec:
                op_dec(machine, operand1);
                break;
            case SingleOperandOpcodes.op_get_child:
                op_get_child(machine, operand1);
                break;
            case SingleOperandOpcodes.op_get_parent:
                op_get_parent(machine, operand1);
                break;
            case SingleOperandOpcodes.op_get_prop_len:
                op_get_prop_len(machine, operand1);
                break;
            case SingleOperandOpcodes.op_get_sibling:
                op_get_sibling(machine, operand1);
                break;
            case SingleOperandOpcodes.op_inc:
                op_inc(machine, operand1);
                break;
            case SingleOperandOpcodes.op_jump:
                op_jump(machine, operand1);
                break;
            case SingleOperandOpcodes.op_jz:
                op_jz(machine, operand1);
                break;
            case SingleOperandOpcodes.op_load:
                op_load(machine, operand1);
                break;
            case SingleOperandOpcodes.op_print_addr:
                op_print_addr(machine, operand1);
                break;
            case SingleOperandOpcodes.op_print_obj:
                op_print_obj(machine, operand1);
                break;
            case SingleOperandOpcodes.op_print_paddr:
                op_print_paddr(machine, operand1);
                break;
            case SingleOperandOpcodes.op_remove_obj:
                op_remove_obj(machine, operand1);
                break;
            case SingleOperandOpcodes.op_ret:
                op_ret(machine, operand1);
                break;
            default:
                fail_unimplemented(machine);
                break;
        }

        return singleOperandOpcode;
        /*
        string? opcodeName = singleOperandOpcode.ToString();
        MethodInfo opcodeMethod = typeof(Machine).GetMethod(opcodeName);

        if (machine.DebugEnabled)
        {
            machine.DebugWrite(machine.ProgramCounterStart.ToString("X4") + "  " + machine.stateString() + " : [1OP/" + opcode.ToString("X2") + "] " + opcodeName + " " + operand1.ToString());
        }
        opcodeMethod.Invoke(machine, new object[] { operand1 });
        */
    }
}