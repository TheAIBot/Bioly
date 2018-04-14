using System;
using System.Collections.Generic;
using System.Linq;
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
            int middleOfComponentYValue = Shape.y + Droplet.DROPLET_HEIGHT / 2;
            int leftDropletInitialXPosition = Shape.x + 1;
            int rightDropletInitialXPosition = Shape.x + Shape.width - 2;
            int time = 0;

            //Moving the two droplets together to the right:
            commands.AddRange(MoveDropletsToTheRight(middleOfComponentYValue, leftDropletInitialXPosition, rightDropletInitialXPosition, ref time));
            
            //The merged droplet is now at the right side. It needs to be moved back and forth:
            int numberOfCommandsToMergeDroplet = 6;
            int numberOfCommandsToMoveBackAndForth = 8;
            int numberOfForwardBackwardMovements = (OperationTime - numberOfCommandsToMergeDroplet * 2) / numberOfCommandsToMoveBackAndForth;
            for (int i = 0; i < numberOfForwardBackwardMovements; i++)
            {
                commands.AddRange(MoveDropletsToTheLeft(middleOfComponentYValue, leftDropletInitialXPosition, rightDropletInitialXPosition, ref time));
                commands.AddRange(MoveDropletsToTheRight(middleOfComponentYValue, leftDropletInitialXPosition, rightDropletInitialXPosition, ref time));
            }

            //Splitting the droplets:
            commands.Add(new Command(rightDropletInitialXPosition - 1, middleOfComponentYValue, CommandType.ELECTRODE_ON , time));
            time++;
            commands.Add(new Command(commands.Last().X, commands.Last().Y, CommandType.ELECTRODE_OFF, time));
            time++;

            commands.Add(new Command(rightDropletInitialXPosition    , middleOfComponentYValue, CommandType.ELECTRODE_ON, time));
            commands.Add(new Command(rightDropletInitialXPosition - 2, middleOfComponentYValue, CommandType.ELECTRODE_ON, time));
            time++;

            commands.Add(new Command(rightDropletInitialXPosition    , middleOfComponentYValue, CommandType.ELECTRODE_OFF, time));
            commands.Add(new Command(rightDropletInitialXPosition - 2, middleOfComponentYValue, CommandType.ELECTRODE_OFF, time));
            time++;

            commands.Add(new Command(leftDropletInitialXPosition, middleOfComponentYValue, CommandType.ELECTRODE_ON , time));
            time++;
            commands.Add(new Command(commands.Last().X, commands.Last().Y, CommandType.ELECTRODE_OFF, time));

            return commands;
        }

        private List<Command> MoveDropletsToTheRight(int middleOfComponentYValue, int leftDropletInitialXPosition, int rightDropletInitialXPosition, ref int time)
        {
            List<Command> commands = new List<Command>();
            int xPos = leftDropletInitialXPosition;
            do
            {
                xPos++;
                commands.Add(new Command(xPos, middleOfComponentYValue, CommandType.ELECTRODE_ON, time));
                time++;
                if (commands.Count > 0)
                {
                    commands.Add(new Command(commands.Last().X, commands.Last().Y, CommandType.ELECTRODE_OFF, time));
                }
            } while (xPos != rightDropletInitialXPosition);
            return commands;
        }

        private List<Command> MoveDropletsToTheLeft(int middleOfComponentYValue, int leftDropletInitialXPosition, int rightDropletInitialXPosition, ref int time)
        {
            List<Command> commands = new List<Command>();
            int xPos = rightDropletInitialXPosition;
            do
            {
                xPos--;
                commands.Add(new Command(xPos, middleOfComponentYValue, CommandType.ELECTRODE_ON, time));
                time++;
                if (commands.Count > 0)
                {
                    commands.Add(new Command(commands.Last().X, commands.Last().Y, CommandType.ELECTRODE_OFF, time));
                }
            } while (xPos != leftDropletInitialXPosition);
            return commands;
        }
    }
}
