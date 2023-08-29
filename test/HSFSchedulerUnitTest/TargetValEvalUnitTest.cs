using System;
using NUnit.Framework;
using Horizon;
using System.IO;
using MissionElements;
using UserModel;
using System.Xml;

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
        public void TargetValueEvaluator_Constructor_Evaluate() // constructor only has one use, makes no sense to split test
        {
            //arrange
            Program programAct = new Program();
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_TargValEval.xml");
            programAct.LoadTargets();

            //XmlNode simNode = XmlParser.ParseSimulationInput(programAct.SimulationInputFilePath); // only need this in order to construct asset
            XmlNode modelNode = XmlParser.GetModelNode(programAct.ModelInputFilePath);
            //Asset asset = new Asset(modelNode.ChildNodes[1]);
            //trying a fancy way to embed dependent methods
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                Console.WriteLine("LoadSubsystems Failed the Unit test");
                Assert.Fail();
            }

            try
            {
                programAct.LoadEvaluator();
            }
            catch
            {
                Console.WriteLine("LoadEvaluator Failed the Unit test");
                Assert.Fail();
            }

            SystemState systemState = new SystemState();

            //List<XmlNode> ICNodes = new List<XmlNode>();
            //ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[2].FirstChild);


            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[2].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[1], asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[2], asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[4].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[5].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[6].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[6].ChildNodes[1], asset));

            ////SystemState systemState = SystemState.SetInitialSystemState(ICNodes, asset);
            //StateHistory hist = new StateHistory(systemState);
            //Stack<Access> accesses = new Stack<Access>();

            //accesses.Push(new Access(asset, systemTasks.Pop()));
            //SystemSchedule sysSched = new SystemSchedule(hist, accesses, 0);
            ////Evaluator TVE = new TargetValueEvaluator(programAct.Dependencies); 
            ////double sum = TVE.Evaluate(sysSched); //started here, needed schedule (54) and evaluator (56)
            ////Assert.AreEqual(1, sum);

            //accesses.Pop();// goes one target deeper into the deck, val=-1

            //accesses.Push(new Access(asset, systemTasks.Pop()));
            //SystemSchedule sysSched2 = new SystemSchedule(hist, accesses, 0);

            ////act
            ////constructor
            //Evaluator TVE = new TargetValueEvaluator(programAct.Dependencies); 
            ////evaluate
            //double sum = TVE.Evaluate(sysSched);

            //double sum2 = TVE.Evaluate(sysSched2);

            ////assert
            //Assert.AreEqual(1, sum);
            //Assert.AreEqual(-1, sum2);
        }
    }
}
