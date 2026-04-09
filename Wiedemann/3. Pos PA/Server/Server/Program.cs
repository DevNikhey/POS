// See https://aka.ms/new-console-template for more information
using NetworkLibrary;
using Server;
using System.Net.Sockets;
using System.Xml.Serialization;

Console.WriteLine("Hello, Server!");
TcpListener server = new TcpListener(System.Net.IPAddress.Any, 12345);
server.Start();

TcpClient client = server.AcceptTcpClient();
Console.WriteLine("Client connected! " + client.Client.RemoteEndPoint);

Transfer<Message> transfer = new Transfer<Message>(client);
transfer.OnMessageReceived += (sender, e) =>
{
    //Transfer t = (Transfer)sender;
    Console.WriteLine("Message received: " + transfer);
};

/*
byte[] buffer = new byte[1024];
NetworkStream stream = client.GetStream();
int bytesRead = stream.Read(buffer, 0, buffer.Length);
string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

Console.WriteLine("Received message: " + message);
*/

StreamReader reader = new StreamReader(client.GetStream());
StreamWriter writer = new StreamWriter(client.GetStream());

XmlSerializer serializer = new XmlSerializer(typeof(Message));

String s = reader.ReadLine();
String line = s;
while (!line.Contains("</Message>"))
{
    line  = reader.ReadLine();
    s += line;
}
Console.WriteLine(s);
Message message = (Message) serializer.Deserialize(new StringReader(s));
Console.WriteLine("Recieved Message: " + message.TheMessage);

Console.ReadLine();
