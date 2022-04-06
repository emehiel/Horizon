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
        public SystemState initialState;
        public StateHistory emptyHist;
        public StateHistory eventHist;
        public StateHistory newHist;
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        bool eventMaker_Switch = false;
        bool newHistory_Maker = false;


        [Test]
        public void StateHistory_Ctor_SystemStateIC()
        {
            //arrange
            SystemState initialState = new SystemState();
            //act
            StateHistory state = new StateHistory(initialState);
            //assert
            Assert.IsInstanceOf(typeof(StateHistory), state);
            Assert.AreEqual(initialState, state.InitialState);
            Assert.AreEqual(0, state.Events.Count);
        }
        [Test]
        public void StateHistory_Ctor3__via_Ctor1_emptyHistIC()
        {
            //arrange
            eventMaker();
            SystemState initialState = new SystemState();
            //constructor 1
            StateHistory emptyHist = new StateHistory(initialState);

            //act
            //constructor 3
            StateHistory eventHist = new StateHistory(emptyHist, vent);

            //assert
            Assert.AreEqual(initialState, eventHist.InitialState);

        }
        [Test]
        public void StateHistory_Ctor2__via_Ctor1_emptyHistIC_Ctor3_CopyStateHist()//unfortunately all three amust be tested to get to the second ctor
        {
            //arrange
            eventMaker();
            newHistory();

            //act
            //constructor 2
            StateHistory newHist = new StateHistory(eventHist);


            //assert
            Assert.AreEqual(initialState, newHist.InitialState);

        }
        [Test]
        public void GetsTests()
        {
            //arrange
            eventMaker();
            newHistory();
            //taskdic for Dictionary of tasks
            Dictionary<Asset, Task> taskdic = new Dictionary<Asset, Task>();
            taskdic.Add(asset, task);

            //act
            // GetLastState
            SystemState lastState = newHist.GetLastState();
            // GetLastTask
            Task lastTask = newHist.GetLastTask(asset);
            // GetLastTask(asset)
            Task getLastTask_asset = emptyHist.GetLastTask(asset);
            // GetLastTaskS
            Dictionary<Asset, Task> newHistTaskDic = newHist.GetLastTasks();
            // GetLastEvent
            Event lastEvent = newHist.GetLastEvent();



            //assert
            // GetLastState
            Assert.AreEqual(vent.State, lastState);
            // GetLastTask
            Assert.AreEqual(vent.GetAssetTask(asset), lastTask);
            // GetLastTask(asset)
            Assert.AreEqual(null, getLastTask_asset);
            // GetLastTaskS
            Assert.AreEqual(taskdic[asset], newHistTaskDic[asset]);
            //GetLastEvent
            Assert.AreEqual(vent, lastEvent);

        }
        [Test]
        public void sizeChecker() {

            //arrange
            eventMaker();
            newHistory();


            //act
            //int no arg size()
            int newHistSize = newHist.size();
            //int size(asset)
            int newHistSize_Asset = newHist.size(asset);

            //assert
            Assert.AreEqual(1, newHistSize);
            Assert.AreEqual(1, newHistSize_Asset);
        }
        [Test]
        public void StateHistory_timesCompletedTask()
        {
            //arrange
            eventMaker();
            newHistory();
            //act
            int timesCompletedTask = newHist.timesCompletedTask(asset, task);
            //assert
            Assert.AreEqual(1, timesCompletedTask);

        }
        [Test]
        public void StateHistory_isEmpty() {
            //arrange
            eventMaker();
            newHistory();

            //act
            bool emptyHist_isEmp_noarg = emptyHist.isEmpty();
            bool emptyHist_isEmp_asset = emptyHist.isEmpty(asset);
            bool newHist_notEmp_noarg = newHist.isEmpty();
            bool newHist_notEmp_asset = newHist.isEmpty(asset);

            // bool isEmpty
            Assert.IsTrue(emptyHist_isEmp_noarg);
            Assert.IsTrue(emptyHist_isEmp_asset);
            Assert.IsFalse(newHist_notEmp_noarg);
            Assert.IsFalse(newHist_notEmp_asset);

        }
            public void newHistory()
        {
            if (!newHistory_Maker)
            { //only run once
                initialState = new SystemState();
                //constructor 1
                emptyHist = new StateHistory(initialState);
                //constructor 3
                eventHist = new StateHistory(emptyHist, vent);
                //constructor 2
                newHist = new StateHistory(eventHist);
                newHistory_Maker = true;
            }
            else
            {//do nothing
            }

        }
        public void eventMaker()
        {
            if (!eventMaker_Switch) 
            { //ensures it only runs once over this class's test suite to save computation time
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
            eventMaker_Switch = true; //now do nothing
             }
            else
            {//do nothing
            }


        }

    }
}
