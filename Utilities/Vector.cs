// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;

namespace Utilities
{
    [Serializable()]
    public class Vector : ISerializable, IEnumerable
    {
        #region Properties
        /// <summary>
        /// The number of elements in the vector
        /// </summary>
        /// 
        
        [XmlIgnore]
        public int Length { get; set; }

        #endregion

        #region Members

        /// <summary>
        /// The vector is an array 
        /// </summary>
        private List<double> _elements;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a 0x0 null Matrix with no content
        /// </summary>
        public Vector(int n)
        {
            Length = n;
            _elements = new List<double>(new double[n]);
        }

        public Vector(List<double> elements)
        {
            Length = elements.Count;
            _elements = elements;
        }
        public Vector(string VectorString)
        {
            string[] elements = VectorString.Split(';');
            elements[0] = elements[0].TrimStart('[');
            elements[elements.Length - 1] = elements[elements.Length - 1].TrimEnd(']');
            double[] dElem;
            dElem = Array.ConvertAll(elements, new Converter<string, double>(Double.Parse));
            Length = dElem.Length;
            _elements = new List<double>(dElem);
        }

        public Vector(SerializationInfo info, StreamingContext context)
         {
             Length = info.GetInt32("Length");
             _elements = (List<double>)info.GetValue("_elements", typeof(List<double>));
         }
         
        #endregion

        #region Overrrides
        // TOD) (Eric):  This should match the constructor based on a string...
        /// <summary>
        /// Converts a Matrix to a string
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            string s = "[";

                foreach (double element in _elements)
                    s += element.ToString() + "," + " ";


            s = s.Substring(0, s.Length - 2);
            s += "]";

            return s;
        }

        /// <summary>
        /// Determines if two matricies are equal by comparing ToString() method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool</returns>
        public override bool Equals(object obj)
        {
            return ToString() == obj.ToString();
        }

        /// <summary>
        /// Gets the hash code of a Matrix based on the ToString() method
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        #endregion

        #region Operators

        /// <summary>
        /// Returns the inner product (or Dot Product) of two n by 1 matrices (or vectors)
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public static double Dot(Vector a, Vector b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Vectors must be of the same length.");
            
            double buf = 0;
            for (int i = 1; i <= a.Length; i++)
            {
                buf += a[i] * b[i];
            }

            return buf;
        }

        /// <summary>
        /// Returns the cross product of two 3x1 (or 1x3) vectors.
        /// The shape of the return vector is determined by the shape of a.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector Cross(Vector a, Vector b)
        {

            if (a.Length != 3 && b.Length != 3)
                throw new ArgumentException("Arguments of the cross product must to be 3x1 vectors.");
            List<double> temp = new List<double>(new double[3] { 0, 0, 0 });
            temp[0] = a[2] * b[3] - a[3] * b[2];
            temp[1] = -a[1] * b[3] + a[3] * b[1];
            temp[2] = a[1] * b[2] - a[2] * b[1];

            Vector c = new Vector(temp);
                return c;
        }

        /// <summary>
        /// Returns the Matrix sum of two matrices
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector operator +(Vector A, Vector B)
        {
            if (A.Length != B.Length)
                throw new ArgumentException("Vectors must be the same length when adding.");

            Vector C = new Vector(A.Length);
            for (int c = 1; c <= A.Length; c++)
            {
                C[c] = A[c] + B[c];
            }

            return C;
        }

        /// <summary>
        /// Returns the Matrix sum of a Matrix and a complex.
        /// The Complex number is added to each element of the Matrix
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector operator +(Vector A, double b)
        {

            Vector C = new Vector(A.Length);

            for (int c = 1; c <= A.Length; c++)
            {
                C[c] = A[c] + b;
            }

            return C;
        }

        /// <summary>
        /// Retruns the Matrix sum of a Complex and a Matrix
        /// The Complex number is added to each element of the Matrix
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector operator +(double a, Vector B)
        {
            
            Vector C = new Vector(B.Length);

            C = B + a;

            return C;
        }

        /// <summary>
        /// Returns the Matrix difference of two matrices
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector operator -(Vector A, Vector B)
        {
            //if (A.Size != B.Size)
            //  throw new ArgumentException("Matrices must be the same dimension when subtracting.");
            if (A.Length != B.Length)
                throw new ArgumentException("Vectors must be the same length when adding.");
            Vector C = A + (-B);

            return C;
        }

        /// <summary>
        /// Returns the Matrix difference of a Matrix and a Complex
        /// The Complex number is subtracted from each element of the Matrix
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector operator -(Vector A, double b)
        {
            Vector C = new Vector(A.Length);

            for (int c = 1; c <= A.Length; c++)
            {
                C[c] = A[c] - b;
            }
            return C;
        }

        /// <summary>
        /// Retruns the Matrix difference of a Complex and a Matrix
        /// The Complex number is subtracted from each element of the Matrix
        /// </summary>
        /// <param name="a"></param>
        /// <param name="B"></param>
        /// <returns>Matrix</returns>
        public static Vector operator -(double a, Vector B)
        {
            Vector C = new Vector(B.Length);

            C = -B + a;

            return C;
        }

        /// <summary>
        /// Returns the negative of a Matrix
        /// </summary>
        /// <param name="A"></param>
        /// <returns>Matrix</returns>
        public static Vector operator -(Vector A)
        {
            Vector C = new Vector(A.Length);

            for (int c = 1; c <= A.Length; c++)
                C[c] = -1 * A[c];

            return C;
        }

        public static Vector operator *(Vector a, Matrix<double> B)
        {
            if (a.Length != B.NumRows)
                throw new InvalidOperationException("The vector a must be length of the number of rows in the Matrix B");
            Vector d = new Vector(B.NumCols);
            for (int c = 1; c <= B.NumCols; c++)
            {
                for (int r = 1; r <= a.Length; r++)
                {
                    d[c] = d[c] + a[r] * B[r, c];
                }
                
            }

            return d;
        }
        public static Vector operator *(Matrix<double> B, Vector a)
        {
            if (a.Length != B.NumRows)
                throw new InvalidOperationException("The vector a must be length of the number of rows in the Matrix B");
            Vector d = new Vector(B.NumCols);
            for (int c = 1; c <= B.NumCols; c++)
            {
                for (int r = 1; r <= a.Length; r++)
                {
                    d[c] = d[c] + a[r] * B[c, r];
                }

            }

            return d;
        }

        /// <summary>
        /// Returns the Matrix product of a Complex and a Matrix (elementwise)
        /// </summary>
        /// <param name="A">Complex</param>
        /// <param name="B">Matrix</param>
        /// <returns>C = A * B</returns>
        public static Vector operator *(double a, Vector B)
        {
            Vector C = new Vector(B.Length);

            for (int c = 1; c <= B.Length; c++)
            {
                C[c] = a * B[c];
            }


            return C;
        }

        /// <summary>
        /// Returns the Matrix product of a Matric and a Complex (elementwise)
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Complex</param>
        /// <returns>C = A * B</returns>
        public static Vector operator *(Vector A, double b)
        {
            Vector C = new Vector(A.Length);

            C = b * A;

            return C;
        }

        /// <summary>
        /// Returns the quotient of a Matrix and a complex (elementwise)
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Complex</param>
        /// <returns>C = A / B</returns>
        public static Vector operator /(Vector A, double b)
        {
            Vector C = new Vector(A.Length);

            for (int c = 1; c <= A.Length; c++)
            {
                C[c] = A[c] / b;
            }

            return C;
        }

        /// <summary>
        /// Returns a boolean indicating if two matricies are equal
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator ==(Vector A, Vector B)
        {

            if (A.Length != B.Length)
                return false;
            else
            {
                for (int i = 1; i <= A.Length; i++)
                    if (A[i] != B[i])
                        return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a boolean indicating if two matricies are not equal
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator !=(Vector A, Vector B)
        {
            return !(A == B);
        }

        #endregion

        #region Dynamics
        /// <summary>
        /// Returns true if all elements of the Matrix are real
        /// </summary>
        /// <returns></returns>
        public bool IsReal()
        {
            bool isreal = true;
            throw new NotImplementedException("Matrix.IsReal()");
            
        }

        /// <summary>
        /// Returns true if all elements of the Matrix are complex
        /// </summary>
        /// <returns></returns>
        public bool IsComplex()
        {
            return !IsReal();
        }

        public void SetValue(int col, double value) ///Morgan Added this
        {
            if( col > 0 && col <= 100)
                this[col] = value; // why do we access it like this?
            else
                throw new ArgumentException("Element indicies out of Vector bounds");
        }
        #endregion

        #region Statics

        /// <summary>
        /// Returns a row vector containing the maximum of the elements of each column.
        /// If A is a row vector, the maximum element is returned.
        /// If A is a column vector, the maximum elelment is returned
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static double Max(Vector A)
        {
            
            double max = A[1];
            foreach (double value in A)
            {
                max = System.Math.Max(max, value);
            }
            return max;
        }
      
        public static double Min(Vector A)
        {
            double min = A[1];
            foreach (double value in A)
            {
                min = System.Math.Min(min, value);
            }
            return min;
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static Vector Abs(Vector A)
        {
            Vector R = (Vector)A.Clone();
            int i = 1;
            foreach (double c in A)
            {
                R[i] = System.Math.Abs(c);
                i++;
            }

            return R;
        }

        public static double Norm(Vector A)
        {

            double temp = 0;
            foreach (double c in A)
                temp += c * c;

            return System.Math.Sqrt(temp);

        }

        public static Vector Cumprod(Vector A)
        {
            return Vector.Cumprod(A, 1);
        }

        public static Vector Cumprod(Vector A, int Dim)
        {
            Vector C = (Vector)A.Clone();

                    for (int c = 1; c <= A.Length; c++)
                    {
                        C[c] *= C[c-1];
                    }
            return C;

        }

        public static implicit operator Matrix<double>(Vector v)
        {
            return new Matrix<double>(v.ToString());
        }

        public static implicit operator Vector(List<double> c)
        {
            return new Vector(c);
        }

        public static explicit operator double(Vector m)
        {
            if (m.Length == 1)
                return m[1];
            else
                throw new NotImplementedException("explicit operator T(Matrix m) - Conversion from N by M Matrix to double not possible when N, M > 1");
        }
        public static explicit operator Vector(Matrix<double> m)
        {
            if (m.NumCols == 1 || m.NumRows == 1)
            {

                Vector v = new Vector(Math.Max(m.NumCols, m.NumRows));
                int index = 1;
                foreach (double elem in m)
                {
                    v[index] = elem;
                    index++;
                }
                return v;
            }
            else
                throw new NotImplementedException();
        }
        #endregion

        #region Interfaces

        public object Clone() // ICloneable
        {
            Vector m = new Vector(3); //TODO: Why was there a ToArray? 
            return m;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {

            info.AddValue("Length", this.Length);
            info.AddValue("_elements", _elements, typeof(double[]));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new VectorEnum(this);
        }

            
        #endregion

        #region Accessors
        /// <summary>
        /// Returns the element of a Matrix based on column major form with (1) based indexing
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double this[int index]
        {
            get
            {
                return _elements[index-1];
            }
            set
            {
                 _elements[index-1] =  value;
            }
        }

        /// <summary>
        /// Returns the elements of a Matrix using the Matrix Index object
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        /// 
       
        public Vector this[MatrixIndex rows]
        {
            get
            {
                Vector C = new Vector(rows.Length);

                for (int r = 1; r <= rows.Length; r++)
                {
                    C[r] = this[rows[r]];
                }

                return C;
            }
            set
            {
                for (int r = 1; r <= rows.Length; r++)
                {
                    this[rows[r]] = value[r];
                }
            }
        }
        
        #endregion
    }

    public class VectorEnum : IEnumerator
    {
        public Vector MatrixData;

        int position = -1;

        public VectorEnum(Vector data)
        {
            MatrixData = data;
        }
        
        public bool MoveNext()
        {
            position++;
            return (position < MatrixData.Length);
        }
        
        public void Reset()
        {
            position = -1;
        }

        public object Current
        {
            get
            {
                try
                {
                    return MatrixData[position + 1];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
    public class VectorSize
    {
        public int NumRows { get; set; }
        public int NumColumns { get; set; }

        public VectorSize(int r, int c)
        {
            NumRows = r;
            NumColumns = c;
        }
    }
}

