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

namespace FractalSharp.Numerics.Symbolic
{
    public class Power<T> : IEquatable<Power<T>> where T : IEquatable<T>
    {
        public T Base { get; }
        public int Exponent { get; }

        public Power(T @base, int exponent)
        {
            Base = @base;
            Exponent = exponent;
        }

        public static implicit operator Power<T>(T value)
        {
            return new Power<T>(value, 1);
        }

        public bool Equals(Power<T> other)
        {
            return Base.Equals(other.Base) && Exponent.Equals(other.Exponent);
        }

        public override int GetHashCode()
        {
            return Base.GetHashCode() ^ Exponent.GetHashCode();
        }
    }
}
