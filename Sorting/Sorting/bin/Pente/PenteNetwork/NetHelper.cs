using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PenteNetwork
{
    /// <summary>
    /// Helper class for sending/receiving XML messages over TCP.
    /// Protocol: each message is a single line of XML (no newlines inside), terminated by \n.
    /// The outer NetMessage envelope is XML, and the inner Data field is also XML (serialized payload).
    /// </summary>
    public static class NetHelper
    {
        public static async Task SendMessage(NetworkStream stream, NetMessage msg)
        {
            string xml = XmlHelper.Serialize(msg);
            byte[] data = Encoding.UTF8.GetBytes(xml + "\n");
            await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        public static async Task<NetMessage?> ReadMessage(NetworkStream stream)
        {
            StringBuilder sb = new StringBuilder();
            byte[] buffer = new byte[1];
            while (true)
            {
                int bytesRead;
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, 1).ConfigureAwait(false);
                }
                catch (IOException)
                {
                    return null;
                }
                catch (ObjectDisposedException)
                {
                    return null;
                }

                if (bytesRead == 0)
                    return null;

                char c = (char)buffer[0];
                if (c == '\n')
                    break;

                sb.Append(c);
            }

            string line = sb.ToString().Trim();
            if (string.IsNullOrEmpty(line))
                return null;

            try
            {
                return XmlHelper.Deserialize<NetMessage>(line);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// XML serialization helper. Produces compact single-line XML (no namespace declarations, no XML declaration).
    /// </summary>
    public static class XmlHelper
    {
        public static string Serialize<T>(T obj) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", ""); // suppress xmlns attributes

            using (StringWriter writer = new StringWriter())
            {
                using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(writer,
                    new System.Xml.XmlWriterSettings
                    {
                        OmitXmlDeclaration = true,
                        Indent = false // single line
                    }))
                {
                    serializer.Serialize(xmlWriter, obj, ns);
                }
                return writer.ToString();
            }
        }

        public static T? Deserialize<T>(string xml) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                return serializer.Deserialize(reader) as T;
            }
        }
    }
}
