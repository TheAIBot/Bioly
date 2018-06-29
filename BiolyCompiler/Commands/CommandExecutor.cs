using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Commands
{
    public abstract class CommandExecutor<T>
    {
        protected readonly List<string> QueuedCommands = new List<string>();

        public abstract void StartExecutor(List<string> inputNames, List<Module> inputs, List<Module> outputs, List<Module> otherStaticModules, bool[] usedElectrodes);
        public abstract void QueueCommands(List<Command> commands);
        public abstract void SendCommands();
        public abstract V WaitForResponse<V>();
        public abstract void UpdateDropletData(List<Dictionary<string, float>> dropsConcentrations);
        protected abstract T ConvertCommand(List<Command> commands);
    }
}
