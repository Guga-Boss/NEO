using UnityEngine;
using System.Collections.Generic;

public class LightMaskGenerator : MonoBehaviour
{
    public enum BlendMode
    {
        Normal,
        Multiply,
        Additive,
        Screen,
        Overlay,
        SoftLight,
        ColorDodge,
        Difference,
        TintOnly,     
    }

    [Header( "Core" )]
    public Shader lightShader;
    public Camera cam;
    public NightShaderApplier ShaderApplier;
    public List<LightSource2D> lights = new List<LightSource2D>();
    public int resolution = 512;
    public Material globalEffectMaterial;

    [Header( "Light Radius" )]
    public float HeroBaseRadius = 50;
    public float HeroRadiusPerFire = 10;
    public float FireBaseRadius = 50;
    public float FireRadiusPerFire = 10;
    public float TorchBaseRadius = 50;
    public float TorchRadiusPerFire = 10;

    [Header( "Dynamic Effects" )]
    public float flickerSpeed = 1f;
    public float flickerAmount = 0.1f;
    public float radiusVariation = 0.2f;
    public Vector2 positionOffset = Vector2.zero;

    [Header( "Night Settings" )]
    [Range( 0, 100 )]
    public float DayLight = 50;
    public float ForcedDaylight = 0;
    public float InitialForcedDaylight = 35;
    public float TransitionSpeed = 1;

    public Color nightDarkColor = new Color( 0.1f, 0.2f, 0.5f, 1f );

    [Header( "Master Controls" )]
    public BlendMode blendMode = BlendMode.Normal;
    public Color MasterColor = Color.white;
    [Range( 0, 5 )]
    public float MasterIntensity = 1f;

    [Header( "Post Effects" )]
    [Range( 0, 3 )]
    public float BloomThreshold = 1.5f;
    [Range( 0, 5 )]
    public float BloomIntensity = 1f;

    private Material lightMaterial;
    public RenderTexture lightMask;

    void Start()
    {
        lightMaterial = new Material( lightShader );
        lightMask = new RenderTexture( resolution, resolution, 0, RenderTextureFormat.ARGB32 );
        lightMask.Create();

        foreach( var light in lights )
            light.noiseSeed = Random.Range( 0f, 1000f );
    }

    public void UpdateIt()
    {
        bool forced = false;
        if( Manager.I.GameType == EGameType.FARM ||
            Manager.I.GameType ==  EGameType.NAVIGATION  ||
            Map.I.RM.GameOver == false )
        if( ForcedDaylight != DayLight )
        {
            ForcedDaylight = Mathf.MoveTowards( ForcedDaylight, DayLight, TransitionSpeed * Time.deltaTime );            // Temporary night fade transition calculation
            forced = true;
        }

        DayLight = CubeData.I.DayLight;
        if( forced == false )
        if( DayLight >= 100 )                                                                                            // Disable lighting for speed in day
        {
            ShaderApplier.enabled = false;
            return;
        }
        //if( Manager.I.GameType != EGameType.CUBES ) return;
        ShaderApplier.enabled = true;

        Vector2 heroPos = G.Hero.Spr.transform.position;
        float angleRad = ( G.Hero.Spr.transform.eulerAngles.z + 90f ) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2( Mathf.Cos( angleRad ), Mathf.Sin( angleRad ) ).normalized;
        lights[ 0 ].Copy( CubeData.I.HeroBase );
        lights[ 0 ].worldPosition = heroPos + dir;      

        int tot = lights.Count - 1;
        for( int i = 0; i < lights.Count; i++ )
        {
            var l = lights[ i ];
            float baseR = HeroBaseRadius;
            float perFire = HeroRadiusPerFire;
            if( l.Type == 2 )
            {
                baseR = TorchBaseRadius;
                perFire = TorchRadiusPerFire;
                l.Copy( CubeData.I.TorchBase, false );
                l.worldPosition = G.Hero.Body.Sp[ l.ID ].SpellSprite.transform.position;
            }
            if( l.Type == 3 )
            {
                baseR = FireBaseRadius;
                perFire = FireRadiusPerFire;
                l.Copy( CubeData.I.FirepitBase, false );
            }

            l.radius = baseR + ( tot * perFire );
        }

        RenderTexture.active = lightMask;
        GL.Clear( true, true, new Color( nightDarkColor.r, nightDarkColor.g, nightDarkColor.b, nightDarkColor.grayscale ) );

        if( forced == false )
        foreach( var light in lights )
        {
            float timeX = Time.time * flickerSpeed;
            float timeY = Time.time * flickerSpeed * 1.3f;

            float flicker = Mathf.PerlinNoise( timeX, light.noiseSeed ) * 2f - 0.5f;
            float intensity = light.intensity * ( 1f + flicker * flickerAmount );

            float rNoise = Mathf.PerlinNoise( timeY, light.noiseSeed * 2f ) * 2f - 1f;
            float radius = light.radius + rNoise * radiusVariation;

            Vector2 offset = new Vector2(
                ( Mathf.PerlinNoise( timeX, light.noiseSeed * 3f ) - 0.5f ) * positionOffset.x,
                ( Mathf.PerlinNoise( timeY, light.noiseSeed * 4f ) - 0.5f ) * positionOffset.y
            );

            Vector3 vp = cam.WorldToViewportPoint( light.worldPosition + offset );
            if( vp.z < 0 ) continue;

            float viewHeight = cam.orthographicSize * 2f;
            float normalizedRadius = ( radius * 2f ) / viewHeight;

            lightMaterial.SetVector( "_Center", new Vector4( vp.x, vp.y, 0, 0 ) );
            lightMaterial.SetFloat( "_Radius", normalizedRadius );
            lightMaterial.SetFloat( "_Intensity", intensity );
            lightMaterial.SetFloat( "_ColorBlend", light.colorBlend );
            lightMaterial.SetFloat( "_FalloffCurve", light.falloffCurve );
            lightMaterial.SetColor( "_InnerColor", light.innerColor * intensity );
            lightMaterial.SetColor( "_OuterColor", light.outerColor * intensity );

            Graphics.Blit( Texture2D.whiteTexture, lightMask, lightMaterial );
        }
        
        RenderTexture.active = null;

        if( globalEffectMaterial != null )
        {
            float dayPct = DayLight / 100f;

            if( forced )
                dayPct = ForcedDaylight / 100f;

            float curve = Mathf.Clamp01( ( 1f - dayPct - 0.01f ) / 0.99f );
            float nightStrength = Mathf.Lerp( 0f, 3f, curve );
            Color blendNight = Color.Lerp( Color.white, nightDarkColor, curve );

            globalEffectMaterial.SetTexture( "_LightMask", lightMask );
            globalEffectMaterial.SetFloat( "_NightIntensity", nightStrength );
            globalEffectMaterial.SetFloat( "_DayLight", dayPct );
            //globalEffectMaterial.SetColor("_DarkColor", blendNight);

            globalEffectMaterial.SetInt( "_BlendMode", ( int ) blendMode );
            globalEffectMaterial.SetColor( "_MasterColor", MasterColor );
            globalEffectMaterial.SetFloat( "_MasterIntensity", MasterIntensity );
            globalEffectMaterial.SetFloat( "_BloomThreshold", BloomThreshold );
            globalEffectMaterial.SetFloat( "_BloomIntensity", BloomIntensity );
            globalEffectMaterial.SetColor( "_DarkColor", nightDarkColor );
            globalEffectMaterial.SetColor( "_TintColor", MasterColor );
        }
    }
    public void SetTempLight( bool upd = true )
    {
        ForcedDaylight = InitialForcedDaylight;
        if( upd ) UpdateIt();
    }
}
