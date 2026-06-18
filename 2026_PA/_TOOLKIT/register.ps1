# =====================================================================
#  register.ps1 - Converter/Ressource in App.xaml (oder Window.Resources) eintragen.
#  Von register.bat aufgerufen.
# =====================================================================
param([Parameter(ValueFromRemainingArguments = $true)][string[]]$P = @())

$proj = if ($P.Count -ge 1) { $P[0] } else { "" }
$kind = if ($P.Count -ge 2) { $P[1] } else { "" }
$name = if ($P.Count -ge 3) { $P[2] } else { "" }
$win = ""
for ($i = 3; $i -lt $P.Count; $i++) { if ($P[$i] -eq '--window') { $win = $P[$i + 1]; $i++ } }

if (-not $proj -or -not (Test-Path $proj) -or $kind -ne 'converter' -or -not $name) {
    Write-Host "Verwendung: register.bat <projekt-ordner> converter <Name> [--window Datei.xaml]"; exit 1
}
$csproj = Get-ChildItem -Path $proj -Filter *.csproj -File -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $csproj) { Write-Host "FEHLER: keine .csproj in '$proj'."; exit 1 }
$ns = [IO.Path]::GetFileNameWithoutExtension($csproj.Name)
$key = $name.Substring(0, 1).ToLower() + $name.Substring(1)

if ($win) { $target = $win; $close = '</Window.Resources>' }
else { $target = (Join-Path $proj 'App.xaml'); $close = '</Application.Resources>' }
if (-not (Test-Path $target)) { Write-Host "FEHLER: Ziel-XAML nicht gefunden ($target)."; exit 1 }

$content = Get-Content -Raw $target
if ($content -match [regex]::Escape("x:Key=`"$key`"")) { Write-Host "x:Key=`"$key`" existiert bereits - nichts zu tun."; exit 0 }
Copy-Item $target "$target.bak" -Force

if ($content -notmatch 'xmlns:local=') {
    $content = $content -replace '(xmlns:x="[^"]*")', ('$1 xmlns:local="clr-namespace:' + $ns + '"')
    Write-Host "  + xmlns:local=`"clr-namespace:$ns`""
}

$res = "        <local:$name x:Key=`"$key`"/>"
if ($content.Contains($close)) {
    # literale Ersetzung (nur ein schliessendes Tag vorhanden) -> Ressource davor einfuegen
    $content = $content.Replace($close, $res + "`r`n    " + $close)
    Set-Content -Path $target -Value $content -NoNewline
    Write-Host "  + <local:$name x:Key=`"$key`"/>  in $(Split-Path $target -Leaf)"
    Write-Host "  Verwendung:  Converter={StaticResource $key}"
} else {
    Write-Host "FEHLER: $close nicht gefunden. Manuell einfuegen: $res"; exit 1
}
