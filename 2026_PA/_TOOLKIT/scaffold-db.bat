@echo off
REM =====================================================================
REM  scaffold-db.bat - erzeugt linq2db-Modellklassen aus einer SQLite-.db
REM  (Alternative zum T4-Template ".tt" in Visual Studio.)
REM
REM  Verwendung:
REM    scaffold-db.bat <ziel-ordner> <pfad\zur.db> [Namespace]
REM  Beispiel:
REM    scaffold-db.bat .\Fotos\Fotos .\Fotos\Fotos\photoworld.db DataModels
REM =====================================================================
setlocal EnableDelayedExpansion

set "TARGET_DIR=%~1"
set "DB_FILE=%~2"
set "NS=%~3"
if "%NS%"=="" set "NS=DataModels"

if "%TARGET_DIR%"=="" goto usage
if "%DB_FILE%"=="" goto usage
if not exist "%DB_FILE%" ( echo FEHLER: DB-Datei '%DB_FILE%' nicht gefunden. & exit /b 1 )
where dotnet >nul 2>nul || ( echo FEHLER: 'dotnet' nicht gefunden. & exit /b 1 )

dotnet tool list -g | findstr /i "linq2db.cli" >nul
if errorlevel 1 (
  echo ==^> installiere linq2db.cli ^(global^)
  dotnet tool install -g linq2db.cli
  echo     Falls 'dotnet linq2db' nicht gefunden wird: neues Terminal oeffnen.
)

if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"
echo ==^> scaffolde Modell aus '%DB_FILE%' nach '%TARGET_DIR%' ^(namespace %NS%^)
dotnet linq2db scaffold -p SQLite -c "Data Source=%DB_FILE%" -o "%TARGET_DIR%" --namespace "%NS%"
echo FERTIG.
exit /b 0

:usage
echo Verwendung: scaffold-db.bat ^<ziel-ordner^> ^<pfad\zur.db^> [Namespace]
exit /b 1
