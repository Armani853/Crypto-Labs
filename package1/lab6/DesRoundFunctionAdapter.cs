using Lab2;
using Lab4;

namespace Lab6;

public sealed class DesRoundFunctionAdapter : IRoundFunction
{
    public byte[] Transform(byte[] block, byte[] roundKey)
    {
        if (block == null || block.Length != 8 || roundKey == null || roundKey.Length != 8)
            throw new ArgumentException("DEAL round requires 8-byte data and key.");
        DesCipher des = new DesCipher();
        des.SetKey(roundKey);
        return des.EncryptBlock(block);
    }
}
