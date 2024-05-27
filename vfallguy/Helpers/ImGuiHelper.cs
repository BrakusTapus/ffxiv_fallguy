using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using System.Runtime.InteropServices;
using System.Text;
using ECommons;
using System.Globalization;
using ImGuiNET;
using System.Numerics;
using Dalamud.Interface;
using System.Collections.Generic;
using System;
using System.Linq;

namespace vfallguy.Helpers;

/// <summary>
/// Helper class for ImGui
/// </summary>
public static unsafe partial class ImGuiHelper
{
    /// <summary>
    /// Get the font based on size.
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public unsafe static ImFontPtr GetFont(float size)
    {
        var style = new Dalamud.Interface.GameFonts.GameFontStyle(Dalamud.Interface.GameFonts.GameFontStyle.GetRecommendedFamilyAndSize(Dalamud.Interface.GameFonts.GameFontFamily.Axis, size));

        var handle = Svc.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(style);

        try
        {
            var font = handle.Lock().ImFont;

            if ((nint)font.NativePtr == nint.Zero)
            {
                return ImGui.GetFont();
            }
            font.Scale = size / font.FontSize;
            return font;
        }
        catch
        {
            return ImGui.GetFont();
        }
    }

    /// <summary>
    /// Checks if the mouse position is within a specified rectangular area.
    /// </summary>
    public static bool IsInRect(Vector2 leftTop, Vector2 size)
    {
        var pos = ImGui.GetMousePos() - leftTop;
        if (pos.X <= 0 || pos.Y <= 0 || pos.X >= size.X || pos.Y >= size.Y) return false;
        return true;
    }

    #region Buttons
    /// <summary>
    /// Button that is disabled unless CTRL key is held
    /// </summary>
    /// <param name="text">Button ID</param>
    /// <param name="affix">Button affix</param>
    /// <returns></returns>
    public static bool ButtonCtrl(string text, string affix = " (Hold CTRL)")
    {
        var disabled = !ImGui.GetIO().KeyCtrl;
        if (disabled)
        {
            ImGui.BeginDisabled();
        }
        var name = string.Empty;
        if (text.Contains($"###"))
        {
            var p = text.Split($"###");
            name = $"{p[0]}{affix}###{p[1]}";
        }
        else if (text.Contains($"##"))
        {
            var p = text.Split($"##");
            name = $"{p[0]}{affix}##{p[1]}";
        }
        else
        {
            name = $"{text}{affix}";
        }
        var ret = ImGui.Button(name);
        if (disabled)
        {
            ImGui.EndDisabled();
        }
        return ret;
    }

    /// <summary>
    /// Creates a button that copies specified text to the clipboard and shows a success notification when clicked.
    /// </summary>
    public static void ButtonCopy(string buttonText, string copy)
    {
        if (ImGui.Button(buttonText.Replace("$COPY", copy)))
        {
            ImGui.SetClipboardText(copy);
            Notify.Success("Text copied to clipboard");
        }
    }

    /// <summary>
    /// Creates a button with an icon and returns whether the button was clicked.
    /// </summary>
    public static bool IconButton(string icon, string id = "ECommonsButton")
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button($"{icon}##{icon}-{id}");
        ImGui.PopFont();
        return result;
    }

    /// <summary>
    /// Creates a button with a FontAwesome icon and returns whether the button was clicked.
    /// </summary>
    public static bool IconButton(FontAwesomeIcon icon, string id = "IconButton", Vector2 size = default)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-{id}", size);
        ImGui.PopFont();
        return result;
    }

    /// <summary>
    /// Creates an invisible button with the specified width.
    /// </summary>
    public static void InvisibleButton(int width = 0)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0);
        ImGui.Button(" ");
        ImGui.PopStyleVar();
    }

    /// <summary>
    /// Draws two radio buttons for a boolean value.
    /// </summary>
    /// <param name="labelTrue">True choice radio button text</param>
    /// <param name="labelFalse">False choice radio button text</param>
    /// <param name="value">Value</param>
    /// <param name="sameLine">Whether to draw radio buttons on the same line</param>
    /// <param name="prefix">Will be invoked before each radio button draw</param>
    /// <param name="suffix">Will be invoked after each radio button draw</param>
    public static void RadioButtonBool(string labelTrue, string labelFalse, ref bool value, bool sameLine = false, System.Action? prefix = null, System.Action? suffix = null)
    {
        prefix?.Invoke();
        if (ImGui.RadioButton(labelTrue, value)) value = true;
        suffix?.Invoke();
        if (sameLine) ImGui.SameLine();
        prefix?.Invoke();
        if (ImGui.RadioButton(labelFalse, !value)) value = false;
        suffix?.Invoke();
    }

    /// <summary>
    /// Displays a selectable combo box and updates the index based on user selection.
    /// </summary>
    public static unsafe bool SelectableCombo(string popUp, string[] items, ref int index)
    {
        var count = items.Length;
        var name = items[index % count] + "##" + popUp;

        var result = false;

        if (SelectableButton(name))
        {
            if (count < 3)
            {
                index = (index + 1) % count;
                result = true;
            }
            else
            {
                if (!ImGui.IsPopupOpen(popUp)) ImGui.OpenPopup(popUp);
            }
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        if (ImGui.BeginPopup(popUp))
        {
            for (int i = 0; i < count; i++)
            {
                if (ImGui.Selectable(items[i]))
                {
                    index = i;
                    result = true;
                }
            }
            ImGui.EndPopup();
        }

        return result;
    }

    /// <summary>
    /// Creates a button with custom active and hovered styles, returning whether the button was clicked.
    /// </summary>
    public static unsafe bool SelectableButton(string name)
    {
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.HeaderActive)));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.HeaderHovered)));
        ImGui.PushStyleColor(ImGuiCol.Button, 0);
        var result = ImGui.Button(name);
        ImGui.PopStyleColor(3);
        return result;
    }
    #endregion

    #region Checkbox
    /// <summary>
    /// Creates a checkbox with a tooltip, returning whether the value was changed.
    /// </summary>
    public static bool CheckboxWithTooltip(string label, ref bool value, string helpText)
    {
        bool changed = ImGui.Checkbox(label, ref value);

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 40f);
            ImGui.TextColored(EColor.YellowBright, helpText);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }

        return changed;
    }

    /// <summary>
    /// Provides a button that can be used to switch <see langword="bool"/>? variables. Left click - to toggle between <see langword="true"/> and <see langword="null"/>, right click - to toggle between <see langword="false"/> and <see langword="null"/>.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="TrueColor">Color when <paramref name="value"/> is true</param>
    /// <param name="FalseColor">Color when <paramref name="value"/> is false</param>
    /// <param name="smallButton">Whether a button should be small</param>
    /// <returns></returns>
    public static bool ButtonCheckbox(string name, ref bool? value, Vector4? TrueColor = null, Vector4? FalseColor = null, bool smallButton = false)
    {
        TrueColor ??= EColor.Green;
        FalseColor ??= EColor.Red;
        var col = value;
        var ret = false;
        if (col == true)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, TrueColor.Value);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, TrueColor.Value);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, TrueColor.Value);
        }
        else if (col == false)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, FalseColor.Value);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, FalseColor.Value);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, FalseColor.Value);
        }
        if (smallButton ? ImGui.SmallButton(name) : ImGui.Button(name))
        {
            if (value == null || value == false)
            {
                value = true;
            }
            else
            {
                value = false;
            }
            ret = true;
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            if (value == null || value == true)
            {
                value = false;
            }
            else
            {
                value = true;
            }
            ret = true;
        }
        if (col != null) ImGui.PopStyleColor(3);
        return ret;
    }

    /// <summary>
    /// Draws a button that acts like a checkbox.
    /// </summary>
    /// <param name="name">Button text</param>
    /// <param name="value">Value</param>
    /// <param name="smallButton">Whether button should be small</param>
    /// <returns>true when clicked, otherwise false</returns>
    public static bool ButtonCheckbox(string name, ref bool value, bool smallButton = false) => ButtonCheckbox(name, ref value, EColor.Red, smallButton);

    /// <summary>
    /// Draws a button that acts like a checkbox.
    /// </summary>
    /// <param name="name">Button text</param>
    /// <param name="value">Value</param>
    /// <param name="color">Active button color</param>
    /// <param name="smallButton">Whether button should be small</param>
    /// <returns>true when clicked, otherwise false</returns>
    public static bool ButtonCheckbox(string name, ref bool value, uint color, bool smallButton = false) => ButtonCheckbox(name, ref value, color.ToVector4(), smallButton);

    /// <summary>
    /// Draws a button that acts like a checkbox.
    /// </summary>
    /// <param name="name">Button text</param>
    /// <param name="value">Value</param>
    /// <param name="color">Active button color</param>
    /// <param name="smallButton">Whether button should be small</param>
    /// <returns>true when clicked, otherwise false</returns>
    public static bool ButtonCheckbox(string name, ref bool value, Vector4 color, bool smallButton = false)
    {
        var col = value;
        var ret = false;
        if (col)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, color);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, color);
        }
        if (smallButton ? ImGui.SmallButton(name) : ImGui.Button(name))
        {
            value = !value;
            ret = true;
        }
        if (col) ImGui.PopStyleColor(3);
        return ret;
    }

    /// <summary>
    /// Creates a button or checkbox that toggles the inclusion of a value in a collection, with default color.
    /// </summary>
    public static bool CollectionButtonCheckbox<T>(string name, T value, HashSet<T> collection, bool smallButton = false) => CollectionButtonCheckbox(name, value, collection, EColor.Red, smallButton);

    /// <summary>
    /// Creates a button or small button that toggles the inclusion of a value in a collection, with specified color.
    /// </summary>
    public static bool CollectionButtonCheckbox<T>(string name, T value, HashSet<T> collection, Vector4 color, bool smallButton = false)
    {
        var col = collection.Contains(value);
        var ret = false;
        if (col)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, color);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, color);
        }
        if (smallButton ? ImGui.SmallButton(name) : ImGui.Button(name))
        {
            if (col)
            {
                collection.Remove(value);
            }
            else
            {
                collection.Add(value);
            }
            ret = true;
        }
        if (col) ImGui.PopStyleColor(3);
        return ret;
    }

#pragma warning disable S125 // Sections of code should not be commented out
    //[Obsolete("Please switch to CollectionCheckbox")]
    //public static bool HashSetCheckbox<T>(string label, T value, HashSet<T> collection) => CollectionCheckbox(label, value, collection);
#pragma warning restore S125 // Sections of code should not be commented out

    /// <summary>
    /// Creates a checkbox that toggles the inclusion of a value in a collection.
    /// </summary>
    public static bool CollectionCheckbox<T>(string label, T value, HashSet<T> collection)
    {
        var x = collection.Contains(value);
        if (ImGui.Checkbox(label, ref x))
        {
            if (x)
            {
                collection.Add(value);
            }
            else
            {
                collection.Remove(value);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a checkbox that toggles the inclusion of a value in a collection, with optional inversion logic.
    /// </summary>
    public static bool CollectionCheckbox<T>(string label, T value, List<T> collection, bool inverted = false) where T : notnull
    {
        var x = collection.Contains(value);
        if (inverted) x = !x;
        if (ImGui.Checkbox(label, ref x))
        {
            if (inverted) x = !x;
            if (x)
            {
                collection.Add(value);
            }
            else
            {
                collection.RemoveAll(item => item.Equals(value));
            }
            return true;
        }
        return false;
    }
    #endregion

    #region Conversions (colours)
    /// <summary>
    /// Mutates the specified ImGui color by changing its RGB values and returns the new color.
    /// </summary>
    public static Vector4 MutateColor(ImGuiCol col, byte r, byte g, byte b)
    {
        return ImGui.GetStyle().Colors[(int)col] with { X = (float)r / 255f, Y = (float)g / 255f, Z = (float)b / 255f };
    }

    /// <summary>
    /// Converts RGB color to <see cref="Vector4"/> for ImGui
    /// </summary>
    /// <param name="col">Color in format 0xRRGGBB</param>
    /// <param name="alpha">Optional transparency value between 0 and 1</param>
    /// <returns>Color in <see cref="Vector4"/> format ready to be used with <see cref="ImGui"/> functions</returns>
    public static Vector4 Vector4FromRGB(uint col, float alpha = 1.0f)
    {
        byte* bytes = (byte*)&col;
        return new Vector4((float)bytes[2] / 255f, (float)bytes[1] / 255f, (float)bytes[0] / 255f, alpha);
    }

    /// <summary>
    /// Converts RGBA color to <see cref="Vector4"/> for ImGui
    /// </summary>
    /// <param name="col">Color in format 0xRRGGBBAA</param>
    /// <returns>Color in <see cref="Vector4"/> format ready to be used with <see cref="ImGui"/> functions</returns>
    public static Vector4 Vector4FromRGBA(uint col)
    {
        byte* bytes = (byte*)&col;
        return new Vector4((float)bytes[3] / 255f, (float)bytes[2] / 255f, (float)bytes[1] / 255f, (float)bytes[0] / 255f);
    }

    /// <summary>
    /// Returns a color based on the provided percentage value.
    /// </summary>
    public static Vector4 GetParsedColor(int percent)
    {
        if (percent < 25)
        {
            return ImGuiColors.ParsedGrey;
        }
        else if (percent < 50)
        {
            return ImGuiColors.ParsedGreen;
        }
        else if (percent < 75)
        {
            return ImGuiColors.ParsedBlue;
        }
        else if (percent < 95)
        {
            return ImGuiColors.ParsedPurple;
        }
        else if (percent < 99)
        {
            return ImGuiColors.ParsedOrange;
        }
        else if (percent == 99)
        {
            return ImGuiColors.ParsedPink;
        }
        else if (percent == 100)
        {
            return ImGuiColors.ParsedGold;
        }
        else
        {
            return ImGuiColors.DalamudRed;
        }
    }

    #endregion

    #region Combo
    /// <summary>
    /// Initializes a new dictionary for storing search values for combo boxes with string keys and Box&lt;string&gt; values.
    /// </summary>
    public static Dictionary<string, Box<string>> EnumComboSearch { get; private set; } = new Dictionary<string, Box<string>>();

    /// <summary>
    /// Draws an easy combo selector for an enum with a search field for long lists.
    /// </summary>
    /// <typeparam name="T">Enum</typeparam>
    /// <param name="name">ImGui ID</param>
    /// <param name="refConfigField">Value</param>
    /// <param name="names">Optional Name overrides</param>
    public static bool EnumCombo<T>(string name, ref T refConfigField, IDictionary<T, string> names) where T : struct, IConvertible
    {
        return EnumCombo(name, ref refConfigField, null, names);
    }

    /// <summary>
    /// Draws an easy combo selector for an enum with a search field for long lists.
    /// </summary>
    /// <typeparam name="T">Enum</typeparam>
    /// <param name="name">ImGui ID</param>
    /// <param name="refConfigField">Value</param>
    /// <param name="filter">Optional filter</param>
    /// <param name="names">Optional Name overrides</param>
    /// <returns></returns>
    public static bool EnumCombo<T>(string name, ref T refConfigField, Func<T, bool>? filter = null, IDictionary<T, string>? names = null) where T : struct, IConvertible
    {
        var ret = false;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var currentValue = refConfigField.ToString().Replace("_", " ");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        if (names != null && names.TryGetValue(refConfigField, out var nameOverride))
        {
            currentValue = nameOverride;
        }
        if (ImGui.BeginCombo(name, currentValue))
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            Box<string>? fltr = null;
            if (values.Length > 10)
            {
                if (!EnumComboSearch.ContainsKey(name)) EnumComboSearch.Add(name, new(""));
                fltr = EnumComboSearch[name];
                ImGuiHelper.SetNextItemFullWidth();
                ImGui.InputTextWithHint($"##{name.Replace("#", "_")}", "Filter...", ref fltr.Value, 50);
            }
            foreach (var x in values)
            {
                var equals = EqualityComparer<T>.Default.Equals(x, refConfigField);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var element = x.ToString().Replace("_", " ");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (names != null && names.TryGetValue(x, out nameOverride))
                {
                    element = nameOverride;
                }
                if ((filter == null || filter(x))
                    && (fltr == null || element.Contains(fltr.Value, StringComparison.OrdinalIgnoreCase))
                    && ImGui.Selectable(element, equals))
                {
                    ret = true;
                    refConfigField = x;
                }
                if (ImGui.IsWindowAppearing() && equals) ImGui.SetScrollHereY();
            }
            ImGui.EndCombo();
        }
        return ret;
    }

    /// <summary>
    /// Initializes a new dictionary for storing search values for combo boxes with string keys and Box&lt;string&gt; values.
    /// </summary>
    public static Dictionary<string, Box<string>> ComboSearch { get; private set; } = new Dictionary<string, Box<string>>();

    /// <summary>
    /// Creates a combo box for selecting a value from a collection, with optional filtering and custom display names.
    /// </summary>
    public static bool Combo<T>(string name, ref T refConfigField, IEnumerable<T> values, Func<T, bool>? filter = null, Dictionary<T, string>? names = null) where T : notnull
    {
        var ret = false;
        if (ImGui.BeginCombo(name, (names != null && names.TryGetValue(refConfigField, out var n)) ? n : refConfigField.ToString()))
        {
            Box<string>? fltr = null;
            if (values.Count() > 10)
            {
                if (!ComboSearch.ContainsKey(name)) ComboSearch.Add(name, new(""));
                fltr = ComboSearch[name];
                ImGuiHelper.SetNextItemFullWidth();
                ImGui.InputTextWithHint($"##{name}fltr", "Filter...", ref fltr.Value, 50);
            }
            foreach (var x in values)
            {
                var equals = EqualityComparer<T>.Default.Equals(x, refConfigField);
                var element = (names != null && names.TryGetValue(x, out n)) ? n : x.ToString();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if ((filter == null || filter(x))
                    && (fltr == null || element.Contains(fltr.Value, StringComparison.OrdinalIgnoreCase))
                    && ImGui.Selectable(element, equals)
                    )
                {
                    ret = true;
                    refConfigField = x;
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (ImGui.IsWindowAppearing() && equals) ImGui.SetScrollHereY();
            }
            ImGui.EndCombo();
        }
        return ret;
    }
    #endregion

    #region Headers
    /// <summary>
    /// Creates a collapsing header with optional text color and returns whether it is open or collapsed.
    /// </summary>
    public static bool CollapsingHeader(string text, Vector4? col = null)
    {
        if (col != null) ImGui.PushStyleColor(ImGuiCol.Text, col.Value);
        var ret = ImGui.CollapsingHeader(text);
        if (col != null) ImGui.PopStyleColor();
        return ret;
    }
    #endregion

    #region Icons
    /// <summary>
    /// Represents options for configuring a header icon in a user interface.
    /// </summary>
    public record HeaderIconOptions
    {
        /// <summary>
        /// Gets or initializes the offset of the icon.
        /// </summary>
        public Vector2 Offset { get; init; } = Vector2.Zero;

        /// <summary>
        /// Gets or initializes the mouse button used to interact with the icon.
        /// </summary>
        public ImGuiMouseButton MouseButton { get; init; } = ImGuiMouseButton.Left;

        /// <summary>
        /// Gets or initializes the tooltip text displayed when hovering over the icon.
        /// </summary>
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
        public string Tooltip { get; init; } = string.Empty;
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members

        /// <summary>
        /// Gets or initializes the color of the icon.
        /// </summary>
        public uint Color { get; init; } = 0xFFFFFFFF;

        /// <summary>
        /// Gets or initializes whether a toast tooltip is displayed when the icon is clicked.
        /// </summary>
        public bool ToastTooltipOnClick { get; init; } = false;

        /// <summary>
        /// Gets or initializes the mouse button that triggers the toast tooltip on click.
        /// </summary>
        public ImGuiMouseButton ToastTooltipOnClickButton { get; init; } = ImGuiMouseButton.Left;
    }

    private static uint HeaderLastWindowID = 0;
    private static ulong HeaderLastFrame = 0;
    private static float HeaderCurrentPos = 0;
    private static float HeaderImGuiButtonWidth = 0;

    /// <summary>
    /// Adds a header icon with customizable options, handling position, interactions, and visual feedback.
    /// </summary>
    public static bool AddHeaderIcon(string id, FontAwesomeIcon icon, HeaderIconOptions? options = null)
    {
        if (ImGui.IsWindowCollapsed()) return false;

        var scale = ImGuiHelpers.GlobalScale;
        var currentID = ImGui.GetID(0);
        if (currentID != HeaderLastWindowID || HeaderLastFrame != Svc.PluginInterface.UiBuilder.FrameCount)
        {
            HeaderLastWindowID = currentID;
            HeaderLastFrame = Svc.PluginInterface.UiBuilder.FrameCount;
            HeaderCurrentPos = 2.1f * scale;
            HeaderImGuiButtonWidth = 2.1f * scale;

            if (!ImGuiHelper.GetCurrentWindowFlags().HasFlag(ImGuiWindowFlags.NoCollapse))
            {
                HeaderCurrentPos += 3f * scale;
            }
        }

        options ??= new();
        var prevCursorPos = ImGui.GetCursorPos();
        var buttonSize = new Vector2(20 * scale);
        var buttonPos = new Vector2((ImGui.GetWindowWidth() - buttonSize.X - HeaderImGuiButtonWidth * scale * HeaderCurrentPos) - (ImGui.GetStyle().FramePadding.X * scale) - (ImGui.GetStyle().WindowPadding.X * scale), ImGui.GetScrollY() + 1);
        ImGui.SetCursorPos(buttonPos);
        var drawList = ImGui.GetWindowDrawList();
        drawList.PushClipRectFullScreen();

        var pressed = false;
        ImGui.InvisibleButton(id, buttonSize);
        var itemMin = ImGui.GetItemRectMin();
        var itemMax = ImGui.GetItemRectMax();
        var halfSize = ImGui.GetItemRectSize() / 2;
        var center = itemMin + halfSize;
        if (ImGui.IsWindowHovered() && ImGui.IsMouseHoveringRect(itemMin, itemMax, false))
        {
            if (!string.IsNullOrEmpty(options.Tooltip))
                ImGui.SetTooltip(options.Tooltip);
            ImGui.GetWindowDrawList().AddCircleFilled(center, halfSize.X, ImGui.GetColorU32(ImGui.IsMouseDown(ImGuiMouseButton.Left) ? ImGuiCol.ButtonActive : ImGuiCol.ButtonHovered));
            if (ImGui.IsMouseReleased(options.MouseButton))
                pressed = true;
            if (options.ToastTooltipOnClick && ImGui.IsMouseReleased(options.ToastTooltipOnClickButton))
                Notify.Info(options.Tooltip!);
        }

        ImGui.SetCursorPos(buttonPos);
        ImGui.PushFont(UiBuilder.IconFont);
        var iconString = icon.ToIconString();
        drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), itemMin + halfSize - ImGui.CalcTextSize(iconString) / 2 + options.Offset, options.Color, iconString);
        ImGui.PopFont();

        ImGui.PopClipRect();
        ImGui.SetCursorPos(prevCursorPos);

        return pressed;
    }

    /// <summary>
    /// Calculates the size of a FontAwesome icon.
    /// </summary>
    public static Vector2 CalculateIconSize(FontAwesomeIcon icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.CalcTextSize($"{icon.ToIconString()}");
        ImGui.PopFont();
        return result;
    }

    /// <summary>
    /// <br>HelpMarker component to add a help icon with text on hover.</br>
    /// <br>helpText: The text to display on hover.</br>
    /// </summary>
    /// <param name="helpText"></param>
    public static void HelpMarker(string helpText)
    {
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextDisabled(FontAwesomeIcon.InfoCircle.ToIconString());
        ImGui.PopFont();
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 40f);
            ImGui.TextColored(EasyColor.GhostType, helpText);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }
    #endregion

    #region Images
    /// <summary>
    /// texture is the image.   Image will scale if the maxWidth(image width?) is lower then wholewidth(threshold for when to scale)
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="wholeWidth"></param>
    /// <param name="maxWidth"></param>
    /// <param name="maxHeight"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static bool TextureButton(IDalamudTextureWrap texture, float wholeWidth, float maxWidth, float maxHeight, string id = "")
    {
        if (texture == null) return false;

        var size = new Vector2(texture.Width, texture.Height) * MathF.Min(1, MathF.Min(maxWidth, wholeWidth) / texture.Width);
        var result = false;
        DrawItemMiddle(() =>
        {
            result = NoPaddingNoColorImageButton(texture.ImGuiHandle, size, id);
        }, wholeWidth, size.X);
        return result;
    }

    internal static bool TextureButton(IDalamudTextureWrap texture, float wholeWidth, float maxWidth, string id = "")
    {
        if (texture == null) return false;

        var size = new Vector2(texture.Width, texture.Height) * MathF.Min(1, MathF.Min(maxWidth, wholeWidth) / texture.Width);

        var result = false;
        DrawItemMiddle(() =>
        {
            result = NoPaddingNoColorImageButton(texture.ImGuiHandle, size, id);
        }, wholeWidth, size.X);
        return result;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="columnWidth"></param>
    /// <param name="maxWidth"></param>
    /// <param name="maxHeight"></param>
    internal static void ImageInNewColumn(IDalamudTextureWrap? texture, float columnWidth, float maxWidth, float maxHeight)
    {
        if (texture == null) return;

        float aspectRatio = (float)texture.Width / texture.Height;
        float width = Math.Min(columnWidth, maxWidth); // Adjust the maxWidth as needed

        ImGui.TableNextColumn();
        ImGui.Spacing();

        // Calculate the height while maintaining aspect ratio
        float height = Math.Min(width / aspectRatio, maxHeight); // Adjust the maxHeight as needed
        NoPaddingNoColorImageButton(texture.ImGuiHandle, new Vector2(width, height));

        ImGui.Spacing();
    }

    internal static unsafe bool NoPaddingNoColorImageButton(nint handle, Vector2 size, string id = "")
        => NoPaddingNoColorImageButton(handle, size, Vector2.Zero, Vector2.One, id);

    internal static unsafe bool NoPaddingNoColorImageButton(nint handle, Vector2 size, Vector2 uv0, Vector2 uv1, string id = "")
    {
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0);
        ImGui.PushStyleColor(ImGuiCol.Button, 0);
        var result = NoPaddingImageButton(handle, size, uv0, uv1, id);
        ImGui.PopStyleColor(3);

        return result;
    }

    /// <summary>
    /// Renders an image button without padding and allows specifying texture coordinates for rendering.
    /// </summary>
    /// <param name="handle">A pointer to the image texture.</param>
    /// <param name="size">The size of the image button.</param>
    /// <param name="uv0">The UV (texture) coordinates of the upper-left corner of the image within the texture.</param>
    /// <param name="uv1">The UV (texture) coordinates of the lower-right corner of the image within the texture.</param>
    /// <param name="id">Optional identifier for the button.</param>
    /// <returns>True if the button is pressed; otherwise, false.</returns>
    internal static bool NoPaddingImageButton(nint handle, Vector2 size, Vector2 uv0, Vector2 uv1, string id = "")
    {
        var padding = ImGui.GetStyle().FramePadding;
        ImGui.GetStyle().FramePadding = Vector2.Zero;

        ImGui.PushID(id);
        var result = ImGui.ImageButton(handle, size, uv0, uv1);
        ImGui.PopID();
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        ImGui.GetStyle().FramePadding = padding;
        return result;
    }

    internal static unsafe bool SilenceImageButton(nint handle, Vector2 size, bool selected, string id = "")
        => SilenceImageButton(handle, size, Vector2.Zero, Vector2.One, selected, id);
    internal static unsafe bool SilenceImageButton(nint handle, Vector2 size, Vector2 uv0, Vector2 uv1, bool selected, string id = "")
    {
        return SilenceImageButton(handle, size, uv0, uv1, selected ? ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.Header)) : 0, id);
    }

    internal static unsafe bool SilenceImageButton(nint handle, Vector2 size, Vector2 uv0, Vector2 uv1, uint buttonColor, string id = "")
    {
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.HeaderActive)));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.HeaderHovered)));
        ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);

        var result = NoPaddingImageButton(handle, size, uv0, uv1, id);
        ImGui.PopStyleColor(3);

        return result;
    }
    #endregion

    #region InputLists
    /// <summary>
    /// Displays an input field for a hexadecimal uint and updates the value if the input is valid.
    /// </summary>
    public static void InputHex(string name, ref uint hexInt)
    {
        var text = $"{hexInt:X}";
        if (ImGui.InputText(name, ref text, 8))
        {
#pragma warning disable S1066 // Mergeable "if" statements should be combined
            if (uint.TryParse(text.Replace("0x", ""), NumberStyles.HexNumber, null, out var num))
            {
                hexInt = num;
            }
#pragma warning restore S1066 // Mergeable "if" statements should be combined
        }
    }

    /// <summary>
    /// Displays an input field for a hexadecimal byte and updates the value if the input is valid.
    /// </summary>
    public static void InputHex(string name, ref byte hexByte)
    {
        var text = $"{hexByte:X}";
        if (ImGui.InputText(name, ref text, 2))
        {
#pragma warning disable S1066 // Mergeable "if" statements should be combined
            if (byte.TryParse(text, NumberStyles.HexNumber, null, out var num))
            {
                hexByte = num;
            }
#pragma warning restore S1066 // Mergeable "if" statements should be combined
        }
    }

    /// <summary>
    /// Displays an input field for an unsigned integer and updates the value if the input is valid.
    /// </summary>
    public static void InputUint(string name, ref uint uInt)
    {
        var text = $"{uInt}";
        if (ImGui.InputText(name, ref text, 16))
        {
#pragma warning disable S1066 // Mergeable "if" statements should be combined
            if (uint.TryParse(text, out var num))
            {
                uInt = num;
            }
#pragma warning restore S1066 // Mergeable "if" statements should be combined
        }
    }

    /// <summary>
    /// Displays an integer input field with bounded values, ensuring the input stays within the specified minimum and maximum range.
    /// </summary>
    public static bool InputIntBounded(string label, ref int value, int minValue, int maxValue)
    {
        if (ImGui.InputInt(label, ref value))
        {
            if (value > maxValue)
                value = maxValue;

            if (value < minValue)
                value = minValue;

            return true;
        }

        return false;
    }

    static Dictionary<string, float> InputWithRightButtonsAreaValues = new();
    /// <summary>
    /// Convenient way to display stretched input with button or other elements on it's right side.
    /// </summary>
    /// <param name="id">Unique ID</param>
    /// <param name="inputAction">A single element that accepts transformation by ImGui.SetNextItemWidth method</param>
    /// <param name="rightAction">A line of elements on the right side. Can contain multiple elements but only one line.</param>
    public static void InputWithRightButtonsArea(string id, System.Action inputAction, System.Action rightAction)
    {
        if (InputWithRightButtonsAreaValues.ContainsKey(id))
        {
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - InputWithRightButtonsAreaValues[id]);
        }
        inputAction();
        ImGui.SameLine();
        var cur1 = ImGui.GetCursorPosX();
        rightAction();
        ImGui.SameLine(0, 0);
        InputWithRightButtonsAreaValues[id] = ImGui.GetCursorPosX() - cur1 + ImGui.GetStyle().ItemSpacing.X;
        ImGui.Dummy(Vector2.Zero);
    }

    private static Dictionary<string, Box<string>> InputListValuesString = new();

    /// <summary>
    /// Displays an input list for string values with options to add new items and override display values.
    /// </summary>
    public static void InputListString(string name, List<string> list, Dictionary<string, string>? overrideValues = null)
    {
        if (!InputListValuesString.ContainsKey(name)) InputListValuesString[name] = new("");
        InputList(name, list, overrideValues ?? new Dictionary<string, string>(), delegate
        {
            var buttonSize = ImGuiHelpers.GetButtonSize("Add");
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - buttonSize.X - ImGui.GetStyle().ItemSpacing.X);
            ImGui.InputText($"##{name.Replace("#", "_")}", ref InputListValuesString[name].Value, 100);
            ImGui.SameLine();
            if (ImGui.Button("Add"))
            {
                list.Add(InputListValuesString[name].Value);
                InputListValuesString[name].Value = "";
            }
        });
    }

    private static Dictionary<string, Box<uint>> InputListValuesUint = new();

    /// <summary>
    /// Displays an input list for uint values with options to add new items and override display values.
    /// </summary>
    public static void InputListUint(string name, List<uint> list, Dictionary<uint, string>? overrideValues = null)
    {
        if (!InputListValuesUint.ContainsKey(name)) InputListValuesUint[name] = new(0);
        InputList(name, list, overrideValues ?? new Dictionary<uint, string>(), delegate
        {
            var buttonSize = ImGuiHelpers.GetButtonSize("Add");
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - buttonSize.X - ImGui.GetStyle().ItemSpacing.X);
            ImGuiHelper.InputUint($"##{name.Replace("#", "_")}", ref InputListValuesUint[name].Value);
            ImGui.SameLine();
            if (ImGui.Button("Add"))
            {
                list.Add(InputListValuesUint[name].Value);
                InputListValuesUint[name].Value = 0;
            }
        });
    }

    /// <summary>
    /// Displays an input list with options to add, delete, or clear items, supporting optional override values for display.
    /// </summary>
    public static void InputList<T>(string name, List<T> list, Dictionary<T, string> overrideValues, System.Action addFunction) where T : notnull
    {
#pragma warning disable S3358 // Ternary operators should not be nested
        var text = list.Count == 0 ? "- No values -" : (list.Count == 1 ? $"{(overrideValues != null && overrideValues.ContainsKey(list[0]) ? overrideValues[list[0]] : list[0])}" : $"- {list.Count} elements -");
#pragma warning restore S3358 // Ternary operators should not be nested
        if (ImGui.BeginCombo(name, text))
        {
            addFunction();
            var rem = -1;
            for (var i = 0; i < list.Count; i++)
            {
                var id = $"{name}ECommonsDeleItem{i}";
                var x = list[i];
                ImGui.Selectable($"{(overrideValues != null && overrideValues.ContainsKey(x) ? overrideValues[x] : x)}");
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup(id);
                }
                if (ImGui.BeginPopup(id))
                {
                    if (ImGui.Selectable("Delete##ECommonsDeleItem"))
                    {
                        rem = i;
                    }
                    if (ImGui.Selectable("Clear (hold shift+ctrl)##ECommonsDeleItem")
                        && ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl)
                    {
                        rem = -2;
                    }
                    ImGui.EndPopup();
                }
            }
            if (rem > -1)
            {
                list.RemoveAt(rem);
            }
            if (rem == -2)
            {
                list.Clear();
            }
            ImGui.EndCombo();
        }
    }
    #endregion

    #region Keys
    /// <summary>
    /// Checks if a key is pressed, optionally allowing for repeat behavior.
    /// </summary>
    public static bool IsKeyPressed(int key, bool repeat)
    {
        byte repeat2 = (byte)(repeat ? 1 : 0);
        return ImGuiNative.igIsKeyPressed((ImGuiKey)key, repeat2) != 0;
    }
    #endregion

    #region LayOut
    /// <summary>
    /// Adds horizontal spacing by moving the cursor position by a specified number of pixels, optionally accounting for global scale.
    /// </summary>
    public static void Spacing(float pix = 10f, bool accountForScale = true)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (accountForScale ? pix : pix * ImGuiHelpers.GlobalScale));
    }

    /// <summary>
    /// Scales the given float value by the global scale factor.
    /// </summary>
    public static float Scale(this float f)
    {
        return f * ImGuiHelpers.GlobalScale;
    }

    private static readonly Dictionary<string, float> CenteredLineWidths = new();

    /// <summary>
    /// Centers a line of UI elements based on their calculated width and draws them.
    /// </summary>
    public static void ImGuiLineCentered(string id, System.Action func)
    {
        if (CenteredLineWidths.TryGetValue(id, out var dims))
        {
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2 - dims / 2);
        }
        var oldCur = ImGui.GetCursorPosX();
        func();
        ImGui.SameLine(0, 0);
        CenteredLineWidths[id] = ImGui.GetCursorPosX() - oldCur;
        ImGui.Dummy(Vector2.Zero);
    }

    /// <summary>
    /// Sets the width of the next item to the full available content region width, with an optional modifier.
    /// </summary>
    public static void SetNextItemFullWidth(int mod = 0)
    {
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X + mod);
    }

    /// <summary>
    /// Sets the width of the next item as a percentage of the available content region width, with an optional modifier.
    /// </summary>
    public static void SetNextItemWidth(float percent, int mod = 0)
    {
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * percent + mod);
    }
    #endregion

    #region Measurement
    /// <summary>
    /// Measures the width of an item drawn by the provided action, optionally including spacing.
    /// </summary>
    public static float Measure(Action func, bool includeSpacing = true)
    {
        var pos = ImGui.GetCursorPosX();
        func();
        ImGui.SameLine(0, 0);
        var diff = ImGui.GetCursorPosX() - pos;
        ImGui.Dummy(Vector2.Zero);
        return diff + (includeSpacing ? ImGui.GetStyle().ItemSpacing.X : 0);
    }
    #endregion

    #region Memory Management
    internal static unsafe byte* Allocate(int byteCount)
    {
        return (byte*)(void*)Marshal.AllocHGlobal(byteCount);
    }

    internal static unsafe void Free(byte* ptr)
    {
        Marshal.FreeHGlobal((IntPtr)ptr);
    }
    #endregion

    #region Pop-ups
    /// <summary>
    /// Begins a popup positioned next to the current element.
    /// </summary>
    public static bool BeginPopupNextToElement(string popupId)
    {
        ImGui.SameLine(0, 0);
        var pos = ImGui.GetCursorScreenPos();
        ImGui.Dummy(Vector2.Zero);
        ImGui.SetNextWindowPos(pos, ImGuiCond.Appearing);
        return ImGui.BeginPopup(popupId);
    }

    private static void CopyCommand(string command)
    {
        ImGui.SetClipboardText(command);
        Notify.Success($"\"{command}\" copied to clipboard.");
    }

    private static readonly SortedList<string, bool> LastChecked = new();
    private static void ExecuteHotKeys(Action action, params VirtualKey[] keys)
    {
        if (action == null) return;
        var name = string.Join(' ', keys);

        if (!LastChecked.TryGetValue(name, out var last)) last = false;
#pragma warning disable S6603 // The collection-specific "TrueForAll" method should be used instead of the "All" extension
        var now = keys.All(k => Svc.KeyState[k]);
#pragma warning restore S6603 // The collection-specific "TrueForAll" method should be used instead of the "All" extension
        LastChecked[name] = now;

        if (!last && now) action();
    }

    private static void DrawHotKeys(string name, Action action, params string[] keys)
    {
        if (action == null) return;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        if (ImGui.Selectable(name))
        {
            action();
            ImGui.CloseCurrentPopup();
        }

        ImGui.TableNextColumn();
        ImGui.TextDisabled(string.Join(' ', keys));
    }
    #endregion

    #region Sliders
    /// <summary>
    /// Displays ImGui.SliderFloat for internal int value.
    /// </summary>
    /// <param name="id">ImGui ID</param>
    /// <param name="value">Integer value</param>
    /// <param name="min">Minimal value</param>
    /// <param name="max">Maximum value</param>
    /// <param name="divider">Value is divided by divider before being presented to user</param>
    /// <returns></returns>
    public static bool SliderIntAsFloat(string id, ref int value, int min, int max, float divider = 1000)
    {
        var f = (float)value / divider;
        var ret = ImGui.SliderFloat(id, ref f, (float)min / divider, (float)max / divider);
        if (ret)
        {
            value = (int)(f * divider);
        }
        return ret;
    }
    #endregion

    #region Spacing
    /// <summary>
    /// Draws an item centered or left-aligned within the specified width of the whole content region.
    /// </summary>
    internal static void DrawItemMiddle(Action drawAction, float wholeWidth, float width, bool leftAlign = true)
    {
        if (drawAction == null) return;
        var distance = (wholeWidth - width) / 2;
        if (leftAlign) distance = MathF.Max(distance, 0);
        ImGui.SetCursorPosX(distance);
        drawAction();
    }

    /// <summary>
    /// Draws an item centered or left-aligned within the available content region width.
    /// </summary>
    internal static void DrawItemMiddle(Action drawAction, float width, bool leftAlign = true)
    {
        if (drawAction == null) return;

        // Get the available content width
        var availableWidth = ImGui.GetContentRegionAvail().X;

        // Calculate the distance to center the item
        var distance = (availableWidth - width) / 2;

        // Ensure the item is left-aligned if leftAlign is true
        if (leftAlign) distance = MathF.Max(distance, 0);

        // Set the cursor position to the calculated distance
        ImGui.SetCursorPosX(distance);

        // Execute the draw action
        drawAction();
    }

    /// <summary>
    /// Gets the width of the content region of the current window.
    /// </summary>
    public static float GetWindowContentRegionWidth()
    {
        return ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;
    }

    internal static void SetNextWidthWithName(string name)
    {
        ImGui.SetNextItemWidth(Math.Max(80 * ImGuiHelpers.GlobalScale, ImGui.CalcTextSize(name).X + 30 * ImGuiHelpers.GlobalScale));
    }

    /// <summary>
    /// Simple way of adding some space between elements. (Uses 3 ImGui.Spacing's).
    /// </summary>
    public static void TripleSpacing()
    {
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Spacing();
    }

    /// <summary>
    /// Adds spacing before and after a separator line.
    /// </summary>
    public static void SpacingWithSeperator()
    {
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }
    #endregion

    #region Structured Data

    /// <summary>
    /// Creates a collapsing header with a label.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="contentAction"></param>
    /// <param name="defaultOpen"></param>
    /// <returns></returns>
    public static bool CollapsingHeaderWithContent(string label, Action contentAction, bool defaultOpen = false)
    {
        if (ImGui.CollapsingHeader(label, defaultOpen ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None))
        {
            contentAction();
            return true;
        }
        return false;
    }

    #endregion Structured Data

    #region Tabs
    /// <summary>
    /// Creates a tab bar with the given tabs, optionally transparent, and defaulting to non-transparent if not specified.
    /// </summary>
    public static void EzTabBar(string id, params (string name, System.Action function, Vector4? color, bool child)[] tabs) => EzTabBar(id, false, tabs);

    /// <summary>
    /// Creates a tab bar with the given tabs and optional styling and behavior.
    /// </summary>
    public static void EzTabBar(string id, bool KoFiTransparent, params (string name, System.Action function, Vector4? color, bool child)[] tabs)
    {
        ImGui.BeginTabBar(id);
        foreach (var x in tabs)
        {
            if (x.name == null) continue;
            if (x.color != null)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, x.color.Value);
            }
            if (ImGui.BeginTabItem(x.name))
            {
                if (x.color != null)
                {
                    ImGui.PopStyleColor();
                }
                if (x.child) ImGui.BeginChild(x.name + "child");
                x.function();
                if (x.child) ImGui.EndChild();
                ImGui.EndTabItem();
            }
            else
            {
                if (x.color != null)
                {
                    ImGui.PopStyleColor();
                }
            }
        }
        if (KoFiTransparent) KoFiButton.RightTransparentTab();
        ImGui.EndTabBar();
    }

    /// <summary>
    /// Begins a tab item with the given label and flags, handling UTF-8 conversion and memory allocation.
    /// </summary>
    public static unsafe bool BeginTabItem(string label, ImGuiTabItemFlags flags)
    {
        int num = 0;
        byte* ptr;
        if (label != null)
        {
            num = Encoding.UTF8.GetByteCount(label);
            ptr = Allocate(num + 1);
            int utf = GetUtf8(label, ptr, num);
            ptr[utf] = 0;
        }
        else
        {
            ptr = null;
        }

        byte* p_open2 = null;
        byte num2 = ImGuiNative.igBeginTabItem(ptr, p_open2, flags);
        if (num > 2048)
        {
            Free(ptr);
        }
        return num2 != 0;
    }
    #endregion

    #region Tables
    /// <summary>
    /// Draws equally sized columns without ability to resize
    /// </summary>
    /// <param name="id">Unique ImGui ID</param>
    /// <param name="values">List of actions for each column</param>
    public static void EzTableColumns(string id, Action[] values)
    {
        if (values.Length == 1)
        {
            GenericHelpers.Safe(values[0]);
        }
        else
        {
            if (ImGui.BeginTable(id, values.Length, ImGuiTableFlags.SizingStretchSame))
            {
                foreach (Action action in values)
                {
                    ImGui.TableNextColumn();
                    GenericHelpers.Safe(action);
                }
                ImGui.EndTable();
            }
        }
    }

    /// <summary>
    /// Advances to the next table row and adjusts the height to ensure it does not exceed the specified maximum height.
    /// </summary>
    public static void TableNextRowWithMaxHeight(float maxRowHeight)
    {
        ImGui.TableNextRow();
        float currentRowHeight = ImGui.GetContentRegionAvail().Y;
        if (currentRowHeight > maxRowHeight)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + currentRowHeight - maxRowHeight);
        }
    }

    /// <summary>
    /// Sets up table columns with the given headers and displays the header row.
    /// </summary>
    public static void TableSetupHeaders(params string[] headers)
    {
        foreach (string header in headers)
        {
            ImGui.TableSetupColumn(header);
        }
        ImGui.TableHeadersRow();
    }

    /// <summary>
    /// Sets up table columns with the given headers, flags, and initial widths, then displays the header row.
    /// </summary>
    public static void TableSetupHeaders(params (string header, ImGuiTableColumnFlags flags, float initialWidth)[] headers)
    {
        foreach (var (header, flags, initialWidth) in headers)
        {
            ImGui.TableSetupColumn(header, flags, initialWidth);
        }
        ImGui.TableHeadersRow();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="description"></param>
    /// <param name="value"></param>
    public static void AddTableRow(string description, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn(); ImGui.Text(description);
        ImGui.TableNextColumn(); ImGui.Text(value);
    }

    /// <summary>
    /// Adds a new row to the table with the given descriptions and values for four columns.
    /// </summary>
    public static void AddTableRow(string description, string value, string description2, string value2)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn(); ImGui.Text(description);
        ImGui.TableNextColumn(); ImGui.Text(value);
        ImGui.TableNextColumn(); ImGui.Text(description2);
        ImGui.TableNextColumn(); ImGui.Text(value2);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="description"></param>
    /// <param name="value"></param>
    public static void AddTableRow(string description, bool value)
    {
        Vector4 color = value ? EColor.GreenBright : EColor.RedBright; // Green for true, red for false
        string valueText = value.ToString();

        ImGui.TableNextRow();
        ImGui.TableNextColumn(); ImGui.Text(description);
        ImGui.TableNextColumn(); ImGui.TextColored(color, valueText);
    }

    /// <summary>
    /// Adds a new row to the table with the given description and value.
    /// </summary>
    public static void AddTableRow(string description, byte value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn(); ImGui.Text(description);
        ImGui.TableNextColumn(); ImGui.Text(value.ToString());
    }

    /// <summary>
    /// The text in both collums wil be colored
    /// </summary>
    /// <param name="description"></param>
    /// <param name="value"></param>
    /// <param name="textColor"></param>
    public static void AddTableRow(string description, string value, Vector4 textColor)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn(); ImGui.TextColored(textColor, description);
        ImGui.TableNextColumn(); ImGui.TextColored(textColor, value);
    }

    /// <summary>
    /// Adds a table row with two columns. The text in the first column will be white, and the text in the second column will be the specified value.
    /// </summary>
    /// <param name="description">The text to display in the first column.</param>
    /// <param name="value">The value to display in the second column.</param>
#pragma warning disable S4144 // Methods should not have identical implementations
    public static void AddTableRowByte(string description, byte value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn(); ImGui.Text(description);
        ImGui.TableNextColumn(); ImGui.Text(value.ToString());
    }

    /// <summary>
    /// First Colum text will be white and text in the second colum will be whatever color is picked
    /// </summary>
    /// <param name="description"></param>
    /// <param name="value"></param>
    /// <param name="textColor"></param>
    public static void AddTableRowColorLast(string description, string value, Vector4 textColor)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn(); ImGui.Text(description);
        ImGui.TableNextColumn(); ImGui.TextColored(textColor, value);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    public static void AddBooleanTableRow(string text, bool value)
    {
        Vector4 color = value ? EColor.GreenBright : EColor.RedBright; // Green for true, red for false
        string valueText = value.ToString();

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(text);
        ImGui.TableNextColumn();
        ImGui.TextColored(color, valueText);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="description"></param>
    /// <param name="byteValue"></param>
    internal static void AddTableRow2(string description, byte byteValue)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn(); ImGui.Text(description);
        ImGui.TableNextColumn(); ImGui.Text(byteValue.ToString());
    }

    /// <summary>
    /// First Colum text will be white and text in the second colum will be whatever color is picked
    /// </summary>
    /// <param name="description"></param>
    /// <param name="tooltip1"></param>
    /// <param name="value"></param>
    /// <param name="textColor"></param>
    /// <param name="tooltip2"></param>
    public static void AddTableRowColorLastWithToolTip(string description, string tooltip1, string value, Vector4 textColor, string tooltip2)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(description);
        ImGuiHelper.Tooltip(tooltip1);
        ImGui.TableNextColumn();
        ImGui.TextColored(textColor, value);
        ImGuiHelper.Tooltip(tooltip2);
    }

    /// <summary>
    /// First Colum text will be white and text in the second colum will be whatever color is picked
    /// </summary>
    /// <param name="description"></param>
    /// <param name="value"></param>
    /// <param name="textColor"></param>
    public static void AddTableRowColorLast2(string description, bool value, Vector4 textColor)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(description);
        ImGui.TableNextColumn();
        ImGui.TextColored(textColor, value.ToString());
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="description"></param>
    /// <param name="value"></param>
    internal static void AddTableRowColorLast(string description, bool value)
    {
        Vector4 color = value ? EasyColor.GreenBright : EasyColor.RedBright; // Green for true, red for false
        string valueText = value.ToString();

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(description);
        ImGui.TableNextColumn();
        ImGui.TextColored(color, valueText);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="description"></param>
    /// <param name="value"></param>
    public static void AddTableRowint(string description, int value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(description);
        ImGui.TableNextColumn();
        ImGui.Text(value.ToString());
    }

    #endregion

    #region Text
    /// <summary>
    /// Aligns text vertically to a standard size button.
    /// </summary>
    /// <param name="col">Color</param>
    /// <param name="s">Text</param>
    public static void TextV(Vector4? col, string s)
    {
        if (col != null) ImGui.PushStyleColor(ImGuiCol.Text, col.Value);
        ImGuiHelper.TextV(s);
        if (col != null) ImGui.PopStyleColor();
    }

    /// <summary>
    /// Aligns text vertically to a standard size button.
    /// </summary>
    /// <param name="s">Text</param>
    public static void TextV(string s)
    {
        var cur = ImGui.GetCursorPos();
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0);
        ImGui.Button("");
        ImGui.PopStyleVar();
        ImGui.SameLine();
        ImGui.SetCursorPos(cur);
        ImGui.TextUnformatted(s);
    }

    /// <summary>
    /// Writes unformatted text
    /// </summary>
    /// <param name="s"></param>
    public static void Text(string s)
    {
        ImGui.TextUnformatted(s);
    }

    /// <summary>
    /// Text with color
    /// </summary>
    /// <param name="vector4colour"></param>
    /// <param name="s"></param>
    public static void Text(Vector4 vector4colour, string s)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, vector4colour);
        ImGui.TextUnformatted(s);
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// Displays text with the specified color.
    /// </summary>
    public static void Text(uint col, string s)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, col);
        ImGui.TextUnformatted(s);
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// Displays wrapped text.
    /// </summary>
    public static void TextWrapped(string s)
    {
        ImGui.PushTextWrapPos();
        ImGui.TextUnformatted(s);
        ImGui.PopTextWrapPos();
    }

    /// <summary>
    /// Displays wrapped text with the given color.
    /// </summary>
    public static void TextWrapped(Vector4 col, string s)
    {
        ImGui.PushTextWrapPos(0);
        Text(col, s);
        ImGui.PopTextWrapPos();
    }

    /// <summary>
    /// Displays text with an underline.
    /// </summary>
    public static void TextUnderlined(string text)
    {
        var size = ImGui.CalcTextSize(text);
        var cur = ImGui.GetCursorScreenPos();
        cur.Y += size.Y;
        ImGui.GetWindowDrawList().PathLineTo(cur);
        cur.X += size.X;
        ImGui.GetWindowDrawList().PathLineTo(cur);
        ImGui.GetWindowDrawList().PathStroke(ImGuiColors.DalamudWhite.ToUint());
        ImGuiHelper.Text(text);
    }

    /// <summary>
    /// Displays underlined text with the given color.
    /// </summary>
    public static void TextUnderlined(uint color, string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        TextUnderlined(text);
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// Displays underlined text with the given color.
    /// </summary>
    public static void TextUnderlined(Vector4 color, string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        TextUnderlined(text);
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// Displays text with the given color and allows copying the text to the clipboard when clicked.
    /// </summary>
    public static void TextCopy(Vector4 col, string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, col);
        TextCopy(text);
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// Displays unformatted text and allows copying the text to the clipboard when clicked.
    /// </summary>
    public static void TextCopy(string text)
    {
        ImGui.TextUnformatted(text);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            ImGui.SetClipboardText(text);
            Notify.Success("Text copied to clipboard");
        }
    }

    /// <summary>
    /// Displays wrapped text with the given color and allows copying the text to the clipboard when clicked.
    /// </summary>
    public static void TextWrappedCopy(Vector4 col, string text)
    {
        TextWrapped(col, text);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            ImGui.SetClipboardText(text);
            Notify.Success("Text copied to clipboard");
        }
    }

    /// <summary>
    /// Displays centered column text with optional underline.
    /// </summary>
    public static void CenterColumnText(string text, bool underlined = false)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetColumnWidth() * 0.5f) - (ImGui.CalcTextSize(text).X * 0.5f));
        if (underlined)
            TextUnderlined(text);
        else
            Text(text);
    }

    /// <summary>
    /// Displays centered column text with the given color and optional underline.
    /// </summary>
    public static void CenterColumnText(Vector4 colour, string text, bool underlined = false)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, colour);
        CenterColumnText(text, underlined);
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// Executes an action with the specified text color.
    /// </summary>
    public static void WithTextColor(Vector4 col, System.Action func)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, col);
        GenericHelpers.Safe(func);
        ImGui.PopStyleColor();
    }

    private static string Msg = "You're not logged in, debug window will not be drawn.";

    /// <summary>
    /// Displays a centered message indicating that the user is not logged in.
    /// </summary>
    public static void NotLoggedIn()
    {
        ImGuiHelper.CenteredText($"{Msg}");
    }

    internal static void TextCentered(string text)
    {
        float win_width = ImGui.GetWindowSize().X;
        float text_width = ImGui.CalcTextSize(text).X;

        float text_indentation = (win_width - text_width) * 0.5f;

        float min_indentation = 20.0f;
        if (text_indentation <= min_indentation)
        {
            text_indentation = min_indentation;
        }
        ImGui.SameLine(text_indentation);
        ImGui.PushTextWrapPos(win_width - text_indentation);
        ImGui.TextWrapped(text);
        ImGui.PopTextWrapPos();
    }

    /// <summary>
    /// Displays centered text with the given color.
    /// </summary>
    public static void TextCentered(Vector4 col, string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, col);
        TextCentered(text);
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// Custom extension for displaying a centered text
    /// </summary>
    /// <param name="text"></param>
    public static void CenteredText(string text)
    {
        float windowWidth = ImGui.GetWindowSize().X;
        float textWidth = ImGui.CalcTextSize(text).X;

        ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
        ImGui.Text(text);
    }

    /// <summary>
    /// Displays two parts of text using ImGui, with the second part in a specified color.
    /// </summary>
    /// <param name="text1">The first part of the text, displayed in default color.</param>
    /// <param name="text2">The second part of the text, displayed in the specified color.</param>
    /// <param name="color">The Vector4 color value for the second part of the text.</param>
    public static void ImGuiColoredText(string text1, string text2, Vector4 color)
    {
        ImGui.Text(text1);
        ImGui.SameLine();

        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.Text(text2);
        ImGui.PopStyleColor();
    }
    #endregion

    #region Text Encoding
    internal static unsafe int GetUtf8(string s, byte* utf8Bytes, int utf8ByteCount)
    {
        fixed (char* chars = s)
        {
            return Encoding.UTF8.GetBytes(chars, s.Length, utf8Bytes, utf8ByteCount);
        }
    }
    #endregion
}

/// <summary>
/// 
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct ImGuiWindow
{
    /// <summary>
    /// 
    /// </summary>
    [FieldOffset(0xC)] public ImGuiWindowFlags Flags;

    /// <summary>
    /// 
    /// </summary>
    [FieldOffset(0xD5)] public byte HasCloseButton;


    /// <summary>
    /// 0x118 is the start of ImGuiWindowTempData
    /// </summary>
    [FieldOffset(0x130)] public Vector2 CursorMaxPos;
}

public static partial class ImGuiHelper
{
    [LibraryImport("cimgui")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial nint igGetCurrentWindow();

    /// <summary>
    /// Gets a pointer to the current ImGui window.
    /// </summary>
    public static unsafe ImGuiWindow* GetCurrentWindow() => (ImGuiWindow*)igGetCurrentWindow();

    /// <summary>
    /// Gets the flags of the current ImGui window.
    /// </summary>
    public static unsafe ImGuiWindowFlags GetCurrentWindowFlags() => GetCurrentWindow()->Flags;

    /// <summary>
    /// Checks if the current ImGui window has a close button.
    /// </summary>
    public static unsafe bool CurrentWindowHasCloseButton() => GetCurrentWindow()->HasCloseButton != 0;
}


/// <summary>
/// Helper focussed on ImGui's tooltips
/// </summary>
public static partial class ImGuiHelper
{
    /// <summary>
    /// Sets and displays a colored text tooltip with the given text and color if the current item is hovered.
    /// </summary>
    public static void ColoredTextTooltip(string text, Vector4 color)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextColored(color, text);
            ImGui.EndTooltip();
        }
    }

    const ImGuiWindowFlags TOOLTIP_FLAG =
          ImGuiWindowFlags.Tooltip |
          ImGuiWindowFlags.NoMove |
          ImGuiWindowFlags.NoSavedSettings |
          ImGuiWindowFlags.NoBringToFrontOnFocus |
          ImGuiWindowFlags.NoDecoration |
          ImGuiWindowFlags.NoInputs |
          ImGuiWindowFlags.AlwaysAutoResize;
    private const string TOOLTIP_ID = "EasyCombat Tooltips";

    /// <summary>
    ///
    /// </summary>
    /// <param name="s"></param>
    public static void Tooltip(string s)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(s);
        }
    }

    /// <summary>
    /// Sets and displays a tooltip with the given text if the current item is hovered.
    /// </summary>
    public static void SetTooltip(string tooltip)
    {
        if (!ImGui.IsItemHovered())
        {
            return;
        }

        ImGui.BeginTooltip();
        ImGui.TextUnformatted(tooltip);
        ImGui.EndTooltip();
    }
}
