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
            string[] inputArg = { "-m", @"C:\Users\steve\Source\Repos\Horizon\Model_Scripted_RocketEOM.xml", "-s", @"C:\Users\steve\Source\Repos\Horizon\SimulationInput_RocketScripted.xml" };
            program.InitInput(inputArg);
            program.LoadSubsystems();
            program.LoadDependencies();
        }
        [SetUp]
        public void Init()
        {
            XmlNode node = XmlParser.GetModelNode(@"C:\Users\steve\Source\Repos\Horizon\Model_Scripted_RocketEOM.xml");
            testIMU = new IMU(node, program.assetList[0]);
        }
        [Test()]
        public void IMUTest()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void GyroscopeTest()
        {
           
            List<double> input = new List<double>(new double[3] { 2200, -2001, 3 } );
            List<double> expected = new List<double>(new double[3] { 2000, -2000, 3 });

            List<double> output = testIMU.Gyroscope(input);
            Assert.Multiple(() =>
            {
                Assert.That(() => output[0], Is.EqualTo(expected[0]));
                Assert.That(() => output[1], Is.EqualTo(expected[1]));
                Assert.That(() => output[2], Is.InRange(expected[2] - 1, expected[2] + 1));
            });
        }
        [Test()]
        public void GyroscopeGenerateTest()
        {
            //List<List<double[,]>> data = new List<List<double>>(new double[3,1000]);
            List<List<double>> output = Enumerable.Repeat(new List<double>(new double[3] { 0, 0, 0 }), 1000).ToList();
            int i = 0;
            //File.Delete(@"C:\Users\steve\Resilio Sync\Documents\MATLAB\Thesis\TestOutput\gyroscopeTest.csv");
            //var csv = new StringBuilder();
            //csv.Clear();
            List<List<double>> data = Enumerable.Repeat(new List<double>(new double[3] { 0, 0, 0}), 1000).ToList();
            foreach (List<double> dataPt in data)
            {
                output[i] = testIMU.Gyroscope(dataPt);    
                //string outputToWrite = string.Join(",", output[i].ToArray());
                //File.AppendAllText(@"C:\Users\steve\Resilio Sync\Documents\MATLAB\Thesis\TestOutput\gyroscopeTest.csv", outputToWrite);
                //File.AppendAllText(@"C:\Users\steve\Resilio Sync\Documents\MATLAB\Thesis\TestOutput\gyroscopeTest.csv", "\n");
                i++;
            }
            Assert.Multiple(() =>
            {
                for (int ii = 0; ii < 1000; ii++)
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