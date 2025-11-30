using System;
using System.IO;
using UnityEngine;
#if UNITY_STANDALONE_WIN
#endif
using UnityEngine.UI;

public class GlobalGameGuard : MonoBehaviour
{
    public static GlobalGameGuard I { get; private set; }

    [Header( "Save Safety Settings" )]
    public bool preventSaveOnError = true;                                      // Block saves if critical error occurs
    public string saveFolder = "Saves";                                         // Default folder for saves
    public string saveFileName = "savegame.dat";                                // Default save file name
    private bool hasCriticalError = false;                                      // Flag to avoid multiple triggers
    public bool BugFound;

    [TextArea( 1, 120 )]
    public string MyMessage;

    void Awake()
    {
        if( I != null )                                                         // Singleton check
        {
            Destroy( gameObject );                                              // Destroy extra instance
            return;
        }

        I = this;                                                               // Set singleton
        DontDestroyOnLoad( gameObject );                                        // Keep across scenes
        BugFound = false;

        AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException; // Catch .NET exceptions
        Application.logMessageReceived += HandleUnityLog;                       // Catch Unity logs
    }

    void Start()
    {
        Debug.Log( "[GlobalGameGuard] Protection system activated." );          // Debug info
    }

    #region Error Capture
    private void HandleUnhandledException( object sender, UnhandledExceptionEventArgs e )
    {
        Exception ex = e.ExceptionObject as Exception;
        if( ex != null )
            ProcessCriticalError( ex );                                                                  // Process critical errors
    }

    private void HandleUnityLog( string logString, string stackTrace, LogType type )
    {
        if( Application.platform != RuntimePlatform.WindowsPlayer ) return; 
        if( logString.Contains( "Invalid editor window UnityEditor.ObjectSelector" ) ||
            logString.Contains( "UnityEditor.EditorApplicationLayout:FinalizePlaymodeLayout" ) )
            return;                                                                                      // ignora erros de layout e ObjectSelector no Editor

        if( type == LogType.Exception || type == LogType.Error )
        {
            ProcessCriticalError( new Exception( logString + "\n" + stackTrace ) );                      // Treat Unity errors as critical
        }
    }

    private void ProcessCriticalError( Exception ex )
    {
        if( hasCriticalError ) return;                                           // Avoid loop on multiple errors
        hasCriticalError = true;                                                 // Mark critical error

        // Block further saves
        if( preventSaveOnError )
            BugFound = true;                                                     // Use your global save flag

        SaveCrashLog( ex );                                                      // Save crash log

        Debug.LogError( "[GlobalGameGuard] Critical error detected: " 
            + ex.Message + "\n" + ex.StackTrace );                               // Debug log

        // Show message to player
#if UNITY_STANDALONE_WIN
        Manager.I.SaveOnEndGame = true;
        WindowsMessageBox.ShowErrorAndQuit(
            "A critical error occurred.\n\nNo data was"  + 
            " saved to prevent data loss.\n\nDetails:\n" + 
            ex.Message + "\n\n" + MyMessage,
            "Critical Error"                                                     // Message box title
        );
#else
        StartCoroutine( ShowErrorCanvas( ex.Message ) );                         // Canvas fallback for non-Windows
#endif

        Application.Quit();                                                      // Quit game safely
    }

    private void SaveCrashLog( Exception ex )
    {
        try
        {
            string crashFolder = Application.dataPath;                                                    // Folder where Unity output_log.txt is located
            string filePath = Path.Combine( crashFolder, "crashlog.txt" );                                // Fixed crash log file in same folder

            string logText = "=== Crash Report ===\n";                                                    // Crash header
            logText += "Time: " + DateTime.Now.ToString() + "\n";
            logText += "Message: " + ex.Message + "\n";
            logText += "StackTrace:\n" + ex.StackTrace + "\n\n" + MyMessage;

            File.WriteAllText( filePath, logText );                                                       // Write log to disk
            Debug.Log( "[GlobalGameGuard] Crash log saved at: " + filePath );                             // Debug info
        }
        catch( Exception logEx )
        {
            Debug.LogError( "[GlobalGameGuard] Failed to save crash log: " + logEx.Message );             // Warn if logging fails
        }
    }

    private System.Collections.IEnumerator ShowErrorCanvas( string message )
    {
        GameObject canvasGO = new GameObject( "ErrorCanvas" );                     // Create canvas
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject panelGO = new GameObject( "Panel" );
        panelGO.transform.SetParent( canvasGO.transform );
        RectTransform rt = panelGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        UnityEngine.UI.Image img = panelGO.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color( 0, 0, 0, 0.75f );                                        // Semi-transparent background

        GameObject textGO = new GameObject( "Text" );
        textGO.transform.SetParent( panelGO.transform );
        UnityEngine.UI.Text txt = textGO.AddComponent<UnityEngine.UI.Text>();
        txt.text = "Critical error detected!\n\n" + message + 
        "\nThe game will close to prevent data loss.\n\n" + MyMessage;                  // English message
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>( "Arial.ttf" );
        txt.color = Color.white;

        RectTransform txtRt = textGO.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;

        yield return new WaitForSeconds( 5f );                                      // Delay to let player read
        Application.Quit();                                                         // Quit game
    }
    #endregion

    #region Safe Save
    public bool SafeSave( byte[] data )
    {
        if( hasCriticalError )                                                    // Block save if error occurred
        {
            Debug.LogWarning( "[GlobalGameGuard] Save blocked due to critical error." ); // Warning in English
            return false;
        }
        try
        {
            if( !Directory.Exists( saveFolder ) ) Directory.CreateDirectory( saveFolder ); // Ensure save folder exists

            string tempFile = Path.Combine( saveFolder, saveFileName + ".tmp" );   // Temporary file
            string finalFile = Path.Combine( saveFolder, saveFileName );           // Final save file

            File.WriteAllBytes( tempFile, data );                                  // Write temp
            File.Copy( tempFile, finalFile, true );                                 // Overwrite final
            File.Delete( tempFile );                                                // Delete temp

            return true;                                                            // Success
        }
        catch( Exception ex )
        {
            Debug.LogError( "[GlobalGameGuard] Failed to save: " + ex.Message );  // Error saving
            return false;
        }
    }
    #endregion
}
