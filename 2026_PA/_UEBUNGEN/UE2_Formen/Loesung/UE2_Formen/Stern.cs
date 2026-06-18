using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace UE2_Formen;

// =====================================================================
//  Stern : ShapeBase
//  Ein Sternpolygon mit 'Zacken' Spitzen. Die 2*Zacken Punkte liegen
//  abwechselnd auf AussenRadius und InnenRadius. Mittelpunkt = (X1, Y1).
//
//  AUFGABE 3: DependencyProperties AussenRadius (double), InnenRadius
//             (double) und Zacken (int) korrekt registrieren.
//  AUFGABE 4: CreatePathFigure() implementieren -> Stern zeichnen.
//
//  Merke:
//   * LengthConverter NUR bei den beiden Radien, NICHT bei Zacken!
//   * DP-Typ MUSS zum Property-Typ passen.
// =====================================================================
public class Stern : ShapeBase   // public, weil in XAML als <local:Stern> verwendet
{
    // ---- AUFGABE 3: DependencyProperty 'AussenRadius' (double) ----------
    // DP-Typ == Property-Typ -> typeof(double), Default 0.0.
    public static readonly DependencyProperty AussenRadiusProperty = DependencyProperty.Register(
        "AussenRadius", typeof(double), typeof(Stern),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender
                                         | FrameworkPropertyMetadataOptions.AffectsMeasure));

    [TypeConverter(typeof(LengthConverter))]   // Laenge -> ok
    public double AussenRadius
    {
        get => (double)GetValue(AussenRadiusProperty);
        set => SetValue(AussenRadiusProperty, value);
    }

    // ---- AUFGABE 3: DependencyProperty 'InnenRadius' (double) -----------
    // DP-Typ == Property-Typ -> typeof(double), Default 0.0.
    public static readonly DependencyProperty InnenRadiusProperty = DependencyProperty.Register(
        "InnenRadius", typeof(double), typeof(Stern),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender
                                         | FrameworkPropertyMetadataOptions.AffectsMeasure));

    [TypeConverter(typeof(LengthConverter))]   // Laenge -> ok
    public double InnenRadius
    {
        get => (double)GetValue(InnenRadiusProperty);
        set => SetValue(InnenRadiusProperty, value);
    }

    // ---- AUFGABE 3: DependencyProperty 'Zacken' (int, ANZAHL!) ----------
    // DP-Typ == Property-Typ -> typeof(int), Default 0.
    // HINWEIS: KEIN LengthConverter auf Zacken!
    public static readonly DependencyProperty ZackenProperty = DependencyProperty.Register(
        "Zacken", typeof(int), typeof(Stern),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender
                                       | FrameworkPropertyMetadataOptions.AffectsMeasure));

    public int Zacken
    {
        get => (int)GetValue(ZackenProperty);
        set => SetValue(ZackenProperty, value);
    }

    // ---- AUFGABE 4: den Stern zeichnen ----------------------------------
    protected override PathFigure CreatePathFigure()
    {
        Point center = new Point(X1, Y1);
        PathFigure fig = new PathFigure { StartPoint = center, IsClosed = true };

        // 1) sichere Zackenzahl: mindestens 2, damit nichts abstuerzt.
        int zacken = Math.Max(2, Zacken);

        // 2) der Stern hat 2*zacken Punkte, abwechselnd aussen / innen.
        for (int i = 0; i < 2 * zacken; i++)
        {
            double winkel = Math.PI * i / zacken;
            double r = (i % 2 == 0) ? AussenRadius : InnenRadius;
            Point p = center + new Vector(Math.Cos(winkel), Math.Sin(winkel)) * r;

            // 3) erster Punkt -> StartPoint, alle weiteren -> LineSegment.
            if (i == 0)
                fig.StartPoint = p;
            else
                fig.Segments.Add(new LineSegment(p, true));
        }

        // IsClosed = true schliesst den Stern automatisch.
        return fig;
    }
}
