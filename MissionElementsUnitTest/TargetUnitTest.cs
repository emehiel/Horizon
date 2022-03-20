using System;
using NUnit.Framework;
using MissionElements;
using Utilities;
using System.IO;
using UserModel;
using System.Xml;
using HSFUniverse;

namespace MissionElementsUnitTest
{
    [TestFixture]
    public class TargetUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void TargetConstructorUnitTest()
        {
            //arrange
            string SimulationFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            XmlNode simNode = XmlParser.ParseSimulationInput(SimulationFilePath);
            string TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            XmlNode targNode = XmlParser.GetTargetNode(TargetDeckFilePath);
            DynamicState dynamicState = new DynamicState(targNode.FirstChild.FirstChild);

            //act
            Target targ1 = new Target(targNode.FirstChild);
            Target targ2 = new Target("groundstation1", (TargetType)Enum.Parse(typeof(TargetType), "FacilityTarget"), dynamicState, -1);

            //assert
            Assert.AreEqual("groundstation1", targ1.Name);
            Assert.AreEqual((TargetType)Enum.Parse(typeof(TargetType), "FacilityTarget"), targ1.Type);
            Assert.AreEqual(dynamicState.InitialConditions(), targ1.DynamicState.InitialConditions()) ;
            Assert.AreEqual(-1, targ1.Value);

            Assert.AreEqual("groundstation1", targ2.Name);
            Assert.AreEqual((TargetType)Enum.Parse(typeof(TargetType), "FacilityTarget"), targ2.Type);
            Assert.AreEqual(dynamicState.InitialConditions(), targ2.DynamicState.InitialConditions());
            Assert.AreEqual(-1, targ2.Value);
        }   

    }
}
