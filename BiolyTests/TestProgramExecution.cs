using BiolyCompiler;
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

        private JSProgram CreateProgramWithIfStatement(bool[] enableIfs)
        {
            JSProgram program = new JSProgram();
            program.Render = true;
            program.AddInputBlock("k", 100, FluidUnit.drops);
            program.AddOutputDeclarationBlock("z");

            foreach (bool enableIf in enableIfs)
            {
                string left = program.AddConstantBlock(3);
                string right = program.AddConstantBlock(3);
                string conditionalBlock = program.AddBoolOPBlock(BoolOPTypes.EQ, left, right);

                string scopeName = program.GetUniqueName();
                program.AddScope(scopeName);
                program.SetScope(scopeName);
                string guardedBlock = program.AddOutputSegment("k", "z", 1, false);
                program.SetScope(JSProgram.DEFAULT_SCOPE_NAME);

                program.AddIfSegment(conditionalBlock, guardedBlock);
                program.AddOutputSegment("k", "z", 1, false);
            }

            program.Finish();
            return program;
        }
        private JSProgram CreateProgramWithoutIfStatement(bool[] enableIfs)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("k", 100, FluidUnit.drops);
            program.AddOutputDeclarationBlock("z");

            foreach (bool enableIf in enableIfs)
            {
                if (enableIf)
                {
                    program.AddOutputSegment("k", "z", 1, false);
                }
                program.AddOutputSegment("k", "z", 1, false);
            }

            program.Finish();
            return program;
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
        }

        private List<Command> GetProgramCommands(JSProgram program)
        {
            ClearWorkspace();
            TestTools.ExecuteJS(program);
            string xml = TestTools.GetWorkspaceString();
            TestCommandExecutor executor = new TestCommandExecutor();
            ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(executor);
            programExecutor.Run(100, 100, xml);

            return executor.Commands;
        }

        [TestMethod]
        public void ProgramWithEnabledIfStatement()
        {

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
