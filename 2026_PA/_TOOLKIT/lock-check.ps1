# =====================================================================
#  lock-check.ps1 - Threading/Synchronisations-Linter (PA1). Von lock-check.bat aufgerufen.
# =====================================================================
param([string]$Target = ".")

if (-not (Test-Path $Target)) { Write-Host "Verwendung: lock-check.bat [ordner]"; exit 1 }
$script:findings = 0

function Report($sev, $title, $include, $regex) {
    $files = Get-ChildItem -Path $Target -Recurse -File -Include $include -ErrorAction SilentlyContinue
    if (-not $files) { return }
    $m = $files | Select-String -Pattern $regex
    if ($m) {
        $script:findings++
        Write-Host ""; Write-Host "[$sev] $title"
        foreach ($x in $m) { Write-Host ("    {0}:{1}: {2}" -f $x.Filename, $x.LineNumber, $x.Line.Trim()) }
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
        $script:findings++
        Write-Host ""; Write-Host "[$sev] $title"
        foreach ($h in $hits) { Write-Host "    $h" }
    }
}

Write-Host "lock-check (Threading/PA1): $Target"
Write-Host "(Heuristik - Treffer pruefen, nicht blind aendern.)"

Report "WARN" 'lock(this) - stattdessen auf ein privates readonly object sperren (PA1)' @('*.cs') 'lock\s*\(\s*this\b'
Report "WARN" 'lock(typeof(...)) - stattdessen privates readonly object' @('*.cs') 'lock\s*\(\s*typeof'
Report "WARN" 'lock auf String-Literal - falsch, privates readonly object verwenden' @('*.cs') 'lock\s*\(\s*"'
Report "INFO" 'new Thread(() => ...) - Lambda meist unnoetig: new Thread(obj.Methode) (PA1)' @('*.cs') 'new\s+Thread\s*\(\s*\(\s*\)\s*=>'
Report "WARN" 'if(... .WaitOne()/.Wait() ...) - meist while statt if noetig (verlorene Signale)' @('*.cs') 'if\s*\([^)]*\.(WaitOne|Wait)\s*\('
Report "INFO" 'Monitor.Wait - muss in einer while(bedingung)-Schleife stehen, nicht in if()' @('*.cs') 'Monitor\.Wait'
Report "INFO" 'Release()/Set() gefunden - steht es im finally? (sonst Lock-Leak bei Exception)' @('*.cs') '\.(Release|Set)\(\s*\)'
ReportFileMissing "INFO" 'WPF-Datei mit Threading aber ohne Dispatcher - GUI-Zugriff aus Thread? (Dispatcher.Invoke)' @('*.xaml.cs') 'ThreadPool|new\s+Thread|Task\.Run' 'Dispatcher'

Write-Host ""
if ($script:findings -eq 0) { Write-Host "Keine typischen Threading-Muster gefunden." }
else { Write-Host "== $($script:findings) Punkt(e) zum Pruefen. ==" }
