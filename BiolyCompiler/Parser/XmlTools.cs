using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.Parser
{
    public static class XmlTools
    {
        public static XmlNode GetNodeWithName(this XmlNode xmlNode, string name)
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

        internal static XmlNode GetNodeWithAttributeValue(this XmlNode xmlNode, string attributeName)
        {
            foreach (XmlNode item in xmlNode.ChildNodes)
            {
                foreach (XmlNode attribute in item.Attributes)
                {
                    if (attribute.Value == attributeName)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        internal static int ToInt(this XmlNode xmlNode)
        {
            return int.Parse(xmlNode.Value);
        }

        internal static int TextToInt(this XmlNode xmlNode)
        {
            return int.Parse(xmlNode.InnerText);
        }
    }
}
