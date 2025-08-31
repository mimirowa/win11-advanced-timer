using AdvancedTimer.Core;
using Microsoft.UI.Xaml;

namespace AdvancedTimer.App;

public class App : Application
{
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Application entry point. Timers are shown on demand.
    }

    public static void ShowTimer(TimerItem item, bool scheduleToast = true)
    {
        var window = new TimerHudWindow(Program.TimerService, item, scheduleToast);
        window.Activate();
    }
}

