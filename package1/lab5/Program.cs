using System.Security.Cryptography;
using Lab2;
using CipherMode = Lab2.CipherMode;
using PaddingMode = Lab2.PaddingMode;
using Lab4;

if (!DesTest.Check())
    throw new Exception("DES test vector failed.");
Console.WriteLine("DES test vector: OK");

await Run(new DesCipher(), RandomNumberGenerator.GetBytes(8), "DES");

static async Task Run(IBlockCipher cipher, byte[] key, string name)
{
    Directory.CreateDirectory("samples");
    string textFile = Path.Combine("samples", "text.txt");
    string sourceFile = Path.Combine("samples", "source.cs");
    if (!File.Exists(textFile))
        await File.WriteAllTextAsync(textFile, "Sample text file");
    if (!File.Exists(sourceFile))
        File.Copy("Program.cs", sourceFile);

    string[] names = Directory.GetFiles("samples")
        .Where(path => !path.EndsWith(".enc") && !path.EndsWith(".restored"))
        .ToArray();

    foreach (CipherMode mode in Enum.GetValues<CipherMode>())
    {
        byte[] iv = RandomNumberGenerator.GetBytes(cipher.BlockSize);
        byte[] delta = new byte[cipher.BlockSize];
        delta[^1] = 1;
        var context = new CipherContext(cipher, key, mode, PaddingMode.PKCS7,
            mode == CipherMode.ECB ? null : iv,
            mode == CipherMode.RandomDelta ? new object[] { delta } : Array.Empty<object>());

        byte[] original = RandomNumberGenerator.GetBytes(1003);
        byte[] encrypted = await context.EncryptAsync(original);
        byte[] restored = await context.DecryptAsync(encrypted);
        if (!original.SequenceEqual(restored))
            throw new Exception($"{name} {mode} bytes failed.");

        foreach (string file in names)
        {
            string input = file;
            string enc = input + ".enc";
            string dec = input + ".restored";
            await context.EncryptAsync(input, enc);
            await context.DecryptAsync(enc, dec);
            byte[] sourceBytes = await File.ReadAllBytesAsync(input);
            byte[] restoredBytes = await File.ReadAllBytesAsync(dec);
            if (!sourceBytes.SequenceEqual(restoredBytes))
                throw new Exception($"{name} {mode} {file} failed.");
        }
        Console.WriteLine($"{name} {mode}: bytes and files OK");
    }
}
