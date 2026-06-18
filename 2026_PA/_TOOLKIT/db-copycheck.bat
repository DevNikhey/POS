@echo off
REM =====================================================================
REM  db-copycheck.bat - prueft/fixt CopyToOutputDirectory=PreserveNewest (nutzt db-copycheck.ps1).
REM  Verwendung:  db-copycheck.bat <projekt.csproj> [--fix]
REM =====================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0db-copycheck.ps1" %*
