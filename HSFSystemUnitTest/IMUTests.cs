using NUnit.Framework;
using HSFSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MissionElements;
using UserModel;
using Horizon;
using System.IO;
using Utilities;

namespace HSFSystem.Tests
{
    [TestFixture()]
    public class IMUTests
    {
        IMU testIMU;
        Program program;
        [OneTimeSetUp]
        public void OneTimeInit()
        {
            program = new Program();
            string[] inputArg = { "-m", AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\IMUTests.xml", "-s", AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\..\\SimulationInput.xml", "-t", AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\..\\v2.2-300targets.xml" };
            
            program.InitInput(inputArg);
            program.LoadTargets();
            program.LoadSubsystems();
        }
        [SetUp]
        public void Init()
        {
            XmlNode node = XmlParser.GetModelNode(AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\IMUTests.xml");
            testIMU = new IMU(node.FirstChild.ChildNodes[1], program.assetList[0]);
        }

        [Test()]
        public void GyroscopeTest()
        {
           
            Vector input = new Vector(new List<double> (new double[] { 250, -290, 3*Math.PI/180 } ));
            Vector expected = new Vector(new List<double>(new double[3] { 245, -286.72, 3 }));

            Vector output = testIMU.Gyroscope(input);
            Assert.Multiple(() =>
            {
                Assert.That(() => output[1], Is.EqualTo(expected[1]));
                Assert.That(() => output[2], Is.EqualTo(expected[2]));
                Assert.That(() => output[3], Is.InRange(expected[3] - 1, expected[3] + 1));
            });
        }
        [Test()]
        public void GyroscopeGenerateTest()
        {
            List<Vector> output = Enumerable.Repeat(new Vector(new List<double>(new double[3] { 0, 0, 0 })), 250).ToList();
            int i = 0;
            try
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\TestOutput\\gyroscopeTest.csv");
            }
            catch { }
            var csv = new StringBuilder();
            csv.Clear();
            List<Vector> data = Enumerable.Repeat(new Vector(new List<double>(new double[3] { 0, 0, 0})), 250).ToList();
            foreach (Vector dataPt in data)
            {
                output[i] = testIMU.Gyroscope(dataPt);    
                string outputToWrite = string.Join(",", output[i].ToString());
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\TestOutput\\gyroscopeTest.csv", outputToWrite);
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\TestOutput\\gyroscopeTest.csv", "\n");
                i++;
            }
            Assert.Multiple(() =>
            {
                for (int ii = 1; ii < 250; ii++)
                {
                    Assert.That(() => output[ii], Is.All.InRange(-1.0, 1.0));
                }
            });
        }
        [Test()]
        public void AccelerometerTest()
        {
            Vector input = new Vector(new List<double>(new double[] { 90, -90, 20 }));
            Vector expected = new Vector(new List<double>(new double[3] { 8, -8, 2 }));

            Vector output = testIMU.Accelerometer(input);
            Assert.Multiple(() =>
            {
                Assert.That(() => output[1], Is.EqualTo(expected[1]));
                Assert.That(() => output[2], Is.EqualTo(expected[2]));
                Assert.That(() => output[3], Is.InRange(expected[3] - 1, expected[3] + 1));
            });
        }
    }
}