using System;
using AdvancedTimer.Core;
using Windows.UI.Notifications;
using CommunityToolkit.WinUI.Notifications;

namespace AdvancedTimer.App;

public static class NotificationHelper
{
    public static void ScheduleToast(TimerItem item)
    {
        var builder = new ToastContentBuilder()
            .AddText($"{item.Name} finished")
            .AddButton(new ToastButton()
                .SetContent("Restart")
                .SetProtocolActivation(new Uri($"advancedtimer://restart?timerId={item.Id}")))
            .AddToastActivationInfo($"advancedtimer://restart?timerId={item.Id}", ToastActivationType.Protocol);

        var content = builder.GetToastContent();
        var xml = content.GetXml();

        var scheduleTime = item.EndUtc.ToLocalTime();
        var notifier = ToastNotificationManager.CreateToastNotifier();
        var scheduled = new ScheduledToastNotification(xml, scheduleTime);
        notifier.AddToSchedule(scheduled);
    }
}
