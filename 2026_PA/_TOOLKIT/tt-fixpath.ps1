# =====================================================================
#  tt-fixpath.ps1 - absoluten LoadSQLiteMetadata-Pfad im .tt relativ machen.
#  Von tt-fixpath.bat aufgerufen.
# =====================================================================
param([string]$Tt = "", [string]$RelDir = ".")

if (-not $Tt -or -not (Test-Path $Tt)) { Write-Host "Verwendung: tt-fixpath.bat <pfad\zur.tt> [relativer-ordner]"; exit 1 }

$content = Get-Content -Raw $Tt
$pattern = 'LoadSQLiteMetadata\(@?"[^"]*"'
$m = [regex]::Match($content, $pattern)
if (-not $m.Success) { Write-Host "Kein LoadSQLiteMetadata(@`"...`") gefunden."; exit 1 }
Write-Host "vorher : $($m.Value)"

Copy-Item $Tt "$Tt.bak" -Force
$replacement = 'LoadSQLiteMetadata(@"' + $RelDir + '"'
# MatchEvaluator -> ersetzt mit dem LITERALEN String (kein $-Backreference-Problem)
$new = [regex]::Replace($content, $pattern, { param($x) $replacement })
Set-Content -Path $Tt -Value $new -NoNewline

$m2 = [regex]::Match($new, $pattern)
Write-Host "nachher: $($m2.Value)"
Write-Host "Backup : $Tt.bak"
Write-Host "Tipp: die .db muss im selben Ordner wie das .tt liegen."
