using System;
using System.Collections.Generic;
using System.Linq;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;

namespace BiolyCompiler.Modules
{
    public class MixerModule : Module
    {
        public MixerModule(int operationTime) : base(2*Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, operationTime, false)
        {
            InputLayout  = GetDefaultLayout(Shape);
            OutputLayout = GetDefaultLayout(Shape);
        }

        /*
        public MixerModule(int width, int height, int operationTime) : base(width, height, operationTime, 2, 2){
            Layout = GetBasicMixerLayout(width, height);
        }
        */

        public static ModuleLayout GetDefaultLayout(Rectangle Shape)
        {
            List<Rectangle> EmptyRectangles = new List<Rectangle>();
            List<Droplet> OutputLocations  = new List<Droplet>();
            Droplet output1 = new Droplet();
            Droplet output2 = new Droplet();
            output1.Shape = new Rectangle(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 0, 0);
            output2.Shape = new Rectangle(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, Droplet.DROPLET_WIDTH, 0);
            output1.Shape.isEmpty = false;
            output2.Shape.isEmpty = false;
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

        public override List<Command> GetModuleCommands(ref int time)
        {
            int startTime = time;
            List<Command> commands = new List<Command>();
            int middleOfComponentYValue = Shape.y + Droplet.DROPLET_HEIGHT / 2;
            int leftDropletInitialXPosition = Shape.x + Droplet.DROPLET_WIDTH/2;
            int rightDropletInitialXPosition = leftDropletInitialXPosition + Droplet.DROPLET_WIDTH;

            //Moving the two droplets together to the right:
            commands.AddRange(MoveDropletsToTheRight(middleOfComponentYValue, leftDropletInitialXPosition, rightDropletInitialXPosition, ref time));
            
            //The merged droplet is now at the right side. It needs to be moved back and forth:
            int numberOfCommandsToMoveCompletlyOneDirection = 2 * Droplet.DROPLET_WIDTH + 1;
            int numberOfCommandsToMoveBackAndForth = 2*numberOfCommandsToMoveCompletlyOneDirection;
            int numberOfCommandsToMergeDroplet = 2*numberOfCommandsToMoveCompletlyOneDirection + 4;
            int numberOfForwardBackwardMovements = (OperationTime - numberOfCommandsToMergeDroplet) / numberOfCommandsToMoveBackAndForth;
            //int numberOfForwardBackwardMovements = 1;
            for (int i = 0; i < numberOfForwardBackwardMovements; i++)
            {
                commands.AddRange(MoveDropletsToTheLeft(middleOfComponentYValue, leftDropletInitialXPosition, rightDropletInitialXPosition, ref time));
                commands.AddRange(MoveDropletsToTheRight(middleOfComponentYValue, leftDropletInitialXPosition, rightDropletInitialXPosition, ref time));
            }

            //Splitting the droplets:
            commands.Add(new Command(rightDropletInitialXPosition - 1, middleOfComponentYValue, CommandType.ELECTRODE_ON , time));
            time++;
            commands.Add(new Command(rightDropletInitialXPosition - 1, middleOfComponentYValue, CommandType.ELECTRODE_OFF, time));
            time++;

            commands.Add(new Command(rightDropletInitialXPosition    , middleOfComponentYValue, CommandType.ELECTRODE_ON, time));
            commands.Add(new Command(rightDropletInitialXPosition - 2, middleOfComponentYValue, CommandType.ELECTRODE_ON, time));
            time++;

            commands.Add(new Command(rightDropletInitialXPosition    , middleOfComponentYValue, CommandType.ELECTRODE_OFF, time));
            commands.Add(new Command(rightDropletInitialXPosition - 2, middleOfComponentYValue, CommandType.ELECTRODE_OFF, time));
            time++;
            //The droplets have been split. Now the left droplet needs to be moved back to its original position.

            commands.AddRange(MoveDropletsToTheLeft(middleOfComponentYValue, leftDropletInitialXPosition, rightDropletInitialXPosition - 2, ref time));
            
            int restTime = OperationTime - (time - startTime);
            if (restTime < 0) throw new InternalRuntimeException("Remaining waiting time for mixing should not be negative: it is "  + restTime);
            time += restTime;
            commands.Add(new Command(commands.Last().X, commands.Last().Y, CommandType.ELECTRODE_OFF, time));

            if (commands.Last().Time - startTime != OperationTime)
            {
                throw new InternalRuntimeException("WAAAAA");
            }

            return commands;
        }

        private List<Command> MoveDropletsToTheRight(int middleOfComponentYValue, int xPos, int rightDropletInitialXPosition, ref int time)
        {
            List<Command> commands = new List<Command>();

            (int x, int y) toTurnOff = (xPos, middleOfComponentYValue);
            commands.Add(new Command(toTurnOff.x, toTurnOff.y, CommandType.ELECTRODE_ON, time));
            time++;

            do
            {
                xPos++;
                commands.Add(new Command(xPos, middleOfComponentYValue, CommandType.ELECTRODE_ON, time));
                time++;
                commands.Add(new Command(toTurnOff.x, toTurnOff.y, CommandType.ELECTRODE_OFF, time));
                commands.Add(new Command(xPos, middleOfComponentYValue, CommandType.ELECTRODE_ON, time));
                time++;
                toTurnOff = (xPos, middleOfComponentYValue);
            } while (xPos != rightDropletInitialXPosition);

            commands.Add(new Command(toTurnOff.x, toTurnOff.y, CommandType.ELECTRODE_OFF, time));

            return commands;
        }

        private List<Command> MoveDropletsToTheLeft(int middleOfComponentYValue, int leftDropletInitialXPosition, int xPos, ref int time)
        {
            List<Command> commands = new List<Command>();

            (int x, int y) toTurnOff = (xPos, middleOfComponentYValue);
            commands.Add(new Command(toTurnOff.x, toTurnOff.y, CommandType.ELECTRODE_ON, time));
            time++;

            do
            {
                xPos--;
                commands.Add(new Command(xPos, middleOfComponentYValue, CommandType.ELECTRODE_ON, time));
                time++;
                commands.Add(new Command(toTurnOff.x, toTurnOff.y, CommandType.ELECTRODE_OFF, time));
                commands.Add(new Command(xPos, middleOfComponentYValue, CommandType.ELECTRODE_ON, time));
                time++;
                toTurnOff = (xPos, middleOfComponentYValue);
            } while (xPos != leftDropletInitialXPosition);

            commands.Add(new Command(toTurnOff.x, toTurnOff.y, CommandType.ELECTRODE_OFF, time));

            return commands;
        }
    }
}
