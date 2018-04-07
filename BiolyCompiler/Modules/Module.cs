using System;
using System.Collections.Generic;
using System.Drawing;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Commands;
using BiolyCompiler.Modules.OperationTypes;
using BiolyCompiler.Routing;
using BiolyCompiler.Scheduling;
using System.Linq;

namespace BiolyCompiler.Modules
{
    public abstract class Module
    {
        public Rectangle Shape;
        public int OperationTime;
        public Block BindingOperation;
        public readonly int NumberOfInputs;
        public readonly int NumberOfOutputs;
        //The key is the input fluid name, see the operation/block which the module is bound to.
        public Dictionary<string, List<Route>> InputRoutes = new Dictionary<string, List<Route>>();
        protected ModuleLayout Layout;
        

        public Module(int width, int height, int operationTime){
            Shape = new Rectangle(width, height);
            this.OperationTime = operationTime;
            Shape.isEmpty = false;
            NumberOfInputs = 1;
            NumberOfOutputs = 1;
            //At default, the output is placed in the left corner of the module.
            Layout = GetDefaultSingleOutputLayout(Shape);
        }

        public Module(int Width, int Height, int OperationTime, int NumberOfInputs, int NumberOfOutputs)
        {
            Shape = new Rectangle(Width, Height);
            this.OperationTime = OperationTime;
            Shape.isEmpty = false;
            this.NumberOfInputs = NumberOfInputs;
            this.NumberOfOutputs = NumberOfOutputs;
        }

        public Module(int width, int height, int operationTime, int numberOfInputs, int numberOfOutputs, ModuleLayout Layout) : this(width, height, operationTime, numberOfInputs, numberOfOutputs)
        {
            this.NumberOfOutputs = numberOfOutputs;
            this.Layout = Layout;
            /*
            if (DropletOutputLocations.Count != numberOfOutputs) throw new Exception("The modules droplet output locations have not been set correctly. " +
                                                                                     numberOfOutputs + " outputs where expected, but there are " + DropletOutputLocations.Count + ".");
            else if (!canContainPoints(DropletOutputLocations))  throw new Exception("The given droplet output points cannot be contained in the module. The module has dimensions (width,height) = (" +
                                                                                     width + ", " + height + "), and the output locations are : [" + String.Join(", ", DropletOutputLocations) + "].");
            else this.DropletOutputLocations = DropletOutputLocations;
            */
        }


        public static ModuleLayout GetDefaultSingleOutputLayout(Rectangle rectangle)
        {
            Droplet droplet = new Droplet(new BoardFluid("Test"));
            (Rectangle TopRectangle, Rectangle RightRectangle) = rectangle.SplitIntoSmallerRectangles(droplet);
            List<Rectangle> emptyRectangles = new List<Rectangle>();
            if (TopRectangle != null) emptyRectangles.Add(TopRectangle);
            if (RightRectangle != null) emptyRectangles.Add(RightRectangle);
            return new ModuleLayout(rectangle.width, rectangle.height, emptyRectangles, new List<Droplet>() {droplet});
        }

        public void RepositionLayout()
        {
            Layout.Reposition(Shape.x, Shape.y);
        }

        public virtual ModuleLayout GetModuleLayout() {
            if (Layout == null) {
                throw new Exception("The layout for the module \"" + this.ToString() + "\" have not been set/is null");
            } else return Layout;
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

        public abstract OperationType getOperationType();

        public override String ToString()
        {
            return this.GetType().ToString() + ", input/output = (" + NumberOfInputs + ", " + NumberOfOutputs + "), dimensions = " + Shape.ToString() + ", operation time = " + OperationTime;
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
                        NumberOfInputs == moduleObj.NumberOfInputs &&
                        NumberOfOutputs == moduleObj.NumberOfOutputs &&
                        sameBindingOperation;
            }
        }
        

        public bool Implements(FluidBlock operation)
        {
            return  NumberOfInputs  == operation.InputVariables.Count && 
                    //numberOfOutputs == operation.OutputVariable.Count &&
                    this.GetType().Equals(operation.getAssociatedModule().GetType());
        }

        protected abstract List<Command> GetModuleCommands();

        public List<Command> ToCommands()
        {
            List<Command> commands = new List<Command>();

            //i need a way to get this in the correct order
            foreach (List<Route> route in InputRoutes.Values)
            {
                route.ForEach(x => commands.AddRange(x.ToCommands()));
            }

            commands.AddRange(GetModuleCommands());

            return commands;
        }
    }
}
