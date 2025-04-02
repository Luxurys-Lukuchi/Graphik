using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

using static System.Net.Mime.MediaTypeNames;
using LiveCharts.Wpf;
using LiveCharts;

namespace Colors
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SeriesCollection RedSeries { get; set; }
        public SeriesCollection GreenSeries { get; set; }
        public SeriesCollection BlueSeries { get; set; }

        public SeriesCollection MidSeries { get; set; }
        public string[] Labels { get; set; }

        private string imagePath;
        private BitmapImage bitmapImage;
        private double[] red = new double[256];
        private double[] green = new double[256];
        private double[] blue = new double[256];
        private double[] colorMid = new double[256];

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация коллекций серий
            RedSeries = new SeriesCollection();
            GreenSeries = new SeriesCollection();
            BlueSeries = new SeriesCollection();

            MidSeries = new SeriesCollection();
            // Генерируем подписи для оси X (0-255)
            Labels = new string[256];
            for (int i = 0; i < 256; i++)
            {
                Labels[i] = i.ToString();
            }

            DataContext = this;

            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                imagePath = openFileDialog.FileName;
                LoadImage(imagePath);
                CountColors();
                UpdateCharts(); // Вызов обновления графиков после подсчета цветов
            }
        }

        private void CountColors()
        {
            Array.Clear(red, 0, red.Length);
            Array.Clear(green, 0, green.Length);
            Array.Clear(blue, 0, blue.Length);

            if (bitmapImage == null) return;

            int width = bitmapImage.PixelWidth;
            int height = bitmapImage.PixelHeight;
            int stride = width * 4; // 4 байта на пиксель (ARGB)
            byte[] pixels = new byte[height * stride];

            bitmapImage.CopyPixels(pixels, stride, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * stride + x * 4;

                    blue[pixels[index]]++;
                    green[pixels[index + 1]]++;
                    red[pixels[index + 2]]++;
                    
                }
            }

            for (int i = 0;i < 256; i++)
            {
                colorMid[i] = (red[i] + blue[i] + green[i])/3;
            }
        }

        private void UpdateCharts()
        {
            
            // Преобразуем значения в логарифмическую шкалу
            var logRed = red.Select(x => x > 0 ? Math.Log10(x) : 0).ToArray();
            var logGreen = green.Select(x => x > 0 ? Math.Log10(x) : 0).ToArray();
            var logBlue = blue.Select(x => x > 0 ? Math.Log10(x) : 0).ToArray();

            RedSeries.Clear();
            RedSeries.Add(new ColumnSeries
            {
                Title = "Красный",
                Values = new ChartValues<double>(red),
                Fill = System.Windows.Media.Brushes.Red,
                ColumnPadding = 0 // Уменьшает промежутки между столбцами
            });

            GreenSeries.Clear();
            GreenSeries.Add(new ColumnSeries
            {
                Title = "Зеленый",
                Values = new ChartValues<double>(green),
                Fill = System.Windows.Media.Brushes.Green,
                ColumnPadding = 0
            });

            BlueSeries.Clear();
            BlueSeries.Add(new ColumnSeries
            {
                Title = "Синий",
                Values = new ChartValues<double>(blue),
                Fill = System.Windows.Media.Brushes.Blue,
                ColumnPadding = 0
            });

            MidSeries.Clear();
            MidSeries.Add(new ColumnSeries
            {
                Title = "Синий",
                Values = new ChartValues<double>(colorMid),
                Fill = System.Windows.Media.Brushes.Violet,
                ColumnPadding = 0
            });
        }

        private void LoadImage(string path)
        {
            try
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(path);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                img.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
            }
        }

        private void CartesianChart_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}