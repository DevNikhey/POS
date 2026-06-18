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
    // TODO: DependencyProperty 'Radius' vom Typ double registrieren
    //       (Default 0.0, AffectsRender | AffectsMeasure).
    // public static readonly DependencyProperty RadiusProperty = ...

    [TypeConverter(typeof(LengthConverter))]   // Radius ist eine Laenge -> ok
    public double Radius
    {
        // TODO: get/set ueber GetValue/SetValue(RadiusProperty)
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    // ---- AUFGABE 1: DependencyProperty 'Ecken' (int, ANZAHL!) -----------
    // TODO: DependencyProperty 'Ecken' vom Typ int registrieren
    //       (Default 0, AffectsRender | AffectsMeasure).
    // HINWEIS: KEIN LengthConverter auf Ecken (das ist eine Anzahl!).
    // public static readonly DependencyProperty EckenProperty = ...

    public int Ecken
    {
        // TODO: get/set ueber GetValue/SetValue(EckenProperty)
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    // ---- AUFGABE 2: das Vieleck zeichnen --------------------------------
    protected override PathFigure CreatePathFigure()
    {
        Point center = new Point(X1, Y1);
        PathFigure fig = new PathFigure { StartPoint = center, IsClosed = true };

        // TODO:
        //  1) sichere Eckenzahl bestimmen (mindestens 3), z.B. Math.Max(3, Ecken).
        //  2) fuer i = 0 .. ecken-1 den Eckpunkt berechnen:
        //        double winkel = 2 * Math.PI * i / ecken;
        //        Point p = center + new Vector(Math.Cos(winkel), Math.Sin(winkel)) * Radius;
        //  3) den ERSTEN Punkt als StartPoint setzen,
        //     fuer alle weiteren ein LineSegment hinzufuegen:
        //        fig.Segments.Add(new LineSegment(p, true));
        //  IsClosed = true zieht die letzte Kante zurueck zum Start automatisch.

        throw new NotImplementedException();
    }
}
