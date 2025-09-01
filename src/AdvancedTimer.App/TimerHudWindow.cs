using System;
using System.Linq;
using System.Runtime.InteropServices;
using AdvancedTimer.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Threading;

namespace AdvancedTimer.App;

public class TimerHudWindow : Window
{
    private readonly TimerHudView _view;
    private readonly TimerService _service;
    private readonly Guid _timerId;
    private DispatcherTimer? _dispatcher;

    public TimerHudWindow(TimerService service, TimerItem item, bool scheduleToast = true)
    {
        Title = "Timer";
        Width = 260;
        Height = 160;

        _view = new TimerHudView();
        Content = _view;

        _service = service;
        _timerId = item.Id;
        if (scheduleToast)
            NotificationHelper.ScheduleToast(item);

        _view.PauseButton.Click += OnPause;
        _view.ResumeButton.Click += OnResume;
        _view.RestartButton.Click += OnRestart;
        _view.CancelButton.Click += OnCancel;
        _view.TopMostToggle.Checked += OnTopMost;
        _view.TopMostToggle.Unchecked += OnTopMost;

        _dispatcher = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _dispatcher.Tick += OnTick;
        _dispatcher.Start();
        Update();

        Closed += OnClosed;
    }

    private void OnTick(object? sender, object? e) => Update();

    private void Update()
    {
        var current = _service.GetAllActive().FirstOrDefault(t => t.Id == _timerId);
        if (current == null)
        {
            Close();
            return;
        }

        _view.CountdownText.Text = current.Remaining.ToString(@"hh\:mm\:ss");
        _view.PauseButton.IsEnabled = !current.IsPaused;
        _view.ResumeButton.IsEnabled = current.IsPaused;
    }

    private void OnPause(object sender, RoutedEventArgs e) => _service.Pause(_timerId);

    private void OnResume(object sender, RoutedEventArgs e) => _service.Resume(_timerId);

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        _service.Cancel(_timerId);
        Close();
    }

    private void OnClosed(object sender, WindowEventArgs e)
    {
        if (_dispatcher != null)
        {
            _dispatcher.Stop();
            _dispatcher.Tick -= OnTick;
            _dispatcher = null;
        }

        _view.PauseButton.Click -= OnPause;
        _view.ResumeButton.Click -= OnResume;
        _view.RestartButton.Click -= OnRestart;
        _view.CancelButton.Click -= OnCancel;
        _view.TopMostToggle.Checked -= OnTopMost;
        _view.TopMostToggle.Unchecked -= OnTopMost;
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
            _view.TopMostToggle.IsChecked == true ? HWND_TOPMOST : HWND_NOTOPMOST,
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
