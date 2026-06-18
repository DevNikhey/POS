@echo off
REM =====================================================================
REM  new-pa.bat - Scaffolder fuer eine neue Praktische Arbeit (.NET/C#)
REM  Windows-Variante von new-pa.sh. Erzeugt eine Solution mit den
REM  gewuenschten Modulen und kopiert die wiederverwendbaren Vorlagen.
REM
REM  Beispiele:
REM    new-pa.bat MeinePA --wpf --mvvm --shapes
REM    new-pa.bat Chat --wpf --console --network
REM    new-pa.bat Robot --wpf --parser
REM    new-pa.bat Fotos --wpf --db
REM =====================================================================
setlocal EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
set "TEMPLATES=%SCRIPT_DIR%templates"

set "NAME="
set "OUT=."
set "WPF=0"
set "CONSOLE=0"
set "NETWORK=0"
set "MVVM=0"
set "SHAPES=0"
set "PARSER=0"
set "DB=0"
set "PRESET="

:parse
if "%~1"=="" goto endparse
if /i "%~1"=="--wpf"     ( set "WPF=1"     & shift & goto parse )
if /i "%~1"=="--console" ( set "CONSOLE=1" & shift & goto parse )
if /i "%~1"=="--network" ( set "NETWORK=1" & shift & goto parse )
if /i "%~1"=="--mvvm"    ( set "MVVM=1"    & shift & goto parse )
if /i "%~1"=="--shapes"  ( set "SHAPES=1"  & shift & goto parse )
if /i "%~1"=="--parser"  ( set "PARSER=1"  & shift & goto parse )
if /i "%~1"=="--db"      ( set "DB=1"      & shift & goto parse )
if /i "%~1"=="--clientserver" ( set "PRESET=clientserver" & set "NETWORK=1" & set "CONSOLE=1" & shift & goto parse )
if /i "%~1"=="--drawing"      ( set "PRESET=drawing" & set "WPF=1" & set "SHAPES=1" & shift & goto parse )
if /i "%~1"=="--threading"    ( set "PRESET=threading" & set "WPF=1" & shift & goto parse )
if /i "%~1"=="--out"     ( set "OUT=%~2"   & shift & shift & goto parse )
if /i "%~1"=="-h"        goto usage
if /i "%~1"=="--help"    goto usage
set "ARG=%~1"
if "!ARG:~0,1!"=="-" ( echo Unbekannte Option: %~1 & goto usage )
if not defined NAME ( set "NAME=%~1" & shift & goto parse )
echo Zu viele Argumente: %~1 & exit /b 1
:endparse

if not defined NAME ( echo FEHLER: Name fehlt. & goto usage )
echo %NAME%| findstr /r /c:"^[A-Za-z_][A-Za-z0-9_]*$" >nul || ( echo FEHLER: ungueltiger Projektname '%NAME%'. & exit /b 1 )
where dotnet >nul 2>nul || ( echo FEHLER: 'dotnet' nicht gefunden. Bitte .NET SDK installieren. & exit /b 1 )

REM Default-Modul + Abhaengigkeiten
set /a SUM=%WPF%+%CONSOLE%+%NETWORK%
if %SUM%==0 set "WPF=1"
if "%MVVM%"=="1"   if "%WPF%"=="0" ( echo --mvvm braucht --wpf -^> aktiviere --wpf & set "WPF=1" )
if "%SHAPES%"=="1" if "%WPF%"=="0" ( echo --shapes braucht --wpf -^> aktiviere --wpf & set "WPF=1" )

set "SOL_DIR=%OUT%\%NAME%"
if exist "%SOL_DIR%" ( echo FEHLER: '%SOL_DIR%' existiert bereits. & exit /b 1 )

echo ==^> Erzeuge Solution '%NAME%' in '%SOL_DIR%'
mkdir "%SOL_DIR%"
cd /d "%SOL_DIR%"
dotnet new sln -n "%NAME%" >nul

set "APP_DIR="
set "APP_CSPROJ="
set "SERVER_CSPROJ="

if "%WPF%"=="1" (
  echo ==^> WPF-App-Projekt '%NAME%'
  dotnet new wpf -n "%NAME%" -o "%NAME%" >nul
  dotnet sln add "%NAME%\%NAME%.csproj" >nul
  set "APP_DIR=%NAME%"
  set "APP_CSPROJ=%NAME%\%NAME%.csproj"
)

if "%CONSOLE%"=="1" (
  echo ==^> Konsolen-Projekt 'Server'
  dotnet new console -n "Server" -o "Server" >nul
  dotnet sln add "Server\Server.csproj" >nul
  set "SERVER_CSPROJ=Server\Server.csproj"
)

if "%NETWORK%"=="1" (
  echo ==^> Klassenbibliothek 'Network' ^(Transfer^<T^>^)
  dotnet new classlib -n "Network" -o "Network" >nul
  del /q "Network\Class1.cs" 2>nul
  call :copy_tpl "%TEMPLATES%\network\Transfer.cs" "Network\Transfer.cs" "Network"
  dotnet sln add "Network\Network.csproj" >nul
  if defined APP_CSPROJ    dotnet add "!APP_CSPROJ!" reference "Network\Network.csproj" >nul
  if defined SERVER_CSPROJ dotnet add "!SERVER_CSPROJ!" reference "Network\Network.csproj" >nul
)

if "%MVVM%"=="1" (
  echo ==^> MVVM-Vorlagen -^> !APP_DIR!
  call :copy_tpl "%TEMPLATES%\mvvm\ViewModelBase.cs"            "!APP_DIR!\ViewModelBase.cs"            "%NAME%"
  call :copy_tpl "%TEMPLATES%\mvvm\RelayCommand.cs"             "!APP_DIR!\RelayCommand.cs"             "%NAME%"
  call :copy_tpl "%TEMPLATES%\converters\BoolToVisibilityConverter.cs" "!APP_DIR!\BoolToVisibilityConverter.cs" "%NAME%"
)

if "%SHAPES%"=="1" (
  echo ==^> Shape-Vorlage -^> !APP_DIR!
  call :copy_tpl "%TEMPLATES%\shapes\ShapeBase.cs" "!APP_DIR!\ShapeBase.cs" "%NAME%"
)

if "%PARSER%"=="1" (
  if defined SERVER_CSPROJ ( set "TGT_DIR=Server" & set "TGT_NS=Server" ) else ( set "TGT_DIR=!APP_DIR!" & set "TGT_NS=%NAME%" )
  echo ==^> Parser-Skelett -^> !TGT_DIR!
  call :copy_tpl "%TEMPLATES%\parser\Token.cs"      "!TGT_DIR!\Token.cs"      "!TGT_NS!"
  call :copy_tpl "%TEMPLATES%\parser\Expression.cs" "!TGT_DIR!\Expression.cs" "!TGT_NS!"
)

if "%DB%"=="1" (
  if defined SERVER_CSPROJ ( set "TGT_PROJ=Server\Server.csproj" & set "TGT_DIR=Server" & set "TGT_NS=Server" ) else ( set "TGT_PROJ=!APP_CSPROJ!" & set "TGT_DIR=!APP_DIR!" & set "TGT_NS=%NAME%" )
  echo ==^> linq2db/SQLite-Pakete -^> !TGT_PROJ!
  dotnet add "!TGT_PROJ!" package linq2db >nul
  dotnet add "!TGT_PROJ!" package linq2db.SQLite >nul
  dotnet add "!TGT_PROJ!" package Microsoft.Data.Sqlite >nul
  call :copy_tpl "%TEMPLATES%\db\Db.cs" "!TGT_DIR!\Db.cs" "!TGT_NS!"
  echo   -^> Modell erzeugen mit: "%SCRIPT_DIR%scaffold-db.bat" "%SOL_DIR%\!TGT_DIR!" ^<pfad\zur.db^>
)

if /i "%PRESET%"=="clientserver" (
  echo ==^> Preset client/server: MSG + Broadcast-Server + Client
  call :copy_tpl "%TEMPLATES%\preset\MSG.cs" "Network\MSG.cs" "Network"
  del /q "Server\Program.cs" 2>nul
  call "%SCRIPT_DIR%gen.bat" server "Server" --msg MSG --port 12345 >nul
  echo   + Server\ChatServer.cs
  dotnet new console -n "Client" -o "Client" >nul
  dotnet sln add "Client\Client.csproj" >nul
  dotnet add "Client\Client.csproj" reference "Network\Network.csproj" >nul
  call "%SCRIPT_DIR%gen.bat" client "Client" --msg MSG --port 12345 >nul
  echo   + Client\ChatClient.cs
  copy /y "%TEMPLATES%\preset\ClientProgram.cs" "Client\Program.cs" >nul
  echo   + Client\Program.cs
)
if /i "%PRESET%"=="drawing" (
  echo ==^> Preset drawing: Beispiel-Shape 'Stern'
  call "%SCRIPT_DIR%gen.bat" shape "!APP_DIR!" Stern >nul
  echo   + !APP_DIR!\Stern.cs
)
if /i "%PRESET%"=="threading" (
  echo ==^> Preset threading: ThreadingDemo
  call :copy_tpl "%TEMPLATES%\preset\ThreadingDemo.cs" "!APP_DIR!\ThreadingDemo.cs" "%NAME%"
)

echo.
echo FERTIG. Naechste Schritte:
echo   cd "%SOL_DIR%"
echo   dotnet build
echo   dotnet sln list
exit /b 0

:copy_tpl
REM %~1 Quelle  %~2 Ziel  %~3 Namespace
powershell -NoProfile -Command "(Get-Content -Raw '%~1') -replace '__NS__','%~3' | Set-Content -NoNewline '%~2'"
echo   + %~nx2  ^(namespace %~3^)
goto :eof

:usage
echo Verwendung: new-pa.bat ^<Name^> [Optionen]
echo.
echo Module:
echo   --wpf            WPF-App-Projekt (Name)
echo   --console        Konsolen-Projekt "Server"
echo   --network        Klassenbibliothek "Network" mit Transfer^<T^>
echo   --mvvm           ViewModelBase + RelayCommand + Converter (braucht --wpf)
echo   --shapes         ShapeBase fuer eigene Zeichen-Controls (braucht --wpf)
echo   --parser         Token.cs + Expression.cs (Recursive-Descent-Skelett)
echo   --db             linq2db + SQLite NuGet-Pakete + Db.cs
echo.
echo Presets (fertig verdrahtete Skelette):
echo   --clientserver   Network(Transfer)+MSG + Broadcast-Server + Client
echo   --drawing        WPF + ShapeBase + Beispiel-Shape 'Stern'
echo   --threading      WPF + ThreadingDemo
echo.
echo Sonstiges:
echo   --out DIR        Zielordner (Default: aktuelles Verzeichnis)
echo   -h ^| --help      Diese Hilfe
echo.
echo Ohne Modul-Flag wird --wpf angenommen.
exit /b 1
