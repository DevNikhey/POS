#!/usr/bin/env bash
# =====================================================================
#  net-probe.sh - TCP-Verbindung testen / Test-Nachricht senden.
#  Verwendung:  ./net-probe.sh <host> <port> [text]
#  Beispiele:
#    ./net-probe.sh localhost 12345            # nur Verbindung testen
#    ./net-probe.sh localhost 12345 "PING"     # Text senden + Antwort lesen
# =====================================================================
set -uo pipefail

HOST="${1:-}"; PORT="${2:-}"
[ $# -ge 2 ] && shift 2 || true
TEXT="${*:-}"
{ [ -n "$HOST" ] && [ -n "$PORT" ]; } || { echo "Verwendung: ./net-probe.sh <host> <port> [text]"; exit 1; }
command -v nc >/dev/null 2>&1 || { echo "FEHLER: 'nc' (netcat) nicht gefunden." >&2; exit 1; }

if [ -n "$TEXT" ]; then
  echo "-> sende an $HOST:$PORT: $TEXT"
  printf '%s\n' "$TEXT" | nc -w 3 "$HOST" "$PORT"
else
  if nc -z -w 3 "$HOST" "$PORT" 2>/dev/null; then echo "Verbindung zu $HOST:$PORT OK"; else echo "keine Verbindung zu $HOST:$PORT"; fi
fi
