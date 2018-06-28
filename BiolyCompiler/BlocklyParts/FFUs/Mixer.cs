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
using System.Linq;
using BiolyCompiler.Routing;
using MoreLinq;

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

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> mostRecentRef, Dictionary<string, string> renamer, string namePostfix)
        {
            List<FluidInput> inputFluids = new List<FluidInput>();
            InputFluids.ToList().ForEach(x => inputFluids.Add(x.CopyInput(dfg, mostRecentRef, renamer, namePostfix)));

            if (renamer.ContainsKey(OriginalOutputVariable))
            {
                renamer[OriginalOutputVariable] = OriginalOutputVariable + namePostfix;
            }
            else
            {
                renamer.Add(OriginalOutputVariable, OriginalOutputVariable + namePostfix);
            }
            return new Mixer(inputFluids, OriginalOutputVariable + namePostfix, BlockID);
        }

        public override string ToString()
        {
            return "Mixer";
        }

        public override Module getAssociatedModule()
        {
            return new MixerModule(200);
        }

        public override void UpdateInternalDropletConcentrations()
        {
            List<Route> allRoutes = new List<Route>();
            InputRoutes.Select(pair => pair.Value).ForEach(listOfRoutes => allRoutes.AddRange(listOfRoutes));
            if (allRoutes.Count != 2) throw new NotImplementedException("Currently only mixing two droplets is supported.");
            IDropletSource concistingDroplet1  = allRoutes[0].routedDroplet;
            IDropletSource concistingDroplet2  = allRoutes[1].routedDroplet;

            HashSet<string> allFluidParts = concistingDroplet1.GetFluidConcentrations().Keys.ToHashSet();
            allFluidParts.UnionWith(concistingDroplet2.GetFluidConcentrations().Keys);
            foreach (var fluidName in allFluidParts)
            {
                double sumOfConcetrations = 0;
                if (concistingDroplet1.GetFluidConcentrations().TryGetValue(fluidName, out double concentration1))
                    sumOfConcetrations += concentration1;
                if (concistingDroplet2.GetFluidConcentrations().TryGetValue(fluidName, out double concentration2))
                    sumOfConcetrations += concentration2;
                BoundModule.GetOutputLayout().Droplets[0].FluidConcentrations[fluidName] = sumOfConcetrations / 2;
                BoundModule.GetOutputLayout().Droplets[1].FluidConcentrations[fluidName] = sumOfConcetrations / 2;
            }

        }
    }
}
