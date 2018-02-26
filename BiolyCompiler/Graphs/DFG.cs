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

        public void AddInput(Node<N> node)
        {
            Input.Add(node);
        }

        public void AddOutput(Node<N> node)
        {
            Output.Add(node);
        }
    }
}
