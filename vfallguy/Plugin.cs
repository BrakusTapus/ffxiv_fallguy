using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace vfallguy;

public sealed class Plugin : IDalamudPlugin
{
    public DalamudPluginInterface Dalamud { get; init; }

    public WindowSystem WindowSystem = new("vfallguy");
    private MainWindow _wnd;
    [PluginService] public static IChatGui? Chat { get; private set; }
    public Plugin(DalamudPluginInterface dalamud)
    {
        dalamud.Create<Service>();

        _wnd = new();
        WindowSystem.AddWindow(_wnd);

        Dalamud = dalamud;
        dalamud.UiBuilder.DisableAutomaticUiHide = true;
        Dalamud.UiBuilder.Draw += WindowSystem.Draw;
        MiniHacks.Init();
        //Dalamud.UiBuilder.OpenConfigUi += () => _wndConfig.IsOpen = true;

        if (Dalamud.Reason == PluginLoadReason.Reload)
        {
            Chat.Print($"Reloaded {Dalamud.InternalName.ToString()}", Dalamud.InternalName, 45);
            _wnd.IsOpen = !_wnd.IsOpen;
            Notify.Success(EasyCombatPlugin.Name + $"has succesfully reloaded!");
        }
    }

    public void Dispose()
    {
        MiniHacks.Dispose();
        WindowSystem.RemoveAllWindows();
        _wnd.Dispose();
    }
}
