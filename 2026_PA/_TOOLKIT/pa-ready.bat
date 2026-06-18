@echo off
REM =====================================================================
REM  pa-ready.bat - Abgabe-Gate: Build + alle Checks in einem Befehl.
REM  Verwendung:  pa-ready.bat [ordner]
REM =====================================================================
setlocal EnableDelayedExpansion
set "T=%~1"
if "%T%"=="" set "T=."
set "DIR=%~dp0"

echo ==================================================
echo   PA-READY - Abgabe-Check: %T%
echo ==================================================

echo.
echo [1] Build
where dotnet >nul 2>nul
if errorlevel 1 (
  echo     ^(dotnet nicht gefunden^)
) else (
  set "PROJ="
  for /f "delims=" %%s in ('dir /b /s "%T%\*.sln" 2^>nul') do if not defined PROJ set "PROJ=%%s"
  if not defined PROJ for /f "delims=" %%c in ('dir /b /s "%T%\*.csproj" 2^>nul') do if not defined PROJ set "PROJ=%%c"
  if defined PROJ (
    dotnet build "!PROJ!" --nologo -v q
    if errorlevel 1 ( echo     BUILD FEHLGESCHLAGEN ^(siehe oben^) ) else ( echo     BUILD OK )
  ) else echo     ^(keine .sln/.csproj gefunden^)
)

echo.
echo [2] pa-check
call "%DIR%pa-check.bat" "%T%"
echo.
echo [3] lock-check
call "%DIR%lock-check.bat" "%T%"
echo.
echo [4] find-todo
call "%DIR%find-todo.bat" "%T%"

echo.
echo ==================================================
echo   Pruefe oben: Build OK? Keine NotImplementedException? Keine roten Checks?
echo ==================================================
