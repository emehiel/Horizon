// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Runtime.Serialization;

namespace Utilities
{
    [Serializable]
    public class Complex<T> : ICloneable, ISerializable
    {
        #region Properties

        /// <summary>
        /// Contains the real part of a complex number
        /// </summary>
        public T Re { get; set; }

        /// <summary>
        /// Contains the imaginary part of a complex number
        /// </summary>
        public T Im { get; set; }

        /// <summary>
        /// Imaginary unit
        /// </summary>
        public static Complex<T> i
        {
            get
            {
                return new Complex<T>((T)Convert.ChangeType(0, typeof(T)), (T)Convert.ChangeType(1, typeof(T)));
            }
        }

        /// <summary>
        /// Imaginary unit
        /// </summary>
        public static Complex<T> j
        {
            get
            {
                return new Complex<T>((T)Convert.ChangeType(0, typeof(T)), (T)Convert.ChangeType(1, typeof(T)));
            }
        }

        /// <summary>
        /// Complex number zero (additive Identity).
        /// </summary>
        public static Complex<T> Zero
        {
            get
            {
                return new Complex<T>((T)Convert.ChangeType(0, typeof(T)), (T)Convert.ChangeType(0, typeof(T)));
            }
        }

        /// <summary>
        /// Complex number one (multiplicative identity).
        /// </summary>
        public static Complex<T> I
        {
            get
            {
                return new Complex<T>((T)Convert.ChangeType(1, typeof(T)), (T)Convert.ChangeType(0, typeof(T)));
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Represents a complex number with zero real and imaginary part.
        /// </summary>
        public Complex()
        {
            Re = (T)Convert.ChangeType(0, typeof(T));
            Im = (T)Convert.ChangeType(0, typeof(T));
        }

        /// <summary>
        /// Represents a complex number with imaginary part equal to zero
        /// </summary>
        /// <param name="realPart"></param>
        public Complex(T realPart)
        {
            Re = realPart;
            Im = (T)Convert.ChangeType(0, typeof(T));
        }

        /// <summary>
        /// Represents a complex number.
        /// </summary>
        /// <param name="realPart">The real part of the complex number</param>
        /// <param name="imaginaryPart">The imaginary part of the complex number</param>
        public Complex(T realPart, T imaginaryPart)
        {
            Re = realPart;
            Im = imaginaryPart;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Returns the sum of the complex numbers c1 and c2.
        /// </summary>
        public static Complex<T> operator +(Complex<T> c1, Complex<T> c2)
        {
            return new Complex<T>((dynamic)c1.Re + c2.Re, (dynamic)c1.Im + c2.Im);
        }

        /// <summary>
        /// Returns the difference of the complex numbers c1 and c2.
        /// </summary>
        public static Complex<T> operator -(Complex<T> c1, Complex<T> c2)
        {
            return new Complex<T>((dynamic)c1.Re - c2.Re, (dynamic)c1.Im - c2.Im);
        }

        /// <summary>
        /// Returns the additive inverse of the complex number c.
        /// </summary>
        public static Complex<T> operator -(Complex<T> c)
        {
            return new Complex<T>(-(dynamic)c.Re, -(dynamic)c.Im);
        }

        /// <summary>
        /// Returns the multiplicative product of the complex numbers c1 and c2
        /// </summary>
        public static Complex<T> operator *(Complex<T> c1, Complex<T> c2)
        {
            return new Complex<T>((dynamic)c1.Re * c2.Re - (dynamic)c1.Im * c2.Im, (dynamic)c1.Re * c2.Im + (dynamic)c2.Re * c1.Im);
        }

        /// <summary>
        /// Returns the rationalized complex quotent of c1 and c2
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Complex<double> operator /(Complex<T> c1, Complex<T> c2)
        {
            if (c1 == Complex<T>.Zero)
                return Complex<double>.Zero;
            else if (c2 == Complex<T>.Zero)
                throw new Exception("cannot divide by zero: operation Complex<T> c1 / Complex<T> c2");
            else
            {
                Double ac2 = Complex<T>.Abs(c2);
                Complex<T> num = c1 * Complex<T>.Conj(c2);
                Double ReNum = (dynamic)num.Re;
                Double ImNum = (dynamic)num.Im;
                return new Complex<double>(ReNum / ac2 / ac2, ImNum / ac2 / ac2);
            }
        }

        public static bool operator >(Complex<T> c1, Complex<T> c2)
        {
            if (Complex<T>.Abs(c1) == Complex<T>.Abs(c2))
                return (Complex<T>.Angle(c1) > Complex<T>.Angle(c2));
            else
                return (Complex<T>.Abs(c1) > Complex<T>.Abs(c2));
        }

        public static bool operator <(Complex<T> c1, Complex<T> c2)
        {
            return !(c1 > c2);
        }

        public static bool operator >=(Complex<T> c1, Complex<T> c2)
        {
            if (Complex<T>.Abs(c1) == Complex<T>.Abs(c2))
                return (Complex<T>.Angle(c1) >= Complex<T>.Angle(c2));
            else
                return (Complex<T>.Abs(c1) >= Complex<T>.Abs(c2));
        }

        public static bool operator <=(Complex<T> c1, Complex<T> c2)
        {
            if (Complex<T>.Abs(c1) == Complex<T>.Abs(c2))
                return (Complex<T>.Angle(c1) <= Complex<T>.Angle(c2));
            else
                return (Complex<T>.Abs(c1) <= Complex<T>.Abs(c2));
        }

        /// <summary>
        /// Returns a boolean value indicating if the complex numbers, c1 and c2, are the same
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static bool operator ==(Complex<T> c1, Complex<T> c2)
        {
            return ((dynamic)c1.Re == c2.Re && (dynamic)c1.Im == c2.Im);
        }

        /// <summary>
        /// Returns a boolean value indicating if the complex numbers, c1 and c2, are not the same
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static bool operator !=(Complex<T> c1, Complex<T> c2)
        {
            return !(c1 == c2);
        }

        /// <summary>
        /// Creates a Complex<T> number from a type T number with real part r1 and imaginary part 0.0
        /// </summary>
        /// <param name="r1"></param>
        /// <returns></returns>
        public static implicit operator Complex<T>(T r1)
        {
            return new Complex<T>(r1);
        }

        /// <summary>
        /// Creates a Double number from a Complex by accessing the real part of the complex number.
        /// Returns an exception if the number is not purely real.
        /// </summary>
        /// <param name="d1"></param>
        /// <returns></returns>
        public static explicit operator T(Complex<T> d1)
        {
            return d1.Re;
        }

        public static explicit operator Complex<T>(Matrix<T> m)
        {
            if (m.NumElements == 1)
                return new Complex<T>(m[1]);
            else
            {
                String message = "Cannot convert a " + m.NumRows.ToString() + " by " + m.NumCols.ToString() + " matrix into a single complex number.";
                throw new ArgumentOutOfRangeException("Matrix Size", message);
            }
        }

        #endregion

        #region Interfaces

        public object Clone() // ICloneable
        {
            Complex<T> c = new Complex<T>(Re, Im);
            return c;
        }

        // ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue("Real", Re);
            info.AddValue("Complex", Im);
        }

        #endregion

        #region Statics

        /// <summary>
        /// Returns the complex conjugate of the complex number c
        /// </summary>
        /// <param name="c1"></param>
        public static Complex<T> Conj(Complex<T> c)
        {
            return new Complex<T>(c.Re, -(dynamic)c.Im);
        }

        /// <summary>
        /// Returns the absolute value (or norm) of the complex number c
        /// </summary>
        /// <param name="c"></param>
        /// <returns>double</returns>
        public static double Abs(Complex<T> c)
        {
            return System.Math.Sqrt((dynamic)c.Re * c.Re + (dynamic)c.Im * c.Im);
        }

        /// <summary>
        /// Returns the absolute value (or norm) of the complex number c
        /// </summary>
        /// <param name="c"></param>
        /// <returns>double</returns>
        public static double Norm(Complex<T> c)
        {
            return Complex<T>.Abs(c);
        }

        public static double Angle(Complex<T> c)
        {
            return System.Math.Atan2((dynamic)c.Im, (dynamic)c.Re);
        }

        /// <summary>
        /// Returns the Inverse of a complex number
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Complex</returns>
        public static Complex<T> Inv(Complex<T> c)
        {
            return Conj(c) / (dynamic)(Abs(c) * Abs(c));
        }

        /// <summary>
        /// Complex exponential function.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Complex<T> Exp(Complex<T> c)
        {
            return new Complex<T>(System.Math.Exp((dynamic)c.Re) * System.Math.Cos((dynamic)c.Im), System.Math.Exp((dynamic)c.Re) * System.Math.Sin((dynamic)c.Im));
        }

        public static Complex<T> Max(Complex<T> c1, Complex<T> c2)
        {
            if (c1 >= c2)
                return c1;
            else
                return c2;
        }

        public static Complex<T> Min(Complex<T> c1, Complex<T> c2)
        {
            {
                if (c1 <= c2)
                    return c1;
                else
                    return c2;
            }
        }
        #endregion

        #region Overrides

        /// <summary>
        /// Converts a Comlex number to a string such as: "3.2 + 4.3j"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (IsReal())
                return (string.Format("{0}", Re));
            else
                if ((dynamic)Im < 0)
                return (string.Format("{0} - {1}j", Re, System.Math.Abs((dynamic)Im)));
            else
                return (string.Format("{0} + {1}j", Re, Im));
        }

        /// <summary>
        /// Determines if two complex numbers are equal by comparing ToString() method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool</returns>
        public override bool Equals(object obj)
        {
            return obj.ToString() == ToString();
        }

        /// <summary>
        /// Gets the hash code of a complex based on the ToString() method
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #endregion

        #region Dynamics

        public bool IsReal()
        {
            return (dynamic)Im == 0.0;
        }

        public bool IsImaginary()
        {
            return (dynamic)Re == 0.0;
        }

        #endregion
    }
}
