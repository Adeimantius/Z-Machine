namespace zmachine.Library.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using zmachine.Library.Enumerations;
    using zmachine.Library.Interfaces;
    using zmachine.Library.Models;
    using zmachine.Library.Models.IO;

    [TestClass]
    public class TestZMachineCore
    {
        [TestMethod]
        public void Test_pcGetByte()
        {
            IIO staticIO = new NullIO();
            Machine machine = new Machine(
                staticIO,
                TestZMachine.ZorkPath,
                new Dictionary<BreakpointType, BreakpointAction>
                {
                });
            byte pcByte = machine.pc_getByte();
        }

        [TestMethod]
        public void Test_setVar()
        {
            IIO staticIO = new NullIO();
            Machine machine = new Machine(
                staticIO,
                TestZMachine.ZorkPath,
                new Dictionary<BreakpointType, BreakpointAction>
                {
                });

            ushort value = 2345;
            ushort[] variables = new ushort[] { 0, 0x0f, 0xff };
            foreach (ushort variable in variables)
            {
                machine.setVar(
                    variable: variable,
                    value: value);
            }
            // test all three branches of setVar
            // make sure memory setWord was called
            // Variable number $00 refers to the top of the stack
            //$01 to $0f mean the local variables of the current routine
            //and $10 to $ff mean the global variables.
        }

        [TestMethod]
        public void Test_popRoutineData()
        {
            IIO staticIO = new NullIO();
            Machine machine = new Machine(
                staticIO,
                new CPUState(),
                new Dictionary<BreakpointType, BreakpointAction>
                {
                });

            ushort returnVal = 12345;
            machine.pushRoutineData(new List<ushort> { 0 });
            // call depth now 1
            machine.popRoutineData(returnValue: returnVal);
            // we've now called setVar 0 with value 12345

        }
    }
}
