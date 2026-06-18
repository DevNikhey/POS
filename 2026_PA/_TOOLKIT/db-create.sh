#!/usr/bin/env bash
# =====================================================================
#  db-create.sh - erstellt eine neue SQLite-DB aus einem .sql-Schema.
#  Verwendung:  ./db-create.sh <schema.sql> [out.db]
#  Beispiel:    ./db-create.sh schema.sql meinedb.db
# =====================================================================
set -uo pipefail

SCHEMA="${1:-}"; OUT="${2:-}"
{ [ -n "$SCHEMA" ] && [ -f "$SCHEMA" ]; } || { echo "Verwendung: ./db-create.sh <schema.sql> [out.db]"; exit 1; }
[ -n "$OUT" ] || OUT="$(basename "$SCHEMA" .sql).db"
command -v sqlite3 >/dev/null 2>&1 || { echo "FEHLER: 'sqlite3' nicht gefunden." >&2; exit 1; }
[ -e "$OUT" ] && { echo "FEHLER: '$OUT' existiert bereits." >&2; exit 1; }

sqlite3 "$OUT" < "$SCHEMA" && echo "DB erstellt: $OUT"
