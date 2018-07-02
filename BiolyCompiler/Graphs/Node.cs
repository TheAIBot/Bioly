using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class Node<N>
    {
        private List<Node<N>> OutgoingEdges = new List<Node<N>>();
        private List<Node<N>> IngoingEdges = new List<Node<N>>();
        public readonly N value;

        public Node(N value)
        {
            this.value = value;
        }

        public void AddOutgoingEdge(Node<N> target)
        {
            OutgoingEdges.Add(target);
        }

        public void AddIngoingEdge(Node<N> source)
        {
            IngoingEdges.Add(source);
        }

        public List<Node<N>> GetIngoingEdges()
        {
            return IngoingEdges;
        }


        public List<Node<N>> getOutgoingEdges()
        {
            return OutgoingEdges;
        }

        
        public void InvertEdges()
        {
            var temp = OutgoingEdges;
            OutgoingEdges = IngoingEdges;
            IngoingEdges = temp;
        }
    }
}