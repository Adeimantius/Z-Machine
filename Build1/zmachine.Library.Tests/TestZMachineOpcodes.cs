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
        public void Test_setWord()
        {
            IIO staticIO = new NullIO();
            Machine machine = new Machine(
                staticIO,
                TestZMachine.ZorkPath,
                new Dictionary<BreakpointType, BreakpointAction>
                {
                });
            uint address = 1234;
            ushort value = 2345;
            machine.Memory.setWord(address: address, val: value);
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
            ushort variable = 1234;
            ushort value = 2345;
            machine.setVar(variable: variable, value: value);
            // test all three branches of setVar
            // make sure memory setWord was called
        }

        [TestMethod]
        public void Test_popRoutineData()
        {
            IIO staticIO = new NullIO();
            Machine machine = new Machine(
                staticIO,
                TestZMachine.ZorkPath,
                new Dictionary<BreakpointType, BreakpointAction>
                {
                });

            ushort returnVal = 12345;
            machine.popRoutineData(returnVal: returnVal);
        }

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
