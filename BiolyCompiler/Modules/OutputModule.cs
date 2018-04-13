using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.Commands;

namespace BiolyCompiler.Modules
{
    public class OutputModule : Module
    {
        
        public OutputModule(int numberOfInputs) : base(3, 3, 1, true)
        {
            //This is so that mutliple droplets can be routed to the same output,  at the same place in the module.
            //It is a little hacky, and should be changed later, but it works:
            List<Droplet> droplets = new List<Droplet>();
            for (int i = 0; i < numberOfInputs; i++)
            {
                droplets.Add(new Droplet());
            }
            InputLayout.Droplets = droplets;
        }
        
        public override Module GetCopyOf()
        {
            return new OutputModule(InputLayout.Droplets.Count);
        }


        public override bool isStaticModule()
        {
            return true;
        }

        protected override List<Command> GetModuleCommands()
        {
            return new List<Command>();
        }
    }
}
