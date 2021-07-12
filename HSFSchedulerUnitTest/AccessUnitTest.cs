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
using System.IO;

namespace HSFSchedulerUnitTest
{
    [TestFixture]
    public class AccessUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        [Test]
        public void AccessConstructor()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler_access.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub.xml");

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

            Access A1 = new Access(programAct.AssetList[0], systemTasks.Peek());
            Assert.AreSame(systemTasks.Peek(), A1.Task);
            Assert.AreSame(programAct.AssetList[0], A1.Asset);


        }

        [Test]
        public void getCurrentAccesses()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler_access.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub.xml");

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }

            Access A1 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Access A2 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Stack<Access> accesses = new Stack<Access>();
            accesses.Push(A2);
            accesses.Push(A1);

            Stack<Access> currentAcceses = Access.getCurrentAccesses(accesses, 0);
            Assert.AreEqual(A2, currentAcceses.Pop());
            Assert.AreEqual(A1, currentAcceses.Pop());
        }
        [Test]
        public void getCurrentAccessesForAsset()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub_accessmultiasset.xml");

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }

            Access A1 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Access A2 = new Access(programAct.AssetList[1], systemTasks.Pop());
            Stack<Access> accesses = new Stack<Access>();
            Stack<Access> empty = new Stack<Access>();
            accesses.Push(A2);
            accesses.Push(A1);

            Stack<Access> currentAcceses_Asset1 = Access.getCurrentAccessesForAsset(accesses, programAct.AssetList[0], 0);
            Stack<Access> currentAcceses_Asset2 = Access.getCurrentAccessesForAsset(accesses, programAct.AssetList[1], 0);

            Assert.AreEqual(A1, currentAcceses_Asset1.Pop());
            Assert.AreEqual(A2, currentAcceses_Asset2.Pop());
            Assert.AreEqual(empty, currentAcceses_Asset1);
            Assert.AreEqual(empty, currentAcceses_Asset2);
        }

        [Test]
        public void PregenAccessbyAsset()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler_access.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub.xml");

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
            Assert.AreEqual(ExpTask3.ToString(), Task3.ToString());
            Assert.AreEqual(ExpTask2.ToString(), Task2.ToString());
            Assert.AreEqual(ExpTask1.ToString(), Task1.ToString());
            Assert.AreEqual(ExpTask0.ToString(), Task0.ToString());

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