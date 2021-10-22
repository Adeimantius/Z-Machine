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

        [TestMethod]
        public void TestFirstScreen()
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
            Assert.AreEqual(expected: @"ZORK I: The Great Underground Empire
Copyright(c) 1981, 1982, 1983 Infocom, Inc.All rights reserved.
ZORK is a registered trademark of Infocom, Inc.
Revision 88 / Serial number 840726

West of House
You are standing in an open field west of a white house, with a boarded front door.
There is a small mailbox here.

>", actual: staticIO.GetOutput());
        }
    }
}