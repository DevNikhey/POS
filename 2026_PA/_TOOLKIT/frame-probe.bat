@echo off
REM =====================================================================
REM  frame-probe.bat - TCP-Test im Transfer<T>-Format [4-Byte-Laenge LE][UTF-8-XML].
REM  Nutzt frame-probe.ps1 (System.Net.Sockets), kein Python noetig.
REM
REM  Client:       frame-probe.bat <host> <port> [--root MSG] [--field k=v]...
REM  Echo-Server:  frame-probe.bat --listen <port>
REM =====================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0frame-probe.ps1" %*
