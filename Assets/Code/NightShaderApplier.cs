using UnityEngine;
[ExecuteInEditMode]
public class NightShaderApplier : MonoBehaviour
{
    public Material globalEffectMaterial;
    public LightMaskGenerator lightGenerator;

    void OnRenderImage( RenderTexture src, RenderTexture dest )
    {
        if( globalEffectMaterial != null && lightGenerator != null && lightGenerator.lightMask != null )
        {
            globalEffectMaterial.SetTexture( "_LightMask", lightGenerator.lightMask );
            Graphics.Blit( src, dest, globalEffectMaterial );
        }
        else
        {
            Graphics.Blit( src, dest );
        }
    }
}
