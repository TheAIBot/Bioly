using BiolyCompiler.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.Wpf;
using CefSharp;
using BiolyCompiler.Modules;
using System.Globalization;
using System.Diagnostics;

namespace BiolyViewer_Windows
{
    class SimulatorConnector : CommandExecutor<string>
    {
        private readonly ChromiumWebBrowser Browser;
        private readonly int Width;
        private readonly int Height;
        private readonly Random Rando = new Random(237842);
        private bool REALLY_SLOW_COMPUTER = false;

        public SimulatorConnector(ChromiumWebBrowser browser, int width, int height)
        {
            Browser = browser;
            Width = width;
            Height = height;
        }

        public override void StartExecutor(List<Module> inputs, List<Module> outputs)
        {
            StringBuilder inputBuilder = new StringBuilder();
            foreach (Module input in inputs)
            {
                (int centerX, int centerY) = input.Shape.getCenterPosition();
                int electrodeIndex = centerY * Width + centerX;
                string r = Rando.NextDouble().ToString("N3", CultureInfo.InvariantCulture);
                string g = Rando.NextDouble().ToString("N3", CultureInfo.InvariantCulture);
                string b = Rando.NextDouble().ToString("N3", CultureInfo.InvariantCulture);
                inputBuilder.Append($"{{index: {electrodeIndex}, color: vec4({r}, {g}, {b}, 0.5)}},");

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

            ExecuteJs($"startSimulator({Width}, {Height}, [{inputString}], [{outputString}]);");

            for (int i = 0; i < inputs.Count; i++)
            {
                SendCommand(new AreaCommand(inputs[i].Shape, CommandType.SHOW_AREA, 0));
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                SendCommand(new AreaCommand(outputs[i].Shape, CommandType.SHOW_AREA, 0));
            }

            if (REALLY_SLOW_COMPUTER)
            {
                Debug.WriteLine("})();");
            }
        }

        public override void SendCommand(Command command) => SendCommands(new List<Command>() { command });

        public override void SendCommands(List<Command> commands)
        {
            string commandScript = $"addCommand(\"{ConvertCommand(commands)}\");";
            ExecuteJs(commandScript);
        }

        private async void ExecuteJs(string js)
        {
            Debug.WriteLine(js);
            
            if (REALLY_SLOW_COMPUTER)
            {
                //Debug.WriteLine("await sleep(500);");
            }
            await Browser.GetMainFrame().EvaluateScriptAsync(js, null);
        }

        public override V WaitForResponse<V>()
        {
            throw new NotImplementedException();
        }

        protected override string ConvertCommand(List<Command> commands)
        {
            AreaCommand areaCommand = commands.First() as AreaCommand;
            switch (commands.First().Type)
            {
                case CommandType.ELECTRODE_ON:
                    return $"setel {String.Join(" ", commands.Select(x => x.Y * Width + x.X + 1))}";
                case CommandType.ELECTRODE_OFF:
                    return $"clrel {String.Join(" ", commands.Select(x => x.Y * Width + x.X + 1))}";
                case CommandType.SHOW_AREA:
                    return $"show_area {areaCommand.ID} {areaCommand.X} {areaCommand.Y} {areaCommand.Width} {areaCommand.Height} {areaCommand.R.ToString("N3", CultureInfo.InvariantCulture)} {areaCommand.G.ToString("N3", CultureInfo.InvariantCulture)} {areaCommand.B.ToString("N3", CultureInfo.InvariantCulture)}";
                case CommandType.REMOVE_AREA:
                    return $"remove_area {areaCommand.ID}";
                default:
                    throw new Exception($"Can't convert command type {commands.First().Type.ToString()}");
            }
        }
    }
}
