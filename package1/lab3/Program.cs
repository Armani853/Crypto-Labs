
using System;
using System.Linq;
using Lab2;

namespace Lab3;

public class DemoKeyExpansion : IKeyExpansion
{
    public byte[][] GenerateRoundKeys(byte[] key)
    {
        return new byte[][]
        {
            new byte[] { key[0] },
            new byte[] { (byte)(key[0] ^ 1) },
            new byte[] { (byte)(key[0] ^ 2) }
        };
    }
}

public class DemoRoundFunction : IRoundFunction
{
    public byte[] Transform(byte[] block, byte[] roundKey)
    {
        byte[] result = new byte[block.Length];

        for (int i = 0; i < block.Length; i++)
            result[i] = (byte)(block[i] ^ roundKey[0]);

        return result;
    }
}

public static class Program
{
    public static void Main()
    {
        var feistel = new FeistelNetwork(
            new DemoKeyExpansion(),
            new DemoRoundFunction(),
            blockSize: 8);

        feistel.SetKey(new byte[] { 0x0F });

        byte[] original = { 10, 20, 30, 40, 50, 60, 70, 80 };

        byte[] encrypted = feistel.EncryptBlock(original);
        byte[] decrypted = feistel.DecryptBlock(encrypted);

        Console.WriteLine($"Исходный:     {Convert.ToHexString(original)}");
        Console.WriteLine($"Зашифрован:   {Convert.ToHexString(encrypted)}");
        Console.WriteLine($"Расшифрован:  {Convert.ToHexString(decrypted)}");

        Console.WriteLine($"Проверка: {original.SequenceEqual(decrypted)}");

        if (!original.SequenceEqual(decrypted))
            throw new Exception("Ошибка сети Фейстеля!");
    }
}
