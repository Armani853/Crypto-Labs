using Lab2;
using Lab4;

namespace Lab6;

public sealed class DealKeyExpansion : IKeyExpansion
{
    public byte[][] GenerateRoundKeys(byte[] key)
    {
        if (key == null || (key.Length != 16 && key.Length != 24 && key.Length != 32))
            throw new ArgumentException("DEAL key must contain 16, 24 or 32 bytes.");

        int parts = key.Length / 8;
        int rounds = key.Length == 32 ? 8 : 6;

        DesCipher des = new DesCipher();
        des.SetKey(Convert.FromHexString("0123456789ABCDEF"));
        byte[][] keys = new byte[rounds][];
        byte[] previous = new byte[8];

        for (int i = 0; i < rounds; i++)
        {
            byte[] input = new byte[8];
            for (int j = 0; j < 8; j++)
                input[j] = (byte)(key[(i % parts) * 8 + j] ^ previous[j]);

            if (i >= parts)
                input[0] ^= (byte)(0x80 >> (i - parts));

            keys[i] = des.EncryptBlock(input);
            previous = keys[i];
        }
        return keys;
    }
}
