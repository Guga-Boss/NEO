using UnityEngine;

[System.Serializable]
public class LightSource2D
{
    [HideInInspector]
    public Vector2 worldPosition;
    [HideInInspector]
    public float radius = 2f;
    //[HideInInspector]
    public int Type = -1;
    [HideInInspector]
    public int ID = -1;
    public float intensity = 1f;
    public Color innerColor = new Color( 1f, 0.5f, 0.3f, 1f ); // amarelo alaranjado
    public Color outerColor = new Color( 0.2f, 0.4f, 1f, 1f ); // azul
    [Range( 0, 1 )]
    public float colorBlend = 0.5f;
    [Range( 1, 10 )]
    public float falloffCurve = 3f;

    [HideInInspector]
    public float noiseSeed;

    public LightSource2D Clone()
    {
        return ( LightSource2D ) this.MemberwiseClone(); // cópia superficial
    }

    public void Save()
    {
         GS.W.Write( Type );
         GS.W.Write( ID );
         GS.SVector2( worldPosition );
         GS.SColor( innerColor );
         GS.SColor( outerColor );
         GS.W.Write( radius );
         GS.W.Write( intensity );
         GS.W.Write( colorBlend );
         GS.W.Write( falloffCurve );
    }

    public void Load()
    {
        Type = GS.R.ReadInt32();
        ID = GS.R.ReadInt32();
        worldPosition = GS.LVector2();
        innerColor = GS.LColor();
        outerColor = GS.LColor();
        radius = GS.R.ReadSingle();
        intensity = GS.R.ReadSingle();
        colorBlend = GS.R.ReadSingle();
        falloffCurve = GS.R.ReadSingle();
    }

    internal void Copy( LightSource2D l, bool copypos = true )
    {
        if( copypos )
        {
            worldPosition = l.worldPosition;
            ID = l.ID;
        }
        Type = l.Type;
        innerColor = l.innerColor;
        outerColor = l.outerColor;
        radius = l.radius;
        intensity = l.intensity;
        colorBlend = l.colorBlend;
        falloffCurve = l.falloffCurve;
    }
    public static void Create( LightSource2D type, int id = -1 )
    {
        LightSource2D light = new LightSource2D();
        light.Copy( type );                                                              // create light source
        light.ID = id;
        Map.I.Lights.lights.Add( light );  
    }
    public static void Create( LightSource2D type, Vector3 tg )
    {
        LightSource2D light = new LightSource2D();
        light.Copy( type );                                                              // create light source
        light.worldPosition = tg;
        Map.I.Lights.lights.Add( light );
    }
}
