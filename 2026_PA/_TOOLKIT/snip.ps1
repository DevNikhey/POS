# =====================================================================
#  snip.ps1 - Schnipsel-Bibliothek (von snip.bat aufgerufen). Druckt + kopiert (Set-Clipboard).
# =====================================================================
param([Parameter(ValueFromRemainingArguments = $true)][string[]]$P = @())

$snips = [ordered]@{
    dispatcher = @'
// GUI-Zugriff aus einem anderen Thread (Empfangs-/Worker-Thread) -> Dispatcher!
Application.Current.Dispatcher.Invoke(() =>
{
    // hier GUI-Elemente anfassen
});
'@
    loaded = @'
// ActualWidth/ActualHeight sind im Konstruktor 0 - erst im Loaded gesetzt:
this.Loaded += (s, e) =>
{
    // groessenabhaengiges Zeichnen / Markierungen hier
};
'@
    itemssource = @'
liste.ItemsSource = items;   // NICHT liste.Items.Add(items)
'@
    selectionchanged = @'
private void box_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (box.SelectedItem is var sel && sel != null)
    {
        // ausgewaehltes Element verarbeiten
    }
}
'@
    usedb = @'
var options = new DataOptions().UseSQLite("Data Source=meine.db");
using var db = new MeineDB(options);
var liste = db.Tabelle.Where(x => x.Name.Contains(suche)).ToList();
'@
    insertid = @'
long neueId = db.InsertWithInt64Identity(entity);   // Insert + neue Id zurueck
'@
    arc = @'
// Tortenstueck/Bogen: Start am Mittelpunkt -> Linie zum Rand -> ArcSegment (IsClosed schliesst)
var fig = new PathFigure { StartPoint = center, IsClosed = true };
fig.Segments.Add(new LineSegment(p0, true));
fig.Segments.Add(new ArcSegment(p1, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, true));
'@
    xmlroot = @'
// Objekt <-> XML (passt zum Transfer<T>)
var ser = new XmlSerializer(typeof(MSG));
using var sw = new StringWriter(); ser.Serialize(sw, msg); string xml = sw.ToString();
using var sr = new StringReader(xml); MSG back = (MSG)ser.Deserialize(sr);
'@
    elseguard = @'
// Optionales Keyword nur konsumieren, wenn der WERT passt (nicht nur der Typ):
if (tokens.Count > 0 && tokens[0].Type == Token.TokenType.KEYWORD && tokens[0].Value == "ELSE")
{
    tokens.RemoveAt(0);
    elseBlock.Parse(tokens);
}
'@
    overridemeta = @'
// Templated Control: im statischen Konstruktor den Default-Style-Key setzen
static MeinControl()
{
    DefaultStyleKeyProperty.OverrideMetadata(typeof(MeinControl),
        new FrameworkPropertyMetadata(typeof(MeinControl)));
}
'@
    semaphore = @'
sem.Wait();
try
{
    // kritischer Abschnitt
}
finally
{
    sem.Release();   // IMMER im finally!
}
'@
    datatemplate = @'
<ListBox.ItemTemplate>
    <DataTemplate>
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="{Binding Name}" Margin="5"/>
        </StackPanel>
    </DataTemplate>
</ListBox.ItemTemplate>
'@
}

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

function Show($name) {
    if (-not $snips.Contains($name)) { Write-Host "Unbekannt: $name"; Write-Host ("Verfuegbar: " + ($snips.Keys -join ' ')); exit 1 }
    $c = $snips[$name]
    Write-Output $c
    try { Set-Clipboard -Value $c; [Console]::Error.WriteLine("(in Zwischenablage kopiert)") } catch {}
}

$arg = if ($P.Count -ge 1) { $P[0] } else { "" }
if ($arg -eq '-i' -or $arg -eq '--interactive') {
    if ([Console]::IsInputRedirected) { Write-Host "FEHLER: -i braucht ein Terminal."; exit 1 }
    $names = @($snips.Keys)
    $idx = Menu-Single "Snippet waehlen:" $names
    Show $names[$idx]
} elseif (-not $arg) {
    Write-Host ("Snippets: " + ($snips.Keys -join ' '))
    Write-Host "Nutzung: snip.bat <name>   oder   snip.bat -i"
} else {
    Show $arg
}
