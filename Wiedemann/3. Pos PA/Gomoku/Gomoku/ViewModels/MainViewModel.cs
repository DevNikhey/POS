using Gomoku.Controller;
using Gomoku.Network;
using Gomoku.shared.models;
using System;
using System.Windows;
using System.Windows.Input;

public class MainViewModel
{
    // =========================
    // PROPERTIES
    // =========================

    public Gameboard Board { get; set; }

    public int BoardSize { get; set; } = 15;

    private IGameController _controller;

    public ICommand CellClickCommand { get; }

    // =========================
    // KONSTRUKTOR
    // =========================

    public MainViewModel()
    {
        Board = new Gameboard(BoardSize);

        CellClickCommand = new RelayCommand(OnCellClicked);

        var args = Environment.GetCommandLineArgs();

        MessageBox.Show(string.Join(", ", args)); // 🔥 TEST

        bool isServer = args.Length > 1 && args[1].ToLower() == "server";

        if (isServer)
        {
            StartServer();
            _ = StartNetworkGame("127.0.0.1", true);
        }
        else
        {
            _ = StartNetworkGame("127.0.0.1", false);
        }
    }

    // =========================
    // CLICK HANDLING
    // =========================

    private void OnCellClicked(object obj)
    {
        if (_controller == null)
            return;

        if (obj is Cell cell)
        {
            _controller.MakeMove(cell);
        }
    }

    // =========================
    // SPIELMODI
    // =========================

    public void StartLocalGame()
    {
        ResetBoard();
        _controller = new LocalGameController(Board);
    }

    public void StartAiGame()
    {
        ResetBoard();
        _controller = new AiGameController(Board);
    }

    public async Task StartNetworkGame(string ip, bool isServer)
    {
        var client = new NetworkClient();
        await client.Connect(ip);

        Player myPlayer = isServer ? Player.Black : Player.White;

        _controller = new NetworkGameController(Board, client, myPlayer);
    }

    // =========================
    // SERVER STARTEN
    // =========================

    public void StartServer()
    {
        MessageBox.Show("SERVER STARTET");
        var server = new NetworkServer();
        server.Start();
    }

    // =========================
    // RESET
    // =========================

    private void ResetBoard()
    {
        Board = new Gameboard(BoardSize);
    }
}