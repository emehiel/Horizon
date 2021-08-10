using System;
using Horizon;
using NUnit.Framework;

using System.Xml;
using System.Linq;
using UserModel;
using System.Collections.Generic;

using Utilities;
using MissionElements;
using HSFUniverse;
using System.IO;

namespace UniverseUnitTest
{
    [TestFixture]
    public class DynamicStateUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        Program programAct;
        DynamicState dynamic;
        Asset asset;
        [Test]
        public void ConstructDynamicState() //TODO:whats going on here jack
        {
            helperConst("UnitTestInputs\\UnitTestModel_DynState.xml");

            string actual = dynamic.ToString();
            string expected = "Asset1.DynamicState_time,Asset1.DynamicState_R_x,Asset1.DynamicState_R_y,Asset1.DynamicState_R_z,Asset1.DynamicState_V_x,Asset1.DynamicState_V_y,Asset1.DynamicState_V_z,\r\n0,3000,4100,3400,0,6.02088,4.215866\r\n";

            Assert.AreEqual(expected, actual);

            Vector actIC = dynamic.InitialConditions();

            List<double> ICList = new List<double>();
            ICList.Add(3000.0);
            ICList.Add(4100.0);
            ICList.Add(3400.0);
            ICList.Add(0.0);
            ICList.Add(6.02088);
            ICList.Add(4.215866);

            Vector expIC = new Vector(ICList);

            Assert.AreEqual(expIC, actIC);

        }
        /// <summary>
        /// Tests the Dynamic State constructor when a static EOM is passed thru input file
        /// </summary>
        [Test]
        public void ConstructDynamicState_StaticEOM() //TODO:whats going on here jack
        {

            helperConst("UnitTestInputs\\UnitTestModel_DynStateStatic.xml");
            
            DynamicEOMS expected = null;
            Assert.AreEqual(expected, dynamic.Eoms);

            string actual = dynamic.ToString();
            string expectedstring = "Asset1.DynamicState_time,Asset1.DynamicState_R_x,Asset1.DynamicState_R_y,Asset1.DynamicState_R_z,Asset1.DynamicState_V_x,Asset1.DynamicState_V_y,Asset1.DynamicState_V_z,\r\n0,3000,4100,3400,0,6.02088,4.215866\r\n";

            Assert.AreEqual(expectedstring, actual);

            Vector actIC = dynamic.InitialConditions();

            List<double> ICList = new List<double>();
            ICList.Add(3000.0);
            ICList.Add(4100.0);
            ICList.Add(3400.0);
            ICList.Add(0.0);
            ICList.Add(6.02088);
            ICList.Add(4.215866);

            Vector expIC = new Vector(ICList);

            Assert.AreEqual(expIC, actIC);
        }
        /// <summary>
        /// Tests the four integrator parameters which can be adjusted in the XML input files
        /// </summary>
        [Test]
        public void ConstructDynamicState_Integrators()
        {
            helperConst("UnitTestInputs\\UnitTestModel_Integrator.xml");
            
            IntegratorOptions actIntOptions = dynamic.getIntegratorOptions();

            IntegratorOptions expIntOptions = new IntegratorOptions();
            expIntOptions.h = 1;
            expIntOptions.rtol = 1;
            expIntOptions.atol = 1;
            expIntOptions.eps = 1;

            Assert.AreEqual(expIntOptions.h, actIntOptions.h);
            Assert.AreEqual(expIntOptions.rtol, actIntOptions.rtol);
            Assert.AreEqual(expIntOptions.atol, actIntOptions.atol);
            Assert.AreEqual(expIntOptions.eps, actIntOptions.eps);

        }
        [Test]
        public void ConstructDynamicState_unusedConst() 
        {
            DynamicStateType DST = new DynamicStateType();
            DST = (DynamicStateType)Enum.Parse(typeof(DynamicStateType), "PREDETERMINED_ECI");


            List<double> ICList = new List<double>();
            ICList.Add(3000.0);
            ICList.Add(4100.0);
            ICList.Add(3400.0);
            ICList.Add(0.0);
            ICList.Add(6.02088);
            ICList.Add(4.215866);
            Vector expIC = new Vector(ICList);

            OrbitalEOMS expectedorb = new OrbitalEOMS();


            DynamicState dynamicState = new DynamicState(DST, expectedorb, expIC);
            string actual = dynamicState.ToString();

            string expected = "Asset1.DynamicState_time,Asset1.DynamicState_R_x,Asset1.DynamicState_R_y,Asset1.DynamicState_R_z,Asset1.DynamicState_V_x,Asset1.DynamicState_V_y,Asset1.DynamicState_V_z,\r\n0,3000,4100,3400,0,6.02088,4.215866\r\n";

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void PosVelECI()
        {
            helperConst("UnitTestInputs\\UnitTestModel_DynState.xml");
            
            List<double> ICposList = new List<double>();
            ICposList.Add(3000.0);
            ICposList.Add(4100.0);
            ICposList.Add(3400.0);
            Vector expIC = new Vector(ICposList);

            Vector actIC = dynamic.PositionECI(0);
            Assert.AreEqual(expIC, actIC);

            List<double> ICvelList = new List<double>();
            ICvelList.Add(0.0);
            ICvelList.Add(6.02088);
            ICvelList.Add(4.215866);
            Vector expvelIC = new Vector(ICvelList);

            Vector actvelIC = dynamic.VelocityECI(0);

            Assert.AreEqual(expvelIC, actvelIC);
        }
        /// <summary>
        /// Tests the PropagateState(double simTime) function, accessed thru DynamicStateECI, checked with vals from Matlab ode45 propagation, tol = .0001
        /// </summary>
        [Test]
        public void VectorTest()
        {
            helperConst("UnitTestInputs\\UnitTestModel_DynState.xml");
            Vector v0 = dynamic.DynamicStateECI(0);
            Vector v1 = dynamic.DynamicStateECI(1);
            Vector v30 = dynamic.DynamicStateECI(30);
            Vector expv0 = new Vector("3000.0;4100.0;3400.0;0.0;6.02088;4.215866");
            Vector expv1 = new Vector("2999.997385487238;4106.017305084396;3404.212901661460;-0.005226299504;6.013732148925331;4.209939190247180");
            Vector expv30 = new Vector("2997.715635427037;4277.459280509288;3523.855415907365;-0.150003458110018;5.811427244838430;4.042747536995044");
            Assert.IsTrue(Vector.AreEqual(v0, expv0, .0001));
            Assert.IsTrue(Vector.AreEqual(v1, expv1, .0001));
            Assert.IsTrue(Vector.AreEqual(v30, expv30,.0001));
        }

        public void helperConst(string modelInput)
        {
            programAct = new Program();
            programAct.ModelInputFilePath = Path.Combine(baselocation, modelInput) ;
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            var modelInputXMLNode = XmlParser.GetModelNode(programAct.ModelInputFilePath);
            var simulationInputNode = XmlParser.ParseSimulationInput(programAct.SimulationInputFilePath);

            Domain SystemUniverse = new SpaceEnvironment();

            asset = new Asset(modelInputXMLNode.FirstChild);
            programAct.AssetList.Add(asset);

            try
            {
                asset.AssetDynamicState.Eoms.SetEnvironment(SystemUniverse);
            }
            catch
            {
                programAct.log.Info("AssetDynamicState.Eoms.SetEnvironment(SystemUniverse) Failed the Unit test");
            }
            dynamic = new DynamicState(modelInputXMLNode.FirstChild.FirstChild);
        }
    }
}
