# =====================================================================
#  frame-probe.ps1 - TCP-Test im Transfer<T>-Format [4-Byte-Laenge LE][UTF-8-XML].
#  Von frame-probe.bat aufgerufen.
# =====================================================================
param([Parameter(ValueFromRemainingArguments = $true)][string[]]$P = @())

function Send-Frame($stream, $bytes) {
    $len = [BitConverter]::GetBytes([int]$bytes.Length)   # host-endian (auf Windows x86 = little-endian)
    $stream.Write($len, 0, 4); $stream.Write($bytes, 0, $bytes.Length); $stream.Flush()
}
function Read-Exactly($stream, $n) {
    $buf = New-Object byte[] $n; $off = 0
    while ($off -lt $n) { $r = $stream.Read($buf, $off, $n - $off); if ($r -le 0) { return $null }; $off += $r }
    return $buf
}
function Read-Frame($stream) {
    $h = Read-Exactly $stream 4; if ($null -eq $h) { return $null }
    $len = [BitConverter]::ToInt32($h, 0)
    return Read-Exactly $stream $len
}

if ($P.Count -ge 1 -and $P[0] -eq '--listen') {
    $port = [int]$P[1]
    $listener = New-Object System.Net.Sockets.TcpListener([System.Net.IPAddress]::Any, $port)
    $listener.Start()
    Write-Host "frame-probe Echo-Server laeuft auf Port $port (Strg+C beendet)"
    while ($true) {
        $c = $listener.AcceptTcpClient(); Write-Host "-- Client verbunden"
        $st = $c.GetStream()
        try {
            while ($true) {
                $d = Read-Frame $st; if ($null -eq $d) { break }
                Write-Host ("<- empfangen: " + [Text.Encoding]::UTF8.GetString($d))
                Send-Frame $st $d
            }
        } catch { Write-Host "Fehler: $_" }
        $c.Close(); Write-Host "-- Client getrennt"
    }
} else {
    if ($P.Count -lt 2) { Write-Host "Verwendung: frame-probe.bat <host> <port> [--root MSG] [--field k=v]... | --listen <port>"; exit 1 }
    $h = $P[0]; $port = [int]$P[1]
    $root = 'MSG'; $fields = @(); $raw = $null; $i = 2
    while ($i -lt $P.Count) {
        switch ($P[$i]) {
            '--root'  { $root = $P[$i + 1]; $i += 2 }
            '--field' { $kv = $P[$i + 1]; $eq = $kv.IndexOf('='); $fields += , @($kv.Substring(0, $eq), $kv.Substring($eq + 1)); $i += 2 }
            '--xml'   { $raw = $P[$i + 1]; $i += 2 }
            default   { $i++ }
        }
    }
    if ($null -eq $raw) {
        $body = ''
        foreach ($f in $fields) { $body += "<$($f[0])>$($f[1])</$($f[0])>" }
        $raw = "<$root>$body</$root>"
    }
    $payload = [Text.Encoding]::UTF8.GetBytes($raw)
    $client = New-Object System.Net.Sockets.TcpClient; $client.Connect($h, $port)
    $st = $client.GetStream()
    Write-Host "-> sende ($($payload.Length) Bytes XML): $raw"
    Send-Frame $st $payload
    $client.ReceiveTimeout = 3000
    try {
        $r = Read-Frame $st
        if ($null -eq $r) { Write-Host "<- keine gerahmte Antwort" }
        else { Write-Host ("<- Antwort: " + [Text.Encoding]::UTF8.GetString($r)) }
    } catch { Write-Host "<- Timeout/Fehler: $_" }
    $client.Close()
}
