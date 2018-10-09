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

            edges = CreateEdgesBetweenDFGs(cdfg.StartDFG, cdfg, edges, dfgNames, new List<(DFG<Block>, DFG<Block>)>(), new Stack<DFG<Block>>());

            nodes = "[" + nodes + "]";
            edges = "[" + edges + "]";

            return (nodes, edges);
        }

        private static Dictionary<DFG<Block>, string> CreateNodesAndEdgesForEachDFGInCDFG(CDFG cdfg, ref string nodes, ref string edges)
        {
            var dfgNames = new Dictionary<DFG<Block>, string>();
            int dfgNameNumber = 0;
            int nodeNameID = 0;
            foreach (var node in cdfg.Nodes)
            {
                Dictionary<Node<Block>, string> nodeNamer = new Dictionary<Node<Block>, string>();
                foreach (Node<Block> nodeBlock in node.dfg.Nodes)
                {
                    nodeNamer.Add(nodeBlock, nodeNameID++.ToString());
                }
                string dfgName = $"G{dfgNameNumber}";
                var simpleGraph = DFGToSimpleGraph(node.dfg, dfgName, nodeNamer);
                nodes += simpleGraph.nodes;
                edges += simpleGraph.edges;

                dfgNames.Add(node.dfg, dfgName);
                dfgNameNumber++;
            }

            return dfgNames;
        }

        private static (string nodes, string edges) DFGToSimpleGraph(DFG<Block> dfg, string dfgName, Dictionary<Node<Block>, string> nodeNamer)
        {
            string nodes = "";
            string edges = "";

            nodes += CreateNode(dfgName, String.Empty);

            foreach (Node<Block> node in dfg.Nodes)
            {
                nodes += CreateNode(nodeNamer[node], node.value.ToString(), dfgName);

                foreach (Node<Block> edgeNode in node.getOutgoingEdges())
                {
                    edges += CreateEdge(nodeNamer[node], nodeNamer[edgeNode]);
                }
            }

            nodes += CreateNode(dfgName + "-input", String.Empty, dfgName, "hidden");
            nodes += CreateNode(dfgName + "-output", String.Empty, dfgName, "hidden");
            foreach (var node in dfg.Nodes.Where(x => x.GetIngoingEdges().Count == 0))
            {
                edges += CreateEdge(dfgName + "-input", nodeNamer[node], "haystack");
            }
            foreach (var node in dfg.Nodes.Where(x => x.getOutgoingEdges().Count == 0))
            {
                edges += CreateEdge(nodeNamer[node], dfgName + "-output", "haystack");
            }

            return (nodes, edges);
        }

        private static string CreateEdgesBetweenDFGs(DFG<Block> dfg, CDFG cdfg, string edges, Dictionary<DFG<Block>, string> dfgNames, List<(DFG<Block>, DFG<Block>)> edgesAlreadyCreated, Stack<DFG<Block>> nextDFGStack)
        {
            var node = cdfg.Nodes.Single(x => x.dfg == dfg);
            IControlBlock control = node.control;

            //edge from control to after control
            if (control == null && nextDFGStack.Count > 0)
            {
                DFG<Block> nextDFG = nextDFGStack.Peek();
                edges += CreateEdge(dfgNames[dfg], dfgNames[nextDFG]);
                edges += CreateHiddenRankEdgesBetweenDFGs(dfg, nextDFG, dfgNames);
            }
            else if (control is If)
            {
                foreach (Conditional conditional in (control as If).IfStatements)
                {
                    if (conditional.NextDFG != null)
                    {
                        nextDFGStack.Push(conditional.NextDFG);
                    }
                    edges = CreateEdgesBetweenDFGs(conditional.GuardedDFG, cdfg, edges, dfgNames, edgesAlreadyCreated, nextDFGStack);
                    if (conditional.NextDFG != null)
                    {
                        nextDFGStack.Pop();
                        edges = CreateEdgesBetweenDFGs(conditional.NextDFG, cdfg, edges, dfgNames, edgesAlreadyCreated, nextDFGStack);
                    }
                    edges = CreateConditionalEdges(edges, dfgNames, node, conditional, edgesAlreadyCreated, nextDFGStack);
                }
            }
            else if (control is Repeat)
            {
                Conditional conditional = (control as Repeat).Cond;
                if (conditional.NextDFG != null)
                {
                    nextDFGStack.Push(conditional.NextDFG);
                }
                edges = CreateEdgesBetweenDFGs(conditional.GuardedDFG, cdfg, edges, dfgNames, edgesAlreadyCreated, nextDFGStack);
                if (conditional.NextDFG != null)
                {
                    nextDFGStack.Pop();
                    edges = CreateEdgesBetweenDFGs(conditional.NextDFG, cdfg, edges, dfgNames, edgesAlreadyCreated, nextDFGStack);
                }
                edges = CreateConditionalEdges(edges, dfgNames, node, conditional, edgesAlreadyCreated, nextDFGStack);
            }
            else if (control is Direct)
            {
                Conditional conditional = (control as Direct).Cond;
                if (conditional.NextDFG != null)
                {
                    edges = CreateEdgesBetweenDFGs(conditional.NextDFG, cdfg, edges, dfgNames, edgesAlreadyCreated, nextDFGStack);
                }
                edges = CreateConditionalEdges(edges, dfgNames, node, conditional, edgesAlreadyCreated, nextDFGStack);
            }
            else if (control is While)
            {
                Conditional conditional = (control as While).Cond;
                if (conditional.NextDFG != null)
                {
                    nextDFGStack.Push(conditional.NextDFG);
                }
                edges = CreateEdgesBetweenDFGs(conditional.GuardedDFG, cdfg, edges, dfgNames, edgesAlreadyCreated, nextDFGStack);
                if (conditional.NextDFG != null)
                {
                    nextDFGStack.Pop();
                    edges = CreateEdgesBetweenDFGs(conditional.NextDFG, cdfg, edges, dfgNames, edgesAlreadyCreated, nextDFGStack);
                }
                edges = CreateConditionalEdges(edges, dfgNames, node, conditional, edgesAlreadyCreated, nextDFGStack);
            }
            else if (control != null)
            {
                throw new Exception("Unknown Conditional type.");
            }

            return edges;
        }

        private static string CreateConditionalEdges(string edges, Dictionary<DFG<Block>, string> dfgNames, (IControlBlock control, DFG<Block> dfg) node, Conditional conditional, List<(DFG<Block>, DFG<Block>)> edgesAlreadyCreated, Stack<DFG<Block>> nextDFGStack)
        {
            //edge from before if to into if
            if (conditional.GuardedDFG != null)
            {
                edges += CreateEdge(dfgNames[node.dfg], dfgNames[conditional.GuardedDFG]);
                edges += CreateHiddenRankEdgesBetweenDFGs(node.dfg, conditional.GuardedDFG, dfgNames);
            }
            if (conditional.NextDFG != null && !edgesAlreadyCreated.Any(x => x.Item1 == node.dfg && x.Item2 == conditional.NextDFG))
            {
                //edge from before if to after if
                edges += CreateEdge(dfgNames[node.dfg], dfgNames[conditional.NextDFG]);
                edges += CreateHiddenRankEdgesBetweenDFGs(node.dfg, conditional.NextDFG, dfgNames);
                edgesAlreadyCreated.Add((node.dfg, conditional.NextDFG));
            }
            //edge from control to after control
            if (conditional.NextDFG == null && nextDFGStack.Count > 0)
            {
                DFG<Block> nextDFG = nextDFGStack.Peek();
                if (!edgesAlreadyCreated.Any(x => x.Item1 == node.dfg && x.Item2 == nextDFG))
                {
                    edges += CreateEdge(dfgNames[node.dfg], dfgNames[nextDFG]);
                    edges += CreateHiddenRankEdgesBetweenDFGs(node.dfg, nextDFG, dfgNames);
                    edgesAlreadyCreated.Add((node.dfg, nextDFG));
                }
            }

            return edges;
        }

        private static string CreateHiddenRankEdgesBetweenDFGs(DFG<Block> source, DFG<Block> target, Dictionary<DFG<Block>, string> dfgNames)
        {
            return CreateEdge(dfgNames[source] + "-output", dfgNames[target] + "-input");
        }

        private static string CreateNode(string id, string label, string parent = null, string classTarget = null)
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
