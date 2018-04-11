using BiolyCompiler.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.Wpf;
using CefSharp;
using BiolyCompiler.Modules;

namespace BiolyViewer_Windows
{
    class SimulatorConnector : CommandExecutor<string>
    {
        private readonly ChromiumWebBrowser Browser;
        private readonly int Width;
        private readonly int Height;
        private readonly Random Rando = new Random(237842);

        public SimulatorConnector(ChromiumWebBrowser browser, int width, int height)
        {
            Browser = browser;
            Width = width;
            Height = height;
        }

        protected override void StartExecutor(List<DropletSpawner> spawners)
        {
            StringBuilder builder = new StringBuilder();
            spawners.ForEach(x => builder.Append($"{{index: {x.Shape.y * Width + x.Shape.x}, color: vec4({Rando.NextDouble()}, {Rando.NextDouble()}, {Rando.NextDouble()}, 0.5)}},"));
            ExecuteJs($"startSimulator({Width}, {Height}, [{builder.ToString()}], []);");
        }

        public override void SendCommand(Command command)
        {
            string commandScript = $"addCommand({ConvertCommand(command)});";
            ExecuteJs(commandScript);
        }

        private async void ExecuteJs(string js)
        {
            await Browser.GetMainFrame().EvaluateScriptAsync(js, null);
        }

        public override V WaitForResponse<V>()
        {
            throw new NotImplementedException();
        }

        protected override string ConvertCommand(Command command)
        {
            AreaCommand areaCommand = command as AreaCommand;
            switch (command.Type)
            {
                case CommandType.ELECTRODE_ON:
                    return $"setel {command.Y * Width + command.X + 1}";
                case CommandType.ELECTRODE_OFF:
                    return $"clrel {command.Y * Width + command.X + 1}";
                case CommandType.SHOW_AREA:
                    return $"show_area {areaCommand.ID} {areaCommand.X} {areaCommand.Y} {areaCommand.Width} {areaCommand.Height} {areaCommand.R} {areaCommand.G} {areaCommand.B}";
                case CommandType.REMOVE_AREA:
                    return $"remove_area {areaCommand.ID}";
                default:
                    throw new Exception($"Can't convert command type {command.Type.ToString()}");
            }
        }
    }
}
