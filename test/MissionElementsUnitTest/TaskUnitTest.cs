﻿using System;
using NUnit.Framework;
using MissionElements;
using Utilities;
using HSFUniverse;
using UserModel;
using System.IO;
using System.Collections.Generic;
using System.Xml;

namespace MissionElementsUnitTest
{
    [TestFixture]
    public class TaskUnitTest
    {
        Target targ;
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void ConstructorUnitTest()
        {
            //arrange
            taskHelper();
            Task task = new Task("EMPTY", targ, 10);

            //assert
            Assert.AreEqual(targ, task.Target);
            Assert.AreEqual("EMPTY", task.Type);
            Assert.AreEqual(10, task.MaxTimesToPerform);
        }
        /// <summary>
        /// TODO: test pass and fail for TaskType and MaxTimes
        /// </summary>
        [Test]
        public void LoadTargetsIntoList()
        {
            //arrange
            taskHelper();
            XmlNode TargetNodes = null;
            Stack<Task> tasks = new Stack<Task>();

            //act
            bool targetsnotLoaded = Task.LoadTasks(TargetNodes, tasks);

            string targetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            TargetNodes = XmlParser.GetTargetNode(targetDeckFilePath);
            bool loaded = Task.LoadTasks(TargetNodes, tasks);

            //assert
            Assert.IsFalse(targetsnotLoaded);
            Assert.IsTrue(loaded);
        }
        public void taskHelper()
        {
           
            string ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub.xml");
            string SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            var simulationInputNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);
            var modelInputNode = XmlParser.GetModelNode(ModelInputFilePath);
            DynamicState dynState = new DynamicState((DynamicStateType)Enum.Parse(typeof(DynamicStateType), "STATIC_LLA"), new OrbitalEOMS(), new Utilities.Vector("[33.47; -70.65; 0]"));
            SystemState sysState = new SystemState();
            targ = new Target("dummy", "LocationTarget", dynState, 1);
        }
        
    }
}
