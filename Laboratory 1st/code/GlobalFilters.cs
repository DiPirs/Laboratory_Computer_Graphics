using System;
using System.Drawing;
using System.ComponentModel;

namespace CG_lab_1
{
    abstract class GlobalFilters : Filters
    {
        protected float r1; // среднее по каналу R
        protected float g1; // среднее по каналу G
        protected float b1; // среднее по каналу B

        protected float maxR, minR;
        protected float maxG, minG;
        protected float maxB, minB;

        protected long brightness;

        public void GetAverageColor(Bitmap sourceImage)
        {
            Color color = sourceImage.GetPixel(0, 0);

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;

            r1 = b1 = g1 = 0;

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    color = sourceImage.GetPixel(i, j);

                    resultR += color.R;
                    resultG += color.G;
                    resultB += color.B;
                }
            }

            r1 = resultR / sourceImage.Width * sourceImage.Height;
            g1 = resultG / sourceImage.Width * sourceImage.Height;
            b1 = resultB / sourceImage.Width * sourceImage.Height;
        }

        public void GetMaxColor(Bitmap sourceImage)
        {
            maxR = maxG = maxB = 0;

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color color = sourceImage.GetPixel(i, j);

                    maxR = Math.Max(maxR, color.R);
                    maxB = Math.Max(maxB, color.B);
                    maxG = Math.Max(maxG, color.G);
                }
            }
        }

        public void GetMinColor(Bitmap sourceImage)
        {
            minR = minG = minB = 255;

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color color = sourceImage.GetPixel(i, j);

                    minR = Math.Min(minR, color.R);
                    minB = Math.Min(minB, color.B);
                    minG = Math.Min(minG, color.G);
                }
            }
        }

        public void GetBrightness(Bitmap sourseImage)
        {
            brightness = 0;

            for (int i = 0; i < sourseImage.Width; i++)
            {
                for (int j = 0; j < sourseImage.Height; j++)
                {
                    long pix = 0;

                    Color color = sourseImage.GetPixel(i, j);

                    pix += color.R;
                    pix += color.G;
                    pix += color.B;
                    pix /= 3;

                    brightness += pix;
                }
            }

            brightness /= sourseImage.Width * sourseImage.Height;
        }
    }

    class GrayWorldFilter : GlobalFilters
    {
        protected float avg;

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            GetAverageColor(sourceImage);

            avg = (r1 + g1 + b1) / 3;

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));

                if (worker.CancellationPending)
                {
                    return null;
                }

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }

            return resultImage;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color color = sourceImage.GetPixel(x, y);

            float R = color.R * avg / r1;
            float G = color.G * avg / g1;
            float B = color.B * avg / b1;

            return Color.FromArgb(
                Clamp((int)R, 0, 255),
                Clamp((int)G, 0, 255),
                Clamp((int)B, 0, 255)
                );
        }
    }

    class AutoLevelsFilter : GlobalFilters
    {
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            GetMaxColor(sourceImage);
            GetMinColor(sourceImage);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * (100 - 33 - 33)) + (33 + 33));
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }

            return resultImage;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color pixel = sourceImage.GetPixel(x, y);

            float newR = (pixel.R - minR) * 255 / (maxR - minR);
            float newG = (pixel.G - minG) * 255 / (maxG - minG);
            float newB = (pixel.B - minB) * 255 / (maxB - minB);

            return Color.FromArgb(Clamp((int)newR, 0, 255),
                                  Clamp((int)newG, 0, 255),
                                  Clamp((int)newB, 0, 255));
        }
    } // линейное растяжение

    class PerfectReflectorFilter : GlobalFilters
    {
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            GetMaxColor(sourceImage);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));

                if (worker.CancellationPending)
                {
                    return null;
                }

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }

            return resultImage;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color color = sourceImage.GetPixel(x, y);

            float R = color.R * (255 / maxR);
            float G = color.G * (255 / maxG);
            float B = color.B * (255 / maxB);

            return Color.FromArgb(
                Clamp((int)R, 0, 255),
                Clamp((int)G, 0, 255),
                Clamp((int)B, 0, 255)
                );
        }
    } // идеальный отражатель

    class ColorCorrectionFilter : GlobalFilters
    {
        protected float dstR = 143; // вычислено путем тестов и считывания цветов с нормального изображения
        protected float dstB = 134;
        protected float dstG = 137;

        protected float srcR;
        protected float srcB;
        protected float srcG;

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            int scrX = 214; // точка на стенке где-то
            int scrY = 34;

            Color srcColor = sourceImage.GetPixel(scrX, scrY);

            srcR = srcColor.R;
            srcG = srcColor.G;
            srcB = srcColor.B;

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));

                if (worker.CancellationPending)
                {
                    return null;
                }

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }

            return resultImage;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color color = sourceImage.GetPixel(x, y);

            float R = color.R * (dstR / srcR);
            float G = color.G * (dstG / srcG);
            float B = color.B * (dstB / srcB);

            return Color.FromArgb(
                Clamp((int)R, 0, 255),
                Clamp((int)G, 0, 255),
                Clamp((int)B, 0, 255)
                );
        }
    } // коррекция с опорным цветом

    class MaxColorForEdges : GlobalFilters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int max_r = 0, max_g = 0, max_b = 0;
            int radiusX = 1, radiusY = 1;

            for (int k = -radiusX; k <= radiusX; k++)
            {
                for (int l = -radiusY; l <= radiusY; l++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color color = sourceImage.GetPixel(idX, idY);

                    max_r = Math.Max(max_r, color.R);
                    max_g = Math.Max(max_g, color.G);
                    max_b = Math.Max(max_b, color.B);
                }
            }
            return Color.FromArgb(max_r, max_g, max_b);
        }
    } // для краев

    class BrightnessEdges : MaxColorForEdges
    {
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {

            Filters filter1 = new MedianFilter();
            Filters filter2 = new SobelFilter();
            Filters filter3 = new MaxColorForEdges();

            return filter3.processImage(filter2.processImage(filter1.processImage(sourceImage, worker), worker), worker);

        }
    }  // свет. края 
}
