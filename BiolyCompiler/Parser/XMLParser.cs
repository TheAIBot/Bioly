using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.BlocklyParts.Blocks.FFUs;
using BiolyCompiler.BlocklyParts.Blocks.Misc;
using BiolyCompiler.BlocklyParts.Blocks.Sensors;
using BiolyCompiler.Graphs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.Parser
{
    public static class XMLParser
    {
        public static CDFG Parse(string xmlText)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlText);

            XmlNode node = xmlDocument.FirstChild;
            node = XmlTools.GetNodeWithName(node, "block");

            var cdfg = new CDFG();
            var cfg = new CFG<DFG<Block>>();
            var dfgQueue = new Queue<(DFG<Block> savedDFG, XmlNode savedNode)>();
            var currentDFG = new DFG<Block>();
            while (true)
            {
                if (IsConditional(node))
                {
                    //do something about them
                    continue;
                }

                Block block = GetBlock(node);
                Node<Block> dfgNode = new Node<Block>();
                dfgNode.value = block;

                node = XmlTools.GetNodeWithName(node, "next");
                if (node == null)
                {
                    break;
                }
            }

            return cdfg;
        }

        private static bool IsConditional(XmlNode node)
        {
            return node.GetNodeWithName("statement") != null;
        }

        private static Block GetBlock(XmlNode node)
        {
            string blockType = node.Attributes["type"].Value;
            switch (blockType)
            {
                case Fluid.XmlTypeName:
                    return Fluid.TryParseBlock(node);
                case Input.XmlTypeName:
                    return Input.TryParseBlock(node);
                case Output.XmlTypeName:
                    return Output.TryParseBlock(node);
                case Waste.XmlTypeName:
                    return Waste.TryParseBlock(node);
                //case Sensor.XmlTypeName:
                //    return new Sensor();
                default:
                    throw new Exception("Invalid type: " + blockType);
            }
        }
    }
}
