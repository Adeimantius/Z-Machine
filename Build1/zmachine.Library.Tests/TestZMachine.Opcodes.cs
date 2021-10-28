namespace zmachine.Library.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using zmachine.Library.Enumerations;
    using zmachine.Library.Interfaces;
    using zmachine.Library.Models;
    using zmachine.Library.Models.IO;

    [TestClass]
    public class TestZMachineOpcodes
    {
        [TestMethod]
        public void Test_op_rtrue()
        {
            IIO staticIO = new NullIO();
            Machine machine = new Machine(
                staticIO,
                TestZMachine.ZorkPath,
                new Dictionary<BreakpointType, BreakpointAction>
                {
                });
        }
    }
}
