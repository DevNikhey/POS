using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

namespace __NS__;

// =====================================================================
//  Transfer<T> – wiederverwendbare TCP-Kommunikation (Length-Prefix + XML)
//  Aus PA3 (Client/Server). Pro Verbindung GENAU EIN Transfer-Objekt.
//  Senden:    [4 Byte Länge][XML-Bytes]
//  Empfangen: eigener Hintergrund-Thread -> Event OnMessageReceived
//  -> KEINE zweite eigene Empfangsschleife verwenden!
// =====================================================================
public class Transfer<T>
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly XmlSerializer _serializer = new XmlSerializer(typeof(T));

    public EventHandler<T>? OnMessageReceived;
    public EventHandler? OnDisconnected;

    public Transfer(TcpClient client)
    {
        _client = client;
        _stream = _client.GetStream();
        ThreadPool.QueueUserWorkItem(_ => Receive());
    }

    public void Send(T data)
    {
        StringWriter stringWriter = new StringWriter();
        _serializer.Serialize(stringWriter, data);
        byte[] payload = Encoding.UTF8.GetBytes(stringWriter.ToString());
        byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);

        lock (_stream)
        {
            _stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            _stream.Write(payload, 0, payload.Length);
            _stream.Flush();
        }
    }

    private void Receive()
    {
        try
        {
            while (true)
            {
                byte[] lengthBuffer = new byte[4];
                ReadExactly(lengthBuffer, 4);
                int length = BitConverter.ToInt32(lengthBuffer, 0);

                byte[] dataBuffer = new byte[length];
                ReadExactly(dataBuffer, length);

                string xml = Encoding.UTF8.GetString(dataBuffer);
                using StringReader stringReader = new StringReader(xml);
                T dataObject = (T)_serializer.Deserialize(stringReader)!;

                OnMessageReceived?.Invoke(this, dataObject);
            }
        }
        catch (Exception)
        {
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ReadExactly(byte[] buffer, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = _stream.Read(buffer, totalRead, count - totalRead);
            if (read == 0)
                throw new IOException("Verbindung wurde von der Gegenstelle geschlossen.");
            totalRead += read;
        }
    }
}
