using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using System.Xml;
using UserModel;
using Utilities;

namespace PythonTests
{
    [TestFixture]
    public class AeroPredictionTest
    {
        protected dynamic _pythonInstance;
        [OneTimeSetUp]
        public void SetUp()
        {
            string modelInputFilePath = "C:\\Users\\steve\\Source\\Repos\\Horizon\\Model_Scripted_RocketEOM_Concord.xml";
            var modelInputXMLNode = XmlParser.GetModelNode(modelInputFilePath);
            XmlNode assetXMLNode = modelInputXMLNode.ChildNodes[0];

            XmlNode dynamicStateXMLNode = assetXMLNode["DynamicState"];
            XmlNode scriptedNode = dynamicStateXMLNode["EOMS"];
            string pythonFilePath = "C:\\Users\\steve\\Source\\Repos\\Horizon\\PythonScripting\\AeroPrediction.py";
            string className = "AeroPrediction";
            //XmlParser.ParseScriptedSrc(scriptedNode, ref pythonFilePath, ref className);
    
            Dictionary<string, object> options = new Dictionary<string, object>();
            options["Debug"] = true;
            var engine = Python.CreateEngine(options);
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            var p = engine.GetSearchPaths();
            p.Add("C:\\Users\\steve\\Source\\Repos\\Horizon\\PythonScripting\\");
            engine.SetSearchPaths(p);
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType, scriptedNode.ChildNodes[3]);
        }
        [Test]
        public void SupersonicNormalForce()
        {
            // TODO: Add your test code here
            Vector vel = new Vector(3);
            vel[1] = 350;
            double alt = 1000;
            double alpha = .1;
            Vector deflection = new Vector(4);

            double Cn = _pythonInstance.NormalForceCoefficient(alt, vel, alpha, deflection);
            Assert.Pass("Your first passing test");
        }
        [TestCase(100, 1*Math.PI/180, 7.59)]
        [TestCase(10, 10*Math.PI/180, 12.1)]
        [TestCase(210, 5*Math.PI / 180, 9.86)]
        public void PitchingMoment(double velZ, double alpha, double Cna)
        {
            Vector vel = new Vector(3);
            vel[1] = velZ;
            double alt = 0;
            Vector deflection = new Vector(4);

            double Cn = _pythonInstance.PitchingMoment(alt, vel, alpha, deflection, 1.77)/alpha * .155;

            Assert.AreEqual(Cna, Cn, .01);
        }

        [TestCase(100, 1 * Math.PI / 180)]
        [TestCase(10, 10 * Math.PI / 180)]
        [TestCase(210, 5 * Math.PI / 180)]
        public void PitchingandYawingMomentMatch(double velZ, double alpha)
        {
            Vector vel = new Vector(3);
            vel[1] = velZ;
            double alt = 0;
            Vector deflection = new Vector(4);

            double Cn = _pythonInstance.PitchingMoment(alt, vel, alpha, deflection, 1.77) / alpha*.155;
            double Cm = _pythonInstance.YawingMoment(alt, vel, alpha, deflection, 1.77) / alpha*.155;
            Assert.AreEqual(Cm, Cn, .01);
        }

    }
}
