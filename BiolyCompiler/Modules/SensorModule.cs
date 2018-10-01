using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;

namespace BiolyCompiler.Modules
{
    public class SensorModule : Module
    {

        public SensorModule() : base(3, 3, 3000,true){

        }

        public override List<Command> GetModuleCommands(ref int time)
        {
            return new List<Command>();
        }
    }
}
