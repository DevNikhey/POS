using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace WPF_Formen
{
    internal class Hexagram : Basis
    {
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register(
                "Radius",
                typeof(double),
                typeof(Hexagram),
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
            double r = Radius;
            double h = r * Math.Sqrt(3) / 2;

            Point top = new(X1, Y1 - r);
            Point bottom = new(X1, Y1 + r);

            Point leftUp = new(X1 - h, Y1 - r / 2);
            Point rightUp = new(X1 + h, Y1 - r / 2);

            Point leftDown = new(X1 - h, Y1 + r / 2);
            Point rightDown = new(X1 + h, Y1 + r / 2);

            PathFigure figure = new()
            {
                StartPoint = top,
                IsClosed = true
            };

            // ▲ Upward triangle
            figure.Segments.Add(new LineSegment(leftDown, true));
            figure.Segments.Add(new LineSegment(rightDown, true));
            figure.Segments.Add(new LineSegment(top, true));

            // ▼ Downward triangle (drawn in same figure)
            figure.Segments.Add(new LineSegment(bottom, true));
            figure.Segments.Add(new LineSegment(leftUp, true));
            figure.Segments.Add(new LineSegment(rightUp, true));
            figure.Segments.Add(new LineSegment(bottom, true));

            return figure;
        }
    }
}
