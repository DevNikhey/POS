using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PenteNetwork
{
    public enum MessageType
    {
        Hello,
        GameStart,
        PlaceStone,
        StonePlaced,
        Capture,
        GameOver,
        Chat,
        Error,
        ScoreboardRequest,
        ScoreboardResponse
    }

    [XmlRoot("Message")]
    public class NetMessage
    {
        [XmlElement("Type")]
        public MessageType Type { get; set; }

        [XmlElement("Data")]
        public string? Data { get; set; }

        public T? GetData<T>() where T : class
        {
            if (Data == null) return default;
            return XmlHelper.Deserialize<T>(Data);
        }

        public static NetMessage Create<T>(MessageType type, T data) where T : class
        {
            return new NetMessage
            {
                Type = type,
                Data = XmlHelper.Serialize(data)
            };
        }

        public static NetMessage Create(MessageType type)
        {
            return new NetMessage { Type = type };
        }
    }

    [XmlRoot("PlaceStone")]
    public class PlaceStoneData
    {
        [XmlElement("Row")]
        public int Row { get; set; }
        [XmlElement("Col")]
        public int Col { get; set; }
    }

    [XmlRoot("StonePlaced")]
    public class StonePlacedData
    {
        [XmlElement("Row")]
        public int Row { get; set; }
        [XmlElement("Col")]
        public int Col { get; set; }
        [XmlElement("Player")]
        public int Player { get; set; }
    }

    [XmlRoot("Capture")]
    public class CaptureData
    {
        [XmlElement("Row1")]
        public int Row1 { get; set; }
        [XmlElement("Col1")]
        public int Col1 { get; set; }
        [XmlElement("Row2")]
        public int Row2 { get; set; }
        [XmlElement("Col2")]
        public int Col2 { get; set; }
        [XmlElement("CapturedBy")]
        public int CapturedBy { get; set; }
    }

    [XmlRoot("GameOver")]
    public class GameOverData
    {
        [XmlElement("Winner")]
        public int Winner { get; set; }
        [XmlElement("Reason")]
        public string Reason { get; set; } = "";
    }

    [XmlRoot("Hello")]
    public class HelloData
    {
        [XmlElement("PlayerName")]
        public string PlayerName { get; set; } = "";
    }

    [XmlRoot("GameStart")]
    public class GameStartData
    {
        [XmlElement("YourColor")]
        public int YourColor { get; set; }
        [XmlElement("OpponentName")]
        public string OpponentName { get; set; } = "";
    }

    [XmlRoot("ScoreEntry")]
    public class ScoreEntry
    {
        [XmlElement("PlayerName")]
        public string PlayerName { get; set; } = "";
        [XmlElement("Wins")]
        public int Wins { get; set; }
        [XmlElement("Losses")]
        public int Losses { get; set; }
        [XmlElement("TotalCaptures")]
        public int TotalCaptures { get; set; }
    }

    [XmlRoot("Scoreboard")]
    public class ScoreboardData
    {
        [XmlArray("Scores")]
        [XmlArrayItem("ScoreEntry")]
        public List<ScoreEntry> Scores { get; set; } = new List<ScoreEntry>();
    }

    [XmlRoot("Chat")]
    public class ChatData
    {
        [XmlElement("PlayerName")]
        public string PlayerName { get; set; } = "";
        [XmlElement("Message")]
        public string Message { get; set; } = "";
    }

    [XmlRoot("Error")]
    public class ErrorData
    {
        [XmlElement("Message")]
        public string Message { get; set; } = "";
    }
}
