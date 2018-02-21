using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.Parser
{
    internal static class XmlTools
    {
        internal static XmlNode GetNodeWithName(this XmlNode xmlNode, string name)
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

        internal static int ToInt(this XmlNode xmlNode)
        {
            return int.Parse(xmlNode.Value);
        }
    }
}
