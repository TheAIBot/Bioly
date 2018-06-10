using BiolyCompiler.Graphs;
using BiolyCompiler.BlocklyParts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiolyCompiler.BlocklyParts.ControlFlow;

namespace BiolyViewer_Windows
{
    internal static class SimpleGraph
    {
        internal static (string nodes, string edges) CDFGToSimpleGraph(CDFG cdfg)
        {
            string nodes = "";
            string edges = "";
            Dictionary<DFG<Block>, string> dfgNames = CreateNodesAndEdgesForEachDFGInCDFG(cdfg, ref nodes, ref edges);

            edges = CreateEdgesBetweenDFGs(cdfg, edges, dfgNames);

            nodes = "[" + nodes + "]";
            edges = "[" + edges + "]";

            return (nodes, edges);
        }

        private static Dictionary<DFG<Block>, string> CreateNodesAndEdgesForEachDFGInCDFG(CDFG cdfg, ref string nodes, ref string edges)
        {
            var dfgNames = new Dictionary<DFG<Block>, string>();
            int dfgNameNumber = 0;
            foreach (var node in cdfg.Nodes)
            {
                string dfgName = $"G{dfgNameNumber}";
                var simpleGraph = DFGToSimpleGraph(node.dfg, dfgName);
                nodes += simpleGraph.nodes;
                edges += simpleGraph.edges;

                dfgNames.Add(node.dfg, dfgName);
                dfgNameNumber++;
            }

            return dfgNames;
        }

        private static (string nodes, string edges) DFGToSimpleGraph(DFG<Block> dfg, string dfgName)
        {
            string nodes = "";
            string edges = "";

            nodes += CreateNode(dfgName, String.Empty);

            foreach (Node<Block> node in dfg.Nodes)
            {
                nodes += CreateNode(node.value.OutputVariable, node.value.ToString(), dfgName);

                foreach (Node<Block> edgeNode in node.getOutgoingEdges())
                {
                    if (edgeNode.value is VariableBlock)
                    {
                        edges += CreateEdge(node.value.OutputVariable, edgeNode.value.OutputVariable);
                    }
                    else if (edgeNode.value is FluidBlock fluidBlock)
                    {
                        edges += CreateEdge(node.value.OutputVariable, edgeNode.value.OutputVariable, null, fluidBlock.InputVariables.First(x => x.FluidName == node.value.OutputVariable).ToString());
                    }
                }
            }

            return (nodes, edges);
        }

        private static string CreateEdgesBetweenDFGs(CDFG cdfg, string edges, Dictionary<DFG<Block>, string> dfgNames)
        {
            foreach (var node in cdfg.Nodes)
            {
                IControlBlock control = node.control;
                if (control is If)
                {
                    foreach (Conditional conditional in (control as If).IfStatements)
                    {
                        edges = CreateConditionalEdges(edges, dfgNames, node, conditional, edgesAlreadyCreated);
                    }
                }
                else if (control is Repeat)
                {
                    Conditional conditional = (control as Repeat).Cond;
                    edges = CreateConditionalEdges(edges, dfgNames, node, conditional, edgesAlreadyCreated);
                }
                else if (control is Direct)
                {
                    Conditional conditional = (control as Direct).Cond;
                    edges = CreateConditionalEdges(edges, dfgNames, node, conditional, edgesAlreadyCreated);
                }
                else if (control is While)
                {
                    Conditional conditional = (control as While).Cond;
                    edges = CreateConditionalEdges(edges, dfgNames, node, conditional, edgesAlreadyCreated);
                }
                else if (control != null)
                {
                    throw new Exception("Unknown Conditional type.");
                }
            }

            return edges;
        }

        private static string CreateConditionalEdges(string edges, Dictionary<DFG<Block>, string> dfgNames, (IControlBlock control, DFG<Block> dfg) node, Conditional conditional, List<(DFG<Block>, DFG<Block>)> edgesAlreadyCreated)
        {
            //edge from before if to into if
            if (conditional.GuardedDFG != null)
            {
                edges += CreateEdge(dfgNames[node.dfg], dfgNames[conditional.GuardedDFG]);
                edges += CreateHiddenRankEdgesBetweenDFGs(node.dfg, conditional.GuardedDFG);
            }
            if (conditional.NextDFG != null)
            {
                if (!edgesAlreadyCreated.Any(x => x.Item1 == node.dfg && x.Item2 == conditional.NextDFG))
                {
                    //edge from before if to after if
                    edges += CreateEdge(dfgNames[node.dfg], dfgNames[conditional.NextDFG]);
                    edges += CreateHiddenRankEdgesBetweenDFGs(node.dfg, conditional.NextDFG);
                    edgesAlreadyCreated.Add((node.dfg, conditional.NextDFG));
                }
                //edge from inside if to after if
                if (conditional.GuardedDFG != null)
                {
                    edges += CreateEdge(dfgNames[conditional.GuardedDFG], dfgNames[conditional.NextDFG]);
                    edges += CreateHiddenRankEdgesBetweenDFGs(conditional.GuardedDFG, conditional.NextDFG);
                }
            }

            return edges;
        }

        private static string CreateHiddenRankEdgesBetweenDFGs(DFG<Block> source, DFG<Block> target)
        {
            string edges = "";

            foreach (var output in source.Nodes.Where(x => x.getOutgoingEdges().Count == 0))
            {
                foreach (var input in target.Nodes.Where(y => y.getIngoingEdges().Count == 0))
                {
                    edges += CreateEdge(output.value.OutputVariable, input.value.OutputVariable, "haystack");
                }
            }

            return edges;
        }

        private static string CreateNode(string id, string label, string parent = null)
        {
            StringBuilder sBuilder = new StringBuilder();
            sBuilder.Append("{ data: { id: '");
            sBuilder.Append(id);
            sBuilder.Append("', label: '");
            sBuilder.Append(label.Replace("\r\n", @"\n"));
            if (parent != null)
            {
                sBuilder.Append("', parent: '");
                sBuilder.Append(parent);
            }
            sBuilder.Append("' } },");

            return sBuilder.ToString();
        }

        private static string CreateEdge(string source, string target, string classTarget = null, string edgeText = null)
        {
            StringBuilder sBuilder = new StringBuilder();
            sBuilder.Append("{ data: { source: '");
            sBuilder.Append(source);
            sBuilder.Append("', target: '");
            sBuilder.Append(target);
            if (edgeText != null)
            {
                sBuilder.Append("', label: '");
                sBuilder.Append(edgeText);
            }
            sBuilder.Append("' }");
            if (classTarget != null)
            {
                sBuilder.Append(", classes: '");
                sBuilder.Append(classTarget);
                sBuilder.Append("'");
            }
            sBuilder.Append(" },");
            return sBuilder.ToString();
        }
    }
}
