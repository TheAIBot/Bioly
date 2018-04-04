using BiolyCompiler;
using BiolyCompiler.BlocklyParts.Arithmetics;
using BiolyCompiler.BlocklyParts.BoolLogic;
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
        List<string> Segments = new List<string>();
        List<string> Blocks = new List<string>();
        public bool Render = false;

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

        public void AddInputBlock(string fluidName, int fluidAmount, FluidUnit unit)
        {
            string a = GetUniqueName();
            AddBlock(a, Input.XmlTypeName);
            SetField(a, Input.InputFluidFieldName, fluidName);
            SetField(a, Input.InputAmountFieldName, fluidAmount);
            SetField(a, Input.FluidUnitFieldName, Input.FluidUnitToString(unit));

            Segments.Add(a);
        }

        public void AddFluidInputBlock(string blockName, string fluidName, int amount, bool useAllFluid)
        {
            AddBlock(blockName, FluidAsInput.XmlTypeName);
            SetField(blockName, FluidAsInput.FluidNameFieldName, fluidName);
            SetField(blockName, FluidAsInput.FluidAmountFieldName, amount);
            SetField(blockName, FluidAsInput.UseAllFluidFieldName, FluidAsInput.BoolToString(useAllFluid));
        }

        public void AddHeaterSegment(string outputName, int temperature, int time, string inputFluidName, int fluidAmount, bool useAllFluid)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            string c = GetUniqueName();
            AddBlock(a, Fluid.XmlTypeName);
            AddBlock(b, Heater.XmlTypeName);
            AddFluidInputBlock(c, inputFluidName, fluidAmount, useAllFluid);
            SetField(a, Fluid.OutputFluidFieldName, outputName);
            SetField(b, Heater.TemperatureFieldName, temperature);
            SetField(b, Heater.TimeFieldName, time);
            AddConnection(a, Fluid.InputFluidFieldName, b);
            AddConnection(b, Heater.InputFluidFieldName, c);

            Segments.Add(a);
        }

        public void AddMixerSegment(string outputName, string inputNameA, int amountA, bool useAllFluidA, string inputNameB, int amountB, bool useAllFluidB)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            string c = GetUniqueName();
            string d = GetUniqueName();
            AddBlock(a, Fluid.XmlTypeName);
            AddBlock(b, Mixer.XmlTypeName);
            AddFluidInputBlock(c, inputNameA, amountA, useAllFluidA);
            AddFluidInputBlock(d, inputNameB, amountB, useAllFluidB);
            SetField(a, Fluid.OutputFluidFieldName, outputName);
            AddConnection(a, Fluid.InputFluidFieldName, b);
            AddConnection(b, Mixer.FirstInputFieldName , c);
            AddConnection(b, Mixer.SecondInputFieldName, d);

            Segments.Add(a);
        }

        public string AddConstantBlock(int number)
        {
            string a = GetUniqueName();
            AddBlock(a, Constant.XmlTypeName);
            SetField(a, Constant.NumberFieldName, number);

            return a;
        }

        public string AddArithOPBlock(ArithOPTypes type, string leftBlock, string rightBlock)
        {
            string a = GetUniqueName();
            AddBlock(a, ArithOP.XmlTypeName);
            SetField(a, ArithOP.OPTypeFieldName, ArithOP.ArithOpTypeToString(type));
            AddConnection(a, ArithOP.LeftArithFieldName, leftBlock);
            AddConnection(a, ArithOP.RightArithFieldName, rightBlock);

            return a;
        }

        public string AddBoolOPBlock(BoolOPTypes type, string leftBlock, string rightBlock)
        {
            string a = GetUniqueName();
            AddBlock(a, BoolOP.XmlTypeName);
            SetField(a, BoolOP.OPTypeFieldName, BoolOP.BoolOpTypeToString(type));
            AddConnection(a, BoolOP.LeftBoolFieldName, leftBlock);
            AddConnection(a, BoolOP.RightBoolFieldName, rightBlock);

            return a;
        }

        public void AddWasteSegment(string fluidName, int amount, bool useAllFluid)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            AddBlock(a, Waste.XmlTypeName);
            AddFluidInputBlock(b, fluidName, amount, useAllFluid);
            AddConnection(a, Waste.InputFluidFieldName, b);

            Segments.Add(a);
        }

        public void AddOutputSegment(string fluidName, int amount, bool useAllFluid)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            AddBlock(a, Output.XmlTypeName);
            AddFluidInputBlock(b, fluidName, amount, useAllFluid);
            AddConnection(a, Output.InputFluidFieldName, b);

            Segments.Add(a);
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
                        AddHeaterSegment(outputName, random.Next(), random.Next(), firstInputName, random.Next(), GetRandomBool(random));
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
            FinishDFG();
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

        private void FinishDFG()
        {
            string a = GetUniqueName();
            AddBlock(a, "start");
            Builder.Append($"{a}.getInput(\"program\").connection.connect({Segments.First()}.previousConnection);");
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
            for (int i = 1; i < Segments.Count; i++)
            {
                ConnectSegments(Segments[i - 1], Segments[i]);
            }

            return Builder.ToString();
        }
    }
}
