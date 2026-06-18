@echo off
REM =====================================================================
REM  db-tt.bat - erzeugt ein linq2db-T4-Template (.tt) fuer eine SQLite-DB
REM  (nutzt db-tt.ps1). Relativer Pfad statt absolutem C:\...-Pfad.
REM
REM  Verwendung:  db-tt.bat <ziel-ordner> <pfad\zur.db> [Namespace] [tt-name]
REM =====================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0db-tt.ps1" %*
