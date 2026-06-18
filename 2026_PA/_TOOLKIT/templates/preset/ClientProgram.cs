using Network;
using Client;

// Konsolen-Client: Name eingeben, dann Nachrichten tippen. Der Server verteilt sie an alle.
Console.Write("Dein Name: ");
string user = Console.ReadLine() ?? "Anon";

ChatClient client = new ChatClient();   // verbindet zu localhost:12345 (siehe ChatClient.cs)

Console.WriteLine("Nachricht eingeben (leere Zeile = Ende):");
while (true)
{
    string? line = Console.ReadLine();
    if (string.IsNullOrEmpty(line)) break;
    client.Send(new MSG { User = user, Text = line });
}
