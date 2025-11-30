using UnityEngine;

[ExecuteInEditMode]
public class GlobalShader : MonoBehaviour
{
    public Material nightMaterial;

    void OnRenderImage( RenderTexture src, RenderTexture dest )
    {
        if( nightMaterial != null )
        {
            Graphics.Blit( src, dest, nightMaterial );
        }
        else
        {
            Graphics.Blit( src, dest );
        }
    }
}
