using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;

namespace BiolyCompiler.Modules
{
    public class MixerModule : Module
    {
        public MixerModule(int operationTime) : base(2*Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, operationTime, false)
        {
            InputLayout  = GetDefaultLayout();
            OutputLayout = GetDefaultLayout();
        }

        /*
        public MixerModule(int width, int height, int operationTime) : base(width, height, operationTime, 2, 2){
            Layout = GetBasicMixerLayout(width, height);
        }
        */

        public ModuleLayout GetDefaultLayout()
        {
            List<Rectangle> EmptyRectangles = new List<Rectangle>();
            List<Droplet> OutputLocations  = new List<Droplet>();
            Droplet output1 = new Droplet();
            Droplet output2 = new Droplet();
            output1.Shape = new Rectangle(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 0, 0);
            output2.Shape = new Rectangle(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, Droplet.DROPLET_WIDTH, 0);
            OutputLocations.Add(output1);
            OutputLocations.Add(output2);
            return new ModuleLayout(Shape, EmptyRectangles, OutputLocations);
        }
        

        public override Module GetCopyOf()
        {
            throw new NotImplementedException();
        }

        protected override List<Command> GetModuleCommands()
        {
            throw new NotImplementedException();
        }
    }
}
