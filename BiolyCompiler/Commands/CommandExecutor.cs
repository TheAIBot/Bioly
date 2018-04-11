using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Commands
{
    public abstract class CommandExecutor<T>
    {
        protected abstract void StartExecutor();
        public abstract void SendCommand(Command command);
        public abstract V WaitForResponse<V>();
        protected abstract T ConvertCommand(Command command);
    }
}
