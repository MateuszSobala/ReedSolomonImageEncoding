using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace ReedSolomonImageEncoding
{
    public partial class Form1 : Form
    {
        private Bitmap _originalImage;
        private Bitmap _processedImage;
        private Bitmap _processedSimpleImage;
        private string _fileName;
        private ErrorMeasure _errorMeasure = ErrorMeasure.ErrorPercentage;

        public Form1()
        {
            InitializeComponent();
            var maxImageSize = new Size(300, 300);
            pictureBox1.MaximumSize = maxImageSize;

            pictureBox2.MaximumSize = maxImageSize;

            pictureBox3.MaximumSize = maxImageSize;

            pictureBox3.Visible = false;
            label8.Visible = false;
            label9.Visible = false;
            label10.Visible = false;

            pictureBox4.Visible = false;
            label11.Visible = false;
            label12.Visible = false;
            label15.Visible = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                label3.Text = radioButton1.Text + ":";
                textBox3.Text = 5.ToString();
                _errorMeasure = ErrorMeasure.ErrorPercentage;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                label3.Text = radioButton2.Text + ":";
                textBox3.Text = 1000.ToString();
                _errorMeasure = ErrorMeasure.ErrorCount;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                label3.Text = radioButton3.Text + ":";
                textBox3.Text = 16.ToString();
                _errorMeasure = ErrorMeasure.ErrorCountPerBlock;
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                label3.Text = radioButton4.Text + ":";
                textBox3.Text = 0.1M.ToString(CultureInfo.InvariantCulture);
                _errorMeasure = ErrorMeasure.ErrorProbability;
            }
        }

        private void radioButton5_CheckedChanged_1(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
            {
                label3.Text = radioButton5.Text + ":";
                textBox3.Text = 16.ToString();
                _errorMeasure = ErrorMeasure.ErrorGroupCountPerBlock;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            var fileStream = openFileDialog1.OpenFile();
            _fileName = openFileDialog1.FileName;
            using (fileStream)
            {
                _originalImage = new Bitmap(fileStream);
            }

            fileStream.Close();

            pictureBox1.Image = _originalImage;
            pictureBox1.Height = _originalImage.Height;
            pictureBox1.Width = _originalImage.Width;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!ValidateUserInputs())
                return;
            var stopwatch = new Stopwatch();

            stopwatch.Reset();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < 1200)  // A Warmup of 1000-1500 mS 
            // stabilizes the CPU cache and pipeline.
            {
                // Warmup
            }
            stopwatch.Stop();

            stopwatch.Reset();
            stopwatch.Start();

            ProcessImageEncoding();

            stopwatch.Stop();

            textBox4.Text = stopwatch.ElapsedMilliseconds.ToString();

            pictureBox2.Image = _processedImage;

            var comparisonText = new StringBuilder();
            Bitmap diffImage;

            var diffCount = ImageProcessing.Compare(_processedImage, _originalImage, out diffImage);
            if (diffCount == 0)
            {
                comparisonText.Append("Obrazki są identyczne.");
                pictureBox3.Visible = false;
                label9.Visible = false;
                label8.Visible = false;
            }
            else
            {
                comparisonText.Append("Obrazki nie są identyczne. ");
                comparisonText.Append("Różnią się ");
                comparisonText.Append(diffCount);
                comparisonText.Append(" pixelami.");

                pictureBox3.Image = diffImage;
                pictureBox3.Visible = true;
                label9.Visible = true;
                label8.Visible = true;
            }

            label10.Text = comparisonText.ToString();
            label10.Visible = true;   
        }

        private bool ValidateUserInputs()
        {
            if (_originalImage == null)
            {
                MessageBox.Show("Proszę wybrać obrazek do przetworzenia.", "RS koder map bitowych", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Proszę podać miarę liczby błędów.", "RS koder map bitowych", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Proszę podać liczbę bajtów korekcyjnych.", "RS koder map bitowych", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void ProcessImageEncoding()
        {
            var data = ImageProcessing.GetRawBytesFromRGBImage(_originalImage);
            var correctionBytes = int.Parse(textBox1.Text);

            var reedSolomon = new ReedSolomon(correctionBytes);
            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            textBox6.Text = SimulateTransmissionWithErrors(modifiedData).ToString();

            reedSolomon.DecodeRawBytesArray(modifiedData, data);

            _processedImage = ImageProcessing.GetRGBImageFromRawBytes(_originalImage, data);
        }

        private void ProcessImageSimpleEncoding()
        {
            var data = ImageProcessing.GetRawBytesFromRGBImage(_originalImage);
            var correctionBytes = int.Parse(textBox1.Text);

            var reedSolomon = new ReedSolomon(correctionBytes);
            var modifiedData = reedSolomon.EncodeRawBytesArray(data);

            textBox7.Text = SimulateTransmissionWithErrors(modifiedData).ToString();

            reedSolomon.SimplyDecodeRawBytesArray(modifiedData, data);

            _processedSimpleImage = ImageProcessing.GetRGBImageFromRawBytes(_originalImage, data);
        }

        private int SimulateTransmissionWithErrors(int[] data)
        {
            var errorMeasureValue = double.Parse(textBox3.Text, CultureInfo.InvariantCulture);
            
            var blockSize = int.Parse(textBox2.Text);

            var errorsCount = 0;

            switch (_errorMeasure)
            {
                case ErrorMeasure.ErrorCount:
                    errorsCount = ErrorProvider.FillInErrors(data, (int)errorMeasureValue);
                    break;
                case ErrorMeasure.ErrorPercentage:
                    errorsCount = ErrorProvider.FillInPercentageOfErrors(data, (int)errorMeasureValue);
                    break;
                case ErrorMeasure.ErrorCountPerBlock:
                    errorsCount = ErrorProvider.FillInErrorsForEveryBlock(data, (int)errorMeasureValue, blockSize);
                    break;
                case ErrorMeasure.ErrorProbability:
                    errorsCount = ErrorProvider.FillInErrorsWithProbability(data, errorMeasureValue);
                    break;
                case ErrorMeasure.ErrorGroupCountPerBlock:
                    errorsCount = ErrorProvider.FillInGroupErrorsForEveryBlock(data, (int)errorMeasureValue, blockSize);
                    break;
            }

            return errorsCount;
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!ValidateUserInputs())
                return;
            var stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < 1200)  // A Warmup of 1000-1500 mS 
            // stabilizes the CPU cache and pipeline.
            {
                // Warmup
            }
            stopwatch.Stop();

            stopwatch.Reset();
            stopwatch.Start();

            ProcessImageSimpleEncoding();

            stopwatch.Stop();

            textBox5.Text = stopwatch.ElapsedMilliseconds.ToString();

            pictureBox5.Image = _processedSimpleImage;

            var comparisonText = new StringBuilder();
            Bitmap diffImage;
            var diffCount = ImageProcessing.Compare(_processedSimpleImage, _originalImage, out diffImage);
            if (diffCount == 0)
            {
                comparisonText.Append("Obrazki są identyczne.");
                pictureBox4.Visible = false;
                label11.Visible = false;
                label12.Visible = false;
            }
            else
            {
                comparisonText.Append("Obrazki nie są identyczne. ");
                comparisonText.Append("Różnią się ");
                comparisonText.Append(diffCount);
                comparisonText.Append(" pixelami.");

                pictureBox4.Image = diffImage;
                pictureBox4.Visible = true;
                label11.Visible = true;
                label12.Visible = true;
            }

            label15.Text = comparisonText.ToString();
            label15.Visible = true;
        }
    }
}
