using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class InlineProgram
    {
        public const string PROGRAM_NAME_ATTRIBUTE_NAME = "program_name";
        public const string INPUT_COUNT_ATTRIBUTE_NAME = "input_count";
        public const string OUTPUT_COUNT_ATTRIBUTE_NAME = "output_count";
        public const string XML_TYPE_NAME = "inlineProgram";
        public readonly string ID;
        public readonly string ProgramName;
        public readonly string[] Inputs;
        public readonly string[] Outputs;
        public readonly string ProgramXml;
        public readonly Dictionary<string, FluidInput> InputsFromTo = new Dictionary<string, FluidInput>();
        public readonly Dictionary<string, string> OutputsFromTo = new Dictionary<string, string>();

        public InlineProgram(XmlNode node, ParserInfo parserInfo)
        {
            this.ID = node.GetAttributeValue(Block.IDFieldName);

            XmlNode mutatorNode = node.TryGetNodeWithName("mutation");
            if (mutatorNode == null)
            {
                throw new InternalParseException(ID, "No mutator was found in the inline program.");
            }

            this.ProgramName = mutatorNode.TryGetAttributeValue(PROGRAM_NAME_ATTRIBUTE_NAME);
            int inputCount = int.Parse(mutatorNode.TryGetAttributeValue(INPUT_COUNT_ATTRIBUTE_NAME));
            int outputCunt = int.Parse(mutatorNode.TryGetAttributeValue(OUTPUT_COUNT_ATTRIBUTE_NAME));

            (this.Inputs, this.Outputs, this.ProgramXml) = LoadProgram(ProgramName);

            for (int i = 0; i < inputCount; i++)
            {
                XmlNode inputNode = node.GetInnerBlockNode(GetInputFieldName(i), parserInfo, new MissingBlockException(ID, $"Input {Inputs[i]} is missing a fluid block."));
                if (inputNode != null)
                {
                    FluidInput input = XmlParser.GetVariablesCorrectedName(inputNode, parserInfo);
                    InputsFromTo.Add(Inputs[i], input);
                }
            }
            for (int i = 0; i < outputCunt; i++)
            {
                string toName = node.GetNodeWithAttributeValue(GetOutputFieldName(i)).InnerText;
                OutputsFromTo.Add(Outputs[i], toName);
            }
        }

        public static (string[] inputs, string[] outputs, string programXml) LoadProgram(string programName)
        {
            string programNameWithExtension = programName + CompilerOptions.FILE_EXTENSION;
            string fullProgramPath = Path.Combine(CompilerOptions.PROGRAM_FOLDER_PATH, programNameWithExtension);
            if (File.Exists(fullProgramPath))
            {
                string programXml = File.ReadAllText(fullProgramPath);
                (CDFG cdfg, List<ParseException> exceptions) = XmlParser.Parse(programXml);
                if (exceptions.Count == 0)
                {
                    var inputs = cdfg.StartDFG.Input.Where(x => x.value is InputDeclaration)
                                                          .Select(x => x.value.OriginalOutputVariable)
                                                          .ToArray();
                    var outputs = cdfg.StartDFG.Input.Where(x => x.value is OutputDeclaration)
                                                           .Select(x => (x.value as OutputDeclaration).ModuleName)
                                                           .ToArray();

                    return (inputs, outputs, programXml);
                }
                else
                {
                    throw new InternalParseException("The loaded program contains parse exceptions");
                }
            }
            else
            {
                throw new FileNotFoundException("Failed to load the program.", fullProgramPath);
            }
        }

        private string GetInputFieldName(int index)
        {
            return $"input-{index}";
        }

        private string GetOutputFieldName(int index)
        {
            return $"output-{index}";
        }
    }
}