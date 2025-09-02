using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

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

    public async Task<AppState> LoadAsync()
    {
        if (!File.Exists(_path))
            return new AppState();
        try
        {
            var json = await File.ReadAllTextAsync(_path);
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

    public async Task SaveAsync(AppState state)
    {
        var json = JsonSerializer.Serialize(state, _jsonOptions);
        var tempFile = _path + ".tmp";
        await File.WriteAllTextAsync(tempFile, json);
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
