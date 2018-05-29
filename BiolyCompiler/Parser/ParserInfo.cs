using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.Parser
{
    public class ParserInfo
    {
        public readonly CDFG cdfg = new CDFG();
        public readonly List<ParseException> ParseExceptions = new List<ParseException>();
        public readonly Dictionary<VariableType, (HashSet<string> validVariables, Stack<List<string>> scopes)> Scopes = new Dictionary<VariableType, (HashSet<string>, Stack<List<string>>)>();
        public Dictionary<string, string> MostRecentVariableRef;
        public bool DoTypeChecks = true;
        private int Unique = 0;

        public ParserInfo()
        {
            foreach (VariableType type in Enum.GetValues(typeof(VariableType)).Cast<VariableType>())
            {
                Scopes.Add(type, (new HashSet<string>(), new Stack<List<string>>()));
            }
        }

        public void EnterDFG()
        {
            foreach (var scope in Scopes)
            {
                scope.Value.scopes.Push(new List<string>());
            }

            MostRecentVariableRef = new Dictionary<string, string>();
        }

        public void LeftDFG()
        {
            foreach (var scope in Scopes)
            {
                scope.Value.scopes.Pop().ForEach(x => scope.Value.validVariables.Remove(x));
            }

            MostRecentVariableRef = null;
        }

        public void CheckVariable(string id, VariableType type, string variableName)
        {
            //if type checked is disabled then don't type check. duh
            if (!DoTypeChecks)
            {
                return;
            }

            if (!Scopes[type].validVariables.Contains(variableName))
            {
                ParseExceptions.Add(new ParseException(id, $"Variable {variableName} of type {type.ToReadableString()} isn't previously defined."));
            }
        }

        public void AddVariable(string id, VariableType type, string variableName)
        {
            //Not allowed to add the variable if it already 
            //exists as another type
            foreach (var scope in Scopes)
            {
                if (scope.Key == type)
                {
                    continue;
                }

                if (scope.Value.validVariables.Contains(variableName))
                {
                    throw new ParseException(id, $"Can't create the variable {variableName} of type {type.ToReadableString()} as it already exists as the type {scope.Key.ToReadableString()}.");
                }
            }

            //Don't add the variable again if it was
            //already defined in a previous scope
            if (Scopes[type].scopes.All(scope => !scope.Contains(variableName)))
            {
                Scopes[type].scopes.Peek().Add(variableName);
                Scopes[type].validVariables.Add(variableName);
            }
        }

        public string GetUniquePostFix()
        {
            Unique++;
            return $"{Validator.INLINE_PROGRAM_SPECIAL_SEPARATOR}{Unique}";
        }
    }
}
