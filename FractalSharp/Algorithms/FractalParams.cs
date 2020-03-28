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

namespace FractalSharp.Algorithms
{
    public interface IFractalParams : IAlgorithmParams
    {
        int MaxIterations { get; set; }
        INumber Magnification { get; set; }
        IComplex Location { get; set; }

        new IFractalParams Copy();
    }

    public abstract class FractalParams<TNumber> : IFractalParams
    {
        public int MaxIterations { get; set; }
        public Number<TNumber> Magnification { get; set; }
        public Complex<TNumber> Location { get; set; }

        int IFractalParams.MaxIterations { get => MaxIterations; set => MaxIterations = value; }
        INumber IFractalParams.Magnification { get => Magnification; set => Magnification = (Number<TNumber>)value; }
        IComplex IFractalParams.Location { get => Location; set => Location = (Complex<TNumber>)value; }

        IAlgorithmParams IAlgorithmParams.Copy() => Copy();

        public abstract IFractalParams Copy();
    }
}
