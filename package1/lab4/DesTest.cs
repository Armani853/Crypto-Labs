using Lab4;

public static class DesTest
{
    public static bool Check()
    {
        DesCipher des = new DesCipher();
        des.SetKey(Convert.FromHexString("133457799BBCDFF1"));
        byte[] source = Convert.FromHexString("0123456789ABCDEF");
        byte[] result = des.EncryptBlock(source);
        if (Convert.ToHexString(result) != "85E813540F0AB405" ||
            !des.DecryptBlock(result).SequenceEqual(source))
            return false;

        byte[][] keys = new DesKeyExpansion().GenerateRoundKeys(
            Convert.FromHexString("133457799BBCDFF1"));
        if (keys.Length != 16 || Convert.ToHexString(keys[0]) != "1B02EFFC7072")
            return false;

        des.SetKey(Convert.FromHexString("0123456789ABCDEF"));
        byte[] secondSource = Convert.FromHexString("1234567890ABCDEF");
        byte[] secondResult = des.EncryptBlock(secondSource);
        return Convert.ToHexString(secondResult) == "BD661569AE874E25" &&
               des.DecryptBlock(secondResult).SequenceEqual(secondSource);
    }
}
