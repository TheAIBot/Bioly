using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.Commands;

namespace BiolyCompiler.Modules
{
    public class HeaterModule : Module
    {
        public int temperature { get; private set; } = 25; //In celcius
        public bool IsInUse = false;

        public HeaterModule() : base(Droplet.DROPLET_WIDTH*1, Droplet.DROPLET_HEIGHT, 1, true)
        {
        }        
        
        public override Module GetCopyOf()
        {
            return new HeaterModule();
        }

        public void setHeatingTemperatureAndDuration(int temperature, int heatingDuration)
        {
            this.temperature = temperature;
            this.OperationTime = heatingDuration;
        }

        public override bool IsStaticModule()
        {
            return true;
        }

        public override List<Command> GetModuleCommands(ref int time)
        {
            time += OperationTime;
            return new List<Command>() { new Command(InputLayout.Droplets[0].Shape.x, InputLayout.Droplets[0].Shape.y, CommandType.ELECTRODE_OFF, time) };
        }
    }
}
