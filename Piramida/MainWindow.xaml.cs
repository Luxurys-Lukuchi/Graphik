using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Piramida
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private List<Side> edges = new List<Side>();
        private Point3D peak;
        private Point3D[] basePoints;
        private List<Point3D> allPoints = new List<Point3D>();
        private Polygon polyBase;
        private double centerX, centerY;
        private double focalLength = 500;
        private Vector3D baseNormal;
        private Random rnd = new Random();

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;
        }

        private Point Project(Point3D point)
        {
            double scale = focalLength / (focalLength + point.Z);
            double x = centerX + (point.X - centerX) * scale;
            double y = centerY + (point.Y - centerY) * scale;
            return new Point(x, y);
        }

        private void DrawBase(int n, Brush fillBrush, Canvas canvas)
        {
            double radius = 100;
            basePoints = new Point3D[n];
            double angleStep = 2 * Math.PI / n;

            // Создаем основание в плоскости XZ
            for (int i = 0; i < n; i++)
            {
                double angle = i * angleStep;
                double x = centerX + radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);
                basePoints[i] = new Point3D(x, centerY, z);
                allPoints.Add(basePoints[i]);
            }

            // Вычисляем нормаль основания (должна быть направлена вверх по Y)
            baseNormal = CalculateNormal(basePoints[0], basePoints[1], basePoints[2]);

            polyBase = new Polygon
            {
                Fill = fillBrush,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            foreach (var p in basePoints)
                polyBase.Points.Add(Project(p));

            canvas.Children.Add(polyBase);
        }

        private void MakePyramid(double height)
        {
            // Вершина смещена по Y
            peak = new Point3D(centerX, centerY + height, 0);
            allPoints.Add(peak);

            for (int i = 0; i < basePoints.Length; i++)
            {
                int next = (i + 1) % basePoints.Length;
                edges.Add(new Side(basePoints[i], basePoints[next], peak, GetRandomBrush()));
            }
        }

        private void DrawPyramid(Canvas canvas)
        {
            canvas.Children.Clear();

            // Всегда показываем основание
            polyBase.Points.Clear();
            foreach (var p in basePoints)
                polyBase.Points.Add(Project(p));
            canvas.Children.Add(polyBase);

            foreach (var edge in edges)
            {
                Vector3D normal = CalculateNormal(edge.Points[0], edge.Points[1], edge.Points[2]);
                if (IsFaceVisible(normal))
                {
                    var projectedPoints = edge.Points.Select(p => Project(p)).ToList();
                    edge.Poly.Points.Clear();

                    foreach (var point in projectedPoints)
                        edge.Poly.Points.Add(point);

                    canvas.Children.Add(edge.Poly);
                }
            }
        }

        private Vector3D CalculateNormal(Point3D a, Point3D b, Point3D c)
        {
            Vector3D v1 = new Vector3D(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Vector3D v2 = new Vector3D(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
            Vector3D normal = Vector3D.CrossProduct(v1, v2);
            normal.Normalize();
            return normal;
        }

        private bool IsFaceVisible(Vector3D normal)
        {
            Vector3D viewDirection = new Vector3D(0, 0, 1);
            return Vector3D.DotProduct(normal, viewDirection) > 0;
        }

        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            piramydeCanvas.Children.Clear();
            edges.Clear();
            allPoints.Clear();

            if (!int.TryParse(sidesInput.Text, out int sides) || sides < 3)
            {
                MessageBox.Show("Введите корректное количество сторон (≥3)");
                return;
            }

            if (!double.TryParse(heightInput.Text, out double height) || height <= 0)
            {
                MessageBox.Show("Введите корректную высоту (>0)");
                return;
            }

            centerX = piramydeCanvas.ActualWidth / 2;
            centerY = piramydeCanvas.ActualHeight / 2;

            DrawBase(sides, Brushes.Red, piramydeCanvas);
            MakePyramid(height);
            DrawPyramid(piramydeCanvas);
        }

        private void Rotate_Click(object sender, RoutedEventArgs e)
        {
            if (edges.Count == 0) return;

            if (timer.IsEnabled)
            {
                timer.Stop();
                (sender as Button).Content = "Начать вращение";
            }
            else
            {
                timer.Start();
                (sender as Button).Content = "Остановить вращение";
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            RotateObjects();
            DrawPyramid(piramydeCanvas);
        }

        private void RotateObjects()
        {
            double angle = Math.PI / 36;

            foreach (var point in allPoints)
            {
                if (check_X.IsChecked == true)
                {
                    double y = point.Y - centerY;
                    double z = point.Z;

                    point.Y = centerY + y * Math.Cos(angle) - z * Math.Sin(angle);
                    point.Z = y * Math.Sin(angle) + z * Math.Cos(angle);
                }

                if (check_Y.IsChecked == true)
                {
                    double x = point.X - centerX;
                    double z = point.Z;

                    point.X = centerX + x * Math.Cos(angle) - z * Math.Sin(angle);
                    point.Z = x * Math.Sin(angle) + z * Math.Cos(angle);
                }

                if (check_Z.IsChecked == true)
                {
                    double x = point.X - centerX;
                    double y = point.Y - centerY;

                    point.X = centerX + x * Math.Cos(angle) - y * Math.Sin(angle);
                    point.Y = centerY + x * Math.Sin(angle) + y * Math.Cos(angle);
                }
            }
        }

        private Brush GetRandomBrush()
        {
            return new SolidColorBrush(Color.FromRgb(
                (byte)rnd.Next(256),
                (byte)rnd.Next(256),
                (byte)rnd.Next(256)));
        }
    }

    public class Side
    {
        public Polygon Poly { get; }
        public Point3D[] Points { get; }

        public Side(Point3D a, Point3D b, Point3D c, Brush color)
        {
            Points = new[] { a, b, c };
            Poly = new Polygon
            {
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
        }
    }

    public class Point3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}