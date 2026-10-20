using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Lab2;

public static class Padding
{
    public static byte[] Pad(
        byte[] data,
        int blockSize,
        PaddingMode mode)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateBlockSize(blockSize);

        if (mode == PaddingMode.Zeros)
        {
            int lengthWithHeader = checked(data.Length + 8);

            int remainder = lengthWithHeader % blockSize;
            int extra = remainder == 0
                ? 0
                : blockSize - remainder;

            byte[] result = new byte[
                checked(lengthWithHeader + extra)];

            BinaryPrimitives.WriteInt64BigEndian(
                result.AsSpan(0, 8),
                data.Length);

            Array.Copy(
                data, 0,
                result, 8,
                data.Length);

            return result;
        }

        int padLength = blockSize - (data.Length % blockSize);

        byte[] padded = new byte[
            checked(data.Length + padLength)];

        Array.Copy(data, padded, data.Length);

        switch (mode)
        {
            case PaddingMode.PKCS7:
                for (int i = data.Length; i < padded.Length; i++)
                    padded[i] = (byte)padLength;
                break;

            case PaddingMode.ANSI_X923:

                padded[^1] = (byte)padLength;
                break;

            case PaddingMode.ISO10126:
                RandomNumberGenerator.Fill(
                    padded.AsSpan(
                        data.Length,
                        padLength - 1));

                padded[^1] = (byte)padLength;
                break;

            default:
                throw new NotSupportedException(
                    $"Unknown padding: {mode}");
        }

        return padded;
    }

    public static byte[] Unpad(
        byte[] data,
        int blockSize,
        PaddingMode mode)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateBlockSize(blockSize);

        if (data.Length % blockSize != 0)
            throw new ArgumentException(
                "Data length is not a multiple of block size.");

        if (mode == PaddingMode.Zeros)
        {
            if (data.Length < 8)
                throw new InvalidOperationException(
                    "Missing original length.");

            long originalLength =
                BinaryPrimitives.ReadInt64BigEndian(
                    data.AsSpan(0, 8));

            if (originalLength < 0 ||
                originalLength > data.Length - 8)
                throw new InvalidOperationException(
                    "Invalid original length.");

            int length = checked((int)originalLength);

            for (int i = 8 + length; i < data.Length; i++)
            {
                if (data[i] != 0)
                    throw new InvalidOperationException(
                        "Invalid zero padding.");
            }

            byte[] restored = new byte[length];

            Array.Copy(data, 8, restored, 0, length);

            return restored;
        }

        if (data.Length == 0)
            throw new InvalidOperationException(
                "Missing padding block.");

        int padLength = data[^1];

        if (padLength < 1 ||
            padLength > blockSize ||
            padLength > data.Length)
            throw new InvalidOperationException(
                "Invalid padding length.");

        switch (mode)
        {
            case PaddingMode.PKCS7:
                for (int i = data.Length - padLength;
                     i < data.Length;
                     i++)
                {
                    if (data[i] != padLength)
                        throw new InvalidOperationException(
                            "Invalid PKCS7 padding.");
                }
                break;

            case PaddingMode.ANSI_X923:
                for (int i = data.Length - padLength;
                     i < data.Length - 1;
                     i++)
                {
                    if (data[i] != 0)
                        throw new InvalidOperationException(
                            "Invalid ANSI X.923 padding.");
                }
                break;

            case PaddingMode.ISO10126:
                break;

            default:
                throw new NotSupportedException(
                    $"Unknown padding: {mode}");
        }

        byte[] result = new byte[data.Length - padLength];

        Array.Copy(data, result, result.Length);

        return result;
    }

    private static void ValidateBlockSize(int blockSize)
    {
        if (blockSize < 8 || blockSize > 255)
            throw new ArgumentOutOfRangeException(
                nameof(blockSize),
                "Block size must be between 8 and 255.");
    }
}