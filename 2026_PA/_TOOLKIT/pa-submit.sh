#!/usr/bin/env bash
# =====================================================================
#  pa-submit.sh - packt die Solution in ein sauberes ZIP (ohne bin/obj/.vs/.git).
#  Verwendung:  ./pa-submit.sh <projekt-ordner> [zipname]
#  Beispiel:    ./pa-submit.sh ./PA1_4B_2025 PA1_Hey_Niklas
# =====================================================================
set -uo pipefail
SRC="${1:-.}"; NAME="${2:-}"
[ -d "$SRC" ] || { echo "Verwendung: ./pa-submit.sh <projekt-ordner> [zipname]"; exit 1; }
command -v zip >/dev/null 2>&1 || { echo "FEHLER: 'zip' nicht gefunden." >&2; exit 1; }

if [ -z "$NAME" ]; then
  sln="$(find "$SRC" -maxdepth 2 -name '*.sln' 2>/dev/null | head -1)"
  if [ -n "$sln" ]; then NAME="$(basename "$sln" .sln)"; else NAME="$(basename "$(cd "$SRC" && pwd)")"; fi
fi
OUT="$NAME.zip"
[ -e "$OUT" ] && { echo "FEHLER: '$OUT' existiert bereits." >&2; exit 1; }
OUTABS="$(pwd)/$OUT"

( cd "$SRC" && zip -r -q "$OUTABS" . \
    -x '*/bin/*' 'bin/*' '*/obj/*' 'obj/*' '*/.vs/*' '.vs/*' '*/.git/*' '.git/*' \
       '*.user' '*.suo' '.DS_Store' '*/.DS_Store' '*.zip' )

echo "Erstellt: $OUT  ($(du -h "$OUT" | cut -f1))"
echo "Enthaltene Projekte:"
unzip -l "$OUT" | grep -iE '\.(sln|csproj)$' | awk '{print "  "$4}' || echo "  (Achtung: keine .sln/.csproj im ZIP!)"
