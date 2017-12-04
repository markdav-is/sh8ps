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
                    DirectionDegree = GeRandoDirection(),
                    Gravity = level,
                    Speed = level
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

        private (double x, double y) GetVectorEndpoint(Sh8pe sh8pe)
        {
            return GetVectorEndpoint(
                Canvas.GetLeft(sh8pe.Shape),
                Canvas.GetTop(sh8pe.Shape),
                sh8pe.DirectionDegree,
                sh8pe.Speed
                );
        }

        private (double x, double y) GetVectorEndpoint(double x, double y, double angle, double velocity) {
            var velocity_X = velocity * Math.Cos(angle);
            var velocity_Y = velocity * Math.Sin(angle);
            return (x + velocity_X, y + velocity_Y);
        }

        private void GameTimer_Tick(object sender, object e)
        {
            gameTimer.Stop();

            foreach (var sh8pe in Targets)
            {
                // move the shapes
                var newXY = GetVectorEndpoint(sh8pe);
                Canvas.SetTop(sh8pe.Shape, newXY.y);
                Canvas.SetLeft(sh8pe.Shape, newXY.x);
                sh8pe.Shape.Width += sh8pe.Speed;
                sh8pe.Shape.Height += sh8pe.Speed;
            }

            // move the seekers
            foreach (var sh8pe in Targets.Where(_=>_.Seeker!=null))
            {
                var dir = GetDirecton(
                            new Point((int)Canvas.GetLeft(sh8pe.Shape),
                                      (int)Canvas.GetTop(sh8pe.Shape)),
                            new Point((int)Canvas.GetLeft(sh8pe.Seeker),
                                      (int)Canvas.GetTop(sh8pe.Seeker)));

                var newXY = GetVectorEndpoint(
                   Canvas.GetLeft(sh8pe.Seeker),
                   Canvas.GetTop(sh8pe.Seeker),
                   dir,
                   sh8pe.Speed * 2);

                Canvas.SetTop(sh8pe.Seeker, newXY.y);
                Canvas.SetLeft(sh8pe.Seeker, newXY.x);

                sh8pe.Seeker.Width -= sh8pe.Speed;
                sh8pe.Seeker.Height -= sh8pe.Speed;

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

        private bool Collision(Shape a, Shape b) {
            double bottom = Canvas.GetTop(a) + a.Height;
            double top = Canvas.GetTop(a);
            double left = Canvas.GetLeft(a);
            double right = Canvas.GetLeft(a) + a.Width;

            return !((bottom < Canvas.GetTop(b)) ||
                         (top > Canvas.GetTop(b) + b.Height) ||
                         (left > Canvas.GetLeft(b) + b.Width) ||
                         (right < Canvas.GetLeft(b)));
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
