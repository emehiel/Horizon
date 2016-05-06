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
        public void TemplatedGetProfileUnitTest()
        {
            SystemState state = new SystemState();
            HSFProfile<int> intProf = new HSFProfile<int>(0, 1);
            intProf.Add(1, 2);
            intProf.Add(2, -1);

            StateVarKey<int> intKey = new StateVarKey<int>("testIntVar");
            state.setProfile(intKey, intProf);

            HSFProfile<double> doubleProf = new HSFProfile<double>(0, 1);
            doubleProf.Add(1, 2);
            doubleProf.Add(2, -1);

            StateVarKey<double> doubleKey = new StateVarKey<double>("testDoubleVar");
            state.setProfile(doubleKey, doubleProf);

            HSFProfile<int> newIntProf = state.getProfile(intKey);
            HSFProfile<double> newDoubleProf = state.getProfile(doubleKey);

            Console.WriteLine();
        }
    }
}
