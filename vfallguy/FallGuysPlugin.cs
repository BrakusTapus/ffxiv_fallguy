using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.DalamudServices;
using ECommons.EzEventManager;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using vfallguy.Configuration;
using vfallguy.Structures;
using vfallguy.Gui;
using vfallguy.Hacks;

namespace vfallguy;

public unsafe class FallGuysPlugin : IDalamudPlugin
{
    public static string Name = "vfallguy+";

    public DalamudPluginInterface Dalamud { get; init; }

    public WindowSystem WindowSystem = new($"{Name}");
    //public readonly Overlay overlay = new();
    //public string OverlayName => "Player Name Overlay";

    private MainWindow _wnd;

    //public readonly VfallguyConfiguration Configuration;

    //public static FallGuysPlugin Instance { get; private set; } = null!;
    //public static IEnumerable<GameObject>? AllPlayers { get; set; }
    public FallGuysPlugin(DalamudPluginInterface dalamud)
    {
        //Instance = this;

        ECommonsMain.Init(dalamud, this, ECommons.Module.All);

        dalamud.Create<Service>();

        _wnd = new();
        WindowSystem.AddWindow(_wnd);

        Dalamud = dalamud;
        dalamud.UiBuilder.DisableAutomaticUiHide = true;
        Dalamud.UiBuilder.Draw += WindowSystem.Draw;
        MiniHacks.Init();
        //Dalamud.UiBuilder.OpenConfigUi += () => _wndConfig.IsOpen = true;
        //Svc.Framework.Update += OnUpdate;

        if (Dalamud.Reason == PluginLoadReason.Reload)
        {
            Svc.Chat.Print($"Reloaded {Dalamud.InternalName.ToString()}", Dalamud.InternalName, 45);
            _wnd.IsOpen = !_wnd.IsOpen;
            Notify.Success(Dalamud.InternalName.ToString() + $"has succesfully reloaded!");
        }
    }

    //private void OnUpdate(IFramework framework)
    //{
    //    overlay.displayObjects.Clear();

    //    foreach (var obj in Svc.Objects)
    //    {
    //        if (obj is Character character)
    //        {
    //            Vector3 pos = character.GetPositionXZY();
    //            overlay.AddTextDisplayObject(pos.X, pos.Y, pos.Z, character.Name.ToString(), 0x80000000, 0xFFFFFFFF, 1.0f);
    //        }
    //    }

    //    overlay.DrawOverlay();
    //}

    public void Dispose()
    {
        //Svc.Framework.Update -= OnUpdate;
        MiniHacks.Dispose();
        WindowSystem.RemoveAllWindows();
        _wnd.Dispose();
        ECommonsMain.Dispose();
    }

    internal void Log(string s, bool tochat = false)
    {
        if (tochat)
        {
            Svc.Chat.Print("[vFallGuys] " + s);
        }
        InternalLog.Information(s);
    }
}
