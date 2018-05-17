using BiolyCompiler;
using BiolyCompiler.BlocklyParts.Arithmetics;
using BiolyCompiler.BlocklyParts.BoolLogic;
using BiolyCompiler.BlocklyParts.ControlFlow;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiolyTests
{
    public class JSProgram
    {
        StringBuilder Builder = new StringBuilder();
        int nameID = 0;
        //List<string> Segments = new List<string>();
        Dictionary<string, List<string>> Scopes = new Dictionary<string, List<string>>();
        List<string> CurrentScope = null;
        public const string DEFAULT_SCOPE_NAME = "default scope";
        List<string> Blocks = new List<string>();
        public bool Render = false;

        public JSProgram()
        {
            CurrentScope = new List<string>();
            Scopes.Add(DEFAULT_SCOPE_NAME, CurrentScope);
        }

        public void AddScope(string scopeName)
        {
            Scopes.Add(scopeName, new List<string>());
        }

        public void SetScope(string scopeName)
        {
            CurrentScope = Scopes[scopeName];
        }

        public void AddBlock(string name, string blockType)
        {
            Builder.Append($"const {name} = workspace.newBlock(\"{blockType}\");");
            Blocks.Add(name);
            if (Render)
            {
                RenderBlock(name);
            }
        }

        public void AddConnection(string inputBlockname, string inputName, string outputBlockName)
        {
            Builder.Append($"{inputBlockname}.getInput(\"{inputName}\").connection.connect({outputBlockName}.outputConnection);");
        }

        public void SetField<T>(string blockName, string fieldName, T newValue)
        {
            Builder.Append($"{blockName}.setFieldValue(\"{newValue.ToString()}\", \"{fieldName}\");");
        }

        public void ConnectSegments(string topSegment, string bottomSegment)
        {
            Builder.Append($"{topSegment}.nextConnection.connect({bottomSegment}.previousConnection);");
        }

        public void RenderBlock(string blockName)
        {
            Builder.Append($"{blockName}.initSvg();");
            Builder.Append($"{blockName}.render();");
        }

        public string AddInputBlock(string fluidName, int fluidAmount, FluidUnit unit)
        {
            string a = GetUniqueName();
            AddBlock(a, InputDeclaration.XML_TYPE_NAME);
            SetField(a, InputDeclaration.INPUT_FLUID_FIELD_NAME, fluidName);
            SetField(a, InputDeclaration.INPUT_AMOUNT_FIELD_NAME, fluidAmount);
            SetField(a, InputDeclaration.FLUID_UNIT_FIELD_NAME, InputDeclaration.FluidUnitToString(unit));

            CurrentScope.Add(a);
            return a;
        }

        public void AddFluidInputBlock(string blockName, string fluidName, int amount, bool useAllFluid)
        {
            AddBlock(blockName, FluidInput.XmlTypeName);
            SetField(blockName, FluidInput.FluidNameFieldName, fluidName);
            SetField(blockName, FluidInput.FluidAmountFieldName, amount);
            SetField(blockName, FluidInput.UseAllFluidFieldName, FluidInput.BoolToString(useAllFluid));
        }

        public string AddHeaterSegment(string outputName, int temperature, int time, string inputFluidName, int fluidAmount, bool useAllFluid)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            string c = GetUniqueName();
            AddBlock(a, Fluid.XML_TYPE_NAME);
            AddBlock(b, Heater.XmlTypeName);
            AddFluidInputBlock(c, inputFluidName, fluidAmount, useAllFluid);
            SetField(a, Fluid.OutputFluidFieldName, outputName);
            SetField(b, Heater.TemperatureFieldName, temperature);
            SetField(b, Heater.TimeFieldName, time);
            AddConnection(a, Fluid.InputFluidFieldName, b);
            AddConnection(b, Heater.InputFluidFieldName, c);

            CurrentScope.Add(a);
            return a;
        }

        public string AddMixerSegment(string outputName, string inputNameA, int amountA, bool useAllFluidA, string inputNameB, int amountB, bool useAllFluidB)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            string c = GetUniqueName();
            string d = GetUniqueName();
            AddBlock(a, Fluid.XML_TYPE_NAME);
            AddBlock(b, Mixer.XmlTypeName);
            AddFluidInputBlock(c, inputNameA, amountA, useAllFluidA);
            AddFluidInputBlock(d, inputNameB, amountB, useAllFluidB);
            SetField(a, Fluid.OutputFluidFieldName, outputName);
            AddConnection(a, Fluid.InputFluidFieldName, b);
            AddConnection(b, Mixer.FirstInputFieldName , c);
            AddConnection(b, Mixer.SecondInputFieldName, d);

            CurrentScope.Add(a);
            return a;
        }

        public string AddConstantBlock(int number)
        {
            string a = GetUniqueName();
            AddBlock(a, Constant.XML_TYPE_NAME);
            SetField(a, Constant.NumberFieldName, number);

            return a;
        }

        public string AddArithOPBlock(ArithOPTypes type, string leftBlock, string rightBlock)
        {
            string a = GetUniqueName();
            AddBlock(a, ArithOP.XML_TYPE_NAME);
            SetField(a, ArithOP.OPTypeFieldName, ArithOP.ArithOpTypeToString(type));
            AddConnection(a, ArithOP.LeftArithFieldName, leftBlock);
            AddConnection(a, ArithOP.RightArithFieldName, rightBlock);

            return a;
        }

        public string AddBoolOPBlock(BoolOPTypes type, string leftBlock, string rightBlock)
        {
            string a = GetUniqueName();
            AddBlock(a, BoolOP.XML_TYPE_NAME);
            SetField(a, BoolOP.OPTypeFieldName, BoolOP.BoolOpTypeToString(type));
            AddConnection(a, BoolOP.LeftBoolFieldName, leftBlock);
            AddConnection(a, BoolOP.RightBoolFieldName, rightBlock);

            return a;
        }

        public string AddWasteSegment(string fluidName, int amount, bool useAllFluid)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            AddBlock(a, Waste.XML_TYPE_NAME);
            AddFluidInputBlock(b, fluidName, amount, useAllFluid);
            AddConnection(a, Waste.InputFluidFieldName, b);

            CurrentScope.Add(a);
            return a;
        }

        public string AddOutputSegment(string fluidName, int amount, bool useAllFluid)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            AddBlock(a, OutputUseage.XML_TYPE_NAME);
            AddFluidInputBlock(b, fluidName, amount, useAllFluid);
            AddConnection(a, OutputUseage.INPUT_FLUID_FIELD_NAME, b);

            CurrentScope.Add(a);
            return a;
        }

        public string AddIfSegment(string conditionalBlock, string guardedBlock)
        {
            string a = GetUniqueName();
            AddBlock(a, If.XML_TYPE_NAME);
            AddConnection(a, If.GetIfFieldName(), conditionalBlock);
            AddConnection(a, If.GetDoFieldName(), guardedBlock);

            CurrentScope.Add(a);
            return a;
        }

        public void CreateRandomDFG(int size, Random random = null)
        {
            List<string> fluidNames = new List<string>();
            random = random ?? new Random();
            

            for (int i = 0; i < random.Next(10, Math.Max(20, size / 10)); i++)
            {
                string fluidName = GetUniqueName();
                AddInputBlock(fluidName, random.Next(), GetRandomFluidUnit(random));
                fluidNames.Add(fluidName);
            }

            for (int i = 0; i < size; i++)
            {
                string outputName = GetUniqueName();
                string firstInputName = fluidNames[random.Next(fluidNames.Count)];
                string secondInputName = fluidNames[random.Next(fluidNames.Count)];
                if (fluidNames.Count <= 1)
                {
                    break;
                }
                while (firstInputName == secondInputName)
                {
                    secondInputName = fluidNames[random.Next(fluidNames.Count)];
                }

                switch (random.Next(4))
                {
                    case 0:
                        AddHeaterSegment(outputName, random.Next(-273, 1000), random.Next(0, 1000), firstInputName, random.Next(), GetRandomBool(random));
                        fluidNames.Add(outputName);
                        break;
                    case 1:
                        AddMixerSegment(outputName, firstInputName, random.Next(), GetRandomBool(random), secondInputName, random.Next(), GetRandomBool(random));
                        fluidNames.Add(outputName);
                        break;
                    case 2:
                        AddWasteSegment(firstInputName, random.Next(), GetRandomBool(random));
                        fluidNames.Remove(firstInputName);
                        break;
                    case 3:
                        AddOutputSegment(firstInputName, random.Next(), GetRandomBool(random));
                        fluidNames.Remove(firstInputName);
                        break;
                }
            }
            Finish();
        }

        private FluidUnit GetRandomFluidUnit(Random random)
        {
            switch (random.Next(2))
            {
                case 0:
                    return FluidUnit.drops;
                case 1:
                    return FluidUnit.ml;
                default:
                    throw new Exception("random number is not between 0 and 2");
            }
        }

        private bool GetRandomBool(Random random)
        {
            return random.NextDouble() > 0.5;
        }

        public void Finish()
        {
            string a = GetUniqueName();
            AddBlock(a, "start");
            Builder.Append($"{a}.getInput(\"program\").connection.connect({Scopes[DEFAULT_SCOPE_NAME].First()}.previousConnection);");
        }

        public string GetUniqueName()
        {
            nameID++;
            return "N" + nameID;
        }

        public override string ToString()
        {
            //if (Render)
            //{
            //    Blocks.Reverse();
            //    Blocks.ForEach(x => RenderBlock(x));
            //}
            foreach (List<string> segments in Scopes.Values)
            {
                for (int i = 1; i < segments.Count; i++)
                {
                    ConnectSegments(segments[i - 1], segments[i]);
                }
            }

            return Builder.ToString();
        }
    }
}
