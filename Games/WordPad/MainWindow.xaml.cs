using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Win32;

namespace WordPad;

public partial class MainWindow : Window
{
    private string? _currentFile;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void New_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        rtb.Document.Blocks.Clear();
        _currentFile = null;
        Title = "WordPad — Rich Text Editor";
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "XAML Documents|*.xaml|Rich Text|*.rtf|Text Files|*.txt|All Files|*.*"
        };
        if (dlg.ShowDialog() != true) return;

        var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        string format = Path.GetExtension(dlg.FileName).ToLower() switch
        {
            ".rtf" => DataFormats.Rtf,
            ".txt" => DataFormats.Text,
            _ => DataFormats.Xaml
        };

        try
        {
            using var fs = File.OpenRead(dlg.FileName);
            range.Load(fs, format);
            _currentFile = dlg.FileName;
            Title = $"WordPad — {Path.GetFileName(dlg.FileName)}";
            txtStatus.Text = $"Opened: {dlg.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Filter = "XAML Documents|*.xaml|Rich Text|*.rtf|Text Files|*.txt",
            FileName = _currentFile != null ? Path.GetFileName(_currentFile) : "document.xaml"
        };
        if (dlg.ShowDialog() != true) return;

        var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        string format = Path.GetExtension(dlg.FileName).ToLower() switch
        {
            ".rtf" => DataFormats.Rtf,
            ".txt" => DataFormats.Text,
            _ => DataFormats.Xaml
        };

        try
        {
            using var fs = File.Create(dlg.FileName);
            range.Save(fs, format);
            _currentFile = dlg.FileName;
            Title = $"WordPad — {Path.GetFileName(dlg.FileName)}";
            txtStatus.Text = $"Saved: {dlg.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void Bold_Click(object sender, RoutedEventArgs e)
    {
        EditingCommands.ToggleBold.Execute(null, rtb);
    }

    private void Italic_Click(object sender, RoutedEventArgs e)
    {
        EditingCommands.ToggleItalic.Execute(null, rtb);
    }

    private void Underline_Click(object sender, RoutedEventArgs e)
    {
        EditingCommands.ToggleUnderline.Execute(null, rtb);
    }

    private void FontSize_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (rtb == null || rtb.Selection.IsEmpty) return;
        if (cmbFontSize.SelectedItem is ComboBoxItem item && double.TryParse(item.Content?.ToString(), out double size))
        {
            rtb.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, size);
        }
    }

    private void Rtb_SelectionChanged(object sender, RoutedEventArgs e)
    {
        var weight = rtb.Selection.GetPropertyValue(TextElement.FontWeightProperty);
        btnBold.IsChecked = weight != DependencyProperty.UnsetValue && weight.Equals(FontWeights.Bold);

        var style = rtb.Selection.GetPropertyValue(TextElement.FontStyleProperty);
        btnItalic.IsChecked = style != DependencyProperty.UnsetValue && style.Equals(FontStyles.Italic);

        var decorations = rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
        btnUnderline.IsChecked = decorations != DependencyProperty.UnsetValue && decorations != null
            && decorations.Equals(TextDecorations.Underline);
    }
}
