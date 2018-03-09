using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.ControlFlow;
using BiolyCompiler.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                foreach (Node<Block> edgeNode in node.Edges)
                {
                    edges += CreateEdge(node.value.OutputVariable, edgeNode.value.OutputVariable);
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
                        edges = CreateConditionalEdges(edges, dfgNames, node, conditional);
                    }
                }
                else if (control is Repeat)
                {
                    Conditional conditional = (control as Repeat).Cond;
                    edges = CreateConditionalEdges(edges, dfgNames, node, conditional);
                }
                else if (control != null)
                {
                    throw new Exception("Unknown Conditional type.");
                }
            }

            return edges;
        }

        private static string CreateConditionalEdges(string edges, Dictionary<DFG<Block>, string> dfgNames, (IControlBlock control, DFG<Block> dfg) node, Conditional conditional)
        {
            //edge from before if to into if
            edges += CreateEdge(dfgNames[node.dfg], dfgNames[conditional.GuardedDFG]);
            edges += CreateHiddenRankEdgesBetweenDFGs(node.dfg, conditional.GuardedDFG);
            if (conditional.NextDFG != null)
            {
                //edge from before if to after if
                edges += CreateEdge(dfgNames[node.dfg], dfgNames[conditional.NextDFG]);
                edges += CreateHiddenRankEdgesBetweenDFGs(node.dfg, conditional.NextDFG);
                //edge from inside if to after if
                edges += CreateEdge(dfgNames[conditional.GuardedDFG], dfgNames[conditional.NextDFG]);
                edges += CreateHiddenRankEdgesBetweenDFGs(conditional.GuardedDFG, conditional.NextDFG);
            }

            return edges;
        }

        private static string CreateHiddenRankEdgesBetweenDFGs(DFG<Block> source, DFG<Block> target)
        {
            string edges = "";
            source.Nodes.Where(x => source.Nodes.All(y => !y.value.InputVariables.Contains(x.value.OutputVariable)))
                        .ToList()
                        .ForEach(x => target.Input.ForEach(y => edges+= CreateEdge(x.value.OutputVariable, y.value.OutputVariable, "haystack")));
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

        private static string CreateEdge(string source, string target, string classTarget = null)
        {
            StringBuilder sBuilder = new StringBuilder();
            sBuilder.Append("{ data: { source: '");
            sBuilder.Append(source);
            sBuilder.Append("', target: '");
            sBuilder.Append(target);
            if (classTarget != null)
            {
                sBuilder.Append("'}, classes: '");
                sBuilder.Append(classTarget);
                sBuilder.Append("' },");
                return sBuilder.ToString();
            }
            sBuilder.Append("' } },");
            return sBuilder.ToString();
        }
    }
}
