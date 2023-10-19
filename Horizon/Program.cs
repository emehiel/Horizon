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
using System.Linq;
using System.Threading;
using System.Runtime.CompilerServices;
using System.IO.Pipes;
using IronPython.Compiler.Ast;
using System.Security.Policy;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using IronPython.Runtime.Operations;
using System.CodeDom;

namespace Horizon
{
    public class Program
    {
        #region GUI-enabled necessary attributes
        // Create a start signal for the GUI (Jackson Pollock) to invoke Horizon. 
        private static ManualResetEvent guiStartSignal = new ManualResetEvent(false); // initially
        private static bool isRunning = true; //A bool to show that Horizon is running
        private static bool argsFromGUI = false;

        #endregion

        #region Standard HSF Attributes
        public ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string TopHorizonDir { get; set; }
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
        public List<Task> SystemTasks { get; set; } = new List<Task>();
        #endregion

        public static int Main(string[] args) //
        {
            // The main Horizon fucntion is now a Lambda function; otherwise unchanged.
            #region ConsoleApp Main Function
            Func<int> mainHorizon = () =>
            {
                Program program = new Program();
                // Begin the Logger
                program.log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                program.log.Info("STARTING HSF RUN"); //Do not delete
                program.InitInput(args);
                program.InitOutput();
                program.LoadScenario();
                program.LoadTargets();
                program.LoadSubsystems();
                program.LoadEvaluator();
                program.CreateSchedules();
                // Schedules are already evaluated and sorted when they are generated in CreateSchedules().  This method
                // also runs the CanExtend() method which I think we want to eliminate...
                //double maxSched = program.EvaluateSchedules();

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
                var maxValue = program.Schedules.Max(s => s.ScheduleValue);
                program.log.Info("Max Schedule Value: " + maxValue);

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
            };
            #endregion

            // This is the main entry point of Main; adjsusted to accept the GUI 
            #region GUI Wrapping Logic

            /*  The GUI and mainHorizon are seperate C# projects; Horizon & all ref/dependencies are ref/dependencies of
                JacksonPollock, the GUI. These references and (project) dependencies need to be added manually; automated
                functionality is not yet built in... This is also supposed to be experimental as a "Quick 'n' Dirty" version
                of a GUI implentation for Horizon to help me develop quicker... The name is a play off of 'Picasso', the 
                heritage GUI. 

                Furthermore, 
            */

            bool statusGUI = checkStatusGUI(); //Checks if GUI is running with fast timeout
            if (statusGUI) // Setups the dual-startup project in VS
            {
                using (NamedPipeClientStream clientStream = new NamedPipeClientStream("pipe_JacksonPollockGUI"))
                {
                    try
                    {
                        clientStream.Connect(); //Connect to GUI thread (stream (?)) 
                        while (clientStream.IsConnected && isRunning) ; // Did GUI client connect?
                        {

                            if (guiStartSignal.WaitOne())
                            {
                                Console.WriteLine("Received GUI Signal .. Starting Horizon...");
                                argsFromGUI = true;
                                mainHorizon();
                            }
                        }
                        Console.WriteLine("GUI Disconnected ... Exiting Horizon");
                        return 130;
                    }
                    catch (Exception ex) // The
                    {
                        Console.WriteLine("GUI not connected when it initially appeared it was ... Exiting Horizon");
                        return 1;
                        //Console.WriteLine(ex.ToString());

                    }
                }
            }

            else // This is the case where the GUI is not connected; Horizon Console runs once like normal
            {
                int intOut = mainHorizon();
                return intOut; //This is 0
            }
            #endregion
        }

        public string GetTopHorizonDirectory()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string topHorizonDirectory = null;

            while (topHorizonDirectory is null)
            {
                currentDirectory = Directory.GetParent(currentDirectory).FullName;
                var slnFiles = Directory.GetFiles(currentDirectory, "*.sln");
                if (slnFiles.Length > 0 ) { topHorizonDirectory = currentDirectory;  return topHorizonDirectory; }
            }

            throw new Exception("Could not find a valid Top-level directory for Horizon repository (directory with Horizon.sln)"); 
        }
        public void SetDefaultDirectories()
        {
            // Check if the specified default directory exists (this should be used as the top-level horizon directory)
            if (this.TopHorizonDir is null) { this.TopHorizonDir = GetTopHorizonDirectory(); }
            else if (this.TopHorizonDir != GetTopHorizonDirectory()) { throw new Exception("Path used to set up the default 'Inputs' and 'Outputs' (in 'HorizonLog') directories is not the top-level Horizon directory."); }
          
            // Set the Top-level Horizon diretory as the deafault and create folders if they don't already exist.
            string defaultDirectory = this.TopHorizonDir + @"\HorizonLog";
            if (!Directory.Exists(defaultDirectory))
            {
                Directory.CreateDirectory(defaultDirectory);
                Directory.CreateDirectory(defaultDirectory + @"\Inputs");
                Directory.CreateDirectory(defaultDirectory + @"\Outputs");
            }
            else
            {
                if (!Directory.Exists(defaultDirectory + @"\Inputs")) { Directory.CreateDirectory(defaultDirectory + @"\Inputs"); }
                if (!Directory.Exists(defaultDirectory + @"\Outputs")) { Directory.CreateDirectory(defaultDirectory + @"\Outputs"); }
            }
            return;
        }

        public void InitInput(string[] args)
        {
            // Set Defaults:
            SimulationInputFilePath = @"..\..\..\SimulationInput.xml";
            TargetDeckFilePath = @"..\..\..\v2.2-300targets.xml";
            //ModelInputFilePath = @"..\..\..\DSAC_Static_Mod_Scripted.xml"; // Asset 1 Scripted, Asset 2 C#
            //ModelInputFilePath = @"..\..\..\DSAC_Static_Mod_PartialScripted.xml"; // Asset 1 mix Scripted/C#, Asset 2 C#
            ModelInputFilePath = @"..\..\..\DSAC_Static_Mod.xml"; // Asset 1 C#, Asset 2 C#
            bool simulationSet = false, targetSet = false, modelSet = false;
            //string outputDir = @"C:\Users\jbeals\source\repos\Horizon\HorizonLog\";

            // Set up output path:
            // This is the logic that sets up default Horizon Output directory... It is used in InitOutput() but implemented here because of the "-o" CLI argument functionality. 
            this.TopHorizonDir = GetTopHorizonDirectory(); 
            SetDefaultDirectories(); 
            string defaultOutputDir = this.TopHorizonDir + @"\HorizonLog\Outputs";
            string defaultInputDir  = this.TopHorizonDir + @"\HorizonLog\Inputs";
            bool usingCustomOutputDir = false;


            /* Handle ".hsf" saved input files. These input files are files containing past input args that are space
                and line delimited, with a trigger followed by a space and an arg on each line
                ex: "-s SimFilePath.XML" would be one line of the file, with the extension ".hsf" */
            Func<string, string[]> argsFromDotHsf = (filePath) =>
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        // Read all lines from the file
                        string[] lines = File.ReadAllLines(filePath);

                        // Initialize an empty string array to hold the result
                        string[] resultArray = new string[0];

                        foreach (string line in lines)
                        {
                            // Split each line by spaces and add the parts to the result array
                            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            resultArray = resultArray.Concat(parts).ToArray();
                        }
                        return resultArray;
                        //return resultArray;
                    }
                    else
                    {
                        // Handle the case where the file does not exist
                        return new string[0];
                    }
                }
                catch (IOException ex)
                {
                    // Handle any exceptions that may occur during file operations
                    Console.WriteLine("An error occurred while reading the file: " + ex.Message);
                    return new string[0];
                }
            }; //Action<string> handleDotHsf; handleDotHsf = (filePath) =>

            // Regular handling of input args
            Action<string[]> handleArgs = null; //Declare here as an Action so we can use it recursively. (see "-load" arg below) 
            //Action<string[]> handleArgs;
            handleArgs = (Args) =>
            {
                List<string> argList = new List<string>();
                bool saveDotHsf = false; int q = 0;
                // Get the input filenames
                int i = 0;
                //foreach (var input in Args)
                while (i < Args.Length)
                {
                    string input = Args[i];
                    i++;
                    switch (input)
                    {
                        case "-s":
                            SimulationInputFilePath = Args[i];
                            simulationSet = true;
                            Console.WriteLine("Using custom simulation file: " + SimulationInputFilePath);
                            log.Info("Using custom simulation file: " + SimulationInputFilePath);
                            argList.Add("-s"); argList.Add(Args[i]);
                            break;
                        case "-t":
                            TargetDeckFilePath = Args[i];
                            targetSet = true;
                            Console.WriteLine("Using custom target deck file: " + TargetDeckFilePath);
                            log.Info("Using custom simulation file: " + TargetDeckFilePath);
                            argList.Add("-t"); argList.Add(Args[i]);
                            break;
                        case "-m":
                            ModelInputFilePath = Args[i];
                            modelSet = true;
                            Console.WriteLine("Using custom model file: " + ModelInputFilePath);
                            log.Info("Using custom model file: " + ModelInputFilePath);
                            argList.Add("-m"); argList.Add(Args[i]);
                            break;
                        case "-o":
                            string customOutputDir = Args[i];
                            usingCustomOutputDir = true;
                            this.OutputPath = customOutputDir; // Temporarily use just the directory as the path and update in InitOutput()
                            Console.WriteLine("Using custom output directory: " + customOutputDir);
                            log.Info("Using customer output file directory: " + customOutputDir);
                            argList.Add("-o"); argList.Add(Args[i]);
                            break;

                        // Added in by Jason to save simulation run inputs/outputs. 
                        case "-save": //Functionality for saving input paramters in the ".hsf" form
                            // Use: "-save [FILENAME] [FILEDIR]"
                            // If no file name, gets default name; if no dir, gets default dir
                            saveDotHsf = true; q = i;
                            break;
                        case "-load": // This loads .hsf files by reinvoking this same switch loop. See nested Func<> above.
                            Console.WriteLine("Using saved HSF Input file: " + Args[i]);
                            string[] newArgs = argsFromDotHsf(Args[i]); // Generate a newArg list from the .hsf file.
                            handleArgs(newArgs); //Reinvoke same Action<>
                            return;
                    }
                }
                if (saveDotHsf) { SaveDotHSF(argList.ToArray(), Args, q, defaultInputDir); } // Save the input arguments as .hsf to be used in the future.
                return;
            };
            ///add usage statement

            // Handle all arguments by recursively calling "handleArgs" Action.
            if (args.Length > 0) { handleArgs(args); } //Invoke the main, normal, InitInput() handling switch case

            // Handle Logging and User knowledge:
            if (!usingCustomOutputDir) { this.OutputPath = defaultOutputDir; Console.WriteLine("Using default out directory: " + this.OutputPath); }
            if (!simulationSet) { Console.WriteLine("Using Default Simulation File"); log.Info("Using Default Simulation File"); }
            if (!targetSet) { log.Info("Using Default Simulation File"); }
            if (!modelSet) { log.Info("Using Default Simulation File"); }

        } //End InitInput()

        public void InitOutput()
        {

            // Update how the putput files are named to include time
            Func<string, string> changeHyphens = (outputFilename) =>
            {
                int i = 0;
                while (outputFilename[i] != '_') { i++; }

                string date = outputFilename.Substring(0, i + 1);
                string time = outputFilename.Substring(i + 1);

                int hyphenCount = 0;
                StringBuilder modifiedTime = new StringBuilder();

                foreach (char c in time)
                {
                    if (c == '-')
                    {
                        hyphenCount++;
                        if (hyphenCount == 1)
                            modifiedTime.Append('h');
                        else if (hyphenCount == 2)
                            modifiedTime.Append('m');
                        else if (hyphenCount == 3)
                            modifiedTime.Append("s-");
                    }
                    else
                    {
                        modifiedTime.Append(c);
                    }
                }

                return date + modifiedTime.ToString();
            };


            // Initialize Output File
            var outputFileName = string.Format("output-{0:yyyy-MM-dd_HH-mm-ss}-*", DateTime.Now);
            outputFileName = changeHyphens(outputFileName); // update to have a more readable time output file name
            string outputPath = this.OutputPath; //
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
            outputPath = outputPath + "\\" + outputFileName + txt;
            this.OutputPath = outputPath; //Update the filepath from directory to full output filepath.
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
            foreach (XmlNode assetNode in assets)
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
                SchedEvaluator = EvaluatorFactory.GetEvaluator(evalNodes[0], SubList);
                Console.WriteLine("Evaluator Loaded");
                log.Info("Evaluator Loaded");
            }
        }
        public void CreateSchedules()
        {
            SimSystem = new SystemClass(AssetList, SubList, ConstraintsList, SystemUniverse);

            if (SimSystem.CheckForCircularDependencies())
                throw new NotFiniteNumberException("System has circular dependencies! Please correct then try again.");

            Scheduler _scheduler = new Scheduler(SchedEvaluator, new SystemSchedule(InitialSysState));
            Schedules = _scheduler.GenerateSchedules(SimSystem, SystemTasks);
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
                    // THERE IS A SCHEDULE IN THE LIST THAT IS OF THE FIRST EVENT ONLY,
                    // WHEN THAT SCHEDULE PASSES THROUGH THIS LOOP, THE EVENT END TIME IS
                    // EXTENDED TO THE END OF THE SIMULATION.
                    // SINCE ALL EVENTS IN A SCHEDULE ARE REFERENCES, THE EVENT IN THE [0] ALSO CHANGES.
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

        #region Program Methods for GUI
        public void SaveDotHSF(string[] argList, string[] args, int i, string defaultSaveDirectory)
        // This function will handle an input file with the extension ".hsf" 
        {
            //This takes the argList that is output from InitInput() and turns it into an array able to be easily written to a .hsf readable file.
            Func<string[], string[]> createDotHSF = null;
            createDotHSF = (ArgList) =>
            {
                List<string> stringList = new List<string>(); //This is to be iterable to convert to an array upon return
                string nextLine = null;
                int jj = 0; int numOpts = 0; 
                while (jj < ArgList.Length)
                {
                    if (ArgList[jj].StartsWith("-"))
                    {
                        numOpts = 1;
                        nextLine = ArgList[jj];
                        while (!ArgList[numOpts].StartsWith("-"))
                        {
                            nextLine = nextLine + " " + ArgList[numOpts];
                            numOpts++;
                        }
                        stringList.Add(nextLine);
                    }
                    jj = jj + numOpts;
                }
                return stringList.ToArray();

                //List<string> stringList = new List<string>(); //This is to be iterable to convert to an array upon return
                //int j = 0;
                //while (j + 1 < ArgList.Length) // While there are still commands; goes in twos
                //{
                //    //stringList.Add(string.Join(string.Join(ArgList[j], " "), ArgList[j + 1])); // Join the command call out and optoin with a space in betwee
                //    stringList.Add(ArgList[j] + " " + ArgList[j + 1]);
                //    j+=2;
                //}
                //return stringList.ToArray(); // Returns a string array with the command callout and command option after in each element
            };
            Func<string, string> changeHyphens = (outputFilename) =>
            {
                int g = 0;
                while (outputFilename[g] != '_') { g++; }

                string date = outputFilename.Substring(0, g + 1);
                string time = outputFilename.Substring(g + 1);

                int hyphenCount = 0;
                StringBuilder modifiedTime = new StringBuilder();

                foreach (char c in time)
                {
                    if (c == '-')
                    {
                        hyphenCount++;
                        if (hyphenCount == 1)
                            modifiedTime.Append('h');
                        else if (hyphenCount == 2)
                            modifiedTime.Append('m');
                        else if (hyphenCount == 3)
                            modifiedTime.Append("s-");
                    }
                    else
                    {
                        modifiedTime.Append(c);
                    }
                }

                return date + modifiedTime.ToString();
            };
            // Parse the command first ..............................................
            // USAGE: -save [FILENAME] [FILEPATH] ........ Otherwise defualt functionality. 

            // First find out how many args in the "-save" command:
            int numArgs = 0; int opts = 0;
            int ii = i + 1;
            while (ii < argList.Length && !argList[ii].StartsWith("-")) { ii++; opts++; }

            string dotHsfFilePath = null; string dotHsfName = null; // "HSF_Input_Arguments_0";
            string[] stringToWriteOut = createDotHSF(argList);

            
            if (opts == 2) // Both a filename and output directory provided
            {
                dotHsfName = args[i + 1];
                dotHsfFilePath = args[i + 2] + "\\" + dotHsfName + ".hsf";
                File.WriteAllLines(dotHsfFilePath, stringToWriteOut);
                return; 
            }

            // Set the defaults
            dotHsfName = string.Format("LastRunInput-{0:yyyy-MM-dd_HH-mm-ss}-*", DateTime.Now);
            dotHsfName = changeHyphens(dotHsfName);
            string ext = ".txt";
            string saveDir = null;

            // "-save [option1]/[option2]" handling
            if (opts ==0) { saveDir = defaultSaveDirectory; }
            else if (Directory.Exists(args[i + 1])) { saveDir = args[i + 1]; }
            else
            {
                saveDir = defaultSaveDirectory;
                dotHsfName = args[i + 1];
                dotHsfFilePath = saveDir + "\\" + dotHsfName;
                File.WriteAllLines(dotHsfFilePath, stringToWriteOut); return;
            }

            // For what remains, the default name is applied, so do a file serach and update the version: 
            string[] fileNames = System.IO.Directory.GetFiles(defaultSaveDirectory, dotHsfName, System.IO.SearchOption.TopDirectoryOnly);
            double number = 0;
            foreach (var fileName in fileNames)
            {
                char version = fileName[fileName.Length - ext.Length - 1];
                if (number < Char.GetNumericValue(version))
                    number = Char.GetNumericValue(version);
            }
            number++;
            dotHsfName = dotHsfName.Remove(dotHsfName.Length - 1) + number;
            dotHsfFilePath = saveDir + "\\" + dotHsfName + ext;

            // Write it all to file.
            File.WriteAllLines(dotHsfFilePath, stringToWriteOut);
            return;
        }
        static bool checkStatusGUI()
        {
            // Inner Function to check if the GUI is running:
            Func<bool> checkIfGuiIsRunning = () =>
            {
                try
                {
                    using (NamedPipeClientStream client = new NamedPipeClientStream("pipe_JacksonPollockGUI"))
                    {
                        client.Connect(1000); // Did GUI client connect?
                        return true;
                    }
                }
                catch (Exception) { return false; }
            }; // End lambda function

            // Check if the GUI is Running 
            bool guiStatus = checkIfGuiIsRunning();
            if (guiStatus)
            {
                Console.WriteLine("GUI Running...Waiting for signal from it before executing Horizon...\n");
                return true;
            }
            else
            {
                Console.WriteLine("GUI is not Running ... Horizon Console App executing (once) per usual ...");
                return false;
            }
        }
        #endregion

    }
}



// Old code that was originally cloned Oct 18 2023

//// Copyright (c) 2016 California Polytechnic State University
//// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Xml;
//using System.Text;
//using HSFScheduler;
//using MissionElements;
//using UserModel;
//using HSFUniverse;
//using HSFSubsystem;
//using HSFSystem;
//using log4net;
//using Utilities;
//using System.Linq;

//namespace Horizon
//{
//    public class Program
//    {
//        public ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

//        public string SimulationInputFilePath { get; set; }
//        public string TargetDeckFilePath { get; set; }
//        public string ModelInputFilePath { get; set; }

//        // Load the environment. First check if there is an ENVIRONMENT XMLNode in the input file
//        public Domain SystemUniverse { get; set; }

//        //Create singleton dependency dictionary
//        public Dependency Dependencies { get; } = Dependency.Instance;

//        // Initialize Lists to hold assets, subsystems and evaluators
//        public List<Asset> AssetList { get; set; } = new List<Asset>();
//        public List<Subsystem> SubList { get; set; } = new List<Subsystem>();

//        // Maps used to set up preceeding nodes
//        //public Dictionary<ISubsystem, XmlNode> SubsystemXMLNodeMap { get; set; } = new Dictionary<ISubsystem, XmlNode>(); //Depreciated (?)

//        public List<KeyValuePair<string, string>> DependencyList { get; set; } = new List<KeyValuePair<string, string>>();
//        public List<KeyValuePair<string, string>> DependencyFcnList { get; set; } = new List<KeyValuePair<string, string>>();

//        // Create Constraint list 
//        public List<Constraint> ConstraintsList { get; set; } = new List<Constraint>();

//        //Create Lists to hold all the dependency nodes to be parsed later
//        //List<XmlNode> _depNodes = new List<XmlNode>();
//        public SystemState InitialSysState { get; set; } = new SystemState();

//        //XmlNode _evaluatorNode; //Depreciated (?)
//        public Evaluator SchedEvaluator;
//        public List<SystemSchedule> Schedules { get; set; }
//        public SystemClass SimSystem { get; set; }
//        public string OutputPath { get; set; }
//        public List<Task> SystemTasks { get; set; } = new List<Task>();

//        public static int Main(string[] args) //
//        {
//            Program program = new Program();
//            // Begin the Logger
//            program.log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
//            program.log.Info("STARTING HSF RUN"); //Do not delete
//            program.InitInput(args);
//            program.InitOutput();
//            program.LoadScenario();
//            program.LoadTargets();
//            program.LoadSubsystems();
//            program.LoadEvaluator();
//            program.CreateSchedules();
//            // Schedules are already evaluated and sorted when they are generated in CreateSchedules().  This method
//            // also runs the CanExtend() method which I think we want to eliminate...
//            //double maxSched = program.EvaluateSchedules();

//            int i = 0;
//            //Morgan's Way
//            StreamWriter sw = File.CreateText(program.OutputPath);
//            foreach (SystemSchedule sched in program.Schedules)
//            {
//                sw.WriteLine("Schedule Number: " + i + "Schedule Value: " + program.Schedules[i].ScheduleValue);
//                foreach (var eit in sched.AllStates.Events)
//                {
//                    if (i < 5)//just compare the first 5 schedules for now
//                    {
//                        sw.WriteLine(eit.ToString());
//                    }
//                }
//                i++;
//            }
//            var maxValue = program.Schedules.Max(s => s.ScheduleValue);
//            program.log.Info("Max Schedule Value: " + maxValue);

//            // Mehiel's way
//            string stateDataFilePath = @"C:\HorizonLog\Scratch";// + string.Format("output-{0:yyyy-MM-dd-hh-mm-ss}", DateTime.Now);
//            SystemSchedule.WriteSchedule(program.Schedules[0], stateDataFilePath);

//            //  Move this to a method that always writes out data about the dynamic state of assets, the target dynamic state data, other data?
//            //var csv = new StringBuilder();
//            //csv.Clear();
//            //foreach (var asset in program.simSystem.Assets)
//            //{
//            //    File.WriteAllText(@"..\..\..\" + asset.Name + "_dynamicStateData.csv", asset.AssetDynamicState.ToString());
//            //}

//            //Console.ReadKey();
//            return 0;
//        }
//        public void InitInput(string[] args)
//        {
//            // Set Defaults
//            SimulationInputFilePath = @"..\..\..\SimulationInput.xml";
//            TargetDeckFilePath = @"..\..\..\v2.2-300targets.xml";
//            //ModelInputFilePath = @"..\..\..\DSAC_Static_Mod_Scripted.xml"; // Asset 1 Scripted, Asset 2 C#
//            //ModelInputFilePath = @"..\..\..\DSAC_Static_Mod_PartialScripted.xml"; // Asset 1 mix Scripted/C#, Asset 2 C#

//            ModelInputFilePath = @"..\..\..\DSAC_Static_Mod.xml"; // Asset 1 C#, Asset 2 C#
//            bool simulationSet = false, targetSet = false, modelSet = false;

//            // Get the input filenames
//            int i = 0;
//            foreach (var input in args)
//            {
//                i++;
//                switch (input)
//                {
//                    case "-s":
//                        SimulationInputFilePath = args[i];
//                        simulationSet = true;
//                        Console.WriteLine("Using custom simulation file: " + SimulationInputFilePath);
//                        log.Info("Using custom simulation file: " + SimulationInputFilePath);
//                        break;
//                    case "-t":
//                        TargetDeckFilePath = args[i];
//                        targetSet = true;
//                        Console.WriteLine("Using custom target deck file: " + TargetDeckFilePath);
//                        log.Info("Using custom simulation file: " + TargetDeckFilePath);
//                        break;
//                    case "-m":
//                        ModelInputFilePath = args[i];
//                        modelSet = true;
//                        Console.WriteLine("Using custom model file: " + ModelInputFilePath);
//                        log.Info("Using custom model file: " + ModelInputFilePath);
//                        break;
//                }
//            }
//            ///add usage statement

//            if (!simulationSet)
//            {
//                Console.WriteLine("Using Default Simulation File");
//                log.Info("Using Default Simulation File");
//            }

//            if (!targetSet)
//            {
//                log.Info("Using Default Simulation File");
//            }

//            if (!modelSet)
//            {
//                log.Info("Using Default Simulation File");
//            }

//        }
//        public void InitOutput()
//        {
//            // Initialize Output File
//            var outputFileName = string.Format("output-{0:yyyy-MM-dd}-*", DateTime.Now);
//            string outputPath = @"C:\HorizonLog\";
//            var txt = ".txt";
//            string[] fileNames = System.IO.Directory.GetFiles(outputPath, outputFileName, System.IO.SearchOption.TopDirectoryOnly);
//            double number = 0;
//            foreach (var fileName in fileNames)
//            {
//                char version = fileName[fileName.Length - txt.Length - 1];
//                if (number < Char.GetNumericValue(version))
//                    number = Char.GetNumericValue(version);
//            }
//            number++;
//            outputFileName = outputFileName.Remove(outputFileName.Length - 1) + number;
//            outputPath += outputFileName + txt;
//            this.OutputPath = outputPath;
//        }

//        public void LoadScenario()
//        {
//            // Find the main input node from the XML input files
//            XmlParser.ParseSimulationInput(SimulationInputFilePath);
//        }
//        public void LoadTargets()
//        {
//            // Load the target deck into the targets list from the XML target deck input file
//            bool targetsLoaded = Task.loadTargetsIntoTaskList(XmlParser.GetTargetNode(TargetDeckFilePath), SystemTasks);
//            if (!targetsLoaded)
//            {
//                throw new Exception("Targets were not loaded.");
//            }

//        }
//        public void LoadSubsystems()
//        {

//            // Find the main model node from the XML model input file
//            var modelInputXMLNode = XmlParser.GetModelNode(ModelInputFilePath);

//            var environments = modelInputXMLNode.SelectNodes("ENVIRONMENT");

//            // Check if environment count is empty, default is space
//            if (environments.Count == 0)
//            {
//                SystemUniverse = new SpaceEnvironment();
//                Console.WriteLine("Default Space Environment Loaded");
//                log.Info("Default Space Environment Loaded");
//            }

//            // Load Environments
//            foreach (XmlNode environmentNode in environments)
//            {
//                SystemUniverse = UniverseFactory.GetUniverseClass(environmentNode);
//            }

//            var snakes = modelInputXMLNode.SelectNodes("PYTHON");
//            foreach (XmlNode pythonNode in snakes)
//            {
//                throw new NotImplementedException();
//            }

//            // Load Assets
//            var assets = modelInputXMLNode.SelectNodes("ASSET");
//            foreach(XmlNode assetNode in assets)
//            {
//                Asset asset = new Asset(assetNode);
//                asset.AssetDynamicState.Eoms.SetEnvironment(SystemUniverse);
//                AssetList.Add(asset);

//                // Load Subsystems
//                var subsystems = assetNode.SelectNodes("SUBSYSTEM");

//                foreach (XmlNode subsystemNode in subsystems)
//                {
//                    Subsystem subsys = SubsystemFactory.GetSubsystem(subsystemNode, asset);
//                    SubList.Add(subsys);

//                    // Load States (Formerly ICs)
//                    var States = subsystemNode.SelectNodes("STATE");

//                    foreach (XmlNode StateNode in States)
//                    {
//                        // Parse state node for key name and state type, add the key to the subsys's list of keys, return the key name
//                        string keyName = SubsystemFactory.SetStateKeys(StateNode, subsys);
//                        // Use key name and state type to set initial conditions 
//                        InitialSysState.SetInitialSystemState(StateNode, keyName);
//                    }
//                }

//                // Load Constraints
//                var constraints = assetNode.SelectNodes("CONSTRAINT");

//                foreach (XmlNode constraintNode in constraints)
//                {
//                    ConstraintsList.Add(ConstraintFactory.GetConstraint(constraintNode, SubList, asset));
//                }
//            }
//            Console.WriteLine("Environment, Assets, and Constraints Loaded");
//            log.Info("Environment, Assets, and Constraints Loaded");

//            // Load Dependencies
//            var dependencies = modelInputXMLNode.SelectNodes("DEPENDENCY");

//            foreach (XmlNode dependencyNode in dependencies)
//            {
//                var SubFact = new SubsystemFactory();
//                SubFact.SetDependencies(dependencyNode, SubList);
//            }
//            Console.WriteLine("Dependencies Loaded");
//            log.Info("Dependencies Loaded");
//        }

//        public void LoadEvaluator()
//        {
//            var modelInputXMLNode = XmlParser.GetModelNode(ModelInputFilePath);
//            var evalNodes = modelInputXMLNode.SelectNodes("EVALUATOR");
//            if (evalNodes.Count > 1)
//            {
//                throw new NotImplementedException("Too many evaluators in XML input!");
//                Console.WriteLine("Too many evaluators in XML input");
//                log.Info("Too many evaluators in XML input");
//            }
//            else
//            {
//                SchedEvaluator = EvaluatorFactory.GetEvaluator(evalNodes[0],SubList);
//                Console.WriteLine("Evaluator Loaded");
//                log.Info("Evaluator Loaded");
//            }
//        }
//        public void CreateSchedules()
//        {
//            SimSystem = new SystemClass(AssetList, SubList, ConstraintsList, SystemUniverse);

//            if (SimSystem.CheckForCircularDependencies())
//                throw new NotFiniteNumberException("System has circular dependencies! Please correct then try again.");

//            Scheduler _scheduler = new Scheduler(SchedEvaluator, new SystemSchedule(InitialSysState));
//            Schedules = _scheduler.GenerateSchedules(SimSystem, SystemTasks);
//        }
//        public double EvaluateSchedules()
//        {
//            // Evaluate the schedules and set their values
//            foreach (SystemSchedule systemSchedule in Schedules)
//            {
//                systemSchedule.ScheduleValue = SchedEvaluator.Evaluate(systemSchedule);
//                bool canExtendUntilEnd = true;
//                // Extend the subsystem states to the end of the simulation 
//                foreach (var subsystem in SimSystem.Subsystems)
//                {
//                    // THERE IS A SCHEDULE IN THE LIST THAT IS OF THE FIRST EVENT ONLY,
//                    // WHEN THAT SCHEDULE PASSES THROUGH THIS LOOP, THE EVENT END TIME IS
//                    // EXTENDED TO THE END OF THE SIMULATION.
//                    // SINCE ALL EVENTS IN A SCHEDULE ARE REFERENCES, THE EVENT IN THE [0] ALSO CHANGES.
//                    if (systemSchedule.AllStates.Events.Count > 0)
//                            if (!subsystem.CanExtend(systemSchedule.AllStates.Events.Peek(), (Domain)SimSystem.Environment, SimParameters.SimEndSeconds))
//                            log.Error("Cannot Extend " + subsystem.Name + " to end of simulation");
//                }
//            }

//            // Sort the sysScheds by their values
//            Schedules.Sort((x, y) => x.ScheduleValue.CompareTo(y.ScheduleValue));
//            Schedules.Reverse();
//            double maxSched = Schedules[0].ScheduleValue;
//            return maxSched;
//        }
//    }
//}

// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)
