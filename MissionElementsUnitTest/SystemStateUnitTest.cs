using MissionElements;
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;
using NUnit.Framework;

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

        public void SystemStateCtor()
        {
            state = new SystemState();
            intProf = new HSFProfile<int>(0, 1);
            intProf.Add(1, 2);
            intProf.Add(2, -1);

            StateVarKey<int> intKey = new StateVarKey<int>("testIntVar");
            state.SetProfile(intKey, intProf);

            doubleProf = new HSFProfile<double>(0, 1);
            doubleProf.Add(1, 2);
            doubleProf.Add(2, -1);

            StateVarKey<double> doubleKey = new StateVarKey<double>("testDoubleVar");
            state.SetProfile(doubleKey, doubleProf);

            boolProf = new HSFProfile<bool>(0, true);
            boolProf.Add(1, false);
            boolProf.Add(2, true);

            StateVarKey<bool> boolKey = new StateVarKey<bool>("testboolVar");
            state.SetProfile(boolKey, boolProf);

            matrixProf = new HSFProfile<Matrix<double>>(0, new Matrix<double>(1, 2, 1));
            matrixProf.Add(1, new Matrix<double>(1, 2, 2));
            matrixProf.Add(2, new Matrix<double>(1, 2, -1));

            var matrixKey = new StateVarKey<Matrix<double>>("testMatrixVar");
            state.SetProfile(matrixKey, matrixProf);


            quatProf = new HSFProfile<Quat>(0, new Quat(0, new Vector(3)));
            List<double> vectList = new List<double>();
            //vectList.Add();
            quatProf.Add(1, new Quat(1, new Vector(3)));
            quatProf.Add(2, new Quat(.5, new Vector(3)));

            var quatKey = new StateVarKey<Quat>("testQuatVar");
            state.SetProfile(quatKey, quatProf);

        }
        /// <summary>
        /// Tests both empty state construction and based on prev state
        /// </summary>
        [Test]
        public void ConstructorSystemState()
        {
            SystemStateCtor();

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
            SystemStateCtor();
            SystemState emptyState = new SystemState();
            emptyState.Add(state);
            Assert.AreEqual(state.ToString(), emptyState.ToString());
        }
        [Test]
        public void ToStringTest() // doesnt toString anything except for Idata?
        {
            SystemStateCtor();
            string expected = "testintvar,0,1,1,2,2,-1,";
            string actual = state.ToString();
            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void GetProfileTest()
        {
            SystemStateCtor();
            StateVarKey<int> intKey = new StateVarKey<int>("testIntVar");
            StateVarKey<double> doubleKey = new StateVarKey<double>("testDoubleVar");
            var matrixKey = new StateVarKey<Matrix<double>>("testMatrixVar");
            var quatKey = new StateVarKey<Quat>("testQuatVar");
            var boolKey = new StateVarKey<bool>("testBoolVar");

            HSFProfile<Quat> newQuatProf = state.GetProfile(quatKey);
            HSFProfile<bool> newboolProf = state.GetProfile(boolKey);
            HSFProfile<int> newIntProf = state.GetProfile(intKey);
            HSFProfile<double> newDoubleProf = state.GetProfile(doubleKey);
            HSFProfile<Matrix<double>> newMatrixProf = state.GetProfile(matrixKey);

            Assert.AreEqual(quatProf, newQuatProf);
            Assert.AreEqual(matrixProf, newMatrixProf);
            Assert.AreEqual(doubleProf, newDoubleProf);
            Assert.AreEqual(intProf, newIntProf);
            Assert.AreEqual(quatProf, newQuatProf);

        }
    }
}
