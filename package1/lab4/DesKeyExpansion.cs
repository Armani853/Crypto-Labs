using Lab2;

namespace Lab4;

public sealed class DesKeyExpansion : IKeyExpansion
{
    public byte[][] GenerateRoundKeys(byte[] key)
    {
        if (key == null || key.Length != 8)
            throw new ArgumentException("DES key must contain 8 bytes.");

        byte[] bits = PermutationRule.Permute(key, DesTables.PC1,
            PermutationRule.BitOrder.MsbFirst, PermutationRule.BitNumbering.OneBased);

        bool[] left = new bool[28];
        bool[] right = new bool[28];
        for (int i = 0; i < 28; i++)
        {
            left[i] = GetBit(bits, i);
            right[i] = GetBit(bits, i + 28);
        }

        byte[][] keys = new byte[16][];
        for (int round = 0; round < 16; round++)
        {
            left = Shift(left, DesTables.Shifts[round]);
            right = Shift(right, DesTables.Shifts[round]);
            byte[] combined = new byte[7];
            for (int i = 0; i < 28; i++)
            {
                SetBit(combined, i, left[i]);
                SetBit(combined, i + 28, right[i]);
            }
            keys[round] = PermutationRule.Permute(combined, DesTables.PC2,
                PermutationRule.BitOrder.MsbFirst, PermutationRule.BitNumbering.OneBased);
        }
        return keys;
    }

    private static bool[] Shift(bool[] bits, int count)
    {
        bool[] result = new bool[bits.Length];
        for (int i = 0; i < bits.Length; i++)
            result[i] = bits[(i + count) % bits.Length];
        return result;
    }

    private static bool GetBit(byte[] bytes, int index)
    {
        return (bytes[index / 8] & (1 << (7 - index % 8))) != 0;
    }

    private static void SetBit(byte[] bytes, int index, bool value)
    {
        if (value)
            bytes[index / 8] |= (byte)(1 << (7 - index % 8));
    }
}
