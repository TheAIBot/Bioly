using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class Node<T>
    {
        private List<Edge<T>> edges = new List<Edge<T>>();

        public void AddEdge(Node<T> target)
        {
            edges.Add(new Edge<T>(this, target));
        }
    }
}