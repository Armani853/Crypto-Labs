namespace Lab2;

public interface IBlockCipher
{
    int BlockSize { get; }

    void SetKey(byte[] key);

    byte[] EncryptBlock(byte[] block);

    byte[] DecryptBlock(byte[] block);
}