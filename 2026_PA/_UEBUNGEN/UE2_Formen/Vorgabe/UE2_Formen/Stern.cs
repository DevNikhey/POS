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
    // TODO: registrieren (Default 0.0, AffectsRender | AffectsMeasure).
    // public static readonly DependencyProperty AussenRadiusProperty = ...

    [TypeConverter(typeof(LengthConverter))]   // Laenge -> ok
    public double AussenRadius
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    // ---- AUFGABE 3: DependencyProperty 'InnenRadius' (double) -----------
    // TODO: registrieren (Default 0.0, AffectsRender | AffectsMeasure).
    // public static readonly DependencyProperty InnenRadiusProperty = ...

    [TypeConverter(typeof(LengthConverter))]   // Laenge -> ok
    public double InnenRadius
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    // ---- AUFGABE 3: DependencyProperty 'Zacken' (int, ANZAHL!) ----------
    // TODO: registrieren (Default 0, AffectsRender | AffectsMeasure).
    // HINWEIS: KEIN LengthConverter auf Zacken!
    // public static readonly DependencyProperty ZackenProperty = ...

    public int Zacken
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    // ---- AUFGABE 4: den Stern zeichnen ----------------------------------
    protected override PathFigure CreatePathFigure()
    {
        Point center = new Point(X1, Y1);
        PathFigure fig = new PathFigure { StartPoint = center, IsClosed = true };

        // TODO:
        //  1) sichere Zackenzahl bestimmen (mindestens 2), z.B. Math.Max(2, Zacken).
        //  2) der Stern hat 2*zacken Punkte. Fuer i = 0 .. 2*zacken-1:
        //        double winkel = Math.PI * i / zacken;
        //        double r = (i % 2 == 0) ? AussenRadius : InnenRadius;
        //        Point p = center + new Vector(Math.Cos(winkel), Math.Sin(winkel)) * r;
        //  3) ersten Punkt als StartPoint, fuer alle weiteren ein LineSegment:
        //        fig.Segments.Add(new LineSegment(p, true));
        //  IsClosed = true schliesst den Stern automatisch.

        throw new NotImplementedException();
    }
}
