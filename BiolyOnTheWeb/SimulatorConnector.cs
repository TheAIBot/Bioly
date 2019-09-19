using BiolyCompiler.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BiolyCompiler.Modules;
using System.Globalization;
using System.Diagnostics;
using System.Collections.Concurrent;
using BiolyCompiler.Exceptions;
using Microsoft.JSInterop;

namespace BiolyOnTheWeb
{
    class SimulatorConnector : CommandExecutor<string>, IDisposable
    {
        private readonly int Width;
        private readonly int Height;
        private readonly Random Rando = new Random(237842);
        private bool REALLY_SLOW_COMPUTER = false;
        private BlockingCollection<string> PortStrings = new BlockingCollection<string>(new ConcurrentQueue<string>());
        private List<string> InputNames;
        private readonly IJSRuntime JSExecutor;


        public SimulatorConnector(IJSRuntime jsExe, int width, int height)
        {
            this.JSExecutor = jsExe;

            //need these commands to start the high voltage things
            PortStrings.Add("shv 1 290\r");
            PortStrings.Add("hvpoe 1 1\r");
            PortStrings.Add("clra\r");

            Width = width;
            Height = height;
        }

        public override async void StartExecutor(List<string> inputNames, List<Module> inputs, List<Module> outputs, List<Module> otherStaticModules, bool[] usedElectrodes)
        {
            this.InputNames = inputNames;

            StringBuilder inputBuilder = new StringBuilder();
            foreach (Module input in inputs)
            {
                (int centerX, int centerY) = input.Shape.getCenterPosition();
                int electrodeIndex = centerY * Width + centerX;
                string hue = Rando.NextDouble().ToString("N3", CultureInfo.InvariantCulture);
                inputBuilder.Append($"{{index: {electrodeIndex}, color: {hue}}},");
            }
            string inputString = inputBuilder.ToString();

            StringBuilder outputBuilder = new StringBuilder();
            foreach (Module output in outputs)
            {
                (int centerX, int centerY) = output.Shape.getCenterPosition();
                int electrodeIndex = centerY * Width + centerX;
                outputBuilder.Append($"{{index: {electrodeIndex}}},");
            }
            string outputString = outputBuilder.ToString();

            if (REALLY_SLOW_COMPUTER)
            {
                Debug.WriteLine("function sleep(ms) { return new Promise(resolve => setTimeout(resolve, ms));}");
                Debug.WriteLine("(async function() {");
            }

            await JSExecutor.InvokeAsync<object>("startSimulator", Width, Height, $"[{inputString}]", $"[{outputString}]", usedElectrodes);
            //ExecuteJs($"startSimulator({Width}, {Height}, [{inputString}], [{outputString}], {usedElectrodesString});");

            for (int i = 0; i < inputs.Count; i++)
            {
                QueueCommand(new AreaCommand(inputs[i].Shape, CommandType.SHOW_AREA, 0));
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                QueueCommand(new AreaCommand(outputs[i].Shape, CommandType.SHOW_AREA, 0));
            }

            for (int i = 0; i < otherStaticModules.Count; i++)
            {
                QueueCommand(new AreaCommand(otherStaticModules[i].Shape, CommandType.SHOW_AREA, 0));
            }
            SendCommands();

            if (REALLY_SLOW_COMPUTER)
            {
                Debug.WriteLine("})();");
            }
        }

        private void QueueCommand(Command command) => QueueCommands(new List<Command>() { command });

        public override void QueueCommands(List<Command> commands)
        {
            QueuedCommands.Add(ConvertCommand(commands));
        }

        public override async void SendCommands()
        {
            //compiler currently doesn't convert string[] correctly to a js string[] so make a string
            //instead and then eval it on the js side to get the string[]
            await JSExecutor.InvokeAsync<object>("addCommands", $"[{string.Join(",", QueuedCommands.Select(x => $"\"{x}\""))}]");

            QueuedCommands.Clear();
        }

        private void ExecuteJs(string js)
        {
            //Debug.WriteLine(js);
            
            if (REALLY_SLOW_COMPUTER)
            {
                //Debug.WriteLine("await sleep(500);");
            }
            try
            {
                //await Browser.GetMainFrame().EvaluateScriptAsync(js, null);
            }
            catch (Exception) { }

        }

        public override V WaitForResponse<V>()
        {
            throw new NotImplementedException("Doesn't supported feedback from board.");
        }

        public override void UpdateDropletData(List<Dictionary<string, float>> dropsConcentrations)
        {
            string inputNamesAsString = $"[{String.Join(", ", InputNames.Select(x => "\"" + x.Split('#')[0] + "\""))}]";

            List<string> dropsConcentrationsStrings = new List<string>();
            foreach (Dictionary<string, float> concentrationInfo in dropsConcentrations)
            {
                float[] concentrations = new float[InputNames.Count];
                int i = 0;
                foreach (string inputName in InputNames)
                {
                    concentrationInfo.TryGetValue(inputName, out float concentration);
                    concentrations[i] = concentration;
                    i++;
                }

                dropsConcentrationsStrings.Add($"[{String.Join(", ", concentrations.Select(x => x.ToString(CultureInfo.InvariantCulture)))}]");
            }

            string dropletsInfo = $"[{String.Join(", ", dropsConcentrationsStrings)}]";

            ExecuteJs($"ShowDropletsInformation({inputNamesAsString}, {dropletsInfo});");
        }

        protected override string ConvertCommand(List<Command> commands)
        {
            AreaCommand areaCommand = commands.First() as AreaCommand;
            switch (commands.First().Type)
            {
                case CommandType.ELECTRODE_ON:

                    {
                        PortStrings.Add($"setel {String.Join(" ", commands.Select(x => ConvertElectrodeIndex(x.X, x.Y, 8, 16)))}\r");
                        return $"setel {String.Join(" ", commands.Select(x => x.Y * Width + x.X + 1))}";
                    }

                case CommandType.ELECTRODE_OFF:
                    {
                        PortStrings.Add($"clrel {String.Join(" ", commands.Select(x => ConvertElectrodeIndex(x.X, x.Y, 8, 16)))}\r");
                        return $"clrel {String.Join(" ", commands.Select(x => x.Y * Width + x.X + 1))}";
                    }
                case CommandType.SHOW_AREA:
                    return $"show_area {areaCommand.ID} {areaCommand.X} {areaCommand.Y} {areaCommand.Width} {areaCommand.Height} {areaCommand.R.ToString("N3", CultureInfo.InvariantCulture)} {areaCommand.G.ToString("N3", CultureInfo.InvariantCulture)} {areaCommand.B.ToString("N3", CultureInfo.InvariantCulture)}";
                case CommandType.REMOVE_AREA:
                    return $"remove_area {areaCommand.ID}";
                default:
                    throw new InternalRuntimeException($"Can't convert command type {commands.First().Type.ToString()}");
            }
        }

        private int ConvertElectrodeIndex(int col, int row, int width, int height)
        {
            if (col > 3)
            {
                row = height - row - 1;
            }
            return ((col / 4) * height * 4) + (col % 4) + (row * 4) + 1;
        }

        public void Dispose()
        {
        }
    }
}
