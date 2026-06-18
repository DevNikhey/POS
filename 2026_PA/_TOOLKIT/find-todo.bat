@echo off
REM =====================================================================
REM  find-todo.bat - findet unfertige/uebrig gebliebene Stellen vor der Abgabe.
REM  Verwendung:  find-todo.bat [ordner]
REM =====================================================================
setlocal
set "T=%~1"
if "%T%"=="" set "T=."
echo find-todo: %T%
echo.
echo [NotImplementedException - UNFERTIGE Stubs!]
findstr /s /n /i /c:"NotImplementedException" "%T%\*.cs" 2>nul
echo.
echo [TODO / FIXME / HACK]
findstr /s /n /i /c:"TODO" /c:"FIXME" /c:"HACK" "%T%\*.cs" 2>nul
echo.
echo [Debug-Ausgaben]
findstr /s /n /i /c:"Console.WriteLine" /c:"Debug.WriteLine" /c:"Trace.WriteLine" "%T%\*.cs" 2>nul
echo.
echo [Absolute Pfade C:\...]
findstr /s /n /i /c:":\Users" "%T%\*.cs" "%T%\*.tt" 2>nul
echo.
echo Fertig.
