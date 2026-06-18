# =====================================================================
#  db-copycheck.ps1 - prueft/fixt CopyToOutputDirectory=PreserveNewest. Von db-copycheck.bat aufgerufen.
# =====================================================================
param([Parameter(ValueFromRemainingArguments = $true)][string[]]$P = @())

$csproj = if ($P.Count -ge 1) { $P[0] } else { "" }
$fix = ($P -contains '--fix')
if (-not $csproj -or -not (Test-Path $csproj)) { Write-Host "Verwendung: db-copycheck.bat <projekt.csproj> [--fix]"; exit 1 }
$dir = Split-Path $csproj -Parent; if (-not $dir) { $dir = "." }
Write-Host "db-copycheck: $csproj"
$content = Get-Content -Raw $csproj

$codedb = (Get-ChildItem -Path $dir -Recurse -Filter *.cs -ErrorAction SilentlyContinue |
    Select-String -Pattern 'Data Source=([^;"<]+)' |
    ForEach-Object { $_.Matches[0].Groups[1].Value }) | Sort-Object -Unique
if ($codedb) { Write-Host ("  Code oeffnet (Data Source): " + ($codedb -join ' ')) }

$dbs = Get-ChildItem -Path $dir -File -ErrorAction SilentlyContinue | Where-Object { $_.Extension -in '.db', '.sqlite', '.sqlite3', '.sql' }
if (-not $dbs) { Write-Host "  (keine .db/.sql neben der .csproj gefunden)"; exit 0 }

$missing = @()
foreach ($f in $dbs) {
    $n = $f.Name
    if (($content -match [regex]::Escape("Update=`"$n`"")) -and ($content -match 'PreserveNewest')) {
        Write-Host "  OK    $n   (PreserveNewest gesetzt)"
    } else {
        Write-Host "  FEHLT $n   (kein <None Update PreserveNewest>)"; $missing += $n
    }
}
if ($missing.Count -eq 0) { Write-Host "Alles ok."; exit 0 }

if ($fix) {
    $block = "  <ItemGroup>`n"
    foreach ($n in $missing) { $block += "    <None Update=`"$n`"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>`n" }
    $block += "  </ItemGroup>`n"
    Copy-Item $csproj "$csproj.bak" -Force
    $idx = $content.LastIndexOf('</Project>')
    $new = $content.Substring(0, $idx) + $block + $content.Substring($idx)
    Set-Content -Path $csproj -Value $new -NoNewline
    Write-Host "  --fix: ItemGroup eingefuegt (Backup: $csproj.bak)"
} else {
    Write-Host "  -> mit --fix automatisch reparieren."
}
