using System.IO;

namespace Playnite_Copy;

public class GameEntry
{
    public string Name { get; set; } = string.Empty;
    public string ExePath { get; set; } = string.Empty;
    public string RomPath { get; set; } = string.Empty;
    public bool IsRom => !string.IsNullOrEmpty(RomPath);

    public override string ToString()
    {
        return string.IsNullOrEmpty(Name) ? Path.GetFileName(IsRom ? RomPath : ExePath) : Name;
    }
}