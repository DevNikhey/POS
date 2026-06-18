# Loesungshinweise - UE1_Chat

Vollstaendige Loesung, alle TODOs/NotImplementedException entfernt. Geprueft via grep: keine TODOs mehr.

Was implementiert wurde:
- Server/Program.cs (Aufgabe 1-3): HandleClient legt GENAU EINEN Transfer<MSG> pro Verbindung an, abonniert OnMessageReceived (per Lambda, das den transfer kennt) und OnDisconnected (entfernt aus _clients im lock). Transfer wird im lock zur _clients-Liste hinzugefuegt. HandleJoin gibt Konsolen-Ausgabe und schickt NUR dem neuen Client den aktuellen Stand (BuildResult). HandleVote validiert die Option via Array.IndexOf, korrigiert bei Stimm-Aenderung (alte Option -1, neue +1), aktualisiert _lastVote, alles im lock(_sync); danach Broadcast(BuildResult()). BuildResult liest _counts im lock und baut List<VoteCount>. Broadcast zieht eine Kopie der Liste unter lock und sendet ausserhalb des locks an ALLE (auch Absender).
- UE1_Chat/MainWindow.xaml.cs (Aufgabe 4-6): Connect baut TcpClient zu localhost:12345, legt GENAU EINEN Transfer an, abonniert OnMessageReceived + OnDisconnected, sendet JOIN. Vote sendet VOTE mit UserName+Option (Guard: _transfer==null -> nichts tun). OnMessageReceived kapselt JEDEN UI-Zugriff in Dispatcher.Invoke und setzt resultListBox.ItemsSource = msg.Tally (passt zum DataTemplate im XAML, das Option/Count bindet). Bonus Aufgabe 6: Abstimm-Buttons im Konstruktor deaktiviert, erst nach Connect aktiv; OnDisconnected setzt Statuszeile und deaktiviert Buttons wieder (via Dispatcher).

Unveraendert (keine TODOs darin, aber relpath-stabil zurueckgegeben): Network/MSG.cs und UE1_Chat/MainWindow.xaml.

Fallstricke beachtet: ein Transfer pro Verbindung, keine zweite Empfangsschleife, Broadcast an ALLE, _clients/_counts/_lastVote mit lock(_sync), Broadcast iteriert ueber Kopie, Client-UI nur via Dispatcher.Invoke, MSG bleibt XML-serialisierbar (List<VoteCount> statt Dictionary). Transfer.cs wurde NICHT angefasst. Build des WPF-Teils nur unter Windows moeglich.
