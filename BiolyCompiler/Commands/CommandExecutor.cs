using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Commands
{
    public abstract class CommandExecutor<T>
    {
        public abstract void StartExecutor(List<Module> inputs, List<Module> outputs);
        public abstract void SendCommands(List<Command> commands);
        public abstract V WaitForResponse<V>();
        protected abstract T ConvertCommand(List<Command> commands);
    }
}
