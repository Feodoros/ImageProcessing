using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageProcessing
{
    public partial class Form1 : Form
    {
        private List<Bitmap> _bitmaps = new List<Bitmap>();        
        private List<Color> _checkedColors = new List<Color>();
        private Random _random = new Random();

        // TODO
        //private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        //private CancellationToken token = new CancellationToken();

        public Form1()
        {
            InitializeComponent();
            trackBar1.Enabled = saveBtn.Enabled = retryBtn.Enabled = stopBtn.Enabled = false;
            stopBtn.Visible = false;
        }

        /// <summary>
        /// Устанавливаем выбранные цвета
        /// </summary>
        private void SetColors()
        {
            if (checkedColorsList.CheckedItems.Count == 0)
            {
                _checkedColors.Add(Color.White);
            }

            else
            {
                // Выбранные цвета
                foreach (var item in checkedColorsList.CheckedItems)
                {
                    if (item.ToString() == "White")
                        _checkedColors.Add(Color.White);

                    if (item.ToString() == "Black")
                        _checkedColors.Add(Color.Black);

                    if (item.ToString() == "Red")
                        _checkedColors.Add(Color.Red);

                    if (item.ToString() == "Green")
                        _checkedColors.Add(Color.Green);

                    if (item.ToString() == "Blue")
                        _checkedColors.Add(Color.Blue);
                }
            }
            
        }

        // Работа программы
        private async void Run()
        {
            var sw = Stopwatch.StartNew();

            menuStrip1.Enabled = trackBar1.Enabled = saveBtn.Enabled = checkedColorsList.Enabled = retryBtn.Enabled = false;

            stopBtn.Enabled = true;

            _checkedColors.Clear();
            pictureBox1.Image = null;
            _bitmaps.Clear();

            SetColors();
            
            Bitmap bitmap = new Bitmap(openFileDialog1.FileName);
            
            await Task.Run( () => { RunProcessing(bitmap); });
            
            menuStrip1.Enabled = trackBar1.Enabled = saveBtn.Enabled = checkedColorsList.Enabled = retryBtn.Enabled = true;

            sw.Stop();
            Text = $"Processing time: {sw.Elapsed.Seconds} seconds";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Run();
            }
        }

        /// <summary>
        /// Заполняем список _bitmaps битмапами с определенной частью пикселей, выкрашенных в белый
        /// </summary>
        /// <param name="bitmap"></param>
        private void RunProcessing(Bitmap bitmap)
        {
            var pixels = GetPixels(bitmap);
            var pixelsInStep = (bitmap.Width * bitmap.Height) / 100;
            var currentPixelsSet = new List<Pixel>(pixels.Count - pixelsInStep);

            for (int i = 0; i < trackBar1.Maximum; i++)
            {
                for (int j = 0; j < pixelsInStep; j++)
                {                    
                    var index = _random.Next(pixels.Count);
                    currentPixelsSet.Add(pixels[index]);
                    pixels.RemoveAt(index);
                }

                var currentBitmap = new Bitmap(bitmap.Width, bitmap.Height);

                // Изначально на битмапе часть пикселей разукрашена в выбранные цвета
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color randomColor = _checkedColors[_random.Next(_checkedColors.Count)];
                        
                        currentBitmap.SetPixel(x, y, randomColor);                        
                    }
                }


                foreach (var pixel in currentPixelsSet)
                {
                    currentBitmap.SetPixel(pixel.Point.X, pixel.Point.Y, pixel.Color);
                }

                _bitmaps.Add(currentBitmap);

                this.Invoke(new Action(() =>
                {
                    Text = $"{i}%";
                }
                ));
            }

            _bitmaps.Add(bitmap);
        }

        private List<Pixel> GetPixels(Bitmap bitmap)
        {
            var pixels = new List<Pixel>(bitmap.Width * bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = new Pixel()
                    {
                        Color = bitmap.GetPixel(x, y),
                        Point = new Point()
                        {
                            X = x,
                            Y = y
                        }
                    };

                    pixels.Add(pixel);
                }
            }
            
            return pixels;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (_bitmaps == null || _bitmaps.Count == 0)
                return;

            pictureBox1.Image = _bitmaps[trackBar1.Value];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_bitmaps == null || _bitmaps.Count == 0)
                return;

            _bitmaps[trackBar1.Value].Save(@"C:\Users\Fedor\Documents\savedImage.jpg");

            Clipboard.SetText(@"C:\Users\Fedor\Documents\savedImage.jpg");

            if (File.Exists(@"C:\Users\Fedor\Documents\savedImage.jpg"))
            {
                MessageBox.Show(@"Путь сохранен в буфер обмена", "Изображение сохранено");
            }
            else
            {
                MessageBox.Show(@"Изображение не сохранено");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Run();
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {           
            _bitmaps.Clear();

            stopBtn.Enabled = false;
        }
    }
}
