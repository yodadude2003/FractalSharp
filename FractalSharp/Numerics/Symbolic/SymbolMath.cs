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


using FractalSharp.Numerics.Generic;

namespace FractalSharp.Numerics.Symbolic
{
    public class SymbolMath : IMath<Sum>
    {
        public Sum Add(Sum left, Sum right) => left.Add(right);
        public Sum Subtract(Sum left, Sum right) => left.Subtract(right);
        public Sum Multiply(Sum left, Sum right) => left.Multiply(right);
        public Sum Divide(Sum left, Sum right) => left.Divide(right);

        public Sum Negate(Sum value) => value.Negate();

        public bool Equal(Sum left, Sum right) => left.Equals(right);
        public bool NotEqual(Sum left, Sum right) => !left.Equals(right);

        public bool LessThan(Sum left, Sum right) => left.ToDouble() < right.ToDouble();
        public bool GreaterThan(Sum left, Sum right) => left.ToDouble() > right.ToDouble();

        public bool LessThanOrEqual(Sum left, Sum right) => left.ToDouble() <= right.ToDouble();
        public bool GreaterThanOrEqual(Sum left, Sum right) => left.ToDouble() >= right.ToDouble();

        public double ToDouble(Sum value) => value.ToDouble();
        public Sum FromDouble(double value) => new Sum(Product.FromDouble(value));

        public Sum Ln(Sum value) => throw new System.NotImplementedException();
        public Sum Exp(Sum value) => throw new System.NotImplementedException();

        public Sum Pow(Sum x, Sum y) => throw new System.NotImplementedException();

        public Sum Sqrt(Sum value) => throw new System.NotImplementedException();

        public Sum Sin(Sum value) => throw new System.NotImplementedException();
        public Sum Cos(Sum value) => throw new System.NotImplementedException();
        public Sum Tan(Sum value) => throw new System.NotImplementedException();

        public Sum Asin(Sum value) => throw new System.NotImplementedException();
        public Sum Acos(Sum value) => throw new System.NotImplementedException();
        public Sum Atan(Sum value) => throw new System.NotImplementedException();

        public Sum Atan2(Sum y, Sum x) => throw new System.NotImplementedException();
    }
}
