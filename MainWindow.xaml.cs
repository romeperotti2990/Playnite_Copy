using System.Windows;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Playnite_Copy;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    public MainWindow()
    {
        InitializeComponent();
        AllocConsole(); // opens a console window for Console.WriteLine
    }

    private void Eden(object sender, RoutedEventArgs e)
    {
        // use verbatim string (no need to replace spaces)
        var exePath = @"C:\Users\meepu\Music\Totaly Just music\emu\Eden-Windows-v0.0.4-rc3-amd64-msvc-standard\eden.exe";
        if (!System.IO.File.Exists(exePath))
        {
            MessageBox.Show("File not found: " + exePath);
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = System.IO.Path.GetDirectoryName(exePath) ?? string.Empty,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to start process: " + ex.Message);
        }
    }

    private void ChooseExe(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
            Title = "Select an executable to run",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dlg.ShowDialog() != true) return;

        var exePath = dlg.FileName;

        // print to console and debug output
        Console.WriteLine("Selected exe: " + exePath);
        System.Diagnostics.Debug.WriteLine("Selected exe: " + exePath);

        if (!File.Exists(exePath))
        {
            MessageBox.Show("File not found: " + exePath);
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = Path.GetDirectoryName(exePath) ?? string.Empty,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to start process: " + ex.Message);
        }
    }
}