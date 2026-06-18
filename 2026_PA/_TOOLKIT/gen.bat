@echo off
REM =====================================================================
REM  gen.bat - Code-Generatoren (nutzt gen.ps1 / PowerShell).
REM
REM    gen.bat dp <Klasse> <Name> <Typ> [--render]
REM    gen.bat window <projekt-ordner> <Name>
REM    gen.bat vm <projekt-ordner> <Name>
REM    gen.bat converter <projekt-ordner> <Name>
REM =====================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0gen.ps1" %*
