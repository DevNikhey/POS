using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace WPF_Formen
{
    internal class Star : Basis
    {
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register(
                "Radius",
                typeof(double),
                typeof(Star),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        [TypeConverter(typeof(LengthConverter))]
        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        protected override PathFigure CreatePathFigure()
        {
            double rOuter = Radius;
            double rInner = Radius / 2;
            double cx = X1;
            double cy = Y1;

            Point[] points = new Point[12];

            for (int i = 0; i < 12; i++)
            {
                double angleDeg = -90 + i * 30; // start at top
                double angleRad = angleDeg * Math.PI / 180;

                double r = (i % 2 == 0) ? rOuter : rInner;

                points[i] = new Point(
                    cx + r * Math.Cos(angleRad),
                    cy + r * Math.Sin(angleRad)
                );
            }

            PathFigure figure = new()
            {
                StartPoint = points[0],
                IsClosed = true
            };

            for (int i = 1; i < points.Length; i++)
            {
                figure.Segments.Add(new LineSegment(points[i], true));
            }

            return figure;
        }
    }
}
