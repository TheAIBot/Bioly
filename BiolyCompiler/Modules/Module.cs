using System;
using System.Collections.Generic;
using System.Drawing;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Commands;
using BiolyCompiler.Routing;
using BiolyCompiler.Scheduling;
using System.Linq;

namespace BiolyCompiler.Modules
{
    public abstract class Module
    {
        public Rectangle Shape;
        public int OperationTime = 1;
        public Block BindingOperation;
        //The key is the input fluid name, see the operation/block which the module is bound to.
        public Dictionary<string, List<Route>> InputRoutes = new Dictionary<string, List<Route>>();
        protected ModuleLayout InputLayout;
        protected ModuleLayout OutputLayout;
        public int StartTime = int.MinValue;
        


        public Module(int width, int height, int operationTime, bool useDefaultLayout){
            Shape = new Rectangle(width, height);
            this.OperationTime = operationTime;
            Shape.isEmpty = false;
            if (useDefaultLayout)
            {
                //At default, the output is placed in the left corner of the module.
                InputLayout  = GetDefaultSingleOutputOrInputLayout(Shape);
                OutputLayout = GetDefaultSingleOutputOrInputLayout(Shape);
            }
        }


        public Module(int width, int height, int operationTime, int numberOfInputs, int numberOfOutputs, ModuleLayout outputLayout) : this(width, height, operationTime, false)
        {
            this.InputLayout  = GetDefaultSingleOutputOrInputLayout(Shape);
            this.OutputLayout = outputLayout;
        }

        public Module(int width, int height, int operationTime, int numberOfInputs, int numberOfOutputs, ModuleLayout outputLayout, ModuleLayout inputLayout) : this(width, height, operationTime, false)
        {
            this.InputLayout  = inputLayout;
            this.OutputLayout = outputLayout;
        }


        public static ModuleLayout GetDefaultSingleOutputOrInputLayout(Rectangle rectangle)
        {
            Droplet droplet = new Droplet();
            (Rectangle TopRectangle, Rectangle RightRectangle) = rectangle.SplitIntoSmallerRectangles(droplet.Shape);
            List<Rectangle> emptyRectangles = new List<Rectangle>();
            if (TopRectangle != null) emptyRectangles.Add(TopRectangle);
            if (RightRectangle != null) emptyRectangles.Add(RightRectangle);
            return new ModuleLayout(rectangle, emptyRectangles, new List<Droplet>() {droplet});
        }

        public void RepositionLayout()
        {
            InputLayout?.Reposition(Shape.x, Shape.y);
            OutputLayout?.Reposition(Shape.x, Shape.y);
        }

        public virtual int getNumberOfInputs() {
            return InputLayout.Droplets.Count;
        }

        public virtual int getNumberOfOutputs()
        {
            return OutputLayout.Droplets.Count;
        }

        public virtual ModuleLayout GetOutputLayout() {
            if (OutputLayout == null) {
                throw new Exception("The output layout for the module \"" + this.ToString() + "\" have not been set/is null");
            } else return OutputLayout;
        }

        public virtual ModuleLayout GetInputLayout() {
            if (InputLayout == null) {
                throw new Exception("The layout for the module \"" + this.ToString() + "\" have not been set/is null");
            } else return InputLayout;
        }

        private bool canContainPoints(List<Point> DropletOutputLocations)
        {
            foreach (var point in DropletOutputLocations)
            {
                if (point.X < 0 ||
                    point.Y < 0 ||
                    Shape.width  < point.X + Droplet.DROPLET_WIDTH || 
                    Shape.height < point.Y + Droplet.DROPLET_HEIGHT) return false;
            }
            return true;
        }

        public override String ToString()
        {
            return this.GetType().ToString() + ", input/output = (" + getNumberOfInputs() + ", " + getNumberOfOutputs() + "), dimensions = {" + Shape.ToString() + "}, operation time = " + OperationTime;
        }

        
        //Returns a copy of the module (not taking adjacencies into account). 
        //It is used for creating unique modules, for the binding process in the scheduling
        public abstract Module GetCopyOf();

        //True iff the module is placed permenently on the board.
        public virtual bool isStaticModule()
        {
            return false;
        }

        public override int GetHashCode()
        {
            //TODO Can be improved.
            return Shape.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Module moduleObj = obj as Module;
            if (moduleObj == null) return false;
            else if (moduleObj.GetType() != this.GetType()) return false;
            else
            {
                bool sameBindingOperation = (BindingOperation != null && moduleObj.BindingOperation != null && BindingOperation.Equals(moduleObj.BindingOperation)) ||
                                            (BindingOperation == null && moduleObj.BindingOperation == null);
                return Shape.Equals(moduleObj.Shape) &&
                        OperationTime == moduleObj.OperationTime &&
                        getNumberOfInputs()  == moduleObj.getNumberOfInputs() &&
                        getNumberOfOutputs() == moduleObj.getNumberOfOutputs() &&
                        sameBindingOperation;
            }
        }
        

        public bool Implements(FluidBlock operation)
        {
            return  getNumberOfOutputs() == operation.InputVariables.Count && 
                    //numberOfOutputs == operation.OutputVariable.Count &&
                    this.GetType().Equals(operation.getAssociatedModule().GetType());
        }

        protected abstract List<Command> GetModuleCommands(ref int time);

        public List<Command> ToCommands()
        {
            List<Command> commands = new List<Command>();
            int time = 0;
            List<Command> routeCommands = new List<Command>();
            foreach (List<Route> routes in InputRoutes.Values.OrderBy(routes => routes.First().startTime))
            {
                routes.ForEach(route => routeCommands.AddRange(route.ToCommands(ref time)));
            }
            List<Command> moduleCommands = GetModuleCommands(ref time);
            if (moduleCommands.Count > 0)
            {
                //show module on simulator
                commands.Add(new AreaCommand(Shape, CommandType.SHOW_AREA, 0));
            }
            commands.AddRange(routeCommands);
            commands.AddRange(moduleCommands);

            if (moduleCommands.Count > 0)
            {
                //remove module from simulator
                commands.Add(new AreaCommand(Shape, CommandType.REMOVE_AREA, time));
            }

            return commands;
        }
    }
}
