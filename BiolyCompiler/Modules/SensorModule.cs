using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;

namespace BiolyCompiler.Modules
{
    public class SensorModule : Module
    {

        public SensorModule() : base(3, 3, 3000,true){

        }

        public override Module GetCopyOf()
        {
            SensorModule sensor = new SensorModule();
            sensor.Shape = new Rectangle(Shape);
            return sensor;
        }

        protected override List<Command> GetModuleCommands(ref int time)
        {
            return new List<Command>();
        }
    }
}
