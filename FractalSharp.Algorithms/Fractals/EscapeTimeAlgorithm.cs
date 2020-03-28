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
using System.Threading;

namespace FractalSharp.Algorithms.Fractals
{
    public class EscapeTimeParams<TNumber> 
        : FractalParams<TNumber>
    {
        public Number<TNumber> EscapeRadius { get; set; }

        public EscapeTimeParams()
        {
            Location = Complex<TNumber>.Zero;
            Magnification = Number<TNumber>.One;
            MaxIterations = 256;

            EscapeRadius = Number<TNumber>.FromDouble(4.0);
        }

        public override IFractalParams Copy()
        {
            return new EscapeTimeParams<TNumber>
            {
                Location = Location,
                Magnification = Magnification,
                MaxIterations = MaxIterations,

                EscapeRadius = EscapeRadius
            };
        }
    }

    public abstract class EscapeTimeAlgorithm<TNumber, TParam>
        : FractalProvider<TNumber, TParam>
        where TParam : EscapeTimeParams<TNumber>
    {
        protected override bool Initialize(CancellationToken cancellationToken) => true;

        public override Rectangle<TNumber> GetOutputBounds(Number<TNumber> aspectRatio)
        {
            Number<TNumber> xScale = aspectRatio * Number<TNumber>.Two / Params.Magnification;

            Number<TNumber> xMin = -xScale + Params.Location.Real;
            Number<TNumber> xMax = xScale + Params.Location.Real;

            Number<TNumber> yScale = Number<TNumber>.Two / Params.Magnification;

            Number<TNumber> yMin = yScale + Params.Location.Imag;
            Number<TNumber> yMax = -yScale + Params.Location.Imag;

            return new Rectangle<TNumber>(xMin, xMax, yMin, yMax);
        }

        public override PointData Run(Complex<TNumber> mappedPoint)
        {
            // Initialize some variables..
            Complex<TNumber> prevOutput = GetInitialValue(mappedPoint);

            // Initialize our iteration count.
            int iter = 0;

            // Mandelbrot algorithm
            while (Complex<TNumber>.AbsSqu(prevOutput) < Params.EscapeRadius && iter < Params.MaxIterations)
            {
                prevOutput = DoIteration(prevOutput, mappedPoint);
                iter++;
            }

            return new PointData(prevOutput.ToDouble(), iter, iter < Params.MaxIterations ? PointClass.Outer : PointClass.Inner);
        }

        protected abstract Complex<TNumber> DoIteration(Complex<TNumber> prevOutput, Complex<TNumber> mappedPoint);

        protected virtual Complex<TNumber> GetInitialValue(Complex<TNumber> mappedPoint) => Complex<TNumber>.Zero;
    }
}
