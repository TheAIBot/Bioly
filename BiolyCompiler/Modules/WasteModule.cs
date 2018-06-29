﻿using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.Commands;

namespace BiolyCompiler.Modules
{
    public class WasteModule : Module
    {

        public WasteModule() : base(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 1, true)
        {
            OperationTime = 2;
            InputLayout = new InfiniteModuleLayout(InputLayout.width, InputLayout.height, InputLayout.EmptyRectangles, InputLayout.Droplets);
        }
        
        
        public override Module GetCopyOf()
        {
            return new WasteModule();
        }


        public override bool IsStaticModule()
        {
            return true;
        }

        public override List<Command> GetModuleCommands(ref int time)
        {
            time += OperationTime;
            return new List<Command>() { new Command(InputLayout.Droplets[0].Shape.getCenterPosition().Item1, InputLayout.Droplets[0].Shape.getCenterPosition().Item2, CommandType.ELECTRODE_OFF, time) };
        }
    }
}
