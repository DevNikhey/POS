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
    internal class Spirale : Basis
    {

        public static readonly DependencyProperty SteigungProperty = DependencyProperty.Register(
            "Steigung", typeof(double), typeof(Spirale),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        [TypeConverter(typeof(LengthConverter))]
        public double Steigung
        {
            get => (double)GetValue(SteigungProperty);
            set => SetValue(SteigungProperty, value);
        }

        public static readonly DependencyProperty UmdrehungProperty = DependencyProperty.Register(
            "Umdrehung", typeof(double), typeof(Spirale),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        [TypeConverter(typeof(LengthConverter))]
        public int Umdrehung
        {
            get => (int)GetValue(UmdrehungProperty);
            set => SetValue(UmdrehungProperty, value);
        }

        public static readonly DependencyProperty EckenProperty = DependencyProperty.Register(
            "Ecken", typeof(double), typeof(Spirale),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        [TypeConverter(typeof(LengthConverter))]
        public int Ecken
        {
            get => (int)GetValue(EckenProperty);
            set => SetValue(EckenProperty, value);
        }


        protected override PathFigure CreatePathFigure()
        {
            Point Center = new Point(X1, Y1);

            PathFigure pathFigure = new()
            {
                StartPoint = Center,
                IsClosed = true
            };

            return pathFigure;
        }
    }
}
