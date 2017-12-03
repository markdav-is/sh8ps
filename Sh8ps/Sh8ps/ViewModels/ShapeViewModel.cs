using System;

using GalaSoft.MvvmLight;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using Windows.UI.Xaml.Shapes;
using System.Drawing;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Sh8ps.Models;
using Windows.UI.Xaml;

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

        internal void InitGame(Canvas root, InkCanvas inkCanvas, int targets, int level)
        {
            // this is where we start the game by drawing and keeping track
            // of the shapes to be zapped
            Targets = new List<Sh8pe>();
            Center = new Point((int)root.Width / 2, (int)root.Height / 2);
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
            }

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(500d);
            gameTimer.Tick += GameTimer_Tick;


        }

        private void GameTimer_Tick(object sender, object e)
        {
            gameTimer.Stop();

            foreach (var sh8pe in Targets)
            {

            }
            
            gameTimer.Start();
        }

        private double GeRandoDirection()
        {
            Random rnd = new Random();
            int degree = rnd.Next(0, 360);
            return degree;
        }

        private Shape GeRandoTargetShape(int level)
        {
            switch (level) {
                case 1:  // circles and elipses
                    return new Ellipse()
                    {
                        StrokeThickness = 1,
                        Stroke = new SolidColorBrush(Colors.Black),
                        Width = 10
                    };
                //case 2:  // add triangles
                //    break;
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
    }
}
