using System;
using AdvancedTimer.Core;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace AdvancedTimer.App;

public static class NotificationHelper
{
    public static void ScheduleToast(TimerItem item)
    {
        var builder = new AppNotificationBuilder()
            .AddText($"{item.Name} finished")
            .AddArgument("timerId", item.Id.ToString())
            .AddButton(new AppNotificationButton("Restart", $"advancedtimer://restart?timerId={item.Id}"));

        var notification = builder.BuildNotification();
        var scheduleTime = DateTimeOffset.UtcNow.Add(item.Remaining);
        var schedule = new AppNotificationSchedule(notification, scheduleTime);
        AppNotificationManager.Default.AddToSchedule(schedule);
    }
}

