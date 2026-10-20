using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lab2;

public sealed class XorTestCipher : IBlockCipher
{
    private byte[] _key = new byte[8];

    public int BlockSize => 8;

    public void SetKey(byte[] key)
    {
        if (key.Length != 8)
            throw new ArgumentException("Key must be 8 bytes");

        _key = (byte[])key.Clone();
    }

    public byte[] EncryptBlock(byte[] block)
    {
        byte[] result = new byte[8];

        for (int i = 0; i < 8; i++)
            result[i] = (byte)(block[i] ^ _key[i]);

        return result;
    }

    public byte[] DecryptBlock(byte[] block) => EncryptBlock(block);
}

public static class Program
{
    public static async Task Main()
    {
        byte[] key = { 1, 2, 3, 4, 5, 6, 7, 8 };
        byte[] iv = { 11, 22, 33, 44, 55, 66, 77, 88 };
        byte[] delta = { 0, 0, 0, 0, 0, 0, 0, 7 };

        byte[] data = { 10, 20, 30, 40, 50, 0, 0 };

        foreach (CipherMode mode in Enum.GetValues<CipherMode>())
        foreach (PaddingMode padding in Enum.GetValues<PaddingMode>())
        {
            var ctx = new CipherContext(
                new XorTestCipher(), key, mode, padding, iv,
                mode == CipherMode.RandomDelta
                    ? new object[] { delta }
                    : Array.Empty<object>());

            byte[] encrypted = await ctx.EncryptAsync(data);
            byte[] decrypted = await ctx.DecryptAsync(encrypted);

            Console.WriteLine(
                $"{mode} + {padding}: {data.SequenceEqual(decrypted)}");
        }

        await File.WriteAllBytesAsync("input.bin", data);

        var fileCtx = new CipherContext(
            new XorTestCipher(), key,
            CipherMode.CBC, PaddingMode.PKCS7, iv);

        await fileCtx.EncryptAsync("input.bin", "encrypted.bin");
        await fileCtx.DecryptAsync("encrypted.bin", "restored.bin");

        Console.WriteLine(
            "Files equal: " +
            File.ReadAllBytes("input.bin")
                .SequenceEqual(File.ReadAllBytes("restored.bin")));
    }
}