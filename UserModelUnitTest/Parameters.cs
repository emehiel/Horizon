using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using UserModel;

namespace UserModelUnitTest
{
    [TestFixture]
    public class ParametersUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        [Test]
        public void LoadSim()
        {
            string simInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");

            var XmlDoc = new XmlDocument();
            XmlDoc.Load(simInputFilePath);
            XmlNodeList simulationInputXMLNodeList = XmlDoc.GetElementsByTagName("SCENARIO");
            var XmlEnum = simulationInputXMLNodeList.GetEnumerator();
            XmlEnum.MoveNext();
            var simulationInputXMLNode = (XmlNode)XmlEnum.Current;
            bool bingo = SimParameters.LoadSimParameters(simulationInputXMLNode["SIMULATION_PARAMETERS"], "Act1");
            Assert.AreEqual(30, SimParameters.SimEndSeconds);
            Assert.AreEqual(0, SimParameters.SimStartSeconds);
            Assert.AreEqual(2454680.0, SimParameters.SimStartJD);

        }
        [Test]
        public void LoadSched()
        {
            string simInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");

            var XmlDoc = new XmlDocument();
            XmlDoc.Load(simInputFilePath);
            XmlNodeList simulationInputXMLNodeList = XmlDoc.GetElementsByTagName("SCENARIO");
            var XmlEnum = simulationInputXMLNodeList.GetEnumerator();
            XmlEnum.MoveNext();
            var simulationInputXMLNode = (XmlNode)XmlEnum.Current;
            bool bingo = SchedParameters.LoadSchedParameters(simulationInputXMLNode["SCHEDULER_PARAMETERS"]);
            Assert.AreEqual(6, SchedParameters.MaxNumScheds);
            Assert.AreEqual(5, SchedParameters.NumSchedCropTo);
            Assert.AreEqual(30, SchedParameters.SimStepSeconds);
        }
    }
}
