@echo off
REM =====================================================================
REM  lock-check.bat - Threading/Synchronisations-Linter (PA1), nutzt lock-check.ps1.
REM  Verwendung:  lock-check.bat [ordner]
REM =====================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0lock-check.ps1" %*
