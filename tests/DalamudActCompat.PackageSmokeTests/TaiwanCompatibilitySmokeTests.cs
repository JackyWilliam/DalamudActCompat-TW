using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using DalamudActCompat.Plugin;
using DalamudActCompat.Protocol;
using Machina.FFXIV;
using Machina.FFXIV.Headers.Opcodes;

internal static class TaiwanCompatibilitySmokeTests
{
    private static readonly Assembly Overlay = typeof(RainbowMage.OverlayPlugin.PluginMain).Assembly;

    internal static void Run()
    {
        TaiwanOpcodeDiscoverySmokeTests.Run();
        var selection = GameRegionResolver.Resolve(GameRegionMode.Auto, "TraditionalChinese", 7);
        Check(selection.HasDetectedRegion && selection.EffectiveRegion == HostGameRegion.TraditionalChinese &&
              selection.ClientLanguage == HostClientLanguage.TraditionalChinese, "Native TC region was lost.");
        var translatedGlobal = GameRegionResolver.Resolve(GameRegionMode.Auto, "ChineseTraditional", 1);
        Check(translatedGlobal.EffectiveRegion == HostGameRegion.Global, "A translation pack changed the game region.");
        var context = selection.ToHostContext();
        Check(JsonSerializer.Deserialize<HostGameContext>(JsonSerializer.Serialize(context)) == context,
            "TC Host context did not round-trip.");
        var previous = OpcodeManager.Instance.GameRegion;
        try
        {
            IINACT.FfxivActPluginWrapper.ConfigureRegion(Dalamud.Game.ClientLanguage.English,
                regionOverride: GameRegion.TraditionalChinese);
            var opcodes = OpcodeManager.Instance.CurrentOpcodes;
            Check(opcodes["ActorCast"] == 0x35d && opcodes["Ability1"] == 0x1be &&
                  opcodes["ActorMove"] == 553 && opcodes["ActorSetPos"] == 633,
                "TC 2026-09-14 packet table is stale or selected another region.");
            ValidateCustomPacket();
            ValidateCastPacket();
        }
        finally { OpcodeManager.Instance.SetRegion(previous); }
        Console.WriteLine("PASS Taiwan: native region, Host serialization, 2026-09-14 opcodes, movement/cast payloads and wrong-region rejection.");
    }

    internal static void ValidateUnscrambler()
    {
        var previous = OpcodeManager.Instance.GameRegion;
        try
        {
            OpcodeManager.Instance.SetRegion(GameRegion.TraditionalChinese);
            var constants = IINACT.Network.ZoneDownHookManager.GetFallbackVersionConstant(0x100, 4 * 77);
            Check(constants.ObfuscatedOpcodes["ActionEffect01"] == 0x1be &&
                  constants.ObfuscatedOpcodes["ActorCast"] == 0x35d,
                "TC Unscrambler used another region's opcode constants.");
            var decoder = new Unscrambler.Unscramble.Versions.Unscrambler73();
            decoder.Initialize(constants);
            Check(constants.OpcodeKeyTableOffset == 0x100 && constants.OpcodeKeyTableSize == 308,
                "TC decoder lost the runtime-discovered key table.");
        }
        finally { OpcodeManager.Instance.SetRegion(previous); }
        Console.WriteLine("PASS Taiwan: .NET 9 Unscrambler 7.3 initialization and regional constants.");
    }

    private static void ValidateCustomPacket()
    {
        var resource = Overlay.GetManifestResourceNames().Single(name => name.EndsWith("opcodes.jsonc"));
        using var stream = Overlay.GetManifestResourceStream(resource)!;
        using var json = JsonDocument.Parse(stream, new JsonDocumentOptions
            { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        var map = json.RootElement.GetProperty("TraditionalChinese").GetProperty("2026.09.14.0000.0000");
        Check(map.GetProperty("ActorMove").GetProperty("opcode").GetInt32() == 553 &&
              map.GetProperty("MapEffect").GetProperty("opcode").GetInt32() == 593,
            "TC extended network event table is stale.");
        var ns = "RainbowMage.OverlayPlugin.NetworkProcessors.PacketHelper.";
        var packet = Overlay.GetType("RainbowMage.OverlayPlugin.NetworkProcessors.LineActorMove+ActorMove_v655", true)!;
        var headers = new[] { "Global", "CN", "KR", "TC" }.Select(region => Overlay.GetType(ns + "Server_MessageHeader_" + region, true)!).ToArray();
        var generic = Overlay.GetType(ns + "RegionalizedPacketHelper`8", true)!.MakeGenericType(
            headers.SelectMany(header => new[] { header, packet }).ToArray());
        var helper = Activator.CreateInstance(generic, (ushort)473, (ushort)473, (ushort)473, (ushort)553)!;
        var tc = generic.GetProperty("Item")!.GetValue(helper, [GameRegion.TraditionalChinese])!;
        var global = generic.GetProperty("Item")!.GetValue(helper, [GameRegion.Global])!;
        var raw = new byte[48];
        BitConverter.GetBytes(0x40000001u).CopyTo(raw, 4);
        BitConverter.GetBytes((ushort)553).CopyTo(raw, 18);
        BitConverter.GetBytes((ushort)0x4000).CopyTo(raw, 32);
        foreach (var offset in new[] { 38, 40, 42 }) BitConverter.GetBytes((ushort)0x7fff).CopyTo(raw, offset);
        var format = tc.GetType().GetMethod("ToString", [typeof(long), typeof(byte[])])!;
        var line = format.Invoke(tc, [0L, raw]) as string;
        Check(line?.StartsWith("40000001|", StringComparison.Ordinal) == true,
            "TC movement packet did not reach the extended log formatter.");
        Check(global.GetType().GetMethod("ToString", [typeof(long), typeof(byte[])])!.Invoke(global, [0L, raw]) is null && format.Invoke(tc, [0L, raw[..31]]) is null,
            "Wrong-region/truncated movement packet was accepted.");
    }

    private static void ValidateCastPacket()
    {
        // ACT 3.0.3.1 uses global packet layouts with TC opcodes. Exercise the actual
        // regional helper and formatter so a TC->Global opcode fallback cannot pass.
        var wrapper = Overlay.GetType("RainbowMage.OverlayPlugin.NetworkProcessors.LineActorCastExtra+ActorCastExtraPacket", true)!;
        var generic = Overlay.GetType("RainbowMage.OverlayPlugin.NetworkProcessors.PacketHelper.MachinaRegionalizedPacketHelper`1", true)!.MakeGenericType(wrapper);
        object?[] arguments = ["ActorCast", null, null];
        Check(generic.GetMethod("Create")!.Invoke(null, arguments) is true, "TC cast helper could not initialize.");
        var helper = generic.GetProperty("Item")!.GetValue(arguments[1], [GameRegion.TraditionalChinese])!;
        var packetType = (Type)helper.GetType().GetField("packetType")!.GetValue(helper)!;
        Check(packetType.Namespace == "Machina.FFXIV.Headers", "TC cast layout differs from the shipped ACT parser.");
        var value = Activator.CreateInstance(packetType)!;
        packetType.GetField("ActionID")!.SetValue(value, (ushort)0x1234);
        packetType.GetField("Rotation")!.SetValue(value, (ushort)0x4000);
        var raw = new byte[Marshal.SizeOf(packetType)];
        var pointer = Marshal.AllocHGlobal(raw.Length);
        try { Marshal.StructureToPtr(value, pointer, false); Marshal.Copy(pointer, raw, 0, raw.Length); }
        finally { Marshal.FreeHGlobal(pointer); }
        BitConverter.GetBytes(0x40000001u).CopyTo(raw, 4);
        BitConverter.GetBytes((ushort)0x35d).CopyTo(raw, 18);
        var format = helper.GetType().GetMethod("ToString", [typeof(long), typeof(byte[])])!;
        var line = format.Invoke(helper, [0L, raw]) as string;
        Check(line?.StartsWith("40000001|1234|", StringComparison.Ordinal) == true && line.EndsWith("|-1.571", StringComparison.Ordinal),
            "TC cast action/heading was corrupted.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
