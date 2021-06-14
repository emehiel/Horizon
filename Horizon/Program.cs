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

        public string SimulationInputFilePath { get; set; } //wanted to make read only but not sure how to set from 
        public string TargetDeckFilePath { get; set; }
        public string ModelInputFilePath { get; set; }
        

        // Load the environment. First check if there is an ENVIRONMENT XMLNode in the input file
        public Domain SystemUniverse = null;

        //Create singleton dependency dictionary
        Dependency dependencies = Dependency.Instance;
        public Dependency _dependencies = Dependency.Instance;

        // Initialize List to hold assets and subsystem nodes
        public List<Asset> AssetList = new List<Asset>();
        public List<Subsystem> SubList = new List<Subsystem>();

        // Maps used to set up preceeding nodes
        Dictionary<ISubsystem, XmlNode> subsystemXMLNodeMap = new Dictionary<ISubsystem, XmlNode>();
        Dictionary<string, Subsystem> _subsystemMap = new Dictionary<string, Subsystem>();
        public Dictionary<string, Subsystem> SubsystemMap = new Dictionary<string, Subsystem>();
        List<KeyValuePair<string, string>> _dependencyMap = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> DependencyMap = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, string>> _dependencyFcnMap = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> DependencyFcnMap = new List<KeyValuePair<string, string>>();
        // Dictionary<string, ScriptedSubsystem> scriptedSubNames = new Dictionary<string, ScriptedSubsystem>();

        // Create Constraint list 
        List<Constraint> _constraintsList = new List<Constraint>();
        public List<Constraint> ConstraintsList = new List<Constraint>(); //Public Version for Unit Testing

        //Create Lists to hold all the initial condition and dependency nodes to be parsed later
        List<XmlNode> ICNodes = new List<XmlNode>();
        List<XmlNode> DepNodes = new List<XmlNode>();
        public SystemState InitialSysState = new SystemState();

        XmlNode evaluatorNode;
        Evaluator schedEvaluator;
        public List<SystemSchedule> schedules;
        SystemClass simSystem { get; set; }

        static int Main(string[] args)
        {
            Program program = new Program();
            // Begin the Logger
            program.log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            program.log.Info("STARTING HSF RUN"); //Do not delete
            program.InitInput(args);
            string outputPath = program.InitOutput();
            Stack<Task> systemTasks = program.LoadTargets();
            Stack<Task> newTasks = new Stack<Task> (systemTasks);
            program.LoadSubsystems();
            program.LoadDependencies();
            program.CreateSchedules(systemTasks);
            double maxSched = program.EvaluateSchedules();

            int i = 0;
            //Morgan's Way
            using (StreamWriter sw = File.CreateText(outputPath))
            {
                foreach (SystemSchedule sched in program.schedules)
                {
                    sw.WriteLine("Schedule Number: " + i + "Schedule Value: " + program.schedules[i].ScheduleValue);
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
            SystemSchedule.WriteSchedule(program.schedules[0], stateDataFilePath);

            var csv = new StringBuilder();
            csv.Clear();
            foreach (var asset in program.simSystem.Assets)
            {
                File.WriteAllText(@"..\..\..\" + asset.Name + "_dynamicStateData.csv", asset.AssetDynamicState.ToString());
            }

            //Console.ReadKey();
            return 0;
        }
        public void InitInput(string[] args)
        {
            // Set Defaults
            SimulationInputFilePath = @"..\..\..\SimulationInput.XML";
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
                        break;
                    case "-t":
                        TargetDeckFilePath = args[i];
                        targetSet = true;
                        break;
                    case "-m":
                        ModelInputFilePath = args[i];
                        modelSet = true;
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
        public Stack<Task> LoadTargets()
        {
            // Find the main input node from the XML input files
            evaluatorNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);

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
            foreach (XmlNode modelChildNode in modelInputXMLNode.ChildNodes)
            {
                if (modelChildNode.Name.Equals("ENVIRONMENT"))
                {
                    // Create the Environment based on the XMLNode
                    SystemUniverse = UniverseFactory.GetUniverseClass(modelChildNode);
                }
                else if (SystemUniverse == null)
                    SystemUniverse = new SpaceEnvironment();

                if (modelChildNode.Name.Equals("ASSET"))
                {
                    Asset asset = new Asset(modelChildNode);
                    asset.AssetDynamicState.Eoms.SetEnvironment(SystemUniverse);

                    AssetList.Add(asset);
                    // Loop through all the of the ChildNodess for this Asset
                    foreach (XmlNode childNode in modelChildNode.ChildNodes)
                    {
                        // Get the current Subsystem XML Node, and create it using the SubsystemFactory
                        if (childNode.Name.Equals("SUBSYSTEM"))
                        {  //is this how we want to do this?
                           // Check if the type of the Subsystem is scripted, networked, or other
                            string subName = SubsystemFactory.GetSubsystem(childNode, dependencies, asset, _subsystemMap);
                            foreach (XmlNode ICorDepNode in childNode.ChildNodes)
                            {
                                if (ICorDepNode.Name.Equals("IC"))
                                    ICNodes.Add(ICorDepNode);
                                if (ICorDepNode.Name.Equals("DEPENDENCY"))
                                {
                                    string depSubName = "", depFunc = "";
                                    depSubName = Subsystem.parseNameFromXmlNode(ICorDepNode, asset.Name);
                                    _dependencyMap.Add(new KeyValuePair<string, string>(subName, depSubName));

                                    if (ICorDepNode.Attributes["fcnName"] != null)
                                    {
                                        depFunc = ICorDepNode.Attributes["fcnName"].Value.ToString();
                                        _dependencyFcnMap.Add(new KeyValuePair<string, string>(subName, depFunc));
                                    }
                                }
                            }
                        }
                        //Create a new Constraint
                        if (childNode.Name.Equals("CONSTRAINT"))
                        {
                            _constraintsList.Add(ConstraintFactory.GetConstraint(childNode, _subsystemMap, asset));
                        }
                    }
                    if (ICNodes.Count > 0)
                        InitialSysState.Add(SystemState.setInitialSystemState(ICNodes, asset));
                    ICNodes.Clear();
                }
            }

            foreach (KeyValuePair<string, Subsystem> sub in _subsystemMap)
            {
                if (!sub.Value.GetType().Equals(typeof(ScriptedSubsystem)))//let the scripted subsystems add their own dependency collector
                sub.Value.AddDependencyCollector();

                SubList.Add(sub.Value);
            }
            log.Info("Subsystems and Constraints Loaded");
            ConstraintsList = _constraintsList;
            DependencyFcnMap = _dependencyFcnMap;
            DependencyMap = _dependencyMap;
            SubsystemMap = _subsystemMap;

        }
        public void LoadDependencies()
        {
            //Add all the dependent subsystems to the dependent subsystem list of the subsystems
            foreach (KeyValuePair<string, string> depSubPair in _dependencyMap)
            {
                Subsystem subToAddDep, depSub;
                _subsystemMap.TryGetValue(depSubPair.Key, out subToAddDep);
                _subsystemMap.TryGetValue(depSubPair.Value, out depSub);
                subToAddDep.DependentSubsystems.Add(depSub);
            }

            //give the dependency functions to all the subsytems that need them
            foreach (KeyValuePair<string, string> depFunc in _dependencyFcnMap)
            {
                Subsystem subToAddDep;
                _subsystemMap.TryGetValue(depFunc.Key, out subToAddDep);
                subToAddDep.SubsystemDependencyFunctions.Add(depFunc.Value, dependencies.GetDependencyFunc(depFunc.Value));
            }
            _dependencies = dependencies;
            log.Info("Dependencies Loaded");
        }
        public void CreateSchedules(Stack<Task> systemTasks)
        {
            simSystem = new SystemClass(AssetList, SubList, _constraintsList, SystemUniverse);

            if (simSystem.CheckForCircularDependencies())
                throw new NotFiniteNumberException("System has circular dependencies! Please correct then try again.");

            schedEvaluator = EvaluatorFactory.GetEvaluator(evaluatorNode, dependencies);
            Scheduler scheduler = new Scheduler(schedEvaluator);
            schedules = scheduler.GenerateSchedules(simSystem, systemTasks, InitialSysState);
        }
        public double EvaluateSchedules()
        {
            // Evaluate the schedules and set their values
            foreach (SystemSchedule systemSchedule in schedules)
            {
                systemSchedule.ScheduleValue = schedEvaluator.Evaluate(systemSchedule);
                bool canExtendUntilEnd = true;
                // Extend the subsystem states to the end of the simulation 
                foreach (var subsystem in simSystem.Subsystems)
                {
                    if (systemSchedule.AllStates.Events.Count > 0)
                        if (!subsystem.CanExtend(systemSchedule.AllStates.Events.Peek(), (Domain)simSystem.Environment, SimParameters.SimEndSeconds))
                            log.Error("Cannot Extend " + subsystem.Name + " to end of simulation");
                }
            }

            // Sort the sysScheds by their values
            schedules.Sort((x, y) => x.ScheduleValue.CompareTo(y.ScheduleValue));
            schedules.Reverse();
            double maxSched = schedules[0].ScheduleValue;
            return maxSched;
        }
    }
}

