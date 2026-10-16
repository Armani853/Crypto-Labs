using System;

class Program
{
    static void Main()
    {
        byte[] input = new byte[] { 0b11000000 };
        int[] pBlock = new int[] { 6, 7, 0, 1, 2, 3, 4, 5 };

        Console.WriteLine($"Проверка P-блока на валидность: {PermutationRule.IsValidPBlock(pBlock, PermutationRule.BitNumbering.ZeroBased)}");

        byte[] result = PermutationRule.Permute(input, pBlock, PermutationRule.BitOrder.MsbFirst, PermutationRule.BitNumbering.ZeroBased);

        Console.Write("Входные данные:  ");
        PrintBytesInBinary(input);

        Console.Write("Результат PermutationRule:  ");
        PrintBytesInBinary(result);

        int[] invertedPBlock = PermutationRule.InvertPBlock(pBlock, PermutationRule.BitNumbering.ZeroBased);
        byte[] restored = PermutationRule.Permute(result, invertedPBlock, PermutationRule.BitOrder.MsbFirst, PermutationRule.BitNumbering.ZeroBased);

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