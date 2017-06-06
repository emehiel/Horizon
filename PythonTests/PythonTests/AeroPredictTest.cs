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
    public class AeroPredictTest
    {
        protected dynamic _pythonInstance;
        [OneTimeSetUp]
        public void SetUp()
        {
            string modelInputFilePath = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\Hydra70AeroTest.xml";
            var modelInputXMLNode = XmlParser.GetModelNode(modelInputFilePath);
            XmlNode assetXMLNode = modelInputXMLNode.ChildNodes[0];

            XmlNode dynamicStateXMLNode = assetXMLNode["DynamicState"];
            XmlNode scriptedNode = dynamicStateXMLNode["EOMS"];
            string pythonFilePath = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\PythonScripting\\AeroPrediction.py";
            string className = "AeroPrediction";

            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            var p = engine.GetSearchPaths();
            p.Add(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\PythonScripting");
            engine.SetSearchPaths(p);
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType, scriptedNode.ChildNodes[0]);
        }
        [TestCase(1.2, -2 * Math.PI / 180, -.58)]
        [TestCase(1.2, 0 * Math.PI / 180, 0)]
        [TestCase(1.2, 2 * Math.PI / 180, 0.58)]
        [TestCase(1.2, 4 * Math.PI / 180, 1.17)]
        [TestCase(1.2, 6 * Math.PI / 180, 1.76)]
        [TestCase(1.2, 8 * Math.PI / 180, 2.35)]
        public void CnTest(double mach, double alpha, double CnTunnel)
        {
            Vector vel = new Vector(3);
            vel[1] = mach*343;
            double alt = 0;
            Vector deflection = new Vector(4);

            double Cn = _pythonInstance.NormalForceCoefficient(alt, vel, alpha, deflection);
            Assert.AreEqual(CnTunnel, Cn, .01);
        }
        
    }
    public class Hydra70Drag
    {
        protected dynamic _pythonInstance;
        [OneTimeSetUp]
        public void SetUp()
        {
            string modelInputFilePath = AppDomain.CurrentDomain.BaseDirectory+ "..\\..\\..\\L-65931AeroTest.xml";
            var modelInputXMLNode = XmlParser.GetModelNode(modelInputFilePath);
            XmlNode assetXMLNode = modelInputXMLNode.ChildNodes[0];

            XmlNode dynamicStateXMLNode = assetXMLNode["DynamicState"];
            XmlNode scriptedNode = dynamicStateXMLNode["EOMS"];
            string pythonFilePath = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\PythonScripting\\AeroPrediction.py";
            string className = "AeroPrediction";
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            var p = engine.GetSearchPaths();
            p.Add(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\PythonScripting");
            engine.SetSearchPaths(p);
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType, scriptedNode.ChildNodes[0]);
        }

        [TestCase(0.75, 0.34)]
        [TestCase(0.81, 0.34)]
        [TestCase(0.90, 0.35)]
        [TestCase(0.95, 0.39)]
        [TestCase(0.99, 0.44)]
        [TestCase(1.05, 0.49)]
        [TestCase(1.22, 0.48)]
        [TestCase(1.34, 0.47)]
        public void CdTest(double mach, double Cd)
        {
            Vector vel = new Vector(3);
            vel[1] = mach * 343;
            double alt = 0;
            Vector deflection = new Vector(4);

            double CdPredict = _pythonInstance.DragCoefficient(alt, vel);

            Assert.AreEqual(Cd, CdPredict, 0.01);
        }
    }
    }
