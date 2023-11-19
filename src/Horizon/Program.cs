// Copyright (c) 2016-2023 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu) Scott Plantenga (splantenga@hotmail.com)

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

            var M = new Matrix<double>(3, 1, 0);
            Console.WriteLine(M);
            program.log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            program.log.Info("STARTING HSF RUN"); //Do not delete
            program.InitInput(args);
            program.InitOutput();
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

            //Console.ReadKey();
            return 0;
        }
        public void InitInput(string[] args)
        {
            // Set Defaults
            SimulationInputFilePath = @"..\..\..\..\samples\Aeolus\AeolusSimulationInput.xml";
            TargetDeckFilePath = @"..\..\..\..\samples\Aeolus\v2.2-300targets.xml";
            ModelInputFilePath = @"..\..\..\..\samples\Aeolus\DSAC_Static_Mod_Scripted.xml"; // Asset 1 Scripted, Asset 2 C#
            //ModelInputFilePath = @"..\..\..\..\samples\Aeolus\DSAC_Static_Mod_PartialScripted.xml"; // Asset 1 mix Scripted/C#, Asset 2 C#
            //ModelInputFilePath = @"..\..\..\..\samples\Aeolus\DSAC_Static_Mod.xml"; // Asset 1 C#, Asset 2 C#

            bool simulationSet = false, targetSet = false, modelSet = false;

            // Get the input filenames
            int i = 0;
            foreach (var input in args)
            {
                i++;
                switch (input)
                {
                    case "-s":
                        SimulationInputFilePath = args[i];
                        simulationSet = true;
                        Console.WriteLine("Using custom simulation file: " + SimulationInputFilePath);
                        log.Info("Using custom simulation file: " + SimulationInputFilePath);
                        break;
                    case "-t":
                        TargetDeckFilePath = args[i];
                        targetSet = true;
                        Console.WriteLine("Using custom target deck file: " + TargetDeckFilePath);
                        log.Info("Using custom simulation file: " + TargetDeckFilePath);
                        break;
                    case "-m":
                        ModelInputFilePath = args[i];
                        modelSet = true;
                        Console.WriteLine("Using custom model file: " + ModelInputFilePath);
                        log.Info("Using custom model file: " + ModelInputFilePath);
                        break;
                }
            }
            ///add usage statement

            if (!simulationSet)
            {
                Console.WriteLine("Using Default Simulation File");
                log.Info("Using Default Simulation File");
            }

            if (!targetSet)
            {
                log.Info("Using Default Simulation File");
            }

            if (!modelSet)
            {
                log.Info("Using Default Simulation File");
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
                if (asset.AssetDynamicState.Eoms != null)
                {
                    asset.AssetDynamicState.Eoms.SetEnvironment(SystemUniverse);
                }
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
                var SubFact = new SubsystemFactory();
                SubFact.SetDependencies(dependencyNode, SubList);
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
                throw new NotImplementedException("Too many evaluators in XML input!");
                Console.WriteLine("Too many evaluators in XML input");
                log.Info("Too many evaluators in XML input");
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

