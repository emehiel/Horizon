
using System;
using NUnit.Framework;
using Utilities;
using System.Collections;
using System.Collections.Generic;

namespace UtilitiesUnitTest

{
    [TestFixture]
    public class VectorTest
    {
        [TestCaseSource(typeof(DotProductData), "TestCases")]
        public void DotTest(Vector a, Vector b, double c)
        {
            Assert.That(() => Vector.Dot(a, b), Is.EqualTo(c));
        }
        [Test]
        public void CrossProductSize()
        {
            Vector a = new Vector(new List<double>(new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
            Vector b = new Vector(new List<double>(new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
            Assert.That(() => Vector.Cross(a, b), Throws.TypeOf<ArgumentException>());
        }
        [TestCaseSource(typeof(CrossProductData), "TestCases")]
        public void CrossTest(Vector a, Vector b, Vector c)
        {
            Assert.That(() => Vector.Cross(a, b), Is.EqualTo(c));
        }
        [TestCaseSource(typeof(VectorNormData), "TestCases")]
        public void NormTest(Vector a, double b)
        {
            Assert.That(() => Vector.Norm(a), Is.EqualTo(b));
        }
        [TestCaseSource(typeof(MatrixVectorData), "TestCases")]
        public void MatrixVectorMultiplicationTest(Vector a, Matrix<double> B, Vector c)
        {
            Assert.That(() => a * B, Is.EqualTo(c));
        }
        [TestCaseSource(typeof(VectorMatrixData), "TestCases")]
        public void VectorMatrixMultiplicationTest(Matrix<double> A, Vector b, Vector c)
        {
            Assert.That(() => A * b, Is.EqualTo(c));
        }
    }
    public class DotProductData
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new Vector(new List<double>(new double[] { 7378, 0, 0 })), 
                    new Vector(new List<double>(new double[] { -12525, 1346, -3517 })), -92409450);

                yield return new TestCaseData(new Vector(new List<double>(new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })), 
                    new Vector(new List<double>(new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })), 285);

                yield return new TestCaseData(new Vector(new List<double>(new double[] { 0 })), 
                    new Vector(new List<double>(new double[] { 0 })), 0);
            }
        }
    }
    public class CrossProductData
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new Vector(new List<double>(new double[] { 7378, 0, 0 })), 
                    new Vector(new List<double>(new double[] { -12525, 1346, -3517 })), 
                    new Vector(new List<double>(new double[] { 0, 25948426, 9930788 })));

                yield return new TestCaseData(new Vector (new List<double>(new double[] { 0, 0, 0 })), 
                    new Vector(new List<double>(new double[] { 0, 0, 0 })), 
                    new Vector(new List<double>(new double[] { 0, 0, 0 })));
            }
        }
    }
    public class VectorNormData
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new Vector(new List<double>(new double[] { 7378, 0, 0 })), 7378);

                yield return new TestCaseData(new Vector(new List<double>(new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24})), 70);

                yield return new TestCaseData(new Vector(new List<double>(new double[] { 0 })), 0);
            }
        }
    }
    public class MatrixVectorData
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new Vector(new List<double>(new double[] { 1, 2, 3 })),
                    new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } }),
                    new Vector(new List<double>(new double[] { 30, 36, 42 })));

                yield return new TestCaseData(new Vector(new List<double>(new double[] { 1, 2, 3 })),
                    new Matrix<double>(new double[,] { { 1 }, { 2 }, { 3 } }),
                    new Vector(new List<double>(new double[] { 14 })));

                yield return new TestCaseData(new Vector(new List<double>(new double[] { 0 })),
                    new Matrix<double>(new double[,] { { 0 } }),
                    new Vector(new List<double>(new double[] { 0 })));

            }
        }
    }
        public class VectorMatrixData
        {
            public static IEnumerable TestCases
            {
                get
                {
                    yield return new TestCaseData(new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } }),
                        new Vector(new List<double>(new double[] { 1, 2, 3 })),
                        new Vector(new List<double>(new double[] { 14, 32, 50 })));

                }
            }
        }
    
}