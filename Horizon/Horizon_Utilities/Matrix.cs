using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Horizon_Utilities
{
    [Serializable]
    public class Matrix : ISerializable, IEnumerable
    {
        #region Properties
        /// <summary>
        /// The number of rows in the matrix
        /// </summary>
        [XmlIgnore]
        public int NumRows { get; set; }

        /// <summary>
        /// The number of columns in the matrix
        /// </summary>
        [XmlIgnore]
        public int NumCols { get; set; }

        /// <summary>
        /// Gets the size of the matrix (rows by columns) in the elements of a matrix [r, c]
        /// </summary>
        [XmlIgnore]
        public Matrix Size
        {
            get
            {
                return new Matrix(new Complex[1, 2] { { new Complex((double)this.NumRows), new Complex((double)this.NumCols) } });
            }
        }

        /// <summary>
        /// Returns the maximum of the number of rows or columns of the matrix
        /// </summary>
        [XmlIgnore]
        public int Length
        {
            get
            {
                return Math.Max(this.NumCols, this.NumRows);
            }
        }

        /// <summary>
        /// Returns the total number of elements in the matrix, r*c
        /// </summary>
        [XmlIgnore]
        public int NumElements
        {
            get
            {
                return this.NumCols * this.NumRows;
            }
        }

        #endregion

        #region Members

        // TODO:  Should I write a column class that encapsulates a column or is this too much abstraction?
        // TODO:  Should we restructure the matrix class to be a single list of complex numbers?

        /// <summary>
        /// Each matrix is represented as a list of lists of complex numbers.
        /// Each list of complex numbers represents a row of the matrix,
        /// i.e., the matrix is stored in row major form
        /// </summary>
        private List<List<Complex>> elements;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a 0x0 null matrix with no content
        /// </summary>
        public Matrix()
        {
            this.NumCols = 0;
            this.NumRows = 0;
            this.elements = new List<List<Complex>>(0);
        }

        /// <summary>
        /// Initializes an r by c matrix.  The matrix is filled with zeros
        /// </summary>
        /// <param name="r">The number of rows for the matrix</param>
        /// <param name="c">The number of columns for the matrix</param>
        public Matrix(int r, int c)
        {
            this.NumCols = c;
            this.NumRows = r;

            this.elements = new List<List<Complex>>(r);

            for (int i = 0; i < r; i++)
            {
                this.elements.Add(new List<Complex>(c));

                for (int j = 0; j < c; j++)
                    ((List<Complex>)this.elements[i]).Add(Complex.Zero);
            }
        }
        /// <summary>
        /// Initializes an r by c matrix.  The matrix is filled with the Complex number given by 'value'.
        /// </summary>
        /// <param name="r">The number of rows for the matrix</param>
        /// <param name="c">The number of columns for the matrix</param>
        /// <param name="Value">The complex number used to fill the matrix</param>
        public Matrix(int r, int c, Complex Value)
        {
            this.NumCols = c;
            this.NumRows = r;

            this.elements = new List<List<Complex>>(r);

            for (int i = 0; i < r; i++)
            {
                this.elements.Add(new List<Complex>(c));

                for (int j = 0; j < c; j++)
                    ((List<Complex>)this.elements[i]).Add(Value);
            }
        }
        /// <summary>
        /// Initializes a square matrix with n rows and columns.  The matrix is filled with zeros
        /// </summary>
        /// <param name="n">The number of rows and columns for the matrix</param>
        public Matrix(int n)
        {
            this.NumCols = n;
            this.NumRows = n;

            this.elements = new List<List<Complex>>(n);

            for (int i = 0; i < n; i++)
            {
                this.elements.Add(new List<Complex>(n));

                for (int j = 0; j < n; j++)
                    ((List<Complex>)this.elements[i]).Add(Complex.Zero);
            }
        }

        /// <summary>
        /// Initializes a matrix based on an array of complex numbers.
        /// If the array is null, the zero by zero null matrix is created
        /// </summary>
        /// <param name="elements">The complex array used to initialize the matrix</param>
        public Matrix(Complex[,] elements)
        {
            if (elements == null)
            {
                this.NumCols = 0;
                this.NumRows = 0;
                this.elements = new List<List<Complex>>(0);
            }
            else
            {
                this.NumRows = (int)elements.GetLongLength(0);
                this.NumCols = (int)elements.GetLongLength(1);

                this.elements = new List<List<Complex>>(this.NumRows);

                for (int i = 0; i < this.NumRows; i++)
                {
                    this.elements.Add(new List<Complex>(this.NumCols));

                    for (int j = 0; j < this.NumCols; j++)
                        ((List<Complex>)this.elements[i]).Add(elements[i, j]);
                }
            }
        }

        // TODO:  Handle this constructor with an implicit conversion from double{,] to Complex[,] in Complex.cs
        /// <summary>
        /// Initializes a matrix based on an array of double numbers.
        /// If the array is null, the zero by zero null matrix is created
        /// </summary>
        /// <param name="elements">The double array used to initialize the matrix</param>

        public Matrix(double[,] elements)
        {
            if (elements == null)
            {
                this.NumCols = 0;
                this.NumRows = 0;
                this.elements = new List<List<Complex>>(0);
            }
            else
            {
                this.NumRows = (int)elements.GetLongLength(0);
                this.NumCols = (int)elements.GetLongLength(1);

                this.elements = new List<List<Complex>>(this.NumRows);

                for (int i = 0; i < this.NumRows; i++)
                {
                    this.elements.Add(new List<Complex>(this.NumCols));

                    for (int j = 0; j < this.NumCols; j++)
                        ((List<Complex>)this.elements[i]).Add(new Complex(elements[i, j]));
                }
            }
        }

        /// <summary>
        /// Initializes a matrix based on the input matrix A.
        /// The new matrix is the same size as A with each element filled with zeros
        /// </summary>
        /// <param name="A"></param>
        public Matrix(MatrixSize A)
        {
            int r = A.NumRows;
            int c = A.NumColumns;

            this.NumCols = c;
            this.NumRows = r;

            this.elements = new List<List<Complex>>(r);

            for (int i = 0; i < r; i++)
            {
                this.elements.Add(new List<Complex>(c));

                for (int j = 0; j < c; j++)
                    ((List<Complex>)this.elements[i]).Add(Complex.Zero);
            }
        }

        #endregion

        #region Overrrides

        /// <summary>
        /// Converts a matrix to a string
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            string s = Environment.NewLine + Environment.NewLine;

            foreach (List<Complex> row in this.elements)
            {
                foreach (Complex element in row)
                    s += element.ToString() + ", ";
                s = s.Substring(0, s.Length - 2) + ";" + Environment.NewLine;
            }

            s = s.Substring(0, s.Length - 3) + Environment.NewLine;
            return s;
        }

        /// <summary>
        /// Determines if two matricies are equal by comparing ToString() method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool</returns>
        public override bool Equals(object obj)
        {
            return this.ToString() == obj.ToString();
        }

        /// <summary>
        /// Gets the hash code of a Matrix based on the ToString() method
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        #endregion

        #region Operators

        /// <summary>
        /// Returns the inner product (or Dot Product) of two n by 1 matrices (or vectors)
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public static Complex Dot(Matrix a, Matrix b)
        {
            if (!a.IsVector() || !b.IsVector())
                throw new ArgumentException("Arguments of the dot product must to be vectors.");
            else if (a.Length != b.Length)
                throw new ArgumentException("Vectors must be of the same length.");

            Complex buf = Complex.Zero;

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
        public static Matrix Cross(Matrix a, Matrix b)
        {
            if (!a.IsVector() || !b.IsVector())
                throw new ArgumentException("Arguments of the cross product must to be 3x1 vectors.");
            else if (a.Length != 3 && b.Length != 3)
                throw new ArgumentException("Arguments of the cross product must to be 3x1 vectors.");

            Complex[,] temp = new Complex[1, 3];
            temp[0, 0] = a[2] * b[3] - a[3] * b[2];
            temp[0, 1] = a[1] * b[3] - a[3] * b[1];
            temp[0, 2] = a[1] * b[2] - a[2] * b[1];

            Matrix c = new Matrix(temp);
            if (a.NumCols == 3)
                return c;
            else
                return Matrix.Transpose(c);
        }

        /// <summary>
        /// Returns the matrix sum of two matrices
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix operator +(Matrix A, Matrix B)
        {
            if (A.Size != B.Size)
                throw new ArgumentException("Matrices must be the same dimension when adding.");

            Matrix C = new Matrix(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= A.NumCols; c++)
                {
                    C[r, c] = A[r, c] + B[r, c];
                }
            }

            return C;
        }

        /// <summary>
        /// Returns the matrix sum of a matrix and a complex.
        /// The Complex number is added to each element of the matrix
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix operator +(Matrix A, Complex b)
        {
            Matrix C = new Matrix(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= A.NumCols; c++)
                {
                    C[r, c] = A[r, c] + b;
                }
            }

            return C;
        }

        /// <summary>
        /// Retruns the matrix sum of a Complex and a matrix
        /// The Complex number is added to each element of the matrix
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix operator +(Complex a, Matrix B)
        {
            Matrix C = new Matrix(B.NumRows, B.NumCols);

            for (int r = 1; r <= B.NumRows; r++)
            {
                for (int c = 1; c <= B.NumCols; c++)
                {
                    C[r, c] = a + B[r, c];
                }
            }

            return C;
        }

        /// <summary>
        /// Returns the matrix difference of two matrices
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix operator -(Matrix A, Matrix B)
        {
            if (A.Size != B.Size)
                throw new ArgumentException("Matrices must be the same dimension when subtracting.");

            Matrix C = new Matrix(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= A.NumCols; c++)
                {
                    C[r, c] = A[r, c] - B[r, c];
                }
            }

            return C;
        }

        /// <summary>
        /// Returns the matrix difference of a matrix and a Complex
        /// The Complex number is subtracted from each element of the matrix
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix operator -(Matrix A, Complex b)
        {
            Matrix C = new Matrix(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= A.NumCols; c++)
                {
                    C[r, c] = A[r, c] - b;
                }
            }

            return C;
        }

        /// <summary>
        /// Retruns the matrix difference of a Complex and a matrix
        /// The Complex number is subtracted from each element of the matrix
        /// </summary>
        /// <param name="a"></param>
        /// <param name="B"></param>
        /// <returns>Matrix</returns>
        public static Matrix operator -(Complex a, Matrix B)
        {
            Matrix C = new Matrix(B.NumRows, B.NumCols);

            for (int r = 1; r <= B.NumRows; r++)
                for (int c = 1; c <= B.NumCols; c++)
                    C[r, c] = a - B[r, c];

            return C;
        }

        /// <summary>
        /// Returns the negative of a Matrix
        /// </summary>
        /// <param name="A"></param>
        /// <returns>Matrix</returns>
        public static Matrix operator -(Matrix A)
        {
            Matrix C = new Matrix(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
                for (int c = 1; c <= A.NumCols; c++)
                    C[r, c] = -A[r, c];

            return C;
        }

        /// <summary>
        /// Returns the Matrix product of two matricies
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Matrix</param>
        /// <returns>C = A * B</returns>
        public static Matrix operator *(Matrix A, Matrix B)
        {
            if (A.NumCols != B.NumRows)
                throw new ArgumentException("Inner matrix dimensions must agree.");

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

        /// <summary>
        /// Returns the Matrix product of a Complex and a Matrix (elementwise)
        /// </summary>
        /// <param name="A">Complex</param>
        /// <param name="B">Matrix</param>
        /// <returns>C = A * B</returns>
        public static Matrix operator *(Complex a, Matrix B)
        {
            Matrix C = new Matrix(B.NumRows, B.NumCols);

            for (int r = 1; r <= B.NumRows; r++)
            {
                for (int c = 1; c <= B.NumCols; c++)
                {
                    C[r, c] = a * B[r, c];
                }
            }

            return C;
        }

        /// <summary>
        /// Returns the Matrix product of a Matric and a Complex (elementwise)
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Complex</param>
        /// <returns>C = A * B</returns>
        public static Matrix operator *(Matrix A, Complex b)
        {
            Matrix C = new Matrix(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= A.NumCols; c++)
                {
                    C[r, c] = A[r, c] * b;
                }
            }

            return C;
        }

        /// <summary>
        /// Returns the quotient of a Matrix and a complex (elementwise)
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Complex</param>
        /// <returns>C = A / B</returns>
        public static Matrix operator /(Matrix A, Complex b)
        {
            Matrix C = new Matrix(A.NumRows, A.NumCols);

            for (int r = 1; r <= A.NumRows; r++)
            {
                for (int c = 1; c <= A.NumCols; c++)
                {
                    C[r, c] = A[r, c] / b;
                }
            }

            return C;
        }

        /// <summary>
        /// Returns the quotient of a Matrix and a Matrix (elementwise)
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Matrix</param>
        /// <returns>C = A / B</returns>
        public static Matrix operator /(Matrix A, Matrix B)
        {
            Matrix C = new Matrix(A.NumRows, A.NumCols);
            int i = 1;

            foreach (Complex c in A)
            {
                C[i] = A[i] / B[i];
                i++;
            }

            return C;
        }

        /// <summary>
        /// Returns a boolean indicating if two matricies are equal
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator ==(Matrix A, Matrix B)
        {
            if ((A.NumRows != B.NumRows) || (A.NumCols != B.NumCols))
                return false;
            else
                for (int i = 1; i <= A.NumElements; i++)
                    if (A[i] != B[i])
                        return false;
            return true;
        }

        /// <summary>
        /// Returns a boolean indicating if two matricies are not equal
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator !=(Matrix A, Matrix B)
        {
            return !(A == B);
        }

        #endregion

        #region Dynamics

        /// <summary>
        /// Converts a Matrix to an array of complex numbers
        /// </summary>
        /// <returns></returns>
        public Complex[,] ToArray()
        {
            Complex[,] complexArray = new Complex[this.NumRows, this.NumCols];

            for (int r = 0; r < this.NumRows; r++)
                for (int c = 0; c < this.NumCols; c++)
                    complexArray[r, c] = this[r + 1, c + 1];

            return complexArray;
        }

        public bool IsVector()
        {
            return (this.NumCols == 1 || this.NumRows == 1);
        }

        public bool IsRowVector()
        {
            return (this.NumRows == 1);
        }

        public bool IsColumnVector()
        {
            return (this.NumCols == 1);
        }

        /// <summary>
        /// Returns true if all elements of the matrix are real
        /// </summary>
        /// <returns></returns>
        public bool IsReal()
        {
            bool isreal = true;

            foreach (List<Complex> row in this.elements)
                foreach (Complex element in row)
                    isreal &= isreal;

            return isreal;
        }

        /// <summary>
        /// Returns true if all elements of the matrix are complex
        /// </summary>
        /// <returns></returns>
        public bool IsComplex()
        {
            return !this.IsReal();
        }

        public bool IsSquare()
        {
            return (this.NumRows == this.NumCols);
        }

        public bool IsNull()
        {
            return (this.NumCols == 0 && this.NumRows == 0);
        }

        /// <summary>
        /// In place vertical concatination of this matrix and matrix A.
        /// </summary>
        /// <param name="A"></param>
        public void Vertcat(Matrix A)
        {
            if (this.NumCols == A.NumCols)
            {
                int origNumRows = this.NumRows;
                for (int i = 1; i <= A.NumRows; i++)
                    this.SetRow(i + origNumRows, A.GetRow(i));
            }
            else
                throw new ArgumentException("Vertical Concatination Requires Column Compatability");

        }

        /// <summary>
        /// In place horizontal concatination of this matrix and matrix A.
        /// </summary>
        /// <param name="A"></param>
        public void Horzcat(Matrix A)
        {
            if (this.NumCols == A.NumCols)
            {
                int origNumCols = this.NumCols;
                for (int i = 1; i <= A.NumCols; i++)
                    this.SetColumn(i + origNumCols, A.GetColumn(i));
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
            else if (c > this.NumCols)
                throw new ArgumentOutOfRangeException("Invalid column number", "Column index must not exceed size of matrix");
            else
            {
                Matrix C = new Matrix(this.NumRows, 1);
                for (int r = 1; r <= this.NumRows; r++)
                    C[r, 1] = this[r, c];
                return C;
            }
        }
        public void SetColumn(int col, Matrix colData)
        {
            if (colData.NumRows == this.NumRows)
            {
                for (int r = 1; r <= this.NumRows; r++)
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
            Matrix C = new Matrix(1, this.NumCols);
            for (int c = 1; c <= this.NumCols; c++)
                C[1, c] = this[r, c];
            return C;
        }
        public void SetRow(int row, Matrix rowData)
        {
            if (rowData.NumCols == this.NumCols)
            {
                for (int c = 1; c <= this.NumCols; c++)
                    this[row, c] = rowData[1, c];
            }
            else
                throw new ArgumentException("Row Size Not Compatable");
        }

        #endregion

        #region Statics

        // TODO:  SHould these get/set functions be static?
        public static Matrix GetColumn(Matrix A, int column)
        {
            return A.GetColumn(column);
        }

        public static void SetColumn(Matrix A, int column, Matrix columnData)
        {
            A.SetColumn(column, columnData);
        }

        /// <summary>
        /// Returns a row of a Matrix
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Matrix GetRow(Matrix A, int row)
        {
            return A.GetRow(row);
        }

        public static void SetRow(Matrix A, int row, Matrix rowData)
        {
            A.SetRow(row, rowData);
        }

        /// <summary>
        /// Vertical Concatination of two matrices.  Neither matrix is modified.  A new matrix is returned.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix Vertcat(Matrix A, Matrix B)
        {
            Matrix C = (Matrix)A.Clone();
            C.Vertcat(B);
            return C;
        }

        /// <summary>
        /// Horizontal Concatination of two matrices.  Neither matrix is modified.  A new matrix is returned.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Matrix Horzcat(Matrix A, Matrix B)
        {
            Matrix C = (Matrix)A.Clone();
            C.Horzcat(B);
            return C;
        }

        public static Matrix Eye(int size)
        {
            Matrix eye = new Matrix(size);

            for (int i = 1; i <= size; i++)
                eye[i, i] = Complex.I;

            return eye;
        }

        /// <summary>
        /// Returns the transpose of a Matrix
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static Matrix Transpose(Matrix A)
        {
            Matrix C = new Matrix(A.NumCols, A.NumRows);

            for (int r = 1; r <= A.NumCols; ++r)
            {
                for (int c = 1; c <= A.NumRows; ++c)
                    C[r, c] = A[c, r];
            }

            return C;
        }

        public static Complex Trace(Matrix A)
        {
            if (A.IsSquare())
            {
                Complex temp = Complex.Zero;
                for (int i = 1; i <= A.NumRows; i++)
                    temp += A[i, i];
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
        public static Matrix Max(Matrix A)
        {
            if (A.IsColumnVector() || A.IsRowVector())
            {
                Complex temp = A[1];
                foreach (Complex c in A)
                    if (c >= temp)
                        temp = c;
                return temp;
            }
            else
            {
                Matrix max = new Matrix(1, A.NumCols);
                for (int col = 1; col <= A.NumCols; col++)
                {
                    Matrix m = A.GetColumn(col);
                    max[1, col] = (Complex)Matrix.Max(m);
                }
                return max;
            }
        }

        public static Matrix Max(Matrix A, Matrix B)
        {
            throw new ArgumentOutOfRangeException("Method not implemented");
        }

        /// <summary>
        /// Returns a matrix whos elements are the maximum of {A[i,j], b}.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix Max(Matrix A, Complex b)
        {
            Matrix C = (Matrix)A.Clone();
            int i = 1;
            foreach (Complex c in A)
            {
                A[i] = Complex.Max(A[i], b);
                i++;
            }

            return A;
        }
        public static Matrix Min(Matrix A)
        {
            if (A.IsColumnVector() || A.IsRowVector())
            {
                Complex temp = A[1];
                foreach (Complex c in A)
                    if (c <= temp)
                        temp = c;
                return temp;
            }
            else
            {
                Matrix max = new Matrix(1, A.NumCols);
                for (int col = 1; col <= A.NumCols; col++)
                {
                    Matrix m = A.GetColumn(col);
                    max[1, col] = (Complex)Matrix.Min(m);
                }
                return max;
            }
        }

        public static Matrix Abs(Matrix A)
        {
            Matrix R = (Matrix)A.Clone();
            int i = 1;
            foreach (Complex c in A)
            {
                R[i] = Complex.Abs(c);
                i++;
            }

            return R;
        }

        public static Double Norm(Matrix A)
        {
            if (A.IsRowVector() || A.IsColumnVector())
            {
                Complex temp = 0;
                foreach (Complex c in A)
                    temp += c * Complex.Conj(c);

                return Math.Sqrt((Double)temp);
            }
            else
                throw new ArgumentOutOfRangeException("Matrix.Size", "Matrix.Norm not implemented for non-sigleton dimension Matrices");
        }

        public static Matrix Cumprod(Matrix A)
        {
            return Matrix.Cumprod(A, 1);
        }

        public static Matrix Cumprod(Matrix A, int Dim)
        {
            Matrix C = (Matrix)A.Clone();

            if (Dim == 1)
            {
                for (int r = 2; r <= A.NumRows; r++)
                {
                    for (int c = 1; c <= A.NumCols; c++)
                    {
                        C[r, c] *= C[r - 1, c];
                    }
                }
                return C;
            }
            else if (Dim == 2)
            {
                C = Matrix.Cumprod(Matrix.Transpose(A));
                C = Matrix.Transpose(C);
                return C;
            }

            else
                throw new ArgumentOutOfRangeException("Matrix.Cumprod", "Cumprod not implimented for Dim >= 3");

        }

        public static implicit operator Matrix(Complex c)
        {
            return new Matrix(new Complex[,] { { c } });
        }

        public static explicit operator Double(Matrix m)
        {
            if (m.NumElements == 1)
                return m[1].Re;
            else
                throw new ArgumentOutOfRangeException("Matrix explicit cast operator to Double", "Conversion from N by M matrix to double not possible when N, M > 1");
        }

        #endregion

        #region Interfaces

        public object Clone() // ICloneable
        {
            Matrix m = new Matrix(this.ToArray());
            return m;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue("NumRows", this.NumRows);
            info.AddValue("NumCols", this.NumCols);
            //info.AddValue("Length", this.Length);
            //info.AddValue("Values", this.elements[1][1]);
            //info.AddValue("Elements", this.ToArray());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MatrixEnum(this);
        }
        #endregion

        #region Accessors
        /// <summary>
        /// Returns the element of a Matrix based on column major form with (1) based indexing
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Complex this[int index]
        {
            get
            {
                if (this.NumRows == 1) // row vector
                    return this[1, index];
                else if (this.NumCols == 1) // coumn vector
                    return this[index, 1];
                else
                {
                    int ind = index - 1;
                    int c = ind / this.NumRows;
                    int r = ind - this.NumRows * c;
                    return this[r + 1, c + 1];
                }
            }
            set
            {
                if (this.NumRows == 1) // row vector
                    this[1, index] = value;
                else if (this.NumCols == 1) // coumn vector
                    this[index, 1] = value;
                else
                {
                    int ind = index - 1;
                    int c = ind / this.NumRows;
                    int r = ind - this.NumRows * c;
                    this[r + 1, c + 1] = value;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the element of a Matrix based on (row, column) indexing
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public Complex this[int r, int c]
        {
            get
            {
                if (r < 1 || c < 1)
                    throw new IndexOutOfRangeException("Matrix indices must be non-negative");
                else if (r > this.NumRows || c > this.NumCols)
                    throw new IndexOutOfRangeException("Indices must not exceed size of matrix");
                else
                    return this.elements[r - 1][c - 1];
            }
            set
            {
                if (r < 1 || c < 1)
                    throw new ArgumentOutOfRangeException("Matrix indices must be non-negative");

                if (r > this.NumRows)
                {
                    for (int i = this.NumRows; i < r; i++)
                    {
                        this.elements.Add(new List<Complex>(this.NumCols));

                        for (int j = 0; j < this.NumCols; j++)
                            this.elements[i].Add(Complex.Zero);
                    }

                    this.NumRows = r;
                }

                if (c > this.NumCols)
                {
                    for (int i = 0; i < this.NumRows; i++)
                    {
                        for (int j = this.NumCols; j < c; j++)
                            this.elements[i].Add(Complex.Zero);
                    }

                    this.NumCols = c;
                }

                this.elements[r - 1][c - 1] = value;
            }
        }

        /// <summary>
        /// Returns an entire row of a matirx
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c">Should be "all" or ":"</param>
        /// <returns></returns>
        public Matrix this[int r, string c]
        {
            get
            {
                if (c.ToUpper() == "ALL" || c.ToUpper() == ":")
                    return this.GetRow(r);
                else
                    throw new ArgumentException("Invalid Column Selection");
            }
            set
            {
                if (c.ToUpper() == "ALL" || c.ToUpper() == ":")
                    this.SetRow(r, value);
                else
                    throw new ArgumentException("Invalid Column Selection");
            }
        }

        /// <summary>
        /// Returns an entire column of a matrix
        /// </summary>
        /// <param name="r">should be "all" or ":"</param>
        /// <param name="c"></param>
        /// <returns></returns>
        public Matrix this[string r, int c]
        {
            get
            {
                if (r.ToUpper() == "ALL" || r.ToUpper() == ":")
                    return this.GetColumn(c);
                else
                    throw new ArgumentOutOfRangeException("Column", "No column selected");
            }
            set
            {
                if (this.NumRows == value.NumRows)
                {
                    if (r.ToUpper() == "ALL" || r.ToUpper() == ":")
                    {
                        for (int row = 1; row <= this.NumRows; row++)
                            this[row, c] = value[row, 1];
                    }
                    else
                        throw new ArgumentOutOfRangeException("Row", "No Row Selected");
                }
                else
                    throw new ArgumentOutOfRangeException("Column", "Number of rows in Matrix is not the same as new column");
            }
        }

        /// <summary>
        /// Returns the elements of a Matrix using the Matrix Index object
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public Matrix this[MatrixIndex rows, MatrixIndex cols]
        {
            get
            {
                Matrix C = new Matrix(rows.Length, cols.Length);

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

    public class MatrixEnum : IEnumerator
    {
        public Matrix matrixData;

        int position = -1;

        public MatrixEnum(Matrix data)
        {
            matrixData = data;
        }

        public bool MoveNext()
        {
            position++;
            return (position < matrixData.NumElements);
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
                    return matrixData[position + 1];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

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

