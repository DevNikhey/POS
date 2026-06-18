@echo off
REM =====================================================================
REM  db-query.bat - fuehrt eine Ad-hoc-SQL-Abfrage auf einer SQLite-DB aus.
REM  Verwendung:  db-query.bat <pfad\zur.db> "<SQL>"
REM  Beispiel:    db-query.bat photoworld.db "SELECT * FROM photos LIMIT 3;"
REM =====================================================================
setlocal

set "DB=%~1"
set "SQL=%~2"
if "%DB%"=="" goto usage
if "%SQL%"=="" goto usage
if not exist "%DB%" ( echo FEHLER: '%DB%' nicht gefunden. & exit /b 1 )
where sqlite3 >nul 2>nul || ( echo FEHLER: 'sqlite3' nicht im PATH. Siehe _TOOLKIT\README.md. & exit /b 1 )

sqlite3 -header -column "%DB%" "%SQL%"
exit /b 0

:usage
echo Verwendung: db-query.bat ^<pfad\zur.db^> "^<SQL^>"
exit /b 1
