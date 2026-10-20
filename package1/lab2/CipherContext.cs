using System;
using System.IO;
using System.Threading.Tasks;

namespace Lab2;

public sealed class CipherContext
{
    private readonly IBlockCipher _cipher;
    private readonly CipherMode _mode;
    private readonly PaddingMode _padding;
    private readonly byte[] _iv;
    private readonly byte[]? _delta;

    public CipherContext(
        IBlockCipher cipher,
        byte[] key,
        CipherMode mode,
        PaddingMode padding,
        byte[]? iv = null,
        params object[] additionalParams)
    {
        ArgumentNullException.ThrowIfNull(cipher);
        ArgumentNullException.ThrowIfNull(key);

        if (!Enum.IsDefined(mode))
            throw new ArgumentException("Unknown cipher mode.");

        if (!Enum.IsDefined(padding))
            throw new ArgumentException("Unknown padding mode.");

        if (cipher.BlockSize < 8 || cipher.BlockSize > 255)
            throw new ArgumentException(
                "Unsupported block size.");

        _cipher = cipher;
        _mode = mode;
        _padding = padding;

        int bs = cipher.BlockSize;

        if (mode == CipherMode.ECB)
        {
            _iv = new byte[bs];
        }
        else
        {
            if (iv == null || iv.Length != bs)
                throw new ArgumentException(
                    $"IV must contain exactly {bs} bytes.");

            _iv = (byte[])iv.Clone();
        }

        if (mode == CipherMode.RandomDelta)
        {
            if (additionalParams == null ||
                additionalParams.Length != 1 ||
                additionalParams[0] is not byte[] delta ||
                delta.Length != bs)
            {
                throw new ArgumentException(
                    "Random Delta requires a byte[] delta " +
                    "with the same length as the block.");
            }

            if ((delta[^1] & 1) == 0)
                throw new ArgumentException(
                    "Random Delta increment must be odd.");

            _delta = (byte[])delta.Clone();
        }
        else
        {
            if (additionalParams != null &&
                additionalParams.Length != 0)
                throw new ArgumentException(
                    "This mode does not use additional parameters.");

            _delta = null;
        }

        _cipher.SetKey((byte[])key.Clone());
    }

    public byte[] Encrypt(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        byte[] padded = Padding.Pad(
            data,
            _cipher.BlockSize,
            _padding);

        return BlockModes.Process(
            padded,
            _cipher,
            _mode,
            _iv,
            _delta,
            decrypt: false);
    }

    public byte[] Decrypt(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        byte[] decrypted = BlockModes.Process(
            data,
            _cipher,
            _mode,
            _iv,
            _delta,
            decrypt: true);

        return Padding.Unpad(
            decrypted,
            _cipher.BlockSize,
            _padding);
    }


    public void Encrypt(byte[] input, out byte[] output)
    {
        output = Encrypt(input);
    }

    public void Decrypt(byte[] input, out byte[] output)
    {
        output = Decrypt(input);
    }


    public Task<byte[]> EncryptAsync(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        byte[] copy = (byte[])data.Clone();

        return Task.Run(() => Encrypt(copy));
    }

    public Task<byte[]> DecryptAsync(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        byte[] copy = (byte[])data.Clone();

        return Task.Run(() => Decrypt(copy));
    }


    public async Task EncryptAsync(
        string inputPath,
        string outputPath)
    {
        byte[] original =
            await File.ReadAllBytesAsync(inputPath);

        byte[] encrypted =
            await EncryptAsync(original);

        await File.WriteAllBytesAsync(
            outputPath, encrypted);
    }

    public async Task DecryptAsync(
        string inputPath,
        string outputPath)
    {
        byte[] encrypted =
            await File.ReadAllBytesAsync(inputPath);

        byte[] original =
            await DecryptAsync(encrypted);

        await File.WriteAllBytesAsync(
            outputPath, original);
    }
}