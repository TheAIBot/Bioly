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
    public class If : IControlBlock
    {
        public const string XML_TYPE_NAME = "controls_if";
        public readonly IReadOnlyList<Conditional> IfStatements;

        public If(List<Conditional> ifStatements)
        {
            this.IfStatements = ifStatements;
        }

        public If(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            List<Conditional> conditionals = new List<Conditional>();

            int IfBlocksCount = 1;
            bool hasElse = false;

            XmlNode mutatorNode = node.TryGetNodeWithName("mutation");
            if (mutatorNode != null)
            {
                string elseifAttrib = mutatorNode.TryGetAttributeValue("elseif");
                if (elseifAttrib != null)
                {
                    IfBlocksCount += int.Parse(elseifAttrib);
                }

                hasElse = mutatorNode.TryGetAttributeValue("else") != null;
            }

            DFG<Block> nextDFG = null;
            for (int ifCounter = 0; ifCounter < IfBlocksCount; ifCounter++)
            {
                string exceptionStart = $"{ (ifCounter == 0 ? "If" : "Else if") } statement { (ifCounter == 0 ? String.Empty : $"Number {ifCounter}")}";

                VariableBlock decidingBlock = null;
                XmlNode ifNode = node.GetInnerBlockNode(GetIfFieldName(ifCounter), parserInfo, new MissingBlockException(id, $"{exceptionStart} is missing its conditional block."));
                if (ifNode != null)
                {
                    decidingBlock = (VariableBlock)XmlParser.ParseAndAddNodeToDFG(ifNode, dfg, parserInfo);
                }

                XmlNode guardedDFGNode = node.GetInnerBlockNode(GetDoFieldName(ifCounter), parserInfo, new MissingBlockException(id, $"{exceptionStart} is missing blocks to execute."));
                if (guardedDFGNode != null)
                {
                    DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedDFGNode, parserInfo);
                    nextDFG = nextDFG ?? XmlParser.ParseNextDFG(node, parserInfo);

                    conditionals.Add(new Conditional(decidingBlock, guardedDFG, nextDFG));
                }
            }

            if (hasElse)
            {
                XmlNode guardedDFGNode = node.GetInnerBlockNode("ELSE", parserInfo, new MissingBlockException(id, "Else statement is missing blocks to execute"));
                if (guardedDFGNode != null)
                {
                    DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedDFGNode, parserInfo);

                    conditionals.Add(new Conditional(null, guardedDFG, nextDFG));
                }
            }

            this.IfStatements = conditionals;
        }

        public static string GetIfFieldName(int ifCounter = 0)
        {
            return $"IF{ifCounter}";
        }

        public static string GetDoFieldName(int ifCounter = 0)
        {
            return $"DO{ifCounter}";
        }

        public DFG<Block> GuardedDFG<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            foreach (Conditional cond in IfStatements)
            {
                //deciding block is only null for the else statment
                if (cond.DecidingBlock == null)
                {
                    return cond.GuardedDFG;
                }

                bool isTrue = cond.DecidingBlock.Run(variables, executor, dropPositions) == 1f;
                if (isTrue)
                {
                    return cond.GuardedDFG;
                }
            }

            return null;
        }

        public DFG<Block> NextDFG<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return IfStatements[0].NextDFG;
        }

        public DFG<Block> TryLoop<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return null;
        }

        public IControlBlock Copy(DFG<Block> dfg, Dictionary<DFG<Block>, DFG<Block>> knownDFGCopys)
        {
            List<Conditional> statementsCopys = new List<Conditional>();
            foreach (Conditional conditional in IfStatements)
            {
                statementsCopys.Add(conditional.Copy(dfg, knownDFGCopys));
            }

            return new If(statementsCopys);
        }

        public IEnumerator<DFG<Block>> GetEnumerator()
        {
            foreach (Conditional conditional in IfStatements)
            {
                yield return conditional.GuardedDFG;
            }

            DFG<Block> lastDFG = IfStatements[IfStatements.Count - 1].NextDFG;
            if (lastDFG != null)
            {
                yield return lastDFG;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IControlBlock GetNewControlWithNewEnd(DFG<Block> dfg)
        {
            List<Conditional> withNewEnd = new List<Conditional>();
            foreach (Conditional ifs in IfStatements)
            {
                withNewEnd.Add(new Conditional(ifs.DecidingBlock, ifs.GuardedDFG, dfg));
            }

            return new If(withNewEnd);
        }

        public DFG<Block> GetEndDFG()
        {
            return IfStatements[0].NextDFG;
        }
    }
}
