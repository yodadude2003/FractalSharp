/*
 *  Copyright 2018-2020 Chosen Few Software
 *  This file is part of FractalSharp.
 *
 *  FractalSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  FractalSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with FractalSharp.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;

namespace FractalSharp.Numerics.Generic
{
    public interface IComplex
    {
        INumber Real { get; }
        INumber Imag { get; }

        Complex<double> ToDouble();
    }

    public struct Complex<T> : IComplex, IEquatable<Complex<T>>
    {
        public static Complex<T> Zero         { get; } = new Complex<T>(Number<T>.Zero, Number<T>.Zero);
        public static Complex<T> One          { get; } = new Complex<T>(Number<T>.One,  Number<T>.Zero);
        public static Complex<T> ImaginaryOne { get; } = new Complex<T>(Number<T>.Zero, Number<T>.One);

        public Number<T> Real { get; }
        public Number<T> Imag { get; }

        INumber IComplex.Real => Real;
        INumber IComplex.Imag => Imag;

        public Complex(Number<T> real, Number<T> imag)
        {
            Real = real;
            Imag = imag;
        }

        public static implicit operator Complex<T>(T n)
        {
            return new Complex<T>(n, Number<T>.Zero);
        }

        public static implicit operator Complex<T>(Number<T> n)
        {
            return new Complex<T>(n, Number<T>.Zero);
        }

        public static Complex<T> operator +(Complex<T> value)
        {
            return value;
        }

        public static Complex<T> operator -(Complex<T> value)
        {
            return new Complex<T>(-value.Real, -value.Imag);
        }

        public static Complex<T> operator +(Complex<T> left, Complex<T> right)
        {
            return new Complex<T>(left.Real + right.Real, left.Imag + right.Imag);
        }

        public static Complex<T> operator -(Complex<T> left, Complex<T> right)
        {
            return new Complex<T>(left.Real - right.Real, left.Imag - right.Imag);
        }

        public static Complex<T> operator *(Complex<T> left, Complex<T> right)
        {
            // Three multiplication trick
            var ac = left.Real * right.Real;
            var bd = left.Imag * right.Imag;
            var prod = (left.Real + left.Imag) * (right.Real + right.Imag);
            return new Complex<T>(ac - bd, prod - ac - bd);
        }

        public static Complex<T> operator /(Complex<T> left, Complex<T> right)
        {
            Complex<T> conjugate = new Complex<T>(right.Real, -right.Imag);
            Complex<T> numerator = left * conjugate;
            Number<T> denominator = (right * conjugate).Real;
            return new Complex<T>(numerator.Real / denominator, numerator.Imag / denominator);
        }

        public static bool operator ==(Complex<T> left, Complex<T> right)
        {
            return left.Real == right.Real && left.Imag == right.Imag;
        }

        public static bool operator !=(Complex<T> left, Complex<T> right)
        {
            return left.Real != right.Real || left.Imag != right.Imag;
        }

        public static Number<T> Abs(Complex<T> z)
        {
            Number<T> squ = z.Real * z.Real + z.Imag * z.Imag;
            return Number<T>.Sqrt(squ);
        }

        public static Number<T> AbsSqu(Complex<T> value)
        {
            return value.Real * value.Real + value.Imag * value.Imag;
        }

        public static Number<T> Arg(Complex<T> z)
        {
            return Number<T>.Atan2(z.Imag, z.Real);
        }

        public static Complex<T> Exp(Complex<T> z)
        {
            Complex<T> r = Number<T>.Exp(z.Real);
            Complex<T> i = new Complex<T>(Number<T>.Cos(z.Imag), Number<T>.Sin(z.Imag));
            return r * i;
        }

        public static Complex<T> Log(Complex<T> z)
        {
            Number<T> ln_r = Number<T>.Ln(AbsSqu(z)) / Number<T>.Two;
            return new Complex<T>(ln_r, Arg(z));
        }

        public static Complex<T> Pow(Complex<T> z, Complex<T> n)
        {
            if (z == Zero) return Zero;
            return Exp(Log(z) * n);
        }

        public Complex<double> ToDouble()
        {
            return new Complex<double>(Real.ToDouble(), Imag.ToDouble());
        }

        public bool Equals(Complex<T> other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            return obj is Complex<T> && Equals((Complex<T>)obj);
        }

        public override int GetHashCode()
        {
            return 10280812 + (Real.GetHashCode() - Imag.GetHashCode());
        }
    }
}
