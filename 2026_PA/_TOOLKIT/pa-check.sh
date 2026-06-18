#!/usr/bin/env bash
# =====================================================================
#  pa-check.sh - Heuristik-"Linter" fuer die typischen PA-Bewertungsfehler.
#  Durchsucht .cs/.xaml im angegebenen Ordner und gibt eine Checkliste
#  mit Fundstellen aus. KEIN Compiler - nur Mustererkennung.
#
#  Verwendung:  ./pa-check.sh [ordner]   (Default: aktuelles Verzeichnis)
# =====================================================================
set -uo pipefail

TARGET="${1:-.}"
[ -d "$TARGET" ] || { echo "Verwendung: ./pa-check.sh [ordner]"; exit 1; }

FINDINGS=0

# Treffer einer Regex zeilenweise melden
report() {  # <sev> <titel> <glob> <regex> [grep-extra-flags]
  local sev="$1" title="$2" glob="$3" re="$4" extra="${5:-}"
  local out
  out="$(grep -rInE $extra --include="$glob" -- "$re" "$TARGET" 2>/dev/null)"
  if [ -n "$out" ]; then
    FINDINGS=$((FINDINGS+1))
    printf '\n[%s] %s\n' "$sev" "$title"
    printf '%s\n' "$out" | sed 's/^/    /'
  fi
}

# Dateien melden, die <muss> enthalten, aber <fehlt> NICHT
report_file_missing() {  # <sev> <titel> <glob> <muss-regex> <fehlt-regex>
  local sev="$1" title="$2" glob="$3" must="$4" missing="$5"
  local hits="" f
  while IFS= read -r f; do
    [ -z "$f" ] && continue
    if ! grep -qE -- "$missing" "$f"; then hits="$hits    $f"$'\n'; fi
  done < <(grep -rlE --include="$glob" -- "$must" "$TARGET" 2>/dev/null)
  if [ -n "$hits" ]; then
    FINDINGS=$((FINDINGS+1))
    printf '\n[%s] %s\n' "$sev" "$title"
    printf '%s' "$hits"
  fi
}

echo "PA-Check: $TARGET"
echo "(Heuristik - bitte Treffer pruefen, nicht blind aendern.)"

# --- WPF Layout (PA4/A1) ---------------------------------------------
report WARN "Liste mit fixer Hoehe (Height=\"...\") - meist Grid + Height=\"*\" besser (PA4/A1)" \
  '*.xaml' '<(ListBox|ListView|DataGrid)[^>]*Height="[0-9]'
report INFO "StackPanel gefunden - pruefen, ob Grid/DockPanel passt (fuellt Platz, kein fixes Height)" \
  '*.xaml' '<StackPanel'

# --- ActualWidth/Height im Code-Behind (PA4/A4) ----------------------
report WARN "ActualWidth/ActualHeight - im Konstruktor 0! Nur im Loaded-Event nutzen (PA4/A4)" \
  '*.xaml.cs' 'Actual(Width|Height)'

# --- Items.Add statt ItemsSource (PA3/A5) ----------------------------
report WARN "Items.Add( - bei Daten-Listen besser ItemsSource = ... setzen (PA3/A5)" \
  '*.cs' '\.Items\.Add\('

# --- LengthConverter (PA2/A3,A6) -------------------------------------
report WARN "LengthConverter - NUR fuer Laengen. Bei Angle/Ecken/Umdrehung/Count entfernen (PA2/A3,A6)" \
  '*.cs' 'TypeConverter\(typeof\(LengthConverter\)\)' '-A1'

# --- IF/ELSE Parser-Muster (PA4/A5) ----------------------------------
report WARN "Keyword-Check auf Typ statt Wert == \"ELSE\"? Optionale Keywords genau pruefen (PA4/A5)" \
  '*.cs' 'Type *!= *.*TokenType\.KEYWORD'

# --- Absolute Pfade in T4/Code ---------------------------------------
report INFO "Absoluter Pfad (C:\\...) - in T4-Templates/Code vermeiden" \
  '*.cs' '[A-Za-z]:\\Users'
report INFO "Absoluter Pfad im T4-Template (.tt)" \
  '*.tt' '[A-Za-z]:\\'

# --- CreatePathFigure ohne Segmente (PA2/A3) -------------------------
report_file_missing WARN "CreatePathFigure ohne Segments.Add - es wird nichts gezeichnet (PA2/A3)" \
  '*.cs' 'override +PathFigure +CreatePathFigure' 'Segments\.Add'

# --- Transfer ohne Laengen-Praefix (PA3/A2) --------------------------
report_file_missing WARN "Transfer-Klasse ohne BitConverter.GetBytes - sendet evtl. keine Laenge (PA3/A2)" \
  '*.cs' 'class +Transfer' 'BitConverter\.GetBytes'

echo
if [ "$FINDINGS" -eq 0 ]; then
  echo "Keine typischen Muster gefunden. (Trotzdem selbst gegenpruefen!)"
else
  echo "== $FINDINGS Punkt(e) zum Pruefen. =="
fi
