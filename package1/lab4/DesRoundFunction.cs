using Lab2;

namespace Lab4;

public sealed class DesRoundFunction : IRoundFunction
{
    public byte[] Transform(byte[] block, byte[] roundKey)
    {
        if (block == null || block.Length != 4 || roundKey == null || roundKey.Length != 6)
            throw new ArgumentException("Invalid DES round data.");

        byte[] expanded = PermutationRule.Permute(block, DesTables.E,
            PermutationRule.BitOrder.MsbFirst, PermutationRule.BitNumbering.OneBased);

        for (int i = 0; i < 6; i++)
            expanded[i] ^= roundKey[i];

        byte[] result = new byte[4];
        for (int box = 0; box < 8; box++)
        {
            int value = 0;
            for (int j = 0; j < 6; j++)
            {
                int index = box * 6 + j;
                int bit = (expanded[index / 8] >> (7 - index % 8)) & 1;
                value = (value << 1) | bit;
            }

            int row = ((value & 32) >> 4) | (value & 1);
            int column = (value >> 1) & 15;
            int s = DesTables.S[box, row * 16 + column];
            if (box % 2 == 0)
                result[box / 2] = (byte)(s << 4);
            else
                result[box / 2] |= (byte)s;
        }

        return PermutationRule.Permute(result, DesTables.P,
            PermutationRule.BitOrder.MsbFirst, PermutationRule.BitNumbering.OneBased);
    }
}
