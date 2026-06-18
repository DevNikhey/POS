#!/usr/bin/env bash
# =====================================================================
#  port-check.sh - lauscht jemand lokal auf dem Port? (Client/Server-Debug)
#  Verwendung:  ./port-check.sh <port>
# =====================================================================
set -uo pipefail

PORT="${1:-}"
[ -n "$PORT" ] || { echo "Verwendung: ./port-check.sh <port>"; exit 1; }

if command -v lsof >/dev/null 2>&1; then
  out="$(lsof -nP -iTCP:"$PORT" -sTCP:LISTEN 2>/dev/null)"
  if [ -n "$out" ]; then echo "Port $PORT: LAUSCHT"; echo "$out"; else echo "Port $PORT: frei (niemand lauscht)"; fi
elif command -v nc >/dev/null 2>&1; then
  if nc -z localhost "$PORT" 2>/dev/null; then echo "Port $PORT: offen"; else echo "Port $PORT: zu/frei"; fi
else
  echo "FEHLER: weder 'lsof' noch 'nc' gefunden." >&2; exit 1
fi
