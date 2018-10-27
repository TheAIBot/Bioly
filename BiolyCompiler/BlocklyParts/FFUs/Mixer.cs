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
        public const int OPERATION_TIME = 100;

        public Mixer(List<FluidInput> input, string output, string id) : base(true, input, null, output, id)
        {

        }

        public static Block CreateMixer(string output, XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);

            FluidInput fluidInput1 = ParseTools.ParseFluidInput(node, dfg, parserInfo, id, FirstInputFieldName,
                                     new MissingBlockException(id, "Mixer is missing input fluid block."));
            FluidInput fluidInput2 = ParseTools.ParseFluidInput(node, dfg, parserInfo, id, SecondInputFieldName,
                                     new MissingBlockException(id, "Mixer is missing input fluid block."));

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput1);
            inputs.Add(fluidInput2);

            return new Mixer(inputs, output, id);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            return new Mixer(InputFluids.Copy(dfg), OutputVariable, BlockID);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix)
        {
            List<FluidInput> inputFluids = new List<FluidInput>();
            InputFluids.ToList().ForEach(x => inputFluids.Add(x.CopyInput(dfg, renamer, namePostfix)));

            if (renamer.ContainsKey(OutputVariable))
            {
                renamer[OutputVariable] = OutputVariable + namePostfix;
            }
            else
            {
                renamer.Add(OutputVariable, OutputVariable + namePostfix);
            }
            return new Mixer(inputFluids, OutputVariable + namePostfix, BlockID);
        }

        public override string ToString()
        {
            return $"Mixer: {OutputVariable}";
        }

        public override Module getAssociatedModule()
        {
            return new MixerModule(OPERATION_TIME);
        }

        public override void UpdateInternalDropletConcentrations()
        {
            List<Route> allRoutes = new List<Route>();
            InputRoutes.Select(pair => pair.Value).ForEach(listOfRoutes => allRoutes.AddRange(listOfRoutes));
            if (allRoutes.Count != 2) throw new NotImplementedException("Currently only mixing two droplets is supported.");
            Dictionary<string, float> dropletConcentrations1  = allRoutes[0].routedDroplet.GetFluidConcentrations();
            Dictionary<string, float> dropletConcentrations2  = allRoutes[1].routedDroplet.GetFluidConcentrations();

            HashSet<string> allFluidParts = dropletConcentrations1.Keys.Union(dropletConcentrations2.Keys).ToHashSet();
            foreach (var fluidName in allFluidParts)
            {
                dropletConcentrations1.TryGetValue(fluidName, out float concentration1);
                dropletConcentrations2.TryGetValue(fluidName, out float concentration2);

                float sumOfConcetrations = concentration1 + concentration2;

                BoundModule.GetOutputLayout().Droplets[0].FluidConcentrations[fluidName] = sumOfConcetrations / 2;
                BoundModule.GetOutputLayout().Droplets[1].FluidConcentrations[fluidName] = sumOfConcetrations / 2;
            }

        }

        public override List<Block> GetBlockTreeList(List<Block> blocks)
        {
            blocks.Add(this);
            return blocks;
        }
    }
}
