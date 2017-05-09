using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using IronPython.Runtime;
using Utilities;

namespace PythonTest
{
    [TestFixture]
    public class AeropredictionTest
    {
        [NonSerialized]
        private dynamic _pythonInstance;


        [OneTimeSetUp]
        public void OneTimeInit()
        {
            
            string pythonFilePath = @"C:\Users\steve\Source\Repos\Horizon\PythonScripting\Aeroprediction.py";
            string className = "AeroPrediction";
            Dictionary<string, object> options = new Dictionary<string, object>();
            options["Debug"] = true;
            var engine = Python.CreateEngine(options);
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            var p = engine.GetSearchPaths();
            p.Add("C:\\Users\\steve\\Source\\Repos\\Horizon\\");
            engine.SetSearchPaths(p);
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType);
            
        }
        [Test]
        public void DragCoefficientTest()
        {
            // TODO: Add your test code here
            Vector vel = new Vector(3);
            vel[1] = 102.9;
            double Cd = _pythonInstance.DragCoefficient(0, vel);
            Assert.AreEqual(0.38, Cd, .005);
        }
    }
}
