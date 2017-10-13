using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace UtilitiesUnitTest
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestMethod]
        public void QuaternionMultiply()
        {
            Quat q = new Quat(.5, .707, .707, .707);
            Quat p = new Quat(.8, new Vector(new List<double>(new double[] { .1, .1, .1 })));
            Quat result = q * p;
            Quat expected = new Quat(0.1879, 0.6156, 0.6156, 0.6156);
            Assert.AreEqual(expected._eta, result._eta, .0001);
            Assert.AreEqual(expected._eps[1], result._eps[1]);
            Assert.AreEqual(expected._eps[2], result._eps[2]);
            Assert.AreEqual(expected._eps[3], result._eps[3]);
        }
        [TestMethod]
        public void IntegratorTest()
        {
            StateSpaceEOMS dynamics = new StateSpaceEOMS();
            Matrix<double> tspan = new Matrix<double>(new double[1, 2] { { 0, 20 } });
            Matrix<double> y0 = new Matrix<double>(new double[2, 1] { { 1 }, { 0 } });
            Matrix<double> result = Integrator.RK45(dynamics, tspan, y0);

            System.IO.File.WriteAllText("integratorOut.txt", result.ToString());

            //Console.ReadLine(); //FIXME: Why is this here?
        }
        [TestCategory("Matrix")]

        [TestMethod]
        public void MatrixExponent()
        {
            // Using matrix described here to test http://blogs.mathworks.com/cleve/2012/07/23/a-balancing-act-for-the-matrix-exponential/
            double a = 2 * Math.Pow(10, 10);
            double b = 4 * Math.Pow(10, 8) / 6;
            double c = 200 / 3;
            double d = 3;
            double e = 1 * Math.Pow(10, -8);
            Matrix<double> A = new Matrix<double>(new double[,] { { 0, e, 0 }, { -(a + b), -d, a }, { c, 0, -c } });

            Matrix<double> Aexp = Matrix<double>.exp(A);
        }
        [TestMethod]
        public void MatrixInverse()
        {
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 0, 1 }, { 1, 2, 0 }, { 1, 5, 1 } });
            Matrix<double> result = Matrix<double>.Inverse(A);

            Matrix<double> expected = new Matrix<double>(new double[,] { { .4, 1, -.4 }, { -.2, 0, .2 }, { .6, -1, .4 } });

            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        public void MatrixVertCatTest()
        {
            // Create Test Matricies.
            Matrix<double> A = new Matrix<double>(2, 3, 1);
            Matrix<double> B = new Matrix<double>(2, 3, 2);

            // Create Correct Answer.
            Matrix<double> C = new Matrix<double>(new double[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 2, 2, 2 }, { 2, 2, 2 } });

            // Test Method.
            Matrix<double> D = Matrix<double>.Vertcat(A, B);

            // Verify Result.
            Assert.AreEqual(C, D);
        }

        [TestMethod]
        public void MatrixHorizCatTest()
        {
            // Create Test Matrices.
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 1, 1 }, { 1, 1, 1 } });
            Matrix<double> B = new Matrix<double>(new double[,] { { 2, 2, 2 }, { 2, 2, 2 } });

            // Create correct answer.
            Matrix<double> C = new Matrix<double>(new double[,] { { 1, 1, 1, 2, 2, 2 }, { 1, 1, 1, 2, 2, 2 } });

            // Test Method.
            Matrix<double> D = Matrix<double>.Horzcat(A, B);

            // Verify Result  .       
            Assert.AreEqual(C, D);
        }

        [TestMethod]
        public void MatrixCumProdTest()
        {
            // Create Test Matrix.
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });

            // Create Correct Answer.
            Matrix<double> C = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 10, 18 }, { 28, 80, 162 } });

            //Test Method.
            Matrix<double> B = Matrix<double>.Cumprod(A);

            //Verify Result.
            Assert.AreEqual(C, B);
        }
        [TestMethod]
        public void MatrixMultiply()
        {
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });
            Matrix<double> B = new Matrix<double>(new double[,] { { 1, 1, 3 }, { 4, 2, 6 }, { 7, 3, 9 } });

            Matrix<double> C = A * B;

            Matrix<double> D = new Matrix<double>(new double[,] { { 30, 14, 42 }, { 66, 32, 96 }, { 102, 50, 150 } });

            Assert.AreEqual(D, C);
        }

        [TestMethod]
        public void MatrixNormTest()
        {
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 } });
            double normActual = Matrix<double>.Norm(A);
            double normExpected = 3.7417;
            Assert.AreEqual(normExpected, normActual, 0.0001);
        }


        [TestMethod]
        public void MatrixSetColumnTest()
        {
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });
            A.SetColumn(2, new Matrix<double>(new double[,] { { 1 }, { 2 }, { 3 } }));
            Matrix<double> B = new Matrix<double>(new double[,] { { 1, 1, 3 }, { 4, 2, 6 }, { 7, 3, 9 } });

            Assert.AreEqual(B, A);
        }
        [TestMethod]
        public void MatrixSetRowTest()
        {
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });
            A.SetRow(2, new Matrix<double>(new double[,] { { 1, 2, 3 } }));
            Matrix<double> B = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 1, 2, 3 }, { 7, 8, 9 } });

            Assert.AreEqual(B, A);
        }
        [TestMethod]
        public void MatrixDeepCopyTest()
        {
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });

            Matrix<double> B = DeepCopy.Copy(A);

            Assert.AreEqual(B, A);
        }


        [TestMethod]
        public void MatrixToArrayTest()
        {
            // Create Test Matrices.
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });

            // Create correct answer.
            double[,] B = new double[3, 3] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };

            // Test Method.
            double[,] C = A.ToArray();

            // Verify Result.
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.AreEqual(B.GetValue(i, j), C.GetValue(i, j));
                }
            }
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
            //Test copy
            TestCopyClass c2 = DeepCopy.Copy<TestCopyClass>(c1);
            Assert.AreEqual(c1.Helper.helperInt, c2.Helper.helperInt);
            Assert.AreEqual(c1.Num, c2.Num);
            //Test Modify to property
            c2.Num = 5;
            Assert.AreNotEqual(c1.Num, c2.Num);
            Assert.AreEqual(5, c2.Num);
            Assert.AreEqual(1, c1.Num);
            //Test Modification to Helper class
            c2.Helper.helperInt = 12;
            Assert.AreNotEqual(c1.Helper.helperInt, c2.Helper.helperInt);
            Assert.AreEqual(12, c2.Helper.helperInt);
            c2.Helper.helperString = "poop";
            Assert.AreNotEqual(c1.Helper.helperString, c2.Helper.helperString);
            Assert.AreEqual("poop", c2.Helper.helperString);
            //Test clone constructor
            TestCopyClass c3 = new TestCopyClass(c1);
            Assert.AreEqual(c1.Num, c3.Num);
            Assert.AreEqual(c1.Helper.helperString, c3.Helper.helperString);
            c3.Num = 7;
            c3.Helper.helperInt = 7;
            Assert.AreNotEqual(c1.Helper.helperInt, c3.Helper.helperInt);
            Assert.AreEqual(7, c3.Helper.helperInt);
            Assert.AreNotEqual(c1.Num, c3.Num);
        }

    }
    [Serializable]
    public class TestCopyClass
    {
        public int Num;
        public string Name;
        public TestCopyHelperClass Helper;
        public TestCopyClass() { }
        public TestCopyClass(TestCopyClass other)
        {
            TestCopyClass copy = DeepCopy.Copy<TestCopyClass>(other);
            Num = copy.Num;
            Name = copy.Name;
            Helper = copy.Helper;
        }
    }
    [Serializable]
    public class TestCopyHelperClass
    {
        public string helperString;
        public int helperInt;
    }

}
