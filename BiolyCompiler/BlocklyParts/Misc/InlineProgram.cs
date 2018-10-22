using BiolyCompiler.BlocklyParts.Arithmetics;
using BiolyCompiler.BlocklyParts.ControlFlow;
using BiolyCompiler.BlocklyParts.Declarations;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using BiolyCompiler.Scheduling;
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
        public readonly string ProgramName;
        public readonly string[] Inputs;
        public readonly string[] Outputs;
        public readonly string[] VariableImports;
        public readonly CDFG ProgramCDFG;
        public readonly Dictionary<string, FluidInput> InputsFromTo = new Dictionary<string, FluidInput>();
        public readonly Dictionary<string, string> OutputsFromTo = new Dictionary<string, string>();
        public readonly Dictionary<string, VariableBlock> VariablesFromTo = new Dictionary<string, VariableBlock>();
        public readonly bool IsValidProgram;

        public InlineProgram(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);

            XmlNode mutatorNode = node.TryGetNodeWithName("mutation");
            if (mutatorNode == null)
            {
                throw new InternalParseException(id, "No mutator was found in the inline program.");
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
                (this.Inputs, this.Outputs, this.VariableImports, _, this.ProgramCDFG) = LoadProgram(ProgramName);

                for (int i = 0; i < inputCount; i++)
                {
                    XmlNode inputNode = node.GetInnerBlockNode(GetInputFieldName(i), parserInfo, new MissingBlockException(id, $"Input {Inputs[i]} is missing a fluid block."));
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
                    XmlNode variableNode = node.GetInnerBlockNode(GetVariableFieldName(i), parserInfo, new MissingBlockException(id, ""));
                    if (variableNode != null)
                    {
                        VariableBlock varBlock = (VariableBlock)XmlParser.ParseBlock(variableNode, dfg, parserInfo);
                        VariablesFromTo.Add(VariableImports[i], varBlock);
                    }
                }

                TransformCDFGToFunctionCDFG(ProgramCDFG);
                this.IsValidProgram = true;
            }
            catch (Exception e)
            {
                this.IsValidProgram = false;
                Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
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

        public static (string[] inputs, string[] outputs, string[] variableImports, string programXml, CDFG cdfg) LoadProgram(string programName)
        {
            string programXml = GetProgramXml(programName);
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

                return (inputs, outputs, variableImports, programXml, cdfg);
            }
            else
            {
                throw new InternalParseException("The loaded program contains parse exceptions");
            }
        }

        private static string GetProgramXml(string programName)
        {
            string programNameWithExtension = programName + CompilerOptions.FILE_EXTENSION;
            string fullProgramPath = Path.Combine(CompilerOptions.PROGRAM_FOLDER_PATH, programNameWithExtension);
            if (File.Exists(fullProgramPath))
            {
                return File.ReadAllText(fullProgramPath);
            }
            else
            {
                throw new FileNotFoundException("Failed to load the program.", fullProgramPath);
            }
        }

        public void TransformCDFGToFunctionCDFG(CDFG toTransform)
        {
            for (int i = 0; i < toTransform.Nodes.Count; i++)
            {
                TransformDFGToFunctionDFG(toTransform.Nodes[i].dfg);
            }
        }

        private void TransformDFGToFunctionDFG(DFG<Block> dfg)
        {
            //New blocks are crerated which requires new dependencies
            //and dependencies are created when they are inserted into
            //the dfg, so a new dfg is created to create the correct
            //dependencies.
            //The given dfg is still used as the corrected result is then
            //copied into the given dfg.
            DFG<Block> correctOrder = new DFG<Block>();
            Assay fisk = new Assay(dfg);
            foreach (Block block in fisk)
            {
                if (block is InputDeclaration)
                {
                    string newName = block.OutputVariable;
                    string oldName = InputsFromTo[block.OutputVariable].OriginalFluidName;
                    correctOrder.AddNode(new FluidRef(newName, oldName));
                }
                else if (block is OutputDeclaration ||
                         //block is WasteDeclaration ||
                         block is HeaterDeclaration /*||
                         block is SensorDeclaration*/)
                {
                    continue;
                }
                else if (block is OutputUsage outputUsage)
                {
                    List<FluidInput> inputs = new List<FluidInput>()
                    {
                        block.InputFluids[0].TrueCopy(correctOrder),
                        block.InputFluids[0].TrueCopy(correctOrder)
                    };

                    inputs[1].OriginalFluidName = OutputsFromTo[outputUsage.ModuleName];

                    correctOrder.AddNode(new Union(inputs, OutputsFromTo[outputUsage.ModuleName], block.BlockID));
                }
                else if (block is ImportVariable import)
                {
                    VariableBlock asdqwd = (VariableBlock)VariablesFromTo[import.VariableName].TrueCopy(correctOrder);

                    correctOrder.AddNode(new SetNumberVariable(asdqwd, import.VariableName, block.BlockID));
                }
                else
                {
                    List<Block> blocks = block.GetBlockTreeList(new List<Block>());
                    foreach (Block blockTreeBlock in blocks)
                    {
                        correctOrder.AddNode(blockTreeBlock);
                    }
                }
            }
            correctOrder.FinishDFG();

            dfg.Nodes.Clear();
            dfg.Input.Clear();
            dfg.Output.Clear();

            dfg.Nodes.AddRange(correctOrder.Nodes);
            dfg.Input.AddRange(correctOrder.Input);
            dfg.Output.AddRange(correctOrder.Output);
        }


        public Direct GetProgram(ref XmlNode currentProgramXml, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(currentProgramXml);
            CDFG newProgram = ProgramCDFG.Copy();

            TransformVariableNames(newProgram, parserInfo.GetUniquePostFix());
            ChangeIDs(newProgram, id);

            //Add new variables that this program added
            OutputsFromTo.ForEach(x => parserInfo.AddVariable(string.Empty, VariableType.FLUID, x.Value));
            DFG<Block> nextDFG = XmlParser.ParseNextDFG(currentProgramXml, parserInfo);

            //merge the programs together nd return the link between then
            return parserInfo.cdfg.AddCDFG(newProgram, dfg);
        }

        private void TransformVariableNames(CDFG cdfg, string postfix)
        {
            Stack<IEnumerator<DFG<Block>>> stack = new Stack<IEnumerator<DFG<Block>>>();
            HashSet<string> readerBlacklist = new HashSet<string>();
            HashSet<string> writerBlacklist = new HashSet<string>();

            InputsFromTo.ForEach(x => readerBlacklist.Add(x.Value.OriginalFluidName));
            VariablesFromTo.ForEach(x => GetVariableBlockDependencies(x.Value.GetVariableTreeList(new List<VariableBlock>())).ForEach(y => readerBlacklist.Add(y)));
            VariablesFromTo.ForEach(x => readerBlacklist.Add(x.Key));
            OutputsFromTo.ForEach(x => writerBlacklist.Add(x.Value));

            DFG<Block> currentDFG = cdfg.StartDFG;

            do
            {
                Assay inOrder = new Assay(currentDFG);
                foreach (Block block in inOrder)
                {
                    List<Block> blocks = block.GetBlockTreeList(new List<Block>());
                    foreach (var blockInTree in blocks)
                    {
                        foreach (FluidInput fluidInput in blockInTree.InputFluids)
                        {
                            if (!readerBlacklist.Contains(fluidInput.OriginalFluidName))
                            {
                                fluidInput.OriginalFluidName += postfix;
                            }
                        }

                        for (int i = 0; i < blockInTree.InputNumbers.Count; i++)
                        {
                            if (!readerBlacklist.Contains(blockInTree.InputNumbers[i]))
                            {
                                blockInTree.InputNumbers[i] = blockInTree.InputNumbers[i] + postfix;
                            }
                            else if (blockInTree.InputNumbers[i] == "error")
                            {

                            }
                        }

                        if (readerBlacklist.Contains(blockInTree.OutputVariable))
                        {
                            if (blockInTree.OutputVariable == "error")
                            {

                            }
                            readerBlacklist.Remove(blockInTree.OutputVariable);
                        }
                        if (!writerBlacklist.Contains(blockInTree.OutputVariable))
                        {
                            blockInTree.OutputVariable += postfix;
                        }
                    }
                }

                IControlBlock control = cdfg.Nodes.Single(x => x.dfg == currentDFG).control;
                if (control != null)
                {
                    stack.Push(control.GetEnumerator());
                }

                while (stack.Count > 0)
                {
                    if (!stack.Peek().MoveNext())
                    {
                        stack.Pop();
                        continue;
                    }

                    currentDFG = stack.Peek().Current;
                    break;
                }


            } while (stack.Count > 0);
        }

        private List<string> GetVariableBlockDependencies(List<VariableBlock> blocks)
        {
            List<string> dependencies = new List<string>();
            blocks.ForEach(x => dependencies.AddRange(x.InputNumbers));
            blocks.ForEach(x => dependencies.AddRange(x.InputFluids.Select(y => y.OriginalFluidName)));

            dependencies.RemoveAll(x => x == Block.DEFAULT_NAME);
            return dependencies.Distinct().ToList();
        }

        private void ChangeIDs(CDFG cdfg, string newID)
        {
            foreach (DFG<Block> dfg in cdfg.Nodes.Select(x => x.dfg))
            {
                foreach (Node<Block> node in dfg.Nodes)
                {
                    Block block = node.value;
                    block.BlockID = newID;
                    foreach (FluidInput input in block.InputFluids)
                    {
                        input.ID = newID;
                    }
                }
            }
        }
    }
}