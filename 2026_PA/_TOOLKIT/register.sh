#!/usr/bin/env bash
# =====================================================================
#  register.sh - traegt einen Converter (oder eine Ressource) in App.xaml
#  (bzw. Window.Resources) ein: <local:Name x:Key="key"/> UND stellt sicher,
#  dass xmlns:local="clr-namespace:<Projekt>" vorhanden ist.
#  Genau der Hand-Edit, den `gen converter` offen laesst.
#
#  Verwendung:  ./register.sh <projekt-ordner> converter <Name> [--window Datei.xaml]
# =====================================================================
set -uo pipefail

PROJ="${1:-}"; KIND="${2:-}"; NAME="${3:-}"; WIN=""
[ $# -ge 3 ] && shift 3 || true
while [ $# -gt 0 ]; do case "$1" in --window) WIN="${2:-}"; shift 2 || shift ;; *) shift ;; esac; done
{ [ -d "${PROJ:-/nonexistent}" ] && [ "$KIND" = "converter" ] && [ -n "$NAME" ]; } || {
  echo "Verwendung: ./register.sh <projekt-ordner> converter <Name> [--window Datei.xaml]"; exit 1; }

csproj="$(ls "$PROJ"/*.csproj 2>/dev/null | head -1)"
[ -n "$csproj" ] || { echo "FEHLER: keine .csproj in '$PROJ'." >&2; exit 1; }
NS="$(basename "$csproj" .csproj)"
KEY="$(printf '%s' "${NAME:0:1}" | tr '[:upper:]' '[:lower:]')${NAME:1}"

if [ -n "$WIN" ]; then TARGET="$WIN"; CLOSE="</Window.Resources>"; OPENTAG="Window.Resources"
else TARGET="$(ls "$PROJ"/App.xaml 2>/dev/null | head -1)"; CLOSE="</Application.Resources>"; OPENTAG="Application.Resources"; fi
[ -f "${TARGET:-/nonexistent}" ] || { echo "FEHLER: Ziel-XAML nicht gefunden (${TARGET:-App.xaml})." >&2; exit 1; }

if grep -q "x:Key=\"$KEY\"" "$TARGET"; then echo "x:Key=\"$KEY\" existiert bereits in $(basename "$TARGET") - nichts zu tun."; exit 0; fi

cp "$TARGET" "$TARGET.bak"

# xmlns:local sicherstellen (auf gleicher Zeile an xmlns:x anhaengen -> keine Newline-Probleme)
if ! grep -q 'xmlns:local=' "$TARGET"; then
  sed -i.tmp -E "s#(xmlns:x=\"[^\"]*\")#\1 xmlns:local=\"clr-namespace:$NS\"#" "$TARGET" && rm -f "$TARGET.tmp"
  echo "  + xmlns:local=\"clr-namespace:$NS\""
fi

RES="        <local:$NAME x:Key=\"$KEY\"/>"
if grep -qF "$CLOSE" "$TARGET"; then
  awk -v res="$RES" -v endtag="$CLOSE" '
    (index($0, endtag) > 0) && !ins { print res; ins=1 }
    { print }
  ' "$TARGET" > "$TARGET.tmp" && mv "$TARGET.tmp" "$TARGET"
  echo "  + <local:$NAME x:Key=\"$KEY\"/>  in $(basename "$TARGET")"
  echo "  Verwendung:  Converter={StaticResource $KEY}"
else
  echo "FEHLER: <$OPENTAG> ... $CLOSE nicht in $(basename "$TARGET") gefunden."
  echo "Bitte manuell einfuegen: $RES"
  exit 1
fi
