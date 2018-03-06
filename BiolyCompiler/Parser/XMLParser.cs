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
using System.Threading;
using BiolyCompiler.BlocklyParts.Arithmetics;
using BiolyCompiler.BlocklyParts.BoolLogic;
using BiolyCompiler.BlocklyParts.ControlFlow;

namespace BiolyCompiler.Parser
{
    public static class XmlParser
    {
        private static int nameID = 0;
        private static Object nameIDLock = new object();

        public static CDFG Parse(string xmlText)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlText);

            CDFG cdfg = new CDFG();
            XmlNode node = xmlDocument.FirstChild.GetNodeWithName("block").FirstChild.FirstChild;

            DFG<Block> startDFG = ParseDFG(node, cdfg);
            cdfg.StartDFG = startDFG;

            return cdfg;
        }

        internal static DFG<Block> ParseDFG(XmlNode node, CDFG cdfg)
        {
            IControlBlock controlBlock = null;
            var dfg = new DFG<Block>();
            var mostRecentRef = new Dictionary<string, Node<Block>>();
            while (true)
            {
                if (IsConditional(node))
                {
                    controlBlock = ParseConditionalBlocks(node, cdfg, dfg, mostRecentRef);
                    break;
                }

                ParseAndAddNodeToDFG(node, dfg, mostRecentRef);

                //move on to the next node or exit if none
                node = XmlTools.GetNodeWithName(node, "next");
                if (node == null)
                {
                    break;
                }
                node = node.FirstChild;
            }

            dfg.FinishDFG();
            cdfg.AddNode(controlBlock, dfg);

            return dfg;
        }

        internal static Block ParseAndAddNodeToDFG(XmlNode node, DFG<Block> dfg, Dictionary<string, Node<Block>> mostRecentRef)
        {
            Block block = ParseBlock(node, dfg);
            Node<Block> dfgNode = new Node<Block>();
            dfgNode.value = block;

            //add nessesary edges to this new node
            foreach (string inputNodeName in block.InputVariables)
            {
                if (mostRecentRef.TryGetValue(inputNodeName, out Node<Block> inputNode))
                {
                    dfg.AddEdge(inputNode, dfgNode);
                }
                else if (dfg.Nodes.Any(x => x.value.OutputVariable == inputNodeName))
                {
                    Node<Block> inputNodee = dfg.Nodes.Single(x => x.value.OutputVariable == inputNodeName);
                    dfg.AddEdge(inputNodee, dfgNode);
                }
            }

            dfgNode.EdgesCreated = true;
            dfg.AddNode(dfgNode);

            //update map of most recent nodes that outputs the variable
            //so other nodes that get their value from the node that
            //just updated the value
            if (mostRecentRef.ContainsKey(block.OutputVariable))
            {
                mostRecentRef[block.OutputVariable] = dfgNode;
            }
            else
            {
                mostRecentRef.Add(block.OutputVariable, dfgNode);
            }

            return block;
        }

        private static IControlBlock ParseConditionalBlocks(XmlNode node, CDFG cdfg, DFG<Block> dfg, Dictionary<string, Node<Block>> mostRecentRef)
        {
            string blockType = node.Attributes["type"].Value;
            switch (blockType)
            {
                case If.XmlTypeName:
                    return new If(node, cdfg, dfg, mostRecentRef);
                case Repeat.XmlTypeName:
                    return new Repeat(node, cdfg, dfg);
                default:
                    throw new Exception("Invalid type: " + blockType);
            }



        }

        internal static DFG<Block> ParseNextDFG(XmlNode node, CDFG cdfg)
        {
            node = XmlTools.GetNodeWithName(node, "next");
            if (node == null)
            {
                return null;
            }

            node = node.FirstChild;
            return ParseDFG(node, cdfg);
        }

        private static bool IsConditional(XmlNode node)
        {
            return node.GetNodeWithName("statement") != null;
        }

        public static Block ParseBlock(XmlNode node, DFG<Block> dfg)
        {
            string blockType = node.Attributes["type"].Value;
            switch (blockType)
            {
                case ArithOP.XmlTypeName:
                    return ArithOP.Parse(node, dfg);
                case Constant.XmlTypeName:
                    return Constant.Parse(node);
                //case FluidArray.XmlTypeName:
                //    return FluidArray.Parse(node);
                //case SetFluidArray.XmlTypeName:
                //    return SetFluidArray.Parse(node);
                case Fluid.XmlTypeName:
                    return Fluid.Parse(node);
                case Input.XmlTypeName:
                    return Input.Parse(node);
                case Output.XmlTypeName:
                    return Output.Parse(node);
                case Waste.XmlTypeName:
                    return Waste.Parse(node);
                case BoolOP.XmlTypeName:
                    return BoolOP.Parse(node, dfg);
                //case Sensor.XmlTypeName:
                //    return Sensor.Parse(node);
                default:
                    throw new Exception("Invalid type: " + blockType);
            }
        }

        internal static string CreateName()
        {
            lock (nameIDLock)
            {
                int id = nameID;
                nameID++;

                return "N" + id;
            }
        }
    }
}
