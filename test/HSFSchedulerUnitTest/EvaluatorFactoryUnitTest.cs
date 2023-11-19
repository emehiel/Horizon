using System;
using NUnit.Framework;
using HSFScheduler;
using System.Xml;
using System.Linq;
using UserModel;
using System.Collections.Generic;
using HSFSystem;
using HSFSubsystem;
using Utilities;
using MissionElements;
using HSFUniverse;
using Horizon;
using System.IO;

namespace HSFEvaluatorFactoryUnitTest
{
    [TestFixture]
    public class EvaluatorFactoryUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void EvaluatorFactory() //why is this here? not a method in scheduler
        {
            Program programAct = new Program();
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");


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

            XmlNode evaluatorNode = null;
            Evaluator schedEvaluator = EvaluatorFactory.GetEvaluator(evaluatorNode, programAct._dependencies);
            double ExpDepCount = 9;
            string ActDepCount = schedEvaluator.ToString();
            Assert.Inconclusive("Not Implemented");
        }
    }
}

