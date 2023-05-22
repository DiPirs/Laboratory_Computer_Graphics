using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace OpenGL
{
    public partial class Form1 : Form
    {
        public OpenTK.Vector3 CubeColor;
        public OpenTK.Vector3 CameraPosition;
        public OpenTK.Vector3 TetrahedronColor;

        public float Depth;
        public float ReflectionCube;
        public float ReflectionTetrahedron;

        public Form1()
        {
            InitializeComponent();
        }

        //  ====================== Окно openGl ====================== //
        private void openGlControl_Paint(object sender, PaintEventArgs e)
        {
            Draw();
        }

        private void openGlControl_Load(object sender, EventArgs e)
        {
            Shaders.Init();
            Shaders.InitShaders();
        }

        private void glControl1_ReSize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, openGlControl.Width, openGlControl.Height);

            openGlControl.Invalidate();
        }

        // ====================== Отрисовка шейдеров ====================== //
        private void SetUniformVec3(string name, OpenTK.Vector3 value)
        {
            GL.Uniform3(GL.GetUniformLocation(Shaders.BasicProgramID, name), value);
        }

        private void SetUniform1f(string name, float value)
        {
            GL.Uniform1(GL.GetUniformLocation(Shaders.BasicProgramID, name), value);
        }

        private void Draw()
        {
            GL.ClearColor(Color.White);                 // указывает значение для цветового буфера 
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(Shaders.BasicProgramID);          // устанавливает программный объект как часть текущего состояния рендеринга.

            SetUniformVec3("cube_color", CubeColor);
            SetUniformVec3("camera_position", CameraPosition);
            SetUniformVec3("tetrahedron_color", TetrahedronColor);

            SetUniform1f("set_depth", Depth);
            SetUniform1f("set_reflection_cube", ReflectionCube);
            SetUniform1f("set_reflection_tetrahedron", ReflectionTetrahedron);

            // Quad
            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 1);
            GL.Vertex2(-1, -1);

            GL.TexCoord2(1, 1);
            GL.Vertex2(1, -1);

            GL.TexCoord2(1, 0);
            GL.Vertex2(1, 1);

            GL.TexCoord2(0, 0);
            GL.Vertex2(-1, 1);

            GL.End();

            openGlControl.SwapBuffers(); // копируем содержимое буфера вне экрана в буфер на экране
            GL.UseProgram(0);
        }

        // ====================== Ползунки ====================== //
        private void trackBar1_Scroll(object sender, EventArgs e) // цвет "куба" по R 
        {
            CubeColor.X = trackBar1.Value / 255.0f;
            openGlControl.Invalidate();
        }

        private void trackBar2_Scroll_1(object sender, EventArgs e) // цвет "куба" по G 
        {
            CubeColor.Y = trackBar2.Value / 255.0f;
            openGlControl.Invalidate();
        }

        private void trackBar3_Scroll_1(object sender, EventArgs e) // цвет "куба" по B 
        {
            CubeColor.Z = trackBar3.Value / 255.0f;
            openGlControl.Invalidate();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) // зеркальный куб 
        {
            if (checkBox1.Checked)
            {
                ReflectionCube = 1;
                openGlControl.Invalidate();
            }
            else
            {
                ReflectionCube = 0;
                openGlControl.Invalidate();
            }
        }

        // ====================================================== //

        // ====================================================== //

        private void trackBar4_Scroll(object sender, EventArgs e) // координаты камеры по X 
        {
            CameraPosition.X = trackBar4.Value;
            openGlControl.Invalidate();
        }

        private void trackBar5_Scroll(object sender, EventArgs e) // координаты камеры по Y 
        {
            CameraPosition.Y = trackBar5.Value;
            openGlControl.Invalidate();
        }

        private void trackBar6_Scroll(object sender, EventArgs e) // координаты камеры по Z 
        {
            CameraPosition.Z = trackBar6.Value;
            openGlControl.Invalidate();
        }

        private void trackBar7_Scroll(object sender, EventArgs e) // изменение глубины рейтрейсинга 
        {
            Depth = trackBar7.Value;
            openGlControl.Invalidate();
        }
    }
}
