using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZipBackup
{
    public sealed class FastZip: ZipProcess
    {
        public int packLevel { get; set; }
        public int threads { get; set; }

        new public static bool isSupported
        {
            get
            {
                // Removida a trava de "64bit" que falha no Windows 11
                return Application.platform == RuntimePlatform.WindowsEditor && !string.IsNullOrEmpty( path );
            }
        }

        new public static string path
        {
            get
            {
                var guids = AssetDatabase.FindAssets("Fastzip t:Object");
                foreach( var guid in guids )
                {
                    var p = AssetDatabase.GUIDToAssetPath(guid);
                    if( p.EndsWith( ".exe", StringComparison.OrdinalIgnoreCase ) )
                        return Path.GetFullPath( p );
                }
                return string.Empty;
            }
        }

        public FastZip( string output, params string[ ] sources )
        {
            this.output = Path.GetFullPath( output ).Replace( "/", "\\" );
            this.sources = sources;
            this.packLevel = 1;
            this.threads = SystemInfo.processorCount;
        }

        public override bool Start()
        {
            if( !isSupported || !File.Exists( path ) )
            {
                UnityEngine.Debug.LogError( "FastZip.exe não encontrado!" );
                return false;
            }

            startInfo = new ProcessStartInfo();
            startInfo.FileName = path;
            startInfo.WorkingDirectory = Path.GetDirectoryName( path ); // Essencial para evitar erro -1
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            // Construção dos argumentos com aspas e caminhos Windows
            string args = string.Format("-p{0} -t{1} ", packLevel, threads);
            args += string.Format( "\"{0}\" ", output );

            foreach( var src in sources )
            {
                if( Directory.Exists( src ) || File.Exists( src ) )
                {
                    string fullSrc = Path.GetFullPath(src).Replace("/", "\\").TrimEnd('\\');
                    args += string.Format( "\"{0}\" ", fullSrc );
                }
            }

            startInfo.Arguments = args;

            // Limpa o arquivo antigo se existir
            if( File.Exists( output ) ) File.Delete( output );

            Directory.CreateDirectory( Path.GetDirectoryName( output ) );

            process = new Process();
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;
            process.Exited += Exited;

            // Opcional: Logar o comando exato para depuração se falhar
            // UnityEngine.Debug.Log("Executando: " + path + " " + args);

            return process.Start();
        }
    }
}