using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Exceptions.ParserExceptions;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class Mixer : FluidBlock
    {
        public const string FirstInputFieldName = "inputFluidA";
        public const string SecondInputFieldName = "inputFluidB";
        public const string XmlTypeName = "mixer";

        public Mixer(List<FluidInput> input, string output, XmlNode node, string id) : base(true, input, output, id)
        {

        }

        public static Block CreateMixer(string output, XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);

            XmlNode inputFluidNode1 = node.GetInnerBlockNode(FirstInputFieldName , parserInfo, new MissingBlockException(id, "Mixer is missing input fluid block."));
            XmlNode inputFluidNode2 = node.GetInnerBlockNode(SecondInputFieldName, parserInfo, new MissingBlockException(id, "Mixer is missing input fluid block."));

            FluidInput fluidInput1 = null;
            FluidInput fluidInput2 = null;

            if (inputFluidNode1 != null)
            {
                fluidInput1 = XmlParser.GetVariablesCorrectedName(inputFluidNode1, parserInfo);
            }
            if (inputFluidNode2 != null)
            {
                fluidInput2 = XmlParser.GetVariablesCorrectedName(inputFluidNode2, parserInfo);
            }

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput1);
            inputs.Add(fluidInput2);

            return new Mixer(inputs, output, node, id);
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
