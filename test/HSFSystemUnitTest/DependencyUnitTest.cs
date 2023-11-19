using HSFSystem;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace HSFSystemUnitTest
{
    [TestFixture]
    public class DepenedencyUnitTest
    {
        /// <summary>
        /// constructs a dependency using Dependency.Instance 
        /// then tests that DependencyFunctions was constructed by asking GetDependencyFunc to return KeyNotFoundException
        /// If any other error is thrown, the tests fail
        /// Dependency instance is static so D2 and D3 must refer to the same object
        /// </summary>
        [Test]
        public void DependencyConstructorUnitTest() //not sure how else to test this construcrtor
        {
            //first make sure it constructs
            try
            {
                //arrange + act
                Dependency D1 = Dependency.Instance;
                //assert
                Assert.IsInstanceOf(typeof(Dependency), D1);
            }
            catch
            {
                Assert.Fail();
            }
            //then make sure that DependencyFunctions is created but doesnt have any keys
            try
            {
                //arrange + act
                Dependency D1 = Dependency.Instance;
                Delegate elegate = D1.GetDependencyFunc(" "); //since DependencyFunctions is private cannot access DependencyFunctions directly
            }   //assert
            catch (KeyNotFoundException _)
            {
                
                Assert.Pass();
            }
            catch
            {
                Assert.Fail();
            }

            //arrange + act
            Dependency D2 = Dependency.Instance;
            Dependency D3 = Dependency.Instance;
            //assert
            Assert.AreSame(D2, D3);
        }
        /// <summary>
        /// First tests Dependency.Add with a function
        /// then tests the overwrite if a different function is added with same callKey
        /// </summary>
        [Test]
        public void AddUnitTest()
        {
            //arrange
            Dependency D1 = Dependency.Instance;
            Delegate delExp = new Func<double, double>(func1);

            //act
            D1.Add("depfun",delExp);
            Delegate delAct = D1.GetDependencyFunc("depfun");

            Delegate delExp2 = new Func<double, double>(func2);
            D1.Add("depfun", delExp2);
            Delegate delAct2 = D1.GetDependencyFunc("depfun");

            //assert
            Assert.AreEqual(delExp, delAct);
            Assert.AreEqual(delExp2, delAct2);

        }

        [Test]
        public void AppendUnitTest()
        {
            //arrange
            Delegate delExp1 = new Func<double, double>(func1);
            Delegate delExp2 = new Func<double, double>(func2);

            Dictionary<string, Delegate> newDependencies = new Dictionary<string, Delegate>();
            newDependencies.Add("func1", delExp1);
            newDependencies.Add("func2", delExp2);

            Dependency D1 = Dependency.Instance;

            //act
            D1.Append(newDependencies);

            Delegate delAct1 = D1.GetDependencyFunc("func1");
            Delegate delAct2 = D1.GetDependencyFunc("func2");

            //assert
            Assert.AreEqual(delExp1, delAct1);
            Assert.AreEqual(delExp2, delAct2);

        }

        public double func1(double d)
        {
            return d * 5;
        }
        public double func2(double d)
        {
            return d * 10;
        }
    }
}
