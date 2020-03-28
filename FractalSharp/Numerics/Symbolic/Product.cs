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
    public class Product : IEquatable<Product>
    {
        public Power<int>[] Constants { get; }
        public Power<Sum>[] Sums { get; }

        public bool IsZero => Constants.Any(c => c.Base == 0);
        public bool IsOne => !Constants.Any() && !Sums.Any();

        public Product(IEnumerable<Power<int>> constants, IEnumerable<Power<Sum>> sums)
        {
            if (constants.Any(c => c.Base == 0) || sums.Any(s => !s.Base.Summands.Any()))
            {
                Constants = new Power<int>[] { 0 };
                Sums = new Power<Sum>[0];
            }
            else
            {
                var sumz = sums.Where(s => s.Base.Summands.Length > 1);
                var products = sums
                    .Where(s => s.Base.Summands.Length == 1)
                    .Select(s => new Power<Product>(s.Base.Summands.Single(), s.Exponent));
                Constants = constants
                    .Concat(products
                        .SelectMany(
                            p => p.Base.Constants
                                .Select(c => new Power<int>(c.Base, c.Exponent * p.Exponent))
                        )
                    ).Where(c => c.Base != 1 && c.Exponent != 0).ToArray();
                Sums = sumz
                    .Concat(products
                        .SelectMany(
                            p => p.Base.Sums
                                .Select(s => new Power<Sum>(s.Base, s.Exponent * p.Exponent))
                        )
                    ).Where(s => s.Exponent != 0).ToArray();
            }
        }

        public Product(IEnumerable<Power<int>> constants) : this(constants, new Power<Sum>[0])
        {
        }

        public Product(IEnumerable<Power<Sum>> sums) : this(new Power<int>[0], sums)
        {
        }

        public Product(params Power<int>[] constants) : this(constants, new Power<Sum>[0])
        {
        }

        public Product(params Power<Sum>[] sums) : this(new Power<int>[0], sums)
        {
        }

        public Product Negate()
        {
            return Multiply(new Product(-1));
        }

        public Product Reciporical()
        {
            return new Product(
                Constants.Select(c => new Power<int>(c.Base, -c.Exponent)),
                Sums.Select(s => new Power<Sum>(s.Base, -s.Exponent))
                );
        }

        public Product GreatestCommonDivisor(Product other)
        {
            return new Product(
                Constants.Select(l => new Power<int>(
                    l.Base, Math.Min(l.Exponent,
                    (other.Constants.SingleOrDefault(r => l.Base == r.Base)?.Exponent)
                    .GetValueOrDefault()))
                ));
        }

        public bool CanCombineWith(Product other)
        {
            return Equals(other) || !GreatestCommonDivisor(other).IsOne;
        }

        public Product Add(Product other)
        {
            var gcd = GreatestCommonDivisor(other);
            var me = Divide(gcd);
            var oth = other.Divide(gcd);
            if (!me.Sums.Any() && !oth.Sums.Any())
                return FromDouble(me.ToDouble() + oth.ToDouble()).Multiply(gcd);
            else
                return new Product(new Sum(me, oth)).Multiply(gcd);
        }

        public Product Subtract(Product other)
        {
            return Add(other.Negate());
        }

        public Product Multiply(Product other)
        {
            return new Product(Constants
                .Select(l => new Power<int>(
                    l.Base, other.Constants.Where(r => r.Base == l.Base).Aggregate(l.Exponent, (sum, c) => sum + c.Exponent)
                    ))
                .Concat(other.Constants
                    .Where(r => !Constants.Any(l => l.Base == r.Base))
                    ), Sums.Concat(other.Sums));
        }

        public Product Divide(Product other)
        {
            return Multiply(other.Reciporical());
        }

        private static List<Power<int>> PrimeFactors(int N)
        {
            if (N == 0)
                return new List<Power<int>> { 0 };

            int n = Math.Abs(N);
            List<Power<int>> powers = new List<Power<int>> { Math.Sign(N) };

            int count = 0;

            // count the number of times 2 divides  
            while (!(n % 2 > 0))
            {
                // equivalent to n=n/2; 
                n >>= 1;

                count++;
            }

            // if 2 divides it 
            if (count > 0)
                powers.Add(new Power<int>(2, count));

            // check for all the possible 
            // numbers that can divide it 
            for (int i = 3; i <= (int)
                 Math.Sqrt(n); i += 2)
            {
                count = 0;
                while (n % i == 0)
                {
                    count++;
                    n = n / i;
                }
                if (count > 0)
                    powers.Add(new Power<int>(i, count));
            }

            // if n at the end is a prime number. 
            if (n > 2)
                powers.Add(new Power<int>(n, 1));

            return powers;
        }

        public static Product FromDouble(double value)
        {
            int mantissa = (int)value;
            int exponent = 0;
            double scaleFactor = 1;
            while (mantissa != value * scaleFactor)
            {
                exponent -= 1;
                scaleFactor *= 2;
                mantissa = (int)(value * scaleFactor);
            }
            var factors = PrimeFactors(mantissa);
            factors.Add(new Power<int>(2, exponent));
            return new Product(factors);
        }

        public double ToDouble()
        {
            return Constants.Aggregate(1.0, (prod, pow) => prod * Math.Pow(pow.Base, pow.Exponent)) * 
                Sums.Aggregate(1.0, (prod, pow) => prod * Math.Pow(pow.Base.ToDouble(), pow.Exponent));
        }

        public bool Equals(Product other)
        {
            return Constants.All(l => other.Constants.Any(r => l.Equals(r))) &&
                Sums.All(l => Sums.Count(r => l.Equals(r)) == other.Sums.Count(r => l.Equals(r)));
        }

        public override int GetHashCode()
        {
            return 12345678 * Constants.Aggregate(1, (hash, c) => hash * c.GetHashCode()) * 
                Sums.Aggregate(1, (hash, s) => hash * s.GetHashCode());
        }

        public override string ToString()
        {
            return ToDouble().ToString();
        }
    }
}
