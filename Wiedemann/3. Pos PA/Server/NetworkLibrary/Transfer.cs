using Client;
using System.Net.Sockets;
using System.Xml.Serialization;

namespace NetworkLibrary
{
    public class Transfer<T>
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        private EventHandler<T> _onMessageReceived;

        public EventHandler OnDisconnnected;
       

        public event EventHandler<T> OnMessageReceived
        {
            add { _onMessageReceived += value; }

            remove { _onMessageReceived -= value; }
        }

        public Transfer(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
            _reader = new StreamReader(_stream);
            _writer = new StreamWriter(_stream) { AutoFlush = true};
            ThreadPool.QueueUserWorkItem(o => Receive());
        }

        public void Send(T message)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringWriter strngWriter = new StringWriter();
            serializer.Serialize(strngWriter, message);
            _writer.WriteLine(strngWriter.ToString());

        }

        private void Receive()
        {
            string text = "";
            while(true)
            {
                try
                {
                    string data = _reader.ReadLine();
                    text += data;
                    if(data.Contains("</" + typeof(T).Name + ">")) 
                    {

                        StringReader stringReader = new StringReader(text);
                        T message = (T)serializer.Deserialize(stringReader);
                        text = "";
                        _onMessageReceived?.Invoke(this, message);
                    }
                }
                catch (Exception ex)
                {
                    OnDisconnnected?.Invoke(this, EventArgs.Empty);
                    break;
                }
        }
    }
}
