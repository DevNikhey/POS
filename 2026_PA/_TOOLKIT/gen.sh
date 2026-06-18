#!/usr/bin/env bash
# =====================================================================
#  gen.sh - Code-Generatoren gegen Tipparbeit & Boilerplate-Fehler.
#
#    ./gen.sh dp <Klasse> <Name> <Typ> [--render]   # DependencyProperty (stdout)
#    ./gen.sh window <projekt-ordner> <Name>         # WPF-Window (.xaml + .xaml.cs)
#    ./gen.sh vm <projekt-ordner> <Name>             # ViewModel-Stub (ViewModelBase)
#    ./gen.sh converter <projekt-ordner> <Name>      # IValueConverter-Stub
#
#  Beispiele:
#    ./gen.sh dp Spirale Umdrehung int
#    ./gen.sh dp Slice Radius double --render
#    ./gen.sh window ./MeinePA/MeinePA Detail
# =====================================================================
set -uo pipefail

sub="${1:-}"; [ $# -gt 0 ] && shift || true

usage() {
  cat <<'EOF'
Verwendung:
  Ausgabe zum Kopieren (stdout):
    ./gen.sh dp <Klasse> <Name> <Typ> [--render]          DependencyProperty
    ./gen.sh prop <Name> <Typ>                            INotifyPropertyChanged-Property (SetProperty)
    ./gen.sh cmd <Name>                                   RelayCommand-Property + Handler
    ./gen.sh grid <zeilen> <spalten>                      Grid mit Row/Column-Definitionen
    ./gen.sh datatemplate [Typ] [feld...] [optionen]      XAML-DataTemplate fuer Listen
        -i/--interactive  Felder/Layout/Optionen per Pfeiltasten auswaehlen
        --from <Klasse.cs>  Felder aus Klasse lesen      -v/--vertical  untereinander
        -g/--grid  ausgerichtete Tabelle (Label|Wert)    -l/--labels  "Feld:"-Beschriftung
        -b/--border  Rahmen        --width <n>  feste Breite
        --image <Feld>  als <Image>     --checkbox <Feld>  als <CheckBox>   (mehrfach moeglich)
        --click <Handler>  Klick-Event
  Datei im Projekt anlegen:
    ./gen.sh window <projekt-ordner> <Name>               WPF-Window (.xaml + .xaml.cs)
    ./gen.sh vm <projekt-ordner> <Name>                   ViewModel-Stub (braucht ViewModelBase)
    ./gen.sh converter <projekt-ordner> <Name>            IValueConverter-Stub
    ./gen.sh shape <projekt-ordner> <Name>                Zeichen-Control (braucht ShapeBase)
    ./gen.sh control <projekt-ordner> <Name>              Templated Control + Generic.xaml-Snippet
    ./gen.sh model <projekt-ordner> <Klasse> [feld:typ...] POCO-Klasse
    ./gen.sh class <projekt-ordner> <Name>                leere Klasse
    ./gen.sh enum <projekt-ordner> <Name> <Werte...>      Enum-Datei
    ./gen.sh handlers <projekt-ordner>                    fehlende Event-Handler-Stubs ins Code-Behind
  Client/Server (Transfer<T>):
    ./gen.sh server <projekt-ordner> [--msg MSG] [--port 12345] [--name ChatServer]   Multi-Client-Broadcast-Server
    ./gen.sh client <projekt-ordner> [--msg MSG] [--port 12345] [--host localhost]    Client (verbindet/sendet/empfaengt)
  Threading (PA1):
    ./gen.sh sync lock|semaphore|autoreset|monitor|prodcons <Name> [--count N]        Synchronisations-Primitive (richtige Idiome)
EOF
  exit 1
}

get_ns() {
  local d="$1"
  [ -d "$d" ] || { echo "FEHLER: Ordner '$d' nicht gefunden." >&2; exit 1; }
  local cs; cs="$(ls "$d"/*.csproj 2>/dev/null | head -1)"
  [ -n "$cs" ] || { echo "FEHLER: keine .csproj in '$d'." >&2; exit 1; }
  basename "$cs" .csproj
}

write_new() {  # <pfad> <inhalt>
  local p="$1"
  if [ -e "$p" ]; then echo "  ! $(basename "$p") existiert bereits - uebersprungen"; return; fi
  printf '%s' "$2" > "$p"
  echo "  + $(basename "$p")"
}

# Membership-Test in einer durch Leerzeichen getrennten Liste
in_list() { case " $2 " in *" $1 "*) return 0 ;; *) return 1 ;; esac; }

# Interaktive Menüs (Pfeiltasten). Zeichnen auf stderr -> stdout bleibt sauber (XAML).
_tty_guard() { { [ -t 0 ] && [ -t 2 ]; } || { echo "FEHLER: interaktiver Modus (-i) braucht ein echtes Terminal." >&2; exit 1; }; }

menu_single() {  # $1=Titel; danach Optionen -> setzt CHOICE_IDX, CHOICE
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
      $'\x1b')
        IFS= read -rsn2 -t 1 c2
        case "$c2" in
          '[A'|'[D') cur=$(( (cur-1+n)%n )) ;;
          '[B'|'[C') cur=$(( (cur+1)%n )) ;;
        esac ;;
      '') break ;;
    esac
    printf '\033[%dA' "$n" >&2
  done
  CHOICE_IDX=$cur; CHOICE="${opts[$cur]}"
}

menu_multi() {  # $1=Titel; danach Optionen -> setzt SELECTED (Array)
  local title="$1"; shift
  local opts=("$@") n=$# cur=0 c c2 i
  local checked=(); for ((i=0;i<n;i++)); do checked[i]=0; done
  printf '%s  (Pfeile, Leertaste=an/aus, Enter=fertig)\n' "$title" >&2
  while true; do
    for i in "${!opts[@]}"; do
      local mark="[ ]"; [ "${checked[$i]}" -eq 1 ] && mark="[x]"
      if [ "$i" -eq "$cur" ]; then printf '  \033[7m> %s %s\033[0m\033[K\n' "$mark" "${opts[$i]}" >&2
      else printf '    %s %s\033[K\n' "$mark" "${opts[$i]}" >&2; fi
    done
    IFS= read -rsn1 c
    case "$c" in
      $'\x1b')
        IFS= read -rsn2 -t 1 c2
        case "$c2" in
          '[A') cur=$(( (cur-1+n)%n )) ;;
          '[B') cur=$(( (cur+1)%n )) ;;
        esac ;;
      ' ') checked[cur]=$(( 1 - checked[cur] )) ;;
      '') break ;;
    esac
    printf '\033[%dA' "$n" >&2
  done
  SELECTED=(); for i in "${!opts[@]}"; do [ "${checked[$i]}" -eq 1 ] && SELECTED+=("${opts[$i]}"); done
}

case "$sub" in
  dp)
    CLS="${1:-}"; NAME="${2:-}"; TYP="${3:-}"; RENDER="${4:-}"
    { [ -n "$CLS" ] && [ -n "$NAME" ] && [ -n "$TYP" ]; } || usage
    if [ "$RENDER" = "--render" ]; then
      META="new FrameworkPropertyMetadata(default($TYP),
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure)"
    else
      META="new PropertyMetadata(default($TYP))"
    fi
    cat <<EOF
public static readonly DependencyProperty ${NAME}Property =
    DependencyProperty.Register(nameof($NAME), typeof($TYP), typeof($CLS),
        $META);

public $TYP $NAME
{
    get => ($TYP)GetValue(${NAME}Property);
    set => SetValue(${NAME}Property, value);
}
EOF
    ;;
  window)
    PROJ="${1:-}"; NAME="${2:-}"; { [ -n "$PROJ" ] && [ -n "$NAME" ]; } || usage
    NS="$(get_ns "$PROJ")"
    write_new "$PROJ/$NAME.xaml" "<Window x:Class=\"$NS.$NAME\"
        xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"
        xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"
        Title=\"$NAME\" Height=\"450\" Width=\"800\">
    <Grid>

    </Grid>
</Window>
"
    write_new "$PROJ/$NAME.xaml.cs" "using System.Windows;

namespace $NS
{
    public partial class $NAME : Window
    {
        public $NAME()
        {
            InitializeComponent();
        }
    }
}
"
    ;;
  vm)
    PROJ="${1:-}"; NAME="${2:-}"; { [ -n "$PROJ" ] && [ -n "$NAME" ]; } || usage
    NS="$(get_ns "$PROJ")"
    write_new "$PROJ/$NAME.cs" "namespace $NS;

// Braucht ViewModelBase (z.B. via: ./add.sh \"$PROJ\" mvvm)
public class $NAME : ViewModelBase
{
    // private string _text = \"\";
    // public string Text { get => _text; set => SetProperty(ref _text, value); }
}
"
    ;;
  converter)
    PROJ="${1:-}"; NAME="${2:-}"; { [ -n "$PROJ" ] && [ -n "$NAME" ]; } || usage
    NS="$(get_ns "$PROJ")"
    write_new "$PROJ/$NAME.cs" "using System;
using System.Globalization;
using System.Windows.Data;

namespace $NS;

public class $NAME : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
"
    ;;
  prop)
    NAME="${1:-}"; TYP="${2:-}"; { [ -n "$NAME" ] && [ -n "$TYP" ]; } || usage
    FIELD="_$(printf '%s' "${NAME:0:1}" | tr '[:upper:]' '[:lower:]')${NAME:1}"
    cat <<EOF
private $TYP $FIELD;
public $TYP $NAME
{
    get => $FIELD;
    set => SetProperty(ref $FIELD, value);
}
EOF
    ;;
  cmd)
    NAME="${1:-}"; [ -n "$NAME" ] || usage
    cat <<EOF
// using System.Windows.Input;   // fuer ICommand
public ICommand ${NAME}Command { get; }

// Im Konstruktor setzen:
//   ${NAME}Command = new RelayCommand(_ => On${NAME}());

private void On${NAME}()
{
    // TODO
}
EOF
    ;;
  datatemplate)
    TYP="${1:-}"; { [ -n "$TYP" ] && [ "${TYP#-}" = "$TYP" ]; } && shift || TYP=""
    FROM=""; ORIENT="Horizontal"; LABELS=0; BORDER=0; CLICK=""; GRID=0; WIDTH=""; INTERACTIVE=0
    IMAGES=""; CHECKS=""
    fields=()
    while [ $# -gt 0 ]; do
      case "$1" in
        --from)           FROM="${2:-}"; shift 2 || shift ;;
        --vertical|-v)    ORIENT="Vertical"; shift ;;
        --horizontal)     ORIENT="Horizontal"; shift ;;
        --grid|-g)        GRID=1; shift ;;
        --labels|-l)      LABELS=1; shift ;;
        --border|-b)      BORDER=1; shift ;;
        --width)          WIDTH="${2:-}"; shift 2 || shift ;;
        --image)          IMAGES="$IMAGES ${2:-}"; shift 2 || shift ;;
        --checkbox)       CHECKS="$CHECKS ${2:-}"; shift 2 || shift ;;
        --click)          CLICK="${2:-}"; shift 2 || shift ;;
        --interactive|-i) INTERACTIVE=1; shift ;;
        *)                fields+=("$1"); shift ;;
      esac
    done
    # Properties aus einer Klasse lesen (Auto-Properties: public Typ Name { get; set; })
    if [ -n "$FROM" ]; then
      [ -f "$FROM" ] || { echo "FEHLER: '$FROM' nicht gefunden." >&2; exit 1; }
      while IFS= read -r p; do [ -n "$p" ] && fields+=("$p"); done < <(
        grep -E 'public[^;(]*\{[[:space:]]*get[[:space:]]*[;=]' "$FROM" \
          | sed -E 's/.*[^A-Za-z0-9_]([A-Za-z_][A-Za-z0-9_]*)[[:space:]]*\{.*/\1/')
      [ -n "$TYP" ] || TYP="$(grep -E 'class[[:space:]]+[A-Za-z_]' "$FROM" | grep -vE 'static|DataConnection|: *Window|: *Application' | head -1 | sed -E 's/.*class[[:space:]]+([A-Za-z_][A-Za-z0-9_]*).*/\1/')"
    fi
    [ -n "$TYP" ] || TYP="Item"
    # Interaktiv per Pfeiltasten auswaehlen
    if [ "$INTERACTIVE" -eq 1 ]; then
      _tty_guard
      if [ ${#fields[@]} -gt 0 ]; then
        menu_multi "Felder auswaehlen:" "${fields[@]}"
        [ ${#SELECTED[@]} -gt 0 ] && fields=("${SELECTED[@]}")
      fi
      menu_single "Layout:" "nebeneinander (horizontal)" "untereinander (vertical)" "Tabelle (grid)"
      case "$CHOICE_IDX" in 0) ORIENT="Horizontal"; GRID=0 ;; 1) ORIENT="Vertical"; GRID=0 ;; 2) GRID=1 ;; esac
      menu_multi "Optionen:" "Labels (Feld:)" "Rahmen (Border)"
      LABELS=0; BORDER=0
      for s in ${SELECTED[@]+"${SELECTED[@]}"}; do
        case "$s" in Labels*) LABELS=1 ;; Rahmen*) BORDER=1 ;; esac
      done
      printf 'Klick-Handler (leer = keiner): ' >&2; IFS= read -r CLICK
    fi
    [ ${#fields[@]} -gt 0 ] || fields=("Name")
    [ "$GRID" -eq 1 ] && LABELS=1   # Grid nutzt ausgerichtete Label/Wert-Spalten

    # Ein Wert-Element rendern: Image / CheckBox / TextBlock (mit optionaler Breite)
    render_val() {  # $1=Feld  $2=zusatz-attribute
    local f="$1" extra="${2:-}" w=""
    if in_list "$f" "$IMAGES"; then
      echo "<Image$extra Source=\"{Binding $f}\" Width=\"64\" Height=\"64\" Stretch=\"Uniform\" Margin=\"5\"/>"
    elif in_list "$f" "$CHECKS"; then
      echo "<CheckBox$extra IsChecked=\"{Binding $f}\" Margin=\"5\"/>"
    else
      [ -n "$WIDTH" ] && w=" Width=\"$WIDTH\""
      echo "<TextBlock$extra Text=\"{Binding $f}\"$w Margin=\"5\"/>"
    fi
    }

    inner=""
    if [ "$GRID" -eq 1 ]; then
      rowdefs=""; cells=""; idx=0
      for f in "${fields[@]}"; do
        rowdefs="$rowdefs            <RowDefinition Height=\"Auto\"/>
"
        cells="$cells            <TextBlock Grid.Row=\"$idx\" Grid.Column=\"0\" Text=\"$f:\" FontWeight=\"Bold\" Margin=\"5,2\"/>
            $(render_val "$f" " Grid.Row=\"$idx\" Grid.Column=\"1\"")
"
        idx=$((idx+1))
      done
      inner="        <Grid.ColumnDefinitions>
            <ColumnDefinition Width=\"Auto\"/>
            <ColumnDefinition Width=\"*\"/>
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
$rowdefs        </Grid.RowDefinitions>
$cells"
    else
      for f in "${fields[@]}"; do
        if [ "$ORIENT" = "Vertical" ] && [ "$LABELS" -eq 1 ]; then
          inner="$inner            <StackPanel Orientation=\"Horizontal\">
                <TextBlock Text=\"$f:\" Width=\"110\" FontWeight=\"Bold\"/>
                $(render_val "$f")
            </StackPanel>
"
        elif [ "$LABELS" -eq 1 ]; then
          inner="$inner            <TextBlock Text=\"$f:\" FontWeight=\"Bold\" Margin=\"5,0,2,0\"/>
            $(render_val "$f")
"
        else
          inner="$inner            $(render_val "$f")
"
        fi
      done
    fi

    # Klick-Event: auf den Border (falls vorhanden), sonst auf den Container.
    panel_click=""; border_click=""
    if [ -n "$CLICK" ]; then
      if [ "$BORDER" -eq 1 ]; then border_click=" Background=\"Transparent\" MouseLeftButtonDown=\"$CLICK\""
      else panel_click=" Background=\"Transparent\" MouseLeftButtonDown=\"$CLICK\""; fi
    fi
    if [ "$GRID" -eq 1 ]; then open="<Grid$panel_click>"; close="</Grid>"
    else open="<StackPanel Orientation=\"$ORIENT\"$panel_click>"; close="</StackPanel>"; fi

    if [ "$BORDER" -eq 1 ]; then
      cat <<EOF
<DataTemplate DataType="{x:Type local:$TYP}">
    <Border BorderBrush="Gray" BorderThickness="1" CornerRadius="3" Padding="4" Margin="2"$border_click>
        $open
$inner        $close
    </Border>
</DataTemplate>
EOF
    else
      cat <<EOF
<DataTemplate DataType="{x:Type local:$TYP}">
    $open
$inner    $close
</DataTemplate>
EOF
    fi
    ;;
  shape)
    PROJ="${1:-}"; NAME="${2:-}"; { [ -n "$PROJ" ] && [ -n "$NAME" ]; } || usage
    NS="$(get_ns "$PROJ")"
    write_new "$PROJ/$NAME.cs" "using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace $NS;

// Eigenes Zeichen-Control. Braucht ShapeBase (./add.sh \"$PROJ\" shapes).
public class $NAME : ShapeBase
{
    // Eigene Properties z.B. via:  ./gen.sh dp $NAME Radius double --render

    protected override PathFigure CreatePathFigure()
    {
        PathFigure figure = new PathFigure
        {
            StartPoint = new Point(X1, Y1),
            IsClosed = true
        };

        // TODO: Segmente HINZUFUEGEN, sonst wird nichts gezeichnet!
        // figure.Segments.Add(new LineSegment(new Point(X1 + 50, Y1), true));

        return figure;
    }
}
"
    ;;
  control)
    PROJ="${1:-}"; NAME="${2:-}"; { [ -n "$PROJ" ] && [ -n "$NAME" ]; } || usage
    NS="$(get_ns "$PROJ")"
    write_new "$PROJ/$NAME.cs" "using System.Windows;
using System.Windows.Controls;

namespace $NS;

// Templated Control. Der Template-Style gehoert in Themes/Generic.xaml
// (Snippet unten). Meist in einer eigenen Library mit <UseWPF>.
public class $NAME : Control
{
    static $NAME()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof($NAME),
            new FrameworkPropertyMetadata(typeof($NAME)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        // var part = GetTemplateChild(\"PART_X\") as Button;
    }
}
"
    echo
    echo "--> in Themes/Generic.xaml (innerhalb <ResourceDictionary>) einfuegen:"
    cat <<EOF
<Style TargetType="{x:Type local:$NAME}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:$NAME}">
                <Grid>
                    <!-- z.B. <Button x:Name="PART_X"/> -->
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
EOF
    ;;
  model)
    PROJ="${1:-}"; CLS="${2:-}"; { [ -n "$PROJ" ] && [ -n "$CLS" ]; } || usage
    NS="$(get_ns "$PROJ")"
    shift 2 || true
    props=""
    for spec in "$@"; do
      fname="${spec%%:*}"; ftyp="${spec#*:}"
      [ "$fname" = "$ftyp" ] && ftyp="string"
      props="$props    public $ftyp $fname { get; set; }"$'\n'
    done
    [ -n "$props" ] || props="    // public string Name { get; set; }"$'\n'
    write_new "$PROJ/$CLS.cs" "namespace $NS;

public class $CLS
{
$props}
"
    ;;
  class)
    PROJ="${1:-}"; NAME="${2:-}"; { [ -n "$PROJ" ] && [ -n "$NAME" ]; } || usage
    NS="$(get_ns "$PROJ")"
    write_new "$PROJ/$NAME.cs" "namespace $NS;

public class $NAME
{
}
"
    ;;
  sync)
    KIND="${1:-}"; NAME="${2:-}"; { [ -n "$KIND" ] && [ -n "$NAME" ]; } || usage
    [ $# -ge 2 ] && shift 2 || true
    COUNT=1
    while [ $# -gt 0 ]; do case "$1" in --count) COUNT="${2:-1}"; shift 2 || shift ;; *) shift ;; esac; done
    LOW="$(printf '%s' "${NAME:0:1}" | tr '[:upper:]' '[:lower:]')${NAME:1}"
    case "$KIND" in
      lock)
        cat <<EOF
// lock: IMMER auf ein privates readonly-Objekt sperren (nie this/typeof/string/public Feld!)
private readonly object ${LOW}Lock = new object();

public void ${NAME}()
{
    lock (${LOW}Lock)
    {
        // kritischer Abschnitt
    }
}
EOF
        ;;
      semaphore)
        cat <<EOF
// SemaphoreSlim: erlaubt max. $COUNT gleichzeitige Zugriffe (z.B. $COUNT Landebahnen)
private readonly SemaphoreSlim ${LOW}Semaphore = new SemaphoreSlim($COUNT, $COUNT);

public void ${NAME}()
{
    ${LOW}Semaphore.Wait();
    try
    {
        // kritischer Abschnitt (max. $COUNT Threads gleichzeitig)
    }
    finally
    {
        ${LOW}Semaphore.Release();   // IMMER im finally freigeben!
    }
}
EOF
        ;;
      autoreset)
        cat <<EOF
// AutoResetEvent: weckt GENAU EINEN wartenden Thread. Start false = gesperrt.
private readonly AutoResetEvent ${LOW}Event = new AutoResetEvent(false);

// ... im wartenden Thread:
${LOW}Event.WaitOne();                 // blockiert, bis Set() kommt

// ... beim Freigeben (genau einen Wartenden wecken):
${LOW}Event.Set();
// Hinweis: fuer MEHRERE Wartende mehrmals Set() ODER SemaphoreSlim/ManualResetEvent verwenden.
EOF
        ;;
      monitor)
        cat <<EOF
private readonly object ${LOW}Lock = new object();

// Warten auf eine Bedingung -> IMMER in while(), NICHT if()!
lock (${LOW}Lock)
{
    while (/* !bedingung */ true)
        Monitor.Wait(${LOW}Lock);
    // Bedingung erfuellt -> weiter
}

// Zustand geaendert -> Wartende wecken:
lock (${LOW}Lock)
{
    // ... Zustand aendern ...
    Monitor.PulseAll(${LOW}Lock);
}
EOF
        ;;
      prodcons)
        cat <<EOF
// Producer/Consumer mit BlockingCollection (thread-safe Queue; Consumer blockiert, bis etwas da ist)
// using System.Collections.Concurrent;
private readonly BlockingCollection<${NAME}> ${LOW}Queue = new BlockingCollection<${NAME}>();

// Producer (z.B. neues ${NAME} einreihen):
${LOW}Queue.Add(item);

// Consumer (in eigenem Thread starten):
foreach (${NAME} item in ${LOW}Queue.GetConsumingEnumerable())
{
    // item verarbeiten (blockiert automatisch, bis etwas da ist)
}
// Wenn nichts mehr produziert wird:
${LOW}Queue.CompleteAdding();
EOF
        ;;
      *) echo "kind: lock | semaphore | autoreset | monitor | prodcons" >&2; exit 1 ;;
    esac
    ;;
  grid)
    ROWS="${1:-2}"; COLS="${2:-2}"
    echo "<Grid>"
    echo "    <Grid.RowDefinitions>"
    r=0; while [ "$r" -lt "$ROWS" ] 2>/dev/null; do echo "        <RowDefinition Height=\"Auto\"/>"; r=$((r+1)); done
    echo "    </Grid.RowDefinitions>"
    echo "    <Grid.ColumnDefinitions>"
    c=0; while [ "$c" -lt "$COLS" ] 2>/dev/null; do echo "        <ColumnDefinition Width=\"*\"/>"; c=$((c+1)); done
    echo "    </Grid.ColumnDefinitions>"
    echo
    echo "    <!-- <TextBlock Grid.Row=\"0\" Grid.Column=\"0\" Text=\"...\"/> -->"
    echo "</Grid>"
    ;;
  enum)
    PROJ="${1:-}"; ENAME="${2:-}"; { [ -n "$PROJ" ] && [ -n "$ENAME" ]; } || usage
    NS="$(get_ns "$PROJ")"
    shift 2 || true
    body=""
    for v in "$@"; do body="$body    $v,"$'\n'; done
    [ -n "$body" ] || body="    A,
    B,
    C,
"
    write_new "$PROJ/$ENAME.cs" "namespace $NS;

public enum $ENAME
{
$body}
"
    ;;
  handlers)
    PROJ="${1:-}"; [ -d "$PROJ" ] || { echo "Verwendung: ./gen.sh handlers <projekt-ordner>" >&2; exit 1; }
    argtype() {
      case "$1" in
        SelectionChanged) echo "SelectionChangedEventArgs" ;;
        TextChanged) echo "TextChangedEventArgs" ;;
        KeyDown|KeyUp|PreviewKeyDown|PreviewKeyUp) echo "KeyEventArgs" ;;
        MouseDown|MouseUp|MouseLeftButtonDown|MouseLeftButtonUp|MouseRightButtonDown|MouseRightButtonUp|PreviewMouseLeftButtonDown) echo "MouseButtonEventArgs" ;;
        MouseMove|MouseEnter|MouseLeave) echo "MouseEventArgs" ;;
        MouseWheel|PreviewMouseWheel) echo "MouseWheelEventArgs" ;;
        Drop|DragEnter|DragOver|DragLeave) echo "DragEventArgs" ;;
        Closing) echo "System.ComponentModel.CancelEventArgs" ;;
        *) echo "RoutedEventArgs" ;;
      esac
    }
    EVENTS="Click SelectionChanged TextChanged PreviewKeyDown KeyDown KeyUp PreviewMouseLeftButtonDown MouseLeftButtonDown MouseLeftButtonUp MouseRightButtonDown MouseDown MouseUp MouseMove MouseEnter MouseLeave MouseWheel Loaded Unloaded Checked Unchecked Drop DragEnter DragOver Closing"
    total=0
    while IFS= read -r xaml; do
      [ -z "$xaml" ] && continue
      cs="$xaml.cs"; [ -f "$cs" ] || continue
      stubfile="$(mktemp)"; any=0
      for evt in $EVENTS; do
        while IFS= read -r hname; do
          [ -z "$hname" ] && continue
          grep -qE "void[[:space:]]+$hname[[:space:]]*\(" "$cs" && continue
          grep -qE "void $hname\(" "$stubfile" 2>/dev/null && continue
          at="$(argtype "$evt")"
          printf '\n        private void %s(object sender, %s e)\n        {\n        }\n' "$hname" "$at" >> "$stubfile"
          echo "  + $hname(object, $at)  [$evt] -> $(basename "$cs")"
          any=1; total=$((total+1))
        done < <(grep -oE "[[:space:]]$evt=\"[A-Za-z_][A-Za-z0-9_]*\"" "$xaml" 2>/dev/null | sed -E "s/.*$evt=\"([^\"]*)\".*/\1/")
      done
      if [ "$any" -eq 1 ]; then
        tmp="$(mktemp)"
        awk -v sf="$stubfile" '
          { print }
          (!ins && /partial[ \t]+class/) { want=1 }
          (!ins && want && /\{/) { while ((getline l < sf) > 0) print l; ins=1; want=0 }
        ' "$cs" > "$tmp" && mv "$tmp" "$cs"
      fi
      rm -f "$stubfile"
    done < <(find "$PROJ" -name '*.xaml' -not -path '*/bin/*' -not -path '*/obj/*' 2>/dev/null)
    [ "$total" -eq 0 ] && echo "Keine fehlenden Handler gefunden." || echo "$total Handler-Stub(s) ergaenzt."
    ;;
  server|client)
    PROJ="${1:-}"; [ -n "$PROJ" ] && shift || usage
    MSG="MSG"; PORT="12345"; HOST="localhost"; NAME=""
    while [ $# -gt 0 ]; do
      case "$1" in
        --msg)  MSG="${2:-MSG}"; shift 2 || shift ;;
        --port) PORT="${2:-12345}"; shift 2 || shift ;;
        --host) HOST="${2:-localhost}"; shift 2 || shift ;;
        --name) NAME="${2:-}"; shift 2 || shift ;;
        *) shift ;;
      esac
    done
    NS="$(get_ns "$PROJ")"
    [ -n "$NAME" ] || { [ "$sub" = server ] && NAME="ChatServer" || NAME="ChatClient"; }
    DEST="$PROJ/$NAME.cs"
    if [ -e "$DEST" ]; then echo "  ! $NAME.cs existiert bereits - uebersprungen"; exit 0; fi
    if [ "$sub" = server ]; then
      cat > "$DEST" <<'EOF'
using Network;            // Transfer<T>; falls Transfer im selben Projekt liegt: using entfernen
using System.Net;
using System.Net.Sockets;

namespace __NS__;

// Multi-Client-Server: nimmt MEHRERE Verbindungen an und sendet jede empfangene
// __MSG__-Nachricht an ALLE verbundenen Clients weiter (Broadcast).
internal class __NAME__
{
    private const int Port = __PORT__;
    private readonly List<Transfer<__MSG__>> _clients = new List<Transfer<__MSG__>>();
    private readonly object _lock = new object();

    internal void Start()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine($"Server laeuft auf Port {Port}. Warte auf Clients ...");

        while (true)
        {
            TcpClient tcp = listener.AcceptTcpClient();
            Transfer<__MSG__> transfer = new Transfer<__MSG__>(tcp);   // EIN Transfer pro Verbindung

            lock (_lock) { _clients.Add(transfer); }
            Console.WriteLine($"Client verbunden (insgesamt {_clients.Count}).");

            transfer.OnMessageReceived += (sender, msg) => Broadcast(msg);
            transfer.OnDisconnected += (sender, e) =>
            {
                lock (_lock) { _clients.Remove(transfer); }
                Console.WriteLine("Client getrennt.");
            };
        }
    }

    // schickt die Nachricht an ALLE verbundenen Clients
    private void Broadcast(__MSG__ msg)
    {
        lock (_lock)
        {
            foreach (Transfer<__MSG__> client in _clients)
                client.Send(msg);
        }
    }

    public static void Main()
    {
        new __NAME__().Start();
        Console.ReadLine();  // damit der Server nicht sofort endet
    }
}
EOF
    else
      cat > "$DEST" <<'EOF'
using Network;            // Transfer<T>; falls Transfer im selben Projekt liegt: using entfernen
using System.Net.Sockets;

namespace __NS__;

// Verbindet sich zum Server und tauscht __MSG__-Nachrichten ueber Port __PORT__ aus.
internal class __NAME__
{
    private readonly TcpClient _client;
    private readonly Transfer<__MSG__> _transfer;

    internal __NAME__(string host = "__HOST__", int port = __PORT__)
    {
        _client = new TcpClient();
        _client.Connect(host, port);
        _transfer = new Transfer<__MSG__>(_client);
        _transfer.OnMessageReceived += (sender, msg) => OnMessage(msg);
    }

    // Nachricht an den Server senden (der sie an alle Clients verteilt)
    internal void Send(__MSG__ msg) => _transfer.Send(msg);

    private void OnMessage(__MSG__ msg)
    {
        // ACHTUNG: laeuft im Empfangs-Thread! In WPF GUI-Zugriffe ueber Dispatcher.Invoke(...).
        Console.WriteLine($"Empfangen: {msg}");
    }
}
EOF
    fi
    sed -i.bak -e "s|__NS__|$NS|g" -e "s|__MSG__|$MSG|g" -e "s|__NAME__|$NAME|g" -e "s|__PORT__|$PORT|g" -e "s|__HOST__|$HOST|g" "$DEST" && rm -f "$DEST.bak"
    echo "  + $NAME.cs  (namespace $NS, Nachricht $MSG, Port $PORT)"
    echo "    braucht Transfer<T> (./add.sh \"$PROJ\" transfer  oder  new-pa --network) und die Klasse $MSG (./gen.sh model \"$PROJ\" $MSG ...)"
    ;;
  *) usage ;;
esac
