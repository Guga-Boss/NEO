using System;                        // Basic system functions
using System.IO;                     // File operations (Stream, MemoryStream)
using System.Text;                   // Encoding, StringBuilder
using System.Security.Cryptography;  // HMAC, RandomNumberGenerator, AES
using System.Collections.Generic;    // List<T>
using UnityEngine;                   // SystemInfo.deviceUniqueIdentifier

public static class Security
{
    public static string TempUnique = "";
    public static bool FileConsistencyChecked = false;
    public static bool FirstTimePlaying = false;

    private const int HMAC_LENGTH = 32;                       // SHA256 output size in bytes
    private static readonly byte[] HMACKey;                   // Cached HMAC key
    private static readonly byte[] AESKey;                    // Cached AES key
    private static readonly byte[] AESIV;                     // AES initialization vector

    static Security()
    {
        HMACKey = BuildDeviceSpecificKey();                   // Initialize HMAC key once
        AESKey = BuildAESKey();                               // Initialize AES key once
        AESIV = BuildAESIV();                                 // Initialize AES IV once
    }

    /*O sistema de criptografia foi refeito.
    A segurança deixou de usar dados da máquina e agora depende de um ID interno do jogador. 
    Isso evita perda de saves ao trocar de computador ou atualizar windows e aumenta a proteção contra edição externa. 
    Também foi adicionado um sistema de verificação de integridade para impedir alterações ilegais nos arquivos.
    Um modo de compatibilidade temporário Gugaversion foi mantido para permitir abertura de saves antigos usados em testes. 
    Pode ser removido apos testes
   */

    // Build a key derived from static secret + device identifier for HMAC
    private static byte[] BuildDeviceSpecificKey()
    {
        byte[] key = new byte[ 32 ];              // 32 bytes = HMAC-SHA256 key
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes( key );                       // preenche array
        // não chama Dispose(), funciona no Unity 5.6
        return key;
    }
    // Derive AES key from UniqueID
    private static byte[] BuildAESKey()
    {
        string unique = G.Farm.UniqueID;
        if( string.IsNullOrEmpty( unique ) ) unique = "default-key";
        string secret = "&%34547438390@*5$&34@3#$%&57@67667864!";                 // segredo que ninguém conhece para decript
        if( Manager.I.GugaVersion )
            secret = "";                                                          // ignora segredo para seu save
        using( SHA256 sha = SHA256.Create() )
        {
            return sha.ComputeHash( Encoding.UTF8.GetBytes( secret + unique ) );
        }
    }

    // Simple fixed IV (could be randomized per file if desired)
    private static byte[] BuildAESIV()
    {
        return new byte[ 16 ];                                   // 16 bytes = 128-bit IV, all zeros
    }

    // Called before writing to disk: encrypts rawData + appends HMAC
    public static byte[] CheckSave( byte[] rawData )
    {
        if( rawData == null ) throw new ArgumentNullException( "rawData" );

        byte[] encryptedData = EncryptAES( rawData, AESKey, AESIV );     // Encrypt the raw data
        byte[] hmac = ComputeHMAC( encryptedData, HMACKey );             // Compute HMAC over encrypted data

        byte[] outBytes = new byte[ encryptedData.Length + hmac.Length ];
        Buffer.BlockCopy( encryptedData, 0, outBytes, 0, encryptedData.Length );
        Buffer.BlockCopy( hmac, 0, outBytes, encryptedData.Length, hmac.Length );

        return outBytes;                                      // Return encrypted + HMAC
    }

    // Called after reading from disk: verifies HMAC and decrypts content
    public static byte[] CheckLoad( byte[] fileData )
    {
        if( fileData == null )
            throw new ArgumentNullException( "fileData" );

        if( fileData.Length < HMAC_LENGTH )
            throw new Exception( "Save file too small or corrupt" ); // Verificação mínima do tamanho

        // Separar conteúdo criptografado e HMAC
        int contentLen = fileData.Length - HMAC_LENGTH;
        byte[] content = new byte[ contentLen ];
        byte[] savedHmac = new byte[ HMAC_LENGTH ];

        Buffer.BlockCopy( fileData, 0, content, 0, contentLen );
        Buffer.BlockCopy( fileData, contentLen, savedHmac, 0, HMAC_LENGTH );

        // Recalcular HMAC com a chave atual
        byte[] actualHmac = ComputeHMAC( content, HMACKey );

        // Verifica se o HMAC bate
        bool validHmac = CompareBytes( actualHmac, savedHmac );

        if( !validHmac )
        {
            // Se for seu save antigo, tenta descriptografar com chave antiga
            if( Manager.I.GugaVersion )
            {
                try
                {
                    byte[] oldAESKey = BuildOldAESKey(); // chave antiga sem secret
                    return DecryptAES( content, oldAESKey, AESIV );
                }
                catch
                {
                    //WindowsMessageBox.ShowErrorAndQuit( "Save file invalid or tampered (old save)" ); // erro fatal
                }
            }
            else
            {
                // Para outros jogadores: HMAC inválido = arquivo corrompido / modificado
                WindowsMessageBox.ShowErrorAndQuit( "Save file invalid or tampered" );
            }
        }

        // Se HMAC válido ou se GugaVersion não for necessário, descriptografa com chave atual
        byte[] decryptedData = DecryptAES( content, AESKey, AESIV );

        return decryptedData; // Retorna dados validados e descriptografados
    }

    private static byte[] BuildOldAESKey()
    {
        string unique = G.Farm.UniqueID;
        if( string.IsNullOrEmpty( unique ) ) unique = "default-key";
        // Chave antiga (sem secret)
        string oldSecret = "";
        using( SHA256 sha = SHA256.Create() )
        {
            return sha.ComputeHash( Encoding.UTF8.GetBytes( oldSecret + unique ) );
        }
    }

    // AES encryption
    private static byte[] EncryptAES( byte[] data, byte[] key, byte[] iv )
    {
        using( Aes aes = Aes.Create() )
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using( MemoryStream ms = new MemoryStream() )
            using( CryptoStream cs = new CryptoStream( ms, aes.CreateEncryptor(), CryptoStreamMode.Write ) )
            {
                cs.Write( data, 0, data.Length );
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
        }
    }

    // AES decryption
    private static byte[] DecryptAES( byte[] data, byte[] key, byte[] iv )
    {
        using( Aes aes = Aes.Create() )
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using( MemoryStream ms = new MemoryStream() )
            using( CryptoStream cs = new CryptoStream( ms, aes.CreateDecryptor(), CryptoStreamMode.Write ) )
            {
                cs.Write( data, 0, data.Length );
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
        }
    }

    // HMAC-SHA256 computation
    private static byte[] ComputeHMAC( byte[] data, byte[] key )
    {
        if( data == null ) throw new ArgumentNullException( "data" );
        if( key == null ) throw new ArgumentNullException( "key" );

        using( var hmac = new HMACSHA256( key ) )
        {
            return hmac.ComputeHash( data );
        }
    }

    // Constant-time comparison to avoid timing attacks
    private static bool CompareBytes( byte[] a, byte[] b )
    {
        if( a == null || b == null ) return false;
        if( a.Length != b.Length ) return false;
        int result = 0;
        for( int i = 0; i < a.Length; i++ )
            result |= a[ i ] ^ b[ i ];
        return result == 0;
    }

    // Secure UniqueID generator
    public static void SortUniqueID()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; // Uppercase + numbers
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        byte[] randomBytes = new byte[ 16 ];
        var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes( randomBytes );

        for( int i = 0; i < 16; i++ )
        {
            int idx = randomBytes[ i ] % chars.Length;          // Map random byte to char set
            sb.Append( chars[ idx ] );
            if( i == 3 || i == 7 || i == 11 ) sb.Append( '-' ); // Format: XXXX-XXXX-XXXX-XXXX
        }

        G.Farm.UniqueID = sb.ToString();                       // Save generated ID
    }

    internal static void FinalizeSave( MemoryStream ms, string file )
    {
        byte[] rawData = ms.ToArray();                          // Get raw data

        byte[] protectedData = Security.CheckSave( rawData );   // Protect with AES + HMAC

        string tempFile = file + ".tmp";                        // Temporary file for atomic save
        File.WriteAllBytes( tempFile, protectedData );          // Write temp file

        if( File.Exists( file ) ) File.Delete( file );          // Delete old save
        File.Move( tempFile, file );                            // Rename temp file to final save
    }

    public static int LoadHeader()
    {
        int version = GS.R.ReadInt32();                         // Load Version

        string unique = GS.R.ReadString();                      // Load Unique Player ID 

        if( G.Farm.UniqueID == "" )
            G.Farm.UniqueID = unique;                           // Attrib case empty (first time)
        else
            if( unique != G.Farm.UniqueID )
            {
                WindowsMessageBox.ShowErrorAndQuit( "Bad Farm Unique ID" );          // unique id mismatch
            }
        return version;
    }

    internal static int SaveHeader( int version )
    {
        int SaveVersion = version;
        if( G.Farm.UniqueID == "" )
        {
            WindowsMessageBox.ShowErrorAndQuit( "Cannot Save Empty Unique ID" );
            return -1;
        }

        GS.W.Write( SaveVersion );                                                   // Save Version

        GS.W.Write( G.Farm.UniqueID );                                               // Save Player Unique ID
        return version;
    }

    internal static bool CheckPlayerFilesConsistency()
    {
        FirstTimePlaying = false;
        FileConsistencyChecked = true;
        if( Helper.I.ReleaseVersion == false ) return true;

        bool res = true;
        string[] files = {
        "Time.NEO", "Blueprint.NEO", "Building.NEO",                                        // list of files to be tested
        "Farm.NEO", "Idle.NEO",      "Inventory.NEO",
        "Navigation.NEO", "Secret.NEO", "Tutorial.NEO"                                      // all files need to exist. Prevent cheating by delete
        };

        int count = 0;
        foreach( var file in files )
        {
            string exist = Manager.I.GetProfileFolder() + file;                             // Provides File name
            if( File.Exists( exist ) ) count++; 
        }

        if( count == 0 ) { FirstTimePlaying = true; return true; }                                 // no files found: First time

        foreach( var file in files )                                                        // tests unique ids
            if( !TestLoad( file ) )
            { res = false; }

        // *** Atencao: falta Goals pois tem um sistema diferente. Alem disso, nao esta sendo salvo na Farm na salvada inicial
        if( res == false )
            Manager.I.PlayerName = "### Corrupted Player Filename ###";
        return res;
    }
    public static bool TestLoad( string fn )
    {
        string file = Manager.I.GetProfileFolder() + fn;                                     // Provides File name
        if( File.Exists( file ) == false )
        {
            Manager.I.SaveOnEndGame = false;
            WindowsMessageBox.ShowErrorAndQuit(
              "File does not Exist: " + fn );
            return false;
        }

        byte[] fileData = File.ReadAllBytes( file );                                              // Read full file
        byte[] content = Security.CheckLoad( fileData );                                          // Validate HMAC and get clean content

        using( GS.R = new BinaryReader( new MemoryStream( content ) ) )                           // Use MemoryStream for TF
        {
            int SaveVersion = GS.R.ReadInt32();
            string sig = GS.R.ReadString();                                                       // Load Unique Player ID
            //Debug.Log( " File: " + fn + " UniqueID:   " + sig );                                // Debug

            if( fn == "Time.NEO" )
            {
                TempUnique = sig;                                                                 // First on the list: Attrib to Farm Unique ID
                G.Farm.UniqueID = sig;
            }
            else
            if( TempUnique != sig )
            {
                Manager.I.SaveOnEndGame = false;
                WindowsMessageBox.ShowErrorAndQuit(
                "Corrupted Player File: Bad Unique ID: " + fn + " Sig: " +                        // Bad file!
                 sig + "\nTemp: " + TempUnique );
                return false;
            }
        }
        return true;
    }
}
