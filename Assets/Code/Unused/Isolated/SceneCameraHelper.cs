using UnityEngine;
using UnityEditor;

public static class SceneCameraHelper
{
    [MenuItem( "Tools/Scene Camera/Print Current Camera Data" )]
    public static void PrintSceneCameraData()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;

        if( sceneView == null )
        {
            Debug.LogWarning( "No active SceneView found." );
            return;
        }

        Vector3 pivot = sceneView.pivot;
        Quaternion rotation = sceneView.rotation;
        float size = sceneView.size;

        Debug.Log( "==== Scene Camera Data ====" );
        Debug.Log( $"Pivot: {pivot}" );
        Debug.Log( $"Rotation: {rotation}" );
        Debug.Log( $"Size (Zoom): {size}" );
    }

    [MenuItem( "Tools/Scene Camera/Apply Camera Data" )]
    public static void ApplySceneCameraData()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;

        if( sceneView == null )
        {
            Debug.LogWarning( "No active SceneView found." );
            return;
        }

        // 👇 VOCÊ ALTERA AQUI DEPOIS 
        Vector3 pivot = new Vector3(0, 0, 0);
        Quaternion rotation = Quaternion.Euler(45, 0, 0);
        float size = 20f;

        sceneView.pivot = pivot;
        sceneView.rotation = rotation;
        sceneView.size = size;
        sceneView.Repaint();

        Debug.Log( "Scene camera updated." );
    }
}