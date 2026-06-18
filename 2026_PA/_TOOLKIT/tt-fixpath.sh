#!/usr/bin/env bash
# =====================================================================
#  tt-fixpath.sh - ersetzt den absoluten LoadSQLiteMetadata-Pfad in einem
#  linq2db-.tt durch einen RELATIVEN ("."), behaelt den DB-Dateinamen.
#  Behebt den Fehler, wenn ein vorgegebenes/kopiertes .tt einen fremden
#  C:\Users\...-Pfad enthaelt (sonst regeneriert das T4 nicht).
#
#  Verwendung:  ./tt-fixpath.sh <pfad/zur/DataModel.tt> [relativer-ordner]
# =====================================================================
set -uo pipefail

TT="${1:-}"; RELDIR="${2:-.}"
[ -f "$TT" ] || { echo "Verwendung: ./tt-fixpath.sh <pfad/zur.tt> [relativer-ordner]"; exit 1; }

LINE="$(grep -nE 'LoadSQLiteMetadata\(@?"' "$TT" | head -1)"
if [ -z "$LINE" ]; then echo "Kein LoadSQLiteMetadata(@\"...\") in '$TT' gefunden."; exit 1; fi
echo "vorher : $LINE"

# nur den ERSTEN String-Parameter (den Ordner) ersetzen; DB-Dateiname bleibt.
sed -i.bak -E "s|LoadSQLiteMetadata\(@?\"[^\"]*\"|LoadSQLiteMetadata(@\"$RELDIR\"|" "$TT"

echo "nachher: $(grep -nE 'LoadSQLiteMetadata\(@?"' "$TT" | head -1)"
echo "Backup : $TT.bak"
echo "Tipp: die .db muss im selben Ordner wie das .tt liegen (oder den relativen Ordner als 2. Argument angeben)."
