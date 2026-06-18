@echo off
REM =====================================================================
REM  db-inspect.bat - zeigt Tabellen, Schema und Beispieldaten einer SQLite-DB.
REM  Verwendung:  db-inspect.bat <pfad\zur.db>
REM  (benoetigt sqlite3.exe im PATH - siehe README)
REM =====================================================================
setlocal EnableDelayedExpansion

set "DB=%~1"
if "%DB%"=="" goto usage
if not exist "%DB%" ( echo FEHLER: '%DB%' nicht gefunden. & exit /b 1 )
where sqlite3 >nul 2>nul || ( echo FEHLER: 'sqlite3' nicht im PATH. Siehe _TOOLKIT\README.md. & exit /b 1 )

echo Datenbank: %DB%
for /f "delims=" %%t in ('sqlite3 "%DB%" "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%%' ORDER BY name;"') do (
  echo.
  echo == %%t ==
  echo -- Schema --
  sqlite3 "%DB%" ".schema %%t"
  echo -- Beispielzeilen ^(max 5^) --
  sqlite3 -header -column "%DB%" "SELECT * FROM [%%t] LIMIT 5;"
)
exit /b 0

:usage
echo Verwendung: db-inspect.bat ^<pfad\zur.db^>
exit /b 1
