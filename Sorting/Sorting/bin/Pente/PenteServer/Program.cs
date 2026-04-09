using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using PenteNetwork;

namespace PenteServer
{
    class Program
    {
        private static readonly Database _db = new Database();

        static async Task Main(string[] args)
        {
            _db.EnsureCreated();
            Console.WriteLine("Pente Server starting...");

            int port = 5050;
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Listening on port {port}. Waiting for players...");

            while (true)
            {
                try
                {
                    // Wait for first player
                    TcpClient client1 = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("Player 1 connected, waiting for Hello...");

                    string? name1 = await WaitForHello(client1);
                    if (name1 == null)
                    {
                        Console.WriteLine("Player 1 failed to send Hello. Disconnecting.");
                        client1.Close();
                        continue;
                    }
                    Console.WriteLine($"Player 1: {name1}. Waiting for opponent...");

                    // Wait for second player
                    TcpClient client2 = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("Player 2 connected, waiting for Hello...");

                    string? name2 = await WaitForHello(client2);
                    if (name2 == null)
                    {
                        Console.WriteLine("Player 2 failed to send Hello. Disconnecting.");
                        client2.Close();
                        continue;
                    }
                    Console.WriteLine($"Player 2: {name2}. Starting game...");

                    // Start game session in background
                    GameSession session = new GameSession(client1, name1, client2, name2, _db);
                    _ = Task.Run(() => session.RunAsync());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting clients: {ex.Message}");
                }
            }
        }

        private static async Task<string?> WaitForHello(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                NetMessage? msg = await NetHelper.ReadMessage(stream);
                if (msg == null || msg.Type != MessageType.Hello)
                    return null;

                HelloData? hello = msg.GetData<HelloData>();
                return hello?.PlayerName;
            }
            catch
            {
                return null;
            }
        }
    }
}
