
using System;
using Lab2;

namespace Lab3;

public sealed class FeistelNetwork : IBlockCipher
{
    private readonly IKeyExpansion _keyExpansion;
    private readonly IRoundFunction _roundFunction;

    private byte[][]? _roundKeys;

    public int BlockSize { get; }

    public FeistelNetwork(
        IKeyExpansion keyExpansion,
        IRoundFunction roundFunction,
        int blockSize)
    {
        _keyExpansion = keyExpansion
            ?? throw new ArgumentNullException(nameof(keyExpansion));

        _roundFunction = roundFunction
            ?? throw new ArgumentNullException(nameof(roundFunction));

        if (blockSize <= 0 || blockSize % 2 != 0)
        {
            throw new ArgumentException(
                "Block size must be positive and even.");
        }

        BlockSize = blockSize;
    }

    public void SetKey(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);

        byte[][] keys = _keyExpansion.GenerateRoundKeys(
            (byte[])key.Clone());

        if (keys == null || keys.Length == 0)
        {
            throw new InvalidOperationException(
                "Key expansion returned no round keys.");
        }

        byte[][] savedKeys = new byte[keys.Length][];

        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i] == null || keys[i].Length == 0)
            {
                throw new InvalidOperationException(
                    "Invalid round key.");
            }

            savedKeys[i] = (byte[])keys[i].Clone();
        }

        _roundKeys = savedKeys;
    }

    public byte[] EncryptBlock(byte[] block)
    {
        return ProcessBlock(block, decrypt: false);
    }

    public byte[] DecryptBlock(byte[] block)
    {
        return ProcessBlock(block, decrypt: true);
    }

    private byte[] ProcessBlock(byte[] block, bool decrypt)
    {
        ArgumentNullException.ThrowIfNull(block);

        if (block.Length != BlockSize)
        {
            throw new ArgumentException(
                $"Block must contain {BlockSize} bytes.");
        }

        if (_roundKeys == null)
        {
            throw new InvalidOperationException(
                "Call SetKey before encryption.");
        }

        int halfSize = BlockSize / 2;

        byte[] left = new byte[halfSize];
        byte[] right = new byte[halfSize];

        Array.Copy(block, 0, left, 0, halfSize);
        Array.Copy(block, halfSize, right, 0, halfSize);

        for (int i = 0; i < _roundKeys.Length; i++)
        {
            int keyIndex = decrypt
                ? _roundKeys.Length - 1 - i
                : i;

            byte[] roundKey = _roundKeys[keyIndex];

            byte[] f = _roundFunction.Transform(
                (byte[])right.Clone(),
                (byte[])roundKey.Clone());

            if (f == null || f.Length != halfSize)
            {
                throw new InvalidOperationException(
                    "Round function returned invalid size.");
            }

            byte[] newLeft = right;

            byte[] newRight = new byte[halfSize];

            for (int j = 0; j < halfSize; j++)
            {
                newRight[j] = (byte)(left[j] ^ f[j]);
            }

            left = newLeft;
            right = newRight;
        }

        byte[] result = new byte[BlockSize];

        Array.Copy(right, 0, result, 0, halfSize);
        Array.Copy(left, 0, result, halfSize, halfSize);

        return result;
    }
}
