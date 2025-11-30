using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
public enum EModAction
{
    NONE = -1, KillUnitIfChance, RotateUnitByRandomValue, SwapArtifacts, SetPrice
}
 
public enum EArtifactCreationMethod
{
    NONE = -1, BY_ORDER, RANDOM_FREE, RANDOM_NO_REPEAT, BY_MOD_NUMBER
}

public enum EFlipSector
{
    RAND = 0, FORCE_YES, FORCE_NO,
};
public class SectorDefinition : MonoBehaviour 
{
    //public List<string> MapTemplateNames;
    [Space(20)]
    [TabGroup( "Main" )]
    [Range( 1, 64 )]
    public int MinSectorNumber = 0;
    [Range( 1, 64 )]
    [TabGroup( "Main" )]
    public int MaxSectorNumber = 64;
    [Space( 20 )]
    [Range( 1, 200 )]
    [TabGroup( "Main" )]
    public int Height = 100;
    [TabGroup( "Main" )]
    public int UseCount;
    [Space( 10 )]
    [TabGroup( "Rand" )]
    public bool CreateRedArea = false;
    [TabGroup( "Rand" )]
    public bool IslandsRedAreaMode = false;
    [Space( 10 )]
    [TabGroup( "Rand" )]
    public bool CreateGreenArea = false;
    [TabGroup( "Rand" )]
    public bool IslandsGreenAreaMode = false;
    [Space( 10 )]
    [TabGroup( "Rand" )]
    public bool CreateBlueArea = false;
    [TabGroup( "Rand" )]
    public bool IslandsBlueAreaMode = false;
    [Space( 10 )]
    [TabGroup( "Rand" )]
    public bool CreateYellowArea = false;
    [TabGroup( "Rand" )]
    public bool IslandsYellowAreaMode = false;
    [Space( 10 )]
    [TabGroup( "Main" )]
    public EFlipSector FlipX = EFlipSector.RAND;
    [TabGroup( "Main" )]
    public EFlipSector FlipY = EFlipSector.RAND;
    [TabGroup( "Main" )]
    public bool AutoPickupArtifacts = false;
    [TabGroup( "Main" )]
    [Header( "Write Script Here!" )]
    [TextArea( 1, 20 )]
    public string Script = "";
    [TabGroup( "Link" )]
    public Mod[ ] ModList;
    [TabGroup( "Link" )]
    public List<TextAsset> MapTemplates, PuzzleTemplates;
    [TabGroup( "Link" )]
    public List<Vector2> SourcePos;   //Dont delete sourcepos now, wait until you finished all the transposition
    [TabGroup( "Link" )]
    public AreaDefinition[ ] ADList;
    [TabGroup( "Link" )]
    public EModAction[ ] ModAction;
    [TabGroup( "Link" )]
    public float[ ] ModValue;
    [TabGroup( "Old" )]
    public List<float> ExtraLevelPerUniqueMonster;                   // Unique random monster

    public void Reset()
    {
        UseCount = 0;
    }
}
