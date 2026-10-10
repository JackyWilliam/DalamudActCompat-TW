using DalamudActCompat.Plugin;

internal static class TaiwanOpcodeDiscoverySmokeTests
{
    internal static void Run()
    {
        var image = new byte[0x6000];
        const int function = 0x100;
        const int table = 0x1000;
        const int first = 0x60;
        const int count = 0x340;
        byte[] code = [0x49,0x8B,0x40,0x10,0x4C,0x8B,0x50,0x38,0x41,0x0F,0xB7,0x42,0x02,0x83,0xC0,0xA0,
            0x3D,0,0,0,0,0x0F,0x87,0,0,0,0,0x4C,0x8D,0x1D,0,0,0,0,0x48,0x98,0x45,0x8B,0x8C,0x83,0,0,0,0];
        code.CopyTo(image, function);
        void Int(int address, int value) => BitConverter.GetBytes(value).CopyTo(image, address);
        Int(function + 17, count - 1);
        Int(function + 23, 0x500 - function - 27);
        Int(function + 30, -function - 34);
        Int(function + 40, table);
        for (var index = 0; index < count; index++) Int(table + index * 4, 0x500);
        // Fixture values represent distinct packet IDs, not a hardcoded FateInfo opcode.
        var packets = new[] { (0x325, 224), (0x35D, 277), (0xEB, 280), (0x250, 453) };
        for (var index = 0; index < packets.Length; index++)
        {
            var (opcode, id) = packets[index];
            var body = 0x4000 + index * 16;
            Int(table + (opcode - first) * 4, body);
            byte[] jump = [0x48,0x8B,0x01,0x4D,0x8D,0x4A,0x10,0x48,0xFF,0xA0];
            jump.CopyTo(image, body);
            Int(body + 10, (id + 2) * 8);
        }
        ushort? Decode() => TaiwanOpcodeDiscovery.Decode((rva, length) => image.AsSpan(rva, length).ToArray(), function);
        Check(Decode() == 0x250, "Verified TW dispatch did not resolve FateInfo.");
        Int(table + (0x250 - first) * 4, 0x500);
        Int(table + (0x260 - first) * 4, 0x4030);
        Check(Decode() == 0x260, "Discovery ignored the active game's opcode assignment.");
        Int(table + (0x325 - first) * 4, 0x500);
        Check(Decode() is null, "A table without the TW ActorControl anchor was accepted.");
        Int(table + (0x325 - first) * 4, 0x4000);
        Int(function + 40, image.Length - 2);
        Check(Decode() is null, "An out-of-bounds jump table was accepted.");
        Int(function + 40, table);
        Int(function + 17, int.MaxValue);
        Check(Decode() is null, "An overflowing table length was accepted.");
        Console.WriteLine("PASS Taiwan FateInfo discovery: dispatch fixtures, changed opcode, region anchors, truncated table and overflow rejection.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
