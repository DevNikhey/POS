@echo off
REM =====================================================================
REM  pa-check.bat - Heuristik-Linter fuer die typischen PA-Bewertungsfehler.
REM  Nutzt PowerShell (pa-check.ps1), weil findstr fuer diese Regex zu schwach ist.
REM
REM  Verwendung:  pa-check.bat [ordner]   (Default: aktuelles Verzeichnis)
REM =====================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0pa-check.ps1" %*
