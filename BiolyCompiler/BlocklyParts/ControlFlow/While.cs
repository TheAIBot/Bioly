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
    public class While : IControlBlock
    {
        public const string XML_TYPE_NAME = "controls_whileUntil";
        public const string WHILE_MODE_FIELD_NAME = "MODE";
        public const string SUPPORTED_MODE = "WHILE";
        public const string CONDITIONAL_BLOCK_FIELD_NAME = "BOOL";
        public const string DO_BLOCK_FIELD_NAME = "DO";
        public readonly Conditional Cond;

        public While(Conditional cond)
        {
            this.Cond = cond;
        }

        public While(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            string mode = node.GetNodeWithAttributeValue(WHILE_MODE_FIELD_NAME).InnerText;
            if (mode != SUPPORTED_MODE)
            {
                parserInfo.ParseExceptions.Add(new ParseException(id, "While block only supports while mode."));
            }


            XmlNode conditionalNode = node.GetInnerBlockNode(CONDITIONAL_BLOCK_FIELD_NAME, parserInfo, new MissingBlockException(id, "While block is missing its conditional block."));
            VariableBlock decidingBlock = null;
            if (conditionalNode != null)
            {
                decidingBlock = (VariableBlock)XmlParser.ParseAndAddNodeToDFG(conditionalNode, dfg, parserInfo);
            }

            XmlNode guardedNode = node.GetInnerBlockNode(DO_BLOCK_FIELD_NAME, parserInfo, new MissingBlockException(id, "While block is missing blocks to execute."));
            if (guardedNode != null)
            {
                DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedNode, parserInfo);
                DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, parserInfo);

                this.Cond = new Conditional(decidingBlock, guardedDFG, nextDFG);
            }
        }

        public DFG<Block> GuardedDFG<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            bool isTrue = Cond.DecidingBlock.Run(variables, executor, dropPositions) == 1f;
            if (isTrue)
            {
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
            bool isTrue = Cond.DecidingBlock.Run(variables, executor, dropPositions) == 1f;
            if (isTrue)
            {
                return Cond.GuardedDFG;
            }
            else
            {
                return null;
            }
        }

        public IControlBlock Copy(DFG<Block> dfg, Dictionary<DFG<Block>, DFG<Block>> knownDFGCopys)
        {
            return new While(Cond.Copy(dfg, knownDFGCopys));
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
            return new While(new Conditional(Cond.DecidingBlock, Cond.GuardedDFG, dfg));
        }

        public DFG<Block> GetEndDFG()
        {
            return Cond.NextDFG;
        }
    }
}
