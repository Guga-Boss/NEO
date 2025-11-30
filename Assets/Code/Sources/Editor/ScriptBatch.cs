using UnityEditor;
using System.Diagnostics;
using UnityEngine;
using System.IO;

public class ScriptBatch
{
    [MenuItem( "Tools/Build Game" )]
    public static void BuildGame()
    {
        bool result = EditorUtility.DisplayDialog( "Obfuscator", "Use Obfuscator?", "No", "Yes" );
        string file = "Assets/Editor/Beebyte/Obfuscator/ObfuscatorOptions.asset";
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>( file );
        if( asset != null )
        {
            SerializedObject so = new SerializedObject( asset );
            SerializedProperty prop = so.FindProperty( "enabled" );
            if( prop != null )
            {
                prop.boolValue = !result;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty( asset );
                AssetDatabase.SaveAssets();
                UnityEngine.Debug.Log( "Obfuscation: " + !result );
            }
        }

        GameObject ob = GameObject.Find( "Farm" );
        Farm f = ob.GetComponent<Farm>();
        f.UpdateListsCallBack();                                                                             // update data

        // Get filename.
        //string path = EditorUtility.SaveFolderPanel( "C:/Users/Guga/Desktop/NEO Release", "", "" );
        string path = "C:/Users/alien/Desktop/NEO Release";
        string[ ] levels = new string[ ] { "Assets/NEO.unity"};

        EditorApplication.SaveScene();
        AssetDatabase.SaveAssets();

        // Build player.
        BuildPipeline.BuildPlayer( levels, path + "/NEO.exe", BuildTarget.StandaloneWindows, BuildOptions.None );

        // Copy a file from the project folder to the build folder, alongside the built game.
        //FileUtil.CopyFileOrDirectory( "Assets/Templates/Readme.txt", path + "Readme.txt" );

        // Prepare Files
        Process proc = new Process();
        proc.StartInfo.FileName = "C:/Users/alien/Desktop/Prepare Files.bat";
        proc.Start();

        // Run the game (Process class from System.Diagnostics).
        //Process proc2 = new Process();
        //proc2.StartInfo.FileName = path + "/NEO.exe";
        ////proc.Start();

        // --- Copiar LanguageCache.json para dentro de NEO_Data ---
        string sourcePath = "C:/Users/alien/Desktop/NEO/Assets/Resources/Language/LanguageCache.json";

        // Application.dataPath na build aponta para <BuildFolder>/NEO_Data
        string dataDir = Path.Combine( path, "NEO_Data" );
        string targetDir = Path.Combine( dataDir, "Resources/Language" );
        string targetPath = Path.Combine( targetDir, "LanguageCache.json" );

        // Cria diretório se não existir
        if( !Directory.Exists( targetDir ) )
            Directory.CreateDirectory( targetDir );

        // Copia o arquivo, sobrescrevendo se já existir
        File.Copy( sourcePath, targetPath, true );
        UnityEngine.Debug.Log( "LanguageCache.json copiado para: " + targetPath );
    }
}