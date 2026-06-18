@echo off
REM =====================================================================
REM  tt-fixpath.bat - absoluten LoadSQLiteMetadata-Pfad im .tt relativ machen.
REM  Verwendung:  tt-fixpath.bat <pfad\zur.tt> [relativer-ordner]
REM =====================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0tt-fixpath.ps1" %*
