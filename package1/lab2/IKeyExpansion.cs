namespace Lab2;

public interface IKeyExpansion
{
    byte[][] GenerateRoundKeys(byte[] key);
}