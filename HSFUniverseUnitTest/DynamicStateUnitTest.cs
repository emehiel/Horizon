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
            StateSpaceEOMS msd = new StateSpaceEOMS();
            var initialConditions = new Vector(6);
            DynamicState dynamicState = new DynamicState(DynamicStateType.PREDETERMINED_ECI, msd, initialConditions);
        }
    }
}
