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

namespace FractalSharp.Algorithms.Fractals
{
    public class GeneralMandelbrotParams<TNumber> : 
        EscapeTimeParams<TNumber> 
    {
        public Complex<TNumber> Power { get; set; }

        public GeneralMandelbrotParams()
        {
            Power = new Complex<TNumber>(Number<TNumber>.Two, Number<TNumber>.Zero);
        }

        public override IFractalParams Copy()
        {
            return new GeneralMandelbrotParams<TNumber>
            {
                MaxIterations = MaxIterations,
                Magnification = Magnification,
                Location = Location,

                EscapeRadius = EscapeRadius,
                Power = Power
            };
        }
    }
    public class GeneralMandelbrotAlgorithm<TNumber> :
        EscapeTimeAlgorithm<TNumber, GeneralMandelbrotParams<TNumber>> 
    {
        protected override Complex<TNumber> DoIteration(Complex<TNumber> z, Complex<TNumber> c)
        {
            return Complex<TNumber>.Pow(z, Params.Power) + c;
        }
    }
}
