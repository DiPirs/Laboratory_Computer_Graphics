using System;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;


namespace CG_Lab_2
{
    public partial class Form1 : Form
    {
        protected bool loaded = false; // чтобы не запускать отрисовку, пока не загружены данные
        protected int currentLayer = 0; // хранит номер слоя для визуализации

        protected int FrameCount; // счетчик FPS
        protected DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = true;

            trackBar2.Value = View.min;
            trackBar3.Value = View.width;

            Application.Idle += Application_Idle;
        } // предработная настройка 

        private void open_Click(object sender, EventArgs e)
        {
            OpenFileDialog image = new OpenFileDialog();

            if (image.ShowDialog() == DialogResult.OK)
            {
                string str = image.FileName;

                Bin.readBin(str);
                View.SetupView(glControl1.Width, glControl1.Height);

                trackBar1.Maximum = Bin.Z - 1; // отображение кол-ва слоев файла
                loaded = true;

                glControl1.Invalidate();
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                View.ChoiceDraw(currentLayer);   
                glControl1.SwapBuffers();
            }
            else
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // черное окно ожидания файла
                glControl1.SwapBuffers();
            }
        }

        private void glControl1_ReSize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);

            glControl1.Invalidate();
        } // новая отрисовка при изменении окна

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
            View.needReload = true;
            glControl1.Invalidate();
        } // слои

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            View.min = trackBar2.Value;
            View.needReload = true;

            glControl1.Invalidate();
        } // TF min 

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            View.width = trackBar3.Value;
            View.needReload = true;

            glControl1.Invalidate();
        } // TF width

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                View.varMode = View.VarMode.Quads;
                glControl1.Invalidate();
            }
        } // Quads

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                View.needReload = true;
                View.varMode = View.VarMode.Texture;
                glControl1.Invalidate();
            }
        } // Texture

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                View.varMode = View.VarMode.QuadStrip;
                glControl1.Invalidate();
            }
        } // QuadStrip

        private void Application_Idle(object sender, EventArgs e)
        { 
            while (glControl1.IsIdle)
            {
                displayFPS();
                glControl1.Invalidate();
            }
        } // для запуска FPS

        private void displayFPS()
        {
            if (DateTime.Now >= NextFPSUpdate)
            {
                this.Text = String.Format("CG_Lab2: Томография ( FPS {0} )", FrameCount);
                NextFPSUpdate = DateTime.Now.AddSeconds(1);
                FrameCount = 0;
            }
            FrameCount++;
        }

    }
}
