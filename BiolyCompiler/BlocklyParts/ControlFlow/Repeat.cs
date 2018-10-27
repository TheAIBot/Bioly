using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.ControlFlow
{
    public class Repeat : IControlBlock
    {
        public const string XML_TYPE_NAME = "controls_repeat_ext";
        public const string TimesBlockFieldName = "TIMES";
        public const string DoBlockFieldName = "DO";
        public readonly Conditional Cond;
        private int LoopCounter = 0;

        public Repeat(Conditional cond)
        {
            this.Cond = cond;
        }

        public Repeat(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            XmlNode conditionalNode = node.GetInnerBlockNode(TimesBlockFieldName, parserInfo, new MissingBlockException(id, "Repeat block is missing its conditional block."));
            VariableBlock decidingBlock = null;
            if (conditionalNode != null)
            {
                decidingBlock = (VariableBlock)XmlParser.ParseAndAddNodeToDFG(ref conditionalNode, dfg, parserInfo);
            }

            XmlNode guardedNode = node.GetInnerBlockNode(DoBlockFieldName, parserInfo, new MissingBlockException(id, "Repeat block is missing blocks to execute."));
            if (guardedNode != null)
            {
                DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedNode, parserInfo);
                DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, parserInfo);

                this.Cond = new Conditional(decidingBlock, guardedDFG, nextDFG);
            }
        }

        public DFG<Block> GuardedDFG<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            this.LoopCounter = (int)Cond.DecidingBlock.Run(variables, executor, dropPositions);
            if (LoopCounter > 0)
            {
                LoopCounter--;
                return Cond.GuardedDFG;
            }
            else
            {
                return null;
            }
        }

        public DFG<Block> NextDFG<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return Cond.NextDFG;
        }

        public DFG<Block> TryLoop<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            if (LoopCounter > 0)
            {
                LoopCounter--;
                return Cond.GuardedDFG;
            }
            else
            {
                return null;
            }
        }

        public IControlBlock Copy(DFG<Block> dfg, Dictionary<DFG<Block>, DFG<Block>> knownDFGCopys)
        {
            return new Repeat(Cond.Copy(dfg, knownDFGCopys));
        }

        public IEnumerator<DFG<Block>> GetEnumerator()
        {
            yield return Cond.GuardedDFG;
            if (Cond.NextDFG != null)
            {
                yield return Cond.NextDFG;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IControlBlock GetNewControlWithNewEnd(DFG<Block> dfg)
        {
            return new Repeat(new Conditional(Cond.DecidingBlock, Cond.GuardedDFG, dfg));
        }

        public DFG<Block> GetEndDFG()
        {
            return Cond.NextDFG;
        }
    }
}
