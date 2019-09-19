using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace BiolyCompiler
{
    public static class Extensions
    {
        public static XmlNode GetNodeWithName(this XmlNode xmlNode, string name)
        {
            XmlNode result = TryGetNodeWithName(xmlNode, name);
            if (result == null)
            {
                throw new InternalParseException($"Failed to find a node with name {name}. Xml: {xmlNode.InnerXml}");
            }
            return result;
        }
        public static XmlNode TryGetNodeWithName(this XmlNode xmlNode, string name)
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

        internal static XmlNode GetNodeWithAttributeValue(this XmlNode xmlNode, string attributeValue)
        {
            XmlNode result = TryGetNodeWithAttributeValue(xmlNode, attributeValue);
            if (result == null)
            {
                throw new InternalParseException($"Failed to find a node with the attribute value {attributeValue}. Xml: {xmlNode.InnerXml}");
            }
            return result;
        }

        internal static XmlNode TryGetNodeWithAttributeValue(this XmlNode xmlNode, string attributeValue)
        {
            foreach (XmlNode item in xmlNode.ChildNodes)
            {
                foreach (XmlNode attribute in item.Attributes)
                {
                    if (attribute.Value == attributeValue)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        internal static string GetAttributeValue(this XmlNode xmlNode, string attributeName)
        {
            string result = xmlNode.TryGetAttributeValue(attributeName);
            if (result == null)
            {
                throw new InternalParseException($"Failed to find the attribute {attributeName}. Xml: {xmlNode.InnerXml}");
            }
            return result;
        }

        internal static string TryGetAttributeValue(this XmlNode xmlNode, string attributeName)
        {
            //return xmlNode.Attributes["type"].Value;
            try
            {
                foreach (XmlNode attribute in xmlNode.Attributes)
                {
                    if (attribute.Name == attributeName)
                    {
                        return attribute.Value;
                    }
                }
            }
            catch (Exception ee)
            {
                throw new Exception("failed to do the stuff");
            }
            //throw new Exception("asdas" + xmlNode.Attributes["type"].Value);

            return null;
        }

        internal static float TextToFloat(this XmlNode xmlNode, string id, ParserInfo parseInfo)
        {
            if (float.TryParse(xmlNode.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
            {
                return value;
            }

            parseInfo.ParseExceptions.Add(new NotANumberException(id, xmlNode.InnerText));
            return float.NaN;
        }

        internal static XmlNode GetInnerBlockNode(this XmlNode node, string nodeAttribName, ParserInfo parserInfo, ParseException exception)
        {
            XmlNode innerNode = node.TryGetNodeWithAttributeValue(nodeAttribName);
            if (innerNode == null)
            {
                parserInfo.ParseExceptions.Add(exception);
                return null;
            }
            return innerNode.FirstChild;
        }

        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        internal static List<string> Copy(this IReadOnlyList<string> toCopy)
        {
            List<string> copy = new List<string>();
            foreach (string stringCopy in toCopy)
            {
                copy.Add(stringCopy);
            }

            return copy;
        }

        internal static List<FluidInput> Copy(this IReadOnlyList<FluidInput> toCopy, DFG<Block> dfg)
        {
            List<FluidInput> copy = new List<FluidInput>();
            foreach (FluidInput input in toCopy)
            {
                copy.Add(input.TrueCopy(dfg));
            }

            return copy;
        }
    }
}
