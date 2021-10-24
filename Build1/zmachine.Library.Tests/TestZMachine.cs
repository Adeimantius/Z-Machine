using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using zmachine.Library.Enumerations;
using zmachine.Library.Models;

namespace zmachine.Library.Tests;

[TestClass]
public class TestZMachine
{
    private readonly ImmutableDictionary<string, string> Screens = new Dictionary<string, string>
    {
        {
            nameof(TestFirstScreen),
            "ZORK I: The Great Underground Empire\nCopyright (c) 1981, 1982, 1983 Infocom, Inc. All rights reserved.\r\nZORK is a registered trademark of Infocom, Inc.\nRevision 88 / Serial number 840726\r\n\r\nWest of House\r\nYou are standing in an open field west of a white house, with a boarded front door.\r\nThere is a small mailbox here.\r\n\r\n>"
        },
        {
            nameof(TestSaveRestore),
            "South of House\r\nYou are facing the south side of a white house. There is no door here, and all the windows are boarded.\r\n\r\n>"
        },
        {
            nameof(TestScore),
            "Your score is 0 (total of 350 points), in 0 moves.\r\nThis gives you the rank of Beginner.\r\n\r\n>"
        },
        {
            nameof(TestQuit),
            "Your score is 0 (total of 350 points), in 0 moves.\r\nThis gives you the rank of Beginner.\r\nDo you wish to leave the game? (Y is affirmative): >Ok.\r\n>"
        }
    }.ToImmutableDictionary();

    public static string ZorkPath
    {
        get
        {
            string? filePath = AppContext.BaseDirectory; //returns path "C:\..\bin\debug"
            int pos = filePath.IndexOf("zmachine.Library.Tests");
            if (pos == -1)
            {
                throw new Exception();
            }

            string? pathSubstr = filePath.Substring(0, pos);

            return Path.Join(pathSubstr, "zmachine", "zmachine", "ZORK1.DAT");
        }
    }

    [TestMethod]
    public void TestFirstScreen()
    {
        StaticIO? staticIO = new StaticIO();
        Machine? machine = new Machine(
            staticIO,
            ZorkPath,
            new Dictionary<BreakpointType, BreakpointAction>
            {
                {BreakpointType.InputRequired, BreakpointAction.Halt}
            });

        while (!machine.Finished)
        {
            InstructionInfo? instructionInfo = machine.processInstruction();
            if (instructionInfo.BreakpointType != BreakpointType.None)
            {
                break;
            }
        }

        string? actualOutput = staticIO.GetOutput(true);
        string? identicalOutput = staticIO.GetOutput();
        string? emptyOutput = staticIO.GetOutput();

        Assert.AreEqual(386U, machine.InstructionCounter);
        Assert.AreEqual(this.Screens[nameof(TestFirstScreen)], actualOutput);
        Assert.AreEqual(this.Screens[nameof(TestFirstScreen)], identicalOutput);
        Assert.AreEqual("", emptyOutput);
    }

    [TestMethod]
    public void TestQuit()
    {
        StaticIO? staticIO = new StaticIO("quit\nY\n");
        Machine? machine = new Machine(
            staticIO,
            ZorkPath,
            new Dictionary<BreakpointType, BreakpointAction>());

        while (!machine.Finished)
        {
            InstructionInfo? instructionInfo = machine.processInstruction();
            if (instructionInfo.BreakpointType != BreakpointType.None)
            {
                Assert.AreEqual(BreakpointType.Complete, instructionInfo.BreakpointType);
                break;
            }
        }

        string? transcript = staticIO.GetOutput().Substring(this.Screens[nameof(TestFirstScreen)].Length);
        Assert.AreEqual(
            this.Screens[nameof(TestQuit)],
            transcript);
        Assert.AreEqual(BreakpointType.Complete, machine.BreakpointsReached.Last().breakpointType);
        Assert.IsTrue(machine.Finished);
    }

    [TestMethod]
    public void TestScore()
    {
        StaticIO? staticIO = new StaticIO("score\n");
        Machine? machine = new Machine(
            staticIO,
            ZorkPath,
            new Dictionary<BreakpointType, BreakpointAction>
            {
                {BreakpointType.InputRequired, BreakpointAction.Halt}
            })
        {
            // skip the first screen
            BreakAfter = 387
        };

        while (!machine.Finished)
        {
            InstructionInfo? instructionInfo = machine.processInstruction();
            if (instructionInfo.BreakpointType != BreakpointType.None)
            {
                Assert.AreEqual(BreakpointType.InputRequired, instructionInfo.BreakpointType);
                break;
            }
        }

        string? scoreScreen = staticIO.GetOutput().Substring(this.Screens[nameof(TestFirstScreen)].Length);
        Assert.AreEqual(
            this.Screens[nameof(TestScore)],
            scoreScreen);
    }

    [TestMethod]
    public void TestSaveRestore()
    {
        StaticIO? staticIO = new StaticIO("south\nsave\nnorth\nrestore\nopen mailbox\n");
        Machine? machine = new Machine(
            staticIO,
            ZorkPath,
            new Dictionary<BreakpointType, BreakpointAction>
            {
                //{ BreakpointType.Opcode, BreakpointAction.Continue }
            });
        {
        }
        ;
        machine.OpcodeBreakpoints.Add(NoOperandOpcode.op_save);
        machine.OpcodeBreakpoints.Add(NoOperandOpcode.op_show_status);

        List<(InstructionInfo instructionInfo, string output)>? stepTranscripts = new List<(InstructionInfo instructionInfo, string output)>();
        while (!machine.Finished)
        {
            ulong instruction = machine.InstructionCounter;
            InstructionInfo? instructionInfo = machine.processInstruction();
            string transcript = staticIO.GetOutput();
            if (!string.IsNullOrEmpty(transcript))
            {
                stepTranscripts.Add((
                    instructionInfo,
                    output: transcript));
            }

            Assert.AreEqual(BreakpointType.None, instructionInfo.BreakpointType);
        }

        // number of total steps
        Assert.AreEqual(968U, machine.InstructionCounter);
        // number of non empty steps
        Assert.AreEqual(0, stepTranscripts.Count);
        string[]? expected = new[]
        {
            this.Screens[nameof(TestFirstScreen)],
            this.Screens[nameof(TestSaveRestore)]
        };
        for (int i = 0; i < stepTranscripts.Count(); i++)
        {
            Assert.AreEqual(
                expected[i],
                stepTranscripts[i].output);
        }
    }
}