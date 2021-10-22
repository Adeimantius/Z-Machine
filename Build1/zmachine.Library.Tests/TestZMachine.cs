namespace zmachine.Library.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using zmachine.Library.Enumerations;

    [TestClass]
    public class TestZMachine
    {
        private readonly string[] Screens = new string[1]
        {
            "ZORK I: The Great Underground Empire\nCopyright (c) 1981, 1982, 1983 Infocom, Inc. All rights reserved.\r\nZORK is a registered trademark of Infocom, Inc.\nRevision 88 / Serial number 840726\r\n\r\nWest of House\r\nYou are standing in an open field west of a white house, with a boarded front door.\r\nThere is a small mailbox here.\r\n\r\n>",
        };

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
                });

            while (!machine.Finished)
            {
                var breakpoint = machine.processInstruction();
                if (breakpoint != BreakpointType.None)
                {
                    break;
                }
            }
            
            var actualOutput = staticIO.GetOutput(keepContents: true);
            var identicalOutput = staticIO.GetOutput(keepContents: false);
            var emptyOutput = staticIO.GetOutput(keepContents: false);
         
            Assert.AreEqual(expected: 386U, actual: machine.InstructionCounter);
            Assert.AreEqual(expected: Screens[0], actual: actualOutput);
            Assert.AreEqual(expected: Screens[0], actual: identicalOutput);
            Assert.AreEqual(expected: "", actual: emptyOutput);
        }

        [TestMethod]
        public void TestScore()
        {
            StaticIO staticIO = new StaticIO("score\n");
            Machine machine = new Machine(
                io: staticIO,
                programFilename: ZorkPath,
                breakpointTypes: new BreakpointType[]
                {
                    BreakpointType.InputRequired,
                });

            // skip the first screen
            machine.BreakAfter = 387;

            while (!machine.Finished)
            {
                var breakpoint = machine.processInstruction();
                if (breakpoint != BreakpointType.None)
                {
                    Assert.AreEqual(expected: BreakpointType.InputRequired, actual: breakpoint);
                    break;
                }
            }
            var scoreScreen = staticIO.GetOutput(keepContents: false).Substring(startIndex: Screens[0].Length);
            Assert.AreEqual(
                expected: "Your score is 0 (total of 350 points), in 0 moves.\r\nThis gives you the rank of Beginner.\r\n\r\n>",
                actual: scoreScreen);
        }
    }
}