using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace EckTechGames
{
    [InitializeOnLoad]
    public class AutoSaveExtension
    {
        static AutoSaveExtension()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged( PlayModeStateChange state )
        {
            // Quando está prestes a entrar em Play Mode
            if( state == PlayModeStateChange.ExitingEditMode )
            {
                Debug.Log( "Auto-saving scenes..." );
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();
            }
        }
    }
}