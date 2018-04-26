using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Commands
{
    public abstract class CommandExecutor<T>
    {
        protected readonly List<string> QueuedCommands = new List<string>();

        public abstract void StartExecutor(List<Module> inputs, List<Module> outputs);
        public abstract void QueueCommands(List<Command> commands);
        public abstract void SendCommands();
        public abstract V WaitForResponse<V>();
        protected abstract T ConvertCommand(List<Command> commands);
    }
}
