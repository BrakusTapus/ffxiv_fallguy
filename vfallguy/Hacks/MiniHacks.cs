using System;
using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Hooking;
using ECommons.DalamudServices;

namespace vfallguy.Hacks;

public unsafe class MiniHacks
{
    private delegate long NoMoveDelegate(
        long gameobj,
        float rot,
        float length,
        long a4,
        char a5,
        int a6);
    private static Hook<NoMoveDelegate>? antiKickHook;
    public static bool EnableAntiKick = false;
    private static long AntiKickFunc(long gameobj, float rot, float length, long a4, char a5, int a6)
    {
        if (EnableAntiKick)
            length = 0.0f;
        return antiKickHook.Original(gameobj, rot, length, a4, a5, a6);
    }

    internal static void SetAntiKick(bool isenable)
    {
        if (antiKickHook == null)
        {
            Service.ChatGui.Print($"antiKickHook is null!");
        }
        if (isenable)
        {
            antiKickHook?.Enable();
        }
        else
        {
            antiKickHook?.Disable();
        }

    }


    internal static void Init()
    {
        try
        {
            antiKickHook = Service.Hook.HookFromAddress<NoMoveDelegate>(
                Service.SigScanner.ScanText("48 ?? ?? 48 89 70 ?? 57 48 ?? ?? ?? ?? ?? ?? 0f 29 70 ?? 0f"), AntiKickFunc);
        }
        catch (Exception ex)
        {
            Service.Log.Error($"HackHooks Init Error: {ex}");
        }
    }
    public static void Dispose()
    {
        MoveSpeed(1);
        antiKickHook?.Dispose();
    }
    public static void Disable()
    {
        MoveSpeed(1);
        antiKickHook?.Disable();
        EnableAntiKick = false;
    }

    private static Lazy<nint> GetSpeedPtr = new(() =>
    {
        return Service.SigScanner.TryScanText(
            "f3 ?? ?? ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? 48 ?? ?? ?? ?? ?? ?? 0f ?? ?? e8 ?? ?? ?? ?? f3 ?? ?? ?? ?? ?? ?? ?? f3 ?? ?? ?? ?? ?? ?? ?? f3 ?? ?? ?? f3",
            out var num)
            ? num
            : nint.Zero;
    });

    private delegate nint SetSpeedDelegate(byte i);
    internal unsafe static void MoveSpeed(float speedBase)
    {
        Service.ChatGui.Print($"Current move speed is {speedBase:F3}x\nIn game speed is {speedBase * 6}");

        speedBase *= 6f;
        SafeMemory.Write(
            GetSpeedPtr.Value + new nint(4) + Marshal.ReadInt32(GetSpeedPtr.Value + new nint(4)) +
            new nint(4) + new nint(20),
            speedBase);
        SetMoveControlData(speedBase);
    }
    private static void SetMoveControlData(float speed) => SafeMemory.Write(setSpeed(1) + new nint(8), speed);

    private static nint SetSpeedFunPtr
    {
        get
        {
            return Service.SigScanner.TryScanText("E8 ?? ?? ?? ?? 48 ?? ?? 74 ?? 83 ?? ?? 75 ?? 0F ?? ?? ?? 66",
                out var num)
                ? num
                : nint.Zero;
        }
    }

    private static SetSpeedDelegate setSpeed =
        (SetSpeedDelegate)Marshal.GetDelegateForFunctionPointer(SetSpeedFunPtr, typeof(SetSpeedDelegate));

}