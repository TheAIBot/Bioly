using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    class Edge<N, E>
    {
        public Node<N, E> start;
        public Node<N, E> end;
        public E value;

        public Edge(Node<N, E> sstart, Node<N, E> eend) : this(sstart, eend, default(E))
        {
        }

        public Edge(Node<N, E> sstart, Node<N, E> eend, E vvalue)
        {
            this.start = sstart;
            this.end = eend;
            this.value = vvalue;
        }
    }
}