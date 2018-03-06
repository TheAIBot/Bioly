using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class Node<N>
    {
        public readonly List<Node<N>> Edges = new List<Node<N>>();
        public readonly List<Node<N>> EdgesToThis = new List<Node<N>>();
        public N value;
        public bool EdgesCreated = false;

        public void AddEdge(Node<N> target)
        {
            Edges.Add(target);
            target.EdgesToThis.Add(this);
        }
    }
}