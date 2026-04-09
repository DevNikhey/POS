using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using PenteNetwork;

namespace PenteServer
{
    public class GameSession
    {
        private readonly TcpClient _client1;
        private readonly TcpClient _client2;
        private readonly string _name1;
        private readonly string _name2;
        private readonly Database _db;
        private readonly PenteLogic _logic;
        private NetworkStream _stream1;
        private NetworkStream _stream2;

        public GameSession(TcpClient client1, string name1, TcpClient client2, string name2, Database db)
        {
            _client1 = client1;
            _client2 = client2;
            _name1 = name1;
            _name2 = name2;
            _db = db;
            _logic = new PenteLogic();
            _stream1 = client1.GetStream();
            _stream2 = client2.GetStream();
        }

        public async Task RunAsync()
        {
            try
            {
                // Player 1 = White, Player 2 = Black
                await NetHelper.SendMessage(_stream1, NetMessage.Create(MessageType.GameStart,
                    new GameStartData { YourColor = 1, OpponentName = _name2 }));
                await NetHelper.SendMessage(_stream2, NetMessage.Create(MessageType.GameStart,
                    new GameStartData { YourColor = 2, OpponentName = _name1 }));

                Console.WriteLine($"Game started: {_name1} (White) vs {_name2} (Black)");

                // Game loop
                while (true)
                {
                    NetworkStream currentStream = _logic.CurrentPlayer == 1 ? _stream1 : _stream2;
                    string currentName = _logic.CurrentPlayer == 1 ? _name1 : _name2;

                    NetMessage? msg = await NetHelper.ReadMessage(currentStream);
                    if (msg == null)
                    {
                        Console.WriteLine($"{currentName} disconnected.");
                        break;
                    }

                    // Also listen for chat/scoreboard from both players via a simpler approach:
                    // We only read from the current player's stream for moves,
                    // but we handle chat and scoreboard inline
                    if (msg.Type == MessageType.Chat)
                    {
                        // Forward chat to both players
                        await NetHelper.SendMessage(_stream1, msg);
                        await NetHelper.SendMessage(_stream2, msg);
                        continue;
                    }

                    if (msg.Type == MessageType.ScoreboardRequest)
                    {
                        List<ScoreEntry> scores = _db.GetScoreboard();
                        NetMessage response = NetMessage.Create(MessageType.ScoreboardResponse,
                            new ScoreboardData { Scores = scores });
                        await NetHelper.SendMessage(currentStream, response);
                        continue;
                    }

                    if (msg.Type != MessageType.PlaceStone)
                    {
                        await NetHelper.SendMessage(currentStream, NetMessage.Create(MessageType.Error,
                            new ErrorData { Message = "Expected PlaceStone message." }));
                        continue;
                    }

                    PlaceStoneData? placeData = msg.GetData<PlaceStoneData>();
                    if (placeData == null)
                    {
                        await NetHelper.SendMessage(currentStream, NetMessage.Create(MessageType.Error,
                            new ErrorData { Message = "Invalid PlaceStone data." }));
                        continue;
                    }

                    List<(int, int)> captured;
                    bool won;
                    string? winReason;

                    int player = _logic.CurrentPlayer;
                    if (!_logic.TryPlaceStone(placeData.Row, placeData.Col, out captured, out won, out winReason))
                    {
                        await NetHelper.SendMessage(currentStream, NetMessage.Create(MessageType.Error,
                            new ErrorData { Message = "Invalid move." }));
                        continue;
                    }

                    // Send StonePlaced to both
                    NetMessage stonePlaced = NetMessage.Create(MessageType.StonePlaced,
                        new StonePlacedData { Row = placeData.Row, Col = placeData.Col, Player = player });
                    await NetHelper.SendMessage(_stream1, stonePlaced);
                    await NetHelper.SendMessage(_stream2, stonePlaced);

                    // Send captures if any
                    for (int i = 0; i < captured.Count; i += 2)
                    {
                        CaptureData capData = new CaptureData
                        {
                            Row1 = captured[i].Item1,
                            Col1 = captured[i].Item2,
                            Row2 = captured[i + 1].Item1,
                            Col2 = captured[i + 1].Item2,
                            CapturedBy = player
                        };
                        NetMessage capMsg = NetMessage.Create(MessageType.Capture, capData);
                        await NetHelper.SendMessage(_stream1, capMsg);
                        await NetHelper.SendMessage(_stream2, capMsg);
                    }

                    // Check win
                    if (won)
                    {
                        GameOverData gameOver = new GameOverData { Winner = player, Reason = winReason! };
                        NetMessage gameOverMsg = NetMessage.Create(MessageType.GameOver, gameOver);
                        await NetHelper.SendMessage(_stream1, gameOverMsg);
                        await NetHelper.SendMessage(_stream2, gameOverMsg);

                        // Save to database
                        try
                        {
                            int whiteId = _db.GetOrCreatePlayer(_name1);
                            int blackId = _db.GetOrCreatePlayer(_name2);
                            int winnerId = player == 1 ? whiteId : blackId;
                            _db.SaveGame(whiteId, blackId, winnerId, winReason!,
                                _logic.WhiteCaptures, _logic.BlackCaptures, _logic.MoveCount);
                            Console.WriteLine($"Game over: {(player == 1 ? _name1 : _name2)} wins by {winReason}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error saving game: {ex.Message}");
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Game session error: {ex.Message}");
            }
            finally
            {
                try { _client1.Close(); } catch { }
                try { _client2.Close(); } catch { }
            }
        }
    }
}
