using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

namespace Network;

/// <summary>
/// Generische, bidirektionale Kommunikations-Hülle über eine TCP-Verbindung.
///
/// Protokoll: vor jeder Nachricht werden 4 Bytes (int, little-endian) mit der
/// Länge des nachfolgenden XML gesendet ("length prefix"). Dadurch weiß der
/// Empfänger immer exakt, wie viele Bytes zu einer Nachricht gehören.
///
/// Pro TCP-Verbindung wird GENAU EIN Transfer-Objekt angelegt. Es besitzt einen
/// eigenen Empfangs-Thread und meldet vollständige Nachrichten über das Event
/// <see cref="OnMessageReceived"/>. Eine eigene/zweite Empfangsschleife darf es
/// daher nicht geben.
/// </summary>
public class Transfer<T>
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly XmlSerializer _serializer = new XmlSerializer(typeof(T));

    // Wird ausgelöst, sobald eine vollständige Nachricht empfangen und
    // deserialisiert wurde.
    public EventHandler<T>? OnMessageReceived;

    // Wird ausgelöst, wenn die Gegenstelle die Verbindung schließt.
    public EventHandler? OnDisconnected;

    public Transfer(TcpClient client)
    {
        _client = client;
        _stream = _client.GetStream();

        // Empfang läuft im Hintergrund, damit der aufrufende Thread (z. B. die
        // GUI) nicht blockiert wird.
        ThreadPool.QueueUserWorkItem(_ => Receive());
    }

    /// <summary>
    /// Serialisiert <paramref name="data"/> nach XML und sendet zuerst die Länge
    /// (4 Byte) und danach die XML-Bytes.
    /// </summary>
    public void Send(T data)
    {
        // 1) Objekt -> XML-String
        StringWriter stringWriter = new StringWriter();
        _serializer.Serialize(stringWriter, data);

        // 2) XML-String -> Bytes
        byte[] payload = Encoding.UTF8.GetBytes(stringWriter.ToString());

        // 3) Länge als 4-Byte-Präfix
        byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);

        // Senden muss atomar erfolgen, damit sich bei parallelen Send-Aufrufen
        // nicht Länge und Nutzdaten verschiedener Nachrichten vermischen.
        lock (_stream)
        {
            _stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            _stream.Write(payload, 0, payload.Length);
            _stream.Flush();
        }
    }

    /// <summary>
    /// Endlos-Schleife im Hintergrund-Thread: liest Länge + XML und meldet jede
    /// vollständige Nachricht über <see cref="OnMessageReceived"/>.
    /// </summary>
    private void Receive()
    {
        try
        {
            while (true)
            {
                // 1) 4-Byte-Länge lesen
                byte[] lengthBuffer = new byte[4];
                ReadExactly(lengthBuffer, 4);
                int length = BitConverter.ToInt32(lengthBuffer, 0);

                // 2) Genau 'length' Bytes Nutzdaten lesen
                byte[] dataBuffer = new byte[length];
                ReadExactly(dataBuffer, length);

                // 3) Bytes -> XML-String -> Objekt
                string xml = Encoding.UTF8.GetString(dataBuffer);
                using StringReader stringReader = new StringReader(xml);
                T dataObject = (T)_serializer.Deserialize(stringReader)!;

                OnMessageReceived?.Invoke(this, dataObject);
            }
        }
        catch (Exception)
        {
            // Verbindung beendet oder Lesefehler -> Gegenstelle ist weg.
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Liest so lange aus dem Stream, bis genau <paramref name="count"/> Bytes im
    /// Puffer liegen. Ein einzelner Read kann weniger liefern (TCP ist ein
    /// Byte-Strom ohne Nachrichtengrenzen), deshalb wird in einer Schleife
    /// nachgelesen.
    /// </summary>
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
