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
            string[] inputArg = { "-m", @"C:\Users\steve\Source\Repos\Horizon\Model_Scripted_RocketEOM_Concord.xml", "-s", @"C:\Users\steve\Source\Repos\Horizon\SimulationInput_RocketScripted.xml", "-t", @"C:\Users\steve\Source\Repos\Horizon\rocketTargets.xml" };
            
            program.InitInput(inputArg);
            program.LoadTargets();
            program.LoadSubsystems();
            //program.LoadDependencies();
        }
        [SetUp]
        public void Init()
        {
            XmlNode node = XmlParser.GetModelNode(@"C:\Users\steve\Source\Repos\Horizon\Model_Scripted_RocketEOM_Concord.xml");
            testIMU = new IMU(node.FirstChild.ChildNodes[1], program.assetList[0]);
        }
        [Test()]
        public void IMUTest()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void GyroscopeTest()
        {
           
            Vector input = new Vector(new List<double> (new double[] { 2200, -2001, 3 } ));
            Vector expected = new Vector(new List<double>(new double[3] { 2000, -2000, 3 }));

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
            //List<List<double[,]>> data = new List<List<double>>(new double[3,1000]);
            List<Vector> output = Enumerable.Repeat(new Vector(new List<double>(new double[3] { 0, 0, 0 })), 250).ToList();
            int i = 0;
            File.Delete(@"C:\Users\steve\Resilio Sync\Documents\MATLAB\Thesis\TestOutput\gyroscopeTest.csv");
            var csv = new StringBuilder();
            csv.Clear();
            List<Vector> data = Enumerable.Repeat(new Vector(new List<double>(new double[3] { 0, 0, 0})), 250).ToList();
            foreach (Vector dataPt in data)
            {
                output[i] = testIMU.Gyroscope(dataPt);    
                string outputToWrite = string.Join(",", output[i].ToString());
                File.AppendAllText(@"C:\Users\steve\Resilio Sync\Documents\MATLAB\Thesis\TestOutput\gyroscopeTest.csv", outputToWrite);
                File.AppendAllText(@"C:\Users\steve\Resilio Sync\Documents\MATLAB\Thesis\TestOutput\gyroscopeTest.csv", "\n");
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
            throw new NotImplementedException();
        }

        [Test()]
        public void GaussianWhiteNoiseTest()
        {
            throw new NotImplementedException();
        }
    }
}