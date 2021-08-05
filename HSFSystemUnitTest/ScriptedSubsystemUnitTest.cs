using Horizon;
using HSFScheduler;
using HSFSubsystem;
using HSFSystem;
using HSFUniverse;
using MissionElements;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UserModel;

namespace HSFSystemUnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
    public class ScriptedSubsystemUnitTest
    {
        public Evaluator eval;
        public Event vent;
        public SystemState initialstate;
        public Task task;
        public Asset asset;

        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void ScriptedSubsystemCtor()
        {
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_ScriptedSub.xml");
            Dependency dep = Dependency.Instance;
            XmlNode modelNode = XmlParser.GetModelNode(modelInputFilePath);
            eventMaker(true);
            //int f = vent.GetAssetTask(asset).MaxTimesToPerform;
            ScriptedSubsystem s = new ScriptedSubsystem(modelNode.FirstChild.ChildNodes[1], dep, asset);
        }
        [Test]
        public void ScriptCanPerform() 
        {
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_ScriptedSub.xml");
            Dependency dep = Dependency.Instance;
            XmlNode modelNode = XmlParser.GetModelNode(modelInputFilePath);
            eventMaker(true);

            ScriptedSubsystem s = new ScriptedSubsystem(modelNode.FirstChild.ChildNodes[1], dep, asset);
            SpaceEnvironment SE = new SpaceEnvironment();
            bool canperform = s.CanPerform(vent, SE) ;
            Assert.AreEqual(false, canperform);

        }
        public void eventMaker(bool perform)
        {
            string ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_ScriptedSub.xml");
            string SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            var simulationInputNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);
            var modelInputNode = XmlParser.GetModelNode(ModelInputFilePath);
            DynamicState dynState = new DynamicState((DynamicStateType)Enum.Parse(typeof(DynamicStateType), "STATIC_LLA"), new OrbitalEOMS(), new Utilities.Vector("[33.47; -70.65; 0]"));
            SystemState sysState = new SystemState();
            Target targ = new Target("dummy", (TargetType)Enum.Parse(typeof(TargetType), "LocationTarget"), dynState, 1);

            if (perform)
            { task = new Task((TaskType)Enum.Parse(typeof(TaskType), "IMAGING"), targ, 10); }
            else
            { task = new Task((TaskType)Enum.Parse(typeof(TaskType), "IMAGING"), targ, 2); }

                Dictionary<Asset, Task> taskdic = new Dictionary<Asset, Task>();
            asset = new Asset(modelInputNode.FirstChild);
            taskdic.Add(asset, task);
            vent = new Event(taskdic, sysState);
        }

    }
}
