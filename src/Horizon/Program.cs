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
using Microsoft.Scripting.Utils;
using System.Linq;
using System.Web.Configuration;

namespace Horizon
{
    public class Program
    {
        public ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string SimulationInputFilePath { get; set; }
        public string TargetDeckFilePath { get; set; }
        public string ModelInputFilePath { get; set; }

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

            // Define the path to the ModelBase folder relative to the executable
            string modelBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\samples\ModelBase");
            modelBasePath = Path.GetFullPath(modelBasePath); // Convert to absolute path


            if (args.Contains("ModelBase")) // Just to make sure not intervening with any of the other sim input files 
            {   // Generate the input file using the provided arguments
                InputFileGenerator generator = new InputFileGenerator(modelBasePath);
                generator.GenerateInputFile(args);

                program.SimulationInputFilePath = Path.Combine(modelBasePath, "ModelBaseScenario.xml");
            }
            else
            {
                List<string> argsList = args.ToList();
                program.InitInput(argsList);
            }

            program.InitOutput(args.ToList());

            program.LoadScenario();
            program.LoadTargets();
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
        public void InitInput(List<string> argsList)
        {
            // This would be in a config file
            string basePath = @"C:\Users\emehiel\Source\Repos\Horizon\Horizon Working";

            string developmentPath = @"..\..\..\..\";
            string subPath = "";

            bool simulationSet = false, targetSet = false, modelSet = false, outputSet = false;

            // Get the input filenames

            // Can't use -scen with other args
            if (argsList.Contains("-scen"))
            {
                List<string> tags = new List<string>() { "-subpath", "-s", "-t", "-m"};
                foreach(var tag in tags)
                {
                    if (argsList.Contains(tag))
                    {
                        Console.WriteLine("The input argument -scen cannot be used with other arguments.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                }
            }

            if (argsList.Contains("-subpath"))
            {
                int indx = argsList.IndexOf("-subpath");
                subPath = argsList[indx + 1];
            }

            if (argsList.Count == 0)
            {
                argsList.Add("-scen");
                // Set this to the default scenario you would like to run
                string scenarioName = "Aeolus";
                argsList.Add(scenarioName);
            }

            int i = 0;
            foreach (var input in argsList)
            {
                i++;
                switch (input)
                {
                    case "-scen":
                        switch (argsList[i])
                        {
                            case "Aeolus":
                                // Set Defaults
                                subPath = @"samples\Aeolus\";
                                SimulationInputFilePath = developmentPath + subPath + @"AeolusSimulationInput.xml";
                                TargetDeckFilePath = developmentPath + subPath + @"v2.2-300targets.xml";
                                // Asset 1 Scripted, Asset 2 C#
                                ModelInputFilePath = developmentPath + subPath + @"DSAC_Static_Mod_Scripted.xml";
                                // Asset 1 mix Scripted/C#, Asset 2 C#
                                //ModelInputFilePath = subpath + @"DSAC_Static_Mod_PartialScripted.xml"; 
                                // Asset 1 C#, Asset 2 C#
                                //ModelInputFilePath = subpath + @"DSAC_Static_Mod.xml";
                                simulationSet = true;
                                targetSet = true;
                                modelSet = true;
                                break;
                            case "myFirstHSFProject":
                                // Set myFirstHSFProject file paths
                                subPath = @"samples\myFirstHSFProject\";
                                SimulationInputFilePath = developmentPath + subPath + @"myFirstHSFScenario.xml";
                                TargetDeckFilePath = developmentPath + subPath + @"myFirstHSFTargetDeck.xml";
                                ModelInputFilePath = developmentPath + subPath + @"myFirstHSFSystem.xml";
                                simulationSet = true;
                                targetSet = true;
                                modelSet = true;
                                break;
                            case "myFirstHSFProjectLook":
                                // Set myFirstHSFProjectConstraint file paths
                                subPath = @"samples\myFirstHSFProjectConstraint\";
                                SimulationInputFilePath = developmentPath + subPath + @"myFirstHSFScenario.xml";
                                TargetDeckFilePath = developmentPath + subPath + @"myFirstHSFTargetDeck.xml";
                                ModelInputFilePath = developmentPath + subPath + @"myFirstHSFSystemLook.xml";
                                simulationSet = true;
                                targetSet = true;
                                modelSet = true;
                                break;
                            case "myFirstHSFProjectDependency":
                                // Set myFirstHSFProjectDependency file paths
                                subPath = @"samples\myFirstHSFProjectDependency\";
                                SimulationInputFilePath = developmentPath + subPath + @"myFirstHSFScenario.xml";
                                TargetDeckFilePath = developmentPath + subPath + @"myFirstHSFTargetDeck.xml";
                                ModelInputFilePath = developmentPath + subPath + @"myFirstHSFSystemDependency.xml";
                                simulationSet = true;
                                targetSet = true;
                                modelSet = true;
                                break;
                            case "ModelBase":
                                // Set ModelBase file paths
                                subPath = @"samples\ModelBase\";
                                SimulationInputFilePath = developmentPath + subPath + @"ModelBaseScenario.xml";
                                TargetDeckFilePath = developmentPath + subPath + @"ModelBaseTargetDeck.xml";
                                ModelInputFilePath = developmentPath + subPath + @"ModelBaseSystemDependency.xml";
                                simulationSet = true;
                                targetSet = true;
                                modelSet = true;
                                break;
                        }
                        break;
                    case "-s":
                        SimulationInputFilePath = developmentPath + subPath + argsList[i];
                        simulationSet = true;
                        break;
                    case "-t":
                        TargetDeckFilePath = developmentPath + subPath + argsList[i];
                        targetSet = true;
                        break;
                    case "-m":
                        ModelInputFilePath = developmentPath + subPath + argsList[i];
                        modelSet = true;
                        break;
                    case "-o":
                        OutputPath = argsList[i];
                        outputSet = true;
                        break;
                }
            }

            if (simulationSet)
            {
                Console.WriteLine("Using simulation file: " + SimulationInputFilePath);
                log.Info("Using simulation file: " + SimulationInputFilePath);
            }

            if (targetSet)
            {
                Console.WriteLine("Using target deck file: " + TargetDeckFilePath);
                log.Info("Using simulation file: " + TargetDeckFilePath);
            }

            if (modelSet)
            {
                Console.WriteLine("Using model file: " + ModelInputFilePath);
                log.Info("Using model file: " + ModelInputFilePath);
            }

        }
        public void InitOutput(List<string> argsList)
        {
            // Initialize Output File
            var outputFileName = string.Format("output-{0:yyyy-MM-dd}-*", DateTime.Now);
            string outputPath = "";

            if (argsList.Contains("-o"))
            {
                int indx = argsList.IndexOf("-o");
                outputPath = argsList[indx + 1];
            }
            else
                outputPath = @"C:\HorizonLog\";

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
            // Find the main input node from the XML input files
            XmlParser.ParseSimulationInput(SimulationInputFilePath);
        }
        public void LoadTargets()
        {
            // Load the target deck into the targets list from the XML target deck input file
            bool targetsLoaded = Task.loadTargetsIntoTaskList(XmlParser.GetTargetNode(TargetDeckFilePath), SystemTasks);
            if (!targetsLoaded)
            {
                throw new Exception("Targets were not loaded.");
            }

        }
        public void LoadSubsystems()
        {

            // Find the main model node from the XML model input file
            var modelInputXMLNode = XmlParser.GetModelNode(ModelInputFilePath);

            var environments = modelInputXMLNode.SelectNodes("ENVIRONMENT");

            // Check if environment count is empty, default is space
            if (environments.Count == 0)
            {
                SystemUniverse = new SpaceEnvironment();
                Console.WriteLine("Default Space Environment Loaded");
                log.Info("Default Space Environment Loaded");
            }
            
            // Load Environments
            foreach (XmlNode environmentNode in environments)
            {
                SystemUniverse = UniverseFactory.GetUniverseClass(environmentNode);
            }

            var snakes = modelInputXMLNode.SelectNodes("PYTHON");
            foreach (XmlNode pythonNode in snakes)
            {
                throw new NotImplementedException();
            }

            // Load Assets
            var assets = modelInputXMLNode.SelectNodes("ASSET");
            foreach(XmlNode assetNode in assets)
            {
                Asset asset = new Asset(assetNode);
                asset.AssetDynamicState.Eoms.SetEnvironment(SystemUniverse);
                AssetList.Add(asset);

                // Load Subsystems
                var subsystems = assetNode.SelectNodes("SUBSYSTEM");

                foreach (XmlNode subsystemNode in subsystems)
                {
                    Subsystem subsys = SubsystemFactory.GetSubsystem(subsystemNode, asset);
                    SubList.Add(subsys);

                    // Load States (Formerly ICs)
                    var States = subsystemNode.SelectNodes("STATE");

                    foreach (XmlNode StateNode in States)
                    {
                        // Parse state node for key name and state type, add the key to the subsys's list of keys, return the key name
                        string keyName = SubsystemFactory.SetStateKeys(StateNode, subsys);
                        // Use key name and state type to set initial conditions 
                        InitialSysState.SetInitialSystemState(StateNode, keyName);
                    }

                    if (subsys.Type == "scripted")
                    {
                        // Load Subsystem Parameters
                        var parameters = subsystemNode.SelectNodes("PARAMETER");

                        foreach (XmlNode parameterNode in parameters)
                        {
                            SubsystemFactory.SetParamenters(parameterNode, subsys);
                        }
                    }
                }

                // Load Constraints
                var constraints = assetNode.SelectNodes("CONSTRAINT");

                foreach (XmlNode constraintNode in constraints)
                {
                    ConstraintsList.Add(ConstraintFactory.GetConstraint(constraintNode, SubList, asset));
                }
            }
            Console.WriteLine("Environment, Assets, and Constraints Loaded");
            log.Info("Environment, Assets, and Constraints Loaded");

            // Load Dependencies
            var dependencies = modelInputXMLNode.SelectNodes("DEPENDENCY");

            foreach (XmlNode dependencyNode in dependencies)
            {
                //var SubFact = new SubsystemFactory();
                SubsystemFactory.SetDependencies(dependencyNode, SubList);
            }
            Console.WriteLine("Dependencies Loaded");
            log.Info("Dependencies Loaded");
        }

        public void LoadEvaluator()
        {
            var modelInputXMLNode = XmlParser.GetModelNode(ModelInputFilePath);
            var evalNodes = modelInputXMLNode.SelectNodes("EVALUATOR");
            if (evalNodes.Count > 1)
            {
                throw new NotImplementedException("Too many evaluators in input!");
                Console.WriteLine("Too many evaluators in input");
                log.Info("Too many evaluators in input");
            }
            else
            {
                SchedEvaluator = EvaluatorFactory.GetEvaluator(evalNodes[0],SubList);
                Console.WriteLine("Evaluator Loaded");
                log.Info("Evaluator Loaded");
            }
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

