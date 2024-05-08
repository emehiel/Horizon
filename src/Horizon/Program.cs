// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using HSFScheduler;
using MissionElements;
using UserModel;
using HSFUniverse;
//using HSFSubsystem;
using HSFSystem;
using log4net;
using Utilities;
using Microsoft.Scripting.Actions.Calls;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Configuration;
using IronPython.Compiler.Ast;
using System.Diagnostics.Eventing.Reader;
using System.Net.Configuration;

namespace Horizon
{
    public class Program
    {
        public ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string SimulationFilePath { get; set; }
        public string TaskDeckFilePath { get; set; }
        public string ModelFilePath { get; set; }

        // Load the environment. First check if there is an ENVIRONMENT XMLNode in the input file
        public Domain SystemUniverse { get; set; }

        //Create singleton dependency dictionary
        public Dependency Dependencies { get; } = Dependency.Instance;

        // Initialize Lists to hold assets, subsystems and evaluators
        public List<Asset> AssetList { get; set; } = new List<Asset>();
        public List<Subsystem> SubList { get; set; } = new List<Subsystem>();

        // Maps used to set up preceeding nodes
        //public Dictionary<ISubsystem, XmlNode> SubsystemXMLNodeMap { get; set; } = new Dictionary<ISubsystem, XmlNode>(); //Depreciated (?)

        public List<KeyValuePair<string, string>> DependencyList { get; set; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> DependencyFcnList { get; set; } = new List<KeyValuePair<string, string>>();
        
        // Create Constraint list 
        public List<Constraint> ConstraintsList { get; set; } = new List<Constraint>();

        //Create Lists to hold all the dependency nodes to be parsed later
        //List<XmlNode> _depNodes = new List<XmlNode>();
        public SystemState InitialSysState { get; set; } = new SystemState();

        //XmlNode _evaluatorNode; //Depreciated (?)
        public Evaluator SchedEvaluator;
        public List<SystemSchedule> Schedules { get; set; }
        public SystemClass SimSystem { get; set; }
        public string OutputPath { get; set; }
        public Stack<Task> SystemTasks { get; set; } = new Stack<Task>();

        public static int Main(string[] args) //
        {
            Program program = new Program();
            // Begin the Logger
            program.log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            program.log.Info("STARTING HSF RUN"); //Do not delete
            program.InitInput(args);
            program.InitOutput();
            program.LoadScenario();
            program.LoadTasks();
            program.LoadSubsystems();
            program.LoadEvaluator();
            program.CreateSchedules();
            double maxSched = program.EvaluateSchedules();

            int i = 0;
            //Morgan's Way
            StreamWriter sw = File.CreateText(program.OutputPath);
            foreach (SystemSchedule sched in program.Schedules)
            {
                sw.WriteLine("Schedule Number: " + i + "Schedule Value: " + program.Schedules[i].ScheduleValue);
                foreach (var eit in sched.AllStates.Events)
                {
                    if (i < 5)//just compare the first 5 schedules for now
                    {
                        sw.WriteLine(eit.ToString());
                    }
                }
                i++;
            }
            program.log.Info("Max Schedule Value: " + maxSched);

            // Mehiel's way
            string stateDataFilePath = @"C:\HorizonLog\Scratch";// + string.Format("output-{0:yyyy-MM-dd-hh-mm-ss}", DateTime.Now);
            SystemSchedule.WriteSchedule(program.Schedules[0], stateDataFilePath);

            //  Move this to a method that always writes out data about the dynamic state of assets, the target dynamic state data, other data?
            //var csv = new StringBuilder();
            //csv.Clear();
            //foreach (var asset in program.simSystem.Assets)
            //{
            //    File.WriteAllText(@"..\..\..\" + asset.Name + "_dynamicStateData.csv", asset.AssetDynamicState.ToString());
            //}

            Console.ReadKey();
            return 0;
        }
        public void InitInput(string[] args)
        {
            string scenario = "Aeolus JSON";
            string subpath = "";
            switch(scenario)
            {
                case "Aeolus JSON":
                    // Set Defaults
                    subpath = @"..\..\..\..\samples\Aeolus\";
                    SimulationFilePath = subpath + @"AeolusSimulationInput.json";
                    TaskDeckFilePath = subpath + @"AeolusTasks.json";
                    // Asset 1 Scripted, Asset 2 C#
                    ModelFilePath = subpath + @"DSAC_Static_Scripted.json";
                    //ModelFilePath = subpath + @"DSAC_Static_Scripted.xml";
                    // Asset 1 mix Scripted/C#, Asset 2 C#
                    //ModelInputFilePath = subpath + @"DSAC_Static_Mod_PartialScripted.xml"; 
                    // Asset 1 C#, Asset 2 C#
                    //ModelInputFilePath = subpath + @"DSAC_Static_Mod.xml"; 
                    break;
                case "Aeolus":
                    // Set Defaults
                    subpath = @"..\..\..\..\samples\Aeolus\";
                    SimulationFilePath = subpath + @"AeolusSimulationInput.xml";
                    TaskDeckFilePath = subpath + @"v2.2-300targets.xml";
                    // Asset 1 Scripted, Asset 2 C#
                    ModelFilePath = subpath + @"DSAC_Static_Mod_Scripted.xml";
                    // Asset 1 mix Scripted/C#, Asset 2 C#
                    //ModelInputFilePath = subpath + @"DSAC_Static_Mod_PartialScripted.xml"; 
                    // Asset 1 C#, Asset 2 C#
                    //ModelInputFilePath = subpath + @"DSAC_Static_Mod.xml"; 
                    break;

                case "myFirstHSFProject":
                    // Set myFirstHSFProject file paths
                    subpath = @"..\..\..\..\samples\myFirstHSFProject\";
                    SimulationFilePath = subpath + @"myFirstHSFScenario.xml";
                    TaskDeckFilePath = subpath + @"myFirstHSFTargetDeck.xml";
                    ModelFilePath = subpath + @"myFirstHSFSystem.xml";
                    break;

                case "myFirstHSFProjectLook":
                    // Set myFirstHSFProjectConstraint file paths
                    subpath = @"..\..\..\..\samples\myFirstHSFProjectConstraint\";
                    SimulationFilePath = subpath + @"myFirstHSFScenario.xml";
                    TaskDeckFilePath = subpath + @"myFirstHSFTargetDeck.xml";
                    ModelFilePath = subpath + @"myFirstHSFSystemLook.xml";
                    break;
                case "myFirstHSFProjectDependency":
                    // Set myFirstHSFProjectDependency file paths
                    subpath = @"..\..\..\..\samples\myFirstHSFProjectDependency\";
                    SimulationFilePath = subpath + @"myFirstHSFScenario.xml";
                    TaskDeckFilePath = subpath + @"myFirstHSFTargetDeck.xml";
                    ModelFilePath = subpath + @"myFirstHSFSystemDependency.xml";
                    break;
            }

            bool simulationSet = false, targetSet = false, modelSet = false;

            // Get the input filenames
            int i = 0;
            foreach (var input in args)
            {
                i++;
                switch (input)
                {
                    case " - s":
                        SimulationFilePath = args[i];
                        simulationSet = true;
                        Console.WriteLine("Using custom simulation file: " + SimulationFilePath);
                        log.Info("Using custom simulation file: " + SimulationFilePath);
                        break;
                    case "-t":
                        TaskDeckFilePath = args[i];
                        targetSet = true;
                        Console.WriteLine("Using custom target deck file: " + TaskDeckFilePath);
                        log.Info("Using custom simulation file: " + TaskDeckFilePath);
                        break;
                    case "-m":
                        ModelFilePath = args[i];
                        modelSet = true;
                        Console.WriteLine("Using custom model file: " + ModelFilePath);
                        log.Info("Using custom model file: " + ModelFilePath);
                        break;
                }
            }

            ///add usage statement
            if (!simulationSet)
            {
                Console.WriteLine("Using Default Scenario File");
                log.Info("Using Default Scenario File");
            }

            if (!targetSet)
            {
                Console.WriteLine("Using Default Target File");
                log.Info("Using Default Target File");
            }

            if (!modelSet)
            {
                Console.WriteLine("Using Default MOdel File");
                log.Info("Using Default Model File");
            }

        }
        public void InitOutput()
        {
            // Initialize Output File
            var outputFileName = string.Format("output-{0:yyyy-MM-dd}-*", DateTime.Now);
            string outputPath = @"C:\HorizonLog\";
            var txt = ".txt";
            string[] fileNames = System.IO.Directory.GetFiles(outputPath, outputFileName, System.IO.SearchOption.TopDirectoryOnly);
            double number = 0;
            foreach (var fileName in fileNames)
            {
                char version = fileName[fileName.Length - txt.Length - 1];
                if (number < Char.GetNumericValue(version))
                    number = Char.GetNumericValue(version);
            }
            number++;
            outputFileName = outputFileName.Remove(outputFileName.Length - 1) + number;
            outputPath += outputFileName + txt;
            this.OutputPath = outputPath;
        }

        public void LoadScenario()
        {
            StreamReader jsonStream = new StreamReader(SimulationFilePath);

            JObject scenarioJson = JObject.Parse(jsonStream.ReadToEnd());

            // Load Scenario Name
            if (JsonLoader<string>.TryGetValue("name", scenarioJson, out string name))
            {
                Console.WriteLine($"Loading scenario {name}");
                log.Info($"Loading scenario {name}");
            }
            else
            {
                string msg = $"Scenario {name} must contain a name.";
                log.Fatal(msg);
                Console.WriteLine(msg);
                throw new ArgumentException(msg);
            }
            // Load Base Dependencies
            if(JsonLoader<JToken>.TryGetValue("dependencies", scenarioJson, out JToken dependenciesJson))
            {
                Console.WriteLine($"Base Dependecies Loaded for {name} at {SimulationFilePath}");
                log.Info($"Base Dependecies Loaded for {name} at {SimulationFilePath}");
            }
            else
            {
                string msg = $"Base Dependecies not found for {name} at {SimulationFilePath}";
                log.Fatal(msg);
                Console.WriteLine(msg);
                throw new ArgumentException(msg);
            }

            // Load Simulation Parameters
            if (JsonLoader<JToken>.TryGetValue("simulationParameters", scenarioJson, out JToken simulationJson))
            {
                SimParameters.LoadSimulationJson((JObject)simulationJson, name);
            }
            else
            {
                string msg = $"Simulation Parameters are not found in input files for scenario {name}.";
                log.Fatal(msg);
                Console.WriteLine(msg);
                throw new ArgumentException(msg);
            }

            // Load Scheduler Parameters
            if (JsonLoader<JToken>.TryGetValue("schedulerParameters", scenarioJson, out JToken schedulerJson))
            {
                SchedParameters.LoadScheduleJson((JObject)schedulerJson);
            }
            else
            {
                string msg = $"Scheduler Parameters are not found in input files for scenario {name}.";
                log.Fatal(msg);
                Console.WriteLine(msg);
                throw new ArgumentException(msg);
            }

        }
        public void LoadTasks()
        {
            StreamReader jsonStream = new StreamReader(TaskDeckFilePath);
            JObject taskListJson = JObject.Parse(jsonStream.ReadToEnd());

            if (!Task.LoadTasks(taskListJson, SystemTasks))
            {
                log.Fatal("Error loading Tasks at LoadTasks()");
                throw new Exception("Error loading Tasks at LoadTasks()");
            }

        }
        public void LoadSubsystems()
        {
            StreamReader jsonStream = new StreamReader(ModelFilePath);
            JObject scenarioJson = JObject.Parse(jsonStream.ReadToEnd());
            string msg;

            if (scenarioJson != null)
            {
                if (JsonLoader<JObject>.TryGetValue("model", scenarioJson, out JObject modelJson))
                {
                    // Load Environment
                    if (JsonLoader<JObject>.TryGetValue("environment", modelJson, out JObject environmentJson))
                    {
                        SystemUniverse = UniverseFactory.GetUniverseClass(environmentJson);
                    }
                    else
                    {
                        SystemUniverse = new SpaceEnvironment();
                        Console.WriteLine("Default Space Environment Loaded");
                        log.Info("Default Space Environment Loaded");
                    }

                    // Load Assets
                    if (JsonLoader<JToken>.TryGetValue("assets", modelJson, out JToken assetsListJson))
                    {
                        foreach (JObject assetJson in assetsListJson)
                        {
                            Asset asset = new Asset(assetJson);
                            asset.AssetDynamicState.Eoms.SetEnvironment(SystemUniverse);
                            AssetList.Add(asset);

                            // Load Subsystems
                            if (JsonLoader<JToken>.TryGetValue("subsystems", assetJson, out JToken subsystemListJson))
                            {
                                foreach (JObject subsystemJson in subsystemListJson)
                                {
                                    Subsystem subsys = SubsystemFactory.GetSubsystem(subsystemJson, asset);
                                    SubList.Add(subsys);

                                    // Load Subsystem States (Formerly ICs)
                                    if (JsonLoader<JToken>.TryGetValue("states", subsystemJson, out JToken stateListJson))
                                    {
                                        foreach (JObject stateJson in stateListJson)
                                        {
                                            // Parse state node for key name and state type, add the key to the subsys's list of keys, return the key name
                                            string keyName = SubsystemFactory.SetStateKeys(stateJson, subsys);
                                            // Use key name and state type to set initial conditions 
                                            InitialSysState.SetInitialSystemState(stateJson, keyName);
                                        }
                                    }
                                    else
                                    {
                                        msg = $"Warning: Subsystem {subsys.Name} loaded with no states";
                                        Console.WriteLine(msg);
                                        log.Warn(msg);
                                    }

                                    // Load Subsystem Parameters
                                    if (subsys.Type == "scripted")
                                    {
                                        // Load Subsystem Parameters                        
                                        if (JsonLoader<JToken>.TryGetValue("parameters", subsystemJson, out JToken parameterListJson))
                                            foreach (JObject parameterJson in parameterListJson)
                                                SubsystemFactory.SetParameters(parameterJson, subsys);
                                        else
                                        {
                                            msg = $"Warning: Subsystem {subsys.Name} loaded with no parameters";
                                            Console.WriteLine(msg);
                                            log.Warn(msg);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                msg = $"Error loading model for {SimParameters.ScenarioName}.  Error loading subsystems for asset, {asset.Name}";
                                Console.WriteLine(msg);
                                log.Fatal(msg);
                                throw new ArgumentException(msg);
                            }
                            // Load Constraints
                            if (JsonLoader<JToken>.TryGetValue("constraints", assetJson, out JToken constraintListJson))
                                foreach (JObject constraintJson in constraintListJson)
                                    ConstraintsList.Add(ConstraintFactory.GetConstraint(constraintJson, SubList, asset.Name));
                            else
                            {
                                msg = $"Warning: Asset {asset.Name} loaded with no constraints";
                                Console.WriteLine(msg);
                                log.Warn(msg);
                            }
                        }

                        // give some numbers here
                        msg = $"Environment, {AssetList.Count} Assets, Subsystems, and Constraints Loaded";
                        Console.WriteLine(msg);
                        log.Info(msg);
                    }
                    else
                    {
                        msg = $"Error loading assets for {SimParameters.ScenarioName}.";
                        Console.WriteLine(msg);
                        log.Fatal(msg);
                        throw new ArgumentException(msg);
                    }

                    // Load Dependencies
                    if (JsonLoader<JToken>.TryGetValue("dependencies", modelJson, out JToken dependencyListJson))
                    {
                        foreach (JObject dependencyJson in dependencyListJson)
                            SubsystemFactory.SetDependencies(dependencyJson, SubList);

                        Console.WriteLine("Dependencies Loaded");
                        log.Info("Dependencies Loaded");
                    }
                    else
                    {
                        msg = $"Warning: {SimParameters.ScenarioName} loaded with no dependencies.";
                        Console.WriteLine(msg);
                        log.Warn(msg);
                    }
                }
                else
                {
                    msg = $"Error loading model for {SimParameters.ScenarioName}.  No model element found in Model File.";
                    Console.WriteLine(msg);
                    log.Fatal(msg);
                    throw new ArgumentException(msg);
                }
            }
            else
            {
                msg = $"Error loading model for {SimParameters.ScenarioName}.  No model file found or loaded.";
                Console.WriteLine(msg);
                log.Fatal(msg);
                throw new ArgumentException(msg);
            }
        }

        public void LoadEvaluator()
        {
            StreamReader jsonStream = new StreamReader(ModelFilePath);
            JObject scenarioJson = JObject.Parse(jsonStream.ReadToEnd());

            if (scenarioJson != null)
            {
                if (JsonLoader<JObject>.TryGetValue("Model", scenarioJson, out JObject modelJson))
                {
                    // Load Environment
                    if(JsonLoader<JObject>.TryGetValue("Evaluator", modelJson, out JObject evaluatorJson))
                    {
                        SchedEvaluator = EvaluatorFactory.GetEvaluator(evaluatorJson, SubList);
                        Console.WriteLine("Evaluator Loaded");
                        log.Info("Evaluator Loaded");
                    }
                    else
                    {
                        SchedEvaluator = new DefaultEvaluator(); // ensures at least default is used
                        Console.WriteLine("Default Evaluator Loaded");
                    }
                }
            }
            
            //var modelInputXMLNode = XmlParser.GetModelNode(ModelFilePath);
            //var evalNodes = modelInputXMLNode.SelectNodes("EVALUATOR");
            //if (evalNodes.Count > 1)
            //{
            //    throw new NotImplementedException("Too many evaluators in input!");
            //    Console.WriteLine("Too many evaluators in input");
            //    log.Info("Too many evaluators in input");
            //}
            //else
            //{
            //    SchedEvaluator = EvaluatorFactory.GetEvaluator(evalNodes[0],SubList);
            //    Console.WriteLine("Evaluator Loaded");
            //    log.Info("Evaluator Loaded");
            //}
        }
        public void CreateSchedules()
        {
            SimSystem = new SystemClass(AssetList, SubList, ConstraintsList, SystemUniverse);

            if (SimSystem.CheckForCircularDependencies())
                throw new NotFiniteNumberException("System has circular dependencies! Please correct then try again.");

            Scheduler _scheduler = new Scheduler(SchedEvaluator);
            Schedules = _scheduler.GenerateSchedules(SimSystem, SystemTasks, InitialSysState);
        }
        public double EvaluateSchedules()
        {
            // Evaluate the schedules and set their values
            foreach (SystemSchedule systemSchedule in Schedules)
            {
                systemSchedule.ScheduleValue = SchedEvaluator.Evaluate(systemSchedule);
                bool canExtendUntilEnd = true;
                // Extend the subsystem states to the end of the simulation 
                foreach (var subsystem in SimSystem.Subsystems)
                {
                    if (systemSchedule.AllStates.Events.Count > 0)
                            if (!subsystem.CanExtend(systemSchedule.AllStates.Events.Peek(), (Domain)SimSystem.Environment, SimParameters.SimEndSeconds))
                            log.Error("Cannot Extend " + subsystem.Name + " to end of simulation");
                }
            }

            // Sort the sysScheds by their values
            Schedules.Sort((x, y) => x.ScheduleValue.CompareTo(y.ScheduleValue));
            Schedules.Reverse();
            double maxSched = Schedules[0].ScheduleValue;
            return maxSched;
        }
    }
}

