using System;
using System.Collections.Generic;
using System.Drawing;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Modules.OperationTypes;
using BiolyCompiler.Scheduling;

namespace BiolyCompiler.Modules
{
    public abstract class Module
    {
        public Rectangle Shape;
        public int OperationTime;
        public Block BindingOperation;
        public readonly int NumberOfInputs, NumberOfOutputs;
        //The key is the input fluid name, see the operation/block which the module is bound to.
        public Dictionary<string, Route> InputRoutes = new Dictionary<string, Route>();
        protected ModuleLayout Layout;
        
        public Module(int width, int height, int operationTime){
            Shape = new Rectangle(width, height);
            this.OperationTime = operationTime;
            Shape.isEmpty = false;
            NumberOfInputs = 1;
            NumberOfOutputs = 1;
            //At default, the output is placed in the left corner of the module.
            Layout = GetDefaultLayout();
        }

        private ModuleLayout GetDefaultLayout()
        {
            throw new NotImplementedException();
        }

        public Module(int Width, int Height, int OperationTime, int NumberOfInputs, int NumberOfOutputs) : this(Width, Height, OperationTime)
        {
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
            return this.GetType().ToString() + ", dimensions = " + Shape.ToString() + ", operation time = " + OperationTime;
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
        

        public bool Implements(Block operation)
        {
            return  NumberOfInputs  == operation.InputVariables.Count && 
                    //numberOfOutputs == operation.OutputVariable.Count &&
                    this.GetType().Equals(operation.getAssociatedModule().GetType());
        }
    }
}
