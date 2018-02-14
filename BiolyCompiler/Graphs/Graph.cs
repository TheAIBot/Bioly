using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class Graph<N, E>
    {
        private readonly List<Node<N, E>> nodes = new List<Node<N, E>>();

        public void AddNode(Node<N, E> node)
        {
            nodes.Add(node);
        }
        public void AddEdge(Node<N, E> source, Node<N, E> destination)
        {

        }
    }
}
