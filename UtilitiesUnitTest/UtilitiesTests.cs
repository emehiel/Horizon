using System;
using NUnit.Framework;
using Utilities;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace UtilitiesUnitTest
{
    [TestFixture]
    public class UtilitiesTests
    {
        [Test]
        public void QuaternionMultiply()
        {
            Quaternion q = new Quaternion(.5, .707, .707, .707);
            Quaternion p = new Quaternion(.8, new Vector(new List<double>(new double[] { .1, .1, .1 })));
            Quaternion result = q * p;
            Quaternion expected = new Quaternion(0.1879, 0.6156, 0.6156, 0.6156);
            Assert.AreEqual(expected._eta, result._eta, .0001);
            Assert.AreEqual(expected._eps[1], result._eps[1]);
            Assert.AreEqual(expected._eps[2], result._eps[2]);
            Assert.AreEqual(expected._eps[3], result._eps[3]);
        }
        [Test]
        public void IntegratorTest() // TODO check ntrp by checking vals at t=10;
        {

            //arange
            StateSpaceEOMS dynamics = new StateSpaceEOMS();
            Matrix<double> tspan = new Matrix<double>(new double[1, 2] { { 0, 20 } });
            Matrix<double> y0 = new Matrix<double>(new double[2, 1] { { 1 }, { 0 } });

            double y_final = 3.29998029143281E-05;
            double yDot_final = -7.94520751742942E-05;
            double t_final = 20;

            //act
            Matrix<double> result = Integrator.RK45(dynamics, tspan, y0);

            System.IO.File.WriteAllText("integratorOut.txt", result.ToString());

            //assert
            Assert.AreEqual(t_final, result[1, result.Size[2]], .0001);

            Assert.AreEqual(y_final, result[2, result.Size[2]], .0001);

            Assert.AreEqual(yDot_final, result[3, result.Size[2]],.0001);
        }


        [Test]
        public void MatrixExponent()//is this an edge case?
        {
            // Using matrix described here to test http://blogs.mathworks.com/cleve/2012/07/23/a-balancing-act-for-the-matrix-exponential/
            //arrange
            double a = 2 * Math.Pow(10, 10);
            double b = 4 * Math.Pow(10, 8) / 6;
            double c = 200 / 3;
            double d = 3;
            double e = 1 * Math.Pow(10, -8);
            Matrix<double> A = new Matrix<double>(new double[,] { { 0, e, 0 }, { -(a + b), -d, a }, { c, 0, -c } });

            Matrix<double> Aexp = Matrix<double>.exp(A);
        }
        [Test]
        public void MatrixInverse()
        {
            //arrange
            Matrix<double> A = new Matrix<double>(new double[,] { { 3, 0, 2 }, { 2, 0, -2 }, { 0, 1, 1 } });
            Matrix<double> expected = new Matrix<double>(new double[,] { { .2, .2, 0 }, { -.2, .3, 1 }, { .2, -.3, 0 } });

            //act
            Matrix<double> result = Matrix<double>.Inverse(A);

            //assert
            Assert.AreEqual(expected, result);
        }
        [Test]
        public void MatrixVertCatTest()
        {
            //arrange
            // Create Test Matricies.
            Matrix<double> A = new Matrix<double>(2, 3, 1);
            Matrix<double> B = new Matrix<double>(2, 3, 2);
            // Create Correct Answer.
            Matrix<double> C = new Matrix<double>(new double[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 2, 2, 2 }, { 2, 2, 2 } });

            //act
            Matrix<double> D = Matrix<double>.Vertcat(A, B);

            //assert
            Assert.AreEqual(C, D);
        }

        [Test]
        public void MatrixHorizCatTest()
        {
            //arrange
            // Create Test Matrices.
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 1, 1 }, { 1, 1, 1 } });
            Matrix<double> B = new Matrix<double>(new double[,] { { 2, 2, 2 }, { 2, 2, 2 } });

            // Create correct answer.
            Matrix<double> C = new Matrix<double>(new double[,] { { 1, 1, 1, 2, 2, 2 }, { 1, 1, 1, 2, 2, 2 } });

            //act
            // Test Method.
            Matrix<double> D = Matrix<double>.Horzcat(A, B);

            //assert
            // Verify Result  .       
            Assert.AreEqual(C, D);
        }

        [Test]
        public void MatrixCumProdTest()
        {
            //arrange
            // Create Test Matrix.
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });

            // Create Correct Answer.
            Matrix<double> C = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 10, 18 }, { 28, 80, 162 } });

            //act
            //Test Method.
            Matrix<double> B = Matrix<double>.Cumprod(A);

            //assert
            //Verify Result.
            Assert.AreEqual(C, B);
        }
        [Test]
        public void MatrixMultiply()
        {
            //arrange
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });
            Matrix<double> B = new Matrix<double>(new double[,] { { 1, 1, 3 }, { 4, 2, 6 }, { 7, 3, 9 } });
            Matrix<double> D = new Matrix<double>(new double[,] { { 30, 14, 42 }, { 66, 32, 96 }, { 102, 50, 150 } });

            //act
            Matrix<double> C = A * B;

            //assert
            Assert.AreEqual(D, C);
        }

        [Test]
        public void MatrixNormTest()
        {
            //arrange
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 } });
            double normExpected = 3.7417;

            //act
            double normActual = Matrix<double>.Norm(A);

            //assert
            Assert.AreEqual(normExpected, normActual, 0.0001);
        }


        [Test]
        public void MatrixSetColumnTest()
        {
            //arrange
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });
            Matrix<double> B = new Matrix<double>(new double[,] { { 1, 1, 3 }, { 4, 2, 6 }, { 7, 3, 9 } });

            //act
            A.SetColumn(2, new Matrix<double>(new double[,] { { 1 }, { 2 }, { 3 } }));

            //assert
            Assert.AreEqual(B, A);
        }
        [Test]
        public void MatrixSetRowTest()
        {
            //arrange
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });
            Matrix<double> B = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 1, 2, 3 }, { 7, 8, 9 } });

            //act
            A.SetRow(2, new Matrix<double>(new double[,] { { 1, 2, 3 } }));

            //assert
            Assert.AreEqual(B, A);
        }
        [Test]
        public void MatrixDeepCopyTest()
        {
            //arrange
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });

            //act
            Matrix<double> B = DeepCopy.Copy(A);

            //assert
            Assert.AreEqual(B, A);
        }


        [Test]
        public void MatrixToArrayTest()
        {
            //arrange
            // Create Test Matrices.
            Matrix<double> A = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });

            // Create correct answer.
            double[,] B = new double[3, 3] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };

            //act
            double[,] C = A.ToArray();

            //assert
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.AreEqual(B.GetValue(i, j), C.GetValue(i, j));
                }
            }
        }
        [Test]
        public void DeepCopyTest()
        {
            //arrange
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
            c2.Helper.helperString = "pingpong";
            Assert.AreNotEqual(c1.Helper.helperString, c2.Helper.helperString);
            Assert.AreEqual("pingpong", c2.Helper.helperString);
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
        [Test]
        public void CollectionExtensionsUnitTest()
        {
            Assert.Inconclusive();

        }
        [Test]
        public void UpperLowerLimitIntegrateToProfUnitTest()
        {
            Assert.Inconclusive();
        }
        [Test]
        public void VectorTest()
        {
            Assert.Inconclusive();
        }
        [Test]
        public void Matrix_Null_elements_constructor()
        {
            //arrange
            var noElements = new double[0,0];
            //act
            Matrix<double> mat = new Matrix<double>(noElements);
            //assert
            Assert.AreEqual(0, mat.NumCols);
            Assert.AreEqual(0, mat.NumRows);
            Assert.AreEqual(0, mat.NumElements);
            Assert.IsTrue(mat.IsNull());
            Assert.IsTrue(mat.IsSquare());

        }
        [Test]
        public void Matrix_Trace_tests()
        {
            //arrange 
            var elements = new double[2, 2] { { 1, 0 } ,{ 0, 1 } };
            var noncompatable_elements = new double[1,2] { { 1, 0 } }; 
            Matrix<double> mat1 = new Matrix<double>(elements);
            Matrix<double> mat2 = new Matrix<double>(noncompatable_elements);
            //act
            double trace1 = Matrix<double>.Trace(mat1);
            try { double trace2 = Matrix<double>.Trace(mat2); }
            //assert
            catch (ArgumentException)
            {Assert.Pass();}
            Assert.AreEqual(2, trace1);
        }
        [Test]
        public void Matrix_MaxTests()
        {
            //arrange 
            var elements = new double[2, 2] { { 2, 0 }, { 0, 1 } };
            var noncompatable_elements = new double[1, 2] { { 1, 0 } };
            Matrix<double> mat1 = new Matrix<double>(elements);
            Matrix<double> mat2 = new Matrix<double>(noncompatable_elements);
            Matrix<double> expmax1 = new Matrix<double>(new double[1, 2] { { 2, 1 } });
            Matrix<double> expmax2 = new Matrix<double>(new double[1, 1] { { 1 } });
            //act
            Matrix<double> max1 = Matrix<double>.Max(mat1);
            Matrix<double> max2 = Matrix<double>.Max(mat2);
            //assert
            Assert.AreEqual(expmax1, max1);
            Assert.AreEqual(expmax2, max2);

        }
        [Test]
        public void Matrix_MinTests()
        {
            //arrange 
            var elements = new double[2, 2] { { 2, 0 }, { 0, 1 } };
            var noncompatable_elements = new double[1, 2] { { 1, -1 } };
            Matrix<double> mat1 = new Matrix<double>(elements);
            Matrix<double> mat2 = new Matrix<double>(noncompatable_elements);
            Matrix<double> expmin1 = new Matrix<double>(new double[1, 2] { { 0, 0 } });
            Matrix<double> expmin2 = new Matrix<double>(new double[1, 1] { { -1 } });
            //act
            Matrix<double> max1 = Matrix<double>.Min(mat1);
            Matrix<double> max2 = Matrix<double>.Min(mat2);
            //assert
            Assert.AreEqual(expmin1, max1);
            Assert.AreEqual(expmin2, max2);
        }
        [Test]
        public void Matrix_indexOutOfRange()
        {
            //arrange
            Matrix<double> expmin1 = new Matrix<double>(new double[1, 2] { { 0, 0 } });

            //act + assert
            try
            {expmin1[-1, 1] = 1;}
            catch(IndexOutOfRangeException)
            {Assert.Pass();}
            catch
            { Assert.Fail();}

            try
            {double outOfBoundIndex = expmin1[-1, 1];}
            catch (IndexOutOfRangeException)
            {Assert.Pass();}
            catch
            { Assert.Fail();}

            try
            {double outOfBoundIndex = expmin1[3, 1];}
            catch (IndexOutOfRangeException)
            {Assert.Pass();}
            catch
            {Assert.Fail();}
        }
        [Test]
        public void Matrix_IsEqualTest()
        {
            //arrange
            var elements = new double[2, 2] { { 2, 0 }, { 0, 1 } };
            var elements2 = new double[2, 2] { { 2, 0 }, { 0, 0 } };
            var elements3 = new double[1, 2] { { 2, 0 } };
            Matrix<double> matA = new Matrix<double>(elements);
            Matrix<double> matB = new Matrix<double>(elements);
            Matrix<double> matC = new Matrix<double>(elements2);
            Matrix<double> matD = new Matrix<double>(elements3);

            //act
            bool AtoB = (matA == matB);
            bool AtoC = (matA == matC);
            bool AtoD = (matA == matD);

            //assert
            Assert.IsTrue(AtoB);
            Assert.IsFalse(AtoC);
            Assert.IsFalse(AtoD);
        }
        [Test]
        public void Matrix_Cross()
        {
            //arrange
            var elements_Exception = new double[2, 2] { { 2, 0 }, { 0, 1 } };
            var elementsB = new double[1, 3] { { 2, 0, 1 } };
            var elementsC = new double[3, 1] { { 2 }, { 2 }, { 2 } };
            var elementsAns = new double[1, 3] { { -2, -2, 4} };
            Matrix<double> matA = new Matrix<double>(elements_Exception);
            Matrix<double> matB = new Matrix<double>(elementsB);
            Matrix<double> matC = new Matrix<double>(elementsC);
            Matrix<double> matD = new Matrix<double>(elementsAns);
            Matrix<double> AcrossC = new Matrix<double>();

            //Act
            Matrix<double> BcrossC = Matrix<double>.Cross(matB, matC);
            try { AcrossC = Matrix<double>.Cross(matB, matC);}

            //Assert
            catch (ArgumentException)
            { Assert.Pass(); }
            Assert.AreEqual(matD, AcrossC);

        }
        [Test]
        public void Matrix_String_Test()
        {
            //Arrange
            var elements1 = new double[2, 2] { { 2, 0 },{2, 1 } };
            Matrix<double> matA = new Matrix<double>(elements1);

            //Act
            Matrix<double> matB = new Matrix<double>("[2,0;2,1]");

            //Assert
            Assert.AreEqual(matA, matB);
        }
        /// <summary>
        /// Testing Cumulative Product, MatA tests a 2x2, MatB tests a 2x1 and MatC tests a 3x1 which is not implemented  TODO: test 1x2, currently unsupported, need to check for rowvector and treat differently
        /// </summary>
        [Test]
        public void Matrix_CumProd()
        {
            //Arrange
            var elements1 = new double[2, 2] { { 2, 0 }, { 2, 1 } };
            Matrix<double> matA = new Matrix<double>(elements1);
            var elements2 = new double[2,1] { { 2 },{ 1} };
            Matrix<double> matB = new Matrix<double>(elements2);
            var elementsC = new double[3, 1] { { 2 }, { 2 }, { 2 } };
            Matrix<double> matC = new Matrix<double>(elementsC);

            var elements3 = new double[2, 2] { { 2, 0 }, {4, 0 } };
            Matrix<double> expectedAnsA = new Matrix<double>(elements3);
            var elements4 = new double[1, 2] { { 2,2 } };
            Matrix<double> expectedAnsB = new Matrix<double>(elements4);

            //Act
            Matrix<double> actualAnsA = Matrix<double>.Cumprod(matA);
            Matrix<double> actualAnsB = Matrix<double>.Cumprod(matB);
            try { Matrix<double> _ = Matrix<double>.Cumprod(matC); }
            catch (NotImplementedException) 

            //Assert
            { Assert.Pass(); }
            Assert.AreEqual(expectedAnsA, actualAnsA);
            Assert.AreEqual(expectedAnsB, actualAnsB);
           
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
