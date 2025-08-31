#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedTimer.Core;

public class TimerService
{
    private readonly IStateStore _store;
    private readonly AppState _state;
    private int _nameCounter = 1;

    public event EventHandler<AppState>? StateChanged;

    public TimerService(IStateStore store)
    {
        _store = store;
        _state = _store.Load();
    }

    public TimerItem Start(TimeSpan duration, string? name = null, Guid? widgetId = null)
    {
        var resolvedName = string.IsNullOrWhiteSpace(name) ? GenerateName() : name;
        var now = DateTimeOffset.UtcNow;
        var item = new TimerItem(Guid.NewGuid(), resolvedName, duration, duration, now, false, widgetId);
        _state.ActiveTimers.Add(item);
        AddRecent(resolvedName, duration);
        Save();
        return item;
    }

    public TimerItem Restart(Guid id)
    {
        var index = _state.ActiveTimers.FindIndex(t => t.Id == id);
        if (index < 0) throw new InvalidOperationException("Timer not found");
        var now = DateTimeOffset.UtcNow;
        var item = _state.ActiveTimers[index];
        item = item with { Remaining = item.OriginalDuration, StartedAt = now, IsPaused = false };
        _state.ActiveTimers[index] = item;
        Save();
        return item;
    }

    public TimerItem Pause(Guid id)
    {
        var index = _state.ActiveTimers.FindIndex(t => t.Id == id);
        if (index < 0) throw new InvalidOperationException("Timer not found");
        var item = _state.ActiveTimers[index];
        if (!item.IsPaused)
        {
            var now = DateTimeOffset.UtcNow;
            var remaining = item.Remaining - (now - item.StartedAt);
            if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
            item = item with { Remaining = remaining, IsPaused = true };
            _state.ActiveTimers[index] = item;
            Save();
        }
        return item;
    }

    public TimerItem Resume(Guid id)
    {
        var index = _state.ActiveTimers.FindIndex(t => t.Id == id);
        if (index < 0) throw new InvalidOperationException("Timer not found");
        var item = _state.ActiveTimers[index];
        if (item.IsPaused)
        {
            item = item with { StartedAt = DateTimeOffset.UtcNow, IsPaused = false };
            _state.ActiveTimers[index] = item;
            Save();
        }
        return item;
    }

    public void Cancel(Guid id)
    {
        var removed = _state.ActiveTimers.RemoveAll(t => t.Id == id);
        if (removed > 0)
        {
            Save();
        }
    }

    public TimerItem? GetActiveForWidget(Guid widgetId)
    {
        var timer = _state.ActiveTimers.FirstOrDefault(t => t.WidgetId == widgetId);
        return timer == null ? null : UpdateForDisplay(timer);
    }

    public IReadOnlyList<TimerItem> GetAllActive()
    {
        return _state.ActiveTimers.Select(UpdateForDisplay).ToList();
    }

    public IReadOnlyList<RecentItem> GetRecents()
    {
        return _state.Recents.ToList();
    }

    private TimerItem UpdateForDisplay(TimerItem item)
    {
        if (item.IsPaused)
            return item;
        var now = DateTimeOffset.UtcNow;
        var remaining = item.Remaining - (now - item.StartedAt);
        if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
        return item with { Remaining = remaining };
    }

    private void AddRecent(string name, TimeSpan duration)
    {
        var existing = _state.Recents.FirstOrDefault(r => r.Name == name && r.Duration == duration);
        if (existing != null)
        {
            _state.Recents.Remove(existing);
        }
        _state.Recents.Insert(0, new RecentItem(name, duration));
        if (_state.Recents.Count > 10)
        {
            _state.Recents.RemoveRange(10, _state.Recents.Count - 10);
        }
    }

    private string GenerateName() => $"Timer {_nameCounter++}";

    private void Save()
    {
        _store.Save(_state);
        StateChanged?.Invoke(this, _state);
    }
}
