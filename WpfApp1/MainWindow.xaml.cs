using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace StarSimulation {
    public partial class MainWindow : Window {
        private const int InitialStarCount = 50;
        private const double MinStarSize = 2;
        private const double MaxStarSize = 5;
        private const double Speed = 1;
        private const double GrowthFactor = 0.5;
        private readonly Random _random = new Random();
        private readonly List<Star> _stars = new List<Star>();
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public MainWindow() {
            InitializeComponent();
            InitializeStars();
            _timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void InitializeStars() {
            for (int i = 0; i < InitialStarCount; i++) {
                SpawnStar();
            }
        }

        private void SpawnStar() {
            var size = _random.NextDouble() * (MaxStarSize - MinStarSize) + MinStarSize;
            var star = new Star {
                X = _random.NextDouble() * StarCanvas.ActualWidth,
                Y = _random.NextDouble() * StarCanvas.ActualHeight,
                Dx = (_random.NextDouble() - 0.5) * Speed,
                Dy = (_random.NextDouble() - 0.5) * Speed,
                Size = size
            };

            var ellipse = new Ellipse {
                Width = size,
                Height = size,
                Fill = Brushes.White
            };

            Canvas.SetLeft(ellipse, star.X);
            Canvas.SetTop(ellipse, star.Y);
            StarCanvas.Children.Add(ellipse);

            star.Shape = ellipse;
            _stars.Add(star);
        }

        private void Timer_Tick(object sender, EventArgs e) {
            for (int i = _stars.Count - 1; i >= 0; i--) {
                var star = _stars[i];
                star.X += star.Dx;
                star.Y += star.Dy;

                // Bounce off walls
                if (star.X < 0 || star.X > StarCanvas.ActualWidth - star.Size) {
                    star.Dx = -star.Dx;
                }
                if (star.Y < 0 || star.Y > StarCanvas.ActualHeight - star.Size) {
                    star.Dy = -star.Dy;
                }

                // Update position
                Canvas.SetLeft(star.Shape, star.X);
                Canvas.SetTop(star.Shape, star.Y);
            }

            // Check for collisions and consumption
            for (int i = _stars.Count - 1; i >= 0; i--) {
                for (int j = i - 1; j >= 0; j--) {
                    if (AreColliding(_stars[i], _stars[j])) {
                        if (_stars[i].Size > _stars[j].Size) {
                            ConsumeStar(_stars[i], _stars[j]);
                            _stars.RemoveAt(j);
                            i--;
                        } else if (_stars[j].Size > _stars[i].Size) {
                            ConsumeStar(_stars[j], _stars[i]);
                            _stars.RemoveAt(i);
                            break;
                        } else {
                            // Equal size, bounce off each other
                            var tempDx = _stars[i].Dx;
                            var tempDy = _stars[i].Dy;
                            _stars[i].Dx = _stars[j].Dx;
                            _stars[i].Dy = _stars[j].Dy;
                            _stars[j].Dx = tempDx;
                            _stars[j].Dy = tempDy;
                        }
                    }
                }
            }

            // Spawn new stars to maintain population
            while (_stars.Count < InitialStarCount) {
                SpawnStar();
            }
        }

        private void ConsumeStar(Star predator, Star prey) {
            double growthAmount = prey.Size * GrowthFactor;
            predator.Size += 0.2;
            predator.Shape.Width = predator.Size;
            predator.Shape.Height = predator.Size;
            StarCanvas.Children.Remove(prey.Shape);
        }

        private bool AreColliding(Star a, Star b) {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy) < (a.Size + b.Size) / 2;
        }
    }

    public class Star {
        public double X { get; set; }
        public double Y { get; set; }
        public double Dx { get; set; }
        public double Dy { get; set; }
        public double Size { get; set; }
        public Ellipse Shape { get; set; }
    }
}