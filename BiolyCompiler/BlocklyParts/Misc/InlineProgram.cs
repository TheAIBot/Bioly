using BiolyCompiler.BlocklyParts.Declarations;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public readonly bool IsValidProgram;

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

            try
            {
                (this.Inputs, this.Outputs, this.ProgramXml) = LoadProgram(ProgramName);

                for (int i = 0; i < inputCount; i++)
                {
                    XmlNode inputNode = node.GetInnerBlockNode(GetInputFieldName(i), parserInfo, new MissingBlockException(ID, $"Input {Inputs[i]} is missing a fluid block."));
                    if (inputNode != null)
                    {
                        FluidInput input = new FluidInput(inputNode, parserInfo);
                        InputsFromTo.Add(Inputs[i], input);
                    }
                }
                for (int i = 0; i < outputCunt; i++)
                {
                    string toName = node.GetNodeWithAttributeValue(GetOutputFieldName(i)).InnerText;
                    OutputsFromTo.Add(Outputs[i], toName);
                }

                this.IsValidProgram = true;
            }
            catch (Exception)
            {
                this.IsValidProgram = false;
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

        private static string GetInputFieldName(int index)
        {
            return $"input-{index}";
        }

        private static string GetOutputFieldName(int index)
        {
            return $"output-{index}";
        }

        public void AppendProgramXml(ref XmlNode currentProgramXml, ParserInfo parserInfo)
        {
            if (!IsValidProgram)
            {
                parserInfo.ParseExceptions.Add(new ParseException(ID, "The program can't be parsed."));
                return;
            }

            XmlDocument newXmlDoc = new XmlDocument();
            newXmlDoc.LoadXml(ProgramXml);

            //rename variables so they can't clash with the original programs variables
            List<string> variables = GetVariablesFromXmloDocument(newXmlDoc);

            //create pairs of variables for what the variable currently is and what
            //it should be converted into.
            string postfix = parserInfo.GetUniquePostFix();
            Dictionary<string, string> variablesFromTo = new Dictionary<string, string>();
            variables.ForEach(x => variablesFromTo.Add(x, x + postfix));
            variablesFromTo.Where(x => InputsFromTo .ContainsKey(x.Key)).ToList().ForEach(x => InputsFromTo .Add(x.Value, InputsFromTo [x.Key]));
            variablesFromTo.Where(x => OutputsFromTo.ContainsKey(x.Key)).ToList().ForEach(x => OutputsFromTo.Add(x.Value, OutputsFromTo[x.Key]));
            

            //some static blocks needs to include specific changes
            HandleStaticUsageBlockVariableChanges(variablesFromTo);

            //replace the variables and update the document
            InsertNewVariablesIntoXmlDocument(newXmlDoc, variablesFromTo);

            ParserInfo dummyParserInfo = new ParserInfo();
            dummyParserInfo.EnterDFG();
            variablesFromTo.Where(x => OutputsFromTo.ContainsKey(x.Key)).ToList().ForEach(x => dummyParserInfo.AddModuleName(x.Value));
            variablesFromTo.ToList().ForEach(x => dummyParserInfo.AddFluidVariable(x.Value));

            //replace inputs
            //replace outputs
            var splittedXml = SplitBlockXml(currentProgramXml, currentProgramXml.OwnerDocument.OuterXml);
            string xmlWithReplacedBlock = ReplaceBlocks(newXmlDoc.FirstChild.GetNodeWithName("block").FirstChild.FirstChild, dummyParserInfo, newXmlDoc.OuterXml, splittedXml.nextBlockXml);
            newXmlDoc.LoadXml(xmlWithReplacedBlock);

            //rename the id of all the blocks in the inline program
            //so any errors in the inline program is shown on the 
            //inline program block.
            ReplaceIDAttribute(newXmlDoc.FirstChild);

            InsertProgram(ref currentProgramXml, newXmlDoc.FirstChild.GetNodeWithName("block").FirstChild.FirstChild.OuterXml);
        }

        private void ReplaceIDAttribute(XmlNode node)
        {
            if (node.Attributes != null)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (attribute.Name == Block.IDFieldName)
                    {
                        attribute.Value = this.ID;
                    }
                }
            }

            foreach (XmlNode childNode in node.ChildNodes)
            {
                ReplaceIDAttribute(childNode);
            }
        }

        private List<string> GetVariablesFromXmloDocument(XmlDocument document)
        {
            List<string> variables = new List<string>();
            foreach (XmlNode variableNode in document.FirstChild.GetNodeWithName("variables"))
            {
                variables.Add(variableNode.InnerText);
            }
            return variables;
        }

        private void HandleStaticUsageBlockVariableChanges(Dictionary<string, string> variablesFromTo)
        {
            //change the heater and sensor usage module to the new one in the dictionary
        }

        private void InsertNewVariablesIntoXmlDocument(XmlDocument document, Dictionary<string, string> variablesFromTo)
        {
            //take xml and replace the values directly in it because
            //it's known that variables will only appear inside >< blocks so
            //there can't be(many) unintended side effects from doing this!
            string programXml = document.OuterXml;
            foreach (KeyValuePair<string, string> variableFromTo in variablesFromTo)
            {
                programXml = programXml.Replace($">{variableFromTo.Key}<", $">{variableFromTo.Value}<");
            }

            //update the document with the new variables
            document.LoadXml(programXml);
        }

        private string ReplaceBlocks(XmlNode blockNode, ParserInfo dummyParserInfo, string xml, string nextXmlFromPrevDocument, bool straightLine = true)
        {
            if (blockNode.Attributes != null)
            {
                string blockType = blockNode.Attributes[Block.TypeFieldName]?.Value;
                if (blockType != null)
                {
                    switch (blockType)
                    {
                        case InputDeclaration.XML_TYPE_NAME:
                            {
                                var splittedXml = SplitBlockXml(blockNode, xml);
                                InputDeclaration inputBlock = InputDeclaration.Parse(blockNode);
                                string fluidInputXml = InputsFromTo[inputBlock.OriginalOutputVariable].ToXml();
                                string inputXml = Fluid.ToXml(ID, inputBlock.OriginalOutputVariable, fluidInputXml, splittedXml.nextBlockXml);

                                xml = splittedXml.beforeBlockXml + inputXml + splittedXml.afterBlockXml;
                                break;
                            }
                        case OutputDeclaration.XML_TYPE_NAME:
                        //case WasteDeclaration.XML_TYPE_NAME:
                        case HeaterDeclaration.XML_TYPE_NAME:
                            //case SensorDeclaration.XML_TYPE_NAME:
                            {
                                var splittedXml = SplitBlockXml(blockNode, xml);
                                xml = splittedXml.beforeBlockXml + (splittedXml.nextBlockXml ?? String.Empty) + splittedXml.afterBlockXml;
                                break;
                            }
                        case OutputUseage.XML_TYPE_NAME:
                            {
                                var splittedXml = SplitBlockXml(blockNode, xml);
                                OutputUseage output = OutputUseage.Parse(blockNode, dummyParserInfo);
                                FluidInput fluidInputA = new FluidInput(OutputsFromTo[output.ModuleName], OutputsFromTo[output.ModuleName], 0, true);
                                string unionXml = Union.ToXml(ID, fluidInputA.ToXml(), output.InputVariables[0].ToXml());
                                string nextXml = splittedXml.nextBlockXml;
                                if (straightLine && splittedXml.nextBlockXml == null)
                                {
                                    nextXml = nextXmlFromPrevDocument;
                                }
                                string fluidXml = Fluid.ToXml(ID, fluidInputA.OriginalFluidName, unionXml, nextXml);
                                xml = splittedXml.beforeBlockXml + fluidXml + splittedXml.afterBlockXml;
                                break;
                            }
                    }
                }
            }

            foreach (XmlNode node in blockNode.ChildNodes)
            {
                bool stillStraightLine = straightLine && (node.Name == "block" || node.Name == "next");
                xml = ReplaceBlocks(node, dummyParserInfo, xml, nextXmlFromPrevDocument, stillStraightLine);
            }

            return xml;
        }

        private (string beforeBlockXml, string blockXml, string nextBlockXml, string afterBlockXml) SplitBlockXml(XmlNode blockNode, string xml)
        {
            string blockXml = blockNode.OuterXml;
            string nextBlockXml = blockNode.TryGetNodeWithName("next")?.FirstChild.OuterXml;
            if (nextBlockXml != null)
            {
                nextBlockXml = RemoveXmlnsTag(nextBlockXml);
                blockXml = blockXml.Replace(nextBlockXml, String.Empty);
            }

            string[] splittedDocument = xml.Split(new string[] { RemoveXmlnsTag(blockNode.OuterXml) }, StringSplitOptions.None);
            string beforeBlockXml = splittedDocument[0];
            string afterBlockXml = splittedDocument[1];

            return (beforeBlockXml, blockXml, nextBlockXml, afterBlockXml);
        }

        private string RemoveXmlnsTag(string xml)
        {
            return Regex.Replace(xml, " xmlns=\"[^\"]+\"", String.Empty);
        }

        private void InsertProgram(ref XmlNode node, string modifiedXml)
        {
            var splittedXml = SplitBlockXml(node, node.OwnerDocument.OuterXml);
            string address = node.ParentNode.ParentNode.Attributes[Block.IDFieldName].Value;

            string combinedXml = splittedXml.beforeBlockXml + modifiedXml + splittedXml.afterBlockXml;

            XmlDocument doc = node.OwnerDocument;
            doc.LoadXml(combinedXml);

            node = GetXmlNodeWithSpecificID(doc.FirstChild, address);
            node = node.GetNodeWithName("next").FirstChild;
        }

        private XmlNode GetXmlNodeWithSpecificID(XmlNode node, string id)
        {
            if (node.Attributes != null)
            {
                if (node.Attributes[Block.IDFieldName]?.Value == id)
                {
                    return node;
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                XmlNode result = GetXmlNodeWithSpecificID(child, id);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}