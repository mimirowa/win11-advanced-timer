using System;
using System.IO;
using System.Text.Json;

namespace AdvancedTimer.Core;

public class FileStateStore : IStateStore
{
    private readonly string _path;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public FileStateStore()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dir = Path.Combine(localAppData, "AdvancedTimer");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "state.json");
    }

    public AppState Load()
    {
        if (!File.Exists(_path))
            return new AppState();
        try
        {
            var json = File.ReadAllText(_path);
            var state = JsonSerializer.Deserialize<AppState>(json, _jsonOptions);
            if (state == null || state.Version != AppState.CurrentVersion)
                return new AppState();
            return state;
        }
        catch
        {
            return new AppState();
        }
    }

    public void Save(AppState state)
    {
        var json = JsonSerializer.Serialize(state, _jsonOptions);
        var tempFile = _path + ".tmp";
        File.WriteAllText(tempFile, json);
        if (File.Exists(_path))
        {
            File.Replace(tempFile, _path, null);
        }
        else
        {
            File.Move(tempFile, _path);
        }
    }
}
