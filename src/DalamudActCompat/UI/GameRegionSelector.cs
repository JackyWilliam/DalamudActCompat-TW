using Dalamud.Bindings.ImGui;
using DalamudActCompat.Plugin;
using DalamudActCompat.UI.Kit;

namespace DalamudActCompat.UI;

internal static class GameRegionSelector
{
    public static void Draw(UiText text, GameRegionSelection selection, Action<GameRegionMode> setMode)
    {
        // Native layouts are fixed by this independent Taiwan build. An imported
        // mainland preference must not offer a switch to incompatible packet tables.
        ImGui.TextUnformatted($"{text.Get("游戏区域", "Game region")}: {text.Get("台服", "Taiwan")}");
    }

    // 1.0 stacked field. Kept as a disabled single-entry select so the home grid lines up with
    // the other fields, while still never offering the CN/Global tables to a Taiwan client.
    public static void DrawField(UiText text, GameRegionSelection selection, Action<GameRegionMode> setMode)
    {
        DactUi.BeginField(text.Get("游戏区域", "Game region"));
        var index = 0;
        DactUi.Select("game-region", [text.Get("台服", "Taiwan")], ref index, disabled: true);
        DactUi.EndField();
    }
}
