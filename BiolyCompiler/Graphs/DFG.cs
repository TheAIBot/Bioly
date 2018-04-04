using BiolyCompiler.BlocklyParts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BiolyCompiler.BlocklyParts.Misc;

namespace BiolyCompiler.Graphs
{
    public class DFG<N>
    {
        public readonly List<Node<N>> Nodes = new List<Node<N>>();
        public readonly List<Node<N>> Input = new List<Node<N>>();
        public readonly List<Node<N>> Output = new List<Node<N>>();

        public void AddNode(Node<N> node)
        {
            Nodes.Add(node);
        }

        public void AddEdge(Node<N> source, Node<N> target)
        {
            source.AddOutgoingEdge(target);
            target.AddIngoingEdge(source);
        }

        public void FinishDFG()
        {
            foreach (Node<N> node in Nodes)
            {
                if (node.getIngoingEdges().Count == 0)
                {
                    Input.Add(node);
                }
                if (node.value is Block)
                {
                    Block block = node.value as Block;
                    if (node.getOutgoingEdges().Count == 0 && block.CanBeOutput)
                    {
                        Output.Add(node);
                    }

                    if (block is VariableBlock varBlock)
                    {
                        foreach (string nodeName in varBlock.InputVariables)
                        {
                            Node<N> inputNode = Nodes.SingleOrDefault(x => (x.value as Block).OutputVariable == nodeName);
                            if (inputNode != null)
                            {
                                AddEdge(inputNode, node);
                            }
                        }
                    }
                    else if (block is FluidBlock fluidBlock)
                    {
                        foreach (FluidInput nodeName in fluidBlock.InputVariables)
                        {
                            Node<N> inputNode = Nodes.SingleOrDefault(x => (x.value as Block).OutputVariable == nodeName.FluidName);
                            if (inputNode != null)
                            {
                                AddEdge(inputNode, node);
                            }
                        }
                    }
                }
            }
        }

    }

}
