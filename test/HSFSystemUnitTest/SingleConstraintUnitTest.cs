using HSFSystem;
using NUnit.Framework;
using MissionElements;
using System;
using System.Collections.Generic;
using System.Xml;
using UserModel;
using System.IO;

namespace HSFSystemUnitTest
{
    [TestFixture]
    public class SingleConstraintUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        [Test]
        public void ConstructorUnitTest()
        {
            //arrange
            string ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_Constraint.xml");
            string SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.XML");

            var modelInputXMLNode = XmlParser.GetModelNode(ModelInputFilePath);
            var evaluatorNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);

            XmlNode modelChildNode = modelInputXMLNode.FirstChild;
            Asset asset = new Asset(modelChildNode);
            Dictionary<string, Subsystem> _subsystemMap = new Dictionary<string, Subsystem>();
            List<Constraint> _constraintsList = new List<Constraint>();
            XmlNode ConstraintNode = null;

            foreach (XmlNode modelChild2Node in modelChildNode.ChildNodes)
            {
                if (modelChild2Node.Name.Equals("SUBSYSTEM"))
                {
                    string subName = SubsystemFactory.GetSubsystem(modelChild2Node, null, asset, _subsystemMap);
                }
                if (modelChild2Node.Name.Equals("CONSTRAINT"))
                {
                    _constraintsList.Add(ConstraintFactory.GetConstraint(modelChild2Node, _subsystemMap, asset));
                    ConstraintNode = modelChild2Node;
                }
            }
            //act
            SingleConstraint<double> con1 = new SingleConstraint<double>(ConstraintNode, _subsystemMap["asset1.power"]);

            //assert
            Assert.AreEqual(1, _constraintsList.Count);
            Assert.AreEqual("power", _constraintsList[0].Name);

        }
        [Test]
        public void ConstructorNullUnitTest()
        {
            //arrange
            string ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_Constraint.xml");
            string SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.XML");

            var modelInputXMLNode = XmlParser.GetModelNode(ModelInputFilePath);
            var evaluatorNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);

            XmlNode modelChildNode = modelInputXMLNode.FirstChild;
            Asset asset = new Asset(modelChildNode);
            Dictionary<string, Subsystem> _subsystemMap = new Dictionary<string, Subsystem>();
            List<Constraint> _constraintsList = new List<Constraint>();
            XmlNode ConstraintNode = null;

            foreach (XmlNode modelChild2Node in modelChildNode.ChildNodes)
            {
                if (modelChild2Node.Name.Equals("SUBSYSTEM"))
                {
                    string subName = SubsystemFactory.GetSubsystem(modelChild2Node, null, asset, _subsystemMap);
                }
                if (modelChild2Node.Name.Equals("CONSTRAINT"))
                {
                    _constraintsList.Add(ConstraintFactory.GetConstraint(modelChild2Node, _subsystemMap, asset));
                    ConstraintNode = modelChild2Node;
                }
            }

            //act
            SingleConstraint<double> con1 = new SingleConstraint<double>(ConstraintNode, _subsystemMap["asset1.power"]);
            
            //assert
            Assert.AreEqual(1, _constraintsList.Count);
            Assert.AreEqual("power", _constraintsList[0].Name);

        }

        [Test]
        public void AcceptsUnitTest()
        {
            //arrange
            string ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_ManyConstraints.xml");
            string SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.XML");
            var modelInputXMLNode = XmlParser.GetModelNode(ModelInputFilePath);
            var evaluatorNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);
            XmlNode modelChildNode = modelInputXMLNode.FirstChild;
            //List<XmlNode> ICNodes = new List<XmlNode>();
            Asset asset = new Asset(modelChildNode);
            Dictionary<string, Subsystem> _subsystemMap = new Dictionary<string, Subsystem>();
            List<Constraint> _constraintsList = new List<Constraint>();
            List<XmlNode> ConstraintNode = new List<XmlNode>();
            SystemState InitialSysState = new SystemState();

            foreach (XmlNode modelChild2Node in modelChildNode.ChildNodes)
            {
                if (modelChild2Node.Name.Equals("SUBSYSTEM"))
                {
                    string subName = SubsystemFactory.GetSubsystem(modelChild2Node, null, asset, _subsystemMap);
                    foreach (XmlNode modelChild3Node in modelChild2Node.ChildNodes)
                    {
                        InitialSysState.Add(SystemState.SetInitialSystemState(modelChild3Node, asset));
                        //ICNodes.Add(modelChild3Node);
                    }
                }
                if (modelChild2Node.Name.Equals("CONSTRAINT"))
                {
                    _constraintsList.Add(ConstraintFactory.GetConstraint(modelChild2Node, _subsystemMap, asset));
                    ConstraintNode.Add(modelChild2Node);
                }
            }
            SingleConstraint<double> HigherA = new SingleConstraint<double>(ConstraintNode[0], _subsystemMap["asset1.power"]);
            SingleConstraint<double> HigherB = new SingleConstraint<double>(ConstraintNode[1], _subsystemMap["asset1.power"]);
            SingleConstraint<double> EqualA = new SingleConstraint<double>(ConstraintNode[2], _subsystemMap["asset1.power"]);
            SingleConstraint<double> EqualB = new SingleConstraint<double>(ConstraintNode[3], _subsystemMap["asset1.power"]);
            SingleConstraint<double> NotEqualA = new SingleConstraint<double>(ConstraintNode[4], _subsystemMap["asset1.power"]);
            SingleConstraint<double> NotEqualB = new SingleConstraint<double>(ConstraintNode[5], _subsystemMap["asset1.power"]);
            SingleConstraint<double> HigherEqA = new SingleConstraint<double>(ConstraintNode[6], _subsystemMap["asset1.power"]);
            SingleConstraint<double> HigherEqB = new SingleConstraint<double>(ConstraintNode[7], _subsystemMap["asset1.power"]);
            SingleConstraint<double> LowerEqA = new SingleConstraint<double>(ConstraintNode[8], _subsystemMap["asset1.power"]);
            SingleConstraint<double> LowerEqB = new SingleConstraint<double>(ConstraintNode[9], _subsystemMap["asset1.power"]);
            SingleConstraint<double> LowerA = new SingleConstraint<double>(ConstraintNode[10], _subsystemMap["asset1.power"]);
            SingleConstraint<double> LowerB = new SingleConstraint<double>(ConstraintNode[11], _subsystemMap["asset1.power"]);


            //InitialSysState.Add(SystemState.SetInitialSystemState(ICNodes,asset));

            //act + assert (testing accepts which is boolean)
            Assert.IsTrue(HigherA.Accepts(InitialSysState));
            Assert.IsFalse(HigherB.Accepts(InitialSysState));
            Assert.IsTrue(EqualA.Accepts(InitialSysState));
            Assert.IsFalse(EqualB.Accepts(InitialSysState));
            Assert.IsTrue(NotEqualA.Accepts(InitialSysState));
            Assert.IsFalse(NotEqualB.Accepts(InitialSysState));
            Assert.IsTrue(HigherEqA.Accepts(InitialSysState));
            Assert.IsFalse(HigherEqB.Accepts(InitialSysState)); //Fixed, used to pass with equal vals but should fail
            Assert.IsTrue(LowerEqA.Accepts(InitialSysState));
            Assert.IsFalse(LowerEqB.Accepts(InitialSysState)); //BugFIxed passes with equal vals but should fail
            Assert.IsTrue(LowerA.Accepts(InitialSysState));
            Assert.IsFalse(LowerB.Accepts(InitialSysState));

        }
    }
}
