using HSFScheduler;
using HSFUniverse;
using MissionElements;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UserModel;

namespace HSFSchedulerUnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
    public class StateHistoryUnitTest
    {
        public Event vent;
        public SystemState initialstate;
        public Task task;
        public Asset asset;
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void StateHistoryCtor1()
        {
            SystemState initialState = new SystemState();
            StateHistory state = new StateHistory(initialState);
            Assert.IsInstanceOf(typeof(StateHistory), state);
            Assert.AreEqual(initialState, state.InitialState);
            Assert.AreEqual(0, state.Events.Count);
        }
        [Test]
        public void StateHistoryCtor2()
        {
            eventMaker();
            SystemState initialState = new SystemState();
            //constructor 1
            StateHistory emptyHist = new StateHistory(initialState);
            //constructor 2
            StateHistory eventHist = new StateHistory(emptyHist, vent);
            //constructor 3
            StateHistory newHist = new StateHistory(eventHist);
            Assert.AreEqual(initialState, eventHist.InitialState);
            Assert.AreEqual(initialState, newHist.InitialState);
            Assert.AreEqual(task, eventHist.GetLastTask(asset));
            Assert.AreEqual(vent, eventHist.GetLastEvent());
            Assert.AreEqual(task, newHist.GetLastTask(asset));
            Assert.AreEqual(vent, newHist.GetLastEvent());
        }
        public void eventMaker()
        {
            string ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub.xml");
            string SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            var simulationInputNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);
            var modelInputNode = XmlParser.GetModelNode(ModelInputFilePath);
            DynamicState dynState = new DynamicState((DynamicStateType)Enum.Parse(typeof(DynamicStateType), "STATIC_LLA"), new OrbitalEOMS(), new Utilities.Vector("[33.47; -70.65; 0]"));
            SystemState sysState = new SystemState();
            Target targ = new Target("dummy", (TargetType)Enum.Parse(typeof(TargetType), "LocationTarget"), dynState, 1);
            task = new Task((TaskType)Enum.Parse(typeof(TaskType), "IMAGING"), targ, 10);
            Dictionary<Asset, Task> taskdic = new Dictionary<Asset, Task>();
            asset = new Asset(modelInputNode.ChildNodes[1]);
            taskdic.Add(asset, task);
            vent = new Event(taskdic, sysState);
        }

    }
}
