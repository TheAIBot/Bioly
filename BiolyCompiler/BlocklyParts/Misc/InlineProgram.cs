using BiolyCompiler.BlocklyParts.Arithmetics;
using BiolyCompiler.BlocklyParts.Declarations;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public const string VARIABLE_COUNT_ATTRIBUTE_NAME = "variable_count";
        public const string XML_TYPE_NAME = "inlineProgram";
        public const string UNIQUE_ATTRIBUTE_IDENTIFIER = "asjdasljckkrw3209fj48dhsaljdhasdlja";
        private static int  AttributeIdentifierCounter = 0;
        public readonly string ID;
        public readonly string ProgramName;
        public readonly string[] Inputs;
        public readonly string[] Outputs;
        public readonly string[] VariableImports;
        public readonly string ProgramXml;
        public readonly Dictionary<string, FluidInput> InputsFromTo = new Dictionary<string, FluidInput>();
        public readonly Dictionary<string, string> OutputsFromTo = new Dictionary<string, string>();
        public readonly Dictionary<string, VariableBlock> VariablesFromTo = new Dictionary<string, VariableBlock>();
        public readonly bool IsValidProgram;

        public InlineProgram(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            this.ID = node.GetAttributeValue(Block.ID_FIELD_NAME);

            XmlNode mutatorNode = node.TryGetNodeWithName("mutation");
            if (mutatorNode == null)
            {
                throw new InternalParseException(ID, "No mutator was found in the inline program.");
            }

            this.ProgramName = mutatorNode.TryGetAttributeValue(PROGRAM_NAME_ATTRIBUTE_NAME);

            string inputCountString    = mutatorNode.TryGetAttributeValue(INPUT_COUNT_ATTRIBUTE_NAME);
            string outputCountString   = mutatorNode.TryGetAttributeValue(OUTPUT_COUNT_ATTRIBUTE_NAME);
            string variableCountString = mutatorNode.TryGetAttributeValue(VARIABLE_COUNT_ATTRIBUTE_NAME);

            int inputCount    = int.Parse(inputCountString    ?? "0");
            int outputCunt    = int.Parse(outputCountString   ?? "0");
            int variableCount = int.Parse(variableCountString ?? "0");

            try
            {
                (this.Inputs, this.Outputs, this.VariableImports, this.ProgramXml) = LoadProgram(ProgramName);

                for (int i = 0; i < inputCount; i++)
                {
                    XmlNode inputNode = node.GetInnerBlockNode(GetInputFieldName(i), parserInfo, new MissingBlockException(ID, $"Input {Inputs[i]} is missing a fluid block."));
                    if (inputNode != null)
                    {
                        FluidInput input = XmlParser.ParseFluidInput(inputNode, dfg, parserInfo);
                        InputsFromTo.Add(Inputs[i], input);
                    }
                }
                for (int i = 0; i < outputCunt; i++)
                {
                    string toName = node.GetNodeWithAttributeValue(GetOutputFieldName(i)).InnerText;
                    OutputsFromTo.Add(Outputs[i], toName);
                }
                for (int i = 0; i < variableCount; i++)
                {
                    XmlNode variableNode = node.GetInnerBlockNode(GetVariableFieldName(i), parserInfo, new MissingBlockException(ID, ""));
                    if (variableNode != null)
                    {
                        VariableBlock varBlock = (VariableBlock)XmlParser.ParseBlock(variableNode, dfg, parserInfo);
                        VariablesFromTo.Add(VariableImports[i], varBlock);
                    }
                }

                this.IsValidProgram = true;
            }
            catch (Exception e)
            {
                this.IsValidProgram = false;
                Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        public static (string[] inputs, string[] outputs, string[] variableImports, string programXml) LoadProgram(string programName)
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
                                                          .Select(x => x.value.OutputVariable)
                                                          .ToArray();
                    var outputs = cdfg.StartDFG.Input.Where(x => x.value is OutputDeclaration)
                                                           .Select(x => (x.value as OutputDeclaration).ModuleName)
                                                           .ToArray();
                    var variableImports = cdfg.StartDFG.Input.Where(x => x.value is ImportVariable)
                                                             .Select(x => (x.value as ImportVariable).VariableName)
                                                             .ToArray();

                    return (inputs, outputs, variableImports, programXml);
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

        private static string GetVariableFieldName(int index)
        {
            return $"variable-{index}";
        }

        public void AppendProgramXml(ref XmlNode currentProgramXml, ParserInfo parserInfo)
        {
            if (!IsValidProgram)
            {
                parserInfo.ParseExceptions.Add(new ParseException(ID, "The program can't be parsed."));
                return;
            }

            foreach (var item in OutputsFromTo)
            {
                parserInfo.AddVariable(ID, VariableType.FLUID, item.Value);
            }

            XmlDocument newXmlDoc = new XmlDocument();
            newXmlDoc.LoadXml(ProgramXml);

            //rename variables so they can't clash with the original programs variables
            List<string> variables = GetVariablesFromXmloDocument(newXmlDoc);

            //create pairs of variables for what the variable currently is and what
            //it should be converted into.
            string postfix = parserInfo.GetUniquePostFix();
            Dictionary<string, string> variablesFromTo = new Dictionary<string, string>();
            
            InputsFromTo.ToList().ForEach(x => variablesFromTo.Add(x.Key, x.Value.OriginalFluidName));
            variables.Where(x => !variablesFromTo.ContainsKey(x)).ToList().ForEach(x => variablesFromTo.Add(x, x + postfix));
            variablesFromTo.Where(x => OutputsFromTo  .ContainsKey(x.Key)).ToList().ForEach(x => OutputsFromTo  .Add(x.Value, OutputsFromTo  [x.Key]));
            variablesFromTo.Where(x => VariablesFromTo.ContainsKey(x.Key)).ToList().ForEach(x => VariablesFromTo.Add(x.Value, VariablesFromTo[x.Key]));


            //some static blocks needs to include specific changes
            HandleStaticUsageBlockVariableChanges(variablesFromTo);

            //replace the variables and update the document
            InsertNewVariablesIntoXmlDocument(newXmlDoc, variablesFromTo);

            ParserInfo dummyParserInfo = new ParserInfo();
            dummyParserInfo.EnterDFG();
            dummyParserInfo.DoTypeChecks = false;

            //replace inputs
            //replace outputs
            var splittedXml = SplitBlockXml(currentProgramXml, currentProgramXml.OwnerDocument.OuterXml);
            string textToRepresentTheNextBlock = "<to_be_replaced>90234LKASJDW8U923RJJOMFN2978RF30FJ28</to_be_replaced>";
            XmlNode firstBlockNode = newXmlDoc.FirstChild.GetNodeWithName("block").FirstChild.FirstChild;
            string xmlWithReplacedBlock = ReplaceBlocks(firstBlockNode, dummyParserInfo, newXmlDoc.OuterXml);

            //insert dummy xml which will later be replaced with the xml which comes after this inline blocks xml
            string xmlWithDummyXml = InsertDummyXml(xmlWithReplacedBlock, textToRepresentTheNextBlock);

            newXmlDoc.LoadXml(xmlWithDummyXml);

            //rename the id of all the blocks in the inline program
            //so any errors in the inline program is shown on the 
            //inline program block.
            ReplaceIDAttribute(newXmlDoc.FirstChild);

            string xmlWithReplacedIDs = newXmlDoc.OuterXml;
            string xmlWithNextPartOfProgramInserted = xmlWithReplacedIDs.Replace(textToRepresentTheNextBlock, splittedXml.nextBlockXml);
            newXmlDoc.LoadXml(xmlWithNextPartOfProgramInserted);



            InsertProgram(ref currentProgramXml, newXmlDoc.FirstChild.GetNodeWithName("block").FirstChild.FirstChild);
        }

        private void ReplaceIDAttribute(XmlNode node)
        {
            if (node.Attributes != null)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (attribute.Name == Block.ID_FIELD_NAME)
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

        private string ReplaceBlocks(XmlNode blockNode, ParserInfo dummyParserInfo, string xml)
        {
            if (blockNode.Attributes != null)
            {
                string blockType = blockNode.Attributes[Block.TYPE_FIELD_NAME]?.Value;
                if (blockType != null)
                {
                    switch (blockType)
                    {
                        case InputDeclaration.XML_TYPE_NAME:
                        case OutputDeclaration.XML_TYPE_NAME:
                        //case WasteDeclaration.XML_TYPE_NAME:
                        case HeaterDeclaration.XML_TYPE_NAME:
                            //case SensorDeclaration.XML_TYPE_NAME:
                            {
                                var splittedXml = SplitBlockXml(blockNode, xml);
                                xml = splittedXml.beforeBlockXml + (splittedXml.nextBlockXml ?? String.Empty) + splittedXml.afterBlockXml;
                                break;
                            }
                        case OutputUsage.XML_TYPE_NAME:
                            {
                                var splittedXml = SplitBlockXml(blockNode, xml);
                                DFG<Block> dfg = new DFG<Block>();
                                OutputUsage output = OutputUsage.Parse(blockNode, dfg, dummyParserInfo);
                                FluidInput fluidInputA = new BasicInput(String.Empty, OutputsFromTo[output.ModuleName], 0, true);
                                string unionXml = Union.ToXml(ID, fluidInputA.ToXml(), output.InputFluids[0].ToXml());
                                string nextXml = splittedXml.nextBlockXml;
                                string fluidXml = Fluid.ToXml(ID, fluidInputA.OriginalFluidName, unionXml, nextXml);
                                xml = splittedXml.beforeBlockXml + fluidXml + splittedXml.afterBlockXml;
                                break;
                            }
                        case ImportVariable.XML_TYPE_NAME:
                            {
                                var splittedXml = SplitBlockXml(blockNode, xml);
                                ImportVariable importVariable = (ImportVariable)ImportVariable.Parse(blockNode, dummyParserInfo, false);
                                string variabelDefinitionXml = VariablesFromTo[importVariable.VariableName].ToXml();
                                string setVariableXml = SetNumberVariable.ToXml(importVariable.BlockID, importVariable.VariableName, variabelDefinitionXml, splittedXml.nextBlockXml);
                                xml = splittedXml.beforeBlockXml + setVariableXml + splittedXml.afterBlockXml;
                                break;
                            }
                    }
                }
            }

            foreach (XmlNode node in blockNode.ChildNodes)
            {
                xml = ReplaceBlocks(node, dummyParserInfo, xml);
            }

            return xml;
        }

        private (string beforeBlockXml, string blockXml, string nextBlockXml, string afterBlockXml) SplitBlockXml(XmlNode blockNode, string xml)
        {
            string blockXml = RemoveXmlnsTag(blockNode.OuterXml);
            string nextBlockXml = blockNode.TryGetNodeWithName("next")?.FirstChild.OuterXml;
            if (nextBlockXml != null)
            {
                nextBlockXml = RemoveXmlnsTag(nextBlockXml);
                int firstOccurencePosition = blockXml.IndexOf(nextBlockXml);
                blockXml = blockXml.Substring(0, firstOccurencePosition) + blockXml.Substring(firstOccurencePosition + nextBlockXml.Length);
                //blockXml = blockXml.Replace(nextBlockXml, String.Empty);
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

        private string InsertDummyXml(string xml, string dummyXml)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xml);
            XmlNode firstBlockNode = xDoc.FirstChild.GetNodeWithName("block").FirstChild.FirstChild;
            XmlNode lastBlockNode = GetLastBlockNode(firstBlockNode);

            var splittedXml = SplitBlockXml(lastBlockNode, xml);
            string partialBlock = splittedXml.blockXml.Substring(0, splittedXml.blockXml.Length - "</block>".Length);
            return splittedXml.beforeBlockXml + partialBlock + "<next>" + dummyXml + "</next>" + "</block>" + splittedXml.afterBlockXml;
        }

        private XmlNode GetLastBlockNode(XmlNode node)
        {
            while (node.TryGetNodeWithName("next") != null)
            {
                node = node.TryGetNodeWithName("next").FirstChild;
            }

            return node;
        }

        private void InsertProgram(ref XmlNode node, XmlNode modifiedXmlNode)
        {
            XmlAttribute typeAttr = modifiedXmlNode.OwnerDocument.CreateAttribute(UNIQUE_ATTRIBUTE_IDENTIFIER);
            typeAttr.Value = "0";

            modifiedXmlNode.Attributes.Append(typeAttr);

            var splittedXml = SplitBlockXml(node, RemoveXmlnsTag(node.OwnerDocument.OuterXml));

            string combinedXml = splittedXml.beforeBlockXml + RemoveXmlnsTag(modifiedXmlNode.OuterXml) + splittedXml.afterBlockXml;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(combinedXml);

            node = GetXmlNodeWithSpecificID(doc.FirstChild, UNIQUE_ATTRIBUTE_IDENTIFIER, "0");
            node.Attributes.Remove(node.Attributes[UNIQUE_ATTRIBUTE_IDENTIFIER]);
            //node = node.GetNodeWithName("next").FirstChild;
        }

        private XmlNode GetXmlNodeWithSpecificID(XmlNode node, string key, string id)
        {
            if (node.Attributes != null)
            {
                if (node.Attributes[key]?.Value == id)
                {
                    return node;
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                XmlNode result = GetXmlNodeWithSpecificID(child, key, id);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}