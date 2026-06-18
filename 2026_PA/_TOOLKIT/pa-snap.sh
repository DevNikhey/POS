#!/usr/bin/env bash
# =====================================================================
#  pa-snap.sh - leichte Checkpoints (unabhaengig von git) vor riskanten
#  Umbauten. Kopiert die Solution (ohne bin/obj/.vs/.git) in den Toolkit-Ordner.
#
#    ./pa-snap.sh save <label> [ordner=.]
#    ./pa-snap.sh list
#    ./pa-snap.sh restore <label> [ziel=.]
# =====================================================================
set -uo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SNAP_DIR="$SCRIPT_DIR/.snapshots"

copytree() {  # <src> <dest> (ohne bin/obj/.vs/.git)
  if command -v rsync >/dev/null 2>&1; then
    rsync -a --exclude bin --exclude obj --exclude .vs --exclude .git --exclude .snapshots "$1"/ "$2"/
  else
    mkdir -p "$2"
    ( cd "$1" && find . -type d \( -name bin -o -name obj -o -name .vs -o -name .git \) -prune -o -type f -print | while IFS= read -r f; do mkdir -p "$2/$(dirname "$f")"; cp "$f" "$2/$f"; done )
  fi
}

CMD="${1:-}"
case "$CMD" in
  save)
    LABEL="${2:-}"; SRC="${3:-.}"
    [ -n "$LABEL" ] || { echo "Verwendung: ./pa-snap.sh save <label> [ordner]"; exit 1; }
    [ -d "$SRC" ] || { echo "FEHLER: '$SRC' nicht gefunden." >&2; exit 1; }
    DEST="$SNAP_DIR/$LABEL"
    [ -e "$DEST" ] && { echo "Hinweis: Snapshot '$LABEL' wird ueberschrieben."; rm -rf "$DEST"; }
    mkdir -p "$DEST"; copytree "$SRC" "$DEST"
    echo "Snapshot '$LABEL' gespeichert ($(find "$DEST" -type f | wc -l | tr -d ' ') Dateien)."
    ;;
  list)
    [ -d "$SNAP_DIR" ] || { echo "(keine Snapshots)"; exit 0; }
    echo "Snapshots:"; ls -1 "$SNAP_DIR" 2>/dev/null | sed 's/^/  /' || echo "  (keine)"
    ;;
  restore)
    LABEL="${2:-}"; DST="${3:-.}"
    [ -n "$LABEL" ] || { echo "Verwendung: ./pa-snap.sh restore <label> [ziel]"; exit 1; }
    SRC="$SNAP_DIR/$LABEL"
    [ -d "$SRC" ] || { echo "FEHLER: Snapshot '$LABEL' existiert nicht (./pa-snap.sh list)." >&2; exit 1; }
    # Sicherheitsnetz: aktuellen Stand vorher sichern
    AUTO="$SNAP_DIR/_vor_restore_$LABEL"; rm -rf "$AUTO"; mkdir -p "$AUTO"; copytree "$DST" "$AUTO"
    copytree "$SRC" "$DST"
    echo "Snapshot '$LABEL' wiederhergestellt nach '$DST'."
    echo "(vorheriger Stand gesichert als '_vor_restore_$LABEL')"
    ;;
  *)
    echo "Verwendung: ./pa-snap.sh save <label> [ordner] | list | restore <label> [ziel]"; exit 1 ;;
esac
