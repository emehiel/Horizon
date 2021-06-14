using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Xml;
using Utilities;
using HSFUniverse;
using MissionElements;
using HSFScheduler;
using UserModel;
using HSFSystem;
using HSFSubsystem;
using Horizon;

namespace HSFSchedulerUnitTest
{
    [TestFixture]
    public class AccessUnitTest
    {
        [Test]
        public void PregenAccessUnitTest()
        {
            //KInda Tested this already?
            Program programAct = new Program();
            programAct.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml";
            programAct.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets_Scheduler_access.xml";
            programAct.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_TestSub.xml";

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }
            try
            {
                programAct.LoadDependencies();
            }
            catch
            {
                programAct.log.Info("LoadDepenedencies Failed the Unit test");
            }

            SystemClass simSystem = new SystemClass(programAct.AssetList, programAct.SubList, programAct.ConstraintsList, programAct.SystemUniverse);

            Stack<Access> pregenAccess = Access.pregenerateAccessesByAsset(simSystem, systemTasks, 0, 1, 1);
            


        }

        [Test]
        public void getCurrentAccessesUnitTest()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml";
            programAct.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets_Scheduler_access.xml";
            programAct.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_TestSub.xml";

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }
            try
            {
                programAct.LoadDependencies();
            }
            catch
            {
                programAct.log.Info("LoadDepenedencies Failed the Unit test");
            }

            SystemClass simSystem = new SystemClass(programAct.AssetList, programAct.SubList, programAct.ConstraintsList, programAct.SystemUniverse);

            Stack<Access> pregenAccess = Access.pregenerateAccessesByAsset(simSystem, systemTasks, 0, 1, 1);

            Stack<Access> AccessAct = Access.getCurrentAccesses(pregenAccess, 0);


            systemTasks.Pop(); //No access to last task, task4 so ot expected access
            Access Task3 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Task3.AccessEnd = 2;
            Access Task2 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Access Task1 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Access Task0 = new Access(programAct.AssetList[0], systemTasks.Pop());

           
            Access ExpTask3 = AccessAct.Pop();
            Access ExpTask2 = AccessAct.Pop();
            Access ExpTask1 = AccessAct.Pop();
            Access ExpTask0 = AccessAct.Pop();



            //Asserts failed when comparing objects so ToString compares the imporant data
            Assert.AreEqual( ExpTask3.ToString(), Task3.ToString());
            Assert.AreEqual(ExpTask2.ToString(), Task2.ToString());
            Assert.AreEqual(ExpTask1.ToString(), Task1.ToString());
            Assert.AreEqual(ExpTask0.ToString(), Task0.ToString());

        }
        [Test]
        public void getCurrentAccessesForAssetUnitTest()
        {
            Assert.Inconclusive("Not Implemented");
        }
    }
}

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