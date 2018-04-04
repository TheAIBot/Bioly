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

        public void AddBlock(string name, string blockType)
        {
            Builder.Append($"const {name} = workspace.newBlock(\"{blockType}\");");
        }

        public void AddConnection(string inputBlockname, string inputName, string outputBlockName)
        {
            Builder.Append($"{inputBlockname}.getInput(\"{inputName}\").connection.connect({outputBlockName}.outputConnection);");
        }

        public void SetField<T>(string blockName, string fieldName, T newValue)
        {
            Builder.Append($"{blockName}.setFieldValue(\"{newValue.ToString()}\", \"{fieldName}\");");
        }

        public string AddInputBlock(string fluidName, int fluidAmount, FluidUnit unit)
        {
            string a = GetRandomName();
            AddBlock(a, Input.XmlTypeName);
            SetField(a, Input.InputFluidFieldName, fluidName);
            SetField(a, Input.InputAmountFieldName, fluidAmount);
            SetField(a, Input.FluidUnitFieldName, Input.FluidUnitToString(unit));

            return a;
        }

        public void AddFluidInputBlock(string blockName, string fluidName, int amount, bool useAll)
        {
            AddBlock(blockName, FluidAsInput.XmlTypeName);
            SetField(blockName, FluidAsInput.FluidNameFieldName, fluidName);
            SetField(blockName, FluidAsInput.FluidAmountFieldName, amount);
            SetField(blockName, FluidAsInput.UseAllFluidFieldName, FluidAsInput.BoolToString(useAll));
        }

        public string AddHeaterSegment(string outputName, int temperature, int time)
        {
            string a = GetRandomName();
            string b = GetRandomName();
            string c = GetRandomName();
            AddBlock(a, Fluid.XmlTypeName);
            AddBlock(b, Heater.XmlTypeName);
            //AddBlock(c, FluidAsInput.XmlTypeName);
            AddFluidInputBlock(c, "something", 10, false);
            SetField(a, Fluid.OutputFluidFieldName, outputName);
            SetField(b, Heater.TemperatureFieldName, temperature);
            SetField(b, Heater.TimeFieldName, time);
            AddConnection(a, Fluid.InputFluidFieldName, b);
            AddConnection(b, Heater.InputFluidFieldName, c);

            return a;
        }

        public string AddMixerSegment(string outputName, string inputNameA, string inputNameB)
        {
            string a = GetRandomName();
            string b = GetRandomName();
            string c = GetRandomName();
            string d = GetRandomName();
            AddBlock(a, Fluid.XmlTypeName);
            AddBlock(b, Mixer.XmlTypeName);
            //AddBlock(c, FluidAsInput.XmlTypeName);
            //AddBlock(d, FluidAsInput.XmlTypeName);
            AddFluidInputBlock(c, "fish", 10, false);
            AddFluidInputBlock(d, "cake", 0, true);
            SetField(a, Fluid.OutputFluidFieldName, outputName);
            SetField(c, FluidAsInput.FluidNameFieldName, inputNameA);
            SetField(d, FluidAsInput.FluidNameFieldName, inputNameB);
            AddConnection(a, Fluid.InputFluidFieldName, b);
            AddConnection(b, Mixer.FirstInputFieldName , c);
            AddConnection(b, Mixer.SecondInputFieldName, d);

            return a;
        }

        public string AddConstantBlock(int number)
        {
            string a = GetRandomName();
            AddBlock(a, Constant.XmlTypeName);
            SetField(a, Constant.NumberFieldName, number);

            return a;
        }

        public string AddArithOPBlock(ArithOPTypes type, string leftBlock, string rightBlock)
        {
            string a = GetRandomName();
            AddBlock(a, ArithOP.XmlTypeName);
            SetField(a, ArithOP.OPTypeFieldName, ArithOP.ArithOpTypeToString(type));
            AddConnection(a, ArithOP.LeftArithFieldName, leftBlock);
            AddConnection(a, ArithOP.RightArithFieldName, rightBlock);

            return a;
        }

        public string AddBoolOPBlock(BoolOPTypes type, string leftBlock, string rightBlock)
        {
            string a = GetRandomName();
            AddBlock(a, BoolOP.XmlTypeName);
            SetField(a, BoolOP.OPTypeFieldName, BoolOP.BoolOpTypeToString(type));
            AddConnection(a, BoolOP.LeftBoolFieldName, leftBlock);
            AddConnection(a, BoolOP.RightBoolFieldName, rightBlock);

            return a;
        }

        public string GetRandomName()
        {
            nameID++;
            return "N" + nameID;
        }

        public override string ToString()
        {
            return Builder.ToString();
        }
    }
}
