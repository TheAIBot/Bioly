using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class Node<N>
    {
        private readonly List<Node<N>> OutgoingEdges = new List<Node<N>>();
        private readonly List<Node<N>> IngoingEdges = new List<Node<N>>();
        public N value { get; private set; }

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

        public List<Node<N>> GetOutgoingEdges()
        {
            return OutgoingEdges;
        }

        public List<Node<N>> GetIngoingEdges()
        {
            return IngoingEdges;
        }

        public void ReplaceValue(N newValue)
        {
            value = newValue;
        }

        public void RemoveIngoingEdges()
        {
            foreach (Node<N> ingoingEdge in IngoingEdges)
            {
                ingoingEdge.OutgoingEdges.Remove(this);
                OutgoingEdges.Remove(ingoingEdge);
            }
        }
    }
}