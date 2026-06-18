#!/usr/bin/env bash
# =====================================================================
#  find-todo.sh - findet unfertige/uebrig gebliebene Stellen vor der Abgabe.
#  Verwendung:  ./find-todo.sh [ordner]
# =====================================================================
set -uo pipefail
TARGET="${1:-.}"
[ -d "$TARGET" ] || { echo "Verwendung: ./find-todo.sh [ordner]"; exit 1; }
N=0
g() {  # <titel> <regex> [glob]
  local title="$1" re="$2" glob="${3:-*.cs}" out
  out="$(grep -rInE --include="$glob" -- "$re" "$TARGET" 2>/dev/null)"
  if [ -n "$out" ]; then N=$((N+1)); printf '\n[%s]\n' "$title"; printf '%s\n' "$out" | sed 's/^/    /'; fi
}
echo "find-todo: $TARGET"
g "NotImplementedException - UNFERTIGE Stubs! (zero auf alles dahinter)" 'NotImplementedException'
g "TODO / FIXME / HACK / XXX" 'TODO|FIXME|HACK|XXX'
g "Debug-Ausgaben (Console/Debug.WriteLine) - vor Abgabe entfernen?" 'Console\.WriteLine|Debug\.WriteLine|Trace\.WriteLine'
g "Absoluter Pfad (C:\\...) im Code" '[A-Za-z]:\\Users'
g "Absoluter Pfad (C:\\...) im T4-Template" '[A-Za-z]:\\' '*.tt'
echo
if [ "$N" -eq 0 ]; then echo "Nichts gefunden - sauber."; else echo "== $N Kategorie(n) zum Pruefen. =="; fi
