#!/usr/bin/env bash
# =====================================================================
#  db-inspect.sh - zeigt Tabellen, Schema und Beispieldaten einer SQLite-DB.
#  Schnell verstehen, wie die vorgegebene .db aufgebaut ist (Spaltennamen!).
#
#  Verwendung:  ./db-inspect.sh <pfad/zur.db>
# =====================================================================
set -uo pipefail

DB="${1:-}"
{ [ -n "$DB" ] && [ -f "$DB" ]; } || { echo "Verwendung: ./db-inspect.sh <pfad/zur.db>"; exit 1; }
command -v sqlite3 >/dev/null 2>&1 || { echo "FEHLER: 'sqlite3' nicht gefunden (macOS hat es meist; sonst 'brew install sqlite')." >&2; exit 1; }

echo "Datenbank: $DB"
tables="$(sqlite3 "$DB" "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;")"
[ -z "$tables" ] && { echo "(keine Tabellen gefunden)"; exit 0; }

while IFS= read -r t; do
  [ -z "$t" ] && continue
  echo
  echo "== $t =="
  echo "-- Schema --"
  sqlite3 "$DB" ".schema $t"
  cnt="$(sqlite3 "$DB" "SELECT COUNT(*) FROM [$t];")"
  echo "-- Beispielzeilen (max 5 von $cnt) --"
  sqlite3 -header -column "$DB" "SELECT * FROM [$t] LIMIT 5;"
done <<< "$tables"
