using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.FluidicInputs
{
    public abstract class FluidInput
    {
        public readonly string ID;
        public  readonly string FluidName;
        public string OriginalFluidName { get; internal set; }
        protected readonly float AmountInML;
        public  readonly bool UseAllFluid;
        public readonly List<string> InputNumbers;
        public static readonly List<string> EmptyNumbersList = new List<string>();

        public const int ML_PER_DROPLET = 1;
        public const string NO_FLUID_NAME = "ERROR_FINDING_NODE";

        public FluidInput(string id, string fluidName, string originalFluidName, int inputAmountInDroplets, bool useAllFluid, List<string> inputNumbers)
        {
            this.ID = id;
            this.FluidName = fluidName;
            this.OriginalFluidName = originalFluidName;
            this.AmountInML = inputAmountInDroplets;
            this.UseAllFluid = useAllFluid;
            this.InputNumbers = inputNumbers ?? EmptyNumbersList;

            if (!UseAllFluid)
            {
                Validator.ValueWithinRange(id, inputAmountInDroplets, 1, int.MaxValue);
            }
        }

        public virtual void Update<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {

        }

        public int GetAmountInDroplets(Dictionary<string, BoardFluid> FluidVariableLocations)
        {
            if (UseAllFluid)
            {
                return FluidVariableLocations.ContainsKey(OriginalFluidName) ? FluidVariableLocations[OriginalFluidName].GetNumberOfDropletsAvailable() : 0;

            }
            //tempoary until ratio is added
            return (int)Math.Floor((AmountInML / ML_PER_DROPLET) + 0.01);
        }

        public static bool StringToBool(string boolean)
        {
            switch (boolean)
            {
                case "TRUE":
                    return true;
                case "FALSE":
                    return false;
                default:
                    throw new InternalParseException("Failed to parse the boolean type.");
            }
        }

        public static string BoolToString(bool value)
        {
            switch (value)
            {
                case true:
                    return "TRUE";
                case false:
                    return "FALSE";
                default:
                    throw new InternalParseException("Failed to parse the boolean type.");
            }
        }

        public abstract string ToXml();

        public override string ToString()
        {
            if (UseAllFluid)
            {
                return "all of it";
            }
            else
            {
                return AmountInML.ToString("N2") + " ml";
            }
        }
    }
}
