using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Graphs;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class Mixer : FluidBlock
    {
        public const string FirstInputFieldName = "inputFluidA";
        public const string SecondInputFieldName = "inputFluidB";
        public const string XmlTypeName = "mixer";

        public Mixer(List<FluidInput> input, string output, string id) : base(true, input, null, output, id)
        {

        }

        public static Block CreateMixer(string output, XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);

            XmlNode inputFluidNode1 = node.GetInnerBlockNode(FirstInputFieldName , parserInfo, new MissingBlockException(id, "Mixer is missing input fluid block."));
            XmlNode inputFluidNode2 = node.GetInnerBlockNode(SecondInputFieldName, parserInfo, new MissingBlockException(id, "Mixer is missing input fluid block."));

            FluidInput fluidInput1 = null;
            FluidInput fluidInput2 = null;

            if (inputFluidNode1 != null)
            {
                fluidInput1 = XmlParser.ParseFluidInput(inputFluidNode1, dfg, parserInfo);
            }
            if (inputFluidNode2 != null)
            {
                fluidInput2 = XmlParser.ParseFluidInput(inputFluidNode2, dfg, parserInfo);
            }

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput1);
            inputs.Add(fluidInput2);

            return new Mixer(inputs, output, id);
        }

        public override string ToString()
        {
            return "Mixer";
        }

        public override Module getAssociatedModule()
        {
            return new MixerModule(30);
        }
    }
}
