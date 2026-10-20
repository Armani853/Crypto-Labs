using System;
using System.Threading.Tasks;

namespace Lab2;

public static class BlockModes
{
    public static byte[] Process(
        byte[] data,
        IBlockCipher cipher,
        CipherMode mode,
        byte[] iv,
        byte[]? delta,
        bool decrypt)
    {
        int bs = cipher.BlockSize;

        if (data.Length % bs != 0)
            throw new ArgumentException(
                "Data length must be a multiple of block size.");

        if (mode != CipherMode.ECB && iv.Length != bs)
            throw new ArgumentException("Invalid IV length.");

        return mode switch
        {
            CipherMode.ECB =>
                Ecb(data, cipher, decrypt),

            CipherMode.CBC =>
                Cbc(data, cipher, iv, decrypt),

            CipherMode.PCBC =>
                Pcbc(data, cipher, iv, decrypt),

            CipherMode.CFB =>
                Cfb(data, cipher, iv, decrypt),

            CipherMode.OFB =>
                Ofb(data, cipher, iv),

            CipherMode.CTR =>
                Ctr(data, cipher, iv),

            CipherMode.RandomDelta =>
                RandomDelta(data, cipher, iv, delta!),

            _ => throw new NotSupportedException(
                $"Unknown cipher mode: {mode}")
        };
    }

    private static byte[] Xor(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException(
                "XOR requires equal lengths.");

        byte[] result = new byte[a.Length];

        for (int i = 0; i < a.Length; i++)
            result[i] = (byte)(a[i] ^ b[i]);

        return result;
    }

    private static byte[] GetBlock(
        byte[] data, int index, int bs)
    {
        byte[] block = new byte[bs];

        Array.Copy(
            data, index * bs,
            block, 0, bs);

        return block;
    }

    private static void PutBlock(
        byte[] result, int index, byte[] block, int bs)
    {
        if (block.Length != bs)
            throw new InvalidOperationException(
                "Cipher returned an invalid block.");

        Array.Copy(
            block, 0,
            result, index * bs,
            bs);
    }

    private static byte[] Ecb(
        byte[] data,
        IBlockCipher cipher,
        bool decrypt)
    {
        int bs = cipher.BlockSize;
        int count = data.Length / bs;

        byte[] result = new byte[data.Length];

        Parallel.For(0, count, i =>
        {
            byte[] block = GetBlock(data, i, bs);

            byte[] output = decrypt
                ? cipher.DecryptBlock(block)
                : cipher.EncryptBlock(block);

            PutBlock(result, i, output, bs);
        });

        return result;
    }

    private static byte[] Cbc(
        byte[] data,
        IBlockCipher cipher,
        byte[] iv,
        bool decrypt)
    {
        int bs = cipher.BlockSize;
        int count = data.Length / bs;

        byte[] result = new byte[data.Length];
        byte[] previous = (byte[])iv.Clone();

        for (int i = 0; i < count; i++)
        {
            byte[] block = GetBlock(data, i, bs);
            byte[] output;

            if (!decrypt)
            {
                byte[] mixed = Xor(block, previous);

                output = cipher.EncryptBlock(mixed);

                previous = output;
            }
            else
            {
                byte[] decrypted =
                    cipher.DecryptBlock(block);

                output = Xor(decrypted, previous);

                previous = block;
            }

            PutBlock(result, i, output, bs);
        }

        return result;
    }

    private static byte[] Pcbc(
        byte[] data,
        IBlockCipher cipher,
        byte[] iv,
        bool decrypt)
    {
        int bs = cipher.BlockSize;
        int count = data.Length / bs;

        byte[] result = new byte[data.Length];

        byte[] previousPlain = new byte[bs];
        byte[] previousCipher = (byte[])iv.Clone();

        for (int i = 0; i < count; i++)
        {
            byte[] block = GetBlock(data, i, bs);

            byte[] output;

            if (!decrypt)
            {
                byte[] mixed =
                    Xor(Xor(block, previousPlain),
                        previousCipher);

                output = cipher.EncryptBlock(mixed);

                previousPlain = block;
                previousCipher = output;
            }
            else
            {
                byte[] decrypted =
                    cipher.DecryptBlock(block);

                output =
                    Xor(Xor(decrypted, previousPlain),
                        previousCipher);

                previousPlain = output;
                previousCipher = block;
            }

            PutBlock(result, i, output, bs);
        }

        return result;
    }

    private static byte[] Cfb(
        byte[] data,
        IBlockCipher cipher,
        byte[] iv,
        bool decrypt)
    {
        int bs = cipher.BlockSize;
        int count = data.Length / bs;

        byte[] result = new byte[data.Length];
        byte[] feedback = (byte[])iv.Clone();

        for (int i = 0; i < count; i++)
        {
            byte[] block = GetBlock(data, i, bs);

            byte[] gamma =
                cipher.EncryptBlock(feedback);

            byte[] output = Xor(block, gamma);

            feedback = decrypt ? block : output;

            PutBlock(result, i, output, bs);
        }

        return result;
    }

    private static byte[] Ofb(
        byte[] data,
        IBlockCipher cipher,
        byte[] iv)
    {
        int bs = cipher.BlockSize;
        int count = data.Length / bs;

        byte[] result = new byte[data.Length];
        byte[] feedback = (byte[])iv.Clone();

        for (int i = 0; i < count; i++)
        {
            feedback = cipher.EncryptBlock(feedback);

            byte[] block = GetBlock(data, i, bs);

            byte[] output = Xor(block, feedback);

            PutBlock(result, i, output, bs);
        }

        return result;
    }

    private static byte[] Ctr(
        byte[] data,
        IBlockCipher cipher,
        byte[] iv)
    {
        int bs = cipher.BlockSize;
        int count = data.Length / bs;

        byte[] result = new byte[data.Length];

        Parallel.For(0, count, i =>
        {
            byte[] counter = (byte[])iv.Clone();

            AddNumber(counter, (ulong)i);

            byte[] gamma = cipher.EncryptBlock(counter);
            byte[] block = GetBlock(data, i, bs);

            PutBlock(result, i, Xor(block, gamma), bs);
        });

        return result;
    }


    private static byte[] RandomDelta(
        byte[] data,
        IBlockCipher cipher,
        byte[] iv,
        byte[] delta)
    {
        int bs = cipher.BlockSize;
        int count = data.Length / bs;

        byte[] result = new byte[data.Length];
        byte[] counter = (byte[])iv.Clone();

        for (int i = 0; i < count; i++)
        {
            byte[] gamma = cipher.EncryptBlock(counter);
            byte[] block = GetBlock(data, i, bs);

            PutBlock(result, i, Xor(block, gamma), bs);

            AddBytes(counter, delta);
        }

        return result;
    }

    private static void AddNumber(
        byte[] counter, ulong value)
    {
        ulong carry = value;

        for (int i = counter.Length - 1; i >= 0; i--)
        {
            ulong sum = (ulong)counter[i] +
                        (carry & 0xFF);

            counter[i] = (byte)sum;

            carry = (carry >> 8) + (sum >> 8);
        }
    }

    private static void AddBytes(
        byte[] counter, byte[] delta)
    {
        int carry = 0;

        for (int i = counter.Length - 1; i >= 0; i--)
        {
            int sum = counter[i] + delta[i] + carry;

            counter[i] = (byte)sum;
            carry = sum >> 8;
        }
    }
}