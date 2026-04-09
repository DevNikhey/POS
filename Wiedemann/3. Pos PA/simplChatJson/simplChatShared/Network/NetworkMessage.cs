using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace simplChatShared.Network
{
    [XmlRoot("NetworkMessage")]
    public class NetworkMessage
    {
        public string Type { get; set; } = "";
        public string Payload { get; set; } = "";
    }
}
