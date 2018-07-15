using BiolyCompiler.BlocklyParts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.BlocklyParts.FluidicInputs;

namespace BiolyCompiler.Graphs
{
    public class DFG<N>
    {
        public readonly List<Node<N>> Nodes = new List<Node<N>>();
        public readonly List<Node<N>> Input = new List<Node<N>>();
        public readonly List<Node<N>> Output = new List<Node<N>>();

        public void AddNode(N nodeValue)
        {
            Nodes.Add(new Node<N>(nodeValue));
        }

        public void FinishDFG()
        {
            foreach (Node<N> node in Nodes)
            {
                Block block = node.value as Block;

                foreach (string nodeName in block.InputNumbers)
                {
                    Node<N> inputNode = Nodes.SingleOrDefault(x => (x.value as Block).OutputVariable == nodeName);
                    if (inputNode != null)
                    {
                        AddEdge(inputNode, node);
                    }
                }

                foreach (FluidInput nodeName in block.InputFluids)
                {
                    Node<N> inputNode = Nodes.SingleOrDefault(x => (x.value as Block).OutputVariable == nodeName.FluidName);
                    if (inputNode != null)
                    {
                        AddEdge(inputNode, node);
                    }
                }
            }

            foreach (Node<N> node in Nodes)
            {
                Block block = node.value as Block;

                if (node.getOutgoingEdges().Count == 0 && block.CanBeOutput)
                {
                    Output.Add(node);
                }
                if (node.GetIngoingEdges().Count == 0 || (block is VariableBlock && node.GetIngoingEdges().All(x => x.value is VariableBlock && !(x.value as VariableBlock).CanBeScheduled)))
                {
                    if (block is VariableBlock varBlock)
                    {
                        if (!varBlock.CanBeScheduled)
                        {
                            continue;
                        }
                    }
                    Input.Add(node);
                }
            }
        }

        private void AddEdge(Node<N> source, Node<N> target)
        {
            source.AddOutgoingEdge(target);
            target.AddIngoingEdge(source);
        }
    }
}


