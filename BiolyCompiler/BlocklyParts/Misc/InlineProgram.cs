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
        private const string PROGRAM_NAME_ATTRIBUTE_NAME = "program_name";
        private const string INPUT_COUNT_ATTRIBUTE_NAME = "input_count";
        private const string OUTPUT_COUNT_ATTRIBUTE_NAME = "output_count";
        private const string VARIABLE_COUNT_ATTRIBUTE_NAME = "variable_count";
        public const string XML_TYPE_NAME = "inlineProgram";
        public readonly string ProgramName;
        private readonly CDFG ProgramCDFG;
        private readonly string[] Inputs;
        private readonly string[] Outputs;
        private readonly string[] VariableImports;
        public readonly bool IsValidProgram;

        private class InlineProgramInfo
        {
            public readonly Dictionary<string, FluidInput> InputsFromTo = new Dictionary<string, FluidInput>();
            public readonly Dictionary<string, string> OutputsFromTo = new Dictionary<string, string>();
            public readonly Dictionary<string, VariableBlock> VariablesFromTo = new Dictionary<string, VariableBlock>();
        }

        public InlineProgram(XmlNode node, ParserInfo parserInfo) : this(node, parserInfo, GetProgramXml(GetProgramName(node, node.GetAttributeValue(Block.ID_FIELD_NAME))))
        {
        }

        public InlineProgram(XmlNode node, ParserInfo parserInfo, string fileContent)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            this.ProgramName = GetProgramName(node, id);


            try
            {
                (this.Inputs, this.Outputs, this.VariableImports, _, this.ProgramCDFG) = LoadProgram(ProgramName, fileContent);
                this.IsValidProgram = true;
            }
            catch (Exception e)
            {
                this.IsValidProgram = false;
                Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        private InlineProgramInfo GetInlineProgramInfo(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            XmlNode mutatorNode = node.TryGetNodeWithName("mutation");

            string inputCountString = mutatorNode.TryGetAttributeValue(INPUT_COUNT_ATTRIBUTE_NAME);
            string outputCountString = mutatorNode.TryGetAttributeValue(OUTPUT_COUNT_ATTRIBUTE_NAME);
            string variableCountString = mutatorNode.TryGetAttributeValue(VARIABLE_COUNT_ATTRIBUTE_NAME);

            int inputCount = int.Parse(inputCountString ?? "0");
            int outputCunt = int.Parse(outputCountString ?? "0");
            int variableCount = int.Parse(variableCountString ?? "0");

            if (inputCount != Inputs.Length ||
                outputCunt != Outputs.Length ||
                variableCount != VariableImports.Length)
            {
                throw new InternalParseException($"Actual argument count doesn't match expected argument count when loading the program: {ProgramName}");
            }

            DFG<Block> dfg = new DFG<Block>();
            InlineProgramInfo info = new InlineProgramInfo();
            for (int i = 0; i < Inputs.Length; i++)
            {
                XmlNode inputNode = node.GetInnerBlockNode(GetInputFieldName(i), parserInfo, new MissingBlockException(id, $"Input {Inputs[i]} is missing a fluid block."));
                if (inputNode != null)
                {
                    FluidInput input = XmlParser.ParseFluidInput(inputNode, dfg, parserInfo);
                    info.InputsFromTo.Add(Inputs[i], input);
                }
            }
            for (int i = 0; i < Outputs.Length; i++)
            {
                string toName = node.GetNodeWithAttributeValue(GetOutputFieldName(i)).InnerText;
                info.OutputsFromTo.Add(Outputs[i], toName);
            }
            for (int i = 0; i < VariableImports.Length; i++)
            {
                XmlNode variableNode = node.GetInnerBlockNode(GetVariableFieldName(i), parserInfo, new MissingBlockException(id, ""));
                if (variableNode != null)
                {
                    VariableBlock varBlock = (VariableBlock)XmlParser.ParseBlock(variableNode, dfg, parserInfo, false, false);
                    info.VariablesFromTo.Add(VariableImports[i], varBlock);
                }
            }

            return info;
        }

        public static string GetProgramName(XmlNode node, string id)
        {
            XmlNode mutatorNode = node.TryGetNodeWithName("mutation");
            if (mutatorNode == null)
            {
                throw new InternalParseException(id, "No mutator was found in the inline program.");
            }

            return mutatorNode.TryGetAttributeValue(PROGRAM_NAME_ATTRIBUTE_NAME);
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
            return LoadProgram(programName, programXml);
        }

        public static (string[] inputs, string[] outputs, string[] variableImports, string programXml, CDFG cdfg) LoadProgram(string programName, string programXml)
        {
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

        private void TransformCDFGToFunctionCDFG(CDFG toTransform, InlineProgramInfo programInfo)
        {
            for (int i = 0; i < toTransform.Nodes.Count; i++)
            {
                TransformDFGToFunctionDFG(toTransform.Nodes[i].dfg, programInfo);
            }
        }

        private void TransformDFGToFunctionDFG(DFG<Block> dfg, InlineProgramInfo programInfo)
        {
            //New blocks are crerated which requires new dependencies
            //and dependencies are created when they are inserted into
            //the dfg, so a new dfg is created to create the correct
            //dependencies.
            //The given dfg is still used as the corrected result is then
            //copied into the given dfg.
            DFG<Block> correctOrder = new DFG<Block>();
            Dictionary<string, string> namesToReplace = new Dictionary<string, string>();
            programInfo.InputsFromTo.ForEach(x => namesToReplace.Add(x.Key, x.Value.OriginalFluidName));

            foreach (Node<Block> node in dfg.Nodes)
            {
                Block block = node.value;

                foreach (FluidInput input in block.InputFluids)
                {
                    if (namesToReplace.ContainsKey(input.OriginalFluidName))
                    {
                        input.OriginalFluidName = namesToReplace[input.OriginalFluidName];
                    }
                }

                if (namesToReplace.ContainsKey(block.OutputVariable))
                {
                    namesToReplace.Remove(block.OutputVariable);
                }

                if (block is VariableBlock varBlock)
                {
                    if (!varBlock.CanBeScheduled)
                    {
                        continue;
                    }
                }

                if (block is InputDeclaration)
                {
                    //string newName = block.OutputVariable;
                    //string oldName = InputsFromTo[block.OutputVariable].OriginalFluidName;
                    //correctOrder.AddNode(new FluidRef(newName, oldName));
                }
                else if (block is OutputDeclaration output)
                {
                    string name = programInfo.OutputsFromTo[output.ModuleName];
                    correctOrder.AddNode(new Fluid(new List<FluidInput>() { new BasicInput("", name, 0, true) }, name, ""));
                }
                else if (//block is WasteDeclaration ||
                         block is HeaterDeclaration /*||
                         block is SensorDeclaration*/)
                {
                    //remove these blocks which is the same as not adding them
                }
                else if (block is OutputUsage outputUsage)
                {
                    List<FluidInput> inputs = new List<FluidInput>()
                    {
                        block.InputFluids[0].TrueCopy(correctOrder),
                        block.InputFluids[0].TrueCopy(correctOrder)
                    };

                    inputs[1].OriginalFluidName = programInfo.OutputsFromTo[outputUsage.ModuleName];
                    inputs[1].UseAllFluid = true;

                    correctOrder.AddNode(new Union(inputs, programInfo.OutputsFromTo[outputUsage.ModuleName], block.BlockID));
                }
                else if (block is ImportVariable import)
                {
                    VariableBlock asdqwd = (VariableBlock)programInfo.VariablesFromTo[import.VariableName].TrueCopy(correctOrder);

                    correctOrder.AddNode(asdqwd);

                    correctOrder.AddNode(new SetNumberVariable(asdqwd, import.VariableName, block.BlockID));
                }
                else
                {
                    List<Block> blocks = block.GetBlockTreeList(new List<Block>());
                    blocks.Reverse();
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


        public Direct GetProgram(XmlNode currentProgramXml, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(currentProgramXml);
            InlineProgramInfo programInfo = GetInlineProgramInfo(currentProgramXml, parserInfo);
            CDFG newProgram = ProgramCDFG.Copy();

            TransformCDFGToFunctionCDFG(newProgram, programInfo);
            TransformVariableNames(newProgram, programInfo, parserInfo.GetUniquePostFix());
            ChangeIDs(newProgram, id);

            //Add new variables that this program added
            programInfo.OutputsFromTo.ForEach(x => parserInfo.AddVariable(string.Empty, VariableType.FLUID, x.Value));
            DFG<Block> nextDFG = XmlParser.ParseNextDFG(currentProgramXml, parserInfo);



            DFG<Block> endDFG = newProgram.GetEndDFGInFirstScope();
            int i = newProgram.Nodes.FindIndex(x => x.dfg == endDFG);
            if (newProgram.Nodes[i].control == null)
            {
                newProgram.Nodes[i] = (new Direct(nextDFG), endDFG);
            }
            else
            {
                newProgram.Nodes[i] = (newProgram.Nodes[i].control.GetNewControlWithNewEnd(nextDFG), endDFG);
            }

            //merge the programs together nd return the link between then
            parserInfo.cdfg.AddCDFG(newProgram);

            return new Direct(newProgram.StartDFG);
        }

        private void TransformVariableNames(CDFG cdfg, InlineProgramInfo programInfo, string postfix)
        {
            Stack<IEnumerator<DFG<Block>>> stack = new Stack<IEnumerator<DFG<Block>>>();
            HashSet<string> readerBlacklist = new HashSet<string>();
            HashSet<string> writerBlacklist = new HashSet<string>();

            programInfo.InputsFromTo.ForEach(x => readerBlacklist.Add(x.Value.OriginalFluidName));
            programInfo.VariablesFromTo.ForEach(x => GetVariableBlockDependencies(x.Value.GetVariableTreeList(new List<VariableBlock>())).ForEach(y => readerBlacklist.Add(y)));
            programInfo.VariablesFromTo.ForEach(x => readerBlacklist.Add(x.Key));
            programInfo.OutputsFromTo.ForEach(x => writerBlacklist.Add(x.Value));

            DFG<Block> currentDFG = cdfg.StartDFG;
            do
            {
                foreach (Node<Block> node in currentDFG.Nodes)
                {
                    Block block = node.value;
                    foreach (FluidInput fluidInput in block.InputFluids)
                    {
                        if (!readerBlacklist.Contains(fluidInput.OriginalFluidName) &&
                            !writerBlacklist.Contains(fluidInput.OriginalFluidName))
                        {
                            fluidInput.OriginalFluidName += postfix;
                        }
                    }

                    for (int i = 0; i < block.InputNumbers.Count; i++)
                    {
                        if (!readerBlacklist.Contains(block.InputNumbers[i]))
                        {
                            block.InputNumbers[i] = block.InputNumbers[i] + postfix;
                        }
                    }

                    if (readerBlacklist.Contains(block.OutputVariable))
                    {
                        readerBlacklist.Remove(block.OutputVariable);
                    }
                    if (!writerBlacklist.Contains(block.OutputVariable))
                    {
                        block.OutputVariable += postfix;
                    }
                }

                IControlBlock control = cdfg.DfgToControl[currentDFG];
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