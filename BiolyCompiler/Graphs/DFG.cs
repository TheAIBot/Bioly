using BiolyCompiler.BlocklyParts;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class DFG<N>
    {
        public readonly List<Node<N>> Nodes = new List<Node<N>>();
        public readonly List<Node<N>> Input = new List<Node<N>>();
        public readonly List<Node<N>> Output = new List<Node<N>>();

        public void AddNode(Node<N> node)
        {
            Nodes.Add(node);
        }

        public void AddEdge(Node<N> source, Node<N> destination)
        {
            source.AddEdge(destination);
        }

        public void FinishDFG()
        {
            foreach (Node<N> node in Nodes)
            {
                if (node.EdgesToThis.Count == 0)
                {
                    Input.Add(node);
                }
                if (node.Edges.Count == 0 && node.value is Block && (node.value as Block).CanBeOutput)
                {
                    Output.Add(node);
                }
            }
        }
    }
}
