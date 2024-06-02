using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;

public static class PositionHelper
{
    private static Vector3? playerPosCache = null;

    public static Vector3 GetPlayerPositionXZY()
    {
        if (Svc.ClientState.LocalPlayer != null)
        {
            if (playerPosCache == null)
            {
                playerPosCache = new Vector3(
                    Svc.ClientState.LocalPlayer.Position.X,
                    Svc.ClientState.LocalPlayer.Position.Z,
                    Svc.ClientState.LocalPlayer.Position.Y);
            }
            return playerPosCache.Value;
        }
        return Vector3.Zero;
    }

    public static Vector3 GetPositionXZY(this GameObject a)
    {
        return new Vector3(a.Position.X, a.Position.Z, a.Position.Y);
    }
}
