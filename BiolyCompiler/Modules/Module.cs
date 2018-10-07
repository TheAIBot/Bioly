using System;
using System.Collections.Generic;
using System.Drawing;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Commands;
using BiolyCompiler.Routing;
using BiolyCompiler.Scheduling;
using System.Linq;
using BiolyCompiler.Exceptions;

namespace BiolyCompiler.Modules
{
    public abstract class Module
    {
        public Rectangle Shape;
        public int OperationTime = 1;
        //public Block BindingOperation;
        protected ModuleLayout InputLayout;
        protected ModuleLayout OutputLayout;
        


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
            var newRectangles = Rectangle.SplitIntoSmallerRectangles(rectangle, droplet.Shape);
            droplet.Shape = newRectangles.newSmaller;
            List<Rectangle> emptyRectangles = new List<Rectangle>();
            if (newRectangles.top != null)
            {
                emptyRectangles.Add(newRectangles.top);
            }
            if (newRectangles.right != null)
            {
                emptyRectangles.Add(newRectangles.right);
            }
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
                throw new InternalRuntimeException("The output layout for the module \"" + this.ToString() + "\" have not been set/is null");
            } else return OutputLayout;
        }

        public virtual ModuleLayout GetInputLayout() {
            if (InputLayout == null) {
                throw new InternalRuntimeException("The layout for the module \"" + this.ToString() + "\" have not been set/is null");
            } else return InputLayout;
        }

        public bool HasOutputLayout()
        {
            return OutputLayout != null;
        }

        public override String ToString()
        {
            return this.GetType().ToString() + ", input/output = (" + getNumberOfInputs() + ", " + getNumberOfOutputs() + "), dimensions = {" + Shape.ToString() + "}, operation time = " + OperationTime;
        }

        //True iff the module is placed permenently on the board.
        public virtual bool IsStaticModule()
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
                return Shape.Equals(moduleObj.Shape) &&
                        OperationTime == moduleObj.OperationTime &&
                        getNumberOfInputs() == moduleObj.getNumberOfInputs() &&
                        getNumberOfOutputs() == moduleObj.getNumberOfOutputs(); //&& sameBindingOperation;
            }
        }
        

        public bool Implements(FluidBlock operation)
        {
            return  getNumberOfOutputs() == operation.InputFluids.Count && 
                    //numberOfOutputs == operation.OutputVariable.Count &&
                    this.GetType().Equals(operation.getAssociatedModule().GetType());
        }

        public abstract List<Command> GetModuleCommands(ref int time);

       
    }
}
