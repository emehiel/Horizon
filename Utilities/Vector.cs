﻿// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;

namespace Utilities
{
    [Serializable()]
    public class Vector<T> : ISerializable, IEnumerable
    {
        #region Properties
        /// <summary>
        /// The number of rows in the Matrix<T>
        /// </summary>
        /// 

        [XmlIgnore]
        public int Length { get; set; }
        /*
        /// <summary>
        /// The number of columns in the Matrix<T>
        /// </summary>
        [XmlIgnore]
        public int NumCols { get; set; }

        /// <summary>
        /// Gets the size of the Matrix<T> (rows by columns) in the elements of a Matrix<T> [r, c]
        /// </summary>
        [XmlIgnore]
        public Matrix<int> Size
        {
            get
            {
                return new Matrix<int>(new int[1, 2] { { NumRows, NumCols } });
            }
        }

        /// <summary>
        /// Returns the maximum of the number of rows or columns of the Matrix<T>
        /// </summary>
        
            [XmlIgnore]
        public int Length
        {
            get
            {
                return _elements;
            }
        }
        
        /// <summary>
        /// Returns the total number of elements in the Matrix<T>, r*c
        /// </summary>
        [XmlIgnore]
        public int NumElements
        {
            get
            {
                return 3;
            }
        }
        */
        #endregion

        #region Members

        // TODO:  Should I write a column class that encapsulates a column or is this too much abstraction?
        // TODO:  Should we restructure the Matrix<T> class to be a single list of complex numbers?

        /// <summary>
        /// Each Vector<T> is represented as an array of type T numbers.
        /// </summary>
        private T[] _elements;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a n length null Vector<T> with no content
        /// </summary>
        public Vector(int n)
        {
            Length = n;
            _elements = new T[n];
        }

        /// <summary>
        /// Initializes a Vector<T> based on an array of T type numbers.
        /// </summary>
        /// <param name="elements"></param>
        public Vector(T[] elements)
        {
            Length = elements.Length;
            _elements = elements;

        }
        /// <summary>
        /// Initializes a vector based on a string format [ x; x ]
        /// </summary>
        /// <param name="VectorString"></param>
        public Vector(string VectorString)
        {
            string[] elements = VectorString.Split(';');
            elements[0] = elements[0].TrimStart('[');
            elements[elements.Length - 1] = elements[elements.Length - 1].TrimEnd(']');
            double[] dElem;

            dElem = Array.ConvertAll(elements, new Converter<string, double>(Double.Parse));
            Length = dElem.Length;
            _elements = (T[])Convert.ChangeType(dElem, typeof(T[]));
        }


        public Vector(SerializationInfo info, StreamingContext context)
        {
            Length = info.GetInt32("Length");
            //NumRows = info.GetInt32("NumCols");
            _elements = (T[])info.GetValue("_elements", typeof(T[]));
        }

        #endregion

        #region Overrrides
        // TOD) (Eric):  This should match the constructor based on a string...
        /// <summary>
        /// Converts a Vector<T> to a string
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            string s = "[";

            foreach (T element in _elements)
                s += element.ToString() + ";" + " ";

            s = s.Substring(0, s.Length - 2);
            s += "]";

            return s;
        }

        /// <summary>
        /// Determines if two vectors are equal by comparing ToString() method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool</returns>
        public override bool Equals(object obj)
        {
            return ToString() == obj.ToString();
        }

        /// <summary>
        /// Gets the hash code of a Vector<T> based on the ToString() method
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }



        #endregion

        #region Operators

        /// <summary>
        /// Returns the inner product (or Dot Product) of two vectors
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public static T Dot(Vector<T> a, Vector<T> b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Vectors must be of the same length.");

            T buf = (T)Convert.ChangeType(0, typeof(T));

            for (int i = 1; i <= a.Length; i++)
            {
                buf += (dynamic)a[i] * b[i];
            }

            return buf;
        }

        /// <summary>
        /// Returns the cross product of two length 3 vectors.
        /// The shape of the return vector is determined by the shape of a.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns name="c">The cross product</returns>
        public static Vector<T> Cross(Vector<T> a, Vector<T> b)
        {
            if (a.Length != 3 && b.Length != 3)
                throw new ArgumentException("Arguments of the cross product must to be 3x1 vectors.");
            T[] temp = new T[3];
            temp[0] = (dynamic)a[2] * b[3] - (dynamic)a[3] * b[2];
            temp[1] = (dynamic)a[1] * b[3] - (dynamic)a[3] * b[1];
            temp[2] = (dynamic)a[1] * b[2] - (dynamic)a[2] * b[1];

            Vector<T> c = new Vector<T>(temp);

            return c;

        }

        /// <summary>
        /// Returns the Vecotr<T> sum of two matrices
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector<T> operator +(Vector<T> a, Vector<T> b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Vectors must be the same length when adding.");

            Vector<T> C = new Vector<T>(a.Length);
            for (int c = 1; c <= 2; c++)
            {
                C[c] = (dynamic)a[c] + b[c];
            }

            return C;
        }

        /// <summary>
        /// Returns the Vector<T> sum of a Vector<T> and a complex.
        /// The Complex number is added to each element of the Vector<T>
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector<T> operator +(Vector<T> A, T b)
        {

            Vector<T> C = new Vector<T>(A.Length);
            for (int c = 1; c <= 2; c++)
            {
                C[c] = (dynamic)A[c] + b;
            }

            return C;
        }

        /// <summary>
        /// Retruns the Vector<T> sum of a Complex and a Vector<T>
        /// The Complex number is added to each element of the Vector<T>
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector<T> operator +(T a, Vector<T> B)
        {

            Vector<T> C = new Vector<T>(B.Length);

            C = B + a;

            return C;
        }

        /// <summary>
        /// Returns the Vector<T> difference of two matrices
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector<T> operator -(Vector<T> A, Vector<T> B)
        {

            if (A.Length != B.Length)
                throw new ArgumentException("Vectors must be the same length when subtracting.");
            Vector<T> C = A + (-B);

            return C;
        }

        /// <summary>
        /// Returns the Vector<T> difference of a Vector<T> and a Complex
        /// The Complex number is subtracted from each element of the Vector<T>
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector<T> operator -(Vector<T> A, T b)
        {
            Vector<T> C = new Vector<T>(A.Length);
            for (int c = 1; c <= 2; c++)
            {
                C[c] = (dynamic)A[c] - b;
            }

            return C;
        }

        /// <summary>
        /// Retruns the Vector<T> difference of a Complex and a Vector<T>
        /// The Complex number is subtracted from each element of the Vector<T>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="B"></param>
        /// <returns>Vector<T></returns>
        public static Vector<T> operator -(T a, Vector<T> B)
        {
            Vector<T> C = new Vector<T>(B.Length);

            C = -B + a;

            return C;
        }

        /// <summary>
        /// Returns the negative of a Vector<T>
        /// </summary>
        /// <param name="A"></param>
        /// <returns>Vector<T></returns>
        public static Vector<T> operator -(Vector<T> A)
        {
            Vector<T> C = new Vector<T>(A.Length);

            for (int c = 1; c <= 2; c++)
                C[c] = -1 * (dynamic)A[c];

            return C;
        }

        /// <summary>
        /// Returns the Vector<T> product of two matricies
        /// </summary>
        /// <param name="A">Matrix<T></param>
        /// <param name="B">Matrix<T></param>
        /// <returns>C = A * B</returns>
        /// 
        /*
        public static Matrix<T> operator *(Matrix<T> A, Matrix<T> B)
        {
            if (A.NumCols != B.NumRows)
                throw new ArgumentException("Inner Matrix<T> dimensions must agree.");

            Matrix<T> C = new Matrix<T>(A.NumRows, B.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= B.NumCols; c++)
                {
                    C[r, c] = Matrix<T>.Dot(A.GetRow(r), B.GetColumn(c));
                }
            }

            return C;
        }
        */
        /// <summary>
        /// Returns the Vector<T> product of a Complex and a Vector<T> (elementwise)
        /// </summary>
        /// <param name="A">Complex</param>
        /// <param name="B">Matrix<T></param>
        /// <returns>C = A * B</returns>
        public static Vector<T> operator *(T a, Vector<T> B)
        {
            Vector<T> C = new Vector<T>(B.Length);

            for (int c = 1; c <= 2; c++)
            {
                C[c] = (dynamic)a * B[c];
            }

            return C;
        }

        /// <summary>
        /// Returns the Vector<T> product of a Vector and a Complex (elementwise)
        /// </summary>
        /// <param name="A">Matrix<T></param>
        /// <param name="B">Complex</param>
        /// <returns>C = A * B</returns>
        public static Vector<T> operator *(Vector<T> A, T b)
        {
            Vector<T> C = new Vector<T>(A.Length);

            C = b * A;

            return C;
        }

        /// <summary>
        /// Returns the quotient of a Vector<T> and a complex (elementwise)
        /// </summary>
        /// <param name="A">Matrix<T></param>
        /// <param name="B">Complex</param>
        /// <returns>C = A / B</returns>
        public static Vector<T> operator /(Vector<T> A, T b)
        {
            Vector<T> C = new Vector<T>(A.Length);

            for (int c = 1; c <= 2; c++)
            {
                C[c] = (dynamic)A[c] / b;
            }

            return C;
        }

        /// <summary>
        /// Returns the quotient of a Vector<T> and a Vector<T> (elementwise)
        /// </summary>
        /// <param name="A">Matrix<T></param>
        /// <param name="B">Matrix<T></param>
        /// <returns>C = A / B</returns>
        /*
        public static Matrix<T> operator /(Matrix<T> A, Matrix<T> B)
        {
            // TODO: Throw exception if A and B are different shape
            Matrix<T> C = new Matrix<T>(A.NumRows, A.NumCols);
            int i = 1;

            for (int r = 1; r <= B.NumRows; r++)
            {
                for (int c = 1; c <= B.NumCols; c++)
                {
                    C[r, c] = (dynamic)A[r, c] / B[r, c];
                }
            }

            return C;
        }
        */
        /// <summary>
        /// Returns a boolean indicating if two vectors are equal
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator ==(Vector<T> A, Vector<T> B)
        {
            if (A.Length != B.Length)
                return false;
            else
            {
                for (int i = 1; i <= 2; i++)
                    if ((dynamic)A[i] != B[i])
                        return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a boolean indicating if two vectors are not equal
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator !=(Vector<T> A, Vector<T> B)
        {
            return !(A == B);
        }

        #endregion

        #region Dynamics

        /// <summary>
        /// Converts a Matrix<T> to an array of T type numbers
        /// </summary>
        /// <returns></returns>
        /*public T[,] ToArray()
        {
            T[,] T_Array = new T[NumRows, NumCols];

            for (int r = 0; r < NumRows; r++)
                for (int c = 0; c < NumCols; c++)
                    T_Array[r, c] = this[r + 1, c + 1];

            return T_Array;
        }

        public bool IsVector()
        {
            return (NumCols == 1 || NumRows == 1);
        }

        public bool IsRowVector()
        {
            return (NumRows == 1);
        }

        public bool IsColumnVector()
        {
            return (NumCols == 1);
        }
        */
        /// <summary>
        /// Returns true if all elements of the Matrix<T> are real
        /// </summary>
        /// <returns></returns>
        public bool IsReal()
        {
            bool isreal = true;
            /*
            foreach (List<T> row in _elements)
                foreach (T element in row)
                    isreal &= (T as Matrix<T>).IsReal;
            
            return isreal;
    */
            throw new NotImplementedException("Matrix<T>.IsReal()");

        }

        /// <summary>
        /// Returns true if all elements of the Matrix<T> are complex
        /// </summary>
        /// <returns></returns>
        public bool IsComplex()
        {
            return !IsReal();
        }
        /*
        public bool IsSquare()
        {
            return (NumRows == NumCols);
        }

        public bool IsNull()
        {
            return (NumCols == 0 && NumRows == 0);
        }
        */
        /// <summary>
        /// In place vertical concatination of this Matrix<T> and Matrix<T> A.
        /// </summary>
        /// <param name="A"></param>
        /*public void Vertcat(Matrix<T> A)
        {
            if (NumCols == A.NumCols)
            {
                int origNumRows = NumRows;
                for (int i = 1; i <= A.NumRows; i++)
                    SetRow(i + origNumRows, A.GetRow(i));
            }
            else
                throw new ArgumentException("Vertical Concatination Requires Column Compatability");

        }
        */

        /// <summary>
        /// In place horizontal concatination of this Matrix<T> and Matrix<T> A.
        /// </summary>
        /// <param name="A"></param>
       /* public void Horzcat(Matrix<T> A)
        {
            if (NumRows == A.NumRows)
            {
                int origNumCols = NumCols;
                for (int i = 1; i <= A.NumCols; i++)
                    SetColumn(i + origNumCols, A.GetColumn(i));
            }
            else
                throw new ArgumentException("Vertical Concatination Requires Column Compatability");
        }
        
        /// <summary>
        /// Returns a column of this Matrix<T>
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Matrix<T> GetColumn(int c)
        {
            if (c < 1)
                throw new ArgumentOutOfRangeException("Invalid column number", "Matrix<T> indices must be non-negative");
            else if (c > NumCols)
                throw new ArgumentOutOfRangeException("Invalid column number", "Column index must not exceed size of Matrix<T>");
            else
            {
                Matrix<T> C = new Matrix<T>(NumRows, 1);
                for (int r = 1; r <= NumRows; r++)
                    C[r, 1] = this[r, c];
                return C;
            }
        }
        public void SetColumn(int col, Matrix<T> colData)
        {
            if (colData.NumRows == NumRows)
            {
                for (int r = 1; r <= NumRows; r++)
                    this[r, col] = colData[r, 1];
            }
            else
                throw new ArgumentException("Column Size Not Compatable");
        }

        /// <summary>
        /// Returns a row of this Matrix<T>
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Matrix<T> GetRow(int r)
        {
            Matrix<T> C = new Matrix<T>(1, NumCols);
            for (int c = 1; c <= NumCols; c++)
                C[1, c] = this[r, c];
            return C;
        }
        public void SetRow(int row, Matrix<T> rowData)
        {
            if (rowData.NumCols == NumCols)
            {
                for (int c = 1; c <= NumCols; c++)
                    this[row, c] = rowData[1, c];
            }
            else
                throw new ArgumentException("Row Size Not Compatable");
        }
        */
        public void SetValue(int col, T value) ///Morgan Added this
        {
            if (col > 0 && col <= 2)
                this[col] = value; // why do we access it like this?
            else
                throw new ArgumentException("Element indicies out of Vector<T> bounds");
        }
        #endregion

        #region Statics



        // TODO:  SHould these get/set functions be static?
        /*public static Matrix<T> GetColumn(Matrix<T> A, int column)
        {
            return A.GetColumn(column);
        }

        public static void SetColumn(Matrix<T> A, int column, Matrix<T> columnData)
        {
            A.SetColumn(column, columnData);
        }
        */
        /// <summary>
        /// Returns a row of a Matrix<T>
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        /*public static Matrix<T> GetRow(Matrix<T> A, int row)
        {
            return A.GetRow(row);
        }

        public static void SetRow(Matrix<T> A, int row, Matrix<T> rowData)
        {
            A.SetRow(row, rowData);
        }
        */
        /// <summary>
        /// Vertical Concatination of two matrices.  Neither Matrix<T> is modified.  A new Matrix<T> is returned.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        /*public static Matrix<T> Vertcat(Matrix<T> A, Matrix<T> B)
        {
            Matrix<T> C = (Matrix<T>)A.Clone();
            C.Vertcat(B);
            return C;
        }

        /// <summary>
        /// Horizontal Concatination of two matrices.  Neither Matrix<T> is modified.  A new Matrix<T> is returned.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix<T> Horzcat(Matrix<T> A, Matrix<T> B)
        {
            Matrix<T> C = (Matrix<T>)A.Clone();
            C.Horzcat(B);
            return C;
        }

        public static Matrix<T> Eye(int size)
        {
            Matrix<T> eye = new Matrix<T>(size);

            for (int i = 1; i <= size; i++)
                eye[i, i] = (T)(Convert.ChangeType(1, typeof(T)));

            return eye;
        }
        
        /// <summary>
        /// Returns the transpose of a Matrix<T>
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static Matrix<T> Transpose(Matrix<T> A)
        {
            Matrix<T> C = new Matrix<T>(A.NumCols, A.NumRows);

            for (int r = 1; r <= A.NumCols; ++r)
            {
                for (int c = 1; c <= A.NumRows; ++c)
                    C[r, c] = A[c, r];
            }

            return C;
        }

        public static T Trace(Matrix<T> A)
        {
            if (A.IsSquare())
            {
                T temp = (T)Convert.ChangeType(0, typeof(T));
                for (int i = 1; i <= A.NumRows; i++)
                    temp += (dynamic)A[i, i];
                return temp;
            }
            else
                throw new ArgumentException("Trace does not operate on non-square matrices");
        }
        */
        /// <summary>
        /// Returns a row vector containing the maximum of the elements of each column.
        /// If A is a row vector, the maximum element is returned.
        /// If A is a column vector, the maximum elelment is returned
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static T Max(Vector<T> A)
        {
            /*if (A.IsColumnVector() || A.IsRowVector())
            {
                T max = A[1];

                foreach (T c in A)
                    max = System.Math.Max((dynamic)max, (dynamic)c);
                return new Matrix<T>(1, 1, max);
            }
            else
            {
                Matrix<T> max = new Matrix<T>(1, A.NumCols);
                for (int col = 1; col <= A.NumCols; col++)
                {
                    Matrix<T> m = A.GetColumn(col);
                    max[1, col] = (T)Max(m);
                }
                return max;
            }
            */
            T max = A[1];
            foreach (T value in A)
            {
                max = System.Math.Max((dynamic)max, (dynamic)value);
            }
            return max;
        }
        /*
        public static Matrix<T> Max(Matrix<T> A, Matrix<T> B)
        {
            Matrix<T> C = new Matrix<T>(A.NumRows, A.NumCols);

            int i = 1;
            foreach (T c in A)
            {
                C[i] = (T)System.Math.Max((dynamic)A[i], (dynamic)B[i]);
                i++;
            }

            return C;
        }

        /// <summary>
        /// Returns a Matrix<T> whos elements are the maximum of {A[i,j], b}.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix<T> Max(Matrix<T> A, T b)
        {
            Matrix<T> C = (Matrix<T>)A.Clone();
            int i = 1;
            foreach (T c in A)
            {
                C[i] = (T)System.Math.Max((dynamic)A[i], (dynamic)b);
                i++;
            }

            return C;
        }
        */
        public static Matrix<T> Min(Matrix<T> A)
        {
            /*if (A.IsColumnVector() || A.IsRowVector())
            {
                T min = A[1];
                foreach (T c in A)
                    min = System.Math.Min((dynamic)min, (dynamic)c);

                return min;
            }
            else
            {
                Matrix<T> min = new Matrix<T>(1, A.NumCols);
                for (int col = 1; col <= A.NumCols; col++)
                {
                    Matrix<T> m = A.GetColumn(col);
                    min[1, col] = (T)Min(m);
                }
                return min;
            }
            */
            T min = A[1];
            foreach (T value in A)
            {
                min = System.Math.Min((dynamic)min, (dynamic)value);
            }
            return min;
        }
        /*
        public static Matrix<T> Min(Matrix<T> A, Matrix<T> B)
        {
            Matrix<T> C = new Matrix<T>(A.NumCols, A.NumRows);

            int i = 1;
            foreach (T c in A)
            {
                C[i] = (T)System.Math.Min((dynamic)A[i], (dynamic)B[i]);
                i++;
            }

            return C;
        }

        /// <summary>
        /// Returns a Matrix<T> whos elements are the minimum of {A[i,j], b}.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix<T> Min(Matrix<T> A, T b)
        {
            Matrix<T> C = (Matrix<T>)A.Clone();
            int i = 1;
            foreach (T c in A)
            {
                C[i] = (T)System.Math.Min((dynamic)A[i], (dynamic)b);
                i++;
            }

            return C;
        }
        */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static Vector<T> Abs(Vector<T> A)
        {
            Vector<T> R = (Vector<T>)A.Clone();
            int i = 1;
            foreach (T c in A)
            {
                R[i] = (T)System.Math.Abs((dynamic)c);
                i++;
            }

            return R;
        }

        public static double Norm(Vector<T> A)
        {
            // TODO:  Handle the case when c is complex
            //if (A.IsRowVector() || A.IsColumnVector())
            //{
            T temp = (T)Convert.ChangeType(0, typeof(T));
            foreach (T c in A)
                temp += (dynamic)c * c;

            return System.Math.Sqrt((dynamic)temp);
            //}
            // else
            //     throw new NotImplementedException("Matrix<T>.Norm(Matrix<T> A)");
        }

        public static Vector<T> Cumprod(Vector<T> A)
        {
            return Vector<T>.Cumprod(A, 1);
        }

        public static Vector<T> Cumprod(Vector<T> A, int Dim)
        {
            Vector<T> C = (Vector<T>)A.Clone();

            //if (Dim == 1)
            //{
            //for (int r = 2; r <= A.NumRows; r++)
            //{
            for (int c = 1; c <= 2; c++)
            {
                C[c] *= (dynamic)C[c - 1];
            }
            //}
            return C;
            //}
            /*else if (Dim == 2)
            {
                C = Matrix<T>.Cumprod(Matrix<T>.Transpose(A));
                C = Matrix<T>.Transpose(C);
                return C;
            }

            else
                throw new NotImplementedException("Matrix<T>.Cumprod(Matrix<T> A, int Dim - Cumprod not implimented for Dim >= 3");
                */

        }

        public static implicit operator Matrix<T>(Vector<T> v)
        {
            return new Matrix<T>(v.ToString());
        }

        public static implicit operator Vector<T>(T[] c)
        {
            return new Vector<T>(c);
        }

        public static explicit operator T(Vector<T> m)
        {
            if (m.Length == 1)
                return m[1];
            else
                throw new NotImplementedException("explicit operator T(Matrix<T> m) - Conversion from N by M Matrix<T> to double not possible when N, M > 1");
        }
        public static explicit operator Vector<T>(Matrix<T> v)
        {
            if (v.NumCols == 1 || v.NumRows == 1)
            {
                return new Vector<T>(v.ToString());
            }
            else
                throw new NotImplementedException();
        }
        #endregion

        #region Interfaces

        public object Clone() // ICloneable
        {
            Vector<T> m = new Vector<T>(3); //TODO: Why was there a ToArray? 
            return m;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            //info.AddValue("NumRows", NumRows);
            //info.AddValue("NumCols", NumCols);
            info.AddValue("Length", this.Length);
            //info.AddValue("Values", this.elements[1]);
            info.AddValue("_elements", _elements, typeof(T[]));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new VectorEnum<T>(this);
        }


        #endregion

        #region Accessors
        /// <summary>
        /// Returns the element of a Matrix<T> based on column major form with (1) based indexing
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                /*if (NumRows == 1) // row vector
                    return this[1, index];
                else if (NumCols == 1) // coumn vector
                    return this[index, 1];
                else
                {
                    int ind = index - 1;
                    int c = ind / NumRows;
                    int r = ind - NumRows * c;
                    return this[r + 1, c + 1];
                }
                */
                return (dynamic)_elements[index - 1];
            }
            set
            {
                /*if (NumRows == 1) // row vector
                    this[1, index] = value;
                else if (NumCols == 1) // coumn vector
                    this[index, 1] = value;
                else
                {
                    int ind = index - 1;
                    int c = ind / NumRows;
                    int r = ind - NumRows * c;
                    this[r + 1, c + 1] = value;
                }*/
                _elements[index - 1] = value;
            }
        }

        /// <summary>
        /// Gets or Sets the element of a Matrix<T> based on (row, column) indexing
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        /*public T this[int r, int c]
        {
            get
            {
                if (r < 1 || c < 1)
                    throw new IndexOutOfRangeException("Matrix<T> indices must be non-negative");
                else if (r > NumRows || c > NumCols)
                    throw new IndexOutOfRangeException("Indices must not exceed size of Matrix<T>");
                else
                    return _elements[r - 1][c - 1];
            }
            set
            {
                if (r < 1 || c < 1)
                    throw new IndexOutOfRangeException("Matrix<T> indices must be non-negative");

                if (r > NumRows)
                {
                    for (int i = NumRows; i < r; i++)
                    {
                        _elements.Add(new List<T>(NumCols));

                        for (int j = 0; j < NumCols; j++)
                            _elements[i].Add((T)Convert.ChangeType(0, typeof(T)));
                    }

                    NumRows = r;
                }

                if (c > NumCols)
                {
                    for (int i = 0; i < NumRows; i++)
                    {
                        for (int j = NumCols; j < c; j++)
                            _elements[i].Add((T)Convert.ChangeType(0, typeof(T)));
                    }

                    NumCols = c;
                }

                _elements[r - 1][c - 1] = value;
            }
        }
        */
        /// <summary>
        /// Returns an entire row of a matirx
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c">Should be "all" or ":"</param>
        /// <returns></returns>
        /*public Matrix<T> this[int r, string c]
        {
            get
            {
                if (c.ToUpper() == "ALL" || c.ToUpper() == ":")
                    return GetRow(r);
                else
                    throw new IndexOutOfRangeException("Invalid Column Selection");
            }
            set
            {
                if (c.ToUpper() == "ALL" || c.ToUpper() == ":")
                    SetRow(r, value);
                else
                    throw new IndexOutOfRangeException("Invalid Column Selection");
            }
        }*/
        /*
        /// <summary>
        /// Returns an entire column of a Matrix<T>
        /// </summary>
        /// <param name="r">should be "all" or ":"</param>
        /// <param name="c"></param>
        /// <returns></returns>
        public Matrix<T> this[string r, int c]
        {
            get
            {
                if (r.ToUpper() == "ALL" || r.ToUpper() == ":")
                    return GetColumn(c);
                else
                    throw new IndexOutOfRangeException("Column - No column selected");
            }
            set
            {
                if (NumRows == value.NumRows)
                {
                    if (r.ToUpper() == "ALL" || r.ToUpper() == ":")
                    {
                        for (int row = 1; row <= NumRows; row++)
                            this[row, c] = value[row, 1];
                    }
                    else
                        throw new IndexOutOfRangeException("Row - No Row Selected");
                }
                else
                    throw new IndexOutOfRangeException("Column - Number of rows in Matrix<T> is not the same as new column");
            }
        }
        
        public Matrix<T> this[string r, MatrixIndex c]
        {
            get
            {
                if (r.ToUpper() == "ALL" || r.ToUpper() == ":")
                    return this[new MatrixIndex(1, this.NumRows), c];
                else
                    throw new IndexOutOfRangeException("Column - No column selected");
            }
            set
            {
                if (c.Length == value.NumCols)
                {
                    if (r.ToUpper() == "ALL" || r.ToUpper() == ":")
                    {
                        this[new MatrixIndex(1, this.NumRows), c] = value;
                    }
                    else
                        throw new IndexOutOfRangeException("Row - No Row Selected");
                }
                else
                    throw new IndexOutOfRangeException("Column - Number of rows in Matrix<T> is not the same as new column");
            }
        }

        public Matrix<T> this[MatrixIndex r, string c]
        {
            get
            {
                if (c.ToUpper() == "ALL" || c.ToUpper() == ":")
                    return this[r, new MatrixIndex(1, NumCols)];
                else
                    throw new IndexOutOfRangeException("Column - No column selected");
            }
            set
            {
                if (r.Length == value.NumRows)
                {
                    if (c.ToUpper() == "ALL" || c.ToUpper() == ":")
                    {
                        this[r, new MatrixIndex(1, NumCols)] = value;
                    }
                    else
                        throw new IndexOutOfRangeException("Row - No Row Selected");
                }
                else
                    throw new IndexOutOfRangeException("Column - Number of rows in Matrix<T> is not the same as new column");
            }
        }
        */
        /// <summary>
        /// Returns the elements of a Matrix<T> using the Matrix<T> Index object
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        /// 

        public Vector<T> this[MatrixIndex rows]
        {
            get
            {
                Vector<T> C = new Vector<T>(rows.Length);

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

    public class VectorEnum<T> : IEnumerator
    {
        public Vector<T> MatrixData;

        int position = -1;

        public VectorEnum(Vector<T> data)
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


    // TODO:  How to enumerate? element, row or column?
    /*
    public class MatrixColumnEnum<T> : IEnumerator
    {
        public Matrix<T> MatrixData;

        int position = -1;

        public MatrixColumnEnum(Matrix<T> data)
        {
            MatrixData = data;
        }

        public bool MoveNext()
        {
            position++;
            return (position < MatrixData.NumCols);
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
                    return MatrixData.GetColumn(position + 1);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
    */
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
