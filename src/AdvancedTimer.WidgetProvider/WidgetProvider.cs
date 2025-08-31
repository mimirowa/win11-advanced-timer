using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using AdvancedTimer.Core;
using Microsoft.Windows.Widgets;
using Microsoft.Windows.Widgets.Providers;

using System.Runtime.InteropServices;

namespace AdvancedTimer.WidgetProvider;

[ComVisible(true)]
[Guid("B7D7A1D7-84A5-4B5E-9E37-DADB3312EAAA")]
public sealed class WidgetProvider : IWidgetProvider
{
    private readonly TimerService _service;
    private readonly Dictionary<string, Timer> _timers = new();
    private readonly Dictionary<string, WidgetContext> _contexts = new();
    private static readonly TimeSpan _tickInterval = TimeSpan.FromSeconds(2.5);

    public WidgetProvider()
    {
        _service = new TimerService(new FileStateStore());
    }

    public void CreateWidget(WidgetContext context)
    {
        _contexts[context.Id] = context;
        UpdateWidget(context);
    }

    public void Activate(WidgetContext context)
    {
        _contexts[context.Id] = context;
        if (!_timers.ContainsKey(context.Id))
        {
            var timer = new Timer(_ => UpdateWidget(context), null, _tickInterval, _tickInterval);
            _timers[context.Id] = timer;
        }
        UpdateWidget(context);
    }

    public void Deactivate(string widgetId)
    {
        if (_timers.TryGetValue(widgetId, out var timer))
        {
            timer.Dispose();
            _timers.Remove(widgetId);
        }
    }

    public void DeleteWidget(string widgetId, string customState)
    {
        Deactivate(widgetId);
        _contexts.Remove(widgetId);
        if (Guid.TryParse(widgetId, out var wid))
        {
            var active = _service.GetActiveForWidget(wid);
            if (active != null)
            {
                _service.Cancel(active.Id);
            }
        }
    }

    public void OnActionInvoked(WidgetActionInvokedArgs args)
    {
        var context = args.WidgetContext;
        var widgetId = context.Id;
        var data = args.Data;
        JsonElement root;
        try
        {
            root = JsonDocument.Parse(string.IsNullOrWhiteSpace(data) ? "{}" : data).RootElement;
        }
        catch
        {
            root = JsonDocument.Parse("{}").RootElement;
        }

        if (!Guid.TryParse(widgetId, out var wid))
        {
            return;
        }

        switch (args.Verb)
        {
            case "startPreset":
            case "startCustom":
            case "restartRecent":
                TimeSpan dur = TimeSpan.Zero;
                string? name = null;
                if (root.TryGetProperty("durationSeconds", out var durProp) && durProp.TryGetInt32(out var secs))
                {
                    dur = TimeSpan.FromSeconds(secs);
                }
                if (root.TryGetProperty("name", out var nameProp))
                {
                    name = nameProp.GetString();
                }
                if (dur > TimeSpan.Zero)
                {
                    _service.Start(dur, name, wid);
                }
                break;
            case "pause":
                if (TryGetId(root, out var idp))
                {
                    _service.Pause(idp);
                }
                break;
            case "resume":
                if (TryGetId(root, out var idr))
                {
                    _service.Resume(idr);
                }
                break;
            case "cancel":
                if (TryGetId(root, out var idc))
                {
                    _service.Cancel(idc);
                }
                break;
            case "openHud":
                if (TryGetId(root, out var idh))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo($"advancedtimer://restart?timerId={idh}") { UseShellExecute = true });
                    }
                    catch { }
                }
                break;
        }

        UpdateWidget(context);
    }

    public void OnWidgetContextChanged(WidgetContextChangedArgs args)
    {
        if (_contexts.TryGetValue(args.WidgetContext.Id, out var ctx))
        {
            _contexts[args.WidgetContext.Id] = args.WidgetContext;
            UpdateWidget(args.WidgetContext);
        }
    }

    private static bool TryGetId(JsonElement root, out Guid id)
    {
        id = Guid.Empty;
        if (root.TryGetProperty("id", out var idProp) && Guid.TryParse(idProp.GetString(), out var gid))
        {
            id = gid;
            return true;
        }
        return false;
    }

    private void UpdateWidget(WidgetContext context)
    {
        string templateFile = context.Size switch
        {
            WidgetSize.Small => "Timer.Small.json",
            WidgetSize.Large => "Timer.Large.json",
            _ => "Timer.Medium.json"
        };
        var widgetsFolder = Path.Combine(AppContext.BaseDirectory, "Widgets");
        var templatePath = Path.Combine(widgetsFolder, templateFile);
        string template = File.Exists(templatePath) ? File.ReadAllText(templatePath) : "{}";

        var active = Guid.TryParse(context.Id, out var wid) ? _service.GetActiveForWidget(wid) : null;
        var recents = _service.GetRecents();

        var data = new
        {
            remainingText = active?.Remaining.ToString(),
            activeName = active?.Name,
            activeId = active?.Id,
            recents
        };
        string dataJson = JsonSerializer.Serialize(data);

        // Combine template and data for hosts that expect a single payload
        var payload = new { template, data = JsonDocument.Parse(dataJson).RootElement };
        WidgetManager.GetDefault().UpdateWidget(context.Id, JsonSerializer.Serialize(payload));
    }
}
