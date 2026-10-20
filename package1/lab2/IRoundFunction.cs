namespace Lab2;

public interface IRoundFunction
{
    byte[] Transform(byte[] block, byte[] roundKey);
}