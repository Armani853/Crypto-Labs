using System;
using System.Collections.Generic;

public enum BitOrder
{
    LsbFirst,
    MsbFirst
}

public enum BitNumbering
{
    ZeroBased,
    OneBased
}

public static class PBox
{
    public static byte[] Permute(byte[] input, int[] pBlock, BitOrder order, BitNumbering numbering)
    {
        if (input == null || input.Length == 0)
            throw new ArgumentException("Массив input не может быть пустым");

        if (pBlock == null || pBlock.Length == 0)
            throw new ArgumentException("P-блок не может быть пустым");

        int inputBits = input.Length * 8;
        int outputBits = pBlock.Length;
        
        int outputBytesCount = (outputBits + 7) / 8;
        byte[] result = new byte[outputBytesCount];

        int offset = (numbering == BitNumbering.OneBased) ? 1 : 0;

        for (int i = 0; i < outputBits; i++)
        {
            int srcBitIndex = pBlock[i] - offset;

            if (srcBitIndex < 0 || srcBitIndex >= inputBits)
                throw new ArgumentException($"Индекс {pBlock[i]} выходит за пределы входных данных");

            int srcByte = srcBitIndex / 8;
            int srcBitInByte = srcBitIndex % 8;

            if (order == BitOrder.MsbFirst)
                srcBitInByte = 7 - srcBitInByte;

            int bit = (input[srcByte] >> srcBitInByte) & 1;

            if (bit == 1)
            {
                int destByte = i / 8;
                int destBitInByte = i % 8;

                if (order == BitOrder.MsbFirst)
                    destBitInByte = 7 - destBitInByte;

                result[destByte] |= (byte)(1 << destBitInByte);
            }
        }

        return result;
    }

    public static int[] InvertPBlock(int[] pBlock, BitNumbering numbering)
    {
        int offset = (numbering == BitNumbering.OneBased) ? 1 : 0;
        int[] inverted = new int[pBlock.Length];

        for (int i = 0; i < pBlock.Length; i++)
        {
            int target = pBlock[i] - offset;
            inverted[target] = i + offset;
        }

        return inverted;
    }

    public static bool IsValidPBlock(int[] pBlock, BitNumbering numbering)
    {
        if (pBlock == null || pBlock.Length == 0)
            return false;

        int offset = (numbering == BitNumbering.OneBased) ? 1 : 0;
        var seen = new HashSet<int>();

        foreach (int idx in pBlock)
        {
            int physical = idx - offset;

            if (physical < 0 || physical >= pBlock.Length)
                return false;

            if (!seen.Add(physical))
                return false;
        }

        return true;
    }
}

class Program
{
    static void Main()
    {
        byte[] input = new byte[] { 0b11000000 };
        int[] pBlock = new int[] { 6, 7, 0, 1, 2, 3, 4, 5 };

        Console.WriteLine($"Проверка P-блока на валидность: {PBox.IsValidPBlock(pBlock, BitNumbering.ZeroBased)}");

        byte[] result = PBox.Permute(input, pBlock, BitOrder.MsbFirst, BitNumbering.ZeroBased);

        Console.Write("Входные данные:  ");
        PrintBytesInBinary(input);

        Console.Write("Результат PBox:  ");
        PrintBytesInBinary(result);

        int[] invertedPBlock = PBox.InvertPBlock(pBlock, BitNumbering.ZeroBased);
        byte[] restored = PBox.Permute(result, invertedPBlock, BitOrder.MsbFirst, BitNumbering.ZeroBased);

        Console.Write("После восстановления: ");
        PrintBytesInBinary(restored);
    }

    static void PrintBytesInBinary(byte[] bytes)
    {
        foreach (byte b in bytes)
        {
            Console.Write(Convert.ToString(b, 2).PadLeft(8, '0') + " ");
        }
        Console.WriteLine();
    }
}