using HSFSystem;
using System;
using System.Collections.Generic;
using System.Text;
using HSFScheduler;
using HSFSystem;
using NUnit.Framework;

namespace HSFSchedulerUnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
    public class TargetValueEval
    {

        [Test]
        public void TestMethod1()
        {
            Dependency dep = Dependency.Instance;
            Evaluator TVE = new TargetValueEvaluator(dep);
            //double sum = TargetValueEvaluate()
        }
    }
}
