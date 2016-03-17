using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Universe;
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
            DynamicState dynamicState = new DynamicState(DynamicStateType.PREDETERMINED_ECI, msd);
        }
    }
}
