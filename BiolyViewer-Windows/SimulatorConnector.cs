using BiolyCompiler.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using BiolyCompiler.Modules;
using System.Globalization;
using System.Diagnostics;
using CefSharp.WinForms;
using System.IO.Ports;
using System.Threading;
using System.Collections.Concurrent;

namespace BiolyViewer_Windows
{
    class SimulatorConnector : CommandExecutor<string>, IDisposable
    {
        private readonly ChromiumWebBrowser Browser;
        private readonly int Width;
        private readonly int Height;
        private readonly Random Rando = new Random(237842);
        private bool REALLY_SLOW_COMPUTER = false;
        private BlockingCollection<string> PortStrings = new BlockingCollection<string>(new ConcurrentQueue<string>());
        private SerialPort Port;
        private Thread SerialSendThread;


        public SimulatorConnector(ChromiumWebBrowser browser, int width, int height)
        {
            //need these commands to start the high voltage things
            PortStrings.Add("shv 1 290\r");
            PortStrings.Add("hvpoe 1 1\r");
            PortStrings.Add("clra\r");

            Port = new SerialPort("COM3", 115200);
            Port.Open();
            SerialSendThread = new Thread(() => SendCommandsToSerialPort());
            SerialSendThread.Start();
            Browser = browser;
            Width = width;
            Height = height;
        }

        private void SendCommandsToSerialPort()
        {
            try
            {
                while (true)
                {
                    string command = PortStrings.Take();

                    byte[] bytes = Encoding.ASCII.GetBytes(command);
                    Port.Write(bytes, 0, bytes.Length);
                    Debug.WriteLine(command);
                    Thread.Sleep(500);
                    //Debug.WriteLine(Port.ReadLine());
                }
            }
            catch (Exception)
            {
            }
        }

        public override void StartExecutor(List<Module> inputs, List<Module> outputs, List<Module> otherStaticModules)
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
            QueuedCommands.Add($"addCommand(\"{ConvertCommand(commands)}\");");
        }

        public override void SendCommands()
        {
            if (QueuedCommands.Count > 0)
            {
                ExecuteJs(String.Join(String.Empty, QueuedCommands));
                QueuedCommands.Clear();
            }
        }

        private async void ExecuteJs(string js)
        {
            Debug.WriteLine(js);
            
            if (REALLY_SLOW_COMPUTER)
            {
                //Debug.WriteLine("await sleep(500);");
            }
            try
            {
                await Browser.GetMainFrame().EvaluateScriptAsync(js, null);
            }
            catch (Exception) { }

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

                    {
                        PortStrings.Add($"setel {String.Join(" ", commands.Select(x => ConvertElectrodeIndex(x.X, x.Y, Width, Height)))}\r");
                        return $"setel {String.Join(" ", commands.Select(x => x.Y * Width + x.X + 1))}";
                    }

                case CommandType.ELECTRODE_OFF:
                    {
                        PortStrings.Add($"clrel {String.Join(" ", commands.Select(x => ConvertElectrodeIndex(x.X, x.Y, Width, Height)))}\r");
                        return $"clrel {String.Join(" ", commands.Select(x => x.Y * Width + x.X + 1))}";
                    }
                case CommandType.SHOW_AREA:
                    return $"show_area {areaCommand.ID} {areaCommand.X} {areaCommand.Y} {areaCommand.Width} {areaCommand.Height} {areaCommand.R.ToString("N3", CultureInfo.InvariantCulture)} {areaCommand.G.ToString("N3", CultureInfo.InvariantCulture)} {areaCommand.B.ToString("N3", CultureInfo.InvariantCulture)}";
                case CommandType.REMOVE_AREA:
                    return $"remove_area {areaCommand.ID}";
                default:
                    throw new Exception($"Can't convert command type {commands.First().Type.ToString()}");
            }
        }

        private int ConvertElectrodeIndex(int col, int row, int width, int height)
        {
            return ((col / 4) * height * 4) + (col % 4) + (row * 4) + 1;
        }

        public void Dispose()
        {
            SerialSendThread.Interrupt();
            SerialSendThread.Join();

            byte[] bytes = Encoding.ASCII.GetBytes("hvpoe 1 0\r");
            Port?.Write(bytes, 0, bytes.Length);
            Port?.Close();
        }
    }
}
