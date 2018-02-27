using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class Node<N>
    {
        private List<Node<N>> outgoingEdges = new List<Node<N>>();
        private List<Node<N>> ingoingEdges = new List<Node<N>>();
        public N value;

        public void AddOutgoingEdge(Node<N> target)
        {
            outgoingEdges.Add(target);
        }

        public void AddIngoingEdge(Node<N> source)
        {
            ingoingEdges.Add(source);
        }

        public List<Node<N>> getIngoingEdges()
        {
            return ingoingEdges;
        }


        public List<Node<N>> getOutgoingEdges()
        {
            return ingoingEdges;
        }

    }
}