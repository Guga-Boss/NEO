using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZipBackup
{
    public class SevenZip : ZipProcess
    {
        public static bool usePassword = false;
        public bool Fast = true;
        public static string password;

        new public static bool isSupported
        {
            get
            {
                return SystemInfo.operatingSystem.ToLower().Contains( "windows" ) && File.Exists( path );
            }
        }

        new public static string path
        {
            get
            {
                var exe = EditorApplication.applicationContentsPath + "/Tools/7z.exe";
                return File.Exists( exe ) ? exe : string.Empty;
            }
        }

        public SevenZip( string output, params string[] sources )
        {
            if( !isSupported )
                throw new FileLoadException( "7-Zip não disponível." );

            if( string.IsNullOrEmpty( output ) )
                throw new ArgumentException( "Saída inválida." );

            if( sources == null || sources.Length == 0 )
                throw new ArgumentException( "Nenhuma fonte de backup." );

            this.output = output;
            this.sources = sources;
        }

        static string GeneratePasswordFromFileName( string path )
        {
            var name = Path.GetFileNameWithoutExtension( path );

            // Procura pelo último "_" para separar "NEO_Secure_2025-07-12-09-55"
            var idx = name.LastIndexOf( '_' );
            if( idx < 0 )
            {
                UnityEngine.Debug.LogWarning( "Não foi possível gerar senha do nome do arquivo, usando padrão" );
                return "backup";
            }

            // Pega a parte da data: "2025-07-12-09-55"
            var datePart = name.Substring( idx + 1 );

            // Verifica se tem o formato correto com hífens
            if( datePart.Length > 2 && datePart.Contains( "-" ) )
            {
                // Remove primeiro e último caractere: 
                // "2025-07-12-09-55" → "025-07-12-09-5"
                var pass = datePart.Substring( 1, datePart.Length - 2 );
                password = pass;
                //UnityEngine.Debug.Log( "Senha gerada (com hífens): " + pass );
                return pass;
            }

            return "backup";
        }

        public override bool Start()
        {
            try
            {
                startInfo = new ProcessStartInfo();
                startInfo.FileName = path;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                // Construir argumentos de forma limpa
                string args = BuildArguments();
                startInfo.Arguments = args;

                UnityEngine.Debug.Log( "Safe Backup: " + " Args: " + startInfo.Arguments );
                password = "null";

                // Preparar diretório de saída
                var outputDir = Path.GetDirectoryName( output );
                if( !Directory.Exists( outputDir ) )
                    Directory.CreateDirectory( outputDir );

                // Deletar arquivo existente se houver
                if( File.Exists( output ) )
                    File.Delete( output );

                process = new Process();
                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;
                process.OutputDataReceived += OutputDataReceived;
                process.ErrorDataReceived += ErrorDataReceived;
                process.Exited += Exited;

                var started = process.Start();
                if( started )
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }

                return started;
            }
            catch( Exception ex )
            {
                UnityEngine.Debug.LogError( "Erro no SevenZip.Start: " + ex.Message );
                return false;
            }
        }
        private string BuildArguments()
        {
            System.Text.StringBuilder args = new System.Text.StringBuilder();

            // Comando básico
            args.Append( "a " );

            // Nível de compressão - VOLTAR AO ORIGINAL
            if( Fast )
            {
                // Original: "-tzip -mx=1" para rápido
                args.Append( "-tzip -mx=1 " );
            }
            else
            {
                // Original: "-mx=1" para seguro (mas no original ambos eram mx=1!)
                args.Append( "-mx=1 " ); // ← Como no seu código original
            }

            // Senha se necessário
            if( usePassword )
            {
                string pass = !string.IsNullOrEmpty( password ) ? password : GeneratePasswordFromFileName( output );
                args.AppendFormat( "-p\"{0}\" -mhe=on ", pass );
            }

            // Performance
            args.Append( "-mmt=on " ); // Multi-threading
            args.Append( "-bd " );     // Desabilitar indicador de progresso

            // Arquivo de saída
            args.AppendFormat( "\"{0}\" ", output );

            // Fontes
            foreach( var source in sources )
            {
                if( Directory.Exists( source ) || File.Exists( source ) )
                {
                    args.AppendFormat( "\"{0}\" ", source );
                }
            }

            return args.ToString();
        }
    }
}