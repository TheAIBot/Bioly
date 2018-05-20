﻿using BiolyCompiler;
using BiolyCompiler.BlocklyParts.BoolLogic;
using BiolyCompiler.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiolyTests
{
    [TestClass]
    public class TestProgramExecution
    {
        [TestInitialize()]
        public void ClearWorkspace() => TestTools.ClearWorkspace();

        private string AddFluidBlock(JSProgram program)
        {
            string newFluidName = program.GetUniqueName();
            string block = program.AddFluidSegment("k", "k", 0, true);
            return block;
        }

        private JSProgram CreateProgramWithIfStatement(bool[] enableIfs)
        {
            JSProgram program = new JSProgram();
            program.Render = true;
            program.AddInputBlock("k", 1, FluidUnit.drops);
            AddFluidBlock(program);
            

            foreach (bool enableIf in enableIfs)
            {
                string left = program.AddConstantBlock(enableIf ? 3 : 2);
                string right = program.AddConstantBlock(3);
                string conditionalBlock = program.AddBoolOPBlock(BoolOPTypes.EQ, left, right);

                string scopeName = program.GetUniqueName();
                program.AddScope(scopeName);
                program.SetScope(scopeName);
                string guardedBlock = AddFluidBlock(program);
                program.SetScope(JSProgram.DEFAULT_SCOPE_NAME);

                program.AddIfSegment(conditionalBlock, guardedBlock);
                AddFluidBlock(program);
            }

            program.Finish();
            return program;
        }
        private JSProgram CreateProgramWithoutIfStatement(bool[] enableIfs)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("k", 100, FluidUnit.drops);

            foreach (bool enableIf in enableIfs)
            {
                if (enableIf)
                {
                    AddFluidBlock(program);
                }
                AddFluidBlock(program);
            }

            program.Finish();
            return program;
        }

        private List<Command> GetProgramCommands(JSProgram program)
        {
            ClearWorkspace();
            TestTools.ExecuteJS(program);
            string xml = TestTools.GetWorkspaceString();
            TestCommandExecutor executor = new TestCommandExecutor();
            ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(executor);
            programExecutor.Run(10, 10, xml);

            return executor.Commands;
        }

        [TestMethod]
        public void ProgramWithDisabledIfStatement()
        {
            JSProgram program1 = CreateProgramWithIfStatement(new bool[] { false });
            JSProgram program2 = CreateProgramWithoutIfStatement(new bool[] { false });
            List<Command> program1Commands = GetProgramCommands(program1);
            List<Command> program2Commands = GetProgramCommands(program2);
            Assert.IsTrue(program1Commands.SequenceEqual(program2Commands));

            //JSProgram program3 = CreateProgramWithIfStatement(new bool[] { false, false });
            //JSProgram program4 = CreateProgramWithoutIfStatement(new bool[] { false, false });
            //List<Command> program3Commands = GetProgramCommands(program3);
            //List<Command> program4Commands = GetProgramCommands(program4);
            //Assert.IsTrue(program3Commands.SequenceEqual(program4Commands));

            //JSProgram program5 = CreateProgramWithIfStatement(new bool[] { false, false, false, false, false, false });
            //JSProgram program6 = CreateProgramWithoutIfStatement(new bool[] { false, false, false, false, false, false });
            //List<Command> program5Commands = GetProgramCommands(program5);
            //List<Command> program6Commands = GetProgramCommands(program6);
            //Assert.IsTrue(program5Commands.SequenceEqual(program6Commands));
        }

        [TestMethod]
        public void ProgramWithEnabledIfStatement()
        {
            JSProgram program1 = CreateProgramWithIfStatement(new bool[] { true });
            JSProgram program2 = CreateProgramWithoutIfStatement(new bool[] { true });
            List<Command> program1Commands = GetProgramCommands(program1);
            List<Command> program2Commands = GetProgramCommands(program2);
            Assert.IsTrue(program1Commands.SequenceEqual(program2Commands));

            JSProgram program3 = CreateProgramWithIfStatement(new bool[] { true, true });
            JSProgram program4 = CreateProgramWithoutIfStatement(new bool[] { true, true });
            List<Command> program3Commands = GetProgramCommands(program3);
            List<Command> program4Commands = GetProgramCommands(program4);
            Assert.IsTrue(program3Commands.SequenceEqual(program4Commands));

            JSProgram program5 = CreateProgramWithIfStatement(new bool[] { true, true, true, true, true, true });
            JSProgram program6 = CreateProgramWithoutIfStatement(new bool[] { true, true, true, true, true, true });
            List<Command> program5Commands = GetProgramCommands(program5);
            List<Command> program6Commands = GetProgramCommands(program6);
            Assert.IsTrue(program5Commands.SequenceEqual(program6Commands));
        }

        [TestMethod]
        public void ProgramWithNestedIfStatements()
        {

        }

        [TestMethod]
        public void ProgramWithDisabledRepeatStatement()
        {

        }

        [TestMethod]
        public void ProgramWithEnabledRepeatStatement()
        {

        }

        [TestMethod]
        public void ProgramWithNestedRepeatStatements()
        {

        }

        [TestMethod]
        public void ProgramWithRepeatsAndIfStatements()
        {

        }
    }
}
