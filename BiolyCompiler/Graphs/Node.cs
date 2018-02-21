using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class Node<N>
    {
        private List<Node<N>> edges = new List<Node<N>>();
        public N value;

        public void AddEdge(Node<N> target)
        {
            edges.Add(target);
        }
    }
}