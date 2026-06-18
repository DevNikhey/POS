# Loesungshinweise - UE5_Parkhaus

Alle vier Parkhaus-Klassen und die Aufgabe-5-Antworten sind vollstaendig implementiert; keine TODOs/NotImplementedException mehr. Signaturen, Feld-/Klassennamen und die vorgegebene Parken(...)-Methode bleiben unveraendert.

Kernpunkte:
- EinfachParkhaus: SemaphoreSlim(KAPAZITAET, KAPAZITAET); Wait() vor try, Release() im finally; Status "Wartet".
- TagNachtParkhaus: ManualResetEventSlim(true) als Tor; Hintergrund-Thread (IsBackground=true) schaltet OFFEN/GESCHLOSSEN je 5000 ms und aktualisiert MainWindow.statusLabel ueber den Dispatcher. Reihenfolge bewusst: erst _tor.Wait(), DANN _plaetze.Wait() - so blockiert kein wartendes Auto einen Stellplatz, solange das Tor zu ist. Release() im finally.
- LadeParkhaus: privates readonly object als Lock + SemaphoreSlim(6) + Zaehler _freieLadeplaetze=2. Elektroautos warten mit "while (_freieLadeplaetze==0) Monitor.Wait(_lock)" innerhalb lock(_lock); beim Verlassen _freieLadeplaetze++ und Monitor.PulseAll. Bool-Flag ladeplatzBelegt stellt sicher, dass nur ein tatsaechlich belegter Ladeplatz zurueckgegeben wird; SemaphoreSlim.Release() immer im finally. Platznummer wird innerhalb des Locks gesetzt.
- KassaParkhaus: Producer/Consumer mit Queue<Auto>, privatem readonly Lock und genau einem Kassa-Thread (new Thread(KassaSchleife) - keine zweite Lambda-Schicht). Consumer wartet mit "while (_kassaQueue.Count==0) Monitor.Wait(_lock)" (kein Busy-Waiting), Producer Enqueue + Monitor.Pulse. Kassieren laeuft ausserhalb des Locks, GUI (kassaBox) nur ueber Dispatcher.
- Aufgabe 5: drei Fragen als Kommentar am Ende von Parkhaus.cs beantwortet (Release im finally, while statt if wegen spurious wakeups/Wettlauf um den frei gewordenen Platz, Race Condition beim nicht-atomaren Dekrement von _freieLadeplaetze inkl. SynchronizationLockException-Hinweis).

Bauen/Starten nur unter Windows (dotnet new wpf, Vorgabe-Dateien ins UE5_Parkhaus/-Projekt kopieren). Auf macOS nicht kompilierbar wegen WPF.
