﻿/*
 *  Copyright 2018-2019 Chosen Few Software
 *  This file is part of MandelbrotSharp.
 *
 *  MandelbrotSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MandelbrotSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with MandelbrotSharp.  If not, see <https://www.gnu.org/licenses/>.
 */
using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MandelbrotSharp.Rendering
{
    public class RenderSettings
    {
        private Type _algorithmType = typeof(TraditionalAlgorithmProvider<>);
        private Type _arithmeticType = typeof(double);
        private Type _pixelColoratorType = typeof(PixelColorator);

        private AlgorithmParams _algorithmParams = new AlgorithmParams();

        private int _threadCount = Environment.ProcessorCount;

        private RgbaValue[] _palette;

        public virtual AlgorithmParams AlgorithmParams { get => _algorithmParams; set => _algorithmParams = value; }
        public virtual Type AlgorithmType { get => _algorithmType; set => _algorithmType = value; }
        public virtual Type ArithmeticType { get => _arithmeticType; set => _arithmeticType = value; }
        public virtual Type PixelColoratorType { get => _pixelColoratorType; set => _pixelColoratorType = value; }
        public virtual RgbaValue[] Palette { get => _palette; set => _palette = value; }
        public virtual int ThreadCount { get => _threadCount; set => _threadCount = value; }


        public virtual BigDecimal Magnification { get => _algorithmParams.Magnification; set => _algorithmParams.Magnification = value; }
        public virtual BigDecimal offsetX { get => _algorithmParams.offsetX; set => _algorithmParams.offsetX = value; }
        public virtual BigDecimal offsetY { get => _algorithmParams.offsetY; set => _algorithmParams.offsetY = value; }
        public virtual int MaxIterations { get => _algorithmParams.MaxIterations; set => _algorithmParams.MaxIterations = value; }
        public virtual Dictionary<string, object> ExtraParams { get => _algorithmParams.ExtraParams; set => _algorithmParams.ExtraParams = value; }
    }
}
