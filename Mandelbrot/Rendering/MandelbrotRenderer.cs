using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.VectorTypes;
using Mandelbrot.Algorithms;
using Mandelbrot.Imaging;
using Mandelbrot.Mathematics;
using Mandelbrot.Properties;
using Mandelbrot.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mandelbrot.Rendering
{
    delegate void FrameStartDelegate();
    delegate void FrameStopDelegate(Bitmap frame);

    delegate void RenderStopDelegate();

    class MandelbrotRenderer
    { 

        protected int CellX;
        protected int CellY;

        protected bool Gradual = true;


        protected GenericMathResolver MathResolver;
        protected DirectBitmap CurrentFrame;
        protected dynamic AlgorithmProvider, PointMapper;


        protected bool isInitialized = false;

        protected int ThreadCount = Environment.ProcessorCount;
        public int MaxIterations { get; protected set; }
        public BigDecimal Magnification { get; protected set; }

        protected BigDecimal offsetX;
        protected BigDecimal offsetY;
        protected BigDecimal aspectRatio;

        protected int Width;
        protected int Height;

        protected int TotalCellsX = 4;
        protected int TotalCellsY = 3;

        protected int CellWidth;
        protected int CellHeight;

        protected int[] ChunkSizes = new int[12];
        protected int[] MaxChunkSizes = new int[12];

        protected RGB[] palette;

        protected Type AlgorithmType;
        protected Type ArithmeticType;

        protected CancellationTokenSource Job;

        public event FrameStartDelegate FrameStarted;
        public event FrameStopDelegate FrameFinished;
        public event RenderStopDelegate RenderHalted;

        protected virtual void FrameStart()
        {
            FrameStarted();
        }

        protected virtual void FrameEnd(Bitmap frame)
        {
            FrameFinished(frame);
        }

        #region Initialization and Configuration Methods

        public void Initialize(RenderSettings settings, RGB[] newPalette, GenericMathResolver mathResolver)
        {
            MathResolver = mathResolver;

            Width = settings.Width;
            Height = settings.Height;

            CellWidth = Width / TotalCellsX;
            CellHeight = Height / TotalCellsY;

            aspectRatio = ((BigDecimal)Width / (BigDecimal)Height) * 2;

            CurrentFrame = new DirectBitmap(Width, Height);

            palette = newPalette;

            isInitialized = true;

            Setup(settings);
        }

        public void Setup(RenderSettings settings)
        {
            if (isInitialized)
            {
                Job = new CancellationTokenSource();

                offsetX = settings.offsetX;
                offsetY = settings.offsetY;

                Magnification = settings.Magnification;
                MaxIterations = settings.MaxIterations;

                ThreadCount = settings.ThreadCount;

                AlgorithmType = settings.AlgorithmType;

                ArithmeticType = settings.ArithmeticType;

                Gradual = settings.Gradual;

                MaxChunkSizes = settings.MaxChunkSizes;

                ResetChunkSizes();

                dynamic TMath = MathResolver.CreateMathObject(ArithmeticType);

                var genericType = typeof(PointMapper<>).MakeGenericType(ArithmeticType);
                PointMapper = Activator.CreateInstance(genericType, TMath);

                PointMapper.SetInputSpace(0, Width, 0, Height);

                genericType = AlgorithmType.MakeGenericType(ArithmeticType);
                AlgorithmProvider = Activator.CreateInstance(genericType, TMath);

                AlgorithmProvider.UpdateParams(new AlgorithmParams
                {
                    Magnification = Magnification,
                    offsetX = offsetX,
                    offsetY = offsetY,
                    MaxIterations = MaxIterations,
                    Token = Job.Token
                });
            }
            else
            {
                throw new ApplicationException("Renderer is not Initialized!");
            }
        }

        public void ResetChunkSizes() {
            for (var i = 0; i < ChunkSizes.Length; i++)
            {
                ChunkSizes[i] = MaxChunkSizes[i];
            }
        }

        #endregion

        #region Algorithm Methods

        // Smooth Coloring Algorithm
        private Color GetColorFromIterationCount(int iterCount, double znMagn)
        {
            double temp_i = iterCount;
            // sqrt of inner term removed using log simplification rules.
            double log_zn = Math.Log(znMagn) / 2;
            double nu = Math.Log(log_zn / Math.Log(2)) / Math.Log(2);
            // Rearranging the potential function.
            // Dividing log_zn by log(2) instead of log(N = 1<<8)
            // because we want the entire palette to range from the
            // center to radius 2, NOT our bailout radius.
            temp_i = temp_i + 1 - nu;
            // Grab two colors from the pallete
            RGB color1 = palette[(int)temp_i % (palette.Length - 1)];
            RGB color2 = palette[(int)(temp_i + 1) % (palette.Length - 1)];

            // Lerp between both colors
            RGB final = RGB.LerpColors(color1, color2, temp_i % 1);

            // Return the result.
            return final.toColor();
        }

        public void GetPointFromFrameLocation(int x, int y, out BigDecimal offsetX, out BigDecimal offsetY)
        {
            BigDecimal xRange = aspectRatio / Magnification;
            BigDecimal yRange = 2 / Magnification;
            offsetX = Utils.Map<BigDecimal>(new BigDecimalMath(), x, 0, Width, -xRange + this.offsetX, xRange + this.offsetX);
            offsetY = Utils.Map<BigDecimal>(new BigDecimalMath(), y, 0, Height, -yRange + this.offsetY, yRange + this.offsetY);
        }

        #endregion

        #region Rendering Methods

        protected void IncrementCellCoords()
        {
            if (CellX < TotalCellsX - 1) { CellX++; }
            else if (CellY < TotalCellsY - 1) { CellX = 0; CellY++; }
            else { CellX = 0; CellY = 0; }
        }

        public void RenderCell()
        {
            int in_set = 0;

            int index = CellX + CellY * 4;
            int chunkSize = ChunkSizes[index];
            int maxChunkSize = MaxChunkSizes[index];

            BigDecimal scaleFactor = aspectRatio;
            BigDecimal zoom = Magnification;
            // Predefine minimum and maximum values of the plane, 
            // In order to avoid making unnecisary calculations on each pixel.  

            // x_min = -scaleFactor / zoom
            // x_max =  scaleFactor / zoom
            BigDecimal xMin = -scaleFactor / zoom + offsetX;
            BigDecimal xMax = scaleFactor / zoom + offsetX;

            // y_min = -2 / zoom
            // y_max =  2 / zoom
            BigDecimal yMin = -2 / zoom + offsetY;
            BigDecimal yMax = 2 / zoom + offsetY;

            PointMapper.SetOutputSpace(xMin, xMax, yMin, yMax);

            var loop = Parallel.For(CellX * CellWidth, (CellX + 1) * CellWidth, new ParallelOptions { CancellationToken = Job.Token, MaxDegreeOfParallelism = ThreadCount }, px =>
            {
                var x0 = PointMapper.MapPointX(px);
                for (int py = CellY * CellHeight; py < (CellY + 1) * CellHeight; py++)
                {
                    var y0 = PointMapper.MapPointY(py);
                    if ((px % chunkSize != 0 ||
                         py % chunkSize != 0) ||
                       ((px / chunkSize) % 2 == 0 &&
                        (py / chunkSize) % 2 == 0 &&
                        maxChunkSize != chunkSize))
                        continue;

                    PixelData pixelData = AlgorithmProvider.Run(x0, y0);

                    // Grab the values from our pixel data

                    double magn = pixelData.GetMagnitude();
                    int iterCount = pixelData.GetIterCount();
                    bool pointEscaped = pixelData.GetEscaped();

                    Color PixelColor;

                    // if zn's magnitude surpasses the 
                    // bailout radius, give it a fancy color.
                    if (pointEscaped) // itercount
                    {
                        PixelColor = GetColorFromIterationCount(iterCount, magn);
                    }
                    // Otherwise, make the pixel black, as it is in the set.  
                    else
                    {
                        PixelColor = Color.Black;
                        Interlocked.Increment(ref in_set);
                    }

                    for (var i = px; i < px + chunkSize; i++)
                    {
                        for (var j = py; j < py + chunkSize; j++)
                        {
                            if (i < Width && j < Height)
                            {
                                CurrentFrame.SetPixel(i, j, PixelColor);
                            }
                        }
                    }
                }
            });

            if (chunkSize > 1)
                ChunkSizes[index] /= 2;

            if (in_set == Width * Height) StopRender();
        }

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        public void RenderFrame()
        {
            // Fire frame start event
            FrameStart();

            if (Gradual)
            {
                if (CellX == 0 && CellY == 0)
                    AlgorithmProvider.FrameStart();
                IncrementCellCoords();
                RenderCell();
                if (CellX == 0 && CellY == 0)
                    AlgorithmProvider.FrameEnd();
            }
            else
            {
                AlgorithmProvider.FrameStart();

                for (CellX = 0; CellX < TotalCellsX; CellX++)
                {
                    for (CellY = 0; CellY < TotalCellsY; CellY++)
                    {
                        RenderCell();
                    }
                }

                AlgorithmProvider.FrameEnd();
            }

            Bitmap newFrame = new Bitmap(CurrentFrame.Bitmap);
            FrameEnd(newFrame);
        }

        // Method that signals the render process to stop.  
        public void StopRender()
        {
            Job.Cancel();
            RenderHalted();
        }

        #endregion
    }

    class PointMapper<T> {
        private GenericMath<T> TMath;
        private T inXMin, inXMax, inYMin, inYMax;
        private T outXMin, outXMax, outYMin, outYMax;

        public PointMapper(object TMath) {
            this.TMath = TMath as GenericMath<T>;
        }

        public void SetInputSpace(BigDecimal xMin, BigDecimal xMax, BigDecimal yMin, BigDecimal yMax) {
            inXMin = TMath.fromBigDecimal(xMin);
            inXMax = TMath.fromBigDecimal(xMax);
            inYMin = TMath.fromBigDecimal(yMin);
            inYMax = TMath.fromBigDecimal(yMax);
        }
        public void SetOutputSpace(BigDecimal xMin, BigDecimal xMax, BigDecimal yMin, BigDecimal yMax)
        {
            outXMin = TMath.fromBigDecimal(xMin);
            outXMax = TMath.fromBigDecimal(xMax);
            outYMin = TMath.fromBigDecimal(yMin);
            outYMax = TMath.fromBigDecimal(yMax);
        }
        public T MapPointX(double x)
        {
            T real = Utils.Map<T>(TMath, TMath.fromDouble(x), inXMin, inXMax, outXMin, outXMax);
            return real;
        }
        public T MapPointY(double y)
        {
            T imag = Utils.Map<T>(TMath, TMath.fromDouble(y), inYMin, inYMax, outYMin, outYMax);
            return imag;
        }
    }
}
