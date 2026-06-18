#!/usr/bin/env bash
# =====================================================================
#  snip.sh - Schnipsel-Bibliothek der wiederkehrenden PA-Muster.
#  Druckt das Snippet UND kopiert es in die Zwischenablage (pbcopy/xclip).
#
#  ./snip.sh              # Liste der Namen
#  ./snip.sh <name>       # Snippet ausgeben + kopieren
#  ./snip.sh -i           # interaktiv per Pfeiltasten auswaehlen
# =====================================================================
set -uo pipefail

NAMES="dispatcher loaded itemssource selectionchanged usedb insertid arc xmlroot elseguard overridemeta semaphore datatemplate"

snip_content() {
  case "$1" in
    dispatcher) cat <<'EOF'
// GUI-Zugriff aus einem anderen Thread (Empfangs-/Worker-Thread) -> Dispatcher!
Application.Current.Dispatcher.Invoke(() =>
{
    // hier GUI-Elemente anfassen
});
EOF
;;
    loaded) cat <<'EOF'
// ActualWidth/ActualHeight sind im Konstruktor 0 - erst im Loaded gesetzt:
this.Loaded += (s, e) =>
{
    // groessenabhaengiges Zeichnen / Markierungen hier
};
EOF
;;
    itemssource) cat <<'EOF'
liste.ItemsSource = items;   // NICHT liste.Items.Add(items)
EOF
;;
    selectionchanged) cat <<'EOF'
private void box_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (box.SelectedItem is var sel && sel != null)
    {
        // ausgewaehltes Element verarbeiten
    }
}
EOF
;;
    usedb) cat <<'EOF'
var options = new DataOptions().UseSQLite("Data Source=meine.db");
using var db = new MeineDB(options);
var liste = db.Tabelle.Where(x => x.Name.Contains(suche)).ToList();
EOF
;;
    insertid) cat <<'EOF'
long neueId = db.InsertWithInt64Identity(entity);   // Insert + neue Id zurueck
EOF
;;
    arc) cat <<'EOF'
// Tortenstueck/Bogen: Start am Mittelpunkt -> Linie zum Rand -> ArcSegment -> zu (IsClosed schliesst)
var fig = new PathFigure { StartPoint = center, IsClosed = true };
fig.Segments.Add(new LineSegment(p0, true));
fig.Segments.Add(new ArcSegment(p1, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, true));
EOF
;;
    xmlroot) cat <<'EOF'
// Objekt <-> XML (passt zum Transfer<T>)
var ser = new XmlSerializer(typeof(MSG));
using var sw = new StringWriter(); ser.Serialize(sw, msg); string xml = sw.ToString();
using var sr = new StringReader(xml); MSG back = (MSG)ser.Deserialize(sr);
EOF
;;
    elseguard) cat <<'EOF'
// Optionales Keyword nur konsumieren, wenn der WERT passt (nicht nur der Typ):
if (tokens.Count > 0 && tokens[0].Type == Token.TokenType.KEYWORD && tokens[0].Value == "ELSE")
{
    tokens.RemoveAt(0);
    elseBlock.Parse(tokens);
}
EOF
;;
    overridemeta) cat <<'EOF'
// Templated Control: im statischen Konstruktor den Default-Style-Key setzen
static MeinControl()
{
    DefaultStyleKeyProperty.OverrideMetadata(typeof(MeinControl),
        new FrameworkPropertyMetadata(typeof(MeinControl)));
}
EOF
;;
    semaphore) cat <<'EOF'
sem.Wait();
try
{
    // kritischer Abschnitt
}
finally
{
    sem.Release();   // IMMER im finally!
}
EOF
;;
    datatemplate) cat <<'EOF'
<ListBox.ItemTemplate>
    <DataTemplate>
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="{Binding Name}" Margin="5"/>
        </StackPanel>
    </DataTemplate>
</ListBox.ItemTemplate>
EOF
;;
    *) return 1 ;;
  esac
}

to_clip() {  # liest stdin, kopiert in die Zwischenablage falls moeglich
  if command -v pbcopy >/dev/null 2>&1; then pbcopy && echo "(in Zwischenablage kopiert)" >&2
  elif command -v xclip >/dev/null 2>&1; then xclip -selection clipboard && echo "(in Zwischenablage kopiert)" >&2
  else cat >/dev/null; fi
}

# --- interaktives Menue (Pfeiltasten) ---
menu_single() {
  local title="$1"; shift
  local opts=("$@") n=$# cur=0 c c2 i
  printf '%s  (Pfeil hoch/runter, Enter)\n' "$title" >&2
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
  CHOICE="${opts[$cur]}"
}

show() {  # <name>
  local out
  if ! out="$(snip_content "$1")"; then echo "Unbekannt: $1" >&2; echo "Verfuegbar: $NAMES" >&2; exit 1; fi
  printf '%s\n' "$out"
  printf '%s' "$out" | to_clip
}

ARG="${1:-}"
if [ "$ARG" = "-i" ] || [ "$ARG" = "--interactive" ]; then
  { [ -t 0 ] && [ -t 2 ]; } || { echo "FEHLER: -i braucht ein Terminal." >&2; exit 1; }
  # shellcheck disable=SC2086
  menu_single "Snippet waehlen:" $NAMES
  show "$CHOICE"
elif [ -z "$ARG" ]; then
  echo "Snippets: $NAMES"
  echo "Nutzung: ./snip.sh <name>   oder   ./snip.sh -i"
else
  show "$ARG"
fi
