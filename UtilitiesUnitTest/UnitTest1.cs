using System; 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;


namespace UtilitiesUnitTest
{ 
   [TestClass] 
   public class UnitTest1
    { 
        [TestMethod] 
        public void TestMethod1()
        { 
            string arrayData = "[1, 2.3; 0, -2.1]"; 
            Matrix M = new Matrix(arrayData); 


        }
        [TestMethod]
        public void DeepCopyTest()
        {
            TestCopyClass c1 = new TestCopyClass
            {
                Num = 1,
                Name = "Morgan",
                Helper = new TestCopyHelperClass
                {
                    helperString = "Help",
                    helperInt = 2
                }
            };
            TestCopyClass c2 = DeepCopy.Copy<TestCopyClass>(c1);
            Assert.AreEqual(c1.Helper.helperInt, c2.Helper.helperInt);
            Assert.AreEqual(c1.Num, c2.Num);
            c2.Num = 5;
            Assert.AreNotEqual(c1.Num, c2.Num);
            Assert.AreEqual(5, c2.Num);
            Assert.AreEqual(1, c1.Num);
            c2.Helper.helperInt = 12;
            Assert.AreNotEqual(c1.Helper.helperInt, c2.Helper.helperInt);
            Assert.AreEqual(12, c2.Helper.helperInt);
            c2.Helper.helperString = "poop";
            Assert.AreNotEqual(c1.Helper.helperString, c2.Helper.helperString);
            Assert.AreEqual("poop", c2.Helper.helperString);
        }
    }
    [Serializable]
    public class TestCopyClass
    {
        public int Num;
        public string Name;
        public TestCopyHelperClass Helper;
    }
    [Serializable]
    public class TestCopyHelperClass
    {
        public string helperString;
        public int helperInt;
    }

} 
  
