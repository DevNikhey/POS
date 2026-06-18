@echo off
REM =====================================================================
REM  net-probe.bat - TCP-Verbindung testen / Test-Nachricht senden (nutzt net-probe.ps1).
REM  Verwendung:  net-probe.bat <host> <port> [text]
REM =====================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0net-probe.ps1" %*
