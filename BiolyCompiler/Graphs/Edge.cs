using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    class Edge<T>
    {
        public Node<T> start;
        public Node<T> end;

        public Edge(Node<T> sstart, Node<T> eend)
        {
            this.start = sstart;
            this.end = eend;
        }
    }
}
