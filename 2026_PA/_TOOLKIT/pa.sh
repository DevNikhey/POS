#!/usr/bin/env bash
# =====================================================================
#  pa.sh - EIN Launcher fuer das ganze PA-Toolkit, mit Pfeiltasten-Navigation.
#  Einfach starten:  ./pa.sh
# =====================================================================
set -uo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# --- Pfeiltasten-Menue -> setzt CHOICE, CHOICE_IDX ---
menu() {
  local title="$1"; shift
  local opts=("$@") n=$# cur=0 c c2 i
  printf '\n%s  (Pfeil hoch/runter, Enter)\n' "$title" >&2
  while true; do
    for i in "${!opts[@]}"; do
      if [ "$i" -eq "$cur" ]; then printf '  \033[7m> %s\033[0m\033[K\n' "${opts[$i]}" >&2
      else printf '    %s\033[K\n' "${opts[$i]}" >&2; fi
    done
    IFS= read -rsn1 c
    case "$c" in
      $'\x1b') IFS= read -rsn2 -t 1 c2; case "$c2" in '[A'|'[D') cur=$(((cur-1+n)%n));; '[B'|'[C') cur=$(((cur+1)%n));; esac ;;
      '') break ;;
    esac
    printf '\033[%dA' "$n" >&2
  done
  CHOICE="${opts[$cur]}"; CHOICE_IDX=$cur
}

categories() {
  echo "Projekt & Bausteine"
  echo "Code generieren"
  echo "Checks vor der Abgabe"
  echo "Abgabe verpacken"
  echo "Sicherheit (Snapshots)"
  echo "Datenbank"
  echo "Client/Server testen"
  echo "Snippets & Infos"
}

# label | script | standard-argumente
entries() {
  case "$1" in
    "Projekt & Bausteine")
      echo "Neue Solution (new-pa)|new-pa.sh|MeinePA --wpf --mvvm"
      echo "Preset Client/Server|new-pa.sh|Chat --clientserver"
      echo "Preset Drawing (WPF+Shape)|new-pa.sh|MeineGrafik --drawing"
      echo "Preset Threading (WPF)|new-pa.sh|MeinThreading --threading"
      echo "Baustein einfuegen (add)|add.sh|./MeinePA/MeinePA mvvm converter" ;;
    "Code generieren")
      echo "DataTemplate interaktiv|gen.sh|datatemplate -i"
      echo "Snippet (dp/prop/cmd/model/...)|gen.sh|prop Vorname string"
      echo "Grid-Geruest (gen grid)|gen.sh|grid 3 2"
      echo "Enum (gen enum)|gen.sh|enum ./Proj MessageType SEARCH DETAIL"
      echo "Event-Handler-Stubs (gen handlers)|gen.sh|handlers ./Proj"
      echo "Converter registrieren (register)|register.sh|./Proj converter MeinConverter"
      echo "Server generieren|gen.sh|server ./Proj --msg MSG --port 12345"
      echo "Client generieren|gen.sh|client ./Proj --msg MSG --port 12345"
      echo "Threading-Primitiv|gen.sh|sync semaphore Landebahn --count 3" ;;
    "Checks vor der Abgabe")
      echo "Abgabe-Check (alles)|pa-ready.sh|."
      echo "Bewertungsfehler (pa-check)|pa-check.sh|."
      echo "Threading (lock-check)|lock-check.sh|."
      echo "Unfertige Stellen (find-todo)|find-todo.sh|." ;;
    "Abgabe verpacken")
      echo "Sauberes ZIP (pa-submit)|pa-submit.sh|. PA_Name" ;;
    "Sicherheit (Snapshots)")
      echo "Checkpoint speichern (pa-snap save)|pa-snap.sh|save works ."
      echo "Snapshots auflisten|pa-snap.sh|list"
      echo "Checkpoint zurueck (pa-snap restore)|pa-snap.sh|restore works ." ;;
    "Datenbank")
      echo "DB ansehen (db-inspect)|db-inspect.sh|meine.db"
      echo "SQL-Abfrage (db-query)|db-query.sh|meine.db"
      echo ".tt generieren (db-tt)|db-tt.sh|./Proj ./Proj/meine.db"
      echo ".tt-Pfad reparieren (tt-fixpath)|tt-fixpath.sh|./Proj/DataModel.tt"
      echo "PreserveNewest pruefen (db-copycheck)|db-copycheck.sh|./Proj/Proj.csproj"
      echo "Modell aus .db (scaffold-db)|scaffold-db.sh|./Proj ./Proj/meine.db"
      echo "DB aus .sql (db-create)|db-create.sh|schema.sql meine.db" ;;
    "Client/Server testen")
      echo "Framed Test/Echo (frame-probe)|frame-probe.sh|localhost 12345 --root MSG --field type=SEARCH"
      echo "Port lauscht? (port-check)|port-check.sh|12345"
      echo "Roher TCP-Test (net-probe)|net-probe.sh|localhost 12345 PING" ;;
    "Snippets & Infos")
      echo "Snippet kopieren (snip)|snip.sh|-i"
      echo "Merkkarte (cheatsheet)|cheatsheet.sh|" ;;
  esac
}

clear 2>/dev/null || true
echo "==============================================="
echo "   PA-TOOLKIT   -   Launcher"
echo "   (Pfeiltasten + Enter; im Untermenue 'zurueck')"
echo "==============================================="

while true; do
  cats=(); while IFS= read -r l; do cats+=("$l"); done < <(categories)
  menu "Kategorie:" "${cats[@]}" "Beenden"
  [ "$CHOICE" = "Beenden" ] && { echo "Tschuess."; break; }

  labels=(); cmds=(); defs=()
  while IFS='|' read -r l c d; do labels+=("$l"); cmds+=("$c"); defs+=("$d"); done < <(entries "$CHOICE")

  menu "$CHOICE:" "${labels[@]}" "<- zurueck"
  [ "$CHOICE" = "<- zurueck" ] && continue
  idx=$CHOICE_IDX
  cmd="${cmds[$idx]}"; def="${defs[$idx]}"

  echo
  echo "Standard:  $cmd $def"
  printf 'Argumente (Enter = Standard, 'q' = zurueck): '
  IFS= read -r ARGS
  [ "$ARGS" = "q" ] && continue
  [ -z "$ARGS" ] && ARGS="$def"
  echo "-----------------------------------------------"
  echo "> $cmd $ARGS"
  echo "-----------------------------------------------"
  # shellcheck disable=SC2086
  "$SCRIPT_DIR/$cmd" $ARGS || echo "(Befehl endete mit Fehlercode $?)"
  echo
  printf '[Enter] zurueck zum Menue ... '; IFS= read -r _
done
