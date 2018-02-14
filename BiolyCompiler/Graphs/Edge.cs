using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    class Edge<N, E>
    {
        public Node<N, E> source;
        public Node<N, E> destination;
        public E value;

        public Edge(Node<N, E> src, Node<N, E> dst) : this(src, dst, default(E))
        {
        }

        public Edge(Node<N, E> src, Node<N, E> dst, E vvalue)
        {
            this.source = src;
            this.destination = dst;
            this.value = vvalue;
        }
    }
}