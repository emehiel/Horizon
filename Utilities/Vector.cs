// Copyright (c) 2016 California Polytechnic State University
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
    public class Vector : ISerializable, IEnumerable
    {
        #region Properties
        /// <summary>
        /// The number of rows in the Matrix
        /// </summary>
        /// 
        
        [XmlIgnore]
        public int Length { get; set; }

        #endregion

        #region Members

        // TODO:  Should I write a column class that encapsulates a column or is this too much abstraction?
        // TODO:  Should we restructure the Matrix class to be a single list of complex numbers?

        /// <summary>
        /// Each Matrix is represented as a list of lists of type T numbers.
        /// Each list of type T numbers represents a row of the Matrix,
        /// i.e., the Matrix is stored in row major form
        /// </summary>
        private double[] _elements;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a 0x0 null Matrix with no content
        /// </summary>
        public Vector(int n)
        {
            Length = n;
            _elements = new double[n];
        }

        public Vector(double[] elements)
        {
            Length = elements.Length;
            _elements = elements;
        }
        public Vector(string VectorString)
        {
            string[] elements = VectorString.Split(';');
            elements[0] = elements[0].TrimStart('[');
            elements[elements.Length - 1] = elements[elements.Length - 1].TrimEnd(']');
            double[] dElem;
            //NumElem = elements.Length;
            //int rowNumber = 0;

            //T[NumElem] _elements = new N;

            dElem = Array.ConvertAll(elements, new Converter<string, double>(Double.Parse));
            Length = dElem.Length;
            _elements = dElem;
        }

        public Vector(SerializationInfo info, StreamingContext context)
         {
             Length = info.GetInt32("Length");
             _elements = (double[])info.GetValue("_elements", typeof(double[]));
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
                    s += element.ToString() + ";" + " ";


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
            double[] temp = new double[3];
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
            for (int c = 1; c <= 2; c++)
            {
                C[c] = (dynamic)A[c] + B[c];
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

            for (int c = 1; c <= 2; c++)
            {
                C[c] = (dynamic)A[c] + b;
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

            //for (int r = 1; r <= A.NumRows; r++)
            //{
                for (int c = 1; c <= 2; c++)
                {
                    C[c] = (dynamic)A[c] - b;
                }
            //}

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

            //for (int r = 1; r <= A.NumRows; r++)
                for (int c = 1; c <= 2; c++)
                    C[c] = -1 * (dynamic)A[c];

            return C;
        }

        /// <summary>
        /// Returns the Matrix product of two matricies
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Matrix</param>
        /// <returns>C = A * B</returns>
        /// 
        /*
        public static Matrix operator *(Matrix A, Matrix B)
        {
            if (A.NumCols != B.NumRows)
                throw new ArgumentException("Inner Matrix dimensions must agree.");

            Matrix C = new Matrix(A.NumRows, B.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= B.NumCols; c++)
                {
                    C[r, c] = Matrix.Dot(A.GetRow(r), B.GetColumn(c));
                }
            }

            return C;
        }
        */
        /// <summary>
        /// Returns the Matrix product of a Complex and a Matrix (elementwise)
        /// </summary>
        /// <param name="A">Complex</param>
        /// <param name="B">Matrix</param>
        /// <returns>C = A * B</returns>
        public static Vector operator *(double a, Vector B)
        {
            Vector C = new Vector(B.Length);

            //for (int r = 1; r <= B.NumRows; r++)
            //{
                for (int c = 1; c <= 2; c++)
                {
                    C[c] = (dynamic)a * B[c];
                }
            //}

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

            //for (int r = 1; r <= A.NumRows; r++)
            //{
                for (int c = 1; c <= 2; c++)
                {
                    C[c] = (dynamic)A[c] / b;
                }
            //}

            return C;
        }

        /// <summary>
        /// Returns the quotient of a Matrix and a Matrix (elementwise)
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Matrix</param>
        /// <returns>C = A / B</returns>
        /*
        public static Matrix operator /(Matrix A, Matrix B)
        {
            // TODO: Throw exception if A and B are different shape
            Matrix C = new Matrix(A.NumRows, A.NumCols);
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
        /// Returns a boolean indicating if two matricies are equal
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator ==(Vector A, Vector B)
        {
            // if ((A.NumRows != B.NumRows) || (A.NumCols != B.NumCols))
            //    return false;
            //else
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
        /// Converts a Matrix to an array of doubletype numbers
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
        /// In place vertical concatination of this Matrix and Matrix A.
        /// </summary>
        /// <param name="A"></param>
        /*public void Vertcat(Matrix A)
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
        /// In place horizontal concatination of this Matrix and Matrix A.
        /// </summary>
        /// <param name="A"></param>
       /* public void Horzcat(Matrix A)
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
        /// Returns a column of this Matrix
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Matrix GetColumn(int c)
        {
            if (c < 1)
                throw new ArgumentOutOfRangeException("Invalid column number", "Matrix indices must be non-negative");
            else if (c > NumCols)
                throw new ArgumentOutOfRangeException("Invalid column number", "Column index must not exceed size of Matrix");
            else
            {
                Matrix C = new Matrix(NumRows, 1);
                for (int r = 1; r <= NumRows; r++)
                    C[r, 1] = this[r, c];
                return C;
            }
        }
        public void SetColumn(int col, Matrix colData)
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
        /// Returns a row of this Matrix
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Matrix GetRow(int r)
        {
            Matrix C = new Matrix(1, NumCols);
            for (int c = 1; c <= NumCols; c++)
                C[1, c] = this[r, c];
            return C;
        }
        public void SetRow(int row, Matrix rowData)
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
        public void SetValue(int col, double value) ///Morgan Added this
        {
            if( col > 0 && col <= 2)
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
        /*
        public static Matrix Max(Matrix A, Matrix B)
        {
            Matrix C = new Matrix(A.NumRows, A.NumCols);

            int i = 1;
            foreach (T c in A)
            {
                C[i] = (T)System.Math.Max((dynamic)A[i], (dynamic)B[i]);
                i++;
            }

            return C;
        }

        /// <summary>
        /// Returns a Matrix whos elements are the maximum of {A[i,j], b}.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix Max(Matrix A, T b)
        {
            Matrix C = (Matrix)A.Clone();
            int i = 1;
            foreach (T c in A)
            {
                C[i] = (T)System.Math.Max((dynamic)A[i], (dynamic)b);
                i++;
            }

            return C;
        }
        */
        public static double Min(Vector A)
        {
            double min = A[1];
            foreach (double value in A)
            {
                min = System.Math.Min(min, value);
            }
            return min;
        }
        /*
        public static Matrix Min(Matrix A, Matrix B)
        {
            Matrix C = new Matrix(A.NumCols, A.NumRows);

            int i = 1;
            foreach (T c in A)
            {
                C[i] = (T)System.Math.Min((dynamic)A[i], (dynamic)B[i]);
                i++;
            }

            return C;
        }

        /// <summary>
        /// Returns a Matrix whos elements are the minimum of {A[i,j], b}.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix Min(Matrix A, T b)
        {
            Matrix C = (Matrix)A.Clone();
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
                temp += (dynamic)c * c;

            return System.Math.Sqrt((dynamic)temp);

        }

        public static Vector Cumprod(Vector A)
        {
            return Vector.Cumprod(A, 1);
        }

        public static Vector Cumprod(Vector A, int Dim)
        {
            Vector C = (Vector)A.Clone();

                    for (int c = 1; c <= 2; c++)
                    {
                        C[c] *= (dynamic)C[c-1];
                    }
                return C;


        }

        public static implicit operator Matrix<double>(Vector v)
        {
            return new Matrix<double>(v.ToString());
        }

        public static implicit operator Vector(double[] c)
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
        public static explicit operator Vector(Matrix<double> v)
        {
            if (v.NumCols == 1 || v.NumRows == 1)
            {
                return new Vector(v.ToString());
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
                return (dynamic)_elements[index-1];
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
                 _elements[index-1] =  value;
            }
        }

        /// <summary>
        /// Gets or Sets the element of a Matrix based on (row, column) indexing
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        /*public T this[int r, int c]
        {
            get
            {
                if (r < 1 || c < 1)
                    throw new IndexOutOfRangeException("Matrix indices must be non-negative");
                else if (r > NumRows || c > NumCols)
                    throw new IndexOutOfRangeException("Indices must not exceed size of Matrix");
                else
                    return _elements[r - 1][c - 1];
            }
            set
            {
                if (r < 1 || c < 1)
                    throw new IndexOutOfRangeException("Matrix indices must be non-negative");

                if (r > NumRows)
                {
                    for (int i = NumRows; i < r; i++)
                    {
                        _elements.Add(new List(NumCols));

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
        /*public Matrix this[int r, string c]
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
        /// Returns an entire column of a Matrix
        /// </summary>
        /// <param name="r">should be "all" or ":"</param>
        /// <param name="c"></param>
        /// <returns></returns>
        public Matrix this[string r, int c]
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
                    throw new IndexOutOfRangeException("Column - Number of rows in Matrix is not the same as new column");
            }
        }
        
        public Matrix this[string r, MatrixIndex c]
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
                    throw new IndexOutOfRangeException("Column - Number of rows in Matrix is not the same as new column");
            }
        }

        public Matrix this[MatrixIndex r, string c]
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
                    throw new IndexOutOfRangeException("Column - Number of rows in Matrix is not the same as new column");
            }
        }
        */
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


    // TODO:  How to enumerate? element, row or column?
    /*
    public class MatrixColumnEnum : IEnumerator
    {
        public Matrix MatrixData;

        int position = -1;

        public MatrixColumnEnum(Matrix data)
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

