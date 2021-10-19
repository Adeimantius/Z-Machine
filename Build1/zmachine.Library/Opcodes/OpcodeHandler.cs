namespace zmachine.Library.Opcodes
{
    using System.Reflection;

    public abstract class OpcodeHandler
    {
        public readonly static string ClassName = null;

        /// <summary>
        /// Default name to class name, but allow override
        /// </summary>
        /// <returns></returns>
        public static string Name
        {
            get
            {
                return ClassName is null ? MethodBase.GetCurrentMethod().DeclaringType.Name : ClassName;
            }
        }

        public static void fail_unimplemented(Machine machine)
        {
            machine.Terminate(error: "Unimplemented function: " + Name);
        }
    }
}
