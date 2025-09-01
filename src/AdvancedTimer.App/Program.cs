using System;
using AdvancedTimer.Core;
using Microsoft.UI.Xaml;

namespace AdvancedTimer.App;

public class Program
{
    private static TimerService? _timerService;
    internal static TimerService TimerService => _timerService!;

    [STAThread]
    public static void Main(string[] args)
    {
        var store = new FileStateStore();
        _timerService = new TimerService(store);

        Application.Start(p => new App());
    }
}

