using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Gomoku.Network
{
    public class NetworkClient
    {
        private TcpClient _client;
        private NetworkStream _stream;

        private const string DELIMITER = "<END>";

        public Action<NetworkMessage>? OnMessageReceived;

        public async Task Connect(string ip)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, 5000);

            _stream = _client.GetStream();

            _ = ReceiveLoop();
        }

        public void Send(NetworkMessage message)
        {
            var serializer = new XmlSerializer(typeof(NetworkMessage));

            using var sw = new StringWriter();
            serializer.Serialize(sw, message);

            string xml = sw.ToString() + DELIMITER;

            var data = Encoding.UTF8.GetBytes(xml);
            _stream.Write(data, 0, data.Length);
        }

        private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[1024];
            string bufferString = "";

            while (true)
            {
                int bytes = await _stream.ReadAsync(buffer, 0, buffer.Length);
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
                }
            }
        }
    }
}
