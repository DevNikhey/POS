#!/usr/bin/env bash
# =====================================================================
#  add.sh - fuegt einen wiederverwendbaren Baustein in ein BESTEHENDES
#  Projekt ein (Namespace wird automatisch auf das Projekt gesetzt).
#  Ideal, wenn die PA mit einem vorgegebenen Skelett startet.
#
#  Verwendung:
#    ./add.sh <projekt-ordner> <baustein...>
#  Bausteine: transfer | mvvm | converter | shapes | parser | db | all
#  Beispiel:
#    ./add.sh ./MeinePA/MeinePA mvvm converter
#    ./add.sh ./Server transfer
# =====================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEMPLATES="$SCRIPT_DIR/templates"

PROJ_DIR="${1:-}"
if [ -z "$PROJ_DIR" ] || [ $# -lt 2 ]; then
  echo "Verwendung: ./add.sh <projekt-ordner> <baustein...>"
  echo "Bausteine: transfer | mvvm | converter | shapes | parser | db | all"
  exit 1
fi
shift
[ -d "$PROJ_DIR" ] || { echo "FEHLER: Ordner '$PROJ_DIR' nicht gefunden." >&2; exit 1; }

# .csproj im Zielordner finden -> Namespace = Projektname
csproj="$(ls "$PROJ_DIR"/*.csproj 2>/dev/null | head -1 || true)"
if [ -z "$csproj" ]; then echo "FEHLER: keine .csproj in '$PROJ_DIR'." >&2; exit 1; fi
NS="$(basename "$csproj" .csproj)"
echo "==> Zielprojekt: $(basename "$csproj")  (namespace $NS)"

copy() {  # <relativer template-pfad>
  local dest="$PROJ_DIR/$(basename "$1")"
  if [ -e "$dest" ]; then echo "  ! $(basename "$1") existiert bereits - uebersprungen"; return; fi
  sed "s/__NS__/$NS/g" "$TEMPLATES/$1" > "$dest"
  echo "  + $(basename "$1")"
}

# "all" expandieren
mods=("$@")
for a in "$@"; do
  if [ "$a" = "all" ]; then mods=(transfer mvvm converter shapes parser db); break; fi
done

for m in "${mods[@]}"; do
  case "$m" in
    transfer)  copy network/Transfer.cs ;;
    mvvm)      copy mvvm/ViewModelBase.cs; copy mvvm/RelayCommand.cs ;;
    converter) copy converters/BoolToVisibilityConverter.cs ;;
    shapes)    copy shapes/ShapeBase.cs ;;
    parser)    copy parser/Token.cs; copy parser/Expression.cs ;;
    db)
      copy db/Db.cs
      if command -v dotnet >/dev/null 2>&1; then
        dotnet add "$csproj" package linq2db >/dev/null
        dotnet add "$csproj" package linq2db.SQLite >/dev/null
        dotnet add "$csproj" package Microsoft.Data.Sqlite >/dev/null
        echo "  + NuGet: linq2db, linq2db.SQLite, Microsoft.Data.Sqlite"
      else
        echo "  ! 'dotnet' fehlt - NuGet-Pakete bitte manuell hinzufuegen"
      fi
      ;;
    *) echo "  ? unbekannter Baustein: $m (transfer|mvvm|converter|shapes|parser|db|all)" ;;
  esac
done
echo "FERTIG."
