using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Utilities;
using HSFUniverse;
using HSFScheduler;

namespace HSFSchedulerUnitTest
{
    [TestClass]
    public class AccessUnitTest
    {
        [TestMethod]
        public void PregenAccessUnitTest()
        {
            XmlDocument scenarioDoc = new XmlDocument();
            scenarioDoc.Load("..\\..\\SimulationInput.xml");

            XmlNode scenarioNode = scenarioDoc.FirstChild;
            string scenarioName = scenarioNode.Attributes["scenarioName"].Value;
            XmlNode simulationPramsXml = scenarioNode["SIMULATION_PARAMETERS"];
            XmlNode schedulerParamsXml = scenarioNode["SCHEDULER_PARAMETERS"];
            

            SimParameters.LoadSimParameters(simulationPramsXml, scenarioName);
            SchedParameters.LoadSchedParameters(schedulerParamsXml);

            XmlDocument systemDoc = new XmlDocument();

            systemDoc.Load("..\\..\\DSAC_Static.xml");

            XmlNodeList assetXmlNodes = systemDoc.FirstChild.ChildNodes;
            List<Asset> assets = new List<Asset>();

            foreach (XmlNode xmlNode in assetXmlNodes)
            {
                Asset newAsset = new Asset(xmlNode);
                newAsset.AssetDynamicState.Eoms = new Orbital_EOMS();
                assets.Add(newAsset);
            }

            SystemClass system = new SystemClass(assets);

            XmlDocument targetDeckDoc = new XmlDocument();
            targetDeckDoc.Load("..\\..\\v2.2-300targets.xml");
            Stack<Task> tasks = new Stack<Task>();

            XmlNodeList targetDeckXmlNodes = targetDeckDoc.FirstChild.ChildNodes;
            foreach (XmlNode xmlNode in targetDeckXmlNodes)
            {
                Target newTarget = new Target(xmlNode);
                Task newTask = new Task(taskType.EMPTY, newTarget, 1);
                tasks.Push(newTask);
            }



            // DOING THIS UNTIL SYSTEMCLASS CAN BE COMPLIED - EAM
            //Stack<Access> accesses = Access.pregenerateAccessesByAsset(system, tasks, startTime, endTime, stepTime);

            Stack<Access> accessesByAsset = new Stack<Access>();
            // For all assets...
            foreach (Asset asset in system.Assets)
            {
                // ...for all tasks...
                foreach (Task task in tasks)
                {
                    // ...for all time....    
                    for (double accessTime = SimParameters.SimStartSeconds; accessTime <= SimParameters.SimEndSeconds; accessTime += SchedParameters.SimStepSeconds)
                    {
                        // create a new access, or extend the access endTime if this is an update to an existing access
                        bool hasAccess = Utilities.GeometryUtilities.hasLOS(asset.AssetDynamicState.PositionECI(accessTime), task.Target.DynamicState.PositionECI(accessTime));
                        if (hasAccess)
                        {
                            bool isNewAccess;
                            if (accessesByAsset.Count == 0 || accessTime == SimParameters.SimStartSeconds || accessesByAsset.Peek().Task.Target.Name !=task.Target.Name)
                                isNewAccess = true;
                            else
                                isNewAccess = (accessTime - accessesByAsset.Peek().AccessEnd) > SchedParameters.SimStepSeconds;
                            if (isNewAccess)
                            {
                                Access newAccess = new Access(asset, task);
                                newAccess.AccessStart = accessTime;
                                newAccess.AccessEnd = accessTime;
                                accessesByAsset.Push(newAccess);
                            }
                            else  // extend the access
                                accessesByAsset.Peek().AccessEnd = accessTime;
                        }
                    }
                }
            }

            double currentTime = 0;
            //public static Stack<Access> getCurrentAccessesForAsset(Stack<Access> accesses, Asset asset, double currentTime)
            Stack<Access> allCurrentAccesses = new Stack<Access>(accessesByAsset.Where(item => (item.AccessStart <= currentTime && item.AccessEnd >= currentTime)));

          

            //Stack<Stack<Access>> generateExhaustiveSystemSchedules(Stack<Access> currentAccess, SystemClass system, double currentTime)
            Stack<Stack<Access>> currentAccessesByAsset = new Stack<Stack<Access>>();
            foreach (Asset asset in system.Assets)
                //public static Stack<Access> getCurrentAccessesForAsset(Stack<Access> accesses, Asset asset, double currentTime)
                currentAccessesByAsset.Push(new Stack<Access>(allCurrentAccesses.Where(item => item.Asset == asset)));

            IEnumerable<IEnumerable<Access>> allScheduleCombos = currentAccessesByAsset.CartesianProduct();

            foreach (IEnumerable<Access> combo in allScheduleCombos)
                combo.ToString();

            Console.ReadLine();
        }

    }

    // Empty classes for unit testing only! - EAM
    class SystemClass
    {
        public List<Asset> Assets { get; set; }

        public SystemClass(List<Asset> assets)
        {
            Assets = assets;
        }
    }

    public class Asset
    {
        public DynamicState AssetDynamicState { get; private set; } //was protected, why?
        //TODO:make isTaskable mean something
        public bool IsTaskable { get; private set; }//was protected, why?
        public string Name { get; private set; }

        public Asset()
        {
            IsTaskable = false;
        }

        public Asset(XmlNode assetXMLNode)
        {
            Name = assetXMLNode.Attributes["name"].Value;
            AssetDynamicState = new DynamicState(assetXMLNode["DynamicState"]);  // XmlInput Change - position => DynamicState
            IsTaskable = false;
        }

        public override string ToString()
        {
            return Name;
        }

    }

    public class Access
    {
        public Asset Asset { get; private set; }
        public Task Task { get; private set; }
        public double AccessStart { get; set; }
        public double AccessEnd { get; set; }

        public Access(Asset asset, Task task)
        {
            Asset = asset;
            Task = task;
        }
    }
}

