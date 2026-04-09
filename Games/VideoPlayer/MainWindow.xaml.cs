using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace VideoPlayer;

public partial class MainWindow : Window
{
    private static readonly string[] VideoExtensions = { ".mp4", ".avi", ".mpg", ".mpeg", ".wmv", ".mov", ".mkv", ".webm" };
    private readonly ObservableCollection<VideoItem> _playlist = new();
    private readonly ObservableCollection<VideoItem> _history = new();
    private VideoItem? _current;
    private bool _isPlaying;

    public MainWindow()
    {
        InitializeComponent();
        lstPlaylist.ItemsSource = _playlist;
        lstHistory.ItemsSource = _history;
        video.Volume = sliderVolume.Value;

        AllowDrop = true;
        Drop += MainWindow_Drop;
    }

    private void MainWindow_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var f in files.Where(f => VideoExtensions.Contains(Path.GetExtension(f).ToLower())))
                _playlist.Add(new VideoItem { Path = f, Name = Path.GetFileNameWithoutExtension(f) });
        }
    }

    private void AddVideo_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Video Files|*.mp4;*.avi;*.mpg;*.mpeg;*.wmv;*.mov;*.mkv;*.webm|All Files|*.*",
            Multiselect = true
        };
        if (dlg.ShowDialog() != true) return;
        foreach (var f in dlg.FileNames)
            _playlist.Add(new VideoItem { Path = f, Name = Path.GetFileNameWithoutExtension(f) });
    }

    private void AddFolder_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new System.Windows.Forms.FolderBrowserDialog();
        if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        foreach (var f in Directory.EnumerateFiles(dlg.SelectedPath)
                     .Where(p => VideoExtensions.Contains(Path.GetExtension(p).ToLower())))
            _playlist.Add(new VideoItem { Path = f, Name = Path.GetFileNameWithoutExtension(f) });
    }

    private void ClearPlaylist_Click(object sender, RoutedEventArgs e)
    {
        _playlist.Clear();
        video.Stop();
        video.Source = null;
        _current = null;
        _isPlaying = false;
        UpdateUI();
    }

    private void PlayVideo(VideoItem item)
    {
        if (_current != null)
            _history.Insert(0, _current);

        _current = item;
        video.Source = new Uri(item.Path);
        video.Play();
        _isPlaying = true;
        txtNoVideo.Visibility = Visibility.Collapsed;
        UpdateUI();
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        if (_current == null && _playlist.Count > 0)
        {
            PlayVideo(_playlist[0]);
            return;
        }
        if (_current == null) return;

        if (_isPlaying)
        {
            video.Pause();
            _isPlaying = false;
        }
        else
        {
            video.Play();
            _isPlaying = true;
        }
        UpdateUI();
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        video.Stop();
        _isPlaying = false;
        UpdateUI();
    }

    private void Next_Click(object sender, RoutedEventArgs e) => PlayNext();
    private void Prev_Click(object sender, RoutedEventArgs e) => PlayPrev();

    private void PlayNext()
    {
        if (_current == null || _playlist.Count == 0) return;
        int idx = _playlist.IndexOf(_current) + 1;
        if (idx >= _playlist.Count) { _isPlaying = false; UpdateUI(); return; }
        PlayVideo(_playlist[idx]);
    }

    private void PlayPrev()
    {
        if (_current == null || _playlist.Count == 0) return;
        int idx = _playlist.IndexOf(_current) - 1;
        if (idx < 0) return;
        PlayVideo(_playlist[idx]);
    }

    private void Mute_Click(object sender, RoutedEventArgs e)
    {
        video.IsMuted = !video.IsMuted;
        btnMute.Content = video.IsMuted ? "🔇" : "Mute";
    }

    private void Volume_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (video != null)
            video.Volume = sliderVolume.Value;
    }

    private void Playlist_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (lstPlaylist.SelectedItem is VideoItem v)
            PlayVideo(v);
    }

    private void History_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (lstHistory.SelectedItem is VideoItem v)
            PlayVideo(v);
    }

    private void Video_MediaEnded(object sender, RoutedEventArgs e) => PlayNext();

    private void Video_MediaOpened(object sender, RoutedEventArgs e)
    {
        if (video.NaturalDuration.HasTimeSpan)
            txtStatus.Text = $"Duration: {video.NaturalDuration.TimeSpan:mm\\:ss}";
    }

    private void UpdateUI()
    {
        btnPlay.Content = _isPlaying ? "⏸" : "▶";
        txtStatus.Text = _current != null
            ? $"Now playing: {_current.Name}" + (_isPlaying ? "" : " (paused)")
            : "No video loaded";
    }
}
