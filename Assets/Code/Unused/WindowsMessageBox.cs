#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public static class WindowsMessageBox
{
#if UNITY_STANDALONE_WIN
    [DllImport( "user32.dll", CharSet = CharSet.Auto )]
    private static extern int MessageBox( IntPtr hWnd, string text, string caption, uint type );
#endif

    public static void ShowErrorAndQuit( string message, string title = "Error" )
    {
#if UNITY_EDITOR
        // Apenas para o Editor
        bool userClickedOk = EditorUtility.DisplayDialog( title, message, "OK" );
        if( userClickedOk )
            EditorApplication.isPlaying = false; // termina PlayMode
#else
#if UNITY_STANDALONE_WIN
        // Build Windows
        MessageBox(IntPtr.Zero, message, title, 0);
        Application.Quit();
#else
        // Outras plataformas
        Debug.LogError(message);
        Application.Quit();
#endif
#endif
    }
}
