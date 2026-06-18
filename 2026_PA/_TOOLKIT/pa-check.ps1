# =====================================================================
#  pa-check.ps1 - Heuristik-"Linter" fuer die typischen PA-Bewertungsfehler.
#  Wird von pa-check.bat aufgerufen (oder direkt: powershell -File pa-check.ps1 <ordner>).
# =====================================================================
param([string]$Target = ".")

if (-not (Test-Path $Target)) { Write-Host "Verwendung: pa-check.bat [ordner]"; exit 1 }
$script:groups = 0

function Report($sev, $title, $include, $regex, $context = 0) {
    $files = Get-ChildItem -Path $Target -Recurse -File -Include $include -ErrorAction SilentlyContinue
    if (-not $files) { return }
    $matches = $files | Select-String -Pattern $regex -CaseSensitive -Context 0, $context
    if ($matches) {
        $script:groups++
        Write-Host ""
        Write-Host "[$sev] $title"
        foreach ($m in $matches) {
            Write-Host ("    {0}:{1}: {2}" -f $m.Filename, $m.LineNumber, $m.Line.Trim())
            foreach ($c in $m.Context.PostContext) { Write-Host ("        {0}" -f $c.Trim()) }
        }
    }
}

function ReportFileMissing($sev, $title, $include, $must, $missing) {
    $files = Get-ChildItem -Path $Target -Recurse -File -Include $include -ErrorAction SilentlyContinue
    $hits = @()
    foreach ($f in $files) {
        $c = Get-Content -Raw $f.FullName
        if (($c -match $must) -and ($c -notmatch $missing)) { $hits += $f.FullName }
    }
    if ($hits.Count -gt 0) {
        $script:groups++
        Write-Host ""
        Write-Host "[$sev] $title"
        foreach ($h in $hits) { Write-Host "    $h" }
    }
}

Write-Host "PA-Check: $Target"
Write-Host "(Heuristik - bitte Treffer pruefen, nicht blind aendern.)"

# WPF Layout (PA4/A1)
Report "WARN" 'Liste mit fixer Hoehe (Height="...") - meist Grid + Height="*" besser (PA4/A1)' @('*.xaml') '<(ListBox|ListView|DataGrid)[^>]*Height="[0-9]'
Report "INFO" 'StackPanel gefunden - pruefen, ob Grid/DockPanel passt (fuellt Platz, kein fixes Height)' @('*.xaml') '<StackPanel'

# ActualWidth/Height im Code-Behind (PA4/A4)
Report "WARN" 'ActualWidth/ActualHeight - im Konstruktor 0! Nur im Loaded-Event nutzen (PA4/A4)' @('*.xaml.cs') 'Actual(Width|Height)'

# Items.Add statt ItemsSource (PA3/A5)
Report "WARN" 'Items.Add( - bei Daten-Listen besser ItemsSource = ... setzen (PA3/A5)' @('*.cs') '\.Items\.Add\('

# LengthConverter (PA2/A3,A6) - mit Folgezeile (die Property)
Report "WARN" 'LengthConverter - NUR fuer Laengen. Bei Angle/Ecken/Umdrehung/Count entfernen (PA2/A3,A6)' @('*.cs') 'TypeConverter\(typeof\(LengthConverter\)\)' 1

# IF/ELSE Parser-Muster (PA4/A5)
Report "WARN" 'Keyword-Check auf Typ statt Wert == "ELSE"? Optionale Keywords genau pruefen (PA4/A5)' @('*.cs') 'Type\s*!=\s*.*TokenType\.KEYWORD'

# Absolute Pfade
Report "INFO" 'Absoluter Pfad (C:\...) - in Code vermeiden' @('*.cs') '[A-Za-z]:\\Users'
Report "INFO" 'Absoluter Pfad im T4-Template (.tt)' @('*.tt') '[A-Za-z]:\\'

# CreatePathFigure ohne Segmente (PA2/A3)
ReportFileMissing "WARN" 'CreatePathFigure ohne Segments.Add - es wird nichts gezeichnet (PA2/A3)' @('*.cs') 'override\s+PathFigure\s+CreatePathFigure' 'Segments\.Add'

# Transfer ohne Laengen-Praefix (PA3/A2)
ReportFileMissing "WARN" 'Transfer-Klasse ohne BitConverter.GetBytes - sendet evtl. keine Laenge (PA3/A2)' @('*.cs') 'class\s+Transfer' 'BitConverter\.GetBytes'

Write-Host ""
if ($script:groups -eq 0) {
    Write-Host "Keine typischen Muster gefunden. (Trotzdem selbst gegenpruefen!)"
} else {
    Write-Host "== $($script:groups) Punkt(e) zum Pruefen. =="
}
