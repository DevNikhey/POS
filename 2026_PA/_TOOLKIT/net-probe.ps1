# =====================================================================
#  net-probe.ps1 - TCP-Verbindung testen / Test-Nachricht senden (wird von net-probe.bat aufgerufen).
# =====================================================================
param(
    [Parameter(Position = 0)][string]$Server = "",
    [Parameter(Position = 1)][int]$Port = 0,
    [Parameter(Position = 2)][string]$Text = ""
)

if (-not $Server -or $Port -eq 0) { Write-Host "Verwendung: net-probe.bat <host> <port> [text]"; exit 1 }

try {
    $client = New-Object System.Net.Sockets.TcpClient
    $iar = $client.BeginConnect($Server, $Port, $null, $null)
    if (-not $iar.AsyncWaitHandle.WaitOne(3000)) {
        Write-Host "keine Verbindung zu ${Server}:${Port} (Timeout)"; exit 1
    }
    $client.EndConnect($iar)
    Write-Host "Verbindung zu ${Server}:${Port} OK"

    if ($Text) {
        $stream = $client.GetStream()
        $data = [Text.Encoding]::UTF8.GetBytes($Text + "`n")
        $stream.Write($data, 0, $data.Length); $stream.Flush()
        Write-Host "-> gesendet: $Text"
        Start-Sleep -Milliseconds 400
        if ($stream.DataAvailable) {
            $buf = New-Object byte[] 4096
            $n = $stream.Read($buf, 0, $buf.Length)
            Write-Host ("<- " + [Text.Encoding]::UTF8.GetString($buf, 0, $n))
        }
    }
    $client.Close()
} catch {
    Write-Host "Fehler: $_"; exit 1
}
