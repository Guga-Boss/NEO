using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;
public class CubeData : MonoBehaviour 
{
    public static CubeData I;
    [TabGroup( "" )]
    [Header( "Write Script Here!" )]
    [TextArea( 1, 120 )]
    public string Script = "";
    [TabGroup( "Main" )]
    public bool SteppingMode = false;
    [TabGroup( "Main" )]
    public float TickMoveTime = 1;
    [TabGroup( "Main" )]
    public List<int> TickMoveList = null;
    [TabGroup( "Main" )]
    public float AltarCurve = 1.25f;                          // experimente com 1.5, 2.0, 3.0 - acima de 2 em muito dificil ideal 1.2 a 1.5 - peca para GPT para ver tabela topico: "jogo butcher altar pole"
    [TabGroup( "Main" )]
    public float RandomAltarCurve = 1.5f;                     // 1 fica linear
    [TabGroup( "Main" )]
    [Range( -1, 3 )]
    public int FixedZoomMode = -1;  
    [TabGroup( "Main" )]
    [Range( 0, 3 )]
    public int PreferedZoomMode = 0; 
    [TabGroup( "FX" )]
    [Range(1, 5000)]
    public float SnowStrenght = 300;
    [TabGroup( "FX" )]
    [Range( 1, 5000 )]
    public float FirefliesAmount = 1000;
    [TabGroup( "FX" )]
    public bool ForceFireflies = false;
    [TabGroup( "Night" )]
    [Range( 0, 100 )]
    public float DayLight = 100;
    [TabGroup( "Night" )]
    public float HeroBaseRadius = 30;
    [TabGroup( "Night" )]
    public float HeroRadiusPerFire = 0;
    [TabGroup( "Night" )]
    public float FireBaseRadius = 150;
    [TabGroup( "Night" )]
    public float FireRadiusPerFire = 50;
    [TabGroup( "Night" )]
    public float TorchBaseRadius = 50;
    [TabGroup( "Night" )]
    public float TorchRadiusPerFire = 20;
    [TabGroup( "Night" )]
    public Color nightDarkColor = Color.black;
    [TabGroup( "Night" )]
    public LightSource2D HeroBase, TorchBase, FirepitBase;
    [TabGroup( "Night" )]
    public float flickerSpeed = 6f;
    [TabGroup( "Night" )]
    public float flickerAmount = 0.1f;
    [TabGroup( "Night" )]
    public float radiusVariation = 0.1f;
    [TabGroup( "Night" )]
    public Vector2 positionOffset = new Vector2( .1f, .1f );

    [Header( "Master Controls" )]
    [TabGroup( "Night" )]
    public LightMaskGenerator.BlendMode blendMode = LightMaskGenerator.BlendMode.Normal;
    [TabGroup( "Night" )]
    public Color MasterColor = Color.white;
    [TabGroup( "Night" )]
    [Range( 0, 5 )]
    public float MasterIntensity = 0f;
    [Header( "Post Effects" )]
    [TabGroup( "Night" )]
    [Range( 0, 3 )]
    public float BloomThreshold = 1.5f;
    [TabGroup( "Night" )]
    [Range( 0, 5 )]
    public float BloomIntensity = 1f;

    //[HideInInspector]
    [TabGroup( "Water" )]
    public Material WaterMaterial;
    [TabGroup( "Water" )]
    [Range( 1, 10 )]
    public int WaterTextureID = 1;
    [TabGroup( "Water" )]
    [Range( 0, 1 )]
    public float BlueThreshold = 1f;
    [TabGroup( "Water" )]
    public float WaterScale = 5f;
    [TabGroup( "Water" )]
    public Color WaterColor = new Color32( 0, 98, 163, 255 );
    [TabGroup( "Water" )]
    public Color DeepWaterColor = new Color32( 21, 85, 170, 255 );
    [TabGroup( "Water" )]
    [Range( 0, 2 )]
    public float ColorIntensity = 1.57f;
    [TabGroup( "Water" )]
    [Range( 0, 5 )]
    public float WaveSpeed = 4f;
    [TabGroup( "Water" )]
    [Range( 0, 0.5f )]
    public float WaveIntensity = 0.23f;
    [TabGroup( "Water" )]
    [Range( 0, 180 )]
    public float WaveAngle = 45f;
    [TabGroup( "Water" )]
    [Range( 1, 50 )]
    public float WaveFrequency = 12;
    [TabGroup( "Water" )]
    public float DropletSize = 2.44f;
    [TabGroup( "Water" )]
    [Range( 0, 2 )]
    public float DropletIntensity = 0.864f;
    [TabGroup( "Water" )]
    public float DropletSpeed = 1.32f;
    [TabGroup( "Water" )]
    public float DropletLifetime = 2f;
    [TabGroup( "Water" )]
    [Range( 0, 50 )]
    public int DropletCount = 50;
    [TabGroup( "Water" )]
    [Range( 0, 1 )]
    public float SunReflection = 0.72f;
    [TabGroup( "Water" )]
    public Color ReflectionColor = new Color32( 255, 214, 0, 192 );
    [TabGroup( "Water" )]
    [Range( 1, 100 )]
    public float ReflectionSharpness = 97;
    [TabGroup( "Water" )]
    public Vector4 SunDirection = new Vector4( -125.41f, -101.7f, 177.84f, -98.5f );
    [Space( 20 )]
    [Title( "Save & Load:" )]
    [TabGroup( "Water" )]
    [Range( 1, 8 )]
    public int WaterShaderSaveID;

    // Lava 
    [TabGroup( "Lava" )]
    [Range( 1, 10 )]
    public int LavaTextureID = 1;
    [TabGroup( "Lava" )]
    [Range( 0, 50 )]
    public float LavaParticleForce = 5;
    [TabGroup( "Lava" )]
    [Range( 0, 1 )]
    public float RedThreshold = 0.64f;
    [TabGroup( "Lava" )]
    public float LavaScale = 2f;
    [TabGroup( "Lava" )]
    public Color LavaColor = new Color32( 255, 0, 0, 255 );
    [TabGroup( "Lava" )]
    public Color DeepLavaColor = new Color32( 255, 230, 0, 255 );
    [TabGroup( "Lava" )]
    [Range( 0, 2 )]
    public float LavaColorIntensity = 1.74f;
    [TabGroup( "Lava" )]
    [Range( 0, 5 )]
    public float LavaWaveSpeed = 1.5f;
    [TabGroup( "Lava" )]
    [Range( 0, 0.5f )]
    public float LavaWaveIntensity = 0.255f;
    [TabGroup( "Lava" )]
    [Range( 0, 180 )]
    public float LavaWaveAngle = 45f;
    [TabGroup( "Lava" )]
    [Range( 1, 50 )]
    public float LavaWaveFrequency = 12;
    [Space( 20 )]
    [Title( "Save & Load:" )]
    [TabGroup( "Lava" )]
    [Range( 1, 8 )]
    public int LavaShaderSaveID;

    [TabGroup( "Fog" )]
    [Range( 0, 100 )]
    public float FogIntensity = 0;
    [TabGroup( "Fog" )]
    [Range( 0, 100 )]
    public float FogAlpha = 15f;
    [TabGroup( "Fog" )]
    public Color FogColor = Color.white;
    public static bool UpdateParticles;
    [TabGroup( "Stonepath" )]
    public float StonepathMovementMaxSpeed = -1;
    [TabGroup( "Stonepath" )]
    public float StonepathRotationMaxSpeed = -1;
    [TabGroup( "Stonepath" )]
    public bool StonepathDiagonalMovement = true;

    [TabGroup( "Water" )]
    public bool OverWriteWaterSave;
    [TabGroup( "Water" )]
    [Button( "Load Water Shader", ButtonSizes.Medium ), GUIColor( 0, 1f, 0 )]
    public void Button1()
    {
        string file = Application.dataPath + "/Resources/Shaders/" +
        "Water/Saves/Water Shader " + WaterShaderSaveID + ".bytes";
        if( File.Exists( file ) == false )
        {
            Debug.Log( "Shader Does not Exist:" + file );
            return;
        }
        using( GS.R = new BinaryReader( File.Open( file, FileMode.Open ) ) )                      // Open Stream
        {
            int SaveVersion = GS.R.ReadInt32();                                                   // Load Version

            LoadWaterShader();                                                                    // Load Water Shader

            GS.R.Close();                                                                         // Close Stream
            Debug.Log( "Shader Loaded:" + file );
            OnValidate();
        }
    }    

    [TabGroup( "Water" )]
    [Button( "Save Water Shader", ButtonSizes.Medium ), GUIColor( 1f, 0.2f, 0 )]
    public void Button2()
    {
        string file = Application.dataPath + "/Resources/Shaders/" +
        "Water/Saves/Water Shader " + WaterShaderSaveID + ".bytes";

        if( OverWriteWaterSave == false )                                                        // Overwrite ?
        if( File.Exists( file ) == true ) 
        {
            Debug.LogError( "Shader Already Exists! (Check OverWrite)" ); 
            return; 
        }
        using( GS.W = new BinaryWriter( File.Open( file, FileMode.OpenOrCreate ) ) )             // Open Stream
        {
            int SaveVersion = 1;
            GS.W.Write( SaveVersion );                                                           // Save Version
            SaveWaterShader();                                                                   // Save Water Shader
            GS.W.Close();                                                                        // Close Stream
            Debug.Log( "Shader Saved:" + file );
            OverWriteWaterSave = false;
        }
    }

    [TabGroup( "Lava" )]
    public bool OverWriteLavaSave;
    [TabGroup( "Lava" )]
    [Button( "Load Lava Shader", ButtonSizes.Medium ), GUIColor( 0, 1f, 0 )]
    public void LoadLavaButton()
    {
        string file = Application.dataPath + "/Resources/Shaders/" +
        "Water/Saves/Lava Shader " + LavaShaderSaveID + ".bytes";
        if( File.Exists( file ) == false )
        {
            Debug.Log( "Shader Does not Exist:" + file );
            return;
        }
        using( GS.R = new BinaryReader( File.Open( file, FileMode.Open ) ) )                      // Open Stream
        {
            int SaveVersion = GS.R.ReadInt32();                                                   // Load Version

            LoadLavaShader();                                                                     // Load Lava Shader

            GS.R.Close();                                                                         // Close Stream
            Debug.Log( "Shader Loaded:" + file );
            OnValidate();
        }
    }

    [TabGroup( "Lava" )]
    [Button( "Save Lava Shader", ButtonSizes.Medium ), GUIColor( 1f, 0.2f, 0 )]
    public void SaveLavaButton()
    {
        string file = Application.dataPath + "/Resources/Shaders/" +
        "Water/Saves/Lava Shader " + LavaShaderSaveID + ".bytes";

        if( OverWriteLavaSave == false )                                                         // Overwrite ?
            if( File.Exists( file ) == true )
            {
                Debug.LogError( "Shader Already Exists! (Check OverWrite)" );
                return;
            }
        using( GS.W = new BinaryWriter( File.Open( file, FileMode.OpenOrCreate ) ) )             // Open Stream
        {
            int SaveVersion = 1;
            GS.W.Write( SaveVersion );                                                           // Save Version
            SaveLavaShader();                                                                    // Save Lava Shader
            GS.W.Close();                                                                        // Close Stream
            Debug.Log( "Shader Saved:" + file );
            OverWriteLavaSave = false;
        }
    }
    
    [Space( 20 )]
    [Title( "Save & Load:" )]
    [TabGroup( "Night" )]
    [Range( 1, 8 )]
    public int NightShaderSaveID;
    [TabGroup( "Night" )]
    public bool OverWriteNightSave;

    [TabGroup( "Night" )]
    [Button( "Load Night Shader", ButtonSizes.Medium ), GUIColor( 0, 1f, 0 )]
    public void Button3()
    {
        string file = Application.dataPath + "/Resources/Shaders/" +
        "Global/Saves/Shader " + NightShaderSaveID + ".bytes";
        if( File.Exists( file ) == false )
        {
            Debug.Log( "Shader Does not Exist:" + file );
            return;
        }

        using( GS.R = new BinaryReader( File.Open( file, FileMode.Open ) ) )                      // Open Stream
        {
            int SaveVersion = GS.R.ReadInt32();                                                   // Load Version

            LoadNightShader();                                                                    // Load Night Shader

            GS.R.Close();                                                                         // Close Stream
            Debug.Log( "Shader Loaded:" + file );
            OnValidate();
        }
    }

    [TabGroup( "Night" )]
    [Button( "Save Night Shader", ButtonSizes.Medium ), GUIColor( 1f, 0.2f, 0 )]
    public void Button4()
    {
        string file = Application.dataPath + "/Resources/Shaders/" +
        "Global/Saves/Shader " + NightShaderSaveID + ".bytes";

        if( OverWriteNightSave == false )                                                       // Overwrite ?
        if( File.Exists( file ) == true )
        {
            Debug.LogError( "Shader Already Exists! (Check OverWrite)" );
            return;
        }

        using( GS.W = new BinaryWriter( File.Open( file, FileMode.OpenOrCreate ) ) )             // Open Stream
        {
            int SaveVersion = 1;
            GS.W.Write( SaveVersion );                                                           // Save Version
            SaveNightShader();                                                                   // Save Night Shader
            GS.W.Close();                                                                        // Close Stream
            Debug.Log( "Shader Saved:" + file );
            OverWriteNightSave = false;
        }
    }    
    void Start()
    {
        I = this;
    }    
    public void Reset()
    {
        HeroBase.intensity = .3f;
        HeroBase.innerColor = new Color32( 255, 128, 77, 255 );
        HeroBase.outerColor = new Color32( 19, 31, 125, 255 );
        HeroBase.colorBlend = .25f;
        HeroBase.falloffCurve = 1f;
        HeroBase.Type = 1;

        TorchBase.intensity = .2f;
        TorchBase.innerColor = new Color32( 255, 128, 77, 255 );
        TorchBase.outerColor = new Color32( 19, 31, 125, 255 );
        TorchBase.colorBlend = 5;
        TorchBase.falloffCurve = 3f;
        TorchBase.Type = 2;

        FirepitBase.intensity = .4f;
        FirepitBase.innerColor = new Color32( 255, 128, 77, 255 );
        FirepitBase.outerColor = new Color32( 19, 31, 125, 255 );
        FirepitBase.colorBlend = 5;
        FirepitBase.falloffCurve = 10f;
        FirepitBase.Type = 3;

        WaterMaterial = Resources.Load<Material>( "Shaders/Water/Water Material" );
    }

    public void UpdateData( bool save )
    {
        if( !Application.isPlaying ) return;
        if( Helper.I == null || Helper.I.ReleaseVersion ) return;
        UpdateParticles = true;
        Map.I = GameObject.Find( "----------------Map" ).GetComponent<Map>();
        Map.I.Lights.DayLight = DayLight;
        Map.I.Lights.HeroBaseRadius = HeroBaseRadius;
        Map.I.Lights.HeroRadiusPerFire = HeroRadiusPerFire;
        Map.I.Lights.FireBaseRadius = FireBaseRadius;
        Map.I.Lights.FireRadiusPerFire = FireRadiusPerFire;
        Map.I.Lights.TorchBaseRadius = TorchBaseRadius;
        Map.I.Lights.TorchRadiusPerFire = TorchRadiusPerFire;
        Map.I.Lights.flickerSpeed = flickerSpeed;
        Map.I.Lights.flickerAmount = flickerAmount;
        Map.I.Lights.radiusVariation = radiusVariation;
        Map.I.Lights.positionOffset = positionOffset;
        Map.I.Lights.blendMode = blendMode;
        Map.I.Lights.MasterColor = MasterColor;
        Map.I.Lights.MasterIntensity = MasterIntensity;
        Map.I.Lights.BloomThreshold = BloomThreshold;
        Map.I.Lights.BloomIntensity = BloomIntensity;
        Map.I.Lights.nightDarkColor = nightDarkColor;
        string resourcePath = "Shaders/Water/Water Textures/Water " + WaterTextureID; 
        Texture2D waterTex = Resources.Load<Texture2D>( resourcePath ); 
        if( waterTex != null && WaterMaterial != null )
            WaterMaterial.SetTexture( "_WaterTex", waterTex );

        WaterMaterial.SetFloat( "_BlueThreshold", BlueThreshold );
        WaterMaterial.SetFloat( "_WaterScale", WaterScale );
        WaterMaterial.SetColor( "_WaterColor", WaterColor );
        WaterMaterial.SetColor( "_DeepWaterColor", DeepWaterColor );
        WaterMaterial.SetFloat( "_ColorIntensity", ColorIntensity );
        WaterMaterial.SetFloat( "_WaveSpeed", WaveSpeed );
        WaterMaterial.SetFloat( "_WaveIntensity", WaveIntensity );
        WaterMaterial.SetFloat( "_WaveAngle", WaveAngle );
        WaterMaterial.SetFloat( "_WaveFrequency", WaveFrequency );
        WaterMaterial.SetFloat( "_DropletSize", DropletSize );
        WaterMaterial.SetFloat( "_DropletIntensity", DropletIntensity );
        WaterMaterial.SetFloat( "_DropletSpeed", DropletSpeed );
        WaterMaterial.SetFloat( "_DropletLifetime", DropletLifetime );
        WaterMaterial.SetInt( "_DropletCount", DropletCount );
        WaterMaterial.SetFloat( "_SunReflection", SunReflection );
        WaterMaterial.SetColor( "_ReflectionColor", ReflectionColor );
        WaterMaterial.SetFloat( "_ReflectionSharpness", ReflectionSharpness );
        WaterMaterial.SetVector( "_SunDirection", SunDirection );

        WaterMaterial.SetFloat( "_RedThreshold", RedThreshold );
        WaterMaterial.SetFloat( "_LavaScale", LavaScale );
        WaterMaterial.SetColor( "_LavaColor", LavaColor );
        WaterMaterial.SetColor( "_DeepLavaColor", DeepLavaColor );
        WaterMaterial.SetFloat( "_LavaColorIntensity", LavaColorIntensity );
        WaterMaterial.SetFloat( "_LavaWaveSpeed", LavaWaveSpeed );
        WaterMaterial.SetFloat( "_LavaWaveIntensity", LavaWaveIntensity );
        WaterMaterial.SetFloat( "_LavaWaveAngle", LavaWaveAngle );
        WaterMaterial.SetFloat( "_LavaWaveFrequency", LavaWaveFrequency );

        resourcePath = "Shaders/Water/Lava Textures/Lava " + LavaTextureID;
        Texture2D lavaTex = Resources.Load<Texture2D>( resourcePath );
        if( lavaTex != null && WaterMaterial != null )
            WaterMaterial.SetTexture( "_LavaTex", lavaTex );

        if( save )
        if( MapSaver.I )
        if( Application.isPlaying )
        if( Map.I.SessionFrameCount > 180 )
        if( Map.I && Map.I.RM.HeroSector.Type == Sector.ESectorType.NORMAL )
        if( MapSaver.I.LastLoadedFile != "" )
        {
            MapSaver.I.SaveMap( MapSaver.I.LastLoadedFile, ref MapSaver.I.Tilemap );
            Debug.Log( "Saved: " + MapSaver.I.LastLoadedFile );
        }
    }
    public void OnValidate()
    {
        UpdateData( true );
    }

    public static bool available;
    public static string nm;
    public void Load()
    {
        LoadMain();
        LoadNightShader();
        LoadWaterShader();
        LoadLavaShader();
        UpdateData( false );
    }
    public void Save()
    {
        SaveMain();
        SaveNightShader();
        SaveWaterShader();
        SaveLavaShader();
    }
    private void LoadMain()
    {
        Script = GS.R.ReadString();
        SteppingMode = GS.R.ReadBoolean();
        FogIntensity = GS.R.ReadSingle();
        FogAlpha = GS.R.ReadSingle();
        SnowStrenght = GS.R.ReadSingle();
        FirefliesAmount = GS.R.ReadSingle();
        ForceFireflies = GS.R.ReadBoolean();
        AltarCurve = GS.R.ReadSingle();
        RandomAltarCurve = GS.R.ReadSingle();
        FogColor = GS.LColor();
        StonepathMovementMaxSpeed = GS.R.ReadSingle();
        StonepathRotationMaxSpeed = GS.R.ReadSingle();
        StonepathDiagonalMovement = GS.R.ReadBoolean();
        TickMoveTime = GS.R.ReadSingle();
        FixedZoomMode = GS.R.ReadInt32();
        PreferedZoomMode = GS.R.ReadInt32();

        int sz = GS.R.ReadInt32();
        TickMoveList = new List<int>();
        for( int i = 0; i < sz; i++ )
            TickMoveList.Add( GS.R.ReadInt32() );
    }

    private void SaveMain()
    {
        GS.W.Write( Script );
        GS.W.Write( SteppingMode );
        GS.W.Write( FogIntensity );
        GS.W.Write( FogAlpha );
        GS.W.Write( SnowStrenght );
        GS.W.Write( FirefliesAmount );
        GS.W.Write( ForceFireflies );
        GS.W.Write( AltarCurve );
        GS.W.Write( RandomAltarCurve );   
        GS.SColor( FogColor );
        GS.W.Write( StonepathMovementMaxSpeed );
        GS.W.Write( StonepathRotationMaxSpeed );
        GS.W.Write( StonepathDiagonalMovement );
        GS.W.Write( TickMoveTime );
        GS.W.Write( FixedZoomMode );
        GS.W.Write( PreferedZoomMode );

        int sz = 0;
        if( TickMoveList != null ) 
            sz = TickMoveList.Count;
        GS.W.Write( sz );
        for( int i = 0; i < TickMoveList.Count; i++ )
            GS.W.Write( TickMoveList[ i ] );
    }
    public void SaveNightShader()
    {
        GS.W.Write( DayLight );
        GS.W.Write( HeroBaseRadius );
        GS.W.Write( HeroRadiusPerFire );
        GS.W.Write( FireBaseRadius );
        GS.W.Write( FireRadiusPerFire );
        GS.W.Write( TorchBaseRadius );        
        GS.W.Write( TorchRadiusPerFire );
        GS.SColor( nightDarkColor );
        FirepitBase.Save();
        HeroBase.Save();
        TorchBase.Save();
        GS.W.Write( flickerSpeed );
        GS.W.Write( flickerAmount );
        GS.W.Write( radiusVariation );
        GS.SVector2( positionOffset );
        GS.W.Write( ( int ) blendMode );
        GS.SColor( MasterColor );
        GS.W.Write( MasterIntensity );
        GS.W.Write( BloomThreshold );
        GS.W.Write( BloomIntensity );
    }
    public void LoadNightShader()
    {
        DayLight = GS.R.ReadSingle();
        HeroBaseRadius = GS.R.ReadSingle();
        HeroRadiusPerFire = GS.R.ReadSingle();
        FireBaseRadius = GS.R.ReadSingle();
        FireRadiusPerFire = GS.R.ReadSingle();
        TorchBaseRadius = GS.R.ReadSingle();
        TorchRadiusPerFire = GS.R.ReadSingle();
        nightDarkColor = GS.LColor();
        FirepitBase.Load();
        HeroBase.Load();
        TorchBase.Load();
        flickerSpeed = GS.R.ReadSingle();
        flickerAmount = GS.R.ReadSingle();
        radiusVariation = GS.R.ReadSingle();
        positionOffset = GS.LVector2();
        blendMode = ( LightMaskGenerator.BlendMode ) GS.R.ReadInt32();
        MasterColor = GS.LColor();
        MasterIntensity = GS.R.ReadSingle();
        BloomThreshold = GS.R.ReadSingle();
        BloomIntensity = GS.R.ReadSingle();
    }    
    public void SaveWaterShader()
    {
        GS.W.Write( WaterTextureID );
        GS.W.Write( BlueThreshold );
        GS.W.Write( WaterScale );
        GS.SColor( WaterColor );
        GS.SColor( DeepWaterColor );        
        GS.W.Write( ColorIntensity );
        GS.W.Write( WaveSpeed );
        GS.W.Write( WaveIntensity );
        GS.W.Write( WaveAngle );
        GS.W.Write( WaveFrequency );
        GS.W.Write( DropletSize );
        GS.W.Write( DropletIntensity );
        GS.W.Write( DropletSpeed );
        GS.W.Write( DropletLifetime );
        GS.W.Write( DropletCount );
        GS.W.Write( SunReflection );
        GS.SColor( ReflectionColor );
        GS.W.Write( ReflectionSharpness );
        GS.W.Write( SunDirection.x ); GS.W.Write( SunDirection.y ); GS.W.Write( SunDirection.z ); GS.W.Write( SunDirection.w );
    }

    public void LoadWaterShader()
    {
        WaterTextureID = GS.R.ReadInt32();
        BlueThreshold = GS.R.ReadSingle();
        WaterScale = GS.R.ReadSingle();
        WaterColor = GS.LColor();
        DeepWaterColor = GS.LColor();
        ColorIntensity = GS.R.ReadSingle();
        WaveSpeed = GS.R.ReadSingle();
        WaveIntensity = GS.R.ReadSingle();
        WaveAngle = GS.R.ReadSingle();
        WaveFrequency = GS.R.ReadSingle();
        DropletSize = GS.R.ReadSingle(); 
        DropletIntensity = GS.R.ReadSingle();
        DropletSpeed = GS.R.ReadSingle();
        DropletLifetime = GS.R.ReadSingle();
        DropletCount = GS.R.ReadInt32();
        SunReflection = GS.R.ReadSingle();
        ReflectionColor = GS.LColor();
        ReflectionSharpness = GS.R.ReadSingle();
        SunDirection.x = GS.R.ReadSingle(); SunDirection.y = GS.R.ReadSingle(); SunDirection.z = GS.R.ReadSingle(); SunDirection.w = GS.R.ReadSingle();
    }

    public void SaveLavaShader()
    {
        GS.W.Write( LavaTextureID );
        GS.W.Write( LavaParticleForce );
        GS.W.Write( RedThreshold );
        GS.W.Write( LavaScale );
        GS.SColor( LavaColor );
        GS.SColor( DeepLavaColor );
        GS.W.Write( LavaColorIntensity );
        GS.W.Write( LavaWaveSpeed );
        GS.W.Write( LavaWaveIntensity );
        GS.W.Write( LavaWaveAngle );
        GS.W.Write( LavaWaveFrequency );
    }

    public void LoadLavaShader()
    {
        LavaTextureID = GS.R.ReadInt32();
        LavaParticleForce = GS.R.ReadSingle();
        RedThreshold = GS.R.ReadSingle();
        LavaScale = GS.R.ReadSingle();
        LavaColor = GS.LColor();
        DeepLavaColor = GS.LColor();
        LavaColorIntensity = GS.R.ReadSingle();
        LavaWaveSpeed = GS.R.ReadSingle();
        LavaWaveIntensity = GS.R.ReadSingle();
        LavaWaveAngle = GS.R.ReadSingle();
        LavaWaveFrequency = GS.R.ReadSingle();
    }

    public CubeData Clone()
    {
        return ( CubeData ) this.MemberwiseClone(); // cópia superficial
    }
}