@echo off
REM =====================================================================
REM  db-create.bat - erstellt eine neue SQLite-DB aus einem .sql-Schema.
REM  Verwendung:  db-create.bat <schema.sql> [out.db]
REM =====================================================================
setlocal
set "SCHEMA=%~1"
set "OUT=%~2"
if "%SCHEMA%"=="" ( echo Verwendung: db-create.bat ^<schema.sql^> [out.db] & exit /b 1 )
if not exist "%SCHEMA%" ( echo FEHLER: '%SCHEMA%' nicht gefunden. & exit /b 1 )
if "%OUT%"=="" set "OUT=%~n1.db"
where sqlite3 >nul 2>nul || ( echo FEHLER: 'sqlite3' nicht im PATH. Siehe _TOOLKIT\README.md. & exit /b 1 )
if exist "%OUT%" ( echo FEHLER: '%OUT%' existiert bereits. & exit /b 1 )

sqlite3 "%OUT%" ".read %SCHEMA%"
echo DB erstellt: %OUT%
