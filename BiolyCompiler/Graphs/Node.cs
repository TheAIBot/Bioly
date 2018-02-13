using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class Node<N, E>
    {
        private List<Edge<N, E>> edges = new List<Edge<N, E>>();

        public void AddEdge(Node<N, E> target)
        {
            edges.Add(new Edge<N, E>(this, target));
        }
    }
}