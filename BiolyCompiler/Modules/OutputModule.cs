using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.Commands;

namespace BiolyCompiler.Modules
{
    public class OutputModule : Module
    {

        public OutputModule() : base(3, 3, 1, true)
        {
            OperationTime = 2;
            InputLayout = new InfiniteModuleLayout(InputLayout.width, InputLayout.height, InputLayout.EmptyRectangles, InputLayout.Droplets);
        }
        
        
        public override Module GetCopyOf()
        {
            return new OutputModule();
        }


        public override bool IsStaticModule()
        {
            return true;
        }

        protected override List<Command> GetModuleCommands(ref int time)
        {
            time += 2;
            return new List<Command>() { new Command(InputLayout.Droplets[0].Shape.x, InputLayout.Droplets[0].Shape.y, CommandType.ELECTRODE_OFF, time) };
        }
    }
}
