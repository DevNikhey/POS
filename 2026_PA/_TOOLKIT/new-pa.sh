#!/usr/bin/env bash
# =====================================================================
#  new-pa.sh – Scaffolder fuer eine neue Praktische Arbeit (.NET/C#)
#  Erzeugt eine Solution mit den gewuenschten Modulen und kopiert die
#  wiederverwendbaren Vorlagen (Transfer<T>, MVVM, Shapes, Parser, DB).
#
#  Beispiele:
#    ./new-pa.sh MeinePA --wpf --mvvm --shapes
#    ./new-pa.sh Chat --wpf --console --network
#    ./new-pa.sh Robot --wpf --parser
#    ./new-pa.sh Fotos --wpf --db
#
#  Hinweis: WPF-Projekte werden auf jeder Plattform ANGELEGT, lassen sich
#  aber nur unter WINDOWS bauen/starten. Auf macOS/Linux dient das Skript
#  zum Erzeugen der Struktur und der Nicht-WPF-Module.
# =====================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEMPLATES="$SCRIPT_DIR/templates"

usage() {
  cat <<'EOF'
Verwendung: ./new-pa.sh <Name> [Optionen]

Module:
  --wpf            WPF-App-Projekt (Name)
  --console        Konsolen-Projekt "Server"
  --network        Klassenbibliothek "Network" mit Transfer<T> (Length-Prefix + XML)
  --mvvm           ViewModelBase + RelayCommand + BoolToVisibilityConverter (braucht --wpf)
  --shapes         ShapeBase fuer eigene Zeichen-Controls (braucht --wpf)
  --parser         Token.cs + Expression.cs (Recursive-Descent-Skelett)
  --db             linq2db + SQLite NuGet-Pakete + Db.cs

Presets (fertig verdrahtete, lauffaehige Skelette):
  --clientserver   Network(Transfer)+MSG + Broadcast-Server + Client (Konsole), alles verbunden
  --drawing        WPF + ShapeBase + Beispiel-Shape 'Stern'
  --threading      WPF + ThreadingDemo (lock/SemaphoreSlim/Monitor/Producer-Consumer)

Sonstiges:
  --out DIR        Zielordner (Default: aktuelles Verzeichnis)
  -h | --help      Diese Hilfe

Ohne Modul-Flag wird --wpf angenommen.
EOF
}

# ---- Argumente einlesen ------------------------------------------------
NAME=""
OUT="."
WPF=0; CONSOLE=0; NETWORK=0; MVVM=0; SHAPES=0; PARSER=0; DB=0
PRESET=""

while [ $# -gt 0 ]; do
  case "$1" in
    --wpf) WPF=1 ;;
    --console) CONSOLE=1 ;;
    --network) NETWORK=1 ;;
    --mvvm) MVVM=1 ;;
    --shapes) SHAPES=1 ;;
    --parser) PARSER=1 ;;
    --db) DB=1 ;;
    --clientserver) PRESET="clientserver"; NETWORK=1; CONSOLE=1 ;;
    --drawing) PRESET="drawing"; WPF=1; SHAPES=1 ;;
    --threading) PRESET="threading"; WPF=1 ;;
    --out) shift; OUT="${1:-.}" ;;
    -h|--help) usage; exit 0 ;;
    -*) echo "Unbekannte Option: $1" >&2; usage; exit 1 ;;
    *) if [ -z "$NAME" ]; then NAME="$1"; else echo "Zu viele Argumente: $1" >&2; exit 1; fi ;;
  esac
  shift
done

if [ -z "$NAME" ]; then echo "FEHLER: Name fehlt." >&2; usage; exit 1; fi
if ! [[ "$NAME" =~ ^[A-Za-z_][A-Za-z0-9_]*$ ]]; then
  echo "FEHLER: '$NAME' ist kein gueltiger Projektname (nur Buchstaben/Ziffern/_, nicht mit Ziffer beginnend)." >&2
  exit 1
fi
if ! command -v dotnet >/dev/null 2>&1; then
  echo "FEHLER: 'dotnet' nicht gefunden. Bitte .NET SDK installieren (https://dotnet.microsoft.com)." >&2
  exit 1
fi

# Default-Modul
if [ $((WPF+CONSOLE+NETWORK)) -eq 0 ]; then WPF=1; fi
# Abhaengigkeiten
if [ $MVVM -eq 1 ] && [ $WPF -eq 0 ]; then echo "--mvvm braucht --wpf -> aktiviere --wpf"; WPF=1; fi
if [ $SHAPES -eq 1 ] && [ $WPF -eq 0 ]; then echo "--shapes braucht --wpf -> aktiviere --wpf"; WPF=1; fi

# ---- Hilfsfunktion: Vorlage kopieren + Namespace ersetzen --------------
copy_tpl() {  # <quelle> <ziel> <namespace>
  mkdir -p "$(dirname "$2")"
  sed "s/__NS__/$3/g" "$1" > "$2"
  echo "  + $(basename "$2")  (namespace $3)"
}

SOL_DIR="$OUT/$NAME"
if [ -e "$SOL_DIR" ]; then echo "FEHLER: '$SOL_DIR' existiert bereits." >&2; exit 1; fi

echo "==> Erzeuge Solution '$NAME' in '$SOL_DIR'"
mkdir -p "$SOL_DIR"
cd "$SOL_DIR"
dotnet new sln -n "$NAME" >/dev/null

APP_DIR=""; APP_CSPROJ=""; SERVER_CSPROJ=""

# ---- WPF-App -----------------------------------------------------------
if [ $WPF -eq 1 ]; then
  echo "==> WPF-App-Projekt '$NAME'"
  dotnet new wpf -n "$NAME" -o "$NAME" >/dev/null
  dotnet sln add "$NAME/$NAME.csproj" >/dev/null
  APP_DIR="$NAME"; APP_CSPROJ="$NAME/$NAME.csproj"
fi

# ---- Konsole (Server) --------------------------------------------------
if [ $CONSOLE -eq 1 ]; then
  echo "==> Konsolen-Projekt 'Server'"
  dotnet new console -n "Server" -o "Server" >/dev/null
  dotnet sln add "Server/Server.csproj" >/dev/null
  SERVER_CSPROJ="Server/Server.csproj"
fi

# ---- Network (classlib + Transfer<T>) ----------------------------------
if [ $NETWORK -eq 1 ]; then
  echo "==> Klassenbibliothek 'Network' (Transfer<T>)"
  dotnet new classlib -n "Network" -o "Network" >/dev/null
  rm -f "Network/Class1.cs"
  copy_tpl "$TEMPLATES/network/Transfer.cs" "Network/Transfer.cs" "Network"
  dotnet sln add "Network/Network.csproj" >/dev/null
  [ -n "$APP_CSPROJ" ]    && dotnet add "$APP_CSPROJ" reference "Network/Network.csproj" >/dev/null
  [ -n "$SERVER_CSPROJ" ] && dotnet add "$SERVER_CSPROJ" reference "Network/Network.csproj" >/dev/null
fi

# ---- MVVM (ins WPF-Projekt) -------------------------------------------
if [ $MVVM -eq 1 ]; then
  echo "==> MVVM-Vorlagen -> $APP_DIR"
  copy_tpl "$TEMPLATES/mvvm/ViewModelBase.cs"            "$APP_DIR/ViewModelBase.cs"            "$NAME"
  copy_tpl "$TEMPLATES/mvvm/RelayCommand.cs"             "$APP_DIR/RelayCommand.cs"             "$NAME"
  copy_tpl "$TEMPLATES/converters/BoolToVisibilityConverter.cs" "$APP_DIR/BoolToVisibilityConverter.cs" "$NAME"
fi

# ---- Shapes (ins WPF-Projekt) -----------------------------------------
if [ $SHAPES -eq 1 ]; then
  echo "==> Shape-Vorlage -> $APP_DIR"
  copy_tpl "$TEMPLATES/shapes/ShapeBase.cs" "$APP_DIR/ShapeBase.cs" "$NAME"
fi

# ---- Parser (ins Console- bzw. WPF-Projekt) ---------------------------
if [ $PARSER -eq 1 ]; then
  if [ -n "$SERVER_CSPROJ" ]; then TGT_DIR="Server"; TGT_NS="Server"; else TGT_DIR="$APP_DIR"; TGT_NS="$NAME"; fi
  echo "==> Parser-Skelett -> $TGT_DIR"
  copy_tpl "$TEMPLATES/parser/Token.cs"      "$TGT_DIR/Token.cs"      "$TGT_NS"
  copy_tpl "$TEMPLATES/parser/Expression.cs" "$TGT_DIR/Expression.cs" "$TGT_NS"
fi

# ---- DB (linq2db + SQLite) --------------------------------------------
if [ $DB -eq 1 ]; then
  if [ -n "$SERVER_CSPROJ" ]; then TGT_PROJ="$SERVER_CSPROJ"; TGT_DIR="Server"; TGT_NS="Server"; else TGT_PROJ="$APP_CSPROJ"; TGT_DIR="$APP_DIR"; TGT_NS="$NAME"; fi
  echo "==> linq2db/SQLite-Pakete -> $TGT_PROJ"
  dotnet add "$TGT_PROJ" package linq2db >/dev/null
  dotnet add "$TGT_PROJ" package linq2db.SQLite >/dev/null
  dotnet add "$TGT_PROJ" package Microsoft.Data.Sqlite >/dev/null
  copy_tpl "$TEMPLATES/db/Db.cs" "$TGT_DIR/Db.cs" "$TGT_NS"
  echo "  -> Modell erzeugen mit: $SCRIPT_DIR/scaffold-db.sh \"$SOL_DIR/$TGT_DIR\" <pfad/zur.db>"
fi

# ---- Presets: fertig verdrahtete, lauffaehige Skelette -----------------
case "$PRESET" in
  clientserver)
    echo "==> Preset client/server: MSG + Broadcast-Server + Client"
    copy_tpl "$TEMPLATES/preset/MSG.cs" "Network/MSG.cs" "Network"
    rm -f "Server/Program.cs"
    "$SCRIPT_DIR/gen.sh" server "Server" --msg MSG --port 12345 >/dev/null && echo "  + Server/ChatServer.cs"
    dotnet new console -n "Client" -o "Client" >/dev/null
    dotnet sln add "Client/Client.csproj" >/dev/null
    dotnet add "Client/Client.csproj" reference "Network/Network.csproj" >/dev/null
    "$SCRIPT_DIR/gen.sh" client "Client" --msg MSG --port 12345 >/dev/null && echo "  + Client/ChatClient.cs"
    cp "$TEMPLATES/preset/ClientProgram.cs" "Client/Program.cs" && echo "  + Client/Program.cs"
    echo "  Starten: erst das Server-Projekt, dann ein/mehrere Client-Projekte."
    ;;
  drawing)
    echo "==> Preset drawing: Beispiel-Shape 'Stern'"
    "$SCRIPT_DIR/gen.sh" shape "$APP_DIR" Stern >/dev/null && echo "  + $APP_DIR/Stern.cs"
    echo "  Tipp: in MainWindow.xaml einfuegen:  <local:Stern X1=\"100\" Y1=\"100\" Stroke=\"Black\" StrokeThickness=\"2\"/>"
    ;;
  threading)
    echo "==> Preset threading: ThreadingDemo (lock / SemaphoreSlim / Monitor / Producer-Consumer)"
    copy_tpl "$TEMPLATES/preset/ThreadingDemo.cs" "$APP_DIR/ThreadingDemo.cs" "$NAME"
    ;;
esac

echo ""
echo "FERTIG. Naechste Schritte:"
echo "  cd \"$SOL_DIR\""
echo "  dotnet build        # (WPF nur unter Windows baubar)"
echo "  dotnet sln list"
