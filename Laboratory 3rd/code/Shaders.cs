using System;
using OpenTK.Graphics.OpenGL;

namespace OpenGL
{
    public partial class Shaders : Form1
    {
        public static int BasicProgramID;           // для записи "создание программы"
        public static int BasicVertexShader;        // для записи "Вершинный шейдер"
        public static int BasicFragmentShader;      // для записи "Фрагментный шейдер"

        // ============= Настройка, загрузка шейдеров ============= //
        public static bool Init() // настройка для OpenGl 
        {
            GL.Enable(EnableCap.ColorMaterial);     // включает управление свойством материала с помощью текущего цвета
            GL.ShadeModel(ShadingModel.Smooth);     // включает сглаживание

            GL.Enable(EnableCap.DepthTest);         // включает тест глубины
            GL.Enable(EnableCap.CullFace);          // отключает все нелицевые грани

            GL.Enable(EnableCap.Lighting);          // включаем освещение
            GL.Enable(EnableCap.Light0);            // включает нулевой источник света

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest); // Perspective - Указывает качество интерполяции координат цвета и текстуры
                                                                            // Nicest - Следует выбрать наиболее правильный или высококачественный вариант.

            return true;
        }

        public static void loadShader(String filename, ShaderType type, int program, out int address) // загрузка шейдеров 
        {
            address = GL.CreateShader(type);                    // создает объект шейдера

            using (System.IO.StreamReader sr = new System.IO.StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());       // загружает исходный код в созданный шейдерный объект
            }

            GL.CompileShader(address);                          // комплиментирует наш шейдер
            GL.AttachShader(program, address);                  // добавляем шейдер в программу

            Console.WriteLine(GL.GetShaderInfoLog(address));    // выводит ошибку, если она есть

            /*
                На этапе компоновки производится стыковка входных переменных одного
                шейдера с выходными переменными другого, а также стыковка
                входных/выходных переменных шейдеров с соответствующими областями
                памяти в окружении
             */
        }

        public static void InitShaders() // инициализация шейдерной программы 
        {
            int status = 0;

            BasicProgramID = GL.CreateProgram();    // создаем объект программы

            loadShader("..\\..\\Shaders\\raytracing.vert", ShaderType.VertexShader, BasicProgramID, out BasicVertexShader);
            loadShader("..\\..\\Shaders\\raytracing.frag", ShaderType.FragmentShader, BasicProgramID, out BasicFragmentShader);

            GL.LinkProgram(BasicProgramID);                                                 // линкуем нашу программу
            GL.GetProgram(BasicProgramID, GetProgramParameterName.LinkStatus, out status);  // проверка на успешную линьковку

            Console.WriteLine(GL.GetProgramInfoLog(BasicProgramID));
        }
    }
}
