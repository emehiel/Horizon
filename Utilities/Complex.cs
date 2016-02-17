using System;
using System.Runtime.Serialization;

namespace Utilities
{
    [Serializable]
    public class Complex : ICloneable, ISerializable
    {
        #region Properties

        /// <summary>
        /// Contains the real part of a complex number
        /// </summary>
        public double Re { get; set; }

        /// <summary>
        /// Contains the imaginary part of a complex number
        /// </summary>
        public double Im { get; set; }

        /// <summary>
        /// Imaginary unit
        /// </summary>
        public static Complex i
        {
            get
            {
                return new Complex(0, 1);
            }
        }

        /// <summary>
        /// Imaginary unit
        /// </summary>
        public static Complex j
        {
            get
            {
                return new Complex(0, 1);
            }
        }

        /// <summary>
        /// Complex number zero (additive Identity).
        /// </summary>
        public static Complex Zero
        {
            get
            {
                return new Complex(0, 0);
            }
        }

        /// <summary>
        /// Complex number one (multiplicative identity).
        /// </summary>
        public static Complex I
        {
            get
            {
                return new Complex(1, 0);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Represents a complex number with zero real and imaginary part.
        /// </summary>
        public Complex()
        {
            this.Re = 0;
            this.Im = 0;
        }

        /// <summary>
        /// Represents a complex number with imaginary part equal to zero
        /// </summary>
        /// <param name="realPart"></param>
        public Complex(double realPart)
        {
            this.Re = realPart;
            this.Im = 0;
        }

        /// <summary>
        /// Represents a complex number.
        /// </summary>
        /// <param name="realPart">The real part of the complex number</param>
        /// <param name="imaginaryPart">The imaginary part of the complex number</param>
        public Complex(double realPart, double imaginaryPart)
        {
            this.Re = realPart;
            this.Im = imaginaryPart;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Returns the sum of the complex numbers c1 and c2.
        /// </summary>
        public static Complex operator +(Complex c1, Complex c2)
        {
            return new Complex(c1.Re + c2.Re, c1.Im + c2.Im);
        }

        /// <summary>
        /// Returns the difference of the complex numbers c1 and c2.
        /// </summary>
        public static Complex operator -(Complex c1, Complex c2)
        {
            return new Complex(c1.Re - c2.Re, c1.Im - c2.Im);
        }

        /// <summary>
        /// Returns the additive inverse of the complex number c.
        /// </summary>
        public static Complex operator -(Complex c)
        {
            return new Complex(-c.Re, -c.Im);
        }

        /// <summary>
        /// Returns the multiplicative product of the complex numbers c1 and c2
        /// </summary>
        public static Complex operator *(Complex c1, Complex c2)
        {
            return new Complex(c1.Re * c2.Re - c1.Im * c2.Im, c1.Re * c2.Im + c2.Re * c1.Im);
        }

        /// <summary>
        /// Returns the rationalized complex quotent of c1 and c2
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Complex operator /(Complex c1, Complex c2)
        {
            if (c1 == Complex.Zero)
                return Complex.Zero;
            else if (c2 == Complex.Zero)
                throw new Exception("cannot divide by zero: operation Complex c1 / Complex c2");
            else
            {
                Double ac2 = Complex.Abs(c2);
                Complex num = c1 * Complex.Conj(c2);
                Double ReNum = num.Re;
                Double ImNum = num.Im;
                return new Complex(ReNum / ac2 / ac2, ImNum / ac2 / ac2);
            }
        }

        public static bool operator >(Complex c1, Complex c2)
        {
            if (Complex.Abs(c1) == Complex.Abs(c2))
                return (Complex.Angle(c1) > Complex.Angle(c2));
            else
                return (Complex.Abs(c1) > Complex.Abs(c2));
        }

        public static bool operator <(Complex c1, Complex c2)
        {
            return !(c1 > c2);
        }

        public static bool operator >=(Complex c1, Complex c2)
        {
            if (Complex.Abs(c1) == Complex.Abs(c2))
                return (Complex.Angle(c1) >= Complex.Angle(c2));
            else
                return (Complex.Abs(c1) >= Complex.Abs(c2));
        }

        public static bool operator <=(Complex c1, Complex c2)
        {
            return !(c1 >= c2);
        }

        /// <summary>
        /// Returns a boolean value indicating if the complex numbers, c1 and c2, are the same
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static bool operator ==(Complex c1, Complex c2)
        {
            return (c1.Re == c2.Re && c1.Im == c2.Im);
        }

        /// <summary>
        /// Returns a boolean value indicating if the complex numbers, c1 and c2, are not the same
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static bool operator !=(Complex c1, Complex c2)
        {
            return !(c1 == c2);
        }

        /// <summary>
        /// Creates a Complex number from a double with real part r1 and imaginary part 0.0
        /// </summary>
        /// <param name="r1"></param>
        /// <returns></returns>
        public static implicit operator Complex(double r1)
        {
            return new Complex(r1);
        }

        /// <summary>
        /// Creates a Complex number from a float with real part r1 and imaginary part 0.0
        /// </summary>
        /// <param name="r1"></param>
        /// <returns></returns>
        public static implicit operator Complex(float r1)
        {
            return new Complex((double)r1);
        }

        /// <summary>
        /// Creates a Complex number from an int with real part r1 and imaginary part 0.0
        /// </summary>
        /// <param name="r1"></param>
        /// <returns></returns>
        public static implicit operator Complex(int r1)
        {
            return new Complex((double)r1);
        }

        /// <summary>
        /// Creates a Double number from a Complex by accessing the real part of the complex number.
        /// Returns an exception if the number is not purely real.
        /// </summary>
        /// <param name="d1"></param>
        /// <returns></returns>
        public static explicit operator Double(Complex d1)
        {
            return d1.Re;
        }

        public static explicit operator float (Complex c1)
        {
            return (float)c1.Re;
        }

        public static explicit operator int (Complex c1)
        {
            return (int)c1.Re;
        }

        public static explicit operator Complex(Matrix m)
        {
            if (m.NumElements == 1)
                return (Complex)m[1].Clone();
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
            Complex c = new Complex(this.Re, this.Im);
            return c;
        }

        // ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue("Real", this.Re);
            info.AddValue("Complex", this.Im);
        }

        #endregion

        #region Statics

        /// <summary>
        /// Returns the complex conjugate of the complex number c
        /// </summary>
        /// <param name="c1"></param>
        public static Complex Conj(Complex c)
        {
            return new Complex(c.Re, -c.Im);
        }

        /// <summary>
        /// Returns the absolute value (or norm) of the complex number c
        /// </summary>
        /// <param name="c"></param>
        /// <returns>double</returns>
        public static double Abs(Complex c)
        {
            return Math.Sqrt(c.Re * c.Re + c.Im * c.Im);
        }

        /// <summary>
        /// Returns the absolute value (or norm) of the complex number c
        /// </summary>
        /// <param name="c"></param>
        /// <returns>double</returns>
        public static Double Norm(Complex c)
        {
            return Complex.Abs(c);
        }

        public static Double Angle(Complex c)
        {
            return Math.Atan2(c.Im, c.Re);
        }

        /// <summary>
        /// Returns the Inverse of a complex number
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Complex</returns>
        public static Complex Inv(Complex c)
        {
            return Complex.Conj(c) / (Complex.Abs(c) * Complex.Abs(c));
        }

        /// <summary>
        /// Complex exponential function.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Complex Exp(Complex c)
        {
            return new Complex(Math.Exp(c.Re) * Math.Cos(c.Im), Math.Exp(c.Re) * Math.Sin(c.Im));
        }

        public static Complex Max(Complex c1, Complex c2)
        {
            if (c1 >= c2)
                return c1;
            else
                return c2;
        }

        public static Complex Min(Complex c1, Complex c2)
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
            if (this.IsReal())
                return (String.Format("{0}", this.Re));
            else
                if (this.Im < 0)
                return (String.Format("{0} - {1}j", Re, Math.Abs(Im)));
            else
                return (String.Format("{0} + {1}j", Re, Im));
        }

        /// <summary>
        /// Determines if two complex numbers are equal by comparing ToString() method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool</returns>
        public override bool Equals(object obj)
        {
            return obj.ToString() == this.ToString();
        }

        /// <summary>
        /// Gets the hash code of a complex based on the ToString() method
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        #endregion

        #region Dynamics

        public bool IsReal()
        {
            return this.Im == 0.0;
        }

        public bool IsImaginary()
        {
            return this.Re == 0.0;
        }

        #endregion
    }
}
