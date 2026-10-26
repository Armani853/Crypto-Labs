using Lab2;
using Lab3;

namespace Lab4;

public sealed class DesCipher : IBlockCipher
{
    private readonly FeistelNetwork _network = new(new DesKeyExpansion(), new DesRoundFunction(), 8);

    public int BlockSize => 8;

    public void SetKey(byte[] key) => _network.SetKey(key);

    public byte[] EncryptBlock(byte[] block)
    {
        if (block == null || block.Length != 8)
            throw new ArgumentException("DES block must contain 8 bytes.");
        byte[] input = PermutationRule.Permute(block, DesTables.IP,
            PermutationRule.BitOrder.MsbFirst, PermutationRule.BitNumbering.OneBased);
        byte[] output = _network.EncryptBlock(input);
        return PermutationRule.Permute(output, DesTables.FP,
            PermutationRule.BitOrder.MsbFirst, PermutationRule.BitNumbering.OneBased);
    }

    public byte[] DecryptBlock(byte[] block)
    {
        if (block == null || block.Length != 8)
            throw new ArgumentException("DES block must contain 8 bytes.");
        byte[] input = PermutationRule.Permute(block, DesTables.IP,
            PermutationRule.BitOrder.MsbFirst, PermutationRule.BitNumbering.OneBased);
        byte[] output = _network.DecryptBlock(input);
        return PermutationRule.Permute(output, DesTables.FP,
            PermutationRule.BitOrder.MsbFirst, PermutationRule.BitNumbering.OneBased);
    }
}
