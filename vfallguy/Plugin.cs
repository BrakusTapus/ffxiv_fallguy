using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;

namespace vfallguy;

public sealed class Plugin : IDalamudPlugin
{
    public DalamudPluginInterface Dalamud { get; init; }

    public WindowSystem WindowSystem = new("vfallguy");
    private MainWindow _wnd;
    [PluginService] public static IChatGui? Chat { get; private set; }
    public Plugin(DalamudPluginInterface dalamud)
    {
        ECommonsMain.Init(dalamud, this, ECommons.Module.DalamudReflector, ECommons.Module.ObjectFunctions, ECommons.Module.ObjectLife);

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
            Svc.Chat.Print($"Reloaded {Dalamud.InternalName.ToString()}", Dalamud.InternalName, 45);
            _wnd.IsOpen = !_wnd.IsOpen;
            Notify.Success(Dalamud.InternalName.ToString() + $"has succesfully reloaded!");
        }
    }

    public void Dispose()
    {
        MiniHacks.Dispose();
        WindowSystem.RemoveAllWindows();
        _wnd.Dispose();
        ECommonsMain.Dispose();
    }
}
