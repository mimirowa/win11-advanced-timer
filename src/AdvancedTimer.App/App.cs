using System;
using System.Linq;
using AdvancedTimer.Core;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.Activation;

namespace AdvancedTimer.App;

public class App : Application
{
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Application entry point. Timers are shown on demand.
    }

    protected override void OnActivated(IActivatedEventArgs args)
    {
        if (args is ProtocolActivatedEventArgs protocolArgs)
        {
            var uri = protocolArgs.Uri;
            if (uri.Host.Equals("restart", StringComparison.OrdinalIgnoreCase))
            {
                var id = GetTimerIdFromUri(uri);
                if (id != null)
                {
                    var item = Program.TimerService.Restart(id.Value);
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
                    var item = Program.TimerService.GetAllActive().FirstOrDefault(t => t.Id == id.Value);
                    if (item != null)
                    {
                        ShowTimer(item, false);
                    }
                }
            }
        }
    }

    public static void ShowTimer(TimerItem item, bool scheduleToast = true)
    {
        var window = new TimerHudWindow(Program.TimerService, item, scheduleToast);
        window.Activate();
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

