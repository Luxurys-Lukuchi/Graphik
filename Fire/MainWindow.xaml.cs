using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace FireRoom3D
{
    public partial class MainWindow : Window
    {
        private readonly List<Particle> particles = new List<Particle>();
        private readonly Random random = new Random();
        private readonly Point3D fireOrigin = new Point3D(0, -3, 0);
        private DiffuseMaterial fireMaterial;
        private GeometryModel3D fireModel;
        private bool isAnimating = true;
        private const int ParticleCount = 500;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCamera();
            InitializeRoom();
            InitializeFire();
            CompositionTarget.Rendering += AnimateFire;
        }

        private void InitializeCamera()
        {
            viewport.Camera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, 15),
                LookDirection = new Vector3D(0, 0, -1),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 60
            };
        }

        //private void InitializeRoom()
        //{
        //    const double floorLevel = -5;
        //    const double tileSize = 2.0;
        //    const int tileCount = 10;
        //    const double floorThickness = 0.1;
        //    const double wallHeight = 8; // Высота стен
        //    const double wallThickness = 0.2;

        //    // Материалы для пола (без изменений)
        //    var darkMaterial = new DiffuseMaterial(Brushes.DarkGray);
        //    var lightMaterial = new DiffuseMaterial(Brushes.LightGray);

        //    // Шахматный пол (ваш существующий код)
        //    var meshBuilder = new MeshBuilder();
        //    for (int x = 0; x < tileCount; x++)
        //    {
        //        for (int z = 0; z < tileCount; z++)
        //        {
        //            var material = (x + z) % 2 == 0 ? lightMaterial : darkMaterial;
        //            var center = new Point3D(
        //                x * tileSize - (tileCount * tileSize) / 2 + tileSize / 2,
        //                floorLevel,
        //                z * tileSize - (tileCount * tileSize) / 2 + tileSize / 2);

        //            meshBuilder.AddBox(center, tileSize, floorThickness, tileSize);
        //            var tileModel = new GeometryModel3D(meshBuilder.ToMesh(), material);
        //            viewport.Children.Add(new ModelVisual3D { Content = tileModel });
        //            meshBuilder = new MeshBuilder();
        //        }
        //    }

        //    // Добавляем прозрачные стены (новый код)
        //    var roomSize = tileCount * tileSize;
        //    var transparentWallMaterial = new DiffuseMaterial(
        //        new SolidColorBrush(Color.FromArgb(96, 200, 200, 255)) // 37% прозрачности
        //    );

        //    // 1. Задняя стена
        //    meshBuilder.AddBox(
        //        new Point3D(0, floorLevel + wallHeight / 2, -roomSize / 2),
        //        roomSize + wallThickness * 2,
        //        wallHeight,
        //        wallThickness
        //    );
        //    viewport.Children.Add(new ModelVisual3D
        //    {
        //        Content = new GeometryModel3D(meshBuilder.ToMesh(), transparentWallMaterial)
        //    });
        //    meshBuilder = new MeshBuilder();

        //    // 2. Передняя стена
        //    meshBuilder.AddBox(
        //        new Point3D(0, floorLevel + wallHeight / 2, roomSize / 2),
        //        roomSize + wallThickness * 2,
        //        wallHeight,
        //        wallThickness
        //    );
        //    viewport.Children.Add(new ModelVisual3D
        //    {
        //        Content = new GeometryModel3D(meshBuilder.ToMesh(), transparentWallMaterial)
        //    });
        //    meshBuilder = new MeshBuilder();

        //    // 3. Боковые стены
        //    meshBuilder.AddBox(
        //        new Point3D(-roomSize / 2, floorLevel + wallHeight / 2, 0),
        //        wallThickness,
        //        wallHeight,
        //        roomSize
        //    );
        //    viewport.Children.Add(new ModelVisual3D
        //    {
        //        Content = new GeometryModel3D(meshBuilder.ToMesh(), transparentWallMaterial)
        //    });
        //    meshBuilder = new MeshBuilder();

        //    meshBuilder.AddBox(
        //        new Point3D(roomSize / 2, floorLevel + wallHeight / 2, 0),
        //        wallThickness,
        //        wallHeight,
        //        roomSize
        //    );
        //    viewport.Children.Add(new ModelVisual3D
        //    {
        //        Content = new GeometryModel3D(meshBuilder.ToMesh(), transparentWallMaterial)
        //    });

        //    // Освещение (ваш существующий код)
        //    var light = new PointLight()
        //    {
        //        Position = new Point3D(0, 10, 0),
        //        Color = Colors.White,
        //        Range = 50
        //    };
        //    viewport.Children.Add(new ModelVisual3D { Content = light });
        //}

        private void InitializeRoom()
        {
            const double floorLevel = -5;
            const double tileSize = 2.0; // Размер одной клетки
            const int tileCount = 10;    // Количество клеток в ряду (10x10)
            const double floorThickness = 0.1;

            var meshBuilder = new MeshBuilder();
            var darkMaterial = new DiffuseMaterial(Brushes.DarkGray);
            var lightMaterial = new DiffuseMaterial(Brushes.LightGray);

            for (int x = 0; x < tileCount; x++)
            {
                for (int z = 0; z < tileCount; z++)
                {
                    // Определяем цвет клетки (шахматный порядок)
                    var material = (x + z) % 2 == 0 ? lightMaterial : darkMaterial;

                    // Центр текущей клетки
                    var center = new Point3D(
                        x * tileSize - (tileCount * tileSize) / 2 + tileSize / 2,
                        floorLevel,
                        z * tileSize - (tileCount * tileSize) / 2 + tileSize / 2);

                    // Добавляем клетку
                    meshBuilder.AddBox(center, tileSize, floorThickness, tileSize);

                    // Добавляем модель с материалом
                    var tileModel = new GeometryModel3D(meshBuilder.ToMesh(), material);
                    viewport.Children.Add(new ModelVisual3D { Content = tileModel });

                    meshBuilder = new MeshBuilder(); // Сбрасываем для следующей клетки
                }
            }

            // Освещение для лучшей видимости
            var light = new PointLight()
            {
                Position = new Point3D(0, 10, 0),
                Color = Colors.White,
                Range = 50
            };
            viewport.Children.Add(new ModelVisual3D { Content = light });
        }

        private void InitializeFire()
        {
            // Материал огня с прозрачностью
            var gradient = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(255, 255, 255, 100), 0),
                    new GradientStop(Color.FromArgb(200, 255, 150, 50), 0.4),
                    new GradientStop(Color.FromArgb(150, 200, 50, 0), 0.8),
                    new GradientStop(Color.FromArgb(0, 0, 0, 0), 1)
                }
            };

            fireMaterial = new DiffuseMaterial(gradient) { AmbientColor = Colors.OrangeRed };
            fireModel = new GeometryModel3D { Material = fireMaterial, BackMaterial = fireMaterial };
            viewport.Children.Add(new ModelVisual3D { Content = fireModel });

            // Создание частиц
            for (int i = 0; i < ParticleCount; i++)
            {
                particles.Add(CreateNewParticle());
            }
        }

        private Particle CreateNewParticle()
        {
            return new Particle(
                position: fireOrigin,
                velocity: new Vector3D(
                    (random.NextDouble() - 0.5) * 0.3,
                    random.NextDouble() * 1.2 + 0.3,
                    (random.NextDouble() - 0.5) * 0.3),
                lifeTime: random.NextDouble() * 4 + 1,
                size: random.NextDouble() * 0.4 + 0.1
            );
        }

        private void AnimateFire(object sender, EventArgs e)
        {
            if (!isAnimating) return;

            var meshBuilder = new MeshBuilder();

            for (int i = 0; i < particles.Count; i++)
            {
                var particle = particles[i];

                // Обновляем частицу
                var updatedParticle = new Particle(
                    position: particle.Position + particle.Velocity * 0.03,
                    velocity: new Vector3D(
                        particle.Velocity.X * 0.99,
                        particle.Velocity.Y * 0.97 + 0.01,
                        particle.Velocity.Z * 0.99),
                    lifeTime: particle.LifeTime - 0.02,
                    size: particle.Size * 0.995
                );

                // Заменяем частицу в списке
                particles[i] = updatedParticle.LifeTime > 0 ? updatedParticle : CreateNewParticle();

                // Добавляем сферу для частицы
                meshBuilder.AddSphere(particles[i].Position, particles[i].Size);
            }

            fireModel.Geometry = meshBuilder.ToMesh();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            isAnimating = true;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            isAnimating = false;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        private void BtnResetCamera_Click(object sender, RoutedEventArgs e)
        {
            viewport.ZoomExtents();
        }

        protected override void OnClosed(EventArgs e)
        {
            CompositionTarget.Rendering -= AnimateFire;
            base.OnClosed(e);
        }
    }

    public struct Particle
    {
        public Point3D Position { get; }
        public Vector3D Velocity { get; }
        public double LifeTime { get; }
        public double Size { get; }

        public Particle(Point3D position, Vector3D velocity, double lifeTime, double size)
        {
            Position = position;
            Velocity = velocity;
            LifeTime = lifeTime;
            Size = size;
        }
    }
}