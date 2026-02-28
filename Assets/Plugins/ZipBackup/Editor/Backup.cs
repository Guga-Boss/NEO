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

    //ggini[InitializeOnLoad]
    public static class Backup
    {
        static Backup()
        {
            // Remove antes de adicionar para evitar duplicatas no assembly reload
            EditorApplication.update -= CheckAutoBackup;
            EditorApplication.update += CheckAutoBackup;
        }

        private static void CheckAutoBackup()
        {
            if( autoBackup && CanBackup() &&
                DateTime.Now.Subtract( lastBackup ).TotalMinutes > backupTimeSpan.TotalMinutes )
            {
                try
                {
                    //StartBackupFast( true );
                    Debug.Log( "Tentando backup seguro ao inves..." );
                    StartBackupSecure();
                }
                catch( Exception e )
                {
                    Debug.LogWarning( "Auto backup desativado devido a erro" );
                    Debug.LogException( e );
                    autoBackup = false;
                }
            }
        }

        private static bool backuping;
        private static Vector2 scroll;

        // Novo sistema de Preferences (Project Settings)
        [SettingsProvider]
        public static SettingsProvider CreateZipBackupSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Zip Backup", SettingsScope.Project)
            {
                label = "Zip Backup",
                guiHandler = (searchContext) => PreferencesGUI(),
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Zip", "Backup", "7z", "FastZip" })
            };
            return provider;
        }

        #region Prefs
        private static ZipModes mode
        {
            get { return (ZipModes) EditorPrefs.GetInt( "BackupMode", 1 ); }
            set { EditorPrefs.SetInt( "BackupMode", (int) value ); }
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
            set { EditorPrefs.SetInt( "BackupTimeSpanMinutes", (int) value.TotalMinutes ); }
        }

        internal static DateTime lastBackup
        {
            get
            {
                string s = PlayerPrefs.GetString("BackupLastBackup", string.Empty);
                return string.IsNullOrEmpty( s ) ? DateTime.MinValue : DateTime.Parse( s );
            }
            set { PlayerPrefs.SetString( "BackupLastBackup", value.ToString() ); }
        }
        #endregion

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

        private static void PreferencesGUI()
        {
            EditorGUILayout.Space();

            if( !SevenZip.isSupported && !FastZip.isSupported )
            {
                EditorGUILayout.HelpBox( "7Zip e FastZip não suportados ou executáveis não encontrados.", MessageType.Error );
                return;
            }

            scroll = EditorGUILayout.BeginScrollView( scroll );

            mode = (ZipModes) EditorGUILayout.EnumPopup( "Modo ZIP", mode );

            EditorGUILayout.Space();

            if( mode == ZipModes.FastZip && FastZip.isSupported )
            {
                packLevel = EditorGUILayout.IntSlider( "Nível (0-9)", packLevel, 0, 9 );
                earlyOut = EditorGUILayout.IntSlider( "Early out (%)", earlyOut, 0, 100 );
                threads = EditorGUILayout.IntSlider( "Threads", threads, 1, 32 );
            }

            logToConsole = EditorGUILayout.Toggle( "Log no Console", logToConsole );

            EditorGUILayout.Space();
            useCustomSaveLocation = EditorGUILayout.Toggle( "Pasta Personalizada", useCustomSaveLocation );
            if( useCustomSaveLocation )
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField( "Caminho", customSaveLocation );
                if( GUILayout.Button( "...", GUILayout.Width( 30 ) ) )
                {
                    var path = EditorUtility.OpenFolderPanel("Selecionar pasta", customSaveLocation, "");
                    if( !string.IsNullOrEmpty( path ) ) customSaveLocation = path;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            autoBackup = EditorGUILayout.ToggleLeft( "Ativar Backup Automático", autoBackup );

            if( autoBackup )
            {
                EditorGUI.indentLevel++;
                int minutes = EditorGUILayout.IntSlider("Intervalo (Minutos)", (int)backupTimeSpan.TotalMinutes, 5, 120);
                backupTimeSpan = TimeSpan.FromMinutes( minutes );
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField( "Último backup: " + ( lastBackup == DateTime.MinValue ? "Nunca" : lastBackup.ToString( "g" ) ) );

            EditorGUILayout.EndScrollView();
        }

        [MenuItem( "Assets/ZIP Backup/Backup Rápido" )]
        public static void MenuBackupFast() => StartBackupFast( false );

        [MenuItem( "Assets/ZIP Backup/Backup Seguro" )]
        public static void MenuBackupSecure() => StartBackupSecure();

        internal static void StartBackupFast( bool isAuto )
        {
            if( backuping || !FastZip.isSupported ) return;

            var path = string.Format("{0}/{1}_Fast_{2}.zip",
                SaveLocation, SafeProductName, DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));

            var assets = Application.dataPath;
            var project = Application.dataPath.Replace("/Assets", "/ProjectSettings");

            var zip = new FastZip(path, assets, project);
            zip.packLevel = isAuto ? 1 : packLevel;
            zip.threads = threads;

            AttachEvents( zip, "FASTZIP" );
            Run( zip );
        }

        internal static void StartBackupSecure()
        {
            if( backuping || !SevenZip.isSupported ) return;

            var path = string.Format("{0}/{1}_Secure_{2}.7z",
                SaveLocation, SafeProductName, DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));

            var assets = Application.dataPath;
            var project = Application.dataPath.Replace("/Assets", "/ProjectSettings");

            SevenZip.usePassword = true;
            var zip = new SevenZip(path, assets, project);
            zip.Fast = false;

            AttachEvents( zip, "7ZIP-SECURE" );
            Run( zip );
        }

        static void Run( ZipProcess zip )
        {
            backuping = true;
            if( !zip.Start() ) backuping = false;
        }

        static void AttachEvents( ZipProcess zip, string label )
        {
            var startTime = DateTime.Now;
            zip.onExit += ( o, a ) =>
            {
                backuping = false;
                lastBackup = DateTime.Now;
                if( zip.process.ExitCode == 0 )
                    Debug.Log( $"[{label}] Backup OK em {( DateTime.Now - startTime ).TotalSeconds:0.0}s" );
                else
                    Debug.LogError( $"[{label}] Falha no Backup. Código: {zip.process.ExitCode}" );
            };
        }

        static bool CanBackup() => !backuping && !EditorApplication.isPlaying && !EditorApplication.isCompiling;
    }
}