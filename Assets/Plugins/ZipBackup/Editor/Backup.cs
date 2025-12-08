using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZipBackup
{
    public enum ZipModes
    {
        _7Zip = 1,
        FastZip = 2
    }

    [InitializeOnLoad]
    public static class Backup
    {
        static Backup()
        {
            EditorApplication.update += () =>
            {
                if( DateTime.Now.Subtract( lastBackup ).TotalMinutes > backupTimeSpan.TotalMinutes &&
                    CanBackup() && autoBackup )
                {
                    try
                    {
                        StartBackupFast( true );
                    }
                    catch( Exception e )
                    {
                        Debug.LogWarning( "Auto backup desativado devido a erro" );
                        Debug.LogException( e );
                        autoBackup = false;
                    }
                }
            };
        }

        private static bool backuping;
        private static Vector2 scroll;

        private static ZipModes mode
        {
            get { return ( ZipModes ) EditorPrefs.GetInt( "BackupMode", FastZip.isSupported ? 2 : 1 ); }
            set { EditorPrefs.SetInt( "BackupMode", ( int ) value ); }
        }

        internal static int packLevel
        {
            get { return EditorPrefs.GetInt( "BackupPackLevel", 1 ); }
            set { EditorPrefs.SetInt( "BackupPackLevel", value ); }
        }

        internal static int earlyOut
        {
            get { return EditorPrefs.GetInt( "BackupEarlyOut", 98 ); }
            set { EditorPrefs.SetInt( "BackupEarlyOut", value ); }
        }

        internal static int threads
        {
            get { return EditorPrefs.GetInt( "BackupThreads", SystemInfo.processorCount ); }
            set { EditorPrefs.SetInt( "BackupThreads", value ); }
        }

        internal static bool autoBackup
        {
            get { return EditorPrefs.GetBool( "BackupEnabled", false ); }
            set { EditorPrefs.SetBool( "BackupEnabled", value ); }
        }

        internal static bool logToConsole
        {
            get { return EditorPrefs.GetBool( "BackupLogToConsole", true ); }
            set { EditorPrefs.SetBool( "BackupLogToConsole", value ); }
        }

        internal static bool useCustomSaveLocation
        {
            get { return EditorPrefs.GetBool( "BackupUseCustomSave", false ); }
            set { EditorPrefs.SetBool( "BackupUseCustomSave", value ); }
        }

        internal static string customSaveLocation
        {
            get { return EditorPrefs.GetString( "BackupCustomSave", string.Empty ); }
            set { EditorPrefs.SetString( "BackupCustomSave", value ); }
        }

        internal static TimeSpan backupTimeSpan
        {
            get { return TimeSpan.FromMinutes( EditorPrefs.GetInt( "BackupTimeSpanMinutes", 20 ) ); }
            set { EditorPrefs.SetInt( "BackupTimeSpanMinutes", ( int ) value.TotalMinutes ); }
        }

        internal static DateTime lastBackup
        {
            get { return DateTime.Parse( PlayerPrefs.GetString( "BackupLastBackup", DateTime.MinValue.ToString() ) ); }
            set { PlayerPrefs.SetString( "BackupLastBackup", value.ToString() ); }
        }

        private static string SaveLocation
        {
            get
            {
                if( !useCustomSaveLocation || string.IsNullOrEmpty( customSaveLocation ) )
                    return Path.GetDirectoryName( Application.dataPath ) + "/Backups/";
                else
                    return customSaveLocation + "/";
            }
        }

        private static string SafeProductName
        {
            get
            {
                var name = Application.productName;
                foreach( var c in Path.GetInvalidFileNameChars() )
                    name = name.Replace( c, '_' );
                return name;
            }
        }

        [PreferenceItem( "Zip Backup" )]
        private static void PreferencesGUI()
        {
            EditorGUILayout.Space();

            if( !SevenZip.isSupported && !FastZip.isSupported )
            {
                EditorGUILayout.HelpBox( "7Zip e FastZip não suportados", MessageType.Error );
                return;
            }
            else if( !FastZip.isSupported )
                EditorGUILayout.HelpBox( "FastZip não suportado", MessageType.Warning );
            else if( !SevenZip.isSupported )
                EditorGUILayout.HelpBox( "7z.exe não encontrado", MessageType.Warning );

            scroll = EditorGUILayout.BeginScrollView( scroll, false, false );

            GUI.enabled = FastZip.isSupported && SevenZip.isSupported;
            mode = ( ZipModes ) EditorGUILayout.EnumPopup( new GUIContent( "Modo ZIP", "Aplicativo usado para compactar" ), mode );

            if( !FastZip.isSupported )
                mode = ZipModes._7Zip;
            else if( !SevenZip.isSupported )
                mode = ZipModes.FastZip;

            GUI.enabled = true;
            EditorGUILayout.Space();

            if( mode == ZipModes.FastZip )
            {
                packLevel = EditorGUILayout.IntSlider( new GUIContent( "Nível", "0=Armazenar, 9=Max compressão" ), packLevel, 0, 9 );
                GUI.enabled = packLevel > 0;
                earlyOut = EditorGUILayout.IntSlider( new GUIContent( "Early out (%)", "Compressão ruim para armazenar" ), earlyOut, 0, 100 );
                GUI.enabled = true;
                threads = EditorGUILayout.IntSlider( new GUIContent( "Threads", "Número de threads" ), threads, 1, 30 );
            }

            logToConsole = EditorGUILayout.Toggle( new GUIContent( "Log no console", "Mostrar logs" ), logToConsole );
            EditorGUILayout.Space();

            useCustomSaveLocation = EditorGUILayout.Toggle( new GUIContent( "Pasta personalizada", "Especificar pasta para backups" ), useCustomSaveLocation );
            if( useCustomSaveLocation )
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel( "Localização" );
                if( GUILayout.Button( string.IsNullOrEmpty( customSaveLocation ) ? "Procurar..." : customSaveLocation, EditorStyles.popup, GUILayout.Width( 150f ) ) )
                {
                    var path = EditorUtility.OpenFolderPanel( "Selecionar pasta", customSaveLocation, "" );
                    if( !string.IsNullOrEmpty( path ) )
                        customSaveLocation = path;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            autoBackup = EditorGUILayout.ToggleLeft( new GUIContent( "Backup automático", "Backup automático periódico" ), autoBackup );

            GUI.enabled = autoBackup;
            EditorGUI.indentLevel++;
            var minutes = ( int ) backupTimeSpan.TotalMinutes;
            minutes = EditorGUILayout.IntSlider( "Minutos", minutes, 5, 120 );
            backupTimeSpan = TimeSpan.FromMinutes( minutes );
            EditorGUI.indentLevel--;
            GUI.enabled = true;

            EditorGUILayout.Space();
            if( lastBackup != DateTime.MinValue )
                EditorGUILayout.LabelField( "Último backup: " + lastBackup.ToString( "g" ) );
            else
                EditorGUILayout.LabelField( "Último backup: Nunca" );

            EditorGUILayout.EndScrollView();
        }

        [MenuItem( "Assets/ZIP Backup/Backup Rápido" )]
        public static void MenuBackupFast()
        {
            StartBackupFast( false );
        }

        [MenuItem( "Assets/ZIP Backup/Backup Seguro" )]
        public static void MenuBackupSecure()
        {
            StartBackupSecure();
        }

        internal static void StartBackupFast( bool isAuto )
        {
            if( backuping ) return;
            if( !FastZip.isSupported )
            {
                Debug.LogError( "FastZip not supported" );
                return;
            }

            // FORMATO COM HÍFENS: yyyy-MM-dd-HH-mm
            var path = string.Format( "{0}/{1}_Fast_{2}.zip",
                SaveLocation,
                SafeProductName,
                DateTime.Now.ToString( "yyyy-MM-dd-HH-mm" ) );

            var assets = Application.dataPath;
            var project = Application.dataPath.Replace( "/Assets", "/ProjectSettings" );

            var zip = new FastZip( path, assets, project );
            zip.packLevel = isAuto ? 0 : packLevel;
            zip.threads = threads;
            zip.earlyOutPercent = earlyOut;

            AttachEvents( zip, "FASTZIP" );
            Run( zip );
        }

        internal static void StartBackupSecure()
        {
            if( backuping ) return;
            if( !SevenZip.isSupported )
            {
                Debug.LogError( "7Zip not supported" );
                return;
            }

            //Debug.Log( "Iniciando backup seguro..." );

            // FORMATO COM HÍFENS: yyyy-MM-dd-HH-mm
            var path = string.Format( "{0}/{1}_Secure_{2}.7z",
                SaveLocation,
                SafeProductName,
                DateTime.Now.ToString( "yyyy-MM-dd-HH-mm" ) );

            var assets = Application.dataPath;
            var project = Application.dataPath.Replace( "/Assets", "/ProjectSettings" );

            //Debug.Log( "Destino: " + path );

            SevenZip.usePassword = true;
            SevenZip.password = null;

            var zip = new SevenZip( path, assets, project );
            zip.Fast = false; // Modo compacto para backup seguro

            AttachEvents( zip, "7ZIP-SECURE" );
            Run( zip );
        }

        static void Run( ZipProcess zip )
        {
            try
            {
                backuping = true;
                var success = zip.Start();

                if( success )
                {
                    if( zip is FastZip )
                        Debug.Log( "Fast Backup..." );
                }
                else
                {
                    Debug.LogError( "Falha ao iniciar backup" );
                    backuping = false;
                }
            }
            catch( Exception ex )
            {
                Debug.LogError( "Erro em Run: " + ex.Message );
                backuping = false;
            }
        }

        static void AttachEvents( ZipProcess zip, string label )
        {
            var startTime = DateTime.Now;

            zip.onExit += ( o, a ) =>
            {
                try
                {
                    backuping = false;
                    lastBackup = DateTime.Now;

                    var elapsed = ( DateTime.Now - startTime ).TotalSeconds;

                    if( zip.process.ExitCode == 0 )
                    {
                        if( File.Exists( zip.output ) )
                        {
                            var size = new FileInfo( zip.output ).Length;
                            Debug.Log( string.Format( "[{0}] OK - {1} em {2:0.0}s",
                                label,
                                EditorUtility.FormatBytes( size ),
                                elapsed ) );
                        }
                        else
                        {
                            Debug.LogWarning( string.Format( "[{0}] Concluído mas arquivo não encontrado", label ) );
                        }
                    }
                    else
                    {
                        Debug.LogError( string.Format( "[{0}] ERRO - Código: {1} em {2:0.0}s",
                            label,
                            zip.process.ExitCode,
                            elapsed ) );
                    }
                }
                catch( Exception ex )
                {
                    Debug.LogError( "Erro no evento onExit: " + ex.Message );
                    backuping = false;
                }
            };
        }

        static bool CanBackup()
        {
            return !backuping && !EditorApplication.isPlaying;
        }
    }
}