using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.BlocklyParts.Blocks;

namespace BiolyCompiler.Graphs
{
    public class DFG<N>
    {
        public readonly List<Node<N>> nodes = new List<Node<N>>();
        public readonly List<Node<N>> input = new List<Node<N>>();
        public readonly List<Node<N>> output = new List<Node<N>>();

        public void AddNode(Node<N> node)
        {
            nodes.Add(node);
        }

        public void AddEdge(Node<N> source, Node<N> target)
        {
            source.AddOutgoingEdge(target);
            target.AddIngoingEdge(source);
        }

        public void AddInput(Node<N> node)
        {
            input.Add(node);
        }

        public void AddOutput(Node<N> node)
        {
            output.Add(node);
        }

    }

}
