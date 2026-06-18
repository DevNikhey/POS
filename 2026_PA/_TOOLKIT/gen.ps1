# =====================================================================
#  gen.ps1 - Code-Generatoren (wird von gen.bat aufgerufen).
# =====================================================================
param([Parameter(ValueFromRemainingArguments = $true)][string[]]$All = @())

function Arg($i) { if ($i -lt $All.Count) { $All[$i] } else { "" } }

function Usage {
    Write-Host "Verwendung:"
    Write-Host "  Ausgabe zum Kopieren:"
    Write-Host "    gen.bat dp <Klasse> <Name> <Typ> [--render]"
    Write-Host "    gen.bat prop <Name> <Typ>"
    Write-Host "    gen.bat cmd <Name>"
    Write-Host "    gen.bat datatemplate <Typ> [feld...]"
    Write-Host "  Datei im Projekt anlegen:"
    Write-Host "    gen.bat window <projekt-ordner> <Name>"
    Write-Host "    gen.bat vm <projekt-ordner> <Name>"
    Write-Host "    gen.bat converter <projekt-ordner> <Name>"
    Write-Host "    gen.bat shape <projekt-ordner> <Name>"
    Write-Host "    gen.bat control <projekt-ordner> <Name>"
    Write-Host "    gen.bat model <projekt-ordner> <Klasse> [feld:typ...]"
    Write-Host "    gen.bat class <projekt-ordner> <Name>"
    Write-Host "    gen.bat enum <projekt-ordner> <Name> <Werte...>"
    Write-Host "    gen.bat grid <zeilen> <spalten>"
    Write-Host "    gen.bat handlers <projekt-ordner>"
    Write-Host "  Client/Server (Transfer<T>):"
    Write-Host "    gen.bat server <projekt-ordner> [--msg MSG] [--port 12345] [--name ChatServer]"
    Write-Host "    gen.bat client <projekt-ordner> [--msg MSG] [--port 12345] [--host localhost]"
    Write-Host "  Threading (PA1):"
    Write-Host "    gen.bat sync lock|semaphore|autoreset|monitor <Name> [--count N]"
    exit 1
}

function Get-NS($dir) {
    if (-not (Test-Path $dir)) { Write-Host "FEHLER: Ordner '$dir' nicht gefunden."; exit 1 }
    $cs = Get-ChildItem -Path $dir -Filter *.csproj -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $cs) { Write-Host "FEHLER: keine .csproj in '$dir'."; exit 1 }
    return [IO.Path]::GetFileNameWithoutExtension($cs.Name)
}

function WriteNew($path, $content) {
    if (Test-Path $path) { Write-Host ("  ! {0} existiert bereits - uebersprungen" -f [IO.Path]::GetFileName($path)); return }
    Set-Content -Path $path -Value $content -NoNewline
    Write-Host ("  + {0}" -f [IO.Path]::GetFileName($path))
}

# --- Interaktive Menüs (Pfeiltasten). Zeichnen via stderr -> stdout bleibt sauber. ---
function Menu-Single($title, $options) {
    [Console]::Error.WriteLine($title + "  (Pfeil hoch/runter, Enter)")
    $top = [Console]::CursorTop
    $cur = 0
    while ($true) {
        for ($i = 0; $i -lt $options.Count; $i++) {
            $pfx = if ($i -eq $cur) { "  > " } else { "    " }
            [Console]::Error.WriteLine($pfx + $options[$i] + "        ")
        }
        $k = [Console]::ReadKey($true)
        switch ($k.Key) {
            'UpArrow'   { $cur = ($cur - 1 + $options.Count) % $options.Count }
            'DownArrow' { $cur = ($cur + 1) % $options.Count }
            'Enter'     { return $cur }
        }
        [Console]::SetCursorPosition(0, $top)
    }
}

function Menu-Multi($title, $options) {
    [Console]::Error.WriteLine($title + "  (Pfeile, Leertaste=an/aus, Enter=fertig)")
    $top = [Console]::CursorTop
    $cur = 0
    $checked = New-Object 'bool[]' $options.Count
    while ($true) {
        for ($i = 0; $i -lt $options.Count; $i++) {
            $mark = if ($checked[$i]) { "[x]" } else { "[ ]" }
            $pfx = if ($i -eq $cur) { "  > " } else { "    " }
            [Console]::Error.WriteLine($pfx + $mark + " " + $options[$i] + "        ")
        }
        $k = [Console]::ReadKey($true)
        switch ($k.Key) {
            'UpArrow'   { $cur = ($cur - 1 + $options.Count) % $options.Count }
            'DownArrow' { $cur = ($cur + 1) % $options.Count }
            'Spacebar'  { $checked[$cur] = -not $checked[$cur] }
            'Enter'     {
                $sel = @()
                for ($j = 0; $j -lt $options.Count; $j++) { if ($checked[$j]) { $sel += $options[$j] } }
                return $sel
            }
        }
        [Console]::SetCursorPosition(0, $top)
    }
}

$Cmd = Arg 0
$A1 = Arg 1
$A2 = Arg 2
$A3 = Arg 3

switch ($Cmd) {
    "dp" {
        if (-not $A1 -or -not $A2 -or -not $A3) { Usage }
        if ($All -contains "--render") {
            $meta = "new FrameworkPropertyMetadata(default($A3),`n            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure)"
        } else {
            $meta = "new PropertyMetadata(default($A3))"
        }
        Write-Output @"
public static readonly DependencyProperty ${A2}Property =
    DependencyProperty.Register(nameof($A2), typeof($A3), typeof($A1),
        $meta);

public $A3 $A2
{
    get => ($A3)GetValue(${A2}Property);
    set => SetValue(${A2}Property, value);
}
"@
    }
    "prop" {
        if (-not $A1 -or -not $A2) { Usage }
        $field = "_" + $A1.Substring(0, 1).ToLower() + $A1.Substring(1)
        Write-Output @"
private $A2 $field;
public $A2 $A1
{
    get => $field;
    set => SetProperty(ref $field, value);
}
"@
    }
    "cmd" {
        if (-not $A1) { Usage }
        Write-Output @"
// using System.Windows.Input;   // fuer ICommand
public ICommand ${A1}Command { get; }

// Im Konstruktor setzen:
//   ${A1}Command = new RelayCommand(_ => On${A1}());

private void On$A1()
{
    // TODO
}
"@
    }
    "datatemplate" {
        # optionaler Typ als erstes Argument (sonst aus --from abgeleitet)
        $Typ = ""; $start = 1
        if ($All.Count -gt 1 -and -not $All[1].StartsWith("-")) { $Typ = $All[1]; $start = 2 }
        $rest = @(); if ($All.Count -gt $start) { $rest = $All[$start..($All.Count - 1)] }

        $from = ""; $orient = "Horizontal"; $labels = $false; $border = $false
        $click = ""; $grid = $false; $width = ""; $interactive = $false
        $images = @(); $checks = @(); $fields = @()
        for ($i = 0; $i -lt $rest.Count; $i++) {
            switch ($rest[$i]) {
                "--from"       { $from = $rest[$i + 1]; $i++ }
                { $_ -in @("--vertical", "-v") }     { $orient = "Vertical" }
                "--horizontal" { $orient = "Horizontal" }
                { $_ -in @("--grid", "-g") }         { $grid = $true }
                { $_ -in @("--labels", "-l") }       { $labels = $true }
                { $_ -in @("--border", "-b") }       { $border = $true }
                "--width"      { $width = $rest[$i + 1]; $i++ }
                "--image"      { $images += $rest[$i + 1]; $i++ }
                "--checkbox"   { $checks += $rest[$i + 1]; $i++ }
                "--click"      { $click = $rest[$i + 1]; $i++ }
                { $_ -in @("--interactive", "-i") }  { $interactive = $true }
                default        { $fields += $rest[$i] }
            }
        }
        if ($from) {
            if (-not (Test-Path $from)) { Write-Host "FEHLER: '$from' nicht gefunden."; exit 1 }
            $lines = Get-Content $from
            foreach ($line in $lines) {
                if ($line -match 'public[^;(]*\b([A-Za-z_][A-Za-z0-9_]*)\s*\{\s*get\s*[;=]') { $fields += $matches[1] }
            }
            if (-not $Typ) {
                foreach ($line in $lines) {
                    if ($line -match 'class\s+([A-Za-z_][A-Za-z0-9_]*)' -and $line -notmatch 'static|DataConnection|:\s*Window|:\s*Application') { $Typ = $matches[1]; break }
                }
            }
        }
        if (-not $Typ) { $Typ = "Item" }

        if ($interactive) {
            if ([Console]::IsInputRedirected) { Write-Host "FEHLER: interaktiver Modus (-i) braucht ein Terminal."; exit 1 }
            if ($fields.Count -gt 0) {
                $sel = @(Menu-Multi "Felder auswaehlen:" $fields)
                if ($sel.Count -gt 0) { $fields = $sel }
            }
            $li = Menu-Single "Layout:" @("nebeneinander (horizontal)", "untereinander (vertical)", "Tabelle (grid)")
            switch ($li) { 0 { $orient = "Horizontal"; $grid = $false } 1 { $orient = "Vertical"; $grid = $false } 2 { $grid = $true } }
            $opt = Menu-Multi "Optionen:" @("Labels (Feld:)", "Rahmen (Border)")
            $labels = $false; $border = $false
            foreach ($o in $opt) { if ($o -like "Labels*") { $labels = $true }; if ($o -like "Rahmen*") { $border = $true } }
            $click = Read-Host "Klick-Handler (leer = keiner)"
        }
        if ($fields.Count -eq 0) { $fields = @("Name") }
        if ($grid) { $labels = $true }

        function Render-Val($f, $extra) {
            if ($images -contains $f) { return "<Image$extra Source=`"{Binding $f}`" Width=`"64`" Height=`"64`" Stretch=`"Uniform`" Margin=`"5`"/>" }
            elseif ($checks -contains $f) { return "<CheckBox$extra IsChecked=`"{Binding $f}`" Margin=`"5`"/>" }
            else { $w = ""; if ($width) { $w = " Width=`"$width`"" }; return "<TextBlock$extra Text=`"{Binding $f}`"$w Margin=`"5`"/>" }
        }

        $inner = ""
        if ($grid) {
            $rowdefs = ""; $cells = ""; $idx = 0
            foreach ($f in $fields) {
                $rowdefs += "            <RowDefinition Height=`"Auto`"/>`n"
                $cells += "            <TextBlock Grid.Row=`"$idx`" Grid.Column=`"0`" Text=`"${f}:`" FontWeight=`"Bold`" Margin=`"5,2`"/>`n"
                $cells += "            " + (Render-Val $f " Grid.Row=`"$idx`" Grid.Column=`"1`"") + "`n"
                $idx++
            }
            $inner = "        <Grid.ColumnDefinitions>`n            <ColumnDefinition Width=`"Auto`"/>`n            <ColumnDefinition Width=`"*`"/>`n        </Grid.ColumnDefinitions>`n        <Grid.RowDefinitions>`n$rowdefs        </Grid.RowDefinitions>`n$cells"
        } else {
            foreach ($f in $fields) {
                if ($orient -eq "Vertical" -and $labels) {
                    $inner += "            <StackPanel Orientation=`"Horizontal`">`n                <TextBlock Text=`"${f}:`" Width=`"110`" FontWeight=`"Bold`"/>`n                " + (Render-Val $f "") + "`n            </StackPanel>`n"
                } elseif ($labels) {
                    $inner += "            <TextBlock Text=`"${f}:`" FontWeight=`"Bold`" Margin=`"5,0,2,0`"/>`n            " + (Render-Val $f "") + "`n"
                } else {
                    $inner += "            " + (Render-Val $f "") + "`n"
                }
            }
        }

        $panelClick = ""; $borderClick = ""
        if ($click) {
            if ($border) { $borderClick = " Background=`"Transparent`" MouseLeftButtonDown=`"$click`"" }
            else { $panelClick = " Background=`"Transparent`" MouseLeftButtonDown=`"$click`"" }
        }
        if ($grid) { $open = "<Grid$panelClick>"; $close = "</Grid>" }
        else { $open = "<StackPanel Orientation=`"$orient`"$panelClick>"; $close = "</StackPanel>" }

        if ($border) {
            Write-Output @"
<DataTemplate DataType="{x:Type local:$Typ}">
    <Border BorderBrush="Gray" BorderThickness="1" CornerRadius="3" Padding="4" Margin="2"$borderClick>
        $open
$inner        $close
    </Border>
</DataTemplate>
"@
        } else {
            Write-Output @"
<DataTemplate DataType="{x:Type local:$Typ}">
    $open
$inner    $close
</DataTemplate>
"@
        }
    }
    "window" {
        if (-not $A1 -or -not $A2) { Usage }
        $ns = Get-NS $A1
        WriteNew (Join-Path $A1 "$A2.xaml") @"
<Window x:Class="$ns.$A2"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="$A2" Height="450" Width="800">
    <Grid>

    </Grid>
</Window>
"@
        WriteNew (Join-Path $A1 "$A2.xaml.cs") @"
using System.Windows;

namespace $ns
{
    public partial class $A2 : Window
    {
        public $A2()
        {
            InitializeComponent();
        }
    }
}
"@
    }
    "vm" {
        if (-not $A1 -or -not $A2) { Usage }
        $ns = Get-NS $A1
        WriteNew (Join-Path $A1 "$A2.cs") @"
namespace $ns;

// Braucht ViewModelBase (z.B. via: add.bat "$A1" mvvm)
public class $A2 : ViewModelBase
{
    // private string _text = "";
    // public string Text { get => _text; set => SetProperty(ref _text, value); }
}
"@
    }
    "converter" {
        if (-not $A1 -or -not $A2) { Usage }
        $ns = Get-NS $A1
        WriteNew (Join-Path $A1 "$A2.cs") @"
using System;
using System.Globalization;
using System.Windows.Data;

namespace $ns;

public class $A2 : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
"@
    }
    "shape" {
        if (-not $A1 -or -not $A2) { Usage }
        $ns = Get-NS $A1
        WriteNew (Join-Path $A1 "$A2.cs") @"
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace $ns;

// Eigenes Zeichen-Control. Braucht ShapeBase (add.bat "$A1" shapes).
public class $A2 : ShapeBase
{
    // Eigene Properties z.B. via:  gen.bat dp $A2 Radius double --render

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
"@
    }
    "control" {
        if (-not $A1 -or -not $A2) { Usage }
        $ns = Get-NS $A1
        WriteNew (Join-Path $A1 "$A2.cs") @"
using System.Windows;
using System.Windows.Controls;

namespace $ns;

// Templated Control. Der Template-Style gehoert in Themes/Generic.xaml
// (Snippet unten). Meist in einer eigenen Library mit <UseWPF>.
public class $A2 : Control
{
    static $A2()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof($A2),
            new FrameworkPropertyMetadata(typeof($A2)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        // var part = GetTemplateChild("PART_X") as Button;
    }
}
"@
        Write-Host ""
        Write-Host "--> in Themes/Generic.xaml (innerhalb <ResourceDictionary>) einfuegen:"
        Write-Output @"
<Style TargetType="{x:Type local:$A2}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:$A2}">
                <Grid>
                    <!-- z.B. <Button x:Name="PART_X"/> -->
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
"@
    }
    "model" {
        if (-not $A1 -or -not $A2) { Usage }
        $ns = Get-NS $A1
        $fields = @(); if ($All.Count -gt 3) { $fields = $All[3..($All.Count - 1)] }
        $props = ""
        foreach ($spec in $fields) {
            $parts = $spec.Split(":", 2)
            $fname = $parts[0]
            $ftyp = if ($parts.Count -gt 1 -and $parts[1]) { $parts[1] } else { "string" }
            $props += "    public $ftyp $fname { get; set; }`n"
        }
        if (-not $props) { $props = "    // public string Name { get; set; }`n" }
        WriteNew (Join-Path $A1 "$A2.cs") @"
namespace $ns;

public class $A2
{
$props}
"@
    }
    "class" {
        if (-not $A1 -or -not $A2) { Usage }
        $ns = Get-NS $A1
        WriteNew (Join-Path $A1 "$A2.cs") @"
namespace $ns;

public class $A2
{
}
"@
    }
    "sync" {
        if (-not $A1 -or -not $A2) { Usage }
        $count = 1
        for ($i = 0; $i -lt $All.Count; $i++) { if ($All[$i] -eq '--count') { $count = $All[$i + 1] } }
        $low = $A2.Substring(0, 1).ToLower() + $A2.Substring(1)
        switch ($A1) {
            "lock" { Write-Output @"
// lock: IMMER auf ein privates readonly-Objekt sperren (nie this/typeof/string/public Feld!)
private readonly object ${low}Lock = new object();

public void $A2()
{
    lock (${low}Lock)
    {
        // kritischer Abschnitt
    }
}
"@ }
            "semaphore" { Write-Output @"
// SemaphoreSlim: erlaubt max. $count gleichzeitige Zugriffe (z.B. $count Landebahnen)
private readonly SemaphoreSlim ${low}Semaphore = new SemaphoreSlim($count, $count);

public void $A2()
{
    ${low}Semaphore.Wait();
    try
    {
        // kritischer Abschnitt (max. $count Threads gleichzeitig)
    }
    finally
    {
        ${low}Semaphore.Release();   // IMMER im finally freigeben!
    }
}
"@ }
            "autoreset" { Write-Output @"
// AutoResetEvent: weckt GENAU EINEN wartenden Thread. Start false = gesperrt.
private readonly AutoResetEvent ${low}Event = new AutoResetEvent(false);

// ... im wartenden Thread:
${low}Event.WaitOne();                 // blockiert, bis Set() kommt

// ... beim Freigeben (genau einen Wartenden wecken):
${low}Event.Set();
// Hinweis: fuer MEHRERE Wartende mehrmals Set() ODER SemaphoreSlim/ManualResetEvent verwenden.
"@ }
            "monitor" { Write-Output @"
private readonly object ${low}Lock = new object();

// Warten auf eine Bedingung -> IMMER in while(), NICHT if()!
lock (${low}Lock)
{
    while (/* !bedingung */ true)
        Monitor.Wait(${low}Lock);
    // Bedingung erfuellt -> weiter
}

// Zustand geaendert -> Wartende wecken:
lock (${low}Lock)
{
    // ... Zustand aendern ...
    Monitor.PulseAll(${low}Lock);
}
"@ }
            "prodcons" { Write-Output @"
// Producer/Consumer mit BlockingCollection (thread-safe Queue; Consumer blockiert, bis etwas da ist)
// using System.Collections.Concurrent;
private readonly BlockingCollection<$A2> ${low}Queue = new BlockingCollection<$A2>();

// Producer (z.B. neues $A2 einreihen):
${low}Queue.Add(item);

// Consumer (in eigenem Thread starten):
foreach ($A2 item in ${low}Queue.GetConsumingEnumerable())
{
    // item verarbeiten (blockiert automatisch, bis etwas da ist)
}
// Wenn nichts mehr produziert wird:
${low}Queue.CompleteAdding();
"@ }
            default { Write-Host "kind: lock | semaphore | autoreset | monitor | prodcons"; exit 1 }
        }
    }
    { $_ -in @("server", "client") } {
        if (-not $A1) { Usage }
        $proj = $A1
        $msg = "MSG"; $port = "12345"; $srvHost = "localhost"; $nm = ""
        for ($i = 2; $i -lt $All.Count; $i++) {
            switch ($All[$i]) {
                "--msg"  { $msg = $All[$i + 1]; $i++ }
                "--port" { $port = $All[$i + 1]; $i++ }
                "--host" { $srvHost = $All[$i + 1]; $i++ }
                "--name" { $nm = $All[$i + 1]; $i++ }
            }
        }
        $ns = Get-NS $proj
        if (-not $nm) { if ($Cmd -eq "server") { $nm = "ChatServer" } else { $nm = "ChatClient" } }
        $dest = Join-Path $proj "$nm.cs"
        if (Test-Path $dest) { Write-Host "  ! $nm.cs existiert bereits - uebersprungen"; return }
        if ($Cmd -eq "server") {
            $code = @'
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
'@
        } else {
            $code = @'
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
'@
        }
        $code = $code.Replace("__NS__", $ns).Replace("__MSG__", $msg).Replace("__NAME__", $nm).Replace("__PORT__", $port).Replace("__HOST__", $srvHost)
        Set-Content -Path $dest -Value $code
        Write-Host "  + $nm.cs  (namespace $ns, Nachricht $msg, Port $port)"
        Write-Host "    braucht Transfer<T> (add.bat `"$proj`" transfer) und die Klasse $msg (gen.bat model `"$proj`" $msg ...)"
    }
    "handlers" {
        $proj = $A1
        if (-not $proj -or -not (Test-Path $proj)) { Write-Host "Verwendung: gen.bat handlers <projekt-ordner>"; exit 1 }
        $argmap = @{
            'SelectionChanged' = 'SelectionChangedEventArgs'; 'TextChanged' = 'TextChangedEventArgs';
            'KeyDown' = 'KeyEventArgs'; 'KeyUp' = 'KeyEventArgs'; 'PreviewKeyDown' = 'KeyEventArgs'; 'PreviewKeyUp' = 'KeyEventArgs';
            'MouseDown' = 'MouseButtonEventArgs'; 'MouseUp' = 'MouseButtonEventArgs'; 'MouseLeftButtonDown' = 'MouseButtonEventArgs';
            'MouseLeftButtonUp' = 'MouseButtonEventArgs'; 'MouseRightButtonDown' = 'MouseButtonEventArgs'; 'PreviewMouseLeftButtonDown' = 'MouseButtonEventArgs';
            'MouseMove' = 'MouseEventArgs'; 'MouseEnter' = 'MouseEventArgs'; 'MouseLeave' = 'MouseEventArgs';
            'MouseWheel' = 'MouseWheelEventArgs'; 'Drop' = 'DragEventArgs'; 'DragEnter' = 'DragEventArgs'; 'DragOver' = 'DragEventArgs';
            'Closing' = 'System.ComponentModel.CancelEventArgs'
        }
        $events = @('Click', 'SelectionChanged', 'TextChanged', 'PreviewKeyDown', 'KeyDown', 'KeyUp', 'PreviewMouseLeftButtonDown', 'MouseLeftButtonDown', 'MouseLeftButtonUp', 'MouseRightButtonDown', 'MouseDown', 'MouseUp', 'MouseMove', 'MouseEnter', 'MouseLeave', 'MouseWheel', 'Loaded', 'Unloaded', 'Checked', 'Unchecked', 'Drop', 'DragEnter', 'DragOver', 'Closing')
        $total = 0
        $xamls = Get-ChildItem -Path $proj -Recurse -Filter *.xaml -File -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch '\\(bin|obj|\.vs)\\' }
        foreach ($x in $xamls) {
            $cs = "$($x.FullName).cs"
            if (-not (Test-Path $cs)) { continue }
            $xc = Get-Content -Raw $x.FullName
            $csc = Get-Content -Raw $cs
            $stubs = ""
            foreach ($evt in $events) {
                foreach ($m in [regex]::Matches($xc, "\s$evt=`"([A-Za-z_][A-Za-z0-9_]*)`"")) {
                    $hname = $m.Groups[1].Value
                    if ($csc -match ("void\s+" + [regex]::Escape($hname) + "\s*\(")) { continue }
                    if ($stubs -match ("void " + [regex]::Escape($hname) + "\(")) { continue }
                    $at = if ($argmap.ContainsKey($evt)) { $argmap[$evt] } else { 'RoutedEventArgs' }
                    $stubs += "`n        private void $hname(object sender, $at e)`n        {`n        }`n"
                    Write-Host "  + $hname(object, $at)  [$evt] -> $($x.Name).cs"
                    $total++
                }
            }
            if ($stubs) {
                $idx = $csc.IndexOf('partial class')
                if ($idx -ge 0) {
                    $brace = $csc.IndexOf('{', $idx)
                    if ($brace -ge 0) {
                        $new = $csc.Substring(0, $brace + 1) + $stubs + $csc.Substring($brace + 1)
                        Set-Content -Path $cs -Value $new -NoNewline
                    }
                }
            }
        }
        if ($total -eq 0) { Write-Host "Keine fehlenden Handler gefunden." } else { Write-Host "$total Handler-Stub(s) ergaenzt." }
    }
    "grid" {
        $rows = if ($A1) { [int]$A1 } else { 2 }
        $cols = if ($A2) { [int]$A2 } else { 2 }
        $sb = "<Grid>`n    <Grid.RowDefinitions>`n"
        for ($r = 0; $r -lt $rows; $r++) { $sb += "        <RowDefinition Height=`"Auto`"/>`n" }
        $sb += "    </Grid.RowDefinitions>`n    <Grid.ColumnDefinitions>`n"
        for ($c = 0; $c -lt $cols; $c++) { $sb += "        <ColumnDefinition Width=`"*`"/>`n" }
        $sb += "    </Grid.ColumnDefinitions>`n`n    <!-- <TextBlock Grid.Row=`"0`" Grid.Column=`"0`" Text=`"...`"/> -->`n</Grid>"
        Write-Output $sb
    }
    "enum" {
        if (-not $A1 -or -not $A2) { Usage }
        $ns = Get-NS $A1
        $vals = @(); if ($All.Count -gt 3) { $vals = $All[3..($All.Count - 1)] }
        if ($vals.Count -eq 0) { $vals = @("A", "B", "C") }
        $body = ""; foreach ($v in $vals) { $body += "    $v,`n" }
        WriteNew (Join-Path $A1 "$A2.cs") @"
namespace $ns;

public enum $A2
{
$body}
"@
    }
    default { Usage }
}
