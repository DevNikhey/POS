@echo off
REM =====================================================================
REM  snip.bat - Schnipsel-Bibliothek (nutzt snip.ps1, kopiert in die Zwischenablage).
REM    snip.bat            Liste
REM    snip.bat <name>     Snippet ausgeben + kopieren
REM    snip.bat -i         interaktiv per Pfeiltasten
REM =====================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0snip.ps1" %*
