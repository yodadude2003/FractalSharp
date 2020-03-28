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

using FractalSharp.Algorithms;
using FractalSharp.Numerics.Generic;

namespace FractalSharp.Processing
{
    public class PointMapper<TNumber>
    {
        public Rectangle<double> InputSpace { get; set; }
        public Rectangle<TNumber> OutputSpace { get; set; }

        public Number<TNumber> MapPointX(Number<double> value)
        {
            return MapValue(Number<TNumber>.FromDouble(value),
                Number<TNumber>.FromDouble(InputSpace.XMin), 
                Number<TNumber>.FromDouble(InputSpace.XMax),
                OutputSpace.XMin, OutputSpace.XMax);
        }

        public Number<TNumber> MapPointY(Number<double> value)
        {
            return MapValue(Number<TNumber>.FromDouble(value),
                Number<TNumber>.FromDouble(InputSpace.YMin),
                Number<TNumber>.FromDouble(InputSpace.YMax),
                OutputSpace.YMin, OutputSpace.YMax);
        }

        private static Number<T> MapValue<T>(Number<T> OldValue, Number<T> OldMin, Number<T> OldMax, Number<T> NewMin, Number<T> NewMax)
        {
            Number<T> OldRange = OldMax - OldMin;
            Number<T> NewRange = NewMax - NewMin;
            Number<T> NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }
    }
}
