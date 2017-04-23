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
    public class Matrix<T> : ISerializable, IEnumerable
    {
        #region Properties
        /// <summary>
        /// The number of rows in the Matrix<T>
        /// </summary>
        [XmlIgnore]
        public int NumRows { get; set; }

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
                return System.Math.Max(NumCols, NumRows);
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
                return NumCols * NumRows;
            }
        }

        #endregion

        #region Members

        // TODO:  Should I write a column class that encapsulates a column or is this too much abstraction?
        // TODO:  Should we restructure the Matrix<T> class to be a single list of complex numbers?

        /// <summary>
        /// Each Matrix<T> is represented as a list of lists of type T numbers.
        /// Each list of type T numbers represents a row of the Matrix<T>,
        /// i.e., the Matrix<T> is stored in row major form
        /// </summary>
        private List<List<T>> _elements;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a 0x0 null Matrix<T> with no content
        /// </summary>
        public Matrix()
        {
            NumCols = 0;
            NumRows = 0;
            _elements = new List<List<T>>(0);
        }

        /// <summary>
        /// Initializes an r by c Matrix<T>.  The Matrix<T> is filled with zeros
        /// </summary>
        /// <param name="r">The number of rows for the Matrix<T></param>
        /// <param name="c">The number of columns for the Matrix<T></param>
        public Matrix(int r, int c)
        {
            NumCols = c;
            NumRows = r;

            _elements = new Matrix<T>(r, c, (T)Convert.ChangeType(0, typeof(T)))._elements;
        }
        /// <summary>
        /// Initializes an r by c Matrix<T>.  The Matrix<T> is filled with the Complex number given by 'value'.
        /// </summary>
        /// <param name="r">The number of rows for the Matrix<T></param>
        /// <param name="c">The number of columns for the Matrix<T></param>
        /// <param name="Value">The complex number used to fill the Matrix<T></param>
        public Matrix(int r, int c, T Value)
        {
            NumCols = c;
            NumRows = r;

            _elements = new List<List<T>>(r);

            for (int i = 0; i < r; i++)
            {
                _elements.Add(new List<T>(c));

                for (int j = 0; j < c; j++)
                    _elements[i].Add(Value);
            }
        }

        /// <summary>
        /// Initializes a square Matrix<T> with n rows and columns.  The Matrix<T> is filled with zeros
        /// </summary>
        /// <param name="n">The number of rows and columns for the Matrix<T></param>
        public Matrix(int n)
        {
            NumCols = n;
            NumRows = n;
            _elements = new Matrix<T>(n, n, (T)Convert.ChangeType(0, typeof(T)))._elements;
        }

        /// <summary>
        /// Initializes a Matrix<T> based on an array of T type numbers.
        /// If the array is null, the zero by zero null Matrix<T> is created
        /// </summary>
        /// <param name="elements">The type T array used to initialize the Matrix<T></param>
        public Matrix(T[,] elements)
        {
            if (elements == null)
            {
                NumCols = 0;
                NumRows = 0;
                _elements = new List<List<T>>(0);
            }
            else
            {
                NumRows = elements.GetLength(0);
                NumCols = elements.GetLength(1);

                _elements = new List<List<T>>(NumRows);

                for (int i = 0; i < NumRows; i++)
                {
                    _elements.Add(new List<T>(NumCols));

                    for (int j = 0; j < NumCols; j++)
                        _elements[i].Add((T)Convert.ChangeType(elements[i, j], typeof(T)));

                }
            }
        }

        /// <summary>
        /// Initializes a Matrix<T> based on the input Matrix<T> A.
        /// The new Matrix<T> is the same size as A with each element filled with zeros
        /// </summary>
        /// <param name="A"></param>
        public Matrix(MatrixSize A)
        {
            int r = A.NumRows;
            int c = A.NumColumns;

            NumCols = c;
            NumRows = r;

            _elements = new Matrix<T>(A.NumRows, A.NumColumns)._elements;

        }

        /// <summary>
        /// Creates a new Matrix<double> from a string representing the Matrix<double>
        /// </summary>
        /// <param name="MatrixString">A string representing the Matrix<double> elements.</param>
        public Matrix(string MatrixString)
        {
            string[] rows = MatrixString.Split(';');
            rows[0] = rows[0].TrimStart('[');
            rows[rows.Length-1] = rows[rows.Length - 1].TrimEnd(']');
            double[] dRows;
            NumRows = rows.Length;
            int rowNumber = 0;

            _elements = new List<List<T>>(NumRows);
            foreach (string row in rows)
            {
                dRows = Array.ConvertAll(row.Split(','), new Converter<string, double>(Double.Parse));
                NumCols = dRows.Length;

                _elements.Add(new List<T>(NumCols));

                for (int j = 0; j < NumCols; j++)
                    _elements[rowNumber].Add((T)Convert.ChangeType(dRows[j], typeof(T)));

                rowNumber++;
            }
              
        }

        public Matrix(XmlNode matrixXmlNode)
        {
            NumRows = Convert.ToInt32(matrixXmlNode.Attributes["NumRows"].Value);
            NumCols = Convert.ToInt32(matrixXmlNode.Attributes["NumCols"].Value);
            string[] elementStrings = matrixXmlNode.Attributes["_elements"].Value.Split(',');

            _elements = new List<List<T>>(NumRows);
            for (int r = 0; r < NumRows; r++)
            {
                _elements.Add(new List<T>(NumCols));
                for (int c = 0; c < NumCols; c++)
                    _elements[r].Add((T)Convert.ChangeType(elementStrings[c + r * NumCols], typeof(T)));
            }

        }

        public Matrix(SerializationInfo info, StreamingContext context)
        {
            NumCols = info.GetInt32("NumCols");
            NumRows = info.GetInt32("NumRows");
            _elements = (List<List<T>>)info.GetValue("_elements", typeof(List<List<T>>));
        }
        #endregion

        #region Overrrides
        // TOD) (Eric):  This should match the constructor based on a string...
        /// <summary>
        /// Converts a Matrix<T> to a string
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            string s = "[";

            foreach(List<T> row in _elements)
            {
                foreach (T element in row)
                    s += element.ToString() + "," + " ";
                s = s.Substring(0, s.Length - 2) + "; ";
            }

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
        /// Gets the hash code of a Matrix<T> based on the ToString() method
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
        public static T Dot(Matrix<T> a, Matrix<T> b)
        {
            if (!a.IsVector() || !b.IsVector())
                throw new ArgumentException("Arguments of the dot product must to be vectors.");
            else if (a.Length != b.Length)
                throw new ArgumentException("Vectors must be of the same length.");

            T buf = (T)Convert.ChangeType(0, typeof(T));

            for (int i = 1; i <= a.Length; i++)
            {
                buf += (dynamic)a[i] * b[i];
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
        public static Matrix<T> Cross(Matrix<T> a, Matrix<T> b)
        {
            if (!a.IsVector() || !b.IsVector())
                throw new ArgumentException("Arguments of the cross product must to be 3x1 vectors.");
            else if (a.Length != 3 && b.Length != 3)
                throw new ArgumentException("Arguments of the cross product must to be 3x1 vectors.");

            T[,] temp = new T[1, 3];
            temp[0, 0] = (dynamic)a[2] * b[3] - (dynamic)a[3] * b[2];
            temp[0, 1] = -(dynamic)a[1] * b[3] + (dynamic)a[3] * b[1];
            temp[0, 2] = (dynamic)a[1] * b[2] - (dynamic)a[2] * b[1];

            Matrix<T> c = new Matrix<T>(temp);
            if (a.NumCols == 3)
                return c;
            else
                return Transpose(c);
        }

        /// <summary>
        /// Returns the Matrix<T> sum of two matrices
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix<T> operator +(Matrix<T> A, Matrix<T> B)
        {
            if (A.Size != B.Size)
                throw new ArgumentException("Matrices must be the same dimension when adding.");

            Matrix<T> C = new Matrix<T>(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= A.NumCols; c++)
                {
                    C[r, c] = (dynamic)A[r, c] + B[r, c];
                }
            }

            return C;
        }

        /// <summary>
        /// Returns the Matrix<T> sum of a Matrix<T> and a complex.
        /// The Complex number is added to each element of the Matrix<T>
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix<T> operator +(Matrix<T> A, T b)
        {
            Matrix<T> C = new Matrix<T>(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= A.NumCols; c++)
                {
                    C[r, c] = (dynamic)A[r, c] + b;
                }
            }

            return C;
        }

        /// <summary>
        /// Retruns the Matrix<T> sum of a Complex and a Matrix<T>
        /// The Complex number is added to each element of the Matrix<T>
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix<T> operator +(T a, Matrix<T> B)
        {
            Matrix<T> C = new Matrix<T>(B.NumRows, B.NumCols);

            C = B + a;

            return C;
        }

        /// <summary>
        /// Returns the Matrix<T> difference of two matrices
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix<T> operator -(Matrix<T> A, Matrix<T> B)
        {
            if (A.Size != B.Size)
                throw new ArgumentException("Matrices must be the same dimension when subtracting.");

            Matrix<T> C = A + (-B);

            return C;
        }

        /// <summary>
        /// Returns the Matrix<T> difference of a Matrix<T> and a Complex
        /// The Complex number is subtracted from each element of the Matrix<T>
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix<T> operator -(Matrix<T> A, T b)
        {
            Matrix<T> C = new Matrix<T>(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= A.NumCols; c++)
                {
                    C[r, c] = (dynamic)A[r, c] - b;
                }
            }

            return C;
        }

        /// <summary>
        /// Retruns the Matrix<T> difference of a Complex and a Matrix<T>
        /// The Complex number is subtracted from each element of the Matrix<T>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="B"></param>
        /// <returns>Matrix<T></returns>
        public static Matrix<T> operator -(T a, Matrix<T> B)
        {
            Matrix<T> C = new Matrix<T>(B.NumRows, B.NumCols);

            C = -B + a;

            return C;
        }

        /// <summary>
        /// Returns the negative of a Matrix<T>
        /// </summary>
        /// <param name="A"></param>
        /// <returns>Matrix<T></returns>
        public static Matrix<T> operator -(Matrix<T> A)
        {
            Matrix<T> C = new Matrix<T>(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
                for (int c = 1; c <= A.NumCols; c++)
                    C[r, c] = -1 * (dynamic)A[r, c];

            return C;
        }

        /// <summary>
        /// Returns the Matrix<T> product of two matricies
        /// </summary>
        /// <param name="A">Matrix<T></param>
        /// <param name="B">Matrix<T></param>
        /// <returns>C = A * B</returns>
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

        /// <summary>
        /// Returns the Matrix<T> product of a Complex and a Matrix<T> (elementwise)
        /// </summary>
        /// <param name="A">Complex</param>
        /// <param name="B">Matrix<T></param>
        /// <returns>C = A * B</returns>
        public static Matrix<T> operator *(T a, Matrix<T> B)
        {
            Matrix<T> C = new Matrix<T>(B.NumRows, B.NumCols);

            for (int r = 1; r <= B.NumRows; r++)
            {
                for (int c = 1; c <= B.NumCols; c++)
                {
                    C[r, c] = (dynamic)a * B[r, c];
                }
            }

            return C;
        }

        /// <summary>
        /// Returns the Matrix<T> product of a Matric and a Complex (elementwise)
        /// </summary>
        /// <param name="A">Matrix<T></param>
        /// <param name="B">Complex</param>
        /// <returns>C = A * B</returns>
        public static Matrix<T> operator *(Matrix<T> A, T b)
        {
            Matrix<T> C = new Matrix<T>(A.NumRows, A.NumCols);

            C = b * A;

            return C;
        }

        /// <summary>
        /// Returns the quotient of a Matrix<T> and a complex (elementwise)
        /// </summary>
        /// <param name="A">Matrix<T></param>
        /// <param name="B">Complex</param>
        /// <returns>C = A / B</returns>
        public static Matrix<T> operator /(Matrix<T> A, T b)
        {
            Matrix<T> C = new Matrix<T>(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= A.NumCols; c++)
                {
                    C[r, c] = (dynamic)A[r, c] / b;
                }
            }

            return C;
        }

        /// <summary>
        /// Returns the quotient of a Matrix<T> and a Matrix<T> (elementwise)
        /// </summary>
        /// <param name="A">Matrix<T></param>
        /// <param name="B">Matrix<T></param>
        /// <returns>C = A / B</returns>
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

        /// <summary>
        /// Returns a boolean indicating if two matricies are equal
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator ==(Matrix<T> A, Matrix<T> B)
        {
            if ((A.NumRows != B.NumRows) || (A.NumCols != B.NumCols))
                return false;
            else
                for (int i = 1; i <= A.NumElements; i++)
                    if ((dynamic)A[i] != B[i])
                        return false;
            return true;
        }

        /// <summary>
        /// Returns a boolean indicating if two matricies are not equal
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator !=(Matrix<T> A, Matrix<T> B)
        {
            return !(A == B);
        }

        //TODO: Figure out computer precision (?) issues
        public static Matrix<double> exp(Matrix<double> A)
        {

            double y = Math.Log(Matrix<double>.Norm(A, "inf"), 2);
            int e = (int)y+1;
            //double f = frexp(y, out e);
            int s = Math.Max(0, e + 1);
            A = A / Math.Pow(2, s); // Doesn't match matlab

            //Matrix<double> X = (Matrix<double>)A.Clone();
            Matrix<double> X = A;
            double c = 0.5;
            Matrix<double> E = Matrix<double>.Eye(A.NumRows) + c * A;
            Matrix<double> D = Matrix<double>.Eye(A.NumRows) - c * A;
            int q = 6;
            bool p = true;
            for (int k = 2; k <= q; k++)
            {
                c = c * (q - k + 1) / (k * (2 * q - k + 1));
                X = A * X;
                Matrix<double> cX = c * X;
                E = E + cX;
                if (p)
                   D = D + cX;
                else
                    D = D - cX;
                p = !p;
            }
            E = Matrix<double>.Inverse(D)*E;
            for (int k = 0; k < s; k++)
            {
                E = E*E;
            }

            return E;

            throw new NotImplementedException();
        }
        
        #endregion

            #region Dynamics

            /// <summary>
            /// Converts a Matrix<T> to an array of T type numbers
            /// </summary>
            /// <returns></returns>
        public T[,] ToArray()
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

        public bool IsSquare()
        {
            return (NumRows == NumCols);
        }

        public bool IsNull()
        {
            return (NumCols == 0 && NumRows == 0);
        }

        /// <summary>
        /// In place vertical concatination of this Matrix<T> and Matrix<T> A.
        /// </summary>
        /// <param name="A"></param>
        public void Vertcat(Matrix<T> A)
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

        /// <summary>
        /// In place horizontal concatination of this Matrix<T> and Matrix<T> A.
        /// </summary>
        /// <param name="A"></param>
        public void Horzcat(Matrix<T> A)
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

        public void SetValue(int row, int col, T value) ///Morgan Added this
        {
            if(row > 0 && row <=NumRows && col > 0 && col <= NumCols)
                this[row, col] = value; // why do we access it like this?
            else
                throw new ArgumentException("Element indicies out of Matrix<T> bounds");
        }


        #endregion

        #region Statics


        public static Matrix<double> Inverse(Matrix<double> A)
        {
            int n = A.NumCols;
            if (!A.IsSquare())
                throw new ArgumentException("The matrix must be square to invert.");
            Matrix<double> I = (Matrix<double>)Convert.ChangeType(Eye(A.NumCols), typeof(Matrix<double>));
            Matrix<double> B = Matrix<double>.Horzcat(A, I);

            Matrix<double> temp = new Matrix<double>(B.NumCols);

            for (int k = 1; k <= B.NumRows; k++)
            {
                //Find the kth pivot:
                int row_max = 1;
                double maxval = 0;
                for (int ii = k; ii <= B.NumRows; ii++)
                {
                    if (Math.Abs(B[ii, k]) > maxval)
                    {
                        maxval = Math.Abs(B[ii, k]);
                        row_max = ii;
                    }
                }

                //if (B[i_max, k] == 0)
                    //throw new ArgumentException("Matrix is singular.");

                temp = B.GetRow(k);
                B.SetRow(k, B.GetRow(row_max));
                B.SetRow(row_max, temp);



                // Do for all rows below pivot:
                for (int i = k+1 ; i <= B.NumRows; i++)
                {
                    double f = B[i, k] / B[k, k];
                    //double g = B[i - 1, k] / B[k, k];
                    //Do for all remaining elements in current row:
                    for (int j = k+1; j <= B.NumCols; j++)
                    {
                        B[i, j] = B[i, j] - B[k, j] * f;
                        //B[i-1,j] = B[i-1,j] - B[k, j] * g;

                    }
                    //Fill lower triangular matrix with zeros:
                    B[i, k] = 0;
                }
                if (B[k, k] < 0) //Make the row the will next be calulated have a positive value for a leading 1
                {
                    B.SetRow(k, B.GetRow(k) * -1);
                }

            }

            for (int row = 1; row < B.NumRows; row++)
            {
                // Divide by a constant to get 1 in first column
                double Bk = B[row, row];
                for (int col = row; col <= B.NumCols; col++)
                {
                    B[row, col] = B[row, col] / Bk;
                }
            }


            for (int m = B.NumRows; m > 1; m--)
            {
                for(int r = m; r>1; r--) 
                {
                    B.SetRow(r-1, (B[r - 1, m] * B.GetRow(m)) - B.GetRow(r-1)); // Zero out all values above matrix's main diagonal

                }
                if (B[m-1, m-1] < 0) //Make the row the will next be calulated have a positive value for a leading 1
                {
                    B.SetRow(m-1, B.GetRow(m-1) * -1);
                }

            }
            return B[new MatrixIndex(1, n), new MatrixIndex(n+1, B.NumCols)];
        }


        // TODO:  SHould these get/set functions be static?
        public static Matrix<T> GetColumn(Matrix<T> A, int column)
        {
            return A.GetColumn(column);
        }

        public static void SetColumn(Matrix<T> A, int column, Matrix<T> columnData)
        {
            A.SetColumn(column, columnData);
        }

        /// <summary>
        /// Returns a row of a Matrix<T>
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Matrix<T> GetRow(Matrix<T> A, int row)
        {
            return A.GetRow(row);
        }

        public static void SetRow(Matrix<T> A, int row, Matrix<T> rowData)
        {
            A.SetRow(row, rowData);
        }

        /// <summary>
        /// Vertical Concatination of two matrices.  Neither Matrix<T> is modified.  A new Matrix<T> is returned.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix<T> Vertcat(Matrix<T> A, Matrix<T> B)
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

        /// <summary>
        /// Returns a row vector containing the maximum of the elements of each column.
        /// If A is a row vector, the maximum element is returned.
        /// If A is a column vector, the maximum elelment is returned
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static Matrix<T> Max(Matrix<T> A)
        {
            if (A.IsColumnVector() || A.IsRowVector())
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
        }

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
        public static Matrix<T> Min(Matrix<T> A)
        {
            if (A.IsColumnVector() || A.IsRowVector())
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
        }

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static Matrix<T> Abs(Matrix<T> A)
        {
            Matrix<T> R = (Matrix<T>)A.Clone();
            int i = 1;
            foreach (T c in A)
            {
                R[i] = (T)System.Math.Abs((dynamic)c);
                i++;
            }

            return R;
        }

        public static double Norm(Matrix<T> A)
        {
            // TODO:  Handle the case when c is complex
            if (A.IsRowVector() || A.IsColumnVector())
            {
                T temp = (T)Convert.ChangeType(0, typeof(T));
                foreach (T c in A)
                    temp += (dynamic)c * c;

                return System.Math.Sqrt((dynamic)temp);
            }
            else
                throw new NotImplementedException("Matrix<T>.Norm(Matrix<T> A)");
        }
        public static T Norm(Matrix<T> A, string type)
        {
            // TODO:  Handle the case when c is complex
            if (type == "inf")
            {
                Matrix<T> temp = (Matrix<T>)Convert.ChangeType(new Matrix<T>(A.NumRows), typeof(Matrix<T>));

                temp = CumSum(Abs(A), 1);

                return Max(temp)[1]; 
            }
            else
                throw new NotImplementedException("Matrix<T>.Norm(Matrix<T> A, " + type + ")");
        }

        public static Matrix<T> CumSum(Matrix<T> A, int Dim)
        {
            Matrix<T> C = new Matrix<T>(A.NumRows, 1);

            if (Dim == 1)
            {
                for (int r = 1; r <= A.NumRows; r++)
                {
                    for (int c = 1; c <= A.NumCols; c++)
                    {
                        C[r] += (dynamic)A[r, c];
                    }
                }
                return C;
            }
            else if (Dim == 2)
            {
                C = Matrix<T>.CumSum(Matrix<T>.Transpose(A), 1);
                C = Matrix<T>.Transpose(C);
                return C;
            }

            else
                throw new NotImplementedException("Matrix<T>.Cumprod(Matrix<T> A, int Dim - Cumprod not implimented for Dim >= 3");

        }


        public static Matrix<T> Cumprod(Matrix<T> A)
        {
            return Matrix<T>.Cumprod(A, 1);
        }

        public static Matrix<T> Cumprod(Matrix<T> A, int Dim)
        {
            Matrix<T> C = (Matrix<T>)A.Clone();

            if (Dim == 1)
            {
                for (int r = 2; r <= A.NumRows; r++)
                {
                    for (int c = 1; c <= A.NumCols; c++)
                    {
                        C[r, c] *= (dynamic)C[r - 1, c];
                    }
                }
                return C;
            }
            else if (Dim == 2)
            {
                C = Matrix<T>.Cumprod(Matrix<T>.Transpose(A));
                C = Matrix<T>.Transpose(C);
                return C;
            }

            else
                throw new NotImplementedException("Matrix<T>.Cumprod(Matrix<T> A, int Dim - Cumprod not implimented for Dim >= 3");

        }


        public static implicit operator Matrix<T>(T c)
        {
            return new Matrix<T>(1, 1, c);
        }

        public static explicit operator T(Matrix<T> m)
        {
            if (m.NumElements == 1)
                return m[1, 1];
            else
                throw new NotImplementedException("explicit operator T(Matrix<T> m) - Conversion from N by M Matrix<T> to double not possible when N, M > 1");
        }

        #endregion

        #region Interfaces

        public object Clone() // ICloneable
        {
            Matrix<T> m = new Matrix<T>(ToArray());
            return m;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue("NumRows", NumRows);
            info.AddValue("NumCols", NumCols);
            //info.AddValue("Length", this.Length);
            //info.AddValue("Values", this.elements[1][1]);
            info.AddValue("_elements", _elements, typeof(List<List<T>>));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MatrixEnum<T>(this);
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
                if (NumRows == 1) // row vector
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
            }
            set
            {
                if (NumRows == 1) // row vector
                    this[1, index] = value;
                else if (NumCols == 1) // coumn vector
                    this[index, 1] = value;
                else
                {
                    int ind = index - 1;
                    int c = ind / NumRows;
                    int r = ind - NumRows * c;
                    this[r + 1, c + 1] = value;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the element of a Matrix<T> based on (row, column) indexing
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public T this[int r, int c]
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

        /// <summary>
        /// Returns an entire row of a matirx
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c">Should be "all" or ":"</param>
        /// <returns></returns>
        public Matrix<T> this[int r, string c]
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
        }

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
        /// <summary>
        /// Returns the elements of a Matrix<T> using the Matrix<T> Index object
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public Matrix<T> this[MatrixIndex rows, MatrixIndex cols]
        {
            get
            {
                Matrix<T> C = new Matrix<T>(rows.Length, cols.Length);

                for (int r = 1; r <= rows.Length; r++)
                {
                    for (int c = 1; c <= cols.Length; c++)
                        C[r, c] = this[rows[r], cols[c]];
                }

                return C;
            }
            set
            {
                for (int r = 1; r <= rows.Length; r++)
                {
                    for (int c = 1; c <= cols.Length; c++)
                        this[rows[r], cols[c]] = value[r, c];
                }
            }
        }

        #endregion
    }

    public class MatrixEnum<T> : IEnumerator
    {
        public Matrix<T> MatrixData;

        int position = -1;

        public MatrixEnum(Matrix<T> data)
        {
            MatrixData = data;
        }

        public bool MoveNext()
        {
            position++;
            return (position < MatrixData.NumElements);
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
    public class MatrixSize
    {
        public int NumRows { get; set; }
        public int NumColumns { get; set; }

        public MatrixSize(int r, int c)
        {
            NumRows = r;
            NumColumns = c;
        }
    }
}

