using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HSFUniverse;
using Utilities;

namespace UniverseUnitTest
{
    [TestClass]
    public class DynamicStateUnitTest
    {
        [TestMethod]
        public void PositionECIUnitTest()
        {
            StateSpace_EOMS msd = new StateSpace_EOMS();
            var initialConditions = new Matrix<double>();
            DynamicState dynamicState = new DynamicState(DynamicStateType.PREDETERMINED_ECI, msd, initialConditions);
        }
    }
}
