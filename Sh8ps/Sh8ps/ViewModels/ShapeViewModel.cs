using System;
using System.Linq;
using GalaSoft.MvvmLight;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using Windows.UI.Xaml.Shapes;
using System.Drawing;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Sh8ps.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using System.Numerics;
using System.Diagnostics;

namespace Sh8ps.ViewModels
{
    public class ShapeViewModel : ViewModelBase
    {

        DispatcherTimer gameTimer = null;

        public ShapeViewModel()
        {

        }

        public List<Sh8pe> Targets { get; private set; }
        public Point Center { get; private set; }
        public Canvas RootCanvas { get; private set; }
        public List<(Shape,Shape)> Seekers { get; private set; }
        private Random rnd = new Random();
        private int _targets;
        private int _level;

        internal void InitGame(Canvas root, int targets, int level)
        {
            // this is where we start the game by drawing and keeping track
            // of the shapes to be zapped
            RootCanvas = root;
            _targets = targets;
            Targets = new List<Sh8pe>();
            Center = new Point((int)root.ActualWidth / 2, (int)root.ActualHeight / 2);
            for (int i = 0; i < targets; i++)
            {
                var newshape = GeRandoTargetShape(level);
                Targets.Add(new Sh8pe() {
                    Shape = newshape,
                    ShapeVector = GeRandoVector()
                });
                root.Children.Add(newshape);
                // put new things in the middle
                Canvas.SetTop(newshape, Center.Y - (newshape.Height / 2));
                Canvas.SetLeft(newshape, Center.X - (newshape.Width / 2));
            }

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(500d);
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

        }

        private Vec GeRandoVector()
        {
            Random rnd = new Random();

            var vec = new Vec
            {
                Y = rnd.Next(-100, 100)*.001,
                X = rnd.Next(-100, 100)*.001
            };

            return vec;
        }

  
      
        private void GameTimer_Tick(object sender, object e)
        {
            gameTimer.Stop();

            foreach (var sh8pe in Targets)
            {
                var top0 = Canvas.GetTop(sh8pe.Shape);
                var left0 = Canvas.GetLeft(sh8pe.Shape);

                var topFinal = top0 + 5 * sh8pe.ShapeVector.X;
                var leftFinal = left0 + 5 * sh8pe.ShapeVector.Y;

                // move the shapes
                Canvas.SetTop(sh8pe.Shape, topFinal);
                Canvas.SetLeft(sh8pe.Shape, leftFinal);
                sh8pe.Shape.Width += 1;
                sh8pe.Shape.Height += 1;
                if (sh8pe.Seeker != null) {

                    top0 = Canvas.GetTop(sh8pe.Seeker);
                    left0 = Canvas.GetLeft(sh8pe.Seeker);
                    topFinal = top0 + 5 * sh8pe.SeekerVector.X;
                    leftFinal = left0 + 5 * sh8pe.SeekerVector.Y;

                    // move the shapes
                    Canvas.SetTop(sh8pe.Seeker, topFinal);
                    Canvas.SetLeft(sh8pe.Seeker, leftFinal);
                    sh8pe.Shape.Width -= 1;
                    sh8pe.Shape.Height -= 1;

                    // recalc the vector
                    var targetPoint = new Vec
                    {
                        X = Canvas.GetTop(sh8pe.Shape),
                        Y = Canvas.GetLeft(sh8pe.Shape)
                    };

                    var seekerPoint = new Vec
                    {
                        X = Canvas.GetTop(sh8pe.Seeker),
                        Y = Canvas.GetLeft(sh8pe.Seeker)
                    };

                    sh8pe.SeekerVector = GetVector(targetPoint, seekerPoint);


                }
            }

            //check for collisions
            foreach (var sh8pe in Targets.Where(_ => _.Seeker != null))
            {
                if (Collision(sh8pe.Shape, sh8pe.Seeker)) {
                    // do splody things
                    sh8pe.Shape = null;
                    sh8pe.Seeker = null;
                }
            }

            Targets.RemoveAll(_ => _.Shape == null & _.Seeker == null);

            if (Targets.Count() == 0) {
                // end the round.
                RootCanvas.Children.Clear();
                InitGame(RootCanvas, _targets * 2, _level + 1);
            }
            
            gameTimer.Start();
        }

        private bool Collision(Shape a, Shape b, double zone = 5) {
            var realzone = zone * zone;
            var topdis = Canvas.GetTop(a) - Canvas.GetTop(b);
            var leftdis = Canvas.GetLeft(a) - Canvas.GetLeft(b);

            var d = (topdis * topdis) + (leftdis * leftdis);
            return d < realzone;            
        } 

        private double GetDirecton(Point target, Point seeker)
        {
            float xDiff = target.X - seeker.X;
            float yDiff = target.Y - seeker.Y;
            return Math.Atan2(yDiff, xDiff) *180 / Math.PI;
        }

        private double GeRandoDirection()
        {
            Random rnd = new Random();
            int degree = rnd.Next(0, 360);
            return degree;
        }

        private Shape GeRandoTargetShape(int level)
        {
           
            int width = rnd.Next(10, 20);
            int height = rnd.Next(10, 20);
            LinearGradientBrush newRandomBrush = GetRandomGradientBrush();

            switch (level) {
                case 1:  // circles and elipses
                    return new Ellipse()
                    {
                        StrokeThickness = 1,
                        Stroke = new SolidColorBrush(Colors.Black),
                        Fill = newRandomBrush,
                        Width = width,
                        Height = height,
                    };
                case 2:  // add triangles
                    int flip = rnd.Next(1, 2);
                    if(flip==1) goto case 1;
                    return new Polygon
                    {
                        StrokeThickness = 1,
                        Stroke = new SolidColorBrush(Colors.Black),
                        Fill = newRandomBrush,
                        Width = width,
                        Height = height,
                        Points = new PointCollection() {
                            new Windows.Foundation.Point(0,0),
                            new Windows.Foundation.Point(width,0),
                            new Windows.Foundation.Point(width,height),
                        }
                    };
                //case 3:  // add more complex polygons
                //    break;
                default:
                    goto case 1;               
            }
        }

        public double Distance(Point p0, Point p1)
        {
            double dX = p1.X - p0.X;
            double dY = p1.Y - p0.Y;
            return Math.Sqrt(dX * dX + dY * dY);
        }

        public Vec GetVector(Point p0, Point p1)
        {
            
            double dX = p1.X - p0.X;
            double dY = p1.Y - p0.Y;
            double dist = Math.Sqrt(dX * dX + dY * dY);
           // dX = dX / dist;
            //dY = dY / dist;

            var result = new Vec { X = dX * 5, Y = dY * 5 };
            Debug.WriteLine($"vec: x:{result.X}, y:{result.Y}");
            return result;
        }

        public Vec GetVector(Vec p0, Vec p1)
        {
            Debug.WriteLine($"vec: p0 x:{p0.X}, p0 y:{p0.Y}");
            Debug.WriteLine($"vec: p1 x:{p1.X}, p1 y:{p1.Y}");

            double dX = p1.X - p0.X;
            double dY = p1.Y - p0.Y;
            double dist = Math.Sqrt(dX * dX + dY * dY);
            // dX = dX / dist;
            //dY = dY / dist;

            var result = new Vec { X = dX * 5, Y = dY * 5 };
            Debug.WriteLine($"vec: out x:{result.X}, out y:{result.Y}");
            return result;
        }

        internal void SeekTarget(Shape drawnShape)
        {
            //check drawn shapes agains targets
            foreach (var sh8pe in Targets.Where(_ => _.Seeker == null))
            {
                // are they the same type?
                if (sh8pe.Shape.GetType().IsAssignableFrom(drawnShape.GetType())) {
                    // match
                    sh8pe.Seeker = drawnShape;
                    sh8pe.Shape.Fill = drawnShape.Fill;

                    var targetPoint = new Vec
                    {
                        X = Canvas.GetTop(sh8pe.Shape),
                        Y = Canvas.GetLeft(sh8pe.Shape)
                    };
                    var seekerPoint = new Vec
                    {
                        X = Canvas.GetTop(sh8pe.Seeker),
                        Y = Canvas.GetLeft(sh8pe.Seeker)
                    };
          
                    sh8pe.SeekerVector = GetVector(targetPoint,seekerPoint);

                    AddAnimation(drawnShape);

                    break;  // you only get one
                }
            }

        }

        private LinearGradientBrush GetRandomGradientBrush()
        {
            // create a random linear gradient brush
            byte[] bytes1 = new byte[3];
            rnd.NextBytes(bytes1);
            byte[] bytes2 = new byte[3];
            rnd.NextBytes(bytes2);
            GradientStopCollection gradientStops = new GradientStopCollection();
            gradientStops.Add(new GradientStop() { Color = Windows.UI.Color.FromArgb(128, bytes1[0], bytes1[1], bytes1[2]), Offset = 0 });
            gradientStops.Add(new GradientStop() { Color = Windows.UI.Color.FromArgb(192, bytes2[0], bytes2[1], bytes2[2]), Offset = 1 });
            return new LinearGradientBrush(gradientStops, rnd.Next() * 360);
        }

        private void AddAnimation(Shape shape)
        {
            // apply an animation to the shape element
            Storyboard storyboard = shape.Tag as Storyboard;
            if (storyboard != null)
            {
                storyboard.Resume();
            }
            else
            {
                DoubleAnimation angleAnimation = new DoubleAnimation();
                if (rnd.NextDouble() > 0.5d)
                {
                    angleAnimation.From = 0d;
                    angleAnimation.To = 360d;
                }
                else
                {
                    angleAnimation.From = 360d;
                    angleAnimation.To = 0d;
                }
                angleAnimation.AutoReverse = false;
                angleAnimation.Duration = TimeSpan.FromSeconds(3d);
                angleAnimation.RepeatBehavior = RepeatBehavior.Forever;
                storyboard = new Storyboard();
                Storyboard.SetTargetProperty(angleAnimation, "(Shape.RenderTransform).(CompositionTransform.Rotation)");
                Storyboard.SetTarget(angleAnimation, shape);
                storyboard.Children.Add(angleAnimation);
                storyboard.Begin();
                shape.Tag = storyboard;
            }
        }

    }
}
