using System.Diagnostics;
using System.Reflection.PortableExecutable;
using Dalamud.Plugin.Services;

namespace DalamudActCompat.Plugin;

internal static class TaiwanOpcodeDiscovery
{
    // The Taiwan BossmodReborn branch identifies this dispatch switch and its stable
    // packet IDs. Decode the on-disk PE with bounds checks instead of reading arbitrary
    // process pointers. Three published TW opcodes must agree before accepting FateInfo.
    // Source: cycleapple/BossmodReborn@b838d4cc, Network/{OpcodeMap,ServerIPC}.cs.
    private const string Signature = "49 8B 40 10 4C 8B 50 38 41 0F B7 42 02 83 C0 ?? 3D ?? ?? ?? ?? 0F 87 ?? ?? ?? ?? 4C 8D 1D ?? ?? ?? ?? 48 98 45 8B 8C 83 ?? ?? ?? ??";

    internal static ushort? TryRead(ISigScanner scanner, IPluginLog log)
    {
        try
        {
            var function = scanner.ScanText(Signature);
            using var process = Process.GetCurrentProcess();
            var module = process.MainModule ?? throw new InvalidDataException("Game module unavailable.");
            using var stream = File.OpenRead(module.FileName);
            using var pe = new PEReader(stream);
            var functionRva = checked((int)(function - module.BaseAddress));
            var opcode = Decode((rva, length) => pe.GetSectionData(rva).GetContent(0, length).ToArray(), functionRva);
            if (opcode is null) throw new InvalidDataException("Taiwan dispatch table anchors do not match.");
            log.Information("Taiwan FateInfo opcode discovered and cross-checked: {Opcode:X4}", opcode.Value);
            return opcode;
        }
        catch (Exception exception)
        {
            log.Warning(exception, "Taiwan FateInfo discovery failed; SilverDasher will retain ActorControlSelf events without guessing a foreign opcode.");
            return null;
        }
    }

    internal static ushort? Decode(Func<int, int, byte[]> read, int functionRva)
    {
        try
        {
            var function = read(functionRva, 44);
            var expected = Signature.Split(' ');
            if (function.Length != expected.Length || expected.Where((value, index) =>
                    value != "??" && Convert.ToByte(value, 16) != function[index]).Any()) return null;
            var first = -(sbyte)function[15];
            var count = checked(BitConverter.ToInt32(function, 17) + 1);
            if (first <= 0 || count <= 0 || count > 2048 || first + count >= 0x8000) return null;
            int Relative(int offset) => checked(functionRva + offset + 4 + BitConverter.ToInt32(function, offset));
            var imageBase = Relative(30);
            var defaultCase = Relative(23);
            var table = checked(imageBase + BitConverter.ToInt32(function, 40));
            var entries = read(table, checked(count * 4));
            var byId = new Dictionary<int, ushort>();
            byte[] prefix = [0x48, 0x8B, 0x01, 0x4D, 0x8D, 0x4A, 0x10, 0x48, 0xFF];
            for (var index = 0; index < count; index++)
            {
                var bodyRva = checked(imageBase + BitConverter.ToInt32(entries, index * 4));
                if (bodyRva == defaultCase) continue;
                var body = read(bodyRva, 14);
                if (!body.AsSpan(0, prefix.Length).SequenceEqual(prefix)) return null;
                var offset = body[9] switch
                {
                    0x60 => body[10],
                    0xA0 => BitConverter.ToInt32(body, 10),
                    _ => -1,
                };
                if (offset < 16 || (offset & 7) != 0 ||
                    !byId.TryAdd((offset >> 3) - 2, checked((ushort)(first + index)))) return null;
            }
            return byId.GetValueOrDefault(224) == 0x325 &&
                   byId.GetValueOrDefault(277) == 0x35D &&
                   byId.GetValueOrDefault(280) == 0x00EB &&
                   byId.TryGetValue(453, out var fateInfo) ? fateInfo : null;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or
                                         OverflowException or BadImageFormatException or IndexOutOfRangeException)
        {
            return null;
        }
    }
}
