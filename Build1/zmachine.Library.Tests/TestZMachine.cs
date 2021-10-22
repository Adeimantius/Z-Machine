namespace zmachine.Library.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using zmachine.Library.Enumerations;

    [TestClass]
    public class TestZMachine
    {
        public static string ZorkPath
        {
            get
            {
                var filePath = System.AppContext.BaseDirectory; //returns path "C:\..\bin\debug"
                var pos = filePath.IndexOf("zmachine.Library.Tests");
                if (pos == -1)
                {
                    throw new System.Exception();
                }
                var pathSubstr = filePath.Substring(0, pos);

                return System.IO.Path.Join(pathSubstr, "zmachine", "zmachine", "ZORK1.DAT");
            }
        }

        [DataTestMethod]
        [DataRow("ZORK I: The Great Underground Empire\nCopyright(c) 1981, 1982, 1983 Infocom, Inc.All rights reserved.\r\nZORK is a registered trademark of Infocom, Inc.\nRevision 88 / Serial number 840726\r\n\r\nWest of House\r\nYou are standing in an open field west of a white house, with a boarded front door.\r\nThere is a small mailbox here.\r\n\r\n >")]
        public void TestFirstScreen(string expected)
        {
            StaticIO staticIO = new StaticIO();
            Machine machine = new Machine(
                io: staticIO,
                programFilename: ZorkPath,
                breakpointTypes: new BreakpointType[]
                {
                    BreakpointType.InputRequired,
                    BreakpointType.Terminate
                });

            while (!machine.Finished)
            {
                var breakpoint = machine.processInstruction();
                if (breakpoint != BreakpointType.None)
                {
                    break;
                }
            }
            Assert.AreEqual(expected: 386U, actual: machine.InstructionCounter);
            Assert.IsTrue(string.Compare(expected, staticIO.GetOutput()) == 0);
        }
    }
}