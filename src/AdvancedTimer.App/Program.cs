using System;
using System.Linq;
using AdvancedTimer.Core;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;

namespace AdvancedTimer.App;

public partial class Program
{
    private static TimerService? _timerService;
    internal static TimerService TimerService => _timerService!;

    [STAThread]
    public static void Main(string[] args)
    {
        AppInstance.RegisterProtocolForCurrentAppAsync("advancedtimer").GetAwaiter().GetResult();
        AppInstance.GetCurrent().Activated += OnActivated;

        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
        AppNotificationManager.Default.Register();

        var store = new FileStateStore();
        _timerService = new TimerService(store);

        Application.Start(p => new App());
    }

    private static void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        if (args.Arguments.TryGetValue("timerId", out var idStr) && Guid.TryParse(idStr, out var id))
        {
            var item = _timerService?.Restart(id);
            if (item != null)
            {
                NotificationHelper.ScheduleToast(item);
            }
        }
    }

    private static void OnActivated(object? sender, AppActivationArguments args)
    {
        if (args.Kind == ExtendedActivationKind.Protocol && args.Data is IProtocolActivatedEventArgs proto)
        {
            var uri = proto.Uri;
            if (uri.Host.Equals("restart", StringComparison.OrdinalIgnoreCase))
            {
                var id = GetTimerIdFromUri(uri);
                if (id != null)
                {
                    var item = _timerService?.Restart(id.Value);
                    if (item != null)
                    {
                        NotificationHelper.ScheduleToast(item);
                    }
                }
            }
            else if (uri.Host.Equals("hud", StringComparison.OrdinalIgnoreCase))
            {
                var id = GetTimerIdFromUri(uri);
                if (id != null)
                {
                    var item = _timerService?.GetAllActive().FirstOrDefault(t => t.Id == id.Value);
                    if (item != null)
                    {
                        App.ShowTimer(item, false);
                    }
                }
            }
        }
    }

    private static Guid? GetTimerIdFromUri(Uri uri)
    {
        var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in query)
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 && kv[0] == "timerId" && Guid.TryParse(Uri.UnescapeDataString(kv[1]), out var id))
                return id;
        }
        return null;
    }
}

