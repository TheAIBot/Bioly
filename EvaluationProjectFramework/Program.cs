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

namespace EvaluationProjectFramework
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
            List<perf_data> optimizedNoGCDatas = new List<perf_data>();
            int nameID = 0;
            Random random = new Random(15231);
            TestTools tools = new TestTools();
            for (int i = 0; i < 2000; i++)
            {
                try
                {
                    perf_data unoptimizedData = new perf_data();
                    perf_data optimizedData = new perf_data();
                    perf_data optimizedNoGCData = new perf_data();
                    JSProgram program = new JSProgram();
                    program.Render = false;

                    tools.ClearWorkspace();
                    program.CreateCDFG(3, 15, random);
                    tools.ExecuteJS(program);

                    string xml = tools.GetWorkspaceString();
                    var result = XmlParser.Parse(xml);
                    if (result.Item2.Count > 0)
                    {
                        i--;
                        continue;
                    }

                    int testCount = 5;
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
                    {
                        int minSize = 10;
                        while (true)
                        {
                            try
                            {
                                result.Item1.Nodes.ForEach(x => x.dfg.Nodes.ForEach(qq => qq.value.Reset()));
                                TestCommandExecutor commandExecutor = new TestCommandExecutor();
                                ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(commandExecutor);
                                programExecutor.TimeBetweenCommands = 0;
                                programExecutor.EnableOptimizations = false;
                                programExecutor.EnableGarbageCollection = false;
                                programExecutor.Run(minSize, minSize, result.Item1, false);
                                break;
                            }
                            catch (Exception e)
                            {
                                if (minSize > 100)
                                {
                                    Console.Write(e.Message + Environment.NewLine + e.StackTrace);
                                }
                                minSize++;
                            }
                        }
                        unoptimizedData.size = minSize;
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
                    {
                        int minSize = 10;
                        while (true)
                        {
                            try
                            {
                                result.Item1.Nodes.ForEach(x => x.dfg.Nodes.ForEach(qq => qq.value.Reset()));
                                TestCommandExecutor commandExecutor = new TestCommandExecutor();
                                ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(commandExecutor);
                                programExecutor.TimeBetweenCommands = 0;
                                programExecutor.EnableOptimizations = true;
                                programExecutor.EnableGarbageCollection = true;
                                programExecutor.Run(minSize, minSize, result.Item1, false);
                                break;
                            }
                            catch (Exception)
                            {
                                minSize++;
                            }
                        }
                        optimizedData.size = minSize;
                    }

                    {
                        TestCommandExecutor commandExecutor = new TestCommandExecutor();
                        ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(commandExecutor);
                        programExecutor.TimeBetweenCommands = 0;
                        programExecutor.EnableOptimizations = true;
                        programExecutor.EnableGarbageCollection = false;
                        programExecutor.Run(100, 100, result.Item1, false);
                        optimizedNoGCData.makespan = commandExecutor.ticks;
                    }
                    {
                        int minSize = 10;
                        while (true)
                        {
                            try
                            {
                                result.Item1.Nodes.ForEach(x => x.dfg.Nodes.ForEach(qq => qq.value.Reset()));
                                TestCommandExecutor commandExecutor = new TestCommandExecutor();
                                ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(commandExecutor);
                                programExecutor.TimeBetweenCommands = 0;
                                programExecutor.EnableOptimizations = true;
                                programExecutor.EnableGarbageCollection = false;
                                programExecutor.Run(minSize, minSize, result.Item1, false);
                                break;
                            }
                            catch (Exception)
                            {
                                minSize++;
                            }
                        }
                        optimizedNoGCData.size = minSize;
                    }

                    unoptimizedData.time = untimes.Min();
                    optimizedData.time = optimes.Min();

                    unoptimizedDatas.Add(unoptimizedData);
                    optimizedDatas.Add(optimizedData);
                    optimizedNoGCDatas.Add(optimizedNoGCData);

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

            File.WriteAllText("unoptimized_data.txt", String.Join(Environment.NewLine, unoptimizedDatas.Select(x => x.makespan + " " + x.time.ToString(CultureInfo.InvariantCulture) + " " + x.size)));
            File.WriteAllText("optimized_data.txt"  , String.Join(Environment.NewLine, optimizedDatas  .Select(x => x.makespan + " " + x.time.ToString(CultureInfo.InvariantCulture) + " " + x.size)));
            File.WriteAllText("optimized_no_gc_data.txt", String.Join(Environment.NewLine, optimizedNoGCDatas.Select(x => x.makespan + " " + x.size)));
        }

        private struct perf_data
        {
            public int makespan;
            public float time;
            public int size;
        }
    }
}
