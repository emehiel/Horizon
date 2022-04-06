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
using HSFSubsystem;
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

        // Initialize List to hold assets and subsystem nodes
        public List<Asset> AssetList { get; set; } = new List<Asset>();
        public List<Subsystem> SubList { get; set; } = new List<Subsystem>();

        // Maps used to set up preceeding nodes
        public Dictionary<ISubsystem, XmlNode> SubsystemXMLNodeMap { get; set; } = new Dictionary<ISubsystem, XmlNode>();
        public Dictionary<string, Subsystem> SubsystemMap { get; set; } = new Dictionary<string, Subsystem>();
        public List<KeyValuePair<string, string>> DependencyList { get; set; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> DependencyFcnList { get; set; } = new List<KeyValuePair<string, string>>();
        
        // Create Constraint list 
        public List<Constraint> ConstraintsList { get; set; } = new List<Constraint>();

        //Create Lists to hold all the dependency nodes to be parsed later
        List<XmlNode> _depNodes = new List<XmlNode>();
        public SystemState InitialSysState { get; set; } = new SystemState();

        XmlNode _evaluatorNode;
        Evaluator _schedEvaluator;
        public List<SystemSchedule> Schedules { get; set; }
        public SystemClass SimSystem { get; set; }

        public static int Main(string[] args) //
        {
            Program program = new Program();
            // Begin the Logger
            program.log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            program.log.Info("STARTING HSF RUN"); //Do not delete
            program.InitInput(args);
            string outputPath = program.InitOutput();
            program.LoadScenario();
            Stack<Task> systemTasks = program.LoadTargets();
            program.LoadSubsystems();
            program.LoadDependencies();
            program.LoadEvaluator();
            program.CreateSchedules(systemTasks);
            double maxSched = program.EvaluateSchedules();

            int i = 0;
            //Morgan's Way
            using (StreamWriter sw = File.CreateText(outputPath))
            {
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
            }

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
            SimulationInputFilePath = @"..\..\..\SimulationInput.xml";
            TargetDeckFilePath = @"..\..\..\v2.2-300targets.xml";
            ModelInputFilePath = @"..\..\..\DSAC_Static.xml";
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
                log.Info("Using Default Simulation File");
            }

            if (!targetSet)
            {
                log.Info("Using Default Target File");
            }

            if (!modelSet)
            {
                log.Info("Using Default Model File");
            }

        }
        public string InitOutput()
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
            return outputPath;
        }

        public void LoadScenario()
        {
            // Find the main input node from the XML input files
            _evaluatorNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);
        }
        public Stack<Task> LoadTargets()
        {
            // Load the target deck into the targets list from the XML target deck input file
            Stack<Task> systemTasks = new Stack<Task>();
            bool targetsLoaded = Task.loadTargetsIntoTaskList(XmlParser.GetTargetNode(TargetDeckFilePath), systemTasks);
            if (!targetsLoaded)
            {
                throw new Exception("Targets were not loaded.");
            }

            return systemTasks;

        }
        public void LoadSubsystems()
        {

            // Find the main model node from the XML model input file
            var modelInputXMLNode = XmlParser.GetModelNode(ModelInputFilePath);

            // Set up Subsystem Nodes, first loop through the assets in the XML model input file
            foreach (XmlNode modelNode in modelInputXMLNode.ChildNodes)
            {
                switch (modelNode.Name.ToLower())
                {
                    case ("environment"):
                        SystemUniverse = UniverseFactory.GetUniverseClass(modelNode);
                        break;
                    case ("python"):
                        throw new NotImplementedException();
                        break;
                    case ("asset"):
                        Asset asset = new Asset(modelNode);
                        asset.AssetDynamicState.Eoms.SetEnvironment(SystemUniverse);

                        AssetList.Add(asset);
                        // Loop through all the of the ChildNodess for this Asset
                        foreach (XmlNode assetNode in modelNode.ChildNodes)
                        {
                            switch (assetNode.Name.ToLower())
                            {
                                case ("subsystem"):
                                    string subName = SubsystemFactory.GetSubsystem(assetNode, Dependencies, asset, SubsystemMap);
                                    SubList.Add(SubsystemMap[subName]);
                                    foreach (XmlNode subNode in assetNode.ChildNodes)
                                    {
                                        switch (subNode.Name.ToLower())
                                        {
                                            case ("ic"):
                                                InitialSysState.Add(SystemState.SetInitialSystemState(subNode, asset));
                                                break;
                                            case ("dependency"):

                                                string depSubName;
                                                string depFunc;
                                                depSubName = Subsystem.parseNameFromXmlNode(subNode, asset.Name);
                                                DependencyList.Add(new KeyValuePair<string, string>(subName, depSubName));

                                                if (subNode.Attributes["fcnName"] != null)
                                                {
                                                    depFunc = subNode.Attributes["fcnName"].Value.ToString();
                                                    DependencyFcnList.Add(new KeyValuePair<string, string>(subName, depFunc));
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case ("constraint"):
                                    ConstraintsList.Add(ConstraintFactory.GetConstraint(assetNode, SubsystemMap, asset));
                                    break;
                            }
                        }
                        break;
                    default:
                        SystemUniverse = new SpaceEnvironment();
                        break;
                }
            }
            log.Info("Subsystems and Constraints Loaded");
        }
        public void LoadDependencies()
        {
            //Add all the dependent subsystems to the dependent subsystem list of the subsystems
            foreach (KeyValuePair<string, string> depSubPair in DependencyList)
            {
                Subsystem subToAddDep;
                Subsystem depSub;
                SubsystemMap.TryGetValue(depSubPair.Key, out subToAddDep);
                SubsystemMap.TryGetValue(depSubPair.Value, out depSub);
                subToAddDep.DependentSubsystems.Add(depSub);
            }

            //give the dependency functions to all the subsytems that need them
            foreach (KeyValuePair<string, string> depFunc in DependencyFcnList)
            {
                Subsystem subToAddDep;
                SubsystemMap.TryGetValue(depFunc.Key, out subToAddDep);
                subToAddDep.SubsystemDependencyFunctions.Add(depFunc.Value, Dependencies.GetDependencyFunc(depFunc.Value));
            }
            //Dependencies = dependencies;
            log.Info("Dependencies Loaded");
        }
        public void LoadEvaluator()
        {
            _schedEvaluator = EvaluatorFactory.GetEvaluator(_evaluatorNode, Dependencies);
        }
        public void CreateSchedules(Stack<Task> systemTasks)
        {
            SimSystem = new SystemClass(AssetList, SubList, ConstraintsList, SystemUniverse);

            if (SimSystem.CheckForCircularDependencies())
                throw new NotFiniteNumberException("System has circular dependencies! Please correct then try again.");

            Scheduler _scheduler = new Scheduler(_schedEvaluator);
            Schedules = _scheduler.GenerateSchedules(SimSystem, systemTasks, InitialSysState);
        }
        public double EvaluateSchedules()
        {
            // Evaluate the schedules and set their values
            foreach (SystemSchedule systemSchedule in Schedules)
            {
                systemSchedule.ScheduleValue = _schedEvaluator.Evaluate(systemSchedule);
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

