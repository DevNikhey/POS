#!/usr/bin/env bash
# =====================================================================
#  lock-check.sh - Heuristik-Linter fuer Threading/Synchronisation (PA1).
#  Ergaenzt pa-check um die typischen Nebenlaeufigkeits-Fehler.
#
#  Verwendung:  ./lock-check.sh [ordner]
# =====================================================================
set -uo pipefail

TARGET="${1:-.}"
[ -d "$TARGET" ] || { echo "Verwendung: ./lock-check.sh [ordner]"; exit 1; }
FINDINGS=0

report() {  # <sev> <titel> <glob> <regex>
  local sev="$1" title="$2" glob="$3" re="$4" out
  out="$(grep -rInE --include="$glob" -- "$re" "$TARGET" 2>/dev/null)"
  if [ -n "$out" ]; then
    FINDINGS=$((FINDINGS+1))
    printf '\n[%s] %s\n' "$sev" "$title"
    printf '%s\n' "$out" | sed 's/^/    /'
  fi
}

report_file_missing() {  # <sev> <titel> <glob> <muss> <fehlt>
  local sev="$1" title="$2" glob="$3" must="$4" missing="$5" hits="" f
  while IFS= read -r f; do
    [ -z "$f" ] && continue
    grep -qE -- "$missing" "$f" || hits="$hits    $f"$'\n'
  done < <(grep -rlE --include="$glob" -- "$must" "$TARGET" 2>/dev/null)
  if [ -n "$hits" ]; then
    FINDINGS=$((FINDINGS+1))
    printf '\n[%s] %s\n' "$sev" "$title"
    printf '%s' "$hits"
  fi
}

echo "lock-check (Threading/PA1): $TARGET"
echo "(Heuristik - Treffer pruefen, nicht blind aendern.)"

report WARN "lock(this) - stattdessen auf ein privates readonly object sperren (PA1)" \
  '*.cs' 'lock[[:space:]]*\([[:space:]]*this\b'
report WARN "lock(typeof(...)) - stattdessen privates readonly object" \
  '*.cs' 'lock[[:space:]]*\([[:space:]]*typeof'
report WARN "lock auf String-Literal - falsch, privates readonly object verwenden" \
  '*.cs' 'lock[[:space:]]*\([[:space:]]*"'
report INFO "new Thread(() => ...) - Lambda meist unnoetig: new Thread(obj.Methode) (PA1)" \
  '*.cs' 'new[[:space:]]+Thread[[:space:]]*\([[:space:]]*\([[:space:]]*\)[[:space:]]*=>'
report WARN "if(... .WaitOne()/.Wait() ...) - meist while statt if noetig (verlorene Signale)" \
  '*.cs' 'if[[:space:]]*\([^)]*\.(WaitOne|Wait)[[:space:]]*\('
report INFO "Monitor.Wait - muss in einer while(bedingung)-Schleife stehen, nicht in if()" \
  '*.cs' 'Monitor\.Wait'
report INFO "Release()/Set() gefunden - steht es im finally? (sonst Lock-Leak bei Exception)" \
  '*.cs' '\.(Release|Set)\([[:space:]]*\)'
report_file_missing INFO "WPF-Datei mit Threading aber ohne Dispatcher - GUI-Zugriff aus Thread? (Dispatcher.Invoke)" \
  '*.xaml.cs' 'ThreadPool|new[[:space:]]+Thread|Task\.Run' 'Dispatcher'

echo
if [ "$FINDINGS" -eq 0 ]; then echo "Keine typischen Threading-Muster gefunden."
else echo "== $FINDINGS Punkt(e) zum Pruefen. =="; fi
