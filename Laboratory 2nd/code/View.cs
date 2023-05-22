using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace CG_Lab_2
{
    class View
    {
        private static int VBOtexture = 0; // хранит номер текстуры в памяти видеокарты
        private static byte[] textureBuffer;

        public static bool needReload = true;

        public static int min = 0;
        public static int width = 255;

        public static VarMode varMode = VarMode.Quads;

        public enum VarMode
        {
            Quads,
            Texture,
            QuadStrip
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) { return min; }
            if (value > max) { return max; }

            return value;
        }

        public static byte TransferFunction(short value)
        {
            int max = min + width;

            byte newVal = (byte)Clamp((value - min) * 255 / (max - min), 0, 255);

            return newVal;

        } // функция перевода значения плотностей вокселя в цвет

        public static void SetupView(int width, int height)
        {
            GL.ShadeModel(ShadingModel.Smooth); // интерполирование цветов
            
            GL.MatrixMode(MatrixMode.Projection); // Задаем матрицу проекции, равной матрице тождественного преобразования
            GL.LoadIdentity();
            GL.Ortho(0, Bin.X, 0, Bin.Y, -1, 1); // Задаем ортогональное проецирование массива данных томограммы в окно вывода
                                                 // которое попутно преобразует размеры массива в канонический видимый объем
            GL.Viewport(0, 0, width, height); // вывод в окно OpenTK
        } // настройка окна вывода

        public static void ChoiceDraw(int layerNumber)
        {
            switch(varMode)
            {
                case VarMode.Quads:
                    DrawQuads(layerNumber);
                    break;

                case VarMode.Texture:
                    if (needReload)
                    {
                        GenerateAndLoadTexture(layerNumber);
                        needReload = false;
                    }
                    DrawTexture();
                    break;

                case VarMode.QuadStrip:
                    DrawQuadStrip(layerNumber);
                    break;

                default:
                    break;
            }
        } // выбор режима отрисовки

        public static void DrawQuads(int layerNumber)
        {
            // отрисовка четырехугольниками, вершинами которых являются центры вокселов
            // текущего слоя регулярной воксельной сетки.

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // функция очищения указанных буферов
            GL.Begin(PrimitiveType.Quads); //  команда рисования вершин с заданием типа выводимого примитива.
                                           //  в нашем случае примитив - Quads

            for (int xCoord = 0; xCoord < Bin.X - 1; xCoord++)
            {
                for (int yCoord = 0; yCoord < Bin.Y - 1; yCoord++)
                {
                    byte value;

                    // 1 вершина
                    value = TransferFunction(Bin.array[xCoord + yCoord * Bin.X + layerNumber * Bin.X * Bin.Y]);

                    GL.Color3(value, value, value);
                    GL.Vertex2(xCoord, yCoord);

                    // 2 вершина
                    value = TransferFunction(Bin.array[xCoord + (yCoord + 1) * Bin.X + layerNumber * Bin.X * Bin.Y]);

                    GL.Color3(value, value, value);
                    GL.Vertex2(xCoord, yCoord + 1);

                    // 3 вершина
                    value = TransferFunction(Bin.array[xCoord + 1 + (yCoord + 1) * Bin.X + layerNumber * Bin.X * Bin.Y]);

                    GL.Color3(value, value, value);
                    GL.Vertex2(xCoord + 1, yCoord + 1);

                    // 4 вершина
                    value = TransferFunction(Bin.array[(xCoord + 1) + yCoord * Bin.X + layerNumber * Bin.X * Bin.Y]);

                    GL.Color3(value, value, value);
                    GL.Vertex2(xCoord + 1, yCoord);
                }
            }
            GL.End(); // завершение рисования
        } // каждые четыре вершины образуют четырехугольник

        private static void DrawQuadStrip(int layerNumber) // каждая следующая пара вершин образует четырехугольник вместе с парой предыдущих
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int offset = layerNumber * Bin.X * Bin.Y;

            for (int yCoord = 0; yCoord < Bin.Y - 1; yCoord++)
            {
                GL.Begin(PrimitiveType.QuadStrip);

                for (int xCoord = 0; xCoord < Bin.X; xCoord++)
                {
                    byte value;

                    // 1 вершина
                    value = TransferFunction(Bin.array[xCoord + yCoord * Bin.X + offset]);
                    GL.Color3(value, value, value);
                    GL.Vertex2(xCoord, yCoord);

                    // 2 вершина
                    value = TransferFunction(Bin.array[xCoord + (yCoord + 1) * Bin.X + offset]);
                    GL.Color3(value, value, value);
                    GL.Vertex2(xCoord, yCoord + 1);
                }

                GL.End();
            }

        }

        public static void GenerateAndLoadTexture(int layerNumber)
        {
            // отрисовка текстурой. Текущий слой томограммы
            // визуализируется как один большой четырехугольник, на который изображение
            // слоя накладывается как текстура аппаратной билинейной интерполяцией.

            int bytes = Bin.X * Bin.Y * 4; // умножаем на 4, так как потом в цикле будем использовать 4 раза буфер(i+3),
                                           // X*Y = 262.144, что и есть длина между слоями в массиве array

            int offset = layerNumber * Bin.X * Bin.Y; // ячейка данных о слое из массива array

            if (textureBuffer == null || textureBuffer.Length != bytes)
            {
                textureBuffer = new byte[bytes];
            }

            for (int i = 0; i < bytes; i += 4, offset++) // +4 идем, потому что у нас 4 переменных
            {
                byte color = TransferFunction(Bin.array[offset]);

                textureBuffer[i] = color;       // значение по R
                textureBuffer[i + 1] = color;   // значение по G
                textureBuffer[i + 2] = color;   // значение по B
                textureBuffer[i + 3] = 255;     // значение по A
            }

            GL.BindTexture(TextureTarget.Texture2D, VBOtexture); // позволяет сделать указунную текстуру активной
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Bin.X, Bin.Y, 0, 
                          OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, textureBuffer);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        public static void DrawTexture()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Texture2D); // разрешение использования текстуры
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture); // делаем текстуру активной

            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.White); // делаем белым, чтобы все цвета отображались

            // 1 вершина
            GL.TexCoord2(0f, 0f); // устанавливает текущую координату текстуры относительно вершине
            GL.Vertex2(0, 0);

            // 2 вершина
            GL.TexCoord2(0f, 1f);
            GL.Vertex2(0, Bin.Y);

            // 3 вершина
            GL.TexCoord2(1f, 1f);
            GL.Vertex2(Bin.X, Bin.Y);

            // 4 вершина
            GL.TexCoord2(1f, 0f);
            GL.Vertex2(Bin.X, 0);

            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }
    }
}
