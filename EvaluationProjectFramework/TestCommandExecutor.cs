using BiolyCompiler.Commands;
using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiolyTests
{
    public class TestCommandExecutor : CommandExecutor<string>
    {
        public int ticks = 0;

        public override void QueueCommands(List<Command> commands)
        {
        }

        public override void SendCommands()
        {
            ticks++;
        }

        public override void StartExecutor(List<string> inputNames, List<Module> inputs, List<Module> outputs, List<Module> otherStaticModules, bool[] usedElectrodes)
        {
        }

        public override void UpdateDropletData(List<Dictionary<string, float>> dropsConcentrations)
        {
        }

        public override V WaitForResponse<V>()
        {
            return default(V);
        }

        protected override string ConvertCommand(List<Command> commands)
        {
            return null;
        }
    }
}
