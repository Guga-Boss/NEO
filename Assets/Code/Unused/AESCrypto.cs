using System;
using System.IO;
using System.Security.Cryptography;

public static class AESCrypto
{
    // Chave secreta (32 bytes = 256 bits) — você pode embaralhar ela no código
    static byte[] key = new byte[ 32 ] {
        11,22,33,44,55,66,77,88,99,00,12,23,34,45,56,67,
        78,89,90,21,32,43,54,65,76,87,98,10,20,30,40,50
    };

    public static byte[] Encrypt( byte[] data )
    {
        using( Aes aes = Aes.Create() )
        {
            aes.KeySize = 256;
            aes.Key = key;
            aes.GenerateIV(); // IV aleatório a cada arquivo

            using( var ms = new MemoryStream() )
            {
                ms.Write( aes.IV, 0, aes.IV.Length ); // salva IV no início

                using( var cs = new CryptoStream( ms, aes.CreateEncryptor(), CryptoStreamMode.Write ) )
                    cs.Write( data, 0, data.Length );

                return ms.ToArray();
            }
        }
    }
    public static byte[] Decrypt( byte[] data )
    {
        using( Aes aes = Aes.Create() )
        {
            aes.KeySize = 256;
            aes.Key = key;
            byte[] iv = new byte[ 16 ];
            Array.Copy( data, 0, iv, 0, 16 );
            aes.IV = iv;

            using( var output = new MemoryStream() )
            {
                using( var cs = new CryptoStream(
                    new MemoryStream( data, 16, data.Length - 16 ),
                    aes.CreateDecryptor(),
                    CryptoStreamMode.Read ) )
                {
                    byte[] buffer = new byte[ 4096 ]; // buffer manual
                    int bytesRead;
                    while( ( bytesRead = cs.Read( buffer, 0, buffer.Length ) ) > 0 )
                        output.Write( buffer, 0, bytesRead );
                }
                return output.ToArray();
            }
        }
    }
}
