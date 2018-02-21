using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    class DFG<N>
    {
        private readonly List<Node<N>> nodes = new List<Node<N>>();
        private readonly List<Node<N>> input = new List<Node<N>>();
        private readonly List<Node<N>> output = new List<Node<N>>();

        public void AddNode(Node<N> node)
        {
            nodes.Add(node);
        }

        public void AddEdge(Node<N> source, Node<N> destination)
        {
            source.AddEdge(destination);
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
