using BiolyCompiler;
using BiolyCompiler.Parser;
using BiolyTests;
using System;
using System.IO;

namespace EvaluationProject
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Directory.Exists("unoptimizedPrograms"))
            {
                Directory.CreateDirectory("unoptimizedPrograms");
            }

            int nameID = 0;

            Random random = new Random(15231);
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    JSProgram program = new JSProgram();
                    program.Render = false;

                    TestTools.ClearWorkspace();
                    program.CreateCDFG(5, 15, random);
                    TestTools.ExecuteJS(program);

                    string xml = TestTools.GetWorkspaceString();
                    var result = XmlParser.Parse(xml);

                    if (result.Item2.Count == 0)
                    {
                        TestCommandExecutor commandExecutor = new TestCommandExecutor();
                        ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(commandExecutor);
                        programExecutor.TimeBetweenCommands = 0;
                        programExecutor.EnableOptimizations = false;
                        programExecutor.Run(100, 100, result.Item1, false);
                    }

                    string path = Path.Combine("unoptimizedPrograms", $"program_{nameID++}.bc");
                    File.WriteAllText(path, xml);
                }
                catch (Exception e)
                {
                    Console.Write(e.Message + Environment.NewLine + e.StackTrace);
                    Console.Read();
                    i--;
                }

                Console.WriteLine(i);
            }
        }
    }
}
