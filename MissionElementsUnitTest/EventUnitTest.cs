using System;
using NUnit.Framework;
using MissionElements;
using Utilities;
using System.Collections.Generic;
using HSFUniverse;
using UserModel;
using System.IO;

namespace MissionElementsUnitTest
{
    [TestFixture]
    public class EventUnitTest
    {
        Asset asset;
        Target targ;
        Task task;
        Dictionary<Asset, Task> taskdic;
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        SystemState sysState;
        Event vent;
        [Test]
        public void Constructor()
        {
            helper();
            vent = new Event(taskdic, sysState);
            Event copyVent = new Event(vent);
            Assert.AreEqual(taskdic, vent.Tasks);
            Assert.AreEqual(sysState, vent.State);
            Assert.AreEqual(new Dictionary<Asset, double>(), vent.EventStarts);
            Assert.AreEqual(new Dictionary<Asset, double>(), vent.EventEnds);
            Assert.AreEqual(new Dictionary<Asset, double>(), vent.TaskStarts);
            Assert.AreEqual(new Dictionary<Asset, double>(), vent.TaskEnds);
            Assert.AreEqual(0, vent.isEvaluated);

            Assert.AreNotSame(copyVent.TaskStarts, vent.TaskStarts);
            Assert.AreNotSame(copyVent.TaskEnds, vent.TaskStarts);
            Assert.AreNotSame(copyVent.State, vent.State);
            Assert.AreSame(copyVent.EventStarts, vent.EventStarts);
            Assert.AreSame(copyVent.EventEnds, vent.EventEnds);

        }
        /// <summary>
        /// Tests the both sets for each variable: set(asset,double) and set(dictionary<asset,double>) and the get call when each of these are called
        /// </summary>
        [Test]
        public void Accessors()
        {
            helper();
            vent = new Event(taskdic, sysState);
            
            Assert.AreSame(task, vent.GetAssetTask(asset));
            Dictionary<Asset, double> six = new Dictionary<Asset, double>();
            six.Add(asset, 6);

            // TASKSTART
            Assert.AreEqual(0, vent.GetTaskStart(asset));
            vent.SetTaskStart(asset, 1);
            Assert.AreEqual(1, vent.GetTaskStart(asset));
            vent.SetTaskStart(six);
            Assert.AreEqual(6, vent.GetTaskStart(asset));

            // EVENTSTART
            Assert.AreEqual(0, vent.GetEventStart(asset));
            vent.SetEventStart(asset, 1);
            Assert.AreEqual(1, vent.GetEventStart(asset));
            vent.SetEventStart(six);
            Assert.AreEqual(6, vent.GetEventStart(asset));

            // TASKEND
            Assert.AreEqual(0, vent.GetTaskEnd(asset));
            vent.SetTaskEnd(asset, 2);
            Assert.AreEqual(2, vent.GetTaskEnd(asset));
            vent.SetTaskEnd(six);
            Assert.AreEqual(6, vent.GetTaskEnd(asset));

            // EVENTEND
            Assert.AreEqual(0, vent.GetEventEnd(asset));
            vent.SetEventEnd(asset, 2);
            Assert.AreEqual(2, vent.GetEventEnd(asset));
            vent.SetEventEnd(six);
            Assert.AreEqual(6, vent.GetEventEnd(asset));
        }
        public void helper()
        {
                string ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_ScriptedSub.xml");
                string SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
                var simulationInputNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);
                var modelInputNode = XmlParser.GetModelNode(ModelInputFilePath);
                DynamicState dynState = new DynamicState((DynamicStateType)Enum.Parse(typeof(DynamicStateType), "STATIC_LLA"), new OrbitalEOMS(), new Utilities.Vector("[33.47; -70.65; 0]"));
                sysState = new SystemState();
                Target targ = new Target("dummy", "LocationTarget", dynState, 1);

                bool perform = true;
                if (perform)
                { task = new Task("IMAGING", targ, 10); }
                else
                { task = new Task("IMAGING", targ, 2); }

                taskdic = new Dictionary<Asset, Task>();
                asset = new Asset(modelInputNode.FirstChild);
                taskdic.Add(asset, task);
        }

    }
}
