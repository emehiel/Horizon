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
    public class SystemScheduleUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        /// <summary>
        /// Tests first 3 constructors of SystemSchedule
        /// </summary>
        [Test]
        public void Constructors()
        {
            Program programAct = new Program();
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_EnviroRTA.xml");
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.ModelInputFilePath = modelInputFilePath;
            programAct.SimulationInputFilePath = simulationInputFilePath;
            
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            Stack<Task> systemTasks = programAct.LoadTargets();

            XmlNode simNode = XmlParser.ParseSimulationInput(simulationInputFilePath);
            XmlNode modelNode = XmlParser.GetModelNode(modelInputFilePath);
            Asset asset = new Asset(modelNode.ChildNodes[1]);
            List<XmlNode> ICNodes = new List<XmlNode>();

            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[2].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[3].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[1]);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[2]);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[4].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[5].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[6].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[6].ChildNodes[1]);

            SystemState systemState = SystemState.setInitialSystemState(ICNodes, asset);
            StateHistory expHist = new StateHistory(systemState);

            //First Constructor test SystemSchedule(SystemState initialstates) 
            SystemSchedule newSched = new SystemSchedule(systemState);
            Assert.That(newSched.AllStates, Has.Property("InitialState").EqualTo(expHist.InitialState));

            //Second Constructor test SystemSchedule(StateHistory allStates)
            SystemSchedule newSched1 = new SystemSchedule(expHist); 
            Assert.That(newSched1.AllStates, Has.Property("InitialState").EqualTo(expHist.InitialState));

            //Third Constructor test SystemSchedule(SystemState initialstates)
            Dictionary<Asset, Task> eventDic = new Dictionary<Asset, Task>();
            eventDic.Add(asset, systemTasks.Pop());
            Event event1 = new Event(eventDic, systemState);
            SystemSchedule newSched2 = new SystemSchedule(newSched1,event1);

            Assert.That(newSched2.AllStates, Has.Property("InitialState").EqualTo(expHist.InitialState));
            Assert.AreEqual(event1, newSched2.AllStates.Events.Pop());
        }
        /// <summary>
        /// Tests the final and largest constructor for SystemSchedule
        /// </summary>
        [Test]
        public void BigConstructor()
        {
            Program programAct = new Program();
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_EnviroRTA.xml");
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.ModelInputFilePath = modelInputFilePath;
            programAct.SimulationInputFilePath = simulationInputFilePath;

            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            Stack<Task> systemTasks = programAct.LoadTargets();

            XmlNode simNode = XmlParser.ParseSimulationInput(simulationInputFilePath);
            XmlNode modelNode = XmlParser.GetModelNode(modelInputFilePath);
            Asset asset = new Asset(modelNode.ChildNodes[1]);
            List<XmlNode> ICNodes = new List<XmlNode>();

            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[2].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[3].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[1]);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[2]);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[4].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[5].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[6].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[6].ChildNodes[1]);

            SystemState systemState = SystemState.setInitialSystemState(ICNodes, asset);
            StateHistory initialHist = new StateHistory(systemState);
            Dictionary<Asset, Task> tasks = new Dictionary<Asset, Task>();
            tasks.Add(asset, systemTasks.Peek());

            Event event1 = new Event(tasks, systemState);
            event1.SetEventEnd(asset, 1);
            event1.SetTaskEnd(asset, 0);
            event1.SetTaskStart(asset, 0);
            event1.SetEventStart(asset, 0);

            Event event2 = new Event(null, systemState);
            event2.SetEventEnd(asset, 1);
            event2.SetTaskEnd(asset, 0);
            event2.SetTaskStart(asset, 0);
            event2.SetEventStart(asset, 0);

            StateHistory expHist1 = new StateHistory(initialHist,event1);


            //Fourth Constructor test
            Stack<Access> access = new Stack<Access>();
            Access newAccess = new Access(asset, systemTasks.Pop());

            access.Push(newAccess);

            SystemSchedule newSched3 = new SystemSchedule(expHist1, access, 0);
            Assert.That(newSched3.AllStates, Has.Property("InitialState").EqualTo(expHist1.InitialState));

            access.Pop(); //remove the task from above test to add a null one

            StateHistory expHist2 = new StateHistory(initialHist, event2);
            Access noAccess = new Access(asset, null);
            access.Push(noAccess);
            SystemSchedule newSched4 = new SystemSchedule(expHist2, access, 0);
            Assert.That(newSched4.AllStates, Has.Property("InitialState").EqualTo(expHist2.InitialState));

        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CanAddTasks()
        {
            Program programAct = new Program();
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_SysScheduler.xml");
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_EnviroRTA.xml");

            XmlNode modelNode = XmlParser.GetModelNode(programAct.ModelInputFilePath);
            Stack<Task> systemTasks = programAct.LoadTargets();
            Asset asset = new Asset(modelNode.ChildNodes[1]);

            List<XmlNode> ICNodes = new List<XmlNode>();
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[2].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[3].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[1]);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[2]);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[4].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[5].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[6].FirstChild);
            ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[6].ChildNodes[1]);

            SystemState systemState = SystemState.setInitialSystemState(ICNodes, asset);
            StateHistory initialHist = new StateHistory(systemState);

            Stack<Access> accessList = new Stack<Access>();
            accessList.Push(new Access(asset,systemTasks.Pop()));
            systemTasks.Pop();
            systemTasks.Pop();

            StateHistory expHist1 = new StateHistory(initialHist);

            SystemSchedule sched1 = new SystemSchedule(expHist1);
            Assert.IsTrue(sched1.CanAddTasks(accessList, 0));
            accessList.Pop();
            accessList.Push(new Access(asset, systemTasks.Peek()));
            Assert.IsFalse(sched1.CanAddTasks(accessList, 0)); //need to figure out how to break this test
        }
        /// <summary>
        /// Tests the ability to write schedule into file stored at a desired path
        /// </summary>
        [Test]
        public void WriteSchedule()
        {

        }
    }
}
