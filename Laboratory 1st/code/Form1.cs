using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CG_lab_1
{
    public partial class Form1 : Form
    {
        Bitmap startImage;
        Bitmap[] historyImages = new Bitmap[sizeHistoryImage]; // массив, чтобы вернуться по фильтрам назад

        static int sizeHistoryImage = 5;
        static int indexHistoryImage = -1;

        protected void newHistoryImage()
        {
            sizeHistoryImage *= 2;
            Bitmap[] newHistoryImages = new Bitmap[sizeHistoryImage];

            historyImages.CopyTo(newHistoryImages, 0);
            historyImages = null;
            historyImages = newHistoryImages;
        } // выделение места для большего хранения фильтров

        public Form1()
        {
            InitializeComponent();

        }

        // =================== Визуал, backgroundWorker, кнопка "Отмена" ===================
        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(historyImages[indexHistoryImage], backgroundWorker1);

            if (backgroundWorker1.CancellationPending != true)
            {
                if (indexHistoryImage == sizeHistoryImage - 1) { newHistoryImage(); }

                indexHistoryImage++;
                historyImages[indexHistoryImage] = newImage;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = historyImages[indexHistoryImage];
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        } // кнопка отмены загрузки

        // =================== меню "Файл" ===================
        private void открытьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog LoadDialog = new OpenFileDialog();
            LoadDialog.Filter = "Image files | *.png; *.jpg; *.bmp; | All Files (*.*) | *.*";

            if (LoadDialog.ShowDialog() == DialogResult.OK) // Проверка файла
            {
                indexHistoryImage++;

                historyImages[indexHistoryImage] = new Bitmap(LoadDialog.FileName);
                startImage = historyImages[indexHistoryImage];
            }
            pictureBox1.Image = historyImages[indexHistoryImage];
            pictureBox1.Refresh();
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveDialog = new SaveFileDialog();

            SaveDialog.Filter = "Image files | *.png; *.jpg; *.bmp; | All Files (*.*) | *.*";

            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(SaveDialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        // =================== меню "Правка" ===================

        private void вернутьНазадToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (indexHistoryImage != 0)
            {
                indexHistoryImage--;
                pictureBox1.Image = historyImages[indexHistoryImage];
                pictureBox1.Refresh();
            }
            else
            {
                MessageBox.Show("Уже отображается исходное изображение!");
            }
        }

        private void вернутьКИсходномуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap[] returnToStart = new Bitmap[sizeHistoryImage];
            historyImages = returnToStart;
            indexHistoryImage = 0;
            historyImages[indexHistoryImage] = startImage;
            pictureBox1.Image = historyImages[indexHistoryImage];
            pictureBox1.Refresh();
        }

        // =================== меню "Фильтры" ===================
        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void чернобелоеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sepiya();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void яркостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BrightnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        // === Размытие ===
        private void размытиеToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void размытиеПоГауссуToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void размытиеВДвиженииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MotionBlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }
        
        // === Выделение границ ===
        private void собельToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Filters filter = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void щарраToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Filters filter = new SharraFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void прюиттаToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Filters filter = new PruittaFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        // === Резкость ===
        private void резкостьToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Filters filter = new SharpnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкостьсильнееToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Filters filter = new SharpnessStrongFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        // === Тиснение ===
        private void тиснениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new EmbossFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        // === Глобальные и нелинейные ===
        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayWorldFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void линейноеРастяжениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new AutoLevelsFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void идеальныйОтражательToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new PerfectReflectorFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void коррекцияСОпорнымЦветомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new ColorCorrectionFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void медианныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MedianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        // === Морфология ===
        private void расширениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new DilationFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new DilationFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void сужениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new ErosionFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new ErosionFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void открытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new OpeningFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new OpeningFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void закрытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new ClosingFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new ClosingFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void topHatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new TopHatFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new TopHatFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void blackHatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new BlackHatFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new BlackHatFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void gradToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new GradFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new GradFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        // === Геометрические ===
        private void переносToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MoveFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void поворотНа90ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Revolut90Filet();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void поворотНа180ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Revolut180Filet();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void волныПервыйТипToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new WaveFirstFilet();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void волныВторойТипToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new WaveSecondFilet();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void эффектСтеклаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GlassFilet();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void светящиесяКраяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BrightnessEdges();
            backgroundWorker1.RunWorkerAsync(filter);
        } 
    }
}
