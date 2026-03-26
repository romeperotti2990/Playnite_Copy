using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace Playnite_Copy;

public partial class MainWindow : Window
{
    private readonly string storeFile;
    private ObservableCollection<string> SavedExes = new();

    public MainWindow()
    {
        InitializeComponent();

        var appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Playnite_Copy");
        Directory.CreateDirectory(appDir);
        storeFile = Path.Combine(appDir, "saved_exes.json");

        SavedList.ItemsSource = SavedExes;
        SavedList.MouseDoubleClick += SavedList_MouseDoubleClick;

        LoadSavedExes();
    }

    private void LoadSavedExes()
    {
        try
        {
            if (!File.Exists(storeFile)) return;
            var json = File.ReadAllText(storeFile);
            var list = JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
            SavedExes.Clear();
            foreach (var s in list) SavedExes.Add(s);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to load saved list: " + ex.Message);
        }
    }

    private void SaveSavedExes()
    {
        try
        {
            var arr = new string[SavedExes.Count];
            SavedExes.CopyTo(arr, 0);
            var json = JsonSerializer.Serialize(arr, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(storeFile, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to save list: " + ex.Message);
        }
    }

    private void AddExe_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
            Title = "Select an executable",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };
        if (dlg.ShowDialog() != true) return;

        var path = dlg.FileName;
        if (!File.Exists(path))
        {
            MessageBox.Show("File not found: " + path);
            return;
        }

        if (!SavedExes.Contains(path))
        {
            SavedExes.Add(path);
            SaveSavedExes();
            Console.WriteLine("Saved: " + path);
        }
    }

    private void RemoveExe_Click(object sender, RoutedEventArgs e)
    {
        if (SavedList.SelectedItem is string s)
        {
            SavedExes.Remove(s);
            SaveSavedExes();
            Console.WriteLine("Removed: " + SavedExes);
        }
    }

    private void OpenExe_Click(object sender, RoutedEventArgs e)
    {
        if (SavedList.SelectedItem is string s) StartExe(s);
    }

    private void SavedList_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (SavedList.SelectedItem is string s) StartExe(s);
    }

    private void StartExe(string exePath)
    {
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
            Console.WriteLine("Started: " + exePath); // will appear if you run via dotnet run + OutputType=Exe
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to start: " + ex.Message);
        }
    }
}