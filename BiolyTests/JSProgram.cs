﻿using BiolyCompiler;
using BiolyCompiler.BlocklyParts.Arithmetics;
using BiolyCompiler.BlocklyParts.BoolLogic;
using BiolyCompiler.BlocklyParts.ControlFlow;
using BiolyCompiler.BlocklyParts.Declarations;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.FluidicInputs;
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

        public string AddHeaterDeclarationBlock(string moduleName)
        {
            string a = GetUniqueName();
            AddBlock(a, HeaterDeclaration.XML_TYPE_NAME);
            SetField(a, HeaterUsage.MODULE_NAME_FIELD_NAME, moduleName);

            CurrentScope.Add(a);
            return a;
        }

        public string AddOutputDeclarationBlock(string moduleName)
        {
            string a = GetUniqueName();
            AddBlock(a, OutputDeclaration.XML_TYPE_NAME);
            SetField(a, OutputDeclaration.MODULE_NAME_FIELD_NAME, moduleName);

            CurrentScope.Add(a);
            return a;
        }

        public string AddWasteDeclarationBlock(string moduleName)
        {
            string a = GetUniqueName();
            AddBlock(a, WasteDeclaration.XML_TYPE_NAME);
            SetField(a, WasteDeclaration.MODULE_NAME_FIELD_NAME, moduleName);

            CurrentScope.Add(a);
            return a;
        }

        public void AddBasicInputBlock(string blockName, string fluidName, int amount, bool useAllFluid)
        {
            AddBlock(blockName, BasicInput.XML_TYPE_NAME);
            SetField(blockName, BasicInput.FLUID_NAME_FIELD_NAME, fluidName);
            SetField(blockName, BasicInput.FLUID_AMOUNT_FIELD_NAME, amount);
            SetField(blockName, BasicInput.USE_ALL_FLUID_FIELD_NAME, FluidInput.BoolToString(useAllFluid));
        }

        public string AddFluidSegment(string outputName, string inputName, int amount, bool useAllFluid)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            AddBlock(a, Fluid.XML_TYPE_NAME);
            SetField(a, Fluid.OUTPUT_FLUID_FIELD_NAME, outputName);
            AddBasicInputBlock(b, inputName, amount, useAllFluid);
            AddConnection(a, Fluid.INPUT_FLUID_FIELD_NAME, b);

            CurrentScope.Add(a);
            return a;
        }

        public string AddHeaterSegment(string outputName, string moduleName, int temperature, int time, string inputFluidName, int fluidAmount, bool useAllFluid)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            string c = GetUniqueName();
            AddBlock(a, Fluid.XML_TYPE_NAME);
            AddBlock(b, HeaterUsage.XML_TYPE_NAME);
            AddBasicInputBlock(c, inputFluidName, fluidAmount, useAllFluid);
            SetField(a, Fluid.OUTPUT_FLUID_FIELD_NAME, outputName);
            SetField(b, HeaterUsage.MODULE_NAME_FIELD_NAME, moduleName);
            SetField(b, HeaterUsage.TEMPERATURE_FIELD_NAME, temperature);
            SetField(b, HeaterUsage.TIME_FIELD_NAME, time);
            AddConnection(a, Fluid.INPUT_FLUID_FIELD_NAME, b);
            AddConnection(b, HeaterUsage.INPUT_FLUID_FIELD_NAME, c);

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
            AddBasicInputBlock(c, inputNameA, amountA, useAllFluidA);
            AddBasicInputBlock(d, inputNameB, amountB, useAllFluidB);
            SetField(a, Fluid.OUTPUT_FLUID_FIELD_NAME, outputName);
            AddConnection(a, Fluid.INPUT_FLUID_FIELD_NAME, b);
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

        public string AddOutputSegment(string fluidName, string moduleName, int amount, bool useAllFluid)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            AddBlock(a, OutputUsage.XML_TYPE_NAME);
            SetField(a, OutputUsage.MODULE_NAME_FIELD_NAME, moduleName);
            AddBasicInputBlock(b, fluidName, amount, useAllFluid);
            AddConnection(a, OutputUsage.INPUT_FLUID_FIELD_NAME, b);

            CurrentScope.Add(a);
            return a;
        }

        public string AddWasteSegment(string fluidName, string moduleName, int amount, bool useAllFluid)
        {
            string a = GetUniqueName();
            string b = GetUniqueName();
            AddBlock(a, WasteUsage.XML_TYPE_NAME);
            SetField(a, WasteUsage.MODULE_NAME_FIELD_NAME, moduleName);
            AddBasicInputBlock(b, fluidName, amount, useAllFluid);
            AddConnection(a, WasteUsage.INPUT_FLUID_FIELD_NAME, b);

            CurrentScope.Add(a);
            return a;
        }

        public string AddIfSegment(string conditionalBlock, string guardedBlock)
        {
            string a = GetUniqueName();
            AddBlock(a, If.XML_TYPE_NAME);
            AddConnection(a, If.GetIfFieldName(), conditionalBlock);
            Builder.Append($"{a}.getInput(\"{If.GetDoFieldName()}\").connection.connect({guardedBlock}.previousConnection);");

            CurrentScope.Add(a);
            return a;
        }

        public string AddRepeatSegment(string conditionalBlock, string guardedBlock)
        {
            string a = GetUniqueName();
            AddBlock(a, Repeat.XML_TYPE_NAME);
            AddConnection(a, Repeat.TimesBlockFieldName, conditionalBlock);
            Builder.Append($"{a}.getInput(\"{Repeat.DoBlockFieldName}\").connection.connect({guardedBlock}.previousConnection);");

            CurrentScope.Add(a);
            return a;
        }

        public string AddWhileSegment(string conditionalBlock, string guardedBlock)
        {
            string a = GetUniqueName();
            AddBlock(a, While.XML_TYPE_NAME);
            AddConnection(a, While.CONDITIONAL_BLOCK_FIELD_NAME, conditionalBlock);
            Builder.Append($"{a}.getInput(\"{While.DO_BLOCK_FIELD_NAME}\").connection.connect({guardedBlock}.previousConnection);");

            CurrentScope.Add(a);
            return a;
        }

        public string AddSetNumberSegment(string variableName, string inputBlockName)
        {
            string a = GetUniqueName();
            AddBlock(a, SetNumberVariable.XML_TYPE_NAME);
            SetField(a, SetNumberVariable.VARIABLE_FIELD_NAME, variableName);
            AddConnection(a, SetNumberVariable.INPUT_VARIABLE_FIELD_NAME, inputBlockName);

            CurrentScope.Add(a);
            return a;
        }

        public string AddGetNumberBlock(string variableName)
        {
            string a = GetUniqueName();
            AddBlock(a, GetNumberVariable.XML_TYPE_NAME);
            SetField(a, GetNumberVariable.VARIABLE_FIELD_NAME, variableName);

            return a;
        }

        public string AddRoundBlock(RoundOPTypes roundType, string inputBlockName)
        {
            string a = GetUniqueName();
            AddBlock(a, RoundOP.XML_TYPE_NAME);
            SetField(a, RoundOP.OPTypeFieldName, RoundOP.RoundOpTypeToString(roundType));
            AddConnection(a, RoundOP.NUMBER_FIELD_NAME, inputBlockName);

            return a;
        }

        public string AddImportVariableSegment(string variableName)
        {
            string a = GetUniqueName();
            AddBlock(a, ImportVariable.XML_TYPE_NAME);
            SetField(a, ImportVariable.VARIABLE_FIELD_NAME, variableName);

            return a;
        }

        public void CreateRandomDFG(int size, Random random = null)
        {
            List<string> fluidNames = new List<string>();
            List<string> outputModuleNames = new List<string>();
            List<string> wasteModuleNames = new List<string>();
            List<string> heaterModuleNames = new List<string>();
            random = random ?? new Random();
            

            for (int i = 0; i < random.Next(10, Math.Max(20, size / 10)); i++)
            {
                string fluidName = GetUniqueName();
                AddInputBlock(fluidName, random.Next(), GetRandomFluidUnit(random));
                fluidNames.Add(fluidName);
            }

            for (int i = 0; i < random.Next(10, Math.Max(20, size / 10)); i++)
            {
                string moduleName = GetUniqueName();
                AddOutputDeclarationBlock(moduleName);
                outputModuleNames.Add(moduleName);
            }
            for (int i = 0; i < random.Next(10, Math.Max(20, size / 10)); i++)
            {
                string moduleName = GetUniqueName();
                AddWasteDeclarationBlock(moduleName);
                wasteModuleNames.Add(moduleName);
            }
            for (int i = 0; i < random.Next(10, Math.Max(20, size / 10)); i++)
            {
                string moduleName = GetUniqueName();
                AddHeaterDeclarationBlock(moduleName);
                heaterModuleNames.Add(moduleName);
            }

            for (int i = 0; i < size; i++)
            {
                string outputFluidName = GetUniqueName();
                string outputModuleName = outputModuleNames[random.Next(outputModuleNames.Count)];
                string wasteModuleName = wasteModuleNames[random.Next(wasteModuleNames.Count)];
                string heaterModuleName = heaterModuleNames[random.Next(heaterModuleNames.Count)];
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
                        AddHeaterSegment(outputFluidName, heaterModuleName, random.Next(0, 1000), random.Next(0, 1000), firstInputName, random.Next(), GetRandomBool(random));
                        fluidNames.Add(outputFluidName);
                        break;
                    case 1:
                        AddMixerSegment(outputFluidName, firstInputName, random.Next(), GetRandomBool(random), secondInputName, random.Next(), GetRandomBool(random));
                        fluidNames.Add(outputFluidName);
                        break;
                    case 2:
                        AddWasteSegment(firstInputName, wasteModuleName, random.Next(), GetRandomBool(random));
                        fluidNames.Remove(firstInputName);
                        break;
                    case 3:
                        AddOutputSegment(firstInputName, outputModuleName, random.Next(), GetRandomBool(random));
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
