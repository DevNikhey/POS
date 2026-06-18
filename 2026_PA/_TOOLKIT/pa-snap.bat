@echo off
REM =====================================================================
REM  pa-snap.bat - leichte Checkpoints (ohne bin/obj/.vs/.git) via robocopy.
REM    pa-snap.bat save <label> [ordner]
REM    pa-snap.bat list
REM    pa-snap.bat restore <label> [ziel]
REM =====================================================================
setlocal
set "SNAP=%~dp0.snapshots"
set "CMD=%~1"

if /i "%CMD%"=="save"    goto save
if /i "%CMD%"=="list"    goto list
if /i "%CMD%"=="restore" goto restore
echo Verwendung: pa-snap.bat save ^<label^> [ordner] ^| list ^| restore ^<label^> [ziel]
exit /b 1

:save
set "LABEL=%~2"
set "SRC=%~3"
if "%LABEL%"=="" ( echo Verwendung: pa-snap.bat save ^<label^> [ordner] & exit /b 1 )
if "%SRC%"=="" set "SRC=."
if not exist "%SRC%" ( echo FEHLER: '%SRC%' nicht gefunden. & exit /b 1 )
if not exist "%SNAP%" mkdir "%SNAP%"
robocopy "%SRC%" "%SNAP%\%LABEL%" /MIR /XD bin obj .vs .git .snapshots /NFL /NDL /NJH /NJS >nul
echo Snapshot '%LABEL%' gespeichert.
exit /b 0

:list
echo Snapshots:
dir /b "%SNAP%" 2>nul
exit /b 0

:restore
set "LABEL=%~2"
set "DST=%~3"
if "%LABEL%"=="" ( echo Verwendung: pa-snap.bat restore ^<label^> [ziel] & exit /b 1 )
if "%DST%"=="" set "DST=."
if not exist "%SNAP%\%LABEL%" ( echo Snapshot '%LABEL%' existiert nicht. & exit /b 1 )
robocopy "%DST%" "%SNAP%\_vor_restore_%LABEL%" /MIR /XD bin obj .vs .git .snapshots /NFL /NDL /NJH /NJS >nul
robocopy "%SNAP%\%LABEL%" "%DST%" /E /NFL /NDL /NJH /NJS >nul
echo Snapshot '%LABEL%' wiederhergestellt nach '%DST%'. (vorher gesichert: _vor_restore_%LABEL%)
exit /b 0
