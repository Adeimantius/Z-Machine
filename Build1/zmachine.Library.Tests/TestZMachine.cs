using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zmachine.Library.Tests
{
    [TestClass]
    public class TestZMachine
    {
        [TestMethod]
        public void TestFirstScreen()
        {
            StaticIO staticIO = new StaticIO();
            Machine machine = new Machine(
                io: staticIO,
                programFilename: "ZORK1.DAT");

            int numInstructionsProcessed = 0;
            while (!machine.Finished)
            {

                machine.processInstruction();
                ++numInstructionsProcessed;
            }
        }
    }
}