using Lab2;
using Lab3;

namespace Lab6;

public sealed class DealCipher : IBlockCipher
{
    private readonly FeistelNetwork _network = new(new DealKeyExpansion(), new DesRoundFunctionAdapter(), 16);

    public int BlockSize => 16;

    public void SetKey(byte[] key) => _network.SetKey(key);

    public byte[] EncryptBlock(byte[] block) => SwapHalves(_network.EncryptBlock(block));

    public byte[] DecryptBlock(byte[] block) => _network.DecryptBlock(SwapHalves(block));

    private static byte[] SwapHalves(byte[] block)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Length != 16)
            throw new ArgumentException("DEAL block must contain 16 bytes.", nameof(block));

        byte[] result = new byte[16];
        Array.Copy(block, 8, result, 0, 8);
        Array.Copy(block, 0, result, 8, 8);
        return result;
    }
}
