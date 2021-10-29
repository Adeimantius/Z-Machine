namespace zmachine.Library.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;
    using zmachine.Library.Enumerations;
    using zmachine.Library.Extensions;
    using zmachine.Library.Interfaces;
    using zmachine.Library.Models;
    using zmachine.Library.Models.IO;

    [TestClass]
    public class TestZMachineOpcodes
    {
        [TestMethod]
        public void Test_op_rtrue()
        {
            // Arrange
            IIO staticIO = new NullIO();
            Mock<Machine>? machineMock = new Mock<Machine>(
                staticIO,
                new CPUState(),
                new Dictionary<BreakpointType, BreakpointAction>
                {
                });

            machineMock
                .Setup(machine => machine.popRoutineData(It.Is<ushort>((returnValue) => returnValue == 1)))
                .Verifiable();

            // Act
            MachineOpcodeExtensions.op_rtrue(machine: machineMock.Object);

            // Verify
            machineMock.Verify();
            machineMock.VerifyAll();
            machineMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void Test_op_rfalse()
        {
            // Arrange
            IIO staticIO = new NullIO();
            Mock<Machine>? machineMock = new Mock<Machine>(
                staticIO,
                new CPUState(),
                new Dictionary<BreakpointType, BreakpointAction>
                {
                });

            machineMock
                .Setup(machine => machine.popRoutineData(It.Is<ushort>((returnValue) => returnValue == 0)))
                .Verifiable();

            // Act
            MachineOpcodeExtensions.op_rfalse(machine: machineMock.Object);

            // Assert
            machineMock.Verify();
            machineMock.VerifyAll();
            machineMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void Test_op_quit()
        {
            //Arrange
            StaticIO staticIO = new StaticIO("quit\nY\n\n");
            var machineMock = new Mock<Machine>(
                staticIO,
                TestZMachine.ZorkPath,
                new Dictionary<BreakpointType, BreakpointAction>());

            machineMock.Setup(m => m.QuitNicely())
                .CallBase()
                .Verifiable();

            // Act
            MachineOpcodeExtensions.op_quit(machine: machineMock.Object);

            // Assert
            machineMock.Verify(m => m.QuitNicely(), Times.Once());
            machineMock.Verify();
            machineMock.VerifyAll();
            machineMock.VerifyNoOtherCalls();
            Assert.IsTrue(machineMock.Object.Finished);
        }
    }
}
