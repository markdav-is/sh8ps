using System;
using System.Collections.ObjectModel;
using System.Numerics;
using Windows.UI.Xaml.Shapes;

namespace Sh8ps.Models
{
    public class Sh8pe
    {
        public Shape Shape { get; set; }
        public Vec ShapeVector { get; set; }

        public Shape Seeker { get; internal set; }
        public Vec SeekerVector { get; set; }
    }

    public class Vec {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
