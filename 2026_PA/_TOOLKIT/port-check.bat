@echo off
REM =====================================================================
REM  port-check.bat - lauscht jemand lokal auf dem Port?
REM  Verwendung:  port-check.bat <port>
REM =====================================================================
setlocal
set "PORT=%~1"
if "%PORT%"=="" ( echo Verwendung: port-check.bat ^<port^> & exit /b 1 )

netstat -ano | findstr "LISTENING" | findstr ":%PORT% "
if errorlevel 1 ( echo -^> Port %PORT%: frei ) else ( echo -^> Port %PORT%: LAUSCHT )
