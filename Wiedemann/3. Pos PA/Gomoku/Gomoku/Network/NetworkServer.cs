using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Gomoku.Network
{
    public class NetworkServer
    {
        private TcpListener _listener;
        private List<TcpClient> _clients = new();

        private const string DELIMITER = "<END>";

        public Action<NetworkMessage>? OnMessageReceived;

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, 5000);
            _listener.Start();
            Task.Run(AcceptClients);
        }
        private async Task AcceptClients()
        {
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _clients.Add(client);

                _ = HandleClient(client);
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            var stream = client.GetStream();
            byte[] buffer = new byte[1024];
            string bufferString = "";

            while (true)
            {
                int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                bufferString += Encoding.UTF8.GetString(buffer, 0, bytes);

                while (bufferString.Contains(DELIMITER))
                {
                    int index = bufferString.IndexOf(DELIMITER);
                    string xml = bufferString.Substring(0, index);

                    bufferString = bufferString.Substring(index + DELIMITER.Length);

                    var serializer = new XmlSerializer(typeof(NetworkMessage));

                    using var sr = new StringReader(xml);
                    var msg = (NetworkMessage)serializer.Deserialize(sr)!;

                    OnMessageReceived?.Invoke(msg);

                    Broadcast(xml + DELIMITER);
                }
            }
        }

        private void Broadcast(string xml)
        {
            var data = Encoding.UTF8.GetBytes(xml);

            foreach (var c in _clients)
            {
                c.GetStream().Write(data, 0, data.Length);
            }
        }
    }
}
