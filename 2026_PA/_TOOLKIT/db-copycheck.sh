#!/usr/bin/env bash
# =====================================================================
#  db-copycheck.sh - prueft, ob .db/.sql-Dateien in der .csproj mit
#  CopyToOutputDirectory=PreserveNewest landen. Sonst oeffnet die App die
#  DB in bin/ (leer/alt) -> jede Query gibt still nichts zurueck.
#
#  Verwendung:  ./db-copycheck.sh <projekt.csproj> [--fix]
# =====================================================================
set -uo pipefail

CSPROJ="${1:-}"; FIX=0
for a in "$@"; do [ "$a" = "--fix" ] && FIX=1; done
[ -f "$CSPROJ" ] || { echo "Verwendung: ./db-copycheck.sh <projekt.csproj> [--fix]"; exit 1; }
DIR="$(dirname "$CSPROJ")"
echo "db-copycheck: $CSPROJ"

# Welche DB oeffnet der Code? (Data Source=...)
codedb="$(grep -rhoE 'Data Source=[^;\"<]+' "$DIR" --include='*.cs' 2>/dev/null | sed -E 's/Data Source=//' | sort -u | tr '\n' ' ')"
[ -n "$codedb" ] && echo "  Code oeffnet (Data Source): $codedb"

files="$(cd "$DIR" && ls 2>/dev/null | grep -iE '\.(db|sqlite|sqlite3|sql)$' || true)"
[ -z "$files" ] && { echo "  (keine .db/.sql neben der .csproj gefunden)"; exit 0; }

missing=""
while IFS= read -r f; do
  [ -z "$f" ] && continue
  if grep -qE "Update=\"$f\"" "$CSPROJ" && grep -q "PreserveNewest" "$CSPROJ"; then
    echo "  OK    $f   (PreserveNewest gesetzt)"
  else
    echo "  FEHLT $f   (kein <None Update PreserveNewest>)"
    missing="$missing $f"
  fi
done <<< "$files"

if [ -z "$missing" ]; then echo "Alles ok."; exit 0; fi

if [ "$FIX" -eq 1 ]; then
  tmpblock="$(mktemp)"
  {
    echo "  <ItemGroup>"
    for f in $missing; do
      echo "    <None Update=\"$f\"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>"
    done
    echo "  </ItemGroup>"
  } > "$tmpblock"
  cp "$CSPROJ" "$CSPROJ.bak"
  awk -v bf="$tmpblock" '
    /<\/Project>/ && !ins { while ((getline line < bf) > 0) print line; ins=1 }
    { print }
  ' "$CSPROJ.bak" > "$CSPROJ"
  rm -f "$tmpblock"
  echo "  --fix: ItemGroup eingefuegt (Backup: $CSPROJ.bak)"
else
  echo "  -> mit --fix automatisch reparieren:  ./db-copycheck.sh \"$CSPROJ\" --fix"
fi
