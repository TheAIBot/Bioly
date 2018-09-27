using BiolyCompiler;
using BiolyCompiler.BlocklyParts.BoolLogic;
using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
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
        [TestInitialize]
        public void ResetWorkspace() => TestTools.ResetBrowser();

        private string AddFluidBlock(JSProgram program, string from, string to)
        {
            return program.AddHeaterSegment(to, "z", 100, 1, from, 1, false);
        }

        private void nestIfs(JSProgram program, string prevScopeName, Queue<bool> enableIf, string from, string to)
        {
            string left = program.AddConstantBlock(enableIf.Dequeue() ? 3 : 2);
            string right = program.AddConstantBlock(3);
            string conditionalBlock = program.AddBoolOPBlock(BoolOPTypes.EQ, left, right);

            string scopeName = program.GetUniqueName();
            program.AddScope(scopeName);
            program.SetScope(scopeName);
            string guardedBlock = AddFluidBlock(program, from, to);
            if (enableIf.Count > 0)
            {
                nestIfs(program, scopeName, enableIf, to, to);
            }
            program.SetScope(prevScopeName);

            program.AddIfSegment(conditionalBlock, guardedBlock);
            AddFluidBlock(program, to, to);
        }

        private JSProgram CreateProgramWithIfStatement(bool[][] enableIfs)
        {
            JSProgram program = new JSProgram();
            program.Render = true;
            program.AddInputBlock("a", 1);
            program.AddHeaterDeclarationBlock("z");

            AddFluidBlock(program, "a", "k");
            foreach (bool[] enableIf in enableIfs)
            {
                nestIfs(program, JSProgram.DEFAULT_SCOPE_NAME, new Queue<bool>(enableIf), "k", "k");
            }

            program.Finish();
            return program;
        }

        private JSProgram CreateProgramWithoutIfStatement(bool[] enableIfs)
        {
            JSProgram program = new JSProgram();
            program.Render = true;
            program.AddInputBlock("a", 1);
            program.AddHeaterDeclarationBlock("z");

            AddFluidBlock(program, "a", "k");
            foreach (bool enableIf in enableIfs)
            {
                if (enableIf)
                {
                    AddFluidBlock(program, "k", "k");
                }
                AddFluidBlock(program, "k", "k");
            }

            program.Finish();
            return program;
        }

        private List<Command> GetProgramCommands(JSProgram program)
        {
            (CDFG cdfg, _) = TestTools.ParseProgram(program);
            TestCommandExecutor executor = new TestCommandExecutor();
            ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(executor);
            programExecutor.TimeBetweenCommands = 0;
            programExecutor.ShowEmptyRectangles = false;
            programExecutor.Run(10, 10, cdfg, false);

            return executor.Commands;
        }

        [TestMethod]
        public void ProgramWithDisabledIfStatement()
        {
            JSProgram program1 = CreateProgramWithIfStatement(new bool[][] { new bool[] { false } });
            JSProgram program2 = CreateProgramWithoutIfStatement(new bool[] { false });
            List<Command> program1Commands = GetProgramCommands(program1);
            List<Command> program2Commands = GetProgramCommands(program2);
            Assert.IsTrue(program1Commands.SequenceEqual(program2Commands));

            JSProgram program3 = CreateProgramWithIfStatement(new bool[][] { new bool[] { false }, new bool[] { false } });
            JSProgram program4 = CreateProgramWithoutIfStatement(new bool[] { false, false });
            List<Command> program3Commands = GetProgramCommands(program3);
            List<Command> program4Commands = GetProgramCommands(program4);
            Assert.IsTrue(program3Commands.SequenceEqual(program4Commands));

            JSProgram program5 = CreateProgramWithIfStatement(new bool[][] { new bool[] { false }, new bool[] { false }, new bool[] { false }, new bool[] { false }, new bool[] { false }, new bool[] { false } });
            JSProgram program6 = CreateProgramWithoutIfStatement(new bool[] { false, false, false, false, false, false });
            List<Command> program5Commands = GetProgramCommands(program5);
            List<Command> program6Commands = GetProgramCommands(program6);
            Assert.IsTrue(program5Commands.SequenceEqual(program6Commands));
        }

        [TestMethod]
        public void ProgramWithEnabledIfStatement()
        {
            JSProgram program1 = CreateProgramWithIfStatement(new bool[][] { new bool[] { true } });
            JSProgram program2 = CreateProgramWithoutIfStatement(new bool[] { true });
            List<Command> program1Commands = GetProgramCommands(program1);
            List<Command> program2Commands = GetProgramCommands(program2);
            Assert.IsTrue(program1Commands.SequenceEqual(program2Commands));

            JSProgram program3 = CreateProgramWithIfStatement(new bool[][] { new bool[] { true }, new bool[] { true } });
            JSProgram program4 = CreateProgramWithoutIfStatement(new bool[] { true, true });
            List<Command> program3Commands = GetProgramCommands(program3);
            List<Command> program4Commands = GetProgramCommands(program4);
            Assert.IsTrue(program3Commands.SequenceEqual(program4Commands));

            JSProgram program5 = CreateProgramWithIfStatement(new bool[][] { new bool[] { true }, new bool[] { true }, new bool[] { true }, new bool[] { true }, new bool[] { true }, new bool[] { true } });
            JSProgram program6 = CreateProgramWithoutIfStatement(new bool[] { true, true, true, true, true, true });
            List<Command> program5Commands = GetProgramCommands(program5);
            List<Command> program6Commands = GetProgramCommands(program6);
            Assert.IsTrue(program5Commands.SequenceEqual(program6Commands));
        }

        [TestMethod]
        public void ProgramWithNestedIfStatements()
        {
            JSProgram program1 = CreateProgramWithIfStatement(new bool[][] { new bool[] { false, true, true, true } });
            JSProgram program2 = CreateProgramWithoutIfStatement(new bool[] { false });
            List<Command> program1Commands = GetProgramCommands(program1);
            List<Command> program2Commands = GetProgramCommands(program2);
            Assert.IsTrue(program1Commands.SequenceEqual(program2Commands));
        }

        private void nestRepeats(JSProgram program, string prevScopeName, Queue<int> repeatTimes, string from, string to)
        {
            string times = program.AddConstantBlock(repeatTimes.Dequeue());

            string scopeName = program.GetUniqueName();
            program.AddScope(scopeName);
            program.SetScope(scopeName);
            string guardedBlock = AddFluidBlock(program, from, to);
            if (repeatTimes.Count > 0)
            {
                nestRepeats(program, scopeName, repeatTimes, to, to);
            }
            program.SetScope(prevScopeName);

            program.AddRepeatSegment(times, guardedBlock);
            AddFluidBlock(program, to, to);
        }

        private JSProgram CreateProgramWithRepeatStatement(int[][] repeatTimes)
        {
            JSProgram program = new JSProgram();
            program.Render = true;
            program.AddInputBlock("a", 1);
            program.AddHeaterDeclarationBlock("z");

            AddFluidBlock(program, "a", "k");
            foreach (int[] repeats in repeatTimes)
            {
                nestRepeats(program, JSProgram.DEFAULT_SCOPE_NAME, new Queue<int>(repeats), "k", "k");
            }

            program.Finish();
            return program;
        }

        private JSProgram CreateProgramWithoutRepeatStatement(int repeatTimes)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 1);
            program.AddHeaterDeclarationBlock("z");

            AddFluidBlock(program, "a", "k");
            for (int i = 0; i < repeatTimes; i++)
            {
                AddFluidBlock(program, "k", "k");
            }

            program.Finish();
            return program;
        }

        [TestMethod]
        public void ProgramWithDisabledRepeatStatement()
        {
            JSProgram program1 = CreateProgramWithRepeatStatement(new int[][] { new int[] { -2 } });
            JSProgram program2 = CreateProgramWithoutRepeatStatement(1);
            List<Command> program1Commands = GetProgramCommands(program1);
            List<Command> program2Commands = GetProgramCommands(program2);
            Assert.IsTrue(program1Commands.SequenceEqual(program2Commands));

            JSProgram program3 = CreateProgramWithRepeatStatement(new int[][] { new int[] { -1 }, new int[] { -2 }, new int[] { -3 } });
            JSProgram program4 = CreateProgramWithoutRepeatStatement(3);
            List<Command> program3Commands = GetProgramCommands(program3);
            List<Command> program4Commands = GetProgramCommands(program4);
            Assert.IsTrue(program3Commands.SequenceEqual(program4Commands));

            JSProgram program5 = CreateProgramWithRepeatStatement(new int[][] { new int[] { -2, -1 } });
            JSProgram program6 = CreateProgramWithoutRepeatStatement(1);
            List<Command> program5Commands = GetProgramCommands(program5);
            List<Command> program6Commands = GetProgramCommands(program6);
            Assert.IsTrue(program5Commands.SequenceEqual(program6Commands));
        }

        [TestMethod]
        public void ProgramWithEnabledRepeatStatement()
        {
            JSProgram program1 = CreateProgramWithRepeatStatement(new int[][] { new int[] { 2 } });
            JSProgram program2 = CreateProgramWithoutRepeatStatement(3);
            List<Command> program1Commands = GetProgramCommands(program1);
            List<Command> program2Commands = GetProgramCommands(program2);
            Assert.IsTrue(program1Commands.SequenceEqual(program2Commands));

            JSProgram program3 = CreateProgramWithRepeatStatement(new int[][] { new int[] { 1 }, new int[] { 2 }, new int[] { 3 } });
            JSProgram program4 = CreateProgramWithoutRepeatStatement(9);
            List<Command> program3Commands = GetProgramCommands(program3);
            List<Command> program4Commands = GetProgramCommands(program4);
            Assert.IsTrue(program3Commands.SequenceEqual(program4Commands));

            JSProgram program5 = CreateProgramWithRepeatStatement(new int[][] { new int[] { 2, -1 } });
            JSProgram program6 = CreateProgramWithoutRepeatStatement(5);
            List<Command> program5Commands = GetProgramCommands(program5);
            List<Command> program6Commands = GetProgramCommands(program6);
            Assert.IsTrue(program5Commands.SequenceEqual(program6Commands));
        }

        [TestMethod]
        public void ProgramWithNestedRepeatStatements()
        {
            JSProgram program1 = CreateProgramWithRepeatStatement(new int[][] { new int[] { 10, 1, 5 }, new int[] { -1, 10000, 10000 }, new int[] { 10, 10, 10 } });
            JSProgram program2 = CreateProgramWithoutRepeatStatement(1313);
            List<Command> program1Commands = GetProgramCommands(program1);
            List<Command> program2Commands = GetProgramCommands(program2);
            Assert.IsTrue(program1Commands.SequenceEqual(program2Commands));
        }

        private JSProgram CreateProgramWithWhileStatement(int whileTimes)
        {
            JSProgram program = new JSProgram();
            program.Render = true;
            program.AddInputBlock("a", 1);
            program.AddHeaterDeclarationBlock("z");

            string times = program.AddConstantBlock(whileTimes);
            string zero = program.AddConstantBlock(whileTimes);
            string logicCheck = program.AddBoolOPBlock(BoolOPTypes.EQ, times, zero);

            string scopeName = program.GetUniqueName();
            program.AddScope(scopeName);
            program.SetScope(scopeName);
            string guardedBlock = AddFluidBlock(program, "a", "k");
            program.SetScope(JSProgram.DEFAULT_SCOPE_NAME);

            program.AddRepeatSegment(times, guardedBlock);
            AddFluidBlock(program, "k", "k");

            program.Finish();
            return program;
        }

        public void ProgramWithDisabledWithStatement()
        {

        }
    }
}
