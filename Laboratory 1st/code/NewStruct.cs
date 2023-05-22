using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CG_lab_1
{
    public partial class NewStruct : Form
    {
        protected int size = 3;
       
        public NewStruct()
        {
            InitializeComponent();
        }

        public void creatNewMask()
        {
            
            int r1 = 0, r2 = 0, r3 = 0, r4 = 0, r5 = 0, r6 = 0, r7 = 0, r8 = 0, r9 = 0;

            if (textBox1.Text != "") { r1 = Convert.ToInt32(textBox1.Text); }
            if (textBox2.Text != "") { r2 = Convert.ToInt32(textBox2.Text); }
            if (textBox3.Text != "") { r3 = Convert.ToInt32(textBox3.Text); }
            if (textBox4.Text != "") { r4 = Convert.ToInt32(textBox4.Text); }
            if (textBox5.Text != "") { r5 = Convert.ToInt32(textBox5.Text); }
            if (textBox6.Text != "") { r6 = Convert.ToInt32(textBox6.Text); }
            if (textBox7.Text != "") { r7 = Convert.ToInt32(textBox7.Text); }
            if (textBox8.Text != "") { r8 = Convert.ToInt32(textBox8.Text); }
            if (textBox9.Text != "") { r9 = Convert.ToInt32(textBox9.Text); }

            float[,] newMask = new float[3, 3] {
                        {r1, r2, r3},
                        {r4, r5, r6},
                        {r7, r8, r9}};

            MathMorphols.creatMask(true, newMask, size);
        }

        private void button1_Click(object sender, EventArgs e) // выбрать
        {
            creatNewMask();
            Close();
        }
    }
}
