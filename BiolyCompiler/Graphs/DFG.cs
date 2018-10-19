using BiolyCompiler.BlocklyParts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Scheduling;
using BiolyCompiler.Exceptions.ParserExceptions;

namespace BiolyCompiler.Graphs
{
    public class DFG<N>
    {
        public readonly List<Node<N>> Nodes = new List<Node<N>>();
        public readonly List<Node<N>> Input = new List<Node<N>>();
        public readonly List<Node<N>> Output = new List<Node<N>>();
        private readonly Dictionary<string, Node<N>> MostRecentRef = new Dictionary<string, Node<N>>();

        public void AddNode(N nodeValue)
        {
            Node<N> newNode = new Node<N>(nodeValue);
            Nodes.Add(newNode);

            if (nodeValue is Block block)
            {
                foreach (FluidInput fluidInput in block.InputFluids)
                {
                    if (MostRecentRef.ContainsKey(fluidInput.OriginalFluidName))
                    {
                        AddEdge(MostRecentRef[fluidInput.OriginalFluidName], newNode);
                    }
                }
                foreach (string inputNumber in block.InputNumbers)
                {
                    if (MostRecentRef.ContainsKey(inputNumber))
                    {
                        AddEdge(MostRecentRef[inputNumber], newNode);
                    }
                }

                if (MostRecentRef.ContainsKey(block.OutputVariable))
                {
                    MostRecentRef[block.OutputVariable] = newNode;
                }
                else
                {
                    MostRecentRef.Add(block.OutputVariable, newNode);
                }
            }
        }

        private void AddEdge(Node<N> source, Node<N> target)
        {
            source.AddOutgoingEdge(target);
            target.AddIngoingEdge(source);
        }

        public void FinishDFG()
        {
            foreach (Node<N> node in Nodes)
            {
                Block block = node.value as Block;

                if (node.GetOutgoingEdges().Count == 0 && block.CanBeOutput)
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

        public void ReplaceNode(Node<Block> toReplace, Node<Block> replaceWith)
        {

        }

        public DFG<Block> Copy()
        {
            if (this is DFG<Block> asda)
            {
                DFG<Block> copy = new DFG<Block>();
                Assay inCorrectOrder = new Assay(asda);
                foreach (Block toCopy in inCorrectOrder)
                {
                    copy.AddNode(toCopy.TrueCopy(copy));
                }

                copy.FinishDFG();
                return copy;
            }

            throw new ParseException("", "Can't copy a dfg which is not of type DFG<Block>");
        }
    }
}


