using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.BlocklyParts.Sensors;
using BiolyCompiler.Graphs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;

namespace BiolyCompiler.Parser
{
    public static class XMLParser
    {
        public static CDFG Parse(string xmlText)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlText);

            var cdfg = new CDFG();

            var currentNode = xmlDocument.FirstChild.FirstChild.FirstChild.FirstChild;


            return cdfg;
        }

        private static DFG<Block> ParseDFG(XmlNode node)
        {
            var dfg = new DFG<Block>();
            var mostReventRef = new Dictionary<string, Node<Block>>();
            while (true)
            {
                Block block = ParseBlock(node, dfg);
                Node<Block> dfgNode = new Node<Block>();
                dfgNode.value = block;

                //add nessesary edges to this new node
                foreach (string inputNodeName in block.InputVariables)
                {
                    if (mostReventRef.TryGetValue(inputNodeName, out Node<Block> inputNode))
                    {
                        dfg.AddEdge(inputNode, dfgNode);
                    }
                }

                //update map of most recent nodes that outputs the variable
                //so other nodes that get their value from the node that
                //just updated the value
                mostReventRef.Add(block.OutputVariable, dfgNode);

                //move on to the next node or exit if none
                node = XmlTools.GetNodeWithName(node, "next");
                if (node == null)
                {
                    break;
                }
                node = node.FirstChild;

                if (IsConditional(node))
                {
                    //do something about them
                    continue;
                }
            }

            return dfg;
        }

        private static void ParseConditionalBlocks(XmlNode node, DFG<Block> dfg)
        {

        }            

        private static bool IsConditional(XmlNode node)
        {
            return node.GetNodeWithName("statement") != null;
        }

        internal static Block ParseBlock(XmlNode node, DFG<Block> dfg)
        {
            string blockType = node.Attributes["type"].Value;
            switch (blockType)
            {
                //case ArithOP.XmlTypeName:
                //    return ArithOP.Parse(node, dfg);
                //case Constant.XmlTypeName:
                //    return Constant.Parse(node);
                //case FluidArray.XmlTypeName:
                //    return FluidArray.Parse(node);
                //case SetFluidArray.XmlTypeName:
                //    return SetFluidArray.Parse(node);
                case Fluid.XmlTypeName:
                    return Fluid.Parse(node);
                case Input.XmlTypeName:
                    return Input.Parse(node);
                //case Output.XmlTypeName:
                //    return Output.Parse(node);
                //case Waste.XmlTypeName:
                //    return Waste.Parse(node);
                //case Bool.XmlTypeName:
                //    return Bool.Parse(node);
                //case BoolOP.XmlTypeName:
                //    return BoolOP.Parse(node, dfg);
                //case Sensor.XmlTypeName:
                //    return Sensor.Parse(node);
                default:
                    throw new Exception("Invalid type: " + blockType);
            }
        }
    }
}
