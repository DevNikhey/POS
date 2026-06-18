using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace UE2_Formen;

// =====================================================================
//  Vieleck : ShapeBase
//  Ein regelmaessiges Vieleck (Polygon) mit frei waehlbarer Eckenzahl
//  und Umkreis-Radius. Mittelpunkt = (X1, Y1).
//
//  AUFGABE 1: Die DependencyProperties Radius (double) und Ecken (int)
//             korrekt registrieren.
//  AUFGABE 2: CreatePathFigure() so implementieren, dass das Vieleck
//             tatsaechlich gezeichnet wird.
//
//  Merke (siehe Hinweise):
//   * LengthConverter NUR bei Laengen (Radius), NICHT bei Ecken (Anzahl)!
//   * DP-Typ MUSS zum Property-Typ passen (double -> typeof(double),
//     int -> typeof(int)).
//   * AffectsRender | AffectsMeasure setzen.
//   * Nur SEGMENTE zeichnen etwas (LineSegment hinzufuegen).
// =====================================================================
public class Vieleck : ShapeBase   // public, weil in XAML als <local:Vieleck> verwendet
{
    // ---- AUFGABE 1: DependencyProperty 'Radius' (double, Laenge) --------
    // DP-Typ == Property-Typ -> typeof(double), Default 0.0.
    public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
        "Radius", typeof(double), typeof(Vieleck),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender
                                         | FrameworkPropertyMetadataOptions.AffectsMeasure));

    [TypeConverter(typeof(LengthConverter))]   // Radius ist eine Laenge -> ok
    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    // ---- AUFGABE 1: DependencyProperty 'Ecken' (int, ANZAHL!) -----------
    // DP-Typ == Property-Typ -> typeof(int), Default 0.
    // HINWEIS: KEIN LengthConverter auf Ecken (das ist eine Anzahl!).
    public static readonly DependencyProperty EckenProperty = DependencyProperty.Register(
        "Ecken", typeof(int), typeof(Vieleck),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender
                                       | FrameworkPropertyMetadataOptions.AffectsMeasure));

    public int Ecken
    {
        get => (int)GetValue(EckenProperty);
        set => SetValue(EckenProperty, value);
    }

    // ---- AUFGABE 2: das Vieleck zeichnen --------------------------------
    protected override PathFigure CreatePathFigure()
    {
        Point center = new Point(X1, Y1);
        PathFigure fig = new PathFigure { StartPoint = center, IsClosed = true };

        // 1) sichere Eckenzahl: mindestens 3, damit nichts abstuerzt.
        int ecken = Math.Max(3, Ecken);

        for (int i = 0; i < ecken; i++)
        {
            // 2) Eckpunkt i gleichmaessig auf dem Umkreis (Radius) um center.
            double winkel = 2 * Math.PI * i / ecken;
            Point p = center + new Vector(Math.Cos(winkel), Math.Sin(winkel)) * Radius;

            // 3) erster Punkt -> StartPoint, alle weiteren -> LineSegment.
            if (i == 0)
                fig.StartPoint = p;
            else
                fig.Segments.Add(new LineSegment(p, true));
        }

        // IsClosed = true zieht die letzte Kante automatisch zurueck zum Start.
        return fig;
    }
}
