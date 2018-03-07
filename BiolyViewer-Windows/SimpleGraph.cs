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

            var dfgNames = new Dictionary<DFG<BiolyCompiler.BlocklyParts.Block>, string>();
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

            foreach (var node in cdfg.Nodes)
            {
                IControlBlock control = node.control;
                if (control is If)
                {
                    foreach (Conditional conditional in (control as If).IfStatements)
                    {
                        edges += CreateEdge(dfgNames[node.dfg], dfgNames[conditional.GuardedDFG]);
                        if (conditional.NextDFG != null)
                        {
                            edges += CreateEdge(dfgNames[node.dfg], dfgNames[conditional.NextDFG]);
                            edges += CreateEdge(dfgNames[conditional.GuardedDFG], dfgNames[conditional.NextDFG]);
                        }
                    }
                }
                else if (control is Repeat)
                {
                    Conditional conditional = (control as Repeat).Cond;
                    edges += CreateEdge(dfgNames[node.dfg], dfgNames[conditional.GuardedDFG]);
                    if (conditional.NextDFG != null)
                    {
                        edges += CreateEdge(dfgNames[node.dfg], dfgNames[conditional.NextDFG]);
                        edges += CreateEdge(dfgNames[conditional.GuardedDFG], dfgNames[conditional.NextDFG]);
                    }
                }
            }

            nodes = "[" + nodes + "]";
            edges = "[" + edges + "]";

            return (nodes, edges);
        }

        private static (string nodes, string edges) DFGToSimpleGraph(DFG<BiolyCompiler.BlocklyParts.Block> dfg, string dfgName)
        {
            string nodes = "";
            string edges = "";

            nodes += CreateNode(dfgName, String.Empty);

            foreach (Node<BiolyCompiler.BlocklyParts.Block> node in dfg.Nodes)
            {
                nodes += CreateNode(node.value.OutputVariable, node.value.ToString(), dfgName);

                foreach (Node<BiolyCompiler.BlocklyParts.Block> edgeNode in node.Edges)
                {
                    edges += CreateEdge(node.value.OutputVariable, edgeNode.value.OutputVariable);
                }
            }

            return (nodes, edges);
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

        private static string CreateEdge(string source, string target)
        {
            return "{ data: { source: '" + source + "', target: '" + target + "' } },";
        }
    }
}
