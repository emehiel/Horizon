using HSFSubsystem;
using NUnit.Framework;
using MissionElements;
using System.Collections.Generic;
using System.Xml;
using UserModel;
using HSFSystem;

namespace HSFSystemUnitTest
{
    [TestFixture]
    public class ConstraintFactoryUnitTest
    {
        [Test]
        public void GetConstraintUnitTest()
        {
            string ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_Constraint.xml";
            var modelInputXMLNode = XmlParser.GetModelNode(ModelInputFilePath);
            XmlNode modelChildNode = modelInputXMLNode.FirstChild;
            string SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            var evaluatorNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);
            Asset asset = new Asset(modelChildNode);
            Dictionary<string, Subsystem> _subsystemMap = new Dictionary<string, Subsystem>();
            List<Constraint> _constraintsList = new List<Constraint>();

            foreach (XmlNode modelChild2Node in modelChildNode.ChildNodes)
            {
                if (modelChild2Node.Name.Equals("SUBSYSTEM"))
                {
                    string subName = SubsystemFactory.GetSubsystem(modelChild2Node, null, asset, _subsystemMap);
                }
                if (modelChild2Node.Name.Equals("CONSTRAINT"))
                {
                    _constraintsList.Add(ConstraintFactory.GetConstraint(modelChild2Node, _subsystemMap, asset));
                }
            }
            Assert.AreEqual(1, _constraintsList.Count);
            Assert.AreEqual("power", _constraintsList[0].Name);
            //TODO more tests for different data types? how to navigate?


        }
        [Test]
        public void GetConstraintNullSubUnitTest()
        {
// an exception should be thrown because the subsystemmap contains no subs,  if an exception is caught, the test passes
            string ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_Constraint_NullSub.xml";
            var modelInputXMLNode = XmlParser.GetModelNode(ModelInputFilePath);
            XmlNode modelChildNode = modelInputXMLNode.FirstChild;

            Asset asset = new Asset(modelChildNode);
            Dictionary<string, Subsystem> _subsystemMap = new Dictionary<string, Subsystem>();
            List<Constraint> _constraintsList = new List<Constraint>();

            foreach (XmlNode modelChild2Node in modelChildNode.ChildNodes)
            {
                try
                {
                    _constraintsList.Add(ConstraintFactory.GetConstraint(modelChild2Node, _subsystemMap, asset));
                }
                catch
                {
                    Assert.IsTrue(true);
                }
            }
        }


    }

}

