using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts
{
    public static class ProgramCache
    {
        private static readonly Dictionary<string, InlineProgram> Cache = new Dictionary<string, InlineProgram>();
        private static readonly Dictionary<string, string> StoredFiles = new Dictionary<string, string>();
        private static readonly object Locker = new object();

        public static InlineProgram GetProgram(XmlNode node, string id, ParserInfo parserInfo)
        {
            lock (Locker)
            {
                string programName = InlineProgram.GetProgramName(node, id);

                if (Cache.ContainsKey(programName))
                {
                    return Cache[programName];
                }

                if (StoredFiles.TryGetValue(programName, out string fileContent))
                {
                    Cache.Add(programName, new InlineProgram(node, parserInfo, fileContent));
                }
                else
                {
                    Cache.Add(programName, new InlineProgram(node, parserInfo));
                }

                return Cache[programName];
            }
        }

        public static void AddPrograms(List<(string filename, string fileContent)> files)
        {
            foreach (var file in files)
            {
                StoredFiles.Add(file.filename, file.fileContent);
            }
        }
    }
}
