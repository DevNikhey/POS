#!/usr/bin/env bash
# =====================================================================
#  scaffold-db.sh - erzeugt linq2db-Modellklassen aus einer SQLite-.db
#  (Alternative zum T4-Template ".tt" in Visual Studio.)
#
#  Verwendung:
#    ./scaffold-db.sh <ziel-ordner> <pfad/zur.db> [Namespace]
#  Beispiel:
#    ./scaffold-db.sh ./Fotos/Fotos ./Fotos/Fotos/photoworld.db DataModels
#
#  Ergebnis: generierte DataConnection + Entity-Klassen im Ziel-Ordner.
# =====================================================================
set -euo pipefail

TARGET_DIR="${1:-}"
DB_FILE="${2:-}"
NS="${3:-DataModels}"

if [ -z "$TARGET_DIR" ] || [ -z "$DB_FILE" ]; then
  echo "Verwendung: ./scaffold-db.sh <ziel-ordner> <pfad/zur.db> [Namespace]" >&2
  exit 1
fi
if [ ! -f "$DB_FILE" ]; then echo "FEHLER: DB-Datei '$DB_FILE' nicht gefunden." >&2; exit 1; fi
if ! command -v dotnet >/dev/null 2>&1; then echo "FEHLER: 'dotnet' nicht gefunden." >&2; exit 1; fi

# linq2db.cli als globales Tool sicherstellen
if ! dotnet tool list -g | grep -qi "linq2db.cli"; then
  echo "==> installiere linq2db.cli (global)"
  dotnet tool install -g linq2db.cli
  echo "    Falls 'dotnet linq2db' nicht gefunden wird: neues Terminal oeffnen"
  echo "    oder \$HOME/.dotnet/tools zum PATH hinzufuegen."
fi

mkdir -p "$TARGET_DIR"
echo "==> scaffolde Modell aus '$DB_FILE' nach '$TARGET_DIR' (namespace $NS)"
dotnet linq2db scaffold -p SQLite -c "Data Source=$DB_FILE" -o "$TARGET_DIR" --namespace "$NS"
echo "FERTIG."
