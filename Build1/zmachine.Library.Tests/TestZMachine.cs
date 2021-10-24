namespace zmachine.Library.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using zmachine.Library.Enumerations;
    using System.Linq;
    using System;
    using zmachine.Library.Models;
    using System.Collections.Immutable;

    [TestClass]
    public class TestZMachine
    {
        private readonly ImmutableDictionary<string, string> Screens = (new Dictionary<string, string>()
        {
            { nameof(TestFirstScreen), "ZORK I: The Great Underground Empire\nCopyright (c) 1981, 1982, 1983 Infocom, Inc. All rights reserved.\r\nZORK is a registered trademark of Infocom, Inc.\nRevision 88 / Serial number 840726\r\n\r\nWest of House\r\nYou are standing in an open field west of a white house, with a boarded front door.\r\nThere is a small mailbox here.\r\n\r\n>" },
            { nameof(TestSaveRestore), "South of House\r\nYou are facing the south side of a white house. There is no door here, and all the windows are boarded.\r\n\r\n>" },
            { nameof(TestScore), "Your score is 0 (total of 350 points), in 0 moves.\r\nThis gives you the rank of Beginner.\r\n\r\n>" },
        }).ToImmutableDictionary();

        public static string ZorkPath
        {
            get
            {
                string? filePath = System.AppContext.BaseDirectory; //returns path "C:\..\bin\debug"
                int pos = filePath.IndexOf("zmachine.Library.Tests");
                if (pos == -1)
                {
                    throw new System.Exception();
                }
                string? pathSubstr = filePath.Substring(0, pos);

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
                breakpointTypes: new Dictionary<BreakpointType,BreakpointAction>
                {
                    { BreakpointType.InputRequired, BreakpointAction.Halt },
                });

            while (!machine.Finished)
            {
                var instructionInfo = machine.processInstruction();
                if (instructionInfo.BreakpointType != BreakpointType.None)
                {
                    break;
                }
            }

            string? actualOutput = staticIO.GetOutput(keepContents: true);
            string? identicalOutput = staticIO.GetOutput(keepContents: false);
            string? emptyOutput = staticIO.GetOutput(keepContents: false);

            Assert.AreEqual(expected: 386U, actual: machine.InstructionCounter);
            Assert.AreEqual(expected: this.Screens[nameof(TestFirstScreen)], actual: actualOutput);
            Assert.AreEqual(expected: this.Screens[nameof(TestFirstScreen)], actual: identicalOutput);
            Assert.AreEqual(expected: "", actual: emptyOutput);
        }

        [TestMethod]
        public void TestQuit()
        {
            StaticIO staticIO = new StaticIO("quit\nY\n");
            Machine machine = new Machine(
                io: staticIO,
                programFilename: ZorkPath,
                breakpointTypes: new Dictionary<BreakpointType, BreakpointAction>() { })
            {
            };

            List<(InstructionInfo instructionInfo, string output)> stepTranscripts = new List<(InstructionInfo instructionInfo, string output)>();
            while (!machine.Finished)
            {
                InstructionInfo instructionInfo = machine.processInstruction();
                string? transcript = staticIO.GetOutput(keepContents: false);
                if (transcript is not null && !string.IsNullOrEmpty(transcript))
                {
                    stepTranscripts.Add((
                        instructionInfo: instructionInfo,
                        output: transcript));
                }
                if (instructionInfo.BreakpointType != BreakpointType.None)
                {
                    Assert.AreEqual(expected: BreakpointType.Complete, actual: instructionInfo.BreakpointType);
                    break;
                }
            }
            // test quit output
            throw new Exception();
        }

        [TestMethod]
        public void TestScore()
        {
            StaticIO staticIO = new StaticIO("score\n");
            Machine machine = new Machine(
                io: staticIO,
                programFilename: ZorkPath,
                breakpointTypes: new Dictionary<BreakpointType, BreakpointAction>
                {
                    { BreakpointType.InputRequired, BreakpointAction.Halt },
                })
            {

                // skip the first screen
                BreakAfter = 387
            };

            while (!machine.Finished)
            {
                InstructionInfo instructionInfo = machine.processInstruction();
                if (instructionInfo.BreakpointType != BreakpointType.None)
                {
                    Assert.AreEqual(expected: BreakpointType.InputRequired, actual: instructionInfo.BreakpointType);
                    break;
                }
            }
            string? scoreScreen = staticIO.GetOutput(keepContents: false).Substring(startIndex: this.Screens[nameof(TestFirstScreen)].Length);
            Assert.AreEqual(
                expected: Screens[nameof(TestScore)],
                actual: scoreScreen);
        }

        [TestMethod]
        public void TestSaveRestore()
        {
            StaticIO staticIO = new StaticIO("south\nsave\nnorth\nrestore\nopen mailbox\n");
            Machine machine = new Machine(
                io: staticIO,
                programFilename: ZorkPath,
                breakpointTypes: new Dictionary<BreakpointType, BreakpointAction>{
                    //{ BreakpointType.Opcode, BreakpointAction.Continue }
                });
            {
            };
            machine.OpcodeBreakpoints.Add(NoOperandOpcode.op_save);
            machine.OpcodeBreakpoints.Add(NoOperandOpcode.op_show_status);

            List<(InstructionInfo instructionInfo, string output)> stepTranscripts = new List<(InstructionInfo instructionInfo, string output)>();
            while (!machine.Finished)
            {
                var instruction = machine.InstructionCounter;
                InstructionInfo instructionInfo = machine.processInstruction();
                string? transcript = staticIO.GetOutput(keepContents: false);
                if (transcript is not null && !string.IsNullOrEmpty(transcript))
                {
                    stepTranscripts.Add((
                        instructionInfo: instructionInfo,
                        output: transcript));
                }
                Assert.AreEqual(expected: BreakpointType.None, actual: instructionInfo.BreakpointType);
            }

            // number of total steps
            Assert.AreEqual(expected: 968, actual: machine.InstructionCounter);
            // number of non empty steps
            Assert.AreEqual(expected: 0, actual: stepTranscripts.Count);
            var expected = new string[]
                {
                    Screens[nameof(TestFirstScreen)],
                    Screens[nameof(TestSaveRestore)],
                };
            for (int i = 0; i<stepTranscripts.Count(); i++)
            {
                Assert.AreEqual(
                expected: expected[i],
                actual: stepTranscripts[i].output);
            }
            
        }
    }
}