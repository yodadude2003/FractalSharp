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
using FractalSharp.Algorithms.Coloring;
using FractalSharp.Algorithms.Fractals;
using FractalSharp.Imaging;
using FractalSharp.Numerics.Generic;
using FractalSharp.Numerics.Symbolic;
using FractalSharp.Numerics.Types;
using FractalSharp.Processing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FractalSharp.ExampleApp
{
    class SkiaImageBuilder : ImageBuilder
    {
        public SKBitmap Bitmap { get; private set; }

        public override void InitializeImage(int width, int height)
        {
            Bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul));
        }

        public override void WritePixel(int x, int y, RgbaValue color)
        {
            Bitmap.SetPixel(x, y, new SKColor(color.Red, color.Green, color.Blue, color.Alpha));
        }
    }

    class Program
    {
        private const int WIDTH  = 256;
        private const int HEIGHT = 256;

        private static readonly FractalProcessor<Sum, SquareMandelbrotAlgorithm<Sum>> FractalProcessor =
            new FractalProcessor<Sum, SquareMandelbrotAlgorithm<Sum>>(WIDTH, HEIGHT);

        private static readonly ColorProcessor<SmoothColoringAlgorithm> OuterColorProcessor =
            new ColorProcessor<SmoothColoringAlgorithm>(WIDTH, HEIGHT);

        private static readonly ColorProcessor<RadialGradientAlgorithm> InnerColorProcessor =
            new ColorProcessor<RadialGradientAlgorithm>(WIDTH, HEIGHT);

        private static readonly SkiaImageBuilder Imager = new SkiaImageBuilder();

        private static readonly Gradient Colors =
            new Gradient(256, new List<GradientKey>
            {
                new GradientKey(new RgbaValue(9, 1, 47)),
                new GradientKey(new RgbaValue(4, 4, 73)),
                new GradientKey(new RgbaValue(0, 7, 100)),
                new GradientKey(new RgbaValue(12, 44, 138)),
                new GradientKey(new RgbaValue(24, 82, 177)),
                new GradientKey(new RgbaValue(57, 125, 209)),
                new GradientKey(new RgbaValue(134, 181, 229)),
                new GradientKey(new RgbaValue(211, 236, 248)),
                new GradientKey(new RgbaValue(241, 233, 191)),
                new GradientKey(new RgbaValue(248, 201, 95)),
                new GradientKey(new RgbaValue(255, 170, 0)),
                new GradientKey(new RgbaValue(204, 128, 0)),
                new GradientKey(new RgbaValue(153, 87, 0)),
                new GradientKey(new RgbaValue(106, 52, 3)),
                new GradientKey(new RgbaValue(66, 30, 15)),
                new GradientKey(new RgbaValue(25, 7, 26))
            });

        static async Task Main(string[] args)
        {
            Console.WriteLine("Process started.");

            Console.WriteLine("Computing raw fractal data...");
            await FractalProcessor.SetupAsync(new ProcessorConfig
            {
                ThreadCount = Environment.ProcessorCount,

                Params = new EscapeTimeParams<Sum>
                {
                    MaxIterations = 3,
                    Magnification = Number<Sum>.One,
                    Location = Complex<Sum>.Zero,
                    EscapeRadius = Number<Sum>.FromDouble(4.0),
                }
            }, CancellationToken.None);
            PointData[,] inputData = await FractalProcessor.ProcessAsync(CancellationToken.None);

            Console.WriteLine("Computing colors for inner points...");
            await InnerColorProcessor.SetupAsync(new ColorProcessorConfig
            {
                ThreadCount = Environment.ProcessorCount,
                Params = new RadialGradientParams
                {
                    Scale = 256
                },
                PointClass = PointClass.Inner,
                InputData = inputData
            }, CancellationToken.None);
            double[,] innerIndicies = await InnerColorProcessor.ProcessAsync(CancellationToken.None);

            Console.WriteLine("Computing colors for outer points...");
            await OuterColorProcessor.SetupAsync(new ColorProcessorConfig
            {
                ThreadCount = Environment.ProcessorCount,
                Params = new EmptyColoringParams(),
                PointClass = PointClass.Outer,
                InputData = inputData
            }, CancellationToken.None);
            double[,] outerIndicies = await OuterColorProcessor.ProcessAsync(CancellationToken.None);

            Console.WriteLine("Building image...");
            Imager.CreateImage(outerIndicies, innerIndicies, Colors, Colors);

            Console.WriteLine("Writing image file to disk...");
            SKPixmap.Encode(new SKFileWStream("output.png"), Imager.Bitmap, SKEncodedImageFormat.Png, 100);

            Console.WriteLine("Image rendered successfully!");
        }
    }
}