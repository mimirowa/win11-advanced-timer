using System;
using System.Collections.Generic;

namespace AdvancedTimer.Core;

public record TimerItem(
    Guid Id,
    string Name,
    TimeSpan OriginalDuration,
    TimeSpan Remaining,
    DateTimeOffset StartedAt,
    bool IsPaused,
    Guid? WidgetId);

public record RecentItem(string Name, TimeSpan Duration);

public record AppState
{
    public const int CurrentVersion = 1;
    public int Version { get; init; } = CurrentVersion;
    public List<TimerItem> ActiveTimers { get; init; } = new();
    public List<RecentItem> Recents { get; init; } = new();
}
