#!/usr/bin/env bash
# =====================================================================
#  pa-ready.sh - Abgabe-Gate: orchestriert Build + alle Checks in einem Befehl.
#  Verwendung:  ./pa-ready.sh [ordner]
# =====================================================================
set -uo pipefail
TARGET="${1:-.}"
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
[ -d "$TARGET" ] || { echo "Verwendung: ./pa-ready.sh [ordner]"; exit 1; }
RED=0

echo "=================================================="
echo "  PA-READY  -  Abgabe-Check: $TARGET"
echo "=================================================="

echo; echo "[1] Build"
if command -v dotnet >/dev/null 2>&1; then
  proj="$(find "$TARGET" -maxdepth 3 -name '*.sln' 2>/dev/null | head -1)"
  [ -n "$proj" ] || proj="$(find "$TARGET" -maxdepth 3 -name '*.csproj' 2>/dev/null | head -1)"
  if [ -n "$proj" ]; then
    if out="$(dotnet build "$proj" --nologo -v q 2>&1)"; then
      echo "    BUILD OK"
    else
      echo "    BUILD FEHLGESCHLAGEN:"; printf '%s\n' "$out" | grep -iE 'error' | head -8 | sed 's/^/      /'; RED=1
    fi
  else echo "    (keine .sln/.csproj gefunden)"; fi
else
  echo "    (dotnet nicht installiert - auf dem Schul-/Windows-PC verfuegbar, hier uebersprungen)"
fi

echo; echo "[2] pa-check (Bewertungsfehler)";  "$DIR/pa-check.sh" "$TARGET" | sed 's/^/    /'
echo; echo "[3] lock-check (Threading)";       "$DIR/lock-check.sh" "$TARGET" | sed 's/^/    /'

echo; echo "[4] find-todo (unfertige Stellen)"
ft="$("$DIR/find-todo.sh" "$TARGET")"; printf '%s\n' "$ft" | sed 's/^/    /'
printf '%s' "$ft" | grep -q 'NotImplementedException' && RED=1

sln="$(find "$TARGET" -maxdepth 3 -name '*.sln' 2>/dev/null | head -1)"
if [ -n "$sln" ]; then
  echo; echo "[5] .sln referenziert alle Projekte?"
  while IFS= read -r cp; do
    [ -z "$cp" ] && continue
    base="$(basename "$cp")"
    if grep -qF "$base" "$sln"; then echo "    OK    $base"; else echo "    FEHLT im .sln: $base"; RED=1; fi
  done < <(find "$TARGET" -maxdepth 4 -name '*.csproj' -not -path '*/bin/*' -not -path '*/obj/*' 2>/dev/null)
fi

echo; echo "=================================================="
if [ "$RED" -eq 1 ]; then echo "  >> ABGABE NICHT BEREIT - rote Punkte oben fixen. <<"
else echo "  >> BEREIT (trotzdem kurz selbst gegenpruefen). <<"; fi
echo "=================================================="
[ "$RED" -eq 0 ]
