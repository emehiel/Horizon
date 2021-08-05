using HSFSystem;
using System;
using System.Collections.Generic;
using System.Text;
using HSFScheduler;
using HSFSystem;
using NUnit.Framework;
using System.Xml;
using MissionElements;
using System.IO;
using UserModel;
using Horizon;

namespace HSFSchedulerUnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
    public class TargetValueEval
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        /// <summary>
        /// Tests double Evaluate(SystemSchedule) function from Target Value Evaluator Class
        /// should return the sum of target values from the Access Stack, 
        /// </summary>
        [Test]
        public void EvaluateTest()
        {
            Program programAct = new Program();
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_TargValEval.xml");
            Stack<Task> systemTasks = programAct.LoadTargets();

            XmlNode simNode = XmlParser.ParseSimulationInput(programAct.SimulationInputFilePath); // only need this in order to construct asset
            XmlNode modelNode = XmlParser.GetModelNode(programAct.ModelInputFilePath);
            Asset asset = new Asset(modelNode.ChildNodes[1]);
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                Console.WriteLine("LoadSubsystems Failed the Unit test");
                Assert.Fail();
            }
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
            StateHistory hist = new StateHistory(systemState);
            Stack<Access> accesses = new Stack<Access>();
            accesses.Push(new Access(asset, systemTasks.Pop()));

            SystemSchedule sysSched = new SystemSchedule(hist, accesses, 0);
            Evaluator TVE = new TargetValueEvaluator(programAct._dependencies); 
            double sum = TVE.Evaluate(sysSched); //started here, needed schedule (54) and evaluator (56)
            Assert.AreEqual(1, sum);

            accesses.Pop();// goes one target deeper into the deck, val=-1
            accesses.Push(new Access(asset, systemTasks.Pop()));
            SystemSchedule sysSched2 = new SystemSchedule(hist, accesses, 0);
            double sum2 = TVE.Evaluate(sysSched2); 
            Assert.AreEqual(-1, sum2);

        }
    }
}
