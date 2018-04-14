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
            int middleOfComponentYValue = Shape.y + Droplet.DROPLET_HEIGHT /2;
            int leftDropletInitialXPosition = Shape.x + 1;
            int rightDropletInitialXPosition = Shape.x + Droplet.DROPLET_WIDTH + 1;

            //Moving the two droplets together to the right:
            commands.AddRange(moveDropletsToTheRight(middleOfComponentYValue, leftDropletInitialXPosition, rightDropletInitialXPosition));
            
            //The merged droplet is now at the right side. It needs to be moved back and forth:
            int numberOfCommandsToMergeDroplet = 6;
            int numberOfCommandsToMoveBackAndForth = 8;
            int numberOfForwardBackwardMovements = (OperationTime - numberOfCommandsToMergeDroplet * 2) / numberOfCommandsToMoveBackAndForth;
            for (int i = 0; i < numberOfForwardBackwardMovements; i++)
            {
                commands.AddRange(moveDropletsToTheLeft(middleOfComponentYValue, leftDropletInitialXPosition, rightDropletInitialXPosition));
                commands.AddRange(moveDropletsToTheRight(middleOfComponentYValue, leftDropletInitialXPosition, rightDropletInitialXPosition));
            }

            //Splitting the droplets:
            commands.Add(new Command(rightDropletInitialXPosition - 1, middleOfComponentYValue, CommandType.ELECTRODE_ON, 0));
            commands.Add(new Command(rightDropletInitialXPosition - 1, middleOfComponentYValue, CommandType.ELECTRODE_OFF, 0));

            commands.Add(new Command(rightDropletInitialXPosition, middleOfComponentYValue, CommandType.ELECTRODE_ON, 0));
            commands.Add(new Command(rightDropletInitialXPosition - 2, middleOfComponentYValue, CommandType.ELECTRODE_ON, 0));

            commands.Add(new Command(rightDropletInitialXPosition, middleOfComponentYValue, CommandType.ELECTRODE_OFF, 0));
            commands.Add(new Command(rightDropletInitialXPosition - 2, middleOfComponentYValue, CommandType.ELECTRODE_OFF, 0));

            commands.Add(new Command(leftDropletInitialXPosition, middleOfComponentYValue, CommandType.ELECTRODE_ON, 0));
            commands.Add(new Command(leftDropletInitialXPosition, middleOfComponentYValue, CommandType.ELECTRODE_OFF, 0));

            return commands;
        }

        private List<Command> moveDropletsToTheRight(int middleOfComponentYValue, int leftDropletInitialXPosition, int rightDropletInitialXPosition)
        {
            List<Command> commands = new List<Command>();
            for (int i = 1; i <= (rightDropletInitialXPosition - leftDropletInitialXPosition); i++)
            {
                commands.Add(new Command(leftDropletInitialXPosition + i, middleOfComponentYValue, CommandType.ELECTRODE_ON, 0));
                commands.Add(new Command(leftDropletInitialXPosition + i, middleOfComponentYValue, CommandType.ELECTRODE_OFF, 0));
            }
            return commands;
        }

        private List<Command> moveDropletsToTheLeft(int middleOfComponentYValue, int leftDropletInitialXPosition, int rightDropletInitialXPosition)
        {
            List<Command> commands = new List<Command>();
            for (int i = 1; i <= (rightDropletInitialXPosition - leftDropletInitialXPosition); i++)
            {
                commands.Add(new Command(rightDropletInitialXPosition - i, middleOfComponentYValue, CommandType.ELECTRODE_ON, 0));
                commands.Add(new Command(rightDropletInitialXPosition - i, middleOfComponentYValue, CommandType.ELECTRODE_OFF, 0));
            }
            return commands;
        }
    }
}
