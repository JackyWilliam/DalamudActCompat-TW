namespace DalamudActCompat.Fflogs;

internal static class TaiwanFflogsEncounterTable
{
    // The pinned Taiwan client is on Cruiserweight. Never fall back to the
    // newer CN/global tier or its cached population when TW returns no samples.
    internal const int ZoneId = 68;
    internal const string ZoneName = "AAC Cruiserweight";
    internal static readonly FflogsRankingScope Scope = new("TW", "TW", "TW", null);
    private static readonly string[] Names =
        ["Dancing Green", "Sugar Riot", "Brute Abombinator", "Howling Blade"];

    internal static bool IsSupportedEncounter(int encounterId) => encounterId is >= 97 and <= 100;

    internal static bool IsSupportedRanking(int encounterId, int difficulty)
        => IsSupportedEncounter(encounterId) && difficulty is 100 or 101;

    internal static bool TryResolve(uint territoryId, int phase, out CurrentFflogsEncounter encounter)
    {
        // Cactbot v0.37.5 zone_id.ts: M5N/S through M8N/S are 1256..1263.
        // FFLogs ranks the complete Howling Blade fight under encounter 100.
        if (territoryId is < 1256 or > 1263)
        {
            encounter = null!;
            return false;
        }
        var index = (int)(territoryId - 1256);
        encounter = new(territoryId, 1, 97 + index / 2, Names[index / 2], 100 + index % 2);
        return true;
    }
}
