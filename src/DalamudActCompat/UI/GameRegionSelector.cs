using Dalamud.Bindings.ImGui;
using DalamudActCompat.Plugin;

namespace DalamudActCompat.UI;

internal static class GameRegionSelector
{
    public static void Draw(UiText text, GameRegionSelection selection, Action<GameRegionMode> setMode)
    {
        // Native layouts are fixed by this independent Taiwan build. An imported
        // mainland preference must not offer a switch to incompatible packet tables.
        ImGui.TextUnformatted($"{text.Get("游戏区域", "Game region")}: {text.Get("台服", "Taiwan")}");
    }
}
