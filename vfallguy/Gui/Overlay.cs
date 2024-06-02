using Dalamud.Interface.Utility;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using vfallguy.Structures;

namespace vfallguy.Gui
{
    public class Overlay
    {
        public HashSet<DisplayObject> displayObjects = new();

        public void AddTextDisplayObject(float x, float y, float z, string text, uint bgcolor, uint fgcolor, float fscale)
        {
            displayObjects.Add(new DisplayObjectText(x, y, z, text, bgcolor, fgcolor, fscale));
        }

        public void DrawOverlay()
        {
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);
            foreach (var obj in displayObjects)
            {
                if (obj is DisplayObjectText elementText)
                {
                    DrawTextWorld(elementText); // use this
                }

                //if (obj is DisplayObjectText textObj)
                //{
                //    //Vector2 screenPos = WorldToScreen(new Vector3(textObj.x, textObj.y, textObj.z));
                //    //ImGui.SetNextWindowPos(screenPos);
                //    //ImGui.SetNextWindowBgAlpha(0.5f); // Adjust as needed


                //    ImGui.Begin("##OverlayText", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav/* | ImGuiWindowFlags.NoTitleBar
                //        | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground*/ | ImGuiWindowFlags.AlwaysUseWindowPadding);
                //    ImGui.TextColored(new Vector4(
                //        ((textObj.fgcolor >> 16) & 0xFF) / 255.0f,
                //        ((textObj.fgcolor >> 8) & 0xFF) / 255.0f,
                //        (textObj.fgcolor & 0xFF) / 255.0f,
                //        ((textObj.fgcolor >> 24) & 0xFF) / 255.0f
                //    ), textObj.text);
                //    ImGui.End();
                //}
            }
        }

        private Vector2 WorldToScreen(Vector3 worldPos)
        {
            // Implement the conversion from world coordinates to screen coordinates
            // This typically involves using the game's camera and projection matrices
            return new Vector2(worldPos.X, worldPos.Y); // Placeholder implementation
        }

        private void DrawTextWorld(DisplayObjectText e)
        {
            if (Svc.GameGui.WorldToScreen(
                            new Vector3(e.x, e.z, e.y),
                            out Vector2 pos))
            {
                DrawText(e, pos);
            }
        }
        int uid = 0;
        private void DrawText(DisplayObjectText e, Vector2 pos)
        {
            var scaled = e.fscale != 1f;
            var size = scaled ? ImGui.CalcTextSize(e.text) * e.fscale : ImGui.CalcTextSize(e.text);
            size = new Vector2(size.X + 10f, size.Y + 10f);
            ImGui.SetNextWindowPos(new Vector2(pos.X - size.X / 2, pos.Y - size.Y / 2));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5, 5));
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 10f);
            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.ColorConvertU32ToFloat4(e.bgcolor));
            ImGui.BeginChild("##child" + e.text + ++uid, size, false,
                ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav
                | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysUseWindowPadding);
            ImGui.PushStyleColor(ImGuiCol.Text, e.fgcolor);
            if (scaled) ImGui.SetWindowFontScale(e.fscale);
            ImGuiEx.Text(e.text);
            if (scaled) ImGui.SetWindowFontScale(1f);
            ImGui.PopStyleColor();
            ImGui.EndChild();
            ImGui.PopStyleColor();
            ImGui.PopStyleVar(2);
        }
    }


}
