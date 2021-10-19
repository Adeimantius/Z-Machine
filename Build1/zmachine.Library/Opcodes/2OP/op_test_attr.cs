namespace zmachine.Library.Opcodes._2OP
{
    public class op_test_attr : OpcodeHandler_2OP
    {
        public static void run(Machine machine, ushort v1, ushort v2)
        {
            //                Debug.WriteLine("Looking for attribute in obj " + v1 + " attribute:" + machine.objectTable.getObjectAttribute(v1, v2));
            machine.branch(machine.ObjectTable.getObjectAttribute(v1, v2) == true);
        }
    }
}
