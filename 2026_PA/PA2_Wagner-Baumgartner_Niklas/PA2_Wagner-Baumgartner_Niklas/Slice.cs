using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PA2_Wagner_Baumgartner_Niklas
{
    internal class Slice : Basis
    {

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            "Radius", typeof(double), typeof(Slice),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        [TypeConverter(typeof(LengthConverter))]
        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(
            "Angle", typeof(double), typeof(Slice),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        [TypeConverter(typeof(LengthConverter))]
        public double Angle
        {
            get => (double)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }


        protected override PathFigure CreatePathFigure()
        {
            Point Center = new Point(X1 + Radius, Y1);

            PathFigure pathFigure = new()
            {
                StartPoint = Center,
                IsClosed = true
            };

            double a0 = 0 < 0 ? 0 + 2 * Math.PI : 0;
            double a1 = Angle < 0 ? Angle + 2 * Math.PI : Angle;

            if (a1 < a0)
                a1 += Math.PI * 2;

            SweepDirection d = SweepDirection.Counterclockwise;
            bool large;

            
            large = false;
            d = (a1 - a0) > Math.PI ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
            

            Point p0 = Center + new Vector(Math.Cos(a0), Math.Sin(a0)) * Radius;
            Point p1 = Center + new Vector(Math.Cos(a1), Math.Sin(a1)) * Radius;

            pathFigure.Segments.Add(new ArcSegment(p1, new Size(Radius, Radius), 0.0, large, d, true));

            return pathFigure;
        }
    }
}
