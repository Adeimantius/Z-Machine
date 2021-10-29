#define DISABLE_ASSERT

namespace zmachine.Library.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
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
        public void Test_popRoutineDataUnderrun()
        {
            // Arrange
            ushort returnVal = 12345;
            IIO staticIO = new NullIO();
            Mock<Machine>? machineMock = new Mock<Machine>(
                staticIO,
                new CPUState(),
                new Dictionary<BreakpointType, BreakpointAction>
                {
                });
            Machine machine = machineMock.Object;
            Machine.DEBUG_ASSERT_DISABLED = true;
            machineMock.Setup(m => m.popRoutineData(It.IsAny<ushort>()))
                .CallBase()
                .Verifiable();


            // Act
            Assert.IsFalse(machine.Finished);
            machine.popRoutineData(returnValue: returnVal);

            // Assert
            Assert.IsTrue(machine.Finished);
        }

        [TestMethod]
        public void Test_popRoutineData()
        {
            // Arrange
            ushort returnVal = 12345;
            IIO staticIO = new NullIO();
            Mock<Machine>? machineMock = new Mock<Machine>(
                staticIO,
                new CPUState(),
                new Dictionary<BreakpointType, BreakpointAction>
                {
                });
            Machine machine = machineMock.Object;
            Machine.DEBUG_ASSERT_DISABLED = true;
            machineMock.Setup(m => m.popRoutineData(It.IsAny<ushort>()))
                .CallBase()
                .Verifiable();
            machineMock.Setup(m => m.pushRoutineData(It.IsAny<List<ushort>>()))
                .CallBase()
                .Verifiable();
            machineMock.Setup(m => m.setVar(It.IsAny<ushort>(), It.IsAny<ushort>()))
                .CallBase()
                .Verifiable();
            machineMock.Setup(m => m.pc_getByte())
                .Returns(Machine.VAR_TOP_OF_STACK)
                .Verifiable();

            Assert.AreEqual(expected: 0U, machine.CallDepth);
            machine.pushRoutineData(new List<ushort> { 0 });
            Assert.AreEqual(expected: 1U, machine.CallDepth);

            // Act
            machine.popRoutineData(returnValue: returnVal);

            // Assert
            Assert.AreEqual(expected: machine.getVar(Machine.VAR_TOP_OF_STACK), actual: returnVal);
            Assert.AreEqual(expected: 0U, machine.CallDepth);
            var callStack = machine.CallStackAt(0);
            Assert.AreEqual(expected: callStack.numLocalVars, actual: 0U);
            Assert.AreEqual(expected: callStack.returnAddress, actual: 0U);
            Assert.AreEqual(expected: callStack.stackFrameAddress, actual: 0U);
            Assert.IsFalse(machine.Finished);

            machineMock.Verify();
            machineMock.VerifyNoOtherCalls();
        }
    }
}
