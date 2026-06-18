using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace __NS__;

// =====================================================================
//  ShapeBase – Basis für eigene Zeichen-Controls (aus PA2 "Basis").
//  Ableiten und CreatePathFigure() überschreiben -> fertig.
//
//  WICHTIG aus der PA2-Bewertung:
//   * [TypeConverter(typeof(LengthConverter))] NUR bei LÄNGEN (X1, Y1, Radius,
//     Breite ...). NICHT bei Winkeln (Angle) oder Anzahlen (Ecken, Umdrehung)!
//   * DependencyProperty-Typ MUSS zum Property-Typ passen
//     (int-Property  -> typeof(int);  double-Property -> typeof(double)).
//   * AffectsRender | AffectsMeasure -> Änderung zeichnet automatisch neu.
//   * Tatsächlich zeichnen heißt: in CreatePathFigure Segmente HINZUFÜGEN
//     (LineSegment / ArcSegment), nicht nur einen StartPoint setzen.
// =====================================================================
public class ShapeBase : Shape
{
    protected override Geometry DefiningGeometry
    {
        get
        {
            PathGeometry geometry = new PathGeometry { FillRule = FillRule.EvenOdd };
            geometry.Figures.Add(CreatePathFigure());
            return geometry;
        }
    }

    public static readonly DependencyProperty X1Property = DependencyProperty.Register(
        "X1", typeof(double), typeof(ShapeBase),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender
                                         | FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty Y1Property = DependencyProperty.Register(
        "Y1", typeof(double), typeof(ShapeBase),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender
                                         | FrameworkPropertyMetadataOptions.AffectsMeasure));

    [TypeConverter(typeof(LengthConverter))]   // X1 ist eine Länge -> ok
    public double X1
    {
        get => (double)GetValue(X1Property);
        set => SetValue(X1Property, value);
    }

    [TypeConverter(typeof(LengthConverter))]   // Y1 ist eine Länge -> ok
    public double Y1
    {
        get => (double)GetValue(Y1Property);
        set => SetValue(Y1Property, value);
    }

    // Standard: ein kleines Kreuz als "Punkt". Unterklassen überschreiben das.
    protected virtual PathFigure CreatePathFigure()
    {
        PathFigure fig = new PathFigure { StartPoint = new Point(X1 - 10, Y1), IsClosed = false };
        fig.Segments.Add(new LineSegment(new Point(X1 + 10, Y1), true));
        fig.Segments.Add(new LineSegment(new Point(X1, Y1 - 10), false));
        fig.Segments.Add(new LineSegment(new Point(X1, Y1 + 10), true));
        return fig;
    }
}
