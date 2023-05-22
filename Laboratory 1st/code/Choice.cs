using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CG_lab_1
{
    public partial class Choice : Form
    {
        public bool answer;

        public Choice()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            answer = true;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            float[,] arr = new float[3, 3];
            int size = 3;

            answer = false;

            MathMorphols.creatMask(false, arr, size);
            Close();
        }

        public bool Answer
        {
            get { return answer; }
        }
    }
}
