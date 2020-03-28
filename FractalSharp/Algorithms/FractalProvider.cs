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
    public interface IFractalProvider<TNumber> : IAlgorithmProvider<Complex<TNumber>, PointData>
    {
        Rectangle<TNumber> GetOutputBounds(Number<TNumber> aspectRatio);
    }

    public abstract class FractalProvider<TNumber, TParam> : 
        AlgorithmProvider<Complex<TNumber>, PointData, TParam>, 
        IFractalProvider<TNumber>
        where TParam : FractalParams<TNumber>
    {
        public abstract Rectangle<TNumber> GetOutputBounds(Number<TNumber> aspectRatio);
    }
}
