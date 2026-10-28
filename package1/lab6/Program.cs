using Lab6;

foreach (int keyLength in new[] { 16, 24, 32 })
{
    var cipher = new DealCipher();
    cipher.SetKey(new byte[keyLength]);

    byte[] original = new byte[16];
    byte[] encrypted = cipher.EncryptBlock(original);
    byte[] decrypted = cipher.DecryptBlock(encrypted);

    if (!original.SequenceEqual(decrypted))
        throw new Exception($"DEAL-{keyLength * 8} test failed.");

    Console.WriteLine($"DEAL-{keyLength * 8}: {Convert.ToHexString(encrypted)}; decrypt OK");
}
