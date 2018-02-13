using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class Graph<N, T>
    {
        private readonly List<Node<T>> nodes = new List<Node<T>>();

        public void AddNode(Node<T> node)
        {
            nodes.Add(node);
        }
    }
}
