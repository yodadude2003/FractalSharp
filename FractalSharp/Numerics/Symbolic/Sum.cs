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
using System.Collections.Generic;
using System.Linq;

namespace FractalSharp.Numerics.Symbolic
{
    public class Sum : IEquatable<Sum>
    {
        public Product[] Summands { get; }

        public Sum(IEnumerable<Product> summands)
        {
            List<Product> usedSummands = new List<Product>();

            bool isUsed(Product summand)
            {
                return usedSummands.Any(x => ReferenceEquals(x, summand));
            }

            Summands = summands
                .Select(summand =>
                {
                    if (!isUsed(summand))
                        return summands
                            .Where(x => !isUsed(x) && x.CanCombineWith(summand))
                            .Aggregate((sum, p) =>
                            {
                                usedSummands.Add(p);
                                return sum.Add(p);
                            });
                    else return null;
                }).Where(p => p != null && !p.IsZero).ToArray();
        }

        public Sum(params Product[] summands) : this(summands.AsEnumerable())
        {
        }

        public Sum Negate()
        {
            return new Sum(Summands.Select(s => s.Negate()));
        }

        public Sum Add(Sum other)
        {
            return new Sum(Summands.Concat(other.Summands));
        }

        public Sum Subtract(Sum other)
        {
            return Add(other.Negate());
        }

        public Sum Multiply(Sum other)
        {
            return new Sum(Summands.SelectMany(l => other.Summands.Select(r => l.Multiply(r))));
        }

        public Sum Divide(Sum other)
        {
            return Multiply(new Sum(new Product(other).Reciporical()));
        }

        public double ToDouble()
        {
            return Summands.Aggregate(0.0, (sum, prod) => sum + prod.ToDouble());
        }

        public bool Equals(Sum other)
        {
            return Summands.All(l => Summands.Count(r => l.Equals(r)) == other.Summands.Count(r => l.Equals(r)));
        }

        public override int GetHashCode()
        {
            return Summands.Aggregate(12345678, (hash, c) => hash * c.GetHashCode());
        }

        public override string ToString()
        {
            return ToDouble().ToString();
        }
    }
}
