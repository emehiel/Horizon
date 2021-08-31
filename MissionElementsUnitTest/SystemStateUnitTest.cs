using MissionElements;
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;
using NUnit.Framework;
using System.Xml;
using UserModel;
using System.IO;

namespace MissionElementsUnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
    public class SystemStateUnitTest
    {
        public SystemState state;
        public HSFProfile<Matrix<double>> matrixProf;
        public HSFProfile<int> intProf;
        public HSFProfile<double> doubleProf;
        public HSFProfile<bool> boolProf;
        public HSFProfile<Quat> quatProf;
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        public StateVarKey<int> intKey;
        StateVarKey<double> doubleKey;
        StateVarKey<Matrix<double>> matrixKey;
        StateVarKey<Quat> quatKey;
        StateVarKey<bool> boolKey;
        [SetUp]
        public void SystemStateCtor()
        {
            state = new SystemState();
            intProf = new HSFProfile<int>(0, 1);
            intProf.Add(1, 2);
            intProf.Add(2, -1);

            intKey = new StateVarKey<int>("asset1.IKey");
            state.SetProfile(intKey, intProf);

            doubleProf = new HSFProfile<double>(0, 1);
            doubleProf.Add(1, 2);
            doubleProf.Add(2, -1);

            doubleKey = new StateVarKey<double>("asset1.maj_Key");
            state.SetProfile(doubleKey, doubleProf);

            boolProf = new HSFProfile<bool>(0, true);
            boolProf.Add(1, false);
            boolProf.Add(2, true);

            boolKey = new StateVarKey<bool>("asset1.BKey");
            state.SetProfile(boolKey, boolProf);

            matrixProf = new HSFProfile<Matrix<double>>(0, new Matrix<double>(1, 2, 1));
            matrixProf.Add(1, new Matrix<double>(1, 2, 2));
            matrixProf.Add(2, new Matrix<double>(1, 2, -1));


            matrixKey = new StateVarKey<Matrix<double>>("asset1.MKey");
            state.SetProfile(matrixKey, matrixProf);


            quatProf = new HSFProfile<Quat>(0, new Quat(0, new Vector(3)));
            List<double> vectList = new List<double>();
            //vectList.Add();
            quatProf.Add(1, new Quat(1, new Vector(3)));
            quatProf.Add(2, new Quat(.5, new Vector(3)));

            quatKey = new StateVarKey<Quat>("asset1.QKey");
            state.SetProfile(quatKey, quatProf);



        }
        /// <summary>
        /// Tests both empty state construction and based on prev state
        /// </summary>
        [Test]
        public void ConstructorSystemState()
        {

            SystemState emptyState = new SystemState();
            Assert.IsInstanceOf(typeof(Dictionary<StateVarKey<int>, HSFProfile<int>>), emptyState.Idata);
            Assert.IsInstanceOf(typeof(Dictionary<StateVarKey<double>, HSFProfile<double>>), emptyState.Ddata);
            Assert.IsInstanceOf(typeof(Dictionary<StateVarKey<bool>, HSFProfile<bool>>), emptyState.Bdata);
            Assert.IsInstanceOf(typeof(Dictionary<StateVarKey<Matrix<double>>, HSFProfile<Matrix<double>>>), emptyState.Mdata);
            Assert.IsInstanceOf(typeof(Dictionary<StateVarKey<Quat>, HSFProfile<Quat>>), emptyState.Qdata);

            SystemState fullState = new SystemState(state);
            Assert.AreEqual(state.ToString(), fullState.Previous.ToString());

        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddUnitTest()
        {
            //arrange
            SystemState emptyState = new SystemState();
            //act
            emptyState.Add(state);
            //assert
            Assert.AreEqual(state.ToString(), emptyState.ToString());
        }
        [Test]
        public void ToStringTest() // doesnt toString anything except for Idata?
        {
            //arrange
            string expected = "asset1.ikey,0,1,1,2,2,-1,";
            //act
            string actual = state.ToString();
            //assert
            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void GetProfileTest()
        {
            //act
            HSFProfile<Quat> newQuatProf = state.GetProfile(quatKey);
            HSFProfile<bool> newboolProf = state.GetProfile(boolKey);
            HSFProfile<int> newIntProf = state.GetProfile(intKey);
            HSFProfile<double> newDoubleProf = state.GetProfile(doubleKey);
            HSFProfile<Matrix<double>> newMatrixProf = state.GetProfile(matrixKey);

            HSFProfile<int> fullIntProf = state.GetFullProfile(intKey);
            HSFProfile<bool> fullboolProf = state.GetFullProfile(boolKey);
            HSFProfile<double> fullDoubleProf = state.GetFullProfile(doubleKey);
            HSFProfile<Matrix<double>> fullMatrixProf = state.GetFullProfile(matrixKey);

            //assert
            Assert.AreEqual(quatProf, newQuatProf);
            Assert.AreEqual(matrixProf, newMatrixProf);
            Assert.AreEqual(doubleProf, newDoubleProf);
            Assert.AreEqual(intProf, newIntProf);
            Assert.AreEqual(boolProf, newboolProf);

            Assert.AreEqual(matrixProf, fullMatrixProf);
            Assert.AreEqual(doubleProf, fullDoubleProf);
            Assert.AreEqual(intProf, fullIntProf);
            Assert.AreEqual(boolProf, fullboolProf);
        }
        [Test]
        public void setInitialConditions()
        {
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSubICs.xml");
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            XmlNode simNode = XmlParser.ParseSimulationInput(simulationInputFilePath);

            List<XmlNode> ICNodes = new List<XmlNode>();
            XmlNode modelNode = XmlParser.GetModelNode(modelInputFilePath);


            ICNodes.Add(modelNode.FirstChild.ChildNodes[1].FirstChild);
            ICNodes.Add(modelNode.FirstChild.ChildNodes[1].ChildNodes[1]);
            ICNodes.Add(modelNode.FirstChild.ChildNodes[1].ChildNodes[2]);
            ICNodes.Add(modelNode.FirstChild.ChildNodes[1].ChildNodes[3]);
            ICNodes.Add(modelNode.FirstChild.ChildNodes[1].ChildNodes[4]);
            Asset asset = new Asset(modelNode.FirstChild);
            SystemState systemState = SystemState.setInitialSystemState(ICNodes, asset);

            HSFProfile<Quat> newQuatProf = systemState.GetProfile(quatKey);
            Quat quatVal = newQuatProf.FirstValue();
            HSFProfile<bool> newboolProf = systemState.GetProfile(boolKey);
            HSFProfile<int> newIntProf = systemState.GetProfile(intKey);
            HSFProfile<double> newDoubleProf = systemState.GetProfile(doubleKey);
            HSFProfile<Matrix<double>> newMatrixProf = systemState.GetProfile(matrixKey);



            Quat expQuat = new Quat("[1.0, 0.5, 0.5, 0.4]");
            bool expBool = true;
            int expInt = 2;
            Matrix<double> expMat = new Matrix<double>("[1,2;3,4]");
            double expDouble = 1.0;

            Assert.IsTrue(expQuat.Equals(newQuatProf.FirstValue()));
            Assert.AreEqual(expBool, newboolProf.FirstValue());
            Assert.AreEqual(expInt, newIntProf.FirstValue());
            Assert.AreEqual(expMat, newMatrixProf.FirstValue());
            Assert.AreEqual(expDouble, newDoubleProf.FirstValue());
        }
        [Test]
        public void getLastValueTests()
        {
            //Arrange
            double expLastDoubleVal = -1;
            KeyValuePair<double, double> doubleKVP = new KeyValuePair<double, double>(2,expLastDoubleVal);
            int expLastIntVal = -1;
            KeyValuePair<double, int> intKVP = new KeyValuePair<double, int>(2, expLastIntVal);
            Matrix<double> expLastMatrixVal = new Matrix<double>(1, 2, -1);
            KeyValuePair<double, Matrix<double>> matrixKVP = new KeyValuePair<double,Matrix<double>>(2, expLastMatrixVal);
            bool expLastBoolVal = true;
            KeyValuePair<double, bool> boolKVP = new KeyValuePair<double, bool>(2, expLastBoolVal);

            //Act
            KeyValuePair<double, double> lastDoubleVal = state.GetLastValue(doubleKey);
            KeyValuePair<double, int> lastintVal = state.GetLastValue(intKey);
            KeyValuePair<double, bool> lastBoolVal = state.GetLastValue(boolKey);
            KeyValuePair<double, Matrix<double>> lastMatVal = state.GetLastValue(matrixKey);

            //Assert
            Assert.AreEqual(doubleKVP, lastDoubleVal);
            Assert.AreEqual(boolKVP, lastBoolVal);
            Assert.AreEqual(intKVP, lastintVal);
            Assert.AreEqual(matrixKVP, lastMatVal);

        }
        [Test]
        public void getValueAtTimeTests()
        {
            //Arrange
            double expLastDoubleVal = -1;
            KeyValuePair<double, double> doubleKVP = new KeyValuePair<double, double>(2, expLastDoubleVal);
            int expLastIntVal = -1;
            KeyValuePair<double, int> intKVP = new KeyValuePair<double, int>(2, expLastIntVal);
            Matrix<double> expLastMatrixVal = new Matrix<double>(1, 2, -1);
            KeyValuePair<double, Matrix<double>> matrixKVP = new KeyValuePair<double, Matrix<double>>(2, expLastMatrixVal);
            bool expLastBoolVal = true;
            KeyValuePair<double, bool> boolKVP = new KeyValuePair<double, bool>(2, expLastBoolVal);

            //Act
            KeyValuePair<double, double> lastDoubleVal = state.GetValueAtTime(doubleKey,2);
            KeyValuePair<double, int> lastintVal = state.GetValueAtTime(intKey, 2);
            KeyValuePair<double, bool> lastBoolVal = state.GetValueAtTime(boolKey, 2);
            KeyValuePair<double, Matrix<double>> lastMatVal = state.GetValueAtTime(matrixKey, 2);

            //Assert
            Assert.AreEqual(doubleKVP, lastDoubleVal);
            Assert.AreEqual(boolKVP, lastBoolVal);
            Assert.AreEqual(intKVP, lastintVal);
            Assert.AreEqual(matrixKVP, lastMatVal);

        }
        [Test]
        public void getProfile_datacountzero()
        {
            //arrange
            SystemState newState = new SystemState(state);
            //act
            HSFProfile<Quat> newQuatProf = newState.GetProfile(quatKey);
            HSFProfile<bool> newboolProf = newState.GetProfile(boolKey);
            HSFProfile<int> newIntProf = newState.GetProfile(intKey);
            HSFProfile<double> newDoubleProf = newState.GetProfile(doubleKey);
            HSFProfile<Matrix<double>> newMatrixProf = newState.GetProfile(matrixKey);

            HSFProfile<int> fullIntProf = newState.GetFullProfile(intKey);
            HSFProfile<bool> fullboolProf = newState.GetFullProfile(boolKey);
            HSFProfile<double> fullDoubleProf = newState.GetFullProfile(doubleKey);
            HSFProfile<Matrix<double>> fullMatrixProf = newState.GetFullProfile(matrixKey);

            //assert
            Assert.AreEqual(quatProf, newQuatProf);
            Assert.AreEqual(matrixProf, newMatrixProf);
            Assert.AreEqual(doubleProf, newDoubleProf);
            Assert.AreEqual(intProf, newIntProf);
            Assert.AreEqual(boolProf, newboolProf);

            Assert.AreEqual(matrixProf, fullMatrixProf);
            Assert.AreEqual(doubleProf, fullDoubleProf);
            Assert.AreEqual(intProf, fullIntProf);
            Assert.AreEqual(boolProf, fullboolProf);

        }
    }
}
