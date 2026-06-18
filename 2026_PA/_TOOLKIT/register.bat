@echo off
REM =====================================================================
REM  register.bat - Converter/Ressource in App.xaml eintragen (nutzt register.ps1).
REM  Verwendung:  register.bat <projekt-ordner> converter <Name> [--window Datei.xaml]
REM =====================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0register.ps1" %*
