using BiolyCompiler;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using BiolyTests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

            List<perf_data> unoptimizedDatas = new List<perf_data>();
            List<perf_data> optimizedDatas = new List<perf_data>();
            int nameID = 0;
            Random random = new Random(15231);
            TestTools tools = new TestTools();
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    perf_data unoptimizedData = new perf_data();
                    perf_data optimizedData = new perf_data();
                    JSProgram program = new JSProgram();
                    program.Render = false;

                    tools.ClearWorkspace();
                    program.CreateCDFG(5, 15, random);
                    tools.ExecuteJS(program);

                    string xml = tools.GetWorkspaceString();
                    var result = XmlParser.Parse(xml);
                    if (result.Item2.Count > 0)
                    {
                        i--;
                        continue;
                    }

                    int testCount = 10;
                    float[] untimes = new float[testCount];
                    float[] optimes = new float[testCount];

                    Stopwatch watch = new Stopwatch();
                    for (int z = 0; z < testCount; z++)
                    {
                        watch.Reset();
                        watch.Start();
                        TestCommandExecutor commandExecutor = new TestCommandExecutor();
                        ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(commandExecutor);
                        programExecutor.TimeBetweenCommands = 0;
                        programExecutor.EnableOptimizations = false;
                        programExecutor.EnableGarbageCollection = false;
                        programExecutor.Run(100, 100, result.Item1, false);
                        unoptimizedData.makespan = commandExecutor.ticks;
                        watch.Stop();
                        untimes[z] = watch.ElapsedMilliseconds / (float)testCount;
                    }
                    for (int z = 0; z < testCount; z++)
                    {
                        watch.Reset();
                        watch.Start();
                        TestCommandExecutor commandExecutor = new TestCommandExecutor();
                        ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(commandExecutor);
                        programExecutor.TimeBetweenCommands = 0;
                        programExecutor.EnableOptimizations = true;
                        programExecutor.EnableGarbageCollection = true;
                        programExecutor.Run(100, 100, result.Item1, false);
                        optimizedData.makespan = commandExecutor.ticks;
                        watch.Stop();
                        optimes[z] = watch.ElapsedMilliseconds / (float)testCount;
                    }

                    unoptimizedData.time = untimes.Min();
                    optimizedData.time = optimes.Min();

                    unoptimizedDatas.Add(unoptimizedData);
                    optimizedDatas.Add(optimizedData);

                    string path = Path.Combine("unoptimizedPrograms", $"program_{nameID++}.bc");
                    File.WriteAllText(path, xml);
                }
                catch (Exception e)
                {
                    //Console.Write(e.Message + Environment.NewLine + e.StackTrace);
                    i--;
                }

                Console.WriteLine(i);
            }
            tools.AssemblyCleanup();

            File.WriteAllText("unoptimized_data.txt", String.Join(Environment.NewLine, unoptimizedDatas.Select(x => x.makespan + ", " + x.time.ToString(CultureInfo.InvariantCulture))));
            File.WriteAllText("optimized_data.txt"  , String.Join(Environment.NewLine, optimizedDatas  .Select(x => x.makespan + ", " + x.time.ToString(CultureInfo.InvariantCulture))));
        }

        private struct perf_data
        {
            public int makespan;
            public float time;
        }
    }
}
