# =====================================================================
#  pa.ps1 - EIN Launcher fuer das ganze PA-Toolkit (Pfeiltasten-Navigation).
#  Von pa.bat aufgerufen (oder direkt: powershell -File pa.ps1).
# =====================================================================
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Menu-Single($title, $options) {
    [Console]::Error.WriteLine("")
    [Console]::Error.WriteLine($title + "  (Pfeil hoch/runter, Enter)")
    $top = [Console]::CursorTop
    $cur = 0
    while ($true) {
        for ($i = 0; $i -lt $options.Count; $i++) {
            $pfx = if ($i -eq $cur) { "  > " } else { "    " }
            [Console]::Error.WriteLine($pfx + $options[$i] + "        ")
        }
        $k = [Console]::ReadKey($true)
        switch ($k.Key) {
            'UpArrow'   { $cur = ($cur - 1 + $options.Count) % $options.Count }
            'DownArrow' { $cur = ($cur + 1) % $options.Count }
            'Enter'     { return $cur }
        }
        [Console]::SetCursorPosition(0, $top)
    }
}

$catalog = [ordered]@{
    "Projekt & Bausteine"   = @(
        @{l = "Neue Solution (new-pa)"; c = "new-pa.bat"; d = "MeinePA --wpf --mvvm" },
        @{l = "Preset Client/Server"; c = "new-pa.bat"; d = "Chat --clientserver" },
        @{l = "Preset Drawing (WPF+Shape)"; c = "new-pa.bat"; d = "MeineGrafik --drawing" },
        @{l = "Preset Threading (WPF)"; c = "new-pa.bat"; d = "MeinThreading --threading" },
        @{l = "Baustein einfuegen (add)"; c = "add.bat"; d = ".\MeinePA\MeinePA mvvm converter" }
    )
    "Code generieren"       = @(
        @{l = "DataTemplate interaktiv"; c = "gen.bat"; d = "datatemplate -i" },
        @{l = "Snippet (dp/prop/cmd/model/...)"; c = "gen.bat"; d = "prop Vorname string" },
        @{l = "Grid-Geruest (gen grid)"; c = "gen.bat"; d = "grid 3 2" },
        @{l = "Enum (gen enum)"; c = "gen.bat"; d = "enum .\Proj MessageType SEARCH DETAIL" },
        @{l = "Event-Handler-Stubs (gen handlers)"; c = "gen.bat"; d = "handlers .\Proj" },
        @{l = "Converter registrieren (register)"; c = "register.bat"; d = ".\Proj converter MeinConverter" },
        @{l = "Server generieren"; c = "gen.bat"; d = "server .\Proj --msg MSG --port 12345" },
        @{l = "Client generieren"; c = "gen.bat"; d = "client .\Proj --msg MSG --port 12345" },
        @{l = "Threading-Primitiv"; c = "gen.bat"; d = "sync semaphore Landebahn --count 3" }
    )
    "Checks vor der Abgabe" = @(
        @{l = "Abgabe-Check (alles)"; c = "pa-ready.bat"; d = "." },
        @{l = "Bewertungsfehler (pa-check)"; c = "pa-check.bat"; d = "." },
        @{l = "Threading (lock-check)"; c = "lock-check.bat"; d = "." },
        @{l = "Unfertige Stellen (find-todo)"; c = "find-todo.bat"; d = "." }
    )
    "Abgabe verpacken"      = @(
        @{l = "Sauberes ZIP (pa-submit)"; c = "pa-submit.bat"; d = ". PA_Name" }
    )
    "Sicherheit (Snapshots)" = @(
        @{l = "Checkpoint speichern (pa-snap save)"; c = "pa-snap.bat"; d = "save works ." },
        @{l = "Snapshots auflisten"; c = "pa-snap.bat"; d = "list" },
        @{l = "Checkpoint zurueck (pa-snap restore)"; c = "pa-snap.bat"; d = "restore works ." }
    )
    "Datenbank"             = @(
        @{l = "DB ansehen (db-inspect)"; c = "db-inspect.bat"; d = "meine.db" },
        @{l = "SQL-Abfrage (db-query)"; c = "db-query.bat"; d = "meine.db" },
        @{l = ".tt generieren (db-tt)"; c = "db-tt.bat"; d = ".\Proj .\Proj\meine.db" },
        @{l = ".tt-Pfad reparieren (tt-fixpath)"; c = "tt-fixpath.bat"; d = ".\Proj\DataModel.tt" },
        @{l = "PreserveNewest pruefen (db-copycheck)"; c = "db-copycheck.bat"; d = ".\Proj\Proj.csproj" },
        @{l = "Modell aus .db (scaffold-db)"; c = "scaffold-db.bat"; d = ".\Proj .\Proj\meine.db" },
        @{l = "DB aus .sql (db-create)"; c = "db-create.bat"; d = "schema.sql meine.db" }
    )
    "Client/Server testen"  = @(
        @{l = "Framed Test/Echo (frame-probe)"; c = "frame-probe.bat"; d = "localhost 12345 --root MSG --field type=SEARCH" },
        @{l = "Port lauscht? (port-check)"; c = "port-check.bat"; d = "12345" },
        @{l = "Roher TCP-Test (net-probe)"; c = "net-probe.bat"; d = "localhost 12345 PING" }
    )
    "Snippets & Infos"      = @(
        @{l = "Snippet kopieren (snip)"; c = "snip.bat"; d = "-i" },
        @{l = "Merkkarte (cheatsheet)"; c = "cheatsheet.bat"; d = "" }
    )
}

Clear-Host
Write-Host "==============================================="
Write-Host "   PA-TOOLKIT   -   Launcher"
Write-Host "   (Pfeiltasten + Enter)"
Write-Host "==============================================="

while ($true) {
    $cats = @($catalog.Keys) + "Beenden"
    $ci = Menu-Single "Kategorie:" $cats
    if ($cats[$ci] -eq "Beenden") { Write-Host "Tschuess."; break }
    $cat = $cats[$ci]
    $items = @($catalog[$cat])
    $labels = @($items | ForEach-Object { $_.l }) + "<- zurueck"
    $ii = Menu-Single "${cat}:" $labels
    if ($labels[$ii] -eq "<- zurueck") { continue }
    $sel = $items[$ii]

    Write-Host ""
    Write-Host "Standard:  $($sel.c) $($sel.d)"
    $a = Read-Host "Argumente (Enter = Standard, q = zurueck)"
    if ($a -eq 'q') { continue }
    if (-not $a) { $a = $sel.d }
    Write-Host "-----------------------------------------------"
    Write-Host "> $($sel.c) $a"
    Write-Host "-----------------------------------------------"
    if ([string]::IsNullOrWhiteSpace($a)) { & "$ScriptDir\$($sel.c)" }
    else { & "$ScriptDir\$($sel.c)" @($a -split ' +') }
    Write-Host ""
    Read-Host "[Enter] zurueck zum Menue" | Out-Null
}
