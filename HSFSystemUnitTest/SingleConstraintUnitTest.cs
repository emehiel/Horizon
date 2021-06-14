using HSFSubsystem;
using HSFSystem;
using NUnit.Framework;
using MissionElements;
using System;
using System.Collections.Generic;
using System.Xml;
using UserModel;

namespace HSFSystemUnitTest
{
    [TestFixture]
    public class SingleConstraintUnitTest
    {
        [Test]
        public void ConstructorUnitTest()
        {
            string ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_Constraint.xml";
            string SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";

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
            SingleConstraint<double> con1 = new SingleConstraint<double>(ConstraintNode, _subsystemMap["asset1.power"]);
            Assert.AreEqual(1, _constraintsList.Count);
            Assert.AreEqual("power", _constraintsList[0].Name);

        }
        [Test]
        public void ConstructorNullUnitTest()
        {
            string ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_Constraint.xml";
            string SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";

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
            SingleConstraint<double> con1 = new SingleConstraint<double>(ConstraintNode, _subsystemMap["asset1.power"]);
            Assert.AreEqual(1, _constraintsList.Count);
            Assert.AreEqual("power", _constraintsList[0].Name);

        }

        [Test]
        public void AcceptsUnitTest()
        {
            string ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_Constraint.xml";
            string SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            var modelInputXMLNode = XmlParser.GetModelNode(ModelInputFilePath);
            var evaluatorNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);
            XmlNode modelChildNode = modelInputXMLNode.FirstChild;
            Asset asset = new Asset(modelChildNode);
            Dictionary<string, Subsystem> _subsystemMap = new Dictionary<string, Subsystem>();
            List<Constraint> _constraintsList = new List<Constraint>();
            XmlNode ConstraintNode = null;
            SystemState InitialSysState = new SystemState();

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
            SingleConstraint<double> con1 = new SingleConstraint<double>(ConstraintNode, _subsystemMap["asset1.power"]);
            SystemState nothing = new SystemState();
            SystemState stillnothing = new SystemState(nothing);
            bool t = con1.Accepts(stillnothing);
            Assert.IsTrue(t);

            //Assert.Inconclusive("Not implemented");
        }
    }
}
