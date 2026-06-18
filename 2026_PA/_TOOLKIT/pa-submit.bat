@echo off
REM =====================================================================
REM  pa-submit.bat - sauberes ZIP (ohne bin/obj/.vs/.git), nutzt pa-submit.ps1.
REM  Verwendung:  pa-submit.bat <projekt-ordner> [zipname]
REM =====================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0pa-submit.ps1" %*
