using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Utilities;
using HSFUniverse;
using MissionElements;
using HSFScheduler;
using UserModel;
using HSFSystem;
using HSFSubsystem;

namespace HSFSchedulerUnitTest
{
    [TestClass]
    public class AccessUnitTest
    {
        [TestMethod]
        public void PregenAccessUnitTest()
        {

            List<int> newList = new List<int>();

            Console.Write("dfkalj", )
            /*
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
                newAsset.AssetDynamicState.Eoms = new OrbitalEOMS();
                assets.Add(newAsset);
            }

            SystemClass system = new SystemClass(assets, subsystems, constraints, environment);// assets);

            XmlDocument targetDeckDoc = new XmlDocument();
            targetDeckDoc.Load("..\\..\\v2.2-300targets.xml");
            Stack<Task> tasks = new Stack<Task>();

            XmlNodeList targetDeckXmlNodes = targetDeckDoc.FirstChild.ChildNodes;
            foreach (XmlNode xmlNode in targetDeckXmlNodes)
            {
                Target newTarget = new Target(xmlNode);
                Task newTask = new Task(TaskType.EMPTY, newTarget, 1);
                tasks.Push(newTask);
            }



            // DOING THIS UNTIL SYSTEMCLASS CAN BE COMPLIED - EAM
            double startTime = SimParameters.SimStartSeconds;
            double endTime = SimParameters.SimEndSeconds;
            double stepTime = SchedParameters.SimStepSeconds;
            Stack<Access> accessesByAsset = Access.pregenerateAccessesByAsset(system, tasks, startTime, endTime, stepTime);

           

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
            */
        }



    }
}

