using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Linq; // для list

namespace CG_lab_1
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);

        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

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

        public int Clamp(int value, int min, int max)
        {
            if (value < min) { return min; }
            if (value > max) { return max; }

            return value;
        }
    }

    // ======================= Точечные фильтры =======================
    class InvertFilter : Filters
    {
        public InvertFilter() { }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);

            return resultColor;
        }
    }  // инверсия ( точечный )

    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);

            int intensity = (int)((sourceColor.R * 0.36) + (sourceColor.G * 0.53) + (sourceColor.B * 0.11));

            Color resultColor = Color.FromArgb(intensity, intensity, intensity); // делает цвета по R, G, B соответственно

            return resultColor;
        }
    } // черно-белый фильтр ( точечный )

    class Sepiya : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);

            int intensity = (int)((sourceColor.R * 0.36) + (sourceColor.G * 0.53) + (sourceColor.B * 0.11));

            int resultR = (int)(intensity + 2 * 9);
            int resultB = (int)(intensity + 0.5 * 9);
            int resultG = (int)(intensity - 1 * 9);

            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    } // сепия ( точечный )

    class BrightnessFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);

            int resultR = sourceColor.R + 50;
            int resultG = sourceColor.G + 50;
            int resultB = sourceColor.B + 50;

            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    } // увеличивание яркости ( точечный )

    // ======== Геометрические фильтры ========
    class MoveFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            if (x + 50 < sourceImage.Width) 
            { 
                return sourceImage.GetPixel(x + 50, y); 
            }
            else { return Color.FromArgb(0, 0, 0); }
        }
    } // сдвиг влево ( точечный )

    class Revolut90Filet : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int x0 = sourceImage.Width / 2;
            int y0 = sourceImage.Height / 2; // x0, y0 - центр поворота

            double phi = Math.PI / 2; // угол поворота

            int new_x = (int)((x - x0) * Math.Cos(phi)) - (int)((y - y0) * Math.Sin(phi)) + x0;
            int new_y = (int)((x - x0) * Math.Sin(phi)) + (int)((y - y0) * Math.Cos(phi)) + y0;

            if (new_x >= 0 && new_x < sourceImage.Width && new_y >= 0 && new_y < sourceImage.Height)
            {
                return sourceImage.GetPixel(new_x, new_y);
            }
            else
            {
                return Color.FromArgb(0, 0, 0);
            }
        }
    } // перевернуть фото на 90 градусов ( точечный )

    class Revolut180Filet : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int x0 = sourceImage.Width / 2;
            int y0 = sourceImage.Height / 2; // x0, y0 - центр поворота

            double phi = Math.PI; // угол поворота

            int new_x = (int)((x - x0) * Math.Cos(phi)) - (int)((y - y0) * Math.Sin(phi)) + x0;
            int new_y = (int)((x - x0) * Math.Sin(phi)) + (int)((y - y0) * Math.Cos(phi)) + y0;

            if (new_x >= 0 && new_x < sourceImage.Width && new_y >= 0 && new_y < sourceImage.Height)
            {
                return sourceImage.GetPixel(new_x, new_y);
            }
            else
            {
                return Color.FromArgb(0, 0, 0);
            }
        }
    } // перевернуть фото на 180 градусов ( точечный )

    class WaveFirstFilet : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int newX = x + (int)(20 * Math.Sin(2 * Math.PI * x / 60));
            int newY = y;

            newX = Clamp(newX, 0, sourceImage.Width - 1);
            newY = Clamp(newY, 0, sourceImage.Height - 1);

            return sourceImage.GetPixel(newX, newY);
        }
    } // вертикальные волны ( точечный )

    class WaveSecondFilet : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int newX = x + (int)(20 * Math.Sin(2 * Math.PI * y / 30));
            int newY = y;

            newX = Clamp(newX, 0, sourceImage.Width - 1);
            newY = Clamp(newY, 0, sourceImage.Height - 1);

            return sourceImage.GetPixel(newX, newY);
        }
    } // горизонтальные волны ( точечный )

    class GlassFilet : Filters
    {
        protected Random rand = new Random();

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int newX = x + (int)((rand.NextDouble() - 0.5) * 10);
            int newY = y + (int)((rand.NextDouble() - 0.5) * 10);
            // .NextDouble - возвращает случайное число типа double, которое больше или равно 0.0 и меньше 1.0

            newX = Clamp(newX, 0, sourceImage.Width - 1);
            newY = Clamp(newY, 0, sourceImage.Height - 1);

            return sourceImage.GetPixel(newX, newY);
        }
    } // Эффект "Стекла" ( точечный )

    // ======== Нелинейные фильтры ========
    class MedianFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            List<int> AllR = new List<int>();
            List<int> AllG = new List<int>();
            List<int> AllB = new List<int>();

            int radiusX = 1;
            int radiusY = 1;

            for (int k = -radiusX; k <= radiusX; k++)
            {
                for (int l = -radiusY; l <= radiusY; l++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color color = sourceImage.GetPixel(idX, idY);

                    AllR.Add(color.R);
                    AllG.Add(color.G);
                    AllB.Add(color.B);
                }
            }

            AllR.Sort();
            AllG.Sort();
            AllB.Sort();

            return Color.FromArgb(AllR[AllR.Count() / 2], AllG[AllG.Count() / 2], AllB[AllB.Count() / 2]);
            // При медианной фильтрации (i,j)-му пикселу присваивается медианное значение яркости,
            // т.е. такое значение, частота которого равна 0,5.
        }
    } // Медианный фильтр ( нелинейный )

    // ======================= Матричные фильтры =======================
    abstract class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }

            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }

    }

    // ======== Размытие ========
    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 5;
            int sizeY = 5;

            kernel = new float[sizeX, sizeY];

            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
                }
            }

        }
    } // обычное размытие ( матричный )

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKarnel(int radius, float sigma)
        {
            int size = 2 * radius + 1; // определяем размер ядра

            kernel = new float[size, size]; // создаем ядро фильтра

            float norm = 0; // коэффицент нормировки ядра

            for (int i = -radius; i <= radius; i++) // рассчитываем ядро линейного фильтра
            {
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            }

            for (int i = 0; i < size; i++)  // нормируем ядро
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] /= norm;
                }
            }
        }
        public GaussianFilter()
        {
            createGaussianKarnel(3, 2);
        }
    } // размытие по Гаусу ( матричный )

    class MotionBlurFilter : MatrixFilter
    {
        public MotionBlurFilter()
        {
            int size = 5; // кол-во стобцов или строк

            kernel = new float[size, size];

            for (int i = 0; i < size; i++)
            {
                kernel[i, i] = 1.0f / (float)(size);
            }

        }
    } // размытие в движении ( матричный )

    // ======== Резкость ========
    class SharpnessFilter : MatrixFilter
    {
        public SharpnessFilter()
        {
            kernel = new float[3, 3]
               {
                   {  0,  -1,   0 },
                   { -1,   5,  -1 },
                   {  0,  -1,   0 }
               };
        }
    } // Резкость ( матричный )

    class SharpnessStrongFilter : MatrixFilter
    {
        public SharpnessStrongFilter()
        {
            kernel = new float[3, 3]
               {
                   { -1,  -1,  -1 },
                   { -1,   9,  -1 },
                   { -1,  -1,  -1 }
               };
        }
    } // Резкость сильнее, чем ранее написаная ( матричный )

    // ======== Теснение ========
    class EmbossFilter : MatrixFilter
    {
        public EmbossFilter()
        {
            kernel = new float[3, 3]
               {
                   {  0,  -1,  0 }, // с какой стороны падает свет
                   { -1,   0,  1 },
                   {  0,   1,  0 }
               };
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 128; // делает серой фото
            float resultG = 128;
            float resultB = 128;

            for (int l = -radiusX; l <= radiusX; l++)
            {
                for (int k = -radiusY; k <= radiusY; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighbourColor = sourceImage.GetPixel(idX, idY);

                    resultR += neighbourColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighbourColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighbourColor.B * kernel[k + radiusX, l + radiusY];
                }
            }

            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    } 

    // ======== Выделение границ ========
    abstract class DoubleMatrixFilters : Filters
    {
        protected float[,] kernel1 = null;
        protected float[,] kernel2 = null;
        protected DoubleMatrixFilters() { }
        public DoubleMatrixFilters(float[,] kernel1, float[,] kernel2)
        {
            this.kernel1 = kernel1;
            this.kernel2 = kernel2;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel1.GetLength(0) / 2;
            int radiusY = kernel1.GetLength(1) / 2;

            float resultR1 = 0;
            float resultG1 = 0;
            float resultB1 = 0;

            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);

                    Color neighbor = sourceImage.GetPixel(idX, idY);

                    resultR1 += neighbor.R * kernel1[k + radiusX, i + radiusY];
                    resultG1 += neighbor.G * kernel1[k + radiusX, i + radiusY];
                    resultB1 += neighbor.B * kernel1[k + radiusX, i + radiusY];
                }
            }

            int radiusX2 = kernel2.GetLength(0) / 2;
            int radiusY2 = kernel2.GetLength(1) / 2;

            float resultR2 = 0;
            float resultG2 = 0;
            float resultB2 = 0;

            for (int i = -radiusY2; i <= radiusY2; i++)
            {
                for (int k = -radiusX2; k <= radiusX2; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);

                    Color neighbor = sourceImage.GetPixel(idX, idY);

                    resultR2 += neighbor.R * kernel2[k + radiusX2, i + radiusY2];
                    resultG2 += neighbor.G * kernel2[k + radiusX2, i + radiusY2];
                    resultB2 += neighbor.B * kernel2[k + radiusX2, i + radiusY2];
                }
            }

            return Color.FromArgb(
            Clamp((int)Math.Sqrt(resultR1 * resultR1 + resultR2 * resultR2), 0, 255),
            Clamp((int)Math.Sqrt(resultG1 * resultG1 + resultG2 * resultG2), 0, 255),
            Clamp((int)Math.Sqrt(resultB1 * resultB1 + resultB2 * resultB2), 0, 255));
        }
    }

    class SobelFilter : DoubleMatrixFilters
    {
        public SobelFilter()
        {
            kernel1 = new float[3, 3] // по оси Х
               {
                   { -1,  0,  1 },
                   { -2,  0,  2 },
                   { -1,  0,  1 }
               };
            kernel2 = new float[3, 3] // по оси Y
               {
                   { -1, -2, -1 },
                   {  0,  0,  0 },
                   {  1,  2,  1 }
               };
        }
    } // Собель ( границы объектов, матричный )

    class SharraFilter : DoubleMatrixFilters
    {
        public SharraFilter()
        {
            kernel1 = new float[3, 3]
               {
                   { 3,  0, -3  },
                   { 10, 0, -10 },
                   { 3,  0, -3  }
               };

            kernel2 = new float[3, 3]
               {
                   {  3,  10,  3 },
                   {  0,   0,  0 },
                   { -3, -10, -3 }
               };
        }
    } // Щарра ( границы объектов, матричный )

    class PruittaFilter : DoubleMatrixFilters
    {
        public PruittaFilter()
        {
            kernel1 = new float[3, 3]
               {
                   { -1,  0, 1 },
                   { -1,  0, 1 },
                   { -1,  0, 1 }
               };

            kernel2 = new float[3, 3]
               {
                   { -1, -1, -1 },
                   {  0,  0,  0 },
                   {  1,  1,  1 }
               };
        }
    } // Приюитта ( границы объектов, матричный )
}

