using System;
using System.Collections.Generic;
using BiolyCompiler.Modules.OperationTypes;

namespace BiolyCompiler.Modules
{
    public class MixerModule : Module
    {
        public MixerModule(int operationTime) : base(2*Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, operationTime, 2, 2)
        {
            Layout = GetDefaultLayout();
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
            Droplet output1 = new Droplet(new BoardFluid(""));
            Droplet output2 = new Droplet(new BoardFluid(""));
            output1.Shape = new Rectangle(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 0, 0);
            output2.Shape = new Rectangle(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, Droplet.DROPLET_WIDTH, 0);
            return new ModuleLayout(2* Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, EmptyRectangles, OutputLocations);
        }

        public override OperationType getOperationType(){
            return OperationType.Mixer;
        }
        

        public override Module GetCopyOf()
        {
            throw new NotImplementedException();
        }
    }
}
