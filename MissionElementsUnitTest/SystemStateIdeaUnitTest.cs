using System;
using Utilities;
using NUnit.Framework;
using System.Xml;
using UserModel;
using MissionElements;

namespace MissionElementsUnitTest
{
    [TestFixture]
    public class SystemStateIdeaUnitTest
    {
        [Test]
        public void TestAddMethod()
        {
            SystemStateIdea state = new SystemStateIdea();

            string stateVariableName = "intState";

            state.AddValue(stateVariableName, 0.0, 10);
            state.AddValue(stateVariableName, 1.0, 20);

            state.AddValue("newState", 0.0, 15);
        }
    }
}
