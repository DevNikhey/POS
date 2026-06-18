@echo off
REM =====================================================================
REM  add.bat - fuegt einen wiederverwendbaren Baustein in ein BESTEHENDES
REM  Projekt ein (Namespace wird automatisch auf das Projekt gesetzt).
REM
REM  Verwendung:
REM    add.bat <projekt-ordner> <baustein...>
REM  Bausteine: transfer | mvvm | converter | shapes | parser | db | all
REM =====================================================================
setlocal EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
set "TEMPLATES=%SCRIPT_DIR%templates"

set "PROJ_DIR=%~1"
if "%PROJ_DIR%"=="" goto usage
if not exist "%PROJ_DIR%" ( echo FEHLER: Ordner '%PROJ_DIR%' nicht gefunden. & exit /b 1 )

set "CSPROJ="
set "NS="
for %%f in ("%PROJ_DIR%\*.csproj") do ( set "CSPROJ=%%f" & set "NS=%%~nf" )
if not defined CSPROJ ( echo FEHLER: keine .csproj in '%PROJ_DIR%'. & exit /b 1 )
echo ==^> Zielprojekt: !NS!.csproj  ^(namespace !NS!^)

shift
set "ANY=0"
:loop
if "%~1"=="" goto done
set "ANY=1"
if /i "%~1"=="all" (
  call :add transfer & call :add mvvm & call :add converter & call :add shapes & call :add parser & call :add db
) else (
  call :add "%~1"
)
shift
goto loop
:done
if "%ANY%"=="0" goto usage
echo FERTIG.
exit /b 0

:add
set "M=%~1"
if /i "!M!"=="transfer"  ( call :copy "network\Transfer.cs" & goto :eof )
if /i "!M!"=="mvvm"       ( call :copy "mvvm\ViewModelBase.cs" & call :copy "mvvm\RelayCommand.cs" & goto :eof )
if /i "!M!"=="converter"  ( call :copy "converters\BoolToVisibilityConverter.cs" & goto :eof )
if /i "!M!"=="shapes"     ( call :copy "shapes\ShapeBase.cs" & goto :eof )
if /i "!M!"=="parser"     ( call :copy "parser\Token.cs" & call :copy "parser\Expression.cs" & goto :eof )
if /i "!M!"=="db" (
  call :copy "db\Db.cs"
  where dotnet >nul 2>nul && (
    dotnet add "!CSPROJ!" package linq2db >nul
    dotnet add "!CSPROJ!" package linq2db.SQLite >nul
    dotnet add "!CSPROJ!" package Microsoft.Data.Sqlite >nul
    echo   + NuGet: linq2db, linq2db.SQLite, Microsoft.Data.Sqlite
  ) || echo   ! 'dotnet' fehlt - NuGet-Pakete bitte manuell hinzufuegen
  goto :eof
)
echo   ? unbekannter Baustein: !M!
goto :eof

:copy
REM %~1 = relativer Template-Pfad
set "SRC=%TEMPLATES%\%~1"
for %%n in ("%~1") do set "FN=%%~nxn"
set "DEST=%PROJ_DIR%\!FN!"
if exist "!DEST!" ( echo   ! !FN! existiert bereits - uebersprungen & goto :eof )
powershell -NoProfile -Command "(Get-Content -Raw '!SRC!') -replace '__NS__','!NS!' | Set-Content -NoNewline '!DEST!'"
echo   + !FN!
goto :eof

:usage
echo Verwendung: add.bat ^<projekt-ordner^> ^<baustein...^>
echo Bausteine: transfer ^| mvvm ^| converter ^| shapes ^| parser ^| db ^| all
exit /b 1
