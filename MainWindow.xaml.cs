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
    private ObservableCollection<GameEntry> SavedGames = new();

    public MainWindow()
    {
        InitializeComponent();

        var appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Playnite_Copy");
        Directory.CreateDirectory(appDir);
        storeFile = Path.Combine(appDir, "saved_exes.json");

        SavedList.ItemsSource = SavedGames;
        SavedList.MouseDoubleClick += SavedList_MouseDoubleClick;

        LoadSavedGames();
    }

    private void LoadSavedGames()
    {
        try
        {
            if (!File.Exists(storeFile)) return;
            var json = File.ReadAllText(storeFile);
            var list = JsonSerializer.Deserialize<GameEntry[]>(json) ?? Array.Empty<GameEntry>();
            SavedGames.Clear();
            foreach (var g in list) SavedGames.Add(g);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to load saved list: " + ex.Message);
        }
    }

    private void SaveSavedGames()
    {
        try
        {
            var arr = new GameEntry[SavedGames.Count];
            SavedGames.CopyTo(arr, 0);
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
            Filter = "Executables and ROMs|*.exe;*.nso;*.nca;*.nro;*.nsp;*.xci;*.bk2;*.nes;*.sfc;*.gb;*.gbc;*.gba|Executables (*.exe)|*.exe|Switch ROMs|*.nsp;*.xci;*.nro|BizHawk ROMs|*.nes;*.sfc;*.gb;*.gbc;*.gba|All files (*.*)|*.*",
            Title = "Select a Game or Emulator",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };
        if (dlg.ShowDialog() != true) return;

        var path = dlg.FileName;
        var ext = Path.GetExtension(path).ToLower();
        
        // Basic check: if it's not an EXE, treat it as a ROM and ask for emulator
        if (ext != ".exe")
        {
            var emuDlg = new OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe",
                Title = "Select Emulator for this ROM",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };
            
            if (emuDlg.ShowDialog() == true)
            {
                SavedGames.Add(new GameEntry { ExePath = emuDlg.FileName, RomPath = path, Name = Path.GetFileNameWithoutExtension(path) });
                SaveSavedGames();
            }
        }
        else
        {
            SavedGames.Add(new GameEntry { ExePath = path, Name = Path.GetFileNameWithoutExtension(path) });
            SaveSavedGames();
        }
    }

    private void RemoveExe_Click(object sender, RoutedEventArgs e)
    {
        if (SavedList.SelectedItems.Count > 0)
        {
            var selectedItems = new System.Collections.Generic.List<GameEntry>();
            foreach (var item in SavedList.SelectedItems)
            {
                if (item is GameEntry g) selectedItems.Add(g);
            }

            foreach (var g in selectedItems)
            {
                SavedGames.Remove(g);
            }
            SaveSavedGames();
        }
    }

    private void OpenExe_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in SavedList.SelectedItems)
        {
            if (item is GameEntry g) StartGame(g);
        }
    }

    private void SavedList_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (SavedList.SelectedItem is GameEntry g) StartGame(g);
    }

    private void StartGame(GameEntry game)
    {
        if (!File.Exists(game.ExePath))
        {
            MessageBox.Show("Emulator/Executable not found: " + game.ExePath);
            return;
        }

        try
        {
            var arguments = string.Empty;
            
            if (game.IsRom)
            {
                var lowerExe = game.ExePath.ToLower();
                if (lowerExe.Contains("yuzu") || lowerExe.Contains("ryujinx"))
                {
                    arguments = $"\"{game.RomPath}\""; // Simple path usually works for Switch emus
                }
                else if (lowerExe.Contains("emuhawk") || lowerExe.Contains("bizhawk"))
                {
                    arguments = $"\"{game.RomPath}\"";
                }
                else
                {
                    // Default fallback
                    arguments = $"\"{game.RomPath}\"";
                }
            }

            var psi = new ProcessStartInfo
            {
                FileName = game.ExePath,
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(game.ExePath) ?? string.Empty,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to start: " + ex.Message);
        }
    }
}