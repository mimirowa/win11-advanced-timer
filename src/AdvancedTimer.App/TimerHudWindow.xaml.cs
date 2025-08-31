using System;
using System.Linq;
using System.Runtime.InteropServices;
using AdvancedTimer.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Threading;

namespace AdvancedTimer.App;

public sealed partial class TimerHudWindow : Window
{
    private readonly TimerService _service;
    private readonly Guid _timerId;
    private readonly DispatcherTimer _dispatcher = new() { Interval = TimeSpan.FromSeconds(1) };

    public TimerHudWindow(TimerService service, TimerItem item)
    {
        this.InitializeComponent();
        _service = service;
        _timerId = item.Id;
        NotificationHelper.ScheduleToast(item);

        _dispatcher.Tick += (_, _) => Update();
        _dispatcher.Start();
        Update();
    }

    private void Update()
    {
        var current = _service.GetAllActive().FirstOrDefault(t => t.Id == _timerId);
        if (current == null)
        {
            this.Close();
            return;
        }

        CountdownText.Text = current.Remaining.ToString(@"hh\:mm\:ss");
        PauseButton.IsEnabled = !current.IsPaused;
        ResumeButton.IsEnabled = current.IsPaused;
    }

    private void OnPause(object sender, RoutedEventArgs e)
    {
        _service.Pause(_timerId);
    }

    private void OnResume(object sender, RoutedEventArgs e)
    {
        _service.Resume(_timerId);
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        _service.Cancel(_timerId);
        this.Close();
    }

    private void OnRestart(object sender, RoutedEventArgs e)
    {
        var item = _service.Restart(_timerId);
        NotificationHelper.ScheduleToast(item);
        Update();
    }

    private void OnTopMost(object sender, RoutedEventArgs e)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        SetWindowPos(hwnd,
            TopMostToggle.IsChecked == true ? HWND_TOPMOST : HWND_NOTOPMOST,
            0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    }

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new(-2);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;
}

