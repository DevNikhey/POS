#!/usr/bin/env bash
# =====================================================================
#  db-query.sh - fuehrt eine Ad-hoc-SQL-Abfrage auf einer SQLite-DB aus.
#  Verwendung:  ./db-query.sh <pfad/zur.db> "<SQL>"
#  Beispiel:    ./db-query.sh photoworld.db "SELECT * FROM photos LIMIT 3;"
# =====================================================================
set -uo pipefail

DB="${1:-}"
shift || true
SQL="$*"
{ [ -n "$DB" ] && [ -f "$DB" ] && [ -n "$SQL" ]; } || { echo "Verwendung: ./db-query.sh <pfad/zur.db> \"<SQL>\""; exit 1; }
command -v sqlite3 >/dev/null 2>&1 || { echo "FEHLER: 'sqlite3' nicht gefunden." >&2; exit 1; }

sqlite3 -header -column "$DB" "$SQL"
