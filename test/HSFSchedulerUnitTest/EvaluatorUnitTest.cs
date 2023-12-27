﻿using Horizon;
using HSFScheduler;
using HSFSystem;
using MissionElements;
using NUnit.Framework;
using System;
using System.IO;

namespace HSFSchedulerUnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
    public class EvaluatorUnitTest
    {
        public Evaluator eval;

        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void ScriptedEvaluatorCtor()
        {
            //arrange
            string SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scripted.xml");
            Dependency dep = Dependency.Instance;
            //XmlNode simNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);
            //act / assert
            //ScriptedEvaluator s = new ScriptedEvaluator(simNode, dep);
            //only fails if runtime error since no public acessors

        }
        [Test]
        public void ScriptEvaluate()
        {
            //arrange
            Program program = new Program();
            program.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scripted.xml");
            //Dependency dep = Dependency.Instance;
            program.LoadEvaluator();

            //XmlNode simNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);

            //ScriptedEvaluator s = new ScriptedEvaluator(simNode, dep); 

            //Asset asset = new Asset();
            SystemState sysstate = new SystemState();
            //StateHistory hist = new StateHistory(sysstate);
            SystemSchedule schedule = new SystemSchedule(sysstate);
            program.Schedules.Add(schedule);

            //act
            double sumout = program.EvaluateSchedules();
            //double sumout = s.Evaluate(schedule);

            //assert
            Assert.AreEqual(2, sumout);
        }
        [Test]
        public void EvaluatorFactUT() 
        {
            //arrange
            Program program1 = new Program();
            program1.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scripted.xml");
            //Dependency dep = Dependency.Instance;
            //XmlNode simNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);            
            program1.LoadEvaluator();
            
            Program program2 = new Program();
            program2.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            //XmlNode simNode2 = XmlParser.ParseSimulationInput(SimulationInputFilePath2);
            program2.LoadEvaluator();

            //act
            Evaluator valuator = program1.SchedEvaluator;
            Evaluator valuator2 = program2.SchedEvaluator;

            //Evaluator valuator = EvaluatorFactory.GetEvaluator(simNode, dep);
            //Evaluator valuator2 = EvaluatorFactory.GetEvaluator(simNode2, dep);
            
            //assert
            Assert.IsInstanceOf(typeof(ScriptedEvaluator), valuator);
            Assert.IsInstanceOf(typeof(TargetValueEvaluator), valuator2);
            
            
        }

    }
}