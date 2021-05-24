using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionElements;
using Utilities;

namespace MissionElementsUnitTest
{
    [TestClass]
    public class SystemStateUnitTest
    {
        [TestMethod]
        public void TemplatedGetProfile()
        {
            SystemState state = new SystemState();
            HSFProfile<int> intProf = new HSFProfile<int>(0, 1);
            intProf.Add(1, 2);
            intProf.Add(2, -1);

            StateVarKey<int> intKey = new StateVarKey<int>("testIntVar");
            state.SetProfile(intKey, intProf);

            HSFProfile<double> doubleProf = new HSFProfile<double>(0, 1);
            doubleProf.Add(1, 2);
            doubleProf.Add(2, -1);

            StateVarKey<double> doubleKey = new StateVarKey<double>("testDoubleVar");
            state.SetProfile(doubleKey, doubleProf);

            HSFProfile<Matrix<double>> matrixProf = new HSFProfile<Matrix<double>>(0, new Matrix<double>(1, 2, 1));
            matrixProf.Add(1, new Matrix<double>(1, 2, 2));
            matrixProf.Add(2, new Matrix<double>(1, 2, -1));

            var matrixKey = new StateVarKey<Matrix<double>>("testMatrixVar");
            state.SetProfile(matrixKey, matrixProf);

            HSFProfile<int> newIntProf = state.GetProfile(intKey);
            HSFProfile<double> newDoubleProf = state.GetProfile(doubleKey);
            HSFProfile<Matrix<double>> newMatrixProf = state.GetProfile(matrixKey);

            Console.WriteLine();
        }
        public void GetLastValue()
        {
            Assert.Inconclusive();
        }
        public void GetValueAtTime()
        {
            Assert.Inconclusive();
        }
        public void GetFullProfile()
        {
            Assert.Inconclusive();
        }
        public void SetProfile()
        {
            Assert.Inconclusive();
        }
        public void AddValue()
        {
            Assert.Inconclusive();
        }
    }
}
