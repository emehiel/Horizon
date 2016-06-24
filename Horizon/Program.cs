// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using HSFScheduler;
using Utilities;
using MissionElements;
using UserModel;
using HSFUniverse;
using HSFSubsystem;
using HSFSystem;
using log4net;

namespace Horizon
{
    public class Program
    {
        static int Main(string[] args)
        {
            // Begin the Logger
            ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("STARTING HSF RUN"); //Do not delete

            // Set Defaults
            var simulationInputFilePath = @"..\..\..\SimulationInput.XML";
            var targetDeckFilePath = @"..\..\..\v2.2-300targets.xml";
            var modelInputFilePath = @"..\..\..\DSAC_Static.xml";
            bool simulationSet = false, targetSet = false, modelSet = false;

            // Get the input filenames
            int i = 0;
            foreach(var input in args)
            {
                i++;
                switch (input)
                {
                    case "-s":
                        simulationInputFilePath = args[i];
                        simulationSet = true;
                        break;
                    case "-t":
                        targetDeckFilePath = args[i];
                        targetSet = true;
                        break;
                    case "-m":
                        modelInputFilePath = args[i];
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


            // Initialize Output File
            var outputFileName = string.Format("output-{0:yyyy-MM-dd}-*", DateTime.Now);
            var outputPath = @"C:\HorizonLog\";
            var txt = ".txt";
            string[] fileNames = System.IO.Directory.GetFiles(outputPath, outputFileName, System.IO.SearchOption.TopDirectoryOnly);
            double number = 0;
            foreach (var fileName in fileNames)
            {
                char version = fileName[fileName.Length - txt.Length-1];
                if(number < Char.GetNumericValue(version))
                    number = Char.GetNumericValue(version);
            }
            number++;
            outputFileName = outputFileName.Remove(outputFileName.Length - 1) + number;
            outputPath += outputFileName + txt;
            // Find the main input node from the XML input files
            XmlNode evaluatorNode = XmlParser.ParseSimulationInput(simulationInputFilePath);

            // Load the target deck into the targets list from the XML target deck input file
            Stack<Task> systemTasks = new Stack<Task>();
            bool targetsLoaded = Task.loadTargetsIntoTaskList(XmlParser.GetTargetNode(targetDeckFilePath), systemTasks);
            if (!targetsLoaded)
            {
                return 1;
            }

            // Find the main model node from the XML model input file
            var modelInputXMLNode = XmlParser.GetModelNode(modelInputFilePath);

            // Load the environment. First check if there is an ENVIRONMENT XMLNode in the input file
            Universe SystemUniverse = null;

            //Create singleton dependency dictionary
            Dependency dependencies = Dependency.Instance;

            // Initialize List to hold assets and subsystem nodes
            List<Asset> assetList = new List<Asset>();
            List<Subsystem> subList = new List<Subsystem>();

            // Maps used to set up preceeding nodes
            Dictionary<ISubsystem, XmlNode> subsystemXMLNodeMap = new Dictionary<ISubsystem, XmlNode>();
            Dictionary<string, Subsystem> subsystemMap = new Dictionary<string, Subsystem>();
            List<KeyValuePair<string, string>> dependencyMap = new List<KeyValuePair<string, string>>();
            List<KeyValuePair<string, string>> dependencyFcnMap = new List<KeyValuePair<string, string>>();
            // Dictionary<string, ScriptedSubsystem> scriptedSubNames = new Dictionary<string, ScriptedSubsystem>();

            // Create Constraint list 
            List<Constraint> constraintsList = new List<Constraint>();

            //Create Lists to hold all the initial condition and dependency nodes to be parsed later
            List<XmlNode> ICNodes = new List<XmlNode>();
            List<XmlNode> DepNodes = new List<XmlNode>();
            SystemState initialSysState = new SystemState();

            // Set up Subsystem Nodes, first loop through the assets in the XML model input file
            foreach (XmlNode modelChildNode in modelInputXMLNode.ChildNodes)
            {
                if (modelChildNode.Name.Equals("ENVIRONMENT"))
                {
                    // Create the Environment based on the XMLNode
                    SystemUniverse = new Universe(modelChildNode);
                }
                if (modelChildNode.Name.Equals("ASSET"))
                {
                    Asset asset = new Asset(modelChildNode);
                    assetList.Add(asset);
                    // Loop through all the of the ChildNodess for this Asset
                    foreach (XmlNode childNode in modelChildNode.ChildNodes)
                    {
                        // Get the current Subsystem XML Node, and create it using the SubsystemFactory
                        if (childNode.Name.Equals("SUBSYSTEM"))
                        {  //is this how we want to do this?
                            // Check if the type of the Subsystem is scripted, networked, or other
                            string subName = SubsystemFactory.GetSubsystem(childNode, dependencies, asset, subsystemMap);
                            foreach (XmlNode ICorDepNode in childNode.ChildNodes)
                            {
                                if(ICorDepNode.Name.Equals("IC"))
                                    ICNodes.Add(ICorDepNode);
                                if (ICorDepNode.Name.Equals("DEPENDENCY"))
                                {
                                    string depSubName = "", depFunc = "";
                                    depSubName = Subsystem.parseNameFromXmlNode(ICorDepNode, asset.Name) ;
                                    dependencyMap.Add(new KeyValuePair<string, string>(subName, depSubName));

                                    if (ICorDepNode.Attributes["fcnName"] != null)
                                    {
                                        depFunc = ICorDepNode.Attributes["fcnName"].Value.ToString();
                                        dependencyFcnMap.Add(new KeyValuePair<string, string>(subName, depFunc));
                                    }
                                }  
                            }
                        }
                        //Create a new Constraint
                        if (childNode.Name.Equals("CONSTRAINT"))
                        {
                            constraintsList.Add(ConstraintFactory.GetConstraint(childNode, subsystemMap, asset));
                        }
                    }
                    if (ICNodes.Count > 0)
                        initialSysState.Add(SystemState.setInitialSystemState(ICNodes, asset));
                    ICNodes.Clear();
                }
            }
            if (SystemUniverse == null)
                SystemUniverse = new Universe();

            foreach (KeyValuePair<string, Subsystem> sub in subsystemMap)
            {
                if(!sub.Value.GetType().Equals(typeof(ScriptedSubsystem)))//let the scripted subsystems add their own dependency collector
                    sub.Value.AddDependencyCollector();
                subList.Add(sub.Value);
            }
            log.Info("Subsystems and Constraints Loaded");

            //Add all the dependent subsystems to the dependent subsystem list of the subsystems
            foreach (KeyValuePair<string, string> depSubPair in dependencyMap)
            {
                Subsystem subToAddDep, depSub;
                subsystemMap.TryGetValue(depSubPair.Key, out subToAddDep);
                subsystemMap.TryGetValue(depSubPair.Value, out depSub);
                subToAddDep.DependentSubsystems.Add(depSub);
            }

            //give the dependency functions to all the subsytems that need them
            foreach (KeyValuePair<string, string> depFunc in dependencyFcnMap)
            {
                Subsystem subToAddDep;
                subsystemMap.TryGetValue(depFunc.Key, out subToAddDep);
                subToAddDep.SubsystemDependencyFunctions.Add(depFunc.Value, dependencies.GetDependencyFunc(depFunc.Value));
            }
            log.Info("Dependencies Loaded");

            SystemClass simSystem = new SystemClass(assetList, subList, constraintsList, SystemUniverse);

            if (simSystem.CheckForCircularDependencies())
                throw new NotFiniteNumberException("System has circular dependencies! Please correct then try again.");

            Evaluator schedEvaluator = EvaluatorFactory.GetEvaluator(evaluatorNode, dependencies);
            Scheduler scheduler = new Scheduler(schedEvaluator);
            List<SystemSchedule> schedules = scheduler.GenerateSchedules(simSystem, systemTasks, initialSysState);
            // Evaluate the schedules and set their values
            foreach (SystemSchedule systemSchedule in schedules)
            {
                systemSchedule.ScheduleValue = schedEvaluator.Evaluate(systemSchedule);
                bool canExtendUntilEnd = true;
                // Extend the subsystem states to the end of the simulation 
                foreach (var subsystem in simSystem.Subsystems)
                {
                    if(systemSchedule.AllStates.Events.Count >0)
                        if (!subsystem.CanExtend(systemSchedule.AllStates.Events.Peek(), simSystem.Environment, SimParameters.SimEndSeconds))
                            log.Error("Cannot Extend " + subsystem.Name + " to end of simulation");
                }
            }

            // Sort the sysScheds by their values
            schedules.Sort((x, y) => x.ScheduleValue.CompareTo(y.ScheduleValue));
            schedules.Reverse();
            double maxSched = schedules[0].ScheduleValue;
            i = 0;
            //Morgan's Way
            using (StreamWriter sw = File.CreateText(outputPath))
            {
                foreach (SystemSchedule sched in schedules)
                {
                    sw.WriteLine("Schedule Number: " + i + "Schedule Value: " + schedules[i].ScheduleValue);
                    foreach (var eit in sched.AllStates.Events)
                    {
                        if (i < 5)//just compare the first 5 schedules for now
                        { 
                            sw.WriteLine(eit.ToString());
                        }
                    }
                    i++;
            }
            log.Info("Max Schedule Value: " + maxSched);
            }

            // Mehiel's way
            string stateDataFilePath = @"C:\HorizonLog\" + string.Format("output-{0:yyyy-MM-dd-hh-mm-ss}", DateTime.Now);
            SystemSchedule.WriteSchedule(schedules[0], stateDataFilePath);

            var csv = new StringBuilder();
            csv.Clear();
            foreach (var asset in simSystem.Assets)
            {
                File.WriteAllText(@"..\..\..\" + asset.Name + "_dynamicStateData.csv", asset.AssetDynamicState.ToString());
            }
            return 0;
            //   Console.ReadKey();
        }
    }
}
