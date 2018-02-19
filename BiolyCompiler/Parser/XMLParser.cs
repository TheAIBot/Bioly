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
            node = GetNodeWithName(node, "block");

            CDFG cdfg = new CDFG(); 
            Queue<DFG<Block>> dfgQueue = new Queue<DFG<Block>>();
            DFG<Block> currentDFG = new DFG<Block>();
            do
            {


            } while (GetNodeWithName(node, "next") != null);

            return null;
        }

        private static Block GetBlock(XmlNode node)
        {
            string blockType = node.Attributes["type"].Value;
            switch (blockType)
            {
                case Fluid.XmlTypeName:
                    return new Fluid();
                case Input.XmlTypeName:
                    return new Input();
                case Output.XmlTypeName:
                    return new Output();
                case Waste.XmlTypeName:
                    return new Waste();
                case Heater.XmlTypeName:
                    return new Heater();
                case Mixer.XmlTypeName:
                    return new Mixer();
                case Splitter.XmlTypeName:
                    return new Splitter();
                case Sensor.XmlTypeName:
                    return new Sensor();
                default:
                    throw new Exception("Invalid type: " + blockType);
            }
        }

        private static XmlNode GetNodeWithName(XmlNode xmlNode, string name)
        {
            foreach (XmlNode item in xmlNode.ChildNodes)
            {
                if (item.Name == name)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
