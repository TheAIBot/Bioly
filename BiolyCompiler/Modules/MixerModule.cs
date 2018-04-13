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
            MixerModule mixer  = new MixerModule(OperationTime);
            mixer.Shape = new Rectangle(this.Shape);
            mixer.InputLayout  = this.InputLayout? .GetCopy();
            mixer.OutputLayout = this.OutputLayout?.GetCopy();
            return mixer;
        }

        protected override List<Command> GetModuleCommands()
        {
            List<Command> commands = new List<Command>();
            //Moving the two droplets together:
            commands.Add(new Command(Shape.x + Droplet.DROPLET_WIDTH - 1, Shape.y + Droplet.DROPLET_HEIGHT - 2, CommandType.ELECTRODE_ON, 0));
            commands.Add(new Command(Shape.x + Droplet.DROPLET_WIDTH - 1, Shape.y + Droplet.DROPLET_HEIGHT - 2, CommandType.ELECTRODE_OFF, 0));

            commands.Add(new Command(Shape.x + Droplet.DROPLET_WIDTH, Shape.y + Droplet.DROPLET_HEIGHT - 2, CommandType.ELECTRODE_ON, 0));
            commands.Add(new Command(Shape.x + Droplet.DROPLET_WIDTH, Shape.y + Droplet.DROPLET_HEIGHT - 2, CommandType.ELECTRODE_OFF, 0));

            commands.Add(new Command(Shape.x + Droplet.DROPLET_WIDTH + 1, Shape.y + Droplet.DROPLET_HEIGHT - 2, CommandType.ELECTRODE_ON, 0));
            commands.Add(new Command(Shape.x + Droplet.DROPLET_WIDTH + 1, Shape.y + Droplet.DROPLET_HEIGHT - 2, CommandType.ELECTRODE_OFF, 0));
            return commands;
        }
    }
}
