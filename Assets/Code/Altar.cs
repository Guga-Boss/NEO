using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using PathologicalGames;
public enum EAltarBonusType
{
    ANY = -2, NONE = -1, Give_Bonus, Hang_Bonus, Lock, Angle_Modifier, Rotate_CW, Rotate_CCW,
    Blade, Axe, Dice, Add_Slot, Vault, Power_Booster, Bump_Cost, Prize, Invert_Bonus,
    Rotation_Speed, Bank, Antenna, Horse_Shoe, Bulls_Eye, Bonus_Multiplier,
    Bonus_Adder, Stacker, Spawn_Random_Altar
}

public enum EAltarBonusScope
{
    NONE = -1, Neighbors, Invert, Fork, T, Butcher, Cube   // TODO: Aligned
}

[System.Serializable]
public class AltarBonusStruct
{
    #region Variables
    [TabGroup( "Main" )]
    public EAltarBonusType AltarBonusType = EAltarBonusType.NONE;
    [TabGroup( "Main" )]
    public EAltarBonusScope Scope = EAltarBonusScope.Butcher;
    [TabGroup( "Main" )]
    public float AltarBonusFactor = 1;
    [TabGroup( "Main" )]
    public ItemType AltarBonusItem = ItemType.NONE;
    [TabGroup( "Altar" )]
    public bool Activated = true;
    [TabGroup( "Altar" )]
    public bool Fixed = false;
    [TabGroup( "Altar" )]
    public float SpeedFactor = 100;
    [TabGroup( "Altar" )]
    public float WorkingAngle = 15;
    [TabGroup( "Altar" )]
    public float BullsEyeFactor = 0;
    [TabGroup( "Altar" )]
    public float ShieldTimer = 0;
    [TabGroup( "Altar" )]
    [HideInInspector]
    public float HangTimer = 0;
    [TabGroup( "Data" )]
    public float StackerMultiplier = 1;
    [TabGroup( "Data" )]
    public float PowerBoosterCount = 0;
    [TabGroup( "Altar" )]
    public float TotalHangTimer = -1;
    [TabGroup( "Data" )]
    public Altar Altar;
    [TabGroup( "Data" )]
    public float AdderCount = 0;
    [TabGroup( "Data" )]
    public float MultiplierCount = 0;
    [TabGroup( "Data" )]
    public float LockCount = 0;
    [TabGroup( "Data" )]
    public float StackerCount = 0;
    [TabGroup( "Data" )]
    public float SafeCount = 0;
    [TabGroup( "Data" )]
    public float RotationModCount = 0;
    [TabGroup( "Data" )]
    public float RotationModCountDisp = 0;
    [TabGroup( "Data" )]
    public float AngleModCount = 0;
    [TabGroup( "Data" )]
    public float AngleModCountDisp = 0;
    [TabGroup( "Data" )]
    public int ID = 0;
    [TabGroup( "Data" )]
    public bool Advanced = false;
    [TabGroup( "Data" )]
    public int TimesUsed = 0;
    [TabGroup( "Data" )]
    public int HitCount = -1;
    [TabGroup( "Data" )]
    public float UsageAvailableTimer = 0;
    [TabGroup( "Data" )]
    public float SelfKillTimer = -1;
    [TabGroup( "Data" )]
    public bool Unlocked = false;
    [TabGroup( "Chance" )]
    public float MinChance = 0;
    [TabGroup( "Chance" )]
    public float MaxChance = 50;
    [TabGroup( "Sort" )]
    public int MinLevel = 1;
    [TabGroup( "Sort" )]
    public float SortFactor = 100;
    public static int BoltSpriteID;
    public static float BumpTimeCount = 0, BumpPrecision = -1;
    public static bool BumpSuccessful = false;
    private static readonly System.Random rng = new System.Random();
    #endregion
    public void Reset()
    {
        AltarBonusType = EAltarBonusType.NONE;
        HitCount = 0;
        UsageAvailableTimer = 0;
        SelfKillTimer = -1;
        AltarBonusItem = ItemType.NONE;
        ShieldTimer = 0;
        HangTimer = 0;
        Scope = EAltarBonusScope.NONE;
    }
    public void Save()
    {
        GS.W.Write( ID );
        GS.W.Write( ( int ) AltarBonusType );
        GS.W.Write( AltarBonusFactor );
        GS.W.Write( ( int ) AltarBonusItem );
        GS.W.Write( SpeedFactor );
        GS.W.Write( WorkingAngle );
        GS.W.Write( BullsEyeFactor );
        GS.W.Write( Advanced );
        GS.W.Write( TimesUsed );
        GS.W.Write( UsageAvailableTimer );
        GS.W.Write( SelfKillTimer );
        GS.W.Write( Unlocked );
        GS.W.Write( MinChance );
        GS.W.Write( MaxChance );
        GS.W.Write( HitCount );
        GS.W.Write( MinLevel );
        GS.W.Write( Fixed );
        GS.W.Write( SortFactor );
        GS.W.Write( Activated );
        GS.W.Write( ShieldTimer );
        GS.W.Write( HangTimer );
        GS.W.Write( TotalHangTimer );
        GS.W.Write( MultiplierCount );
        GS.W.Write( AdderCount );
        GS.W.Write( LockCount );
        GS.W.Write( ( int ) Scope );
        GS.W.Write( StackerCount );
        GS.W.Write( SafeCount );
        GS.W.Write( RotationModCount );
        GS.W.Write( AngleModCount );
        GS.W.Write( StackerMultiplier );
        GS.W.Write( PowerBoosterCount ); 
    }

    public void Load()
    {
        ID = GS.R.ReadInt32();
        AltarBonusType = ( EAltarBonusType ) GS.R.ReadInt32();
        AltarBonusFactor = GS.R.ReadSingle();
        AltarBonusItem = ( ItemType ) GS.R.ReadInt32();
        SpeedFactor = GS.R.ReadSingle();
        WorkingAngle = GS.R.ReadSingle();
        BullsEyeFactor = GS.R.ReadSingle();
        Advanced = GS.R.ReadBoolean();
        TimesUsed = GS.R.ReadInt32();
        UsageAvailableTimer = GS.R.ReadSingle();
        SelfKillTimer = GS.R.ReadSingle();
        Unlocked = GS.R.ReadBoolean();
        MinChance = GS.R.ReadSingle();
        MaxChance = GS.R.ReadSingle();
        HitCount = GS.R.ReadInt32();
        MinLevel = GS.R.ReadInt32();
        Fixed = GS.R.ReadBoolean();
        SortFactor = GS.R.ReadSingle();
        Activated = GS.R.ReadBoolean();
        ShieldTimer = GS.R.ReadSingle();
        HangTimer = GS.R.ReadSingle();
        TotalHangTimer = GS.R.ReadSingle();
        MultiplierCount = GS.R.ReadSingle();
        AdderCount = GS.R.ReadSingle();
        LockCount = GS.R.ReadSingle();
        Scope = ( EAltarBonusScope ) GS.R.ReadInt32();
        StackerCount = GS.R.ReadSingle();
        SafeCount = GS.R.ReadSingle();
        RotationModCount = GS.R.ReadSingle();
        AngleModCount = GS.R.ReadSingle();
        StackerMultiplier = GS.R.ReadSingle();
        PowerBoosterCount = GS.R.ReadSingle();
    }

    public void Copy( AltarBonusStruct al )
    {
        AltarBonusType = al.AltarBonusType;
        AltarBonusFactor = al.AltarBonusFactor;
        MinChance = al.MinChance;
        MaxChance = al.MaxChance;
        Unlocked = al.Unlocked;
        TimesUsed = al.TimesUsed;
        Advanced = al.Advanced;
        HitCount = al.HitCount;
        AltarBonusItem = al.AltarBonusItem;
        UsageAvailableTimer = al.UsageAvailableTimer;
        SelfKillTimer = al.SelfKillTimer;
        MinLevel = al.MinLevel;
        SpeedFactor = al.SpeedFactor;
        WorkingAngle = al.WorkingAngle;
        BullsEyeFactor = al.BullsEyeFactor;
        Fixed = al.Fixed;
        SortFactor = al.SortFactor;
        Activated = al.Activated;
        ShieldTimer = al.ShieldTimer;
        HangTimer = al.HangTimer;
        TotalHangTimer = al.TotalHangTimer;
        MultiplierCount = al.MultiplierCount;
        AdderCount = al.AdderCount;
        LockCount = al.LockCount;
        StackerCount = al.StackerCount;
        SafeCount = al.SafeCount;
        RotationModCount = al.RotationModCount;
        AngleModCount = al.AngleModCount;
        Scope = al.Scope;
        StackerMultiplier = al.StackerMultiplier;
        PowerBoosterCount = al.PowerBoosterCount;
    }
    public float GetFact( bool disp = false, EAltarBonusType type = EAltarBonusType.NONE, float fact = 0 )
    {
        if( fact == 0 ) fact = AltarBonusFactor;
        if( type == EAltarBonusType.Rotation_Speed )
        {
            fact = RotationModCount;
            if( disp ) fact = RotationModCountDisp;
            if( disp && Altar.RandomAltar == true ) 
                fact = AltarBonusFactor;
        }
        if( type == EAltarBonusType.Angle_Modifier )
        {
            fact = AngleModCount;
            if( disp ) fact = AngleModCountDisp;
            if( disp && Altar.RandomAltar == true )
                fact = AltarBonusFactor;
        }
        float mult = MultiplierCount;
        if( mult == 0 ) mult = 1;
        if( AltarBonusType == EAltarBonusType.Bonus_Multiplier ) 
            mult = 1;                                                         // to avoid multiplier x 0
        float stack = 0;
        if( Altar )
            if( StackerCount > 0 )
                stack = ( Altar.StackCount * StackerCount );                                // Stacker uses the Altar stack variable. Use multiplier for more than 1 bonus
        return ( fact + ( AdderCount + stack ) ) * mult;
    }
}

public class Altar : MonoBehaviour
{
    [TabGroup( "Main" )]
    public Unit Unit;
    [TabGroup( "Main" )]
    public Body Body;
    [TabGroup( "Main" )]
    public AltarBonusStruct NextAltarBonus;
    [TabGroup( "Main" )]
    public int StackCount = 0;
    [TabGroup( "Main" )]
    public bool RandomAltar = false;
    [TabGroup( "Link" )]
    public List<AltarBonusStruct> PoleObjList;
    [TabGroup( "Link" )]
    public List<tk2dSprite> ScopeObjList;
    [TabGroup( "List" )]
    public List<AltarBonusStruct> AltarBonusList;
    public static List<AltarBonusStruct> Bnl = new List<AltarBonusStruct>();
    public static List<ItemType> ItBnl = new List<ItemType>();
    public static List<Unit> Al = new List<Unit>();

    public void UpdateAltar()
    {
        if( Map.I.IsPaused() ) return;

        UpdateAltarRotation();

        UpdateSprites();

        UpdateAltarPole();

        UpdateButcher();
    }
    private void UpdateButcher()
    {
        if( Map.I.TurnFrameCount == 3 )
        {
            Al = GetAltars();
            UpdateBonusData( EAltarBonusType.Lock, Unit );                                                           // Bonus Lock calculation --- updates data for correct text 
            UpdateBonusData( EAltarBonusType.Bonus_Adder, Unit );                                                    // Bonus Adder calculation
            UpdateBonusData( EAltarBonusType.Bonus_Multiplier, Unit );                                               // Bonus Multiplier calculation
            UpdateBonusData( EAltarBonusType.Bank, Unit );                                                           // Bonus Safe calculation
            UpdateBonusData( EAltarBonusType.Stacker, Unit );                                                        // Bonus Stacker calculation
            UpdateBonusData( EAltarBonusType.Rotation_Speed, Unit );                                                 // Bonus rotaton speed calculation
            UpdateBonusData( EAltarBonusType.Angle_Modifier, Unit );                                                 // Bonus Angle modifier calculation
        }
    }
    public void Copy( Altar al )
    {
        AltarBonusList = new List<AltarBonusStruct>();
        StackCount = al.StackCount;
        RandomAltar = al.RandomAltar;
    }
    public void Save()
    {
        GS.W.Write( StackCount );
        GS.W.Write( RandomAltar );
        NextAltarBonus.Save();
        GS.W.Write( PoleObjList.Count );
        for( int i = 0; i < PoleObjList.Count; i++ )
            PoleObjList[ i ].Save();
        GS.W.Write( AltarBonusList.Count );
        for( int i = 0; i < AltarBonusList.Count; i++ )
            AltarBonusList[ i ].Save();
        GS.SVector3( Unit.Spr.transform.eulerAngles );
        GS.SVector3( Unit.Body.Sprite5.transform.eulerAngles );
    }
    public void Load()
       {
           StackCount = GS.R.ReadInt32();
           RandomAltar = GS.R.ReadBoolean();
           NextAltarBonus.Load();
           int sz = GS.R.ReadInt32();
           PoleObjList = new List<AltarBonusStruct>();
           for( int i = 0; i < sz; i++ )
           {
               var item = new AltarBonusStruct();   
               item.Load();                         
               PoleObjList.Add( item );               
           }
           sz = GS.R.ReadInt32();
           AltarBonusList = new List<AltarBonusStruct>();
           for( int i = 0; i < sz; i++ )
           {
               var item = new AltarBonusStruct();
               item.Load();
               AltarBonusList.Add( item );
           }
           Unit.Spr.transform.eulerAngles =  GS.LVector3();
           Unit.Body.Sprite5.transform.eulerAngles = GS.LVector3();
       }
    public void UpdateAltarRotation()
    {
        EDirection dr = Util.GetTargetUnitDir( G.Hero.Pos, Unit.Pos );
        dr = Util.GetInvDir( dr );
        AltarBonusStruct al = Unit.Altar.AltarBonusList[ ( int ) dr ];

        bool neigh = false;
        if( Util.IsNeighbor( G.Hero.Pos, Unit.Pos ) &&                                   // is hero neighbor to al?
            al.AltarBonusType != EAltarBonusType.NONE ) 
            neigh = true;

        float speed = Body.RotationSpeed;                                                // Pole Rotation
        if( neigh )
        {
            speed = Util.Percent( al.SpeedFactor, speed );                               // multiply x bn speed factor
            float fact = al.GetFact( false, EAltarBonusType.Rotation_Speed );
            speed -= Util.Percent( fact, speed );                                        // DECREASES passive bonus % from Speed Modifier
        }

        float rot = Unit.Spr.transform.eulerAngles.z;
        if( Unit.Dir == EDirection.W )
        {
            rot += Time.deltaTime * speed;
            if( rot >= 360 ) rot -= 360;
        }
        if( Unit.Dir == EDirection.E )
        {
            rot -= Time.deltaTime * speed;
            if( rot < 0 ) rot += 360;
        }

        float rot2 = Body.Sprite5.transform.eulerAngles.z;                                                  // Bump indicator

        if( Unit.Dir == EDirection.W )
        {
            rot2 += Time.deltaTime * speed;
            if( rot2 >= 360 ) rot2 -= 360;
        }
        if( Unit.Dir == EDirection.E )
        {
            rot2 -= Time.deltaTime * speed;
            if( rot2 < 0 ) rot2 += 360;
        }
        Body.Sprite5.transform.eulerAngles = new Vector3( 0, 0, rot2 );
        
        Body.Shadow.gameObject.SetActive( false );
        if( Unit.Control.WaitTimeCounter > 0 )
        {
            Unit.Control.WaitTimeCounter -= Time.deltaTime;                                                  // wait timer decrement
            Body.Shadow.gameObject.SetActive( true );                                                        // shadow marked after bump
        }

        Body.Sprite6.gameObject.SetActive( false );                                                          // Other bump indicators
        Body.Sprite7.gameObject.SetActive( false ); 
        Body.Sprite8.gameObject.SetActive( false );
        Unit.LevelTxt.gameObject.SetActive( false );

        if( neigh )
        {
            Body.Sprite6.gameObject.SetActive( true );                                                       // activate objects
            Body.Sprite7.gameObject.SetActive( true );
            Body.Sprite8.gameObject.SetActive( true );
            Unit.LevelTxt.gameObject.SetActive( true );
            float angle = Util.GetRotationAngleVector( dr ).z;
            float dif = al.WorkingAngle;

            float fact = al.GetFact( false, EAltarBonusType.Angle_Modifier );
            dif += Util.Percent( fact, dif );                                                                // add passive bonus % from angle Modifier. Caution: Angle is added

            Body.Sprite6.transform.eulerAngles = new Vector3( 0, 0, angle - dif );                           // markers angle
            Body.Sprite7.transform.eulerAngles = new Vector3( 0, 0, angle + dif );
            Body.Sprite8.transform.eulerAngles = new Vector3( 0, 0, angle );
            float horseshoe = GetAltarBonusSum( EAltarBonusType.Horse_Shoe, Unit, ( int ) dr );              // Horseshoe
            float maxc = al.MaxChance + horseshoe;
            float minc = al.MinChance + horseshoe;
            string nm = Util.GetName( al.AltarBonusType.ToString() );
            Unit.LevelTxt.text = "" + nm + "\n" + minc.ToString( "0." ) + 
            "% to " + maxc.ToString( "0." ) + "%";                                                           //text mesh
            if( AltarBonusStruct.BumpSuccessful )
                Unit.LevelTxt.color = Color.green;
            else
                Unit.LevelTxt.color = Color.red;
            if( Unit.Control.WaitTimeCounter > 0 )
                Unit.LevelTxt.text += "  " + AltarBonusStruct.BumpPrecision.ToString( "0.#" ) + "%";         // Display precision after bump
            else
                Unit.LevelTxt.color = Color.white;
            Unit.LevelTxt.transform.eulerAngles = new Vector3( 0, 0, 0 );
        }
        Unit.Spr.transform.eulerAngles = new Vector3( 0, 0, rot );                                           // Rotates Sprite and pole
    }
    public void UpdateSprites()
    {
        Body.Sprite2.gameObject.SetActive( false );
        Body.Sprite4.gameObject.SetActive( false );
        //if( Unit.Variation >= 1 )
        {
            if( NextAltarBonus.AltarBonusType != EAltarBonusType.NONE )
                Body.Sprite2.gameObject.SetActive( true );
            Body.Sprite2.spriteId = 647 + ( int ) NextAltarBonus.AltarBonusType;                    // Next Bonus sprite
            AltarBonusStruct.BumpTimeCount -= Time.deltaTime;

            if( NextAltarBonus.AltarBonusItem != ItemType.NONE )
            {
                Body.Sprite4.gameObject.SetActive( true );                                          // Next bonus item sprite
                Body.Sprite4.spriteId = G.GIT( NextAltarBonus.AltarBonusItem ).TKSprite.spriteId;
            }
        }

        if( Body.Level > 1 )                                                                         // Altar level
        {
            Unit.RightText.gameObject.SetActive( true );
            Unit.RightText.text = "lv " + ( int ) Body.Level;
        }

        bool haschild = false;
        for( int i = 0; i < 8; i++ )
        {
            AltarBonusStruct al = AltarBonusList[ i ];
            if( al.Altar == null ) al.Altar = this;
            if( AltarBonusStruct.BumpTimeCount <= 0 )
            {
                Body.BabySprite[ i ].transform.eulerAngles = new Vector3( 0, 0, 0 );
                if( al.AltarBonusType == EAltarBonusType.Rotate_CW  ||                                   // Rotation icon rotates acording to its position
                    al.AltarBonusType == EAltarBonusType.Rotate_CCW ||                                   // Rotation icon rotates acording to its position
                    al.AltarBonusType == EAltarBonusType.Invert_Bonus )
                {
                    Vector3 rot = Util.GetRotationAngleVector( ( EDirection ) i );
                    Body.BabySprite[ i ].transform.eulerAngles = rot;
                }

                Body.BabySprite[ i ].spriteId = 647 + ( int ) al.AltarBonusType;                          // main sprite id

                Body.BabySprite[ i ].color = Color.white;
                Body.BabyBackSprite[ i ].color = Color.white;
                Body.TextList[ i ].color = Color.white; 
                if( al.Activated == false )
                {
                    float a = .4f;
                    Body.BabySprite[ i ].color = new Color( 1, 1, 1, a );                                 // sprite color
                    Body.BabyBackSprite[ i ].color = new Color( 1, 1, 1, a );
                    Body.TextList[ i ].color = new Color( 1, 1, 1, a );
                }

                Body.BabyBackSprite[ i ].gameObject.SetActive( false );                                   // Item sprite update
                if( al.AltarBonusItem != ItemType.NONE )
                {
                    Body.BabyBackSprite[ i ].spriteId =
                    G.GIT( al.AltarBonusItem ).TKSprite.spriteId;
                    Body.BabyBackSprite[ i ].gameObject.SetActive( true );
                }
                Unit.Altar.ScopeObjList[ i ].gameObject.SetActive( false );                               
                if( al.Scope != EAltarBonusScope.NONE )                                                   // Scope Item sprite update
                if( al.Scope != EAltarBonusScope.Cube )
                if( al.Scope != EAltarBonusScope.Butcher )
                {
                    Unit.Altar.ScopeObjList[ i ].spriteId = 679 + ( int ) al.Scope;
                    Unit.Altar.ScopeObjList[ i ].gameObject.SetActive( true );
                }
            }
            
            if( al.Scope == EAltarBonusScope.Cube )
            {
                float amplitude = 0.07f; float val = .85f;
                val += Mathf.Sin( Time.fixedTime * Mathf.PI * 2 ) * amplitude;                            // scale cube Scope sprite animation
                Body.BabySprite[ i ].transform.localScale = new Vector3( val, val, 1 );
            }
            else
            {
                if( Map.I.AdvanceTurn )
                    Body.BabySprite[ i ].transform.localScale = new Vector3( .5f, .5f, 1 );
            }

            Body.HasBaby[ i ] = false;
            Body.BabySprite[ i ].gameObject.SetActive( false );
            Body.TextList[ i ].gameObject.SetActive( false );

            if( AltarBonusList[ i ].AltarBonusType != EAltarBonusType.NONE )                              // Baby sprite
            {
                Body.HasBaby[ i ] = true;
                if( AltarBonusList[ i ].AltarBonusType != EAltarBonusType.Give_Bonus )
                Body.BabySprite[ i ].gameObject.SetActive( true );
                haschild = true;
                string sym = "";
                string post = "";
                string tostring = "+0.#;-0.#;0";
                if( al.AltarBonusType == EAltarBonusType.Power_Booster  ||
                    al.AltarBonusType == EAltarBonusType.Horse_Shoe     ||
                    al.AltarBonusType == EAltarBonusType.Bulls_Eye      ||
                    al.AltarBonusType == EAltarBonusType.Bank           ||
                    al.AltarBonusType == EAltarBonusType.Angle_Modifier ||
                    al.AltarBonusType == EAltarBonusType.Rotation_Speed )
                    post = "%";                                                                        // add post % to these

                if( al.AltarBonusType == EAltarBonusType.Bonus_Multiplier )
                {
                    sym = "x";
                    tostring = "0.#";                                                                  // multiplier
                }
                Body.TextList[ i ].color = Color.white;
                Body.TextList[ i ].gameObject.SetActive( true );

                float fact = AltarBonusList[ i ].GetFact( true, al.AltarBonusType );                   // get main factor power

                if( al.AltarBonusType == EAltarBonusType.Rotation_Speed )                              // these ones have negative effect as default
                    fact *= -1;

                if( al.AltarBonusType == EAltarBonusType.Stacker )                                     //stacker uses altar data
                {
                    if( fact > 0 )
                        fact = Unit.Altar.StackCount;
                }
                Body.TextList[ i ].text = sym + fact.ToString( tostring ) + post;                     // Update baby text
                if( fact <= 1 )
                if( al.AltarBonusType == EAltarBonusType.Bump_Cost  ||
                    al.AltarBonusType == EAltarBonusType.Hang_Bonus ||
                    al.AltarBonusType == EAltarBonusType.Prize      ||
                    al.AltarBonusType == EAltarBonusType.Dice      )                                 // dont show number if amount = to 1 for these 
                    Body.TextList[ i ].text = "";
                if( fact <= 0 )
                if( al.AltarBonusType == EAltarBonusType.Stacker )                                   // dont show number if amount = 0 for these 
                    Body.TextList[ i ].text = "";

               if( al.AltarBonusType == EAltarBonusType.Antenna       ||
                    al.AltarBonusType == EAltarBonusType.Lock         ||
                    al.AltarBonusType == EAltarBonusType.Rotate_CW    ||
                    al.AltarBonusType == EAltarBonusType.Rotate_CCW   ||
                    al.AltarBonusType == EAltarBonusType.Invert_Bonus )                                // dont show numbers for these
                    Body.TextList[ i ].text = "";

                if( al.LockCount > 0 )
                {
                    Body.TextList[ i ].text += " lock";                                               // lock text message
                    Body.TextList[ i ].color = Color.red;
                }
                if( al.AltarBonusType == EAltarBonusType.Spawn_Random_Altar )                         // Spawn Butcher
                {
                    Body.TextList[ i ].text = "Spawn + " + al.AltarBonusFactor;
                }
            }
        }
        Body.Sprite5.gameObject.SetActive( haschild );                                                // Indicator enabled or not
    }
    public void UpdateAltarPole()
    {
        int sz = 3;
        for( int i = 0; i < PoleObjList.Count; i++ )
        {
            AltarBonusStruct al = PoleObjList[ i ];
            Body.PoleSpriteList[ i ].gameObject.SetActive( false );
            Body.PoleBackSpriteList[ i ].gameObject.SetActive( false );
            if( al.AltarBonusType != EAltarBonusType.NONE )
            {
                if( i > 2 ) sz = i + 1;
                tk2dSprite spr = Body.PoleSpriteList[ i ];
                spr.gameObject.SetActive( true );
                if( al.ShieldTimer > 0 )
                    Body.PoleBackSpriteList[ i ].gameObject.SetActive( true );                                       // Shield timer

                al.ShieldTimer -= Time.deltaTime;
                if( Body.PoleBonusAvailableAngle != -1 )                                                             // Pole bonus Shield protecting items
                {
                    bool res = Util.IsWithinAngleRange( Unit.Spr.transform, Body.PoleBonusAvailableAngle, 10 );
                    Body.PoleBackSpriteList[ i ].gameObject.SetActive( true );
                    if( res || al.AltarBonusType != EAltarBonusType.Hang_Bonus )
                        Body.PoleBackSpriteList[ i ].gameObject.SetActive( false );
                }
                al.HangTimer -= Time.deltaTime;
                if( al.TotalHangTimer > 0 )
                if( al.HangTimer <= 0 )
                {
                    al.Reset();                                                                        // Hanged object kill after time is up
                    Controller.CreateMagicEffect( spr.transform.position );
                    MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );
                }

                spr.spriteId = 647 + ( int ) al.AltarBonusType;

                if( al.AltarBonusItem != ItemType.NONE )
                {
                    spr.spriteId = G.GIT( al.AltarBonusItem ).TKSprite.spriteId;                               // Hanged item sprite                    
                    spr.Collection = Map.I.SpriteCollectionList[ ( int ) ESpriteCol.ITEM ];
                }
                else
                {
                    spr.Collection = Map.I.SpriteCollectionList[ ( int ) ESpriteCol.MONSTER_ANIM ];    // Other hanged non item object
                }
                spr.scale = new Vector3( 1, 1, 1 );
                if( al.AltarBonusType == EAltarBonusType.Axe )                                         // sprite rotation
                    spr.transform.Rotate( new Vector3( 0, 0, 300 * Time.deltaTime ) );
                else
                if( al.AltarBonusType == EAltarBonusType.Blade )
                {
                    spr.transform.rotation = Unit.Spr.transform.rotation;                              // same rotation as pole for these
                    spr.scale = new Vector3( 2, 1, 1 );
                }
                else
                    spr.transform.eulerAngles = Vector3.zero;

                al.UsageAvailableTimer -= Time.deltaTime;

                if( al.SelfKillTimer != -1 )
                {
                    al.SelfKillTimer -= Time.deltaTime;
                    if( al.SelfKillTimer <= 0 ) 
                        al.Reset();
                }

                UpdateAltarAttack( al, spr, G.Hero, i );                                               // Hero spike damage

                int posx = 0, posy = 0;
                Map.I.Tilemap.GetTileAtPosition( spr.transform.position, out posx, out posy );        // Monsters Axe damage
                Unit un = Map.I.GetUnit( new Vector2( posx, posy ), ELayerType.MONSTER );
                if( un && un.ValidMonster ) UpdateAltarAttack( al, spr, un, i );
                List<Unit> fl = Map.I.GetFUnit( new Vector2( posx, posy ) );                          // Flying Axe damage
                if ( fl != null )
                for( int f = 0; f < fl.Count; f++ )
                    {
                        if( fl[ f ].ValidMonster )
                            UpdateAltarAttack( al, spr, fl[ f ], i );
                    }
            }
        }            

        float h = 64 * 3;
        if( sz > 2 ) h = ( 64 * sz );
        Body.PoleSprite.dimensions = new Vector2( 64, h );                                             // Pole sprite size
    }
    private void UpdateAltarAttack( AltarBonusStruct al, tk2dSprite spr, Unit un, int id )
    {
        if( Body.PoleBackSpriteList[ id ].gameObject.activeSelf ) return;
        if( al.UsageAvailableTimer > 0 ) return;
        float dist = Vector2.Distance( un.Spr.transform.position, spr.transform.position );     // distance calculation
        if( dist > .5f ) return;

        if( al.AltarBonusType == EAltarBonusType.Blade )                                        // Spike damage
        {
            if( un.UnitType == EUnitType.HERO )
                AltarAttack( al, un );
        }
        else
        if( al.AltarBonusType == EAltarBonusType.Axe )                                         // Axe damage
        {
            if( un.UnitType == EUnitType.MONSTER )
                AltarAttack( al, un );
        }
        else
        if( un.UnitType == EUnitType.HERO )                                                    // Item hanged collect
        {
            Vector2 tg = Body.PoleBackSpriteList[ id ].transform.position;
            Item.AddItem( al.AltarBonusItem, 1 );
            MasterAudio.PlaySound3DAtVector3( "Click 2", tg );
            Controller.CreateMagicEffect( tg );                                                 // Create Magic effect
            al.Reset();
        }
    }
    private void AltarAttack( AltarBonusStruct al, Unit un )
    {
        if( al.HitCount != -1 &&
          --al.HitCount <= 0 )
        {
            al.Reset();
        }
        un.Body.ReceiveDamage( Unit.Md.SpikeDamage, EDamageType.BLEEDING, Unit, null );
        un.Body.CreateDamageAnimation( un.Pos, Unit.Md.SpikeDamage, un, EDamageType.NONE, true );
        al.UsageAvailableTimer = 2;
        MasterAudio.PlaySound3DAtVector3( "Knife Slice", transform.position );                                    // Sound FX
    }
    public static void UpdateAltarBump( Vector2 tg )
    {
        if( Map.I.CheckArrowBlockFromTo( G.Hero.Pos, tg, G.Hero ) ) return;                                       // Arrow Block
        Unit altar = Map.I.GetUnit( ETileType.ALTAR, tg );                                                        // No Altar in the target
        if( altar == null ) return;
        Controller.StitchesPunishment = false;
        if( altar.Control.WaitTimeCounter > 0 ) return;

        if( altar.Altar.RandomAltar )
        {
            if( Map.I.TimeKeyPressing > 3 )
            {
                Map.Kill( altar, true );                                                                          // kill random altar by holding keys to avoid blocking hero
                Map.I.InvalidateInputTimer = 1f;
                return;
            }
            if( Item.GetNum( ItemType.Res_Butcher_Coin ) <= 1 )                                                   // auto kill random altar if out of coins in a few seconds
                Map.TimeKill( altar, 5 ); 
        }

        EDirection dr = Util.GetTargetUnitDir( G.Hero.Pos, tg );
        UpdateAntenna( tg, altar, ( int ) dr );                                                                   // Updates Antenna Bump

        dr = Util.GetInvDir( dr );
        AltarBonusStruct bumpBn = altar.Altar.AltarBonusList[ ( int ) dr ];
        if( bumpBn.AltarBonusType == EAltarBonusType.NONE ) return;

        UpdateBonusData( EAltarBonusType.Lock, altar );                                                           // Bonus Lock calculation

        if( bumpBn.LockCount > 0 )
        {
            Message.RedMessage( "Lock Preventing Activation!" );                                                  // lock message and quit
            return;
        }

        UpdateBonusData( EAltarBonusType.Bonus_Adder, altar );                                                    // Bonus Adder calculation
        UpdateBonusData( EAltarBonusType.Bonus_Multiplier, altar );                                               // Bonus Multiplier calculation
        UpdateBonusData( EAltarBonusType.Bank, altar );                                                           // Bonus Safe calculation

        if( CheckAltarPurchase( altar, bumpBn ) == false ) return;                                                // Checks purchase

        float angle = Util.GetRotationAngleVector( dr ).z;
        if( dr == EDirection.N )
        if( altar.Body.Sprite5.transform.eulerAngles.z > 180 )
            angle = 360;
        float dif = altar.Body.Sprite5.transform.eulerAngles.z - angle;                                           // angle calculations
        dif = Util.Mod( dif );
        float max = bumpBn.WorkingAngle;

        float fact = bumpBn.GetFact( false, EAltarBonusType.Angle_Modifier );
        max += Util.Percent( fact, max );                                                                         // add passive bonus % from angle Modifier. Caution: Angle is added

        float angperc = Util.GetPercent( dif, max );
        angperc = 100 - angperc;         

        if( angperc >=0  )
        {
            // ➕ curva côncava aqui:
            float norm = angperc / 100f;                                                                          // usa curvas para premiar apenas precisao
            float exponent = CubeData.I.AltarCurve;                                                               // experimente com 1.5, 2.0, 3.0 - acima de 2 em muito dificil ideal 1.2 a 1.5 - peca para GPT para ver tabela topico: "jogo butcher altar pole"
            if( altar.Altar.RandomAltar )
                exponent = CubeData.I.RandomAltarCurve;
            float curved = Mathf.Pow( norm, exponent );
            angperc = curved * 100f;
        }

        bool ok = true;
        Color col = Color.green;
        float horseshoe = GetAltarBonusSum( EAltarBonusType.Horse_Shoe, altar, ( int ) dr );                     // Horseshoe
        float maxc = bumpBn.MaxChance + horseshoe;
        float minc = bumpBn.MinChance + horseshoe;
        float perc = minc + Util.Percent( angperc, maxc - minc );

        float bulleye = bumpBn.BullsEyeFactor;
        bulleye += GetAltarBonusSum( EAltarBonusType.Bulls_Eye, altar, ( int ) dr );                            // Bulls eye
        int times = 1;
        if( perc > maxc - bulleye )                                                                             // Bulls eye ok!
        {
            times = 2;
        }

        if( perc < minc ) perc = 0;
        //Map.I.Deb = " ang " + altar.Body.Sprite5.transform.eulerAngles.z + " dr  " + dr +
        //" ang " + angle + " dif  " + dif + " " + angperc + "%   perc: " + perc + "% ";
        //perc = 90;
        if( perc < 0 ) perc = 0;

        altar.Body.Shadow.gameObject.SetActive( true );                                                         // activates shadow sprite
        altar.Body.Shadow.transform.eulerAngles = altar.Body.Sprite5.transform.eulerAngles;

        if( Manager.I.GugaVersion )
        if( Input.GetKey( KeyCode.F1 ) )                                                                        // F1 Debug Win forced
            perc = 100;  

        int sorttime = 0;
        for( int i = 0; i < times; i++ )
        {
            ok = Util.Chance( perc  );                                                                          // Sorting
            if( ok ) { sorttime = i; break; }
        }

        string msg = "" + perc.ToString( "0.#" ) + "%";
        if( bumpBn.AltarBonusType == EAltarBonusType.NONE ) return;
        AltarBonusStruct.BumpPrecision = perc;

        if( ok == true )                                                                                        // Bump Successful
        {
            if( times > 1 )
                msg += " Bull's Eye " + bulleye + "%";
            if( sorttime > 0 )
                msg += " Second Chance!";
            if( times == 1 )
                msg += " Success!";
            AltarBonusStruct.BumpSuccessful = true;
            UpdateBumpSuccessful( altar, bumpBn, ( int ) dr );
        }
        else
        if( ok == false )                                                                                       // Bump failed
        {
            col = Color.red;
            AltarBonusStruct.BumpSuccessful = false;
            if( times > 1 )
                msg += " Bull's Eye " + bulleye + "%";
            msg += " Fail!";
            UpdateBumpFailed( altar, bumpBn, ( int ) dr );
        }
        Message.CreateMessage( ETileType.NONE, msg, tg, col );                                                  // Display message

        float tm = 180 / Util.Percent( altar.Md.PoleBumpWaitTime, altar.Md.PoleRotationSpeed );                 // calculate wait time 180 deegrees

        altar.Control.WaitTimeCounter += tm;

        if( ok == false )
            return;                                                                                             // quit if sort failed

        bumpBn.TimesUsed++;
        bumpBn.Unlocked = true;
        bool sort = true;

        List<Unit> al = GetAltars();
        EAltarBonusType type = bumpBn.AltarBonusType; 
        sort = ApplyBonusEffect( type, ref altar, ( int ) dr, bumpBn );                                         // Apply bonus effect 
        UpdateBonusData( EAltarBonusType.Stacker, altar );                                                      // Bonus Stacker calculation

    }
    public static void UpdateAntenna( Vector2 tg, Unit altar, int id )
    {
        if( altar.Altar.NextAltarBonus.AltarBonusType == EAltarBonusType.NONE ) return;
        AltarBonusStruct bumpBn = altar.Altar.AltarBonusList[ id ];
        Unit tgun = null;
        for( int ds = 1; ds < Sector.TSX; ds++ )                                                                           // Tries to find another butcher pointed by the hero
        {
            Vector2 tgg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * ds;
            if( Map.IsWall( tgg ) ) break;
            if( Sector.IsPtInCube( tgg ) == false ) break;
            Unit un = Map.I.GetUnit( ETileType.ALTAR, tgg );
            if( un && un != altar )           
              {
                  if( un.Altar.NextAltarBonus.AltarBonusType != EAltarBonusType.NONE )                                    // Target is occpied by other bonus
                  {
                      Message.RedMessage( "Target Occupied" );
                      return;
                  }
                tgun = un; 
                break; 
            }                                                                                                             // found!
        }
        if( tgun != null )                                                                                                // Sends the centered bonus to another Butcher
        {
            ArcherArrowAnimation.Create( altar.Pos, tgun.Pos, EBoltType.Altar, altar.Body.Sprite2.spriteId, 30 );         // anim
            tgun.Altar.NextAltarBonus.Copy( altar.Altar.NextAltarBonus );
            altar.Altar.NextAltarBonus.Reset();
            MasterAudio.PlaySound3DAtVector3( "Eletric Shock", G.Hero.Pos );                                              // Sound fx
            Map.I.LineEffect( altar.Pos, tgun.Pos, 2.5f, .5f, Color.blue, Color.blue );                                   // Travel Line FX
            return;
        }

        if( bumpBn.AltarBonusType == EAltarBonusType.NONE )                                                               // Sends the centered bonus to the periphery
        {
            ArcherArrowAnimation.Create( altar.Pos, altar.Body.BabySprite[ id ].transform.position,                       // anim
            EBoltType.Altar, altar.Body.Sprite2.spriteId, 30 );
            bumpBn.Copy( altar.Altar.NextAltarBonus );
            altar.Altar.NextAltarBonus.Reset();
            MasterAudio.PlaySound3DAtVector3( "Eletric Shock", G.Hero.Pos );                                              // Sound fx
            Map.I.LineEffect( altar.Pos, altar.Body.BabySprite[ id ].
            transform.position, 2.5f, .5f, Color.blue, Color.blue );                                                      // Travel Line FX
            return;
        }
    }
    private static bool CheckAltarPurchase( Unit altar, AltarBonusStruct bump )
    {
        if( altar.Altar.RandomAltar )
        if( bump.AltarBonusType != EAltarBonusType.Prize )                                                        // Random altar: passive bonus is free of cost
            return true;

        List<float> icl = new List<float>();
        List<ItemType> il = new List<ItemType>();
        List<AltarBonusStruct> all = GetBabySlotList( EAltarBonusType.Bump_Cost, altar );                         // get all bump cost bonuses

        float custom = -1;
        if( bump.AltarBonusType != EAltarBonusType.Prize )
        if( bump.AltarBonusType != EAltarBonusType.Bump_Cost )
        if( bump.AltarBonusItem != ItemType.NONE )
        {
            all = new List<AltarBonusStruct>();
            all.Add( bump );                                                                                        // custom bonuses
            custom = 1;
        }

        for( int i = 0; i < all.Count; i++ )
        if( all[ i ].Activated )
        {
            il.Add( all[ i ].AltarBonusItem );
            if( custom != -1 )
                icl.Add( custom );
            else
                icl.Add( all[ i ].GetFact() );
        }
        if( il.Count == 0 )                                                                                         // set coin as default cost
        {
            il.Add( ItemType.Res_Butcher_Coin );
            icl.Add( 1 );
        }
        il = Util.CondenseItems( il, icl, ref icl );
        bool purchase = true;
        for( int i = 0; i < il.Count; i++ )
        {
            float amt = Item.GetNum( il[ i ] );
            if( amt < icl[ i ] )                                                                                     // out of items
            {
                purchase = false;
                Message.CreateMessage( ETileType.NONE, il[ i ], ( icl[ i ] - amt ).ToString( "+0;-#" ) +
                " Needed!", altar.Pos, Color.red, Util.Chc(), Util.Chc(), 4, i / 3, 0, 65 );
            }
        }
        if( purchase == false ) return false;                                                                        // not enough, quit
        for( int i = 0; i < il.Count; i++ )
        {
            Item.ForceMessage = true;
            Item.AddItem( il[ i ], -icl[ i ] );                                                                      // Charge items     
        }
        return true;
    }
    private static void UpdateBumpSuccessful( Unit au , AltarBonusStruct bn, int ID )
    {
        MasterAudio.PlaySound3DAtVector3( "Cashier", G.Hero.Pos );                                                    // FX
        bool def = true;
        List<AltarBonusStruct> all = GetBabySlotList( EAltarBonusType.Hang_Bonus );                                   // Hang Successful prizes - Yellow
        if( all.Count > 0 )
        {
            float yellow = GetAltarBonusSum( EAltarBonusType.Hang_Bonus, au, ID );
            for( int i = 0; i < all.Count; i++ )
            {
                if( ( all[ i ].Altar != au.Altar && all[ i ].Scope == EAltarBonusScope.Cube ) ||
                    ( all[ i ].Altar == au.Altar && yellow > 0 ) )
                {
                    float amt = all[ i ].GetFact();
                    if( ( au.Altar.RandomAltar == false || all[ i ].ID == ID ) && all[ i ].Activated )                // Random altar only hangs directly bumped item
                        HangItem( all[ i ], ( int ) amt );                                                            // Hang items                                             
                    def = false;
                }
            }
        }

        int axe = ( int ) GetAltarBonusSum( EAltarBonusType.Axe, au, ID );                                            // Axe hang
        if( axe > 0 )
        {
            HangItem( Bnl[ 0 ], 1 );
            au.Body.PoleSprite.Build();
        }

        all = GetBabySlotList( EAltarBonusType.Prize );                                                               // Gives Successful prizes - Green
        if( all.Count > 0 )
        {
            float green = GetAltarBonusSum( EAltarBonusType.Prize, au, ID );
            for( int i = 0; i < all.Count; i++ )
            {
                if( ( all[ i ].Altar != au.Altar && all[ i ].Scope == EAltarBonusScope.Cube ) ||
                    ( all[ i ].Altar == au.Altar && green > 0 ) ) 
                {
                    float amt = all[ i ].GetFact();
                    if( ( au.Altar.RandomAltar == false || all[ i ].ID == ID ) && all[ i ].Activated )               // Random altar only gives directly bumped item
                        Item.AddItem( all[ i ].AltarBonusItem, amt );                                                // add items
                    def = false;
                }
            }
        }
        if( def )                                                                                  
        {
            float amt = bn.GetFact( false, EAltarBonusType.NONE, 1 );
            Item.AddItem( ItemType.Res_Butcher_Coin, amt );                                                          // set coin as default cost
        }
        
        au.Altar.StackCount++;                                                                                       // Stack bump success increment

        GlobalAltar.I.UpdateBumpSuccessful( au, bn );                                                                // Random Butcher update
    }
    public static void UpdateBonusData( EAltarBonusType type, Unit au )
    {
        RESET( type );
        for( int a = 0; a < Al.Count; a++ )
        for( int i = 0; i < 8; i++ )
        if( Al[ a ].Altar.AltarBonusList[ i ].AltarBonusType == type )
        if( Al[ a ].Altar.AltarBonusList[ i ].Activated )
        if( Al[ a ].Altar.AltarBonusList[ i ].Scope == EAltarBonusScope.Cube )
        {
            UPD( Al[ a ].Altar.AltarBonusList[ i ], type, EAltarBonusScope.Cube );
        }          

        for( int a = 0; a < Al.Count; a++ )
        for( int i = 0; i < 8; i++ )
        if( Al[ a ].Altar.AltarBonusList[ i ].AltarBonusType == type )
        if( Al[ a ].Altar.AltarBonusList[ i ].Activated )
        {
            AltarBonusStruct bn = Al[ a ].Altar.AltarBonusList[ i ];
            bn.Altar = Al[ a ].Altar;

            if( bn.Scope == EAltarBonusScope.Butcher )
            {
                UPD( bn, type, bn.Scope, bn.Altar );
            }
            if( bn.Scope == EAltarBonusScope.Neighbors )
            {
                int id1 = Util.ReturnNeighborID( i, +1 );
                int id2 = Util.ReturnNeighborID( i, -1 );
                List<int> idl = new List<int>() { id1, id2 };
                UPD( bn, type, bn.Scope, bn.Altar, idl );
            }
            if( bn.Scope == EAltarBonusScope.Invert )
            {
                int id1 = Util.ReturnNeighborID( i, +4 );
                List<int> idl = new List<int>() { id1 };
                UPD( bn, type, bn.Scope, bn.Altar, idl );
            }
            if( bn.Scope == EAltarBonusScope.T )
            {
                int id1 = Util.ReturnNeighborID( i, +2 );
                int id2 = Util.ReturnNeighborID( i, -2 );
                List<int> idl = new List<int>() { id1, id2 };
                UPD( bn, type, bn.Scope, bn.Altar, idl );
            }
            if( bn.Scope == EAltarBonusScope.Fork )
            {
                int id1 = Util.ReturnNeighborID( i, +3 );
                int id2 = Util.ReturnNeighborID( i, -3 );
                List<int> idl = new List<int>() { id1, id2 };
                UPD( bn, type, bn.Scope, bn.Altar, idl );
            }               
        }
    }
    private static void UPD( AltarBonusStruct bn, EAltarBonusType type, EAltarBonusScope scope, Altar altar = null, List<int> idl = null )
    {
        if( scope == EAltarBonusScope.Cube )
        {
            for( int a = 0; a < Al.Count; a++ )
            for( int i = 0; i < 8; i++ )
                 SET( bn, type, Al[ a ].Altar, i );
        }
        else
        {
            for( int i = 0; i < 8; i++ )
            if( scope == EAltarBonusScope.Butcher || idl.Contains( i ) )
                SET( bn, type, altar, i );
        }
    }
    private static void SET( AltarBonusStruct bn, EAltarBonusType type, Altar altar, int i )
    {
        bool lk = false;
        if( type == EAltarBonusType.Lock || bn.LockCount == 0 ) lk = true;
        bool same = false;
        if( bn == altar.AltarBonusList[ i ] ) 
            same = true;

        float val = bn.AltarBonusFactor;
        if( type != EAltarBonusType.Stacker )
        if( altar.AltarBonusList[ i ].StackerCount > 0 )
            val += altar.StackCount * bn.StackerMultiplier;
        if( type == EAltarBonusType.Bonus_Adder )
            altar.AltarBonusList[ i ].AdderCount += val;
        if( type == EAltarBonusType.Bonus_Multiplier )
            altar.AltarBonusList[ i ].MultiplierCount += val;
        if( type == EAltarBonusType.Lock )
            altar.AltarBonusList[ i ].LockCount += val;
        if( type == EAltarBonusType.Stacker )
            altar.AltarBonusList[ i ].StackerCount += val;
        if( type == EAltarBonusType.Bank )
            altar.AltarBonusList[ i ].SafeCount += val;
        if( type == EAltarBonusType.Dice )
        if( altar.AltarBonusList[ i ].AltarBonusType != EAltarBonusType.NONE )
        if( altar.AltarBonusList[ i ].AltarBonusType != EAltarBonusType.Dice )
            Bnl.Add( altar.AltarBonusList[ i ] );
        if( type == EAltarBonusType.Rotation_Speed )
        {
            if( lk ) altar.AltarBonusList[ i ].RotationModCount += val;
            if( same ) altar.AltarBonusList[ i ].RotationModCountDisp += val;
        }
        if( type == EAltarBonusType.Angle_Modifier )
        {
            if( lk ) altar.AltarBonusList[ i ].AngleModCount += val;
            if( same ) altar.AltarBonusList[ i ].AngleModCountDisp += val;
        }
    }
    private static void RESET( EAltarBonusType type )
    {
        for( int a = 0; a < Al.Count; a++ )
        for( int i = 0; i < 8; i++ )
        {
            if( type == EAltarBonusType.Lock )
                Al[ a ].Altar.AltarBonusList[ i ].LockCount = 0;
            if( type == EAltarBonusType.Bonus_Adder )
                Al[ a ].Altar.AltarBonusList[ i ].AdderCount = 0;
            if( type == EAltarBonusType.Bonus_Multiplier )
                Al[ a ].Altar.AltarBonusList[ i ].MultiplierCount = 0;
            if( type == EAltarBonusType.Stacker )
                Al[ a ].Altar.AltarBonusList[ i ].StackerCount = 0;
            if( type == EAltarBonusType.Bank )
                Al[ a ].Altar.AltarBonusList[ i ].SafeCount = 0;
            if( type == EAltarBonusType.Rotation_Speed )
            {
                Al[ a ].Altar.AltarBonusList[ i ].RotationModCount = 0;
                Al[ a ].Altar.AltarBonusList[ i ].RotationModCountDisp = 0;
            }
            if( type == EAltarBonusType.Angle_Modifier )
            {
                Al[ a ].Altar.AltarBonusList[ i ].AngleModCount = 0;
                Al[ a ].Altar.AltarBonusList[ i ].AngleModCountDisp = 0;
            }
        }
    }
    private static void UpdateBumpFailed( Unit au, AltarBonusStruct bn, int ID )
    {
        au.Altar.StackCount = 0;
        MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );                                                      // FX

        int blade = ( int ) GetAltarBonusSum( EAltarBonusType.Blade, au, ID );                                          // Blade hang

        if( blade > 0 )
        {
            HangItem( Bnl[ 0 ], blade );
            au.Body.PoleSprite.Build(); 
        }
        GlobalAltar.I.UpdateBumpFailed( au, bn, ID );                                                                   // Updates Random altar bump failed
    }
    public static bool ApplyBonusEffect( EAltarBonusType type, ref Unit altar, int dr, AltarBonusStruct bumpBn )
    {
        if( bumpBn.Activated == false )                                                                                 // inactive bonus 
        {
            Message.GreenMessage( "Bonus Activated!" );
            bumpBn.Activated = true;
            return false;                                                                                               // activates inactive bonus and quit
        }

        switch( type )
        {
            case EAltarBonusType.Lock:
            Message.GreenMessage( "Lock Destroyed!" );
            bumpBn.Reset();
            break;

            case EAltarBonusType.Dice:

            Al = GetAltars();
            Bnl = new List<AltarBonusStruct>();
            UpdateBonusData( EAltarBonusType.Dice, altar );                                                 // Bonus Angle modifier calculation

            List<AltarBonusStruct> bl = new List<AltarBonusStruct>();
            for( int i = 0; i < Bnl.Count; i++ )
            {
                AltarBonusStruct tm = new AltarBonusStruct();
                tm.Copy( Bnl[ i ] );
                bl.Add( tm );
                AltarBonusStruct resetItem = Bnl[ i ];
                resetItem.Reset();
                Bnl[ i ] = resetItem;
            }
            bl.Shuffle();
            for( int i = 0; i < bl.Count; i++ )
            {
                AltarBonusStruct tm = Bnl[ i ];
                tm.Copy( bl[ i ] );
                Bnl[ i ] = tm;
            }
            break;

            case EAltarBonusType.Power_Booster:
            ScopeAffectedList( EAltarBonusType.ANY, altar, ( int ) dr, bumpBn, false );
            Bnl.Remove( bumpBn );
            for( int i = 0; i < Bnl.Count; i++ )
            {
                AltarBonusStruct bnn = Bnl[ i ];
                if( bnn.AltarBonusType != EAltarBonusType.Dice )
                if( bnn.AltarBonusType != EAltarBonusType.Antenna )
                if( bnn.AltarBonusType != EAltarBonusType.Invert_Bonus )
                if( bnn.AltarBonusType != EAltarBonusType.Rotate_CCW )
                if( bnn.AltarBonusType != EAltarBonusType.Rotate_CW )
                    bnn.AltarBonusFactor += Util.Percent( bumpBn.AltarBonusFactor, bnn.AltarBonusFactor );
            }
            break;

            case EAltarBonusType.Add_Slot:
            List<AltarBonusStruct> all = GetBabySlotList( EAltarBonusType.NONE );
            break;

            case EAltarBonusType.Bank:
            ScopeAffectedList( EAltarBonusType.ANY, altar, ( int ) dr, bumpBn, true );
            Util.RemoveDuplicates( ItBnl );
            for( int i = 0; i < ItBnl.Count; i++ )
            {
                float amt = Item.GetNum( ItBnl[ i ] );
                float rate = bumpBn.GetFact();
                float add = Util.Percent( rate, amt );
                if( add != 0 )
                {
                    Item.PostMessage = " Bank Interest: " + rate + "%";
                    Item.AddItem( ItBnl[ i ], add );
                }
            }
            break;

            case EAltarBonusType.Antenna:
            AltarBonusStruct bn = altar.Altar.AltarBonusList[ ( int ) G.Hero.Dir ];
            if( bn.AltarBonusType != EAltarBonusType.NONE )
            {
                Vector3 tgg = altar.Pos + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ];
                ArcherArrowAnimation.Create( tgg, altar.Pos, EBoltType.Altar,
                altar.Body.BabySprite[ ( int ) G.Hero.Dir ].spriteId, 6 );
                altar.Altar.NextAltarBonus.Copy( bn );                                                                    // sends hero indicated bonus to the center
                bn.Reset();
                bumpBn.Reset();
                MasterAudio.PlaySound3DAtVector3( "Eletric Shock", G.Hero.Pos );                                          // Sound fx
                Map.I.LineEffect( tgg, altar.Pos, 2.5f, .5f, Color.blue, Color.blue );                                    // line effect
            }
            break;
            case EAltarBonusType.Invert_Bonus:
            AltarBonusStruct temp = new AltarBonusStruct();
            dr = ( int ) Util.GetTargetUnitDir( G.Hero.Pos, altar.Pos );
            if( altar.Altar.AltarBonusList[ dr ].AltarBonusType != EAltarBonusType.NONE )
            {
                temp.Copy( bumpBn );
                bumpBn.Copy( altar.Altar.AltarBonusList[ dr ] );
                altar.Altar.AltarBonusList[ dr ].Copy( temp );
            }
            else
                Message.RedMessage( "No Bonus on the other Side!" );
            break;

            case EAltarBonusType.Rotate_CW:
            Unit ta = altar;
            bool hasEmpty = false;
            for( int i = 0; i < 8; i++ )
            if( ta.Altar.AltarBonusList[ i ].AltarBonusType == EAltarBonusType.NONE )
            {
                hasEmpty = true;                                                                       // Found an empty slot
                break;
            }
            if( hasEmpty )
            {
                int start = ( dr + 1 ) % 8;                                                             // Start checking from the next slot after hit
                int end = start;

                while( ta.Altar.AltarBonusList[ end ].AltarBonusType != EAltarBonusType.NONE )
                {
                    end = ( end + 1 ) % 8;                                                             // Move to the next index
                    if( end == dr ) break;                                                             // Prevent infinite loop
                }

                int current = ( end - 1 + 8 ) % 8;                                                     // Start copying from the last valid item
                while( current != ( dr - 1 + 8 ) % 8 )
                {
                    int next = ( current + 1 ) % 8;                                                    // Compute the next index
                    ta.Altar.AltarBonusList[ next ].Copy( ta.Altar.AltarBonusList[ current ] );        // Copy current to next
                    current = ( current - 1 + 8 ) % 8;                                                 // Move backward
                }

                ta.Altar.AltarBonusList[ dr ].Reset();                                                 // Clear the original hit slot
            }
            else
            {
                temp = new AltarBonusStruct();                                                         // Create a temp to hold last item
                temp.Copy( ta.Altar.AltarBonusList[ ( dr + 7 ) % 8 ] );                                // Copy last item into temp

                for( int i = 7; i > 0; i-- )
                {
                    int from = ( dr + i - 1 ) % 8;                                                     // Source index
                    int to = ( dr + i ) % 8;                                                           // Destination index
                    ta.Altar.AltarBonusList[ to ].Copy( ta.Altar.AltarBonusList[ from ] );             // Shift forward
                }
                ta.Altar.AltarBonusList[ dr ].Copy( temp );                                            // Place the saved item in the hit slot
            }
            return false;
            break;
            case EAltarBonusType.Rotate_CCW:
                ta = altar;
                hasEmpty = false;
                for( int i = 0; i < 8; i++ )
                if( ta.Altar.AltarBonusList[ i ].AltarBonusType == EAltarBonusType.NONE )
                {
                    hasEmpty = true;                                                                   // Found an empty slot
                    break;
                }
                if( hasEmpty )
                {
                    int start = ( dr - 1 + 8 ) % 8;                                                        // Start checking from the previous slot before hit
                    int end = start;
                    while( ta.Altar.AltarBonusList[ end ].AltarBonusType != EAltarBonusType.NONE )
                    {
                        end = ( end - 1 + 8 ) % 8;                                                         // Move to the previous index
                        if( end == dr ) break;                                                             // Prevent infinite loop
                    }

                    int current = ( end + 1 ) % 8;                                                         // Start copying from the first valid item before empty
                    while( current != ( dr + 1 ) % 8 )
                    {
                        int prev = ( current - 1 + 8 ) % 8;                                                // Compute the previous index
                        ta.Altar.AltarBonusList[ prev ].Copy( ta.Altar.AltarBonusList[ current ] );        // Copy current to previous
                        current = ( current + 1 ) % 8;                                                     // Move forward
                    }

                    ta.Altar.AltarBonusList[ dr ].Reset();                                                 // Clear the original hit slot
                }
                else
                {
                    temp = new AltarBonusStruct();                                                         // Create a temp to hold first item
                    temp.Copy( ta.Altar.AltarBonusList[ ( dr + 1 ) % 8 ] );                                // Copy first item into temp
                    for( int i = 1; i < 8; i++ )
                    {
                        int from = ( dr + i ) % 8;                                                         // Source index
                        int to = ( dr + i - 1 ) % 8;                                                       // Destination index
                        ta.Altar.AltarBonusList[ to ].Copy( ta.Altar.AltarBonusList[ from ] );             // Shift backward
                    }
                    ta.Altar.AltarBonusList[ ( dr + 7 ) % 8 ].Copy( temp );                                // Place saved item in the last slot
                }
                return false;
                break;
            case EAltarBonusType.Blade:  
            all = GetPoleObjList( EAltarBonusType.Blade, null );
            int bnid = Random.Range( 0, all.Count );
            if( all.Count > 0 )
            {
                all[ bnid ].Reset();
            }
            break;
            case EAltarBonusType.Give_Bonus:
            Item.AddItem( bumpBn.AltarBonusItem, bumpBn.GetFact() );
            break;
        }

        List<Unit> al = GetAltars();
        for( int i = 0; i < al.Count; i++ )
            al[ i ].Body.PoleSprite.Build();                                                                            // update build pole sprite, can be optimized 
              
            return true;
    } 
    public static float GetAltarBonusSum( EAltarBonusType type, Unit altar, int ID, bool inactive = false )
    {
        Bnl = new List<AltarBonusStruct>();
        float sum = 0;
        List<Unit> al = GetAltars();
        for( int a = 0; a < al.Count; a++ )
        for( int i = 0; i < 8; i++ )
        if( al[ a ].Altar.AltarBonusList[ i ].AltarBonusType == type )
        if( al[ a ].Altar.AltarBonusList[ i ].Activated || inactive == true )
        {
            AltarBonusStruct bn = al[ a ].Altar.AltarBonusList[ i ];
            bn.Altar = al[ a ].Altar;
            if( altar == al[ a ] )                                            // same altar bonuses
            if( bn.LockCount == 0 )
            {
                if( bn.Scope == EAltarBonusScope.Butcher )
                if( CheckBonus( bn, ID, i ) )
                {
                    sum += bn.GetFact();
                    Bnl.Add( bn );
                }
                int sig1 = 0, sig2 = 0;
                if( bn.Scope == EAltarBonusScope.Neighbors )
                {
                    sig1 = +1; sig2 = -1;
                }
                if( bn.Scope == EAltarBonusScope.Invert )
                {
                    sig1 = +4;
                }
                if( bn.Scope == EAltarBonusScope.T )
                {
                    sig1 = +2; sig2 = -2;
                }
                if( bn.Scope == EAltarBonusScope.Fork )
                {
                    sig1 = +3; sig2 = -3;
                }
                int id1 = Util.ReturnNeighborID( i, sig1 );
                if( sig1 != 0 && id1 == ID )
                { 
                    sum += bn.GetFact();
                    Bnl.Add( bn );
                }
                int id2 = Util.ReturnNeighborID( i, sig2 );
                if( sig2 != 0 && id2 == ID )
                {
                    sum += bn.GetFact();
                    Bnl.Add( bn );
                }
            }

            if( bn.Scope == EAltarBonusScope.Cube )
            if( bn.LockCount == 0 )
            if( CheckBonus( bn, ID, i, altar ) )
            {
                sum += bn.GetFact();
                Bnl.Add( bn );
            }  
        }
        return sum;
    }

    public static bool IsPassiveBonus( EAltarBonusType type )
    {
        if( type == EAltarBonusType.Horse_Shoe ||
            type == EAltarBonusType.Angle_Modifier ||
            type == EAltarBonusType.Bonus_Adder ||
            type == EAltarBonusType.Bonus_Multiplier ||
            type == EAltarBonusType.Bulls_Eye ||
            type == EAltarBonusType.Rotation_Speed ||
            type == EAltarBonusType.Bank )
            return true;
        return false;

    }
    private static bool CheckBonus( AltarBonusStruct bn, int ID, int i, Unit al = null )
    {
        if( IsPassiveBonus( bn.AltarBonusType ) )                                         // these ones do not have bonus value applied to the bonus source itself (only passive bonuses here)
        {
            if( al )                                                                      // cube scope
            {
                if( al != bn.Altar.Unit || i != ID ) 
                    return true;
            }
            else
            if( i != ID )                                                                 // butcher scope
                return true;
            return false;
        }
        return true;
    } 
    public static void ScopeAffectedList( EAltarBonusType type, Unit altar, int ID, AltarBonusStruct bump, bool res )
    {
        Bnl = new List<AltarBonusStruct>();
        ItBnl = new List<ItemType>();
        List<Unit> al = GetAltars();
        if( bump.Scope ==  EAltarBonusScope.Cube )
        for( int a = 0; a < al.Count; a++ )
        for( int i = 0; i < 8; i++ )
        if( al[ a ].Altar.AltarBonusList[ i ].AltarBonusType != EAltarBonusType.NONE )
        if( al[ a ].Altar.AltarBonusList[ i ].AltarBonusType == type || type == EAltarBonusType.ANY )
        if( al[ a ].Altar.AltarBonusList[ i ].Activated )
        {
            if( al[ a ].Altar.AltarBonusList[ i ].LockCount == 0 )
                ADDBN( al[ a ].Altar.AltarBonusList[ i ], i, res );
        }
        if( bump.Scope == EAltarBonusScope.Butcher )
        {
            for( int j = 0; j < 8; j++ )
                ADDBN( bump, j, res );
        }
        if( bump.Scope == EAltarBonusScope.Neighbors )
        {
            int id1 = Util.ReturnNeighborID( ID, +1 );
            int id2 = Util.ReturnNeighborID( ID, -1 );
            ADDBN( bump, id1, res );
            ADDBN( bump, id2, res );
        }
        if( bump.Scope == EAltarBonusScope.Invert )
        {
            int id1 = Util.ReturnNeighborID( ID, +4 );
            ADDBN( bump, id1, res );
        }
        if( bump.Scope == EAltarBonusScope.T )
        {
            int id1 = Util.ReturnNeighborID( ID, +2 );
            int id2 = Util.ReturnNeighborID( ID, -2 );
            ADDBN( bump, id1, res );
            ADDBN( bump, id2, res );
        }
        if( bump.Scope == EAltarBonusScope.Fork )
        {
            int id1 = Util.ReturnNeighborID( ID, +3 );
            int id2 = Util.ReturnNeighborID( ID, -3 );
            ADDBN( bump, id1, res );
            ADDBN( bump, id2, res );
        }
    }
    private static void ADDBN( AltarBonusStruct bump, int id, bool res )
    {
        AltarBonusStruct bn = bump.Altar.AltarBonusList[ id ];
        if( bn.AltarBonusType != EAltarBonusType.NONE )
        if( bn.AltarBonusItem != ItemType.NONE || res == false )
        {
            Bnl.Add( bn );
            if( bn.AltarBonusItem != ItemType.NONE )
                ItBnl.Add( bn.AltarBonusItem );
        }
    }
    public static List<AltarBonusStruct> GetBabySlotList( EAltarBonusType type, Unit altar = null )
    {
        List<AltarBonusStruct> res = new List<AltarBonusStruct>();
        List<Unit> al = GetAltars();
        if( altar )
        {
            al = new List<Unit>();
            al.Add( altar );
        }

        for( int a = 0; a < al.Count; a++ )
        for( int i = 0; i < 8; i++ )
        if( al[ a ].Altar.AltarBonusList[ i ].AltarBonusType == type ||
          ( type == EAltarBonusType.ANY && al[ a ].Altar.AltarBonusList[ i ].AltarBonusType != EAltarBonusType.NONE ) )
            res.Add( al[ a ].Altar.AltarBonusList[ i ] );
        return res;
    }
    public static void HangItem( AltarBonusStruct bn, int amt = 1, Unit altar = null )
    {
        if( amt < 1 ) return;
        List<Unit> al = GetAltars();
        if( altar )
        {
            al = new List<Unit>();
            al.Add( altar );
        }

        for( int i = al.Count - 1; i >= 0; i-- )                                                  // LOS needed for target altar
        if( al[ i ] != bn.Altar.Unit )
           {
               if( Map.I.HasLOS( al[ i ].Pos, bn.Altar.Unit.Pos ) == false )
                   al.RemoveAt( i );
           }  

        List<AltarBonusStruct> ls = new List<AltarBonusStruct>();
        List<int> aid = new List<int>();
        for( int pass = 1; pass >= 0; pass-- )                                                    // no spikes close to the altars first
        {
            for( int a = 0; a < al.Count; a++ )
            {
                Unit ta = al[ a ];
                for( int i = pass; i < ta.Altar.PoleObjList.Count; i++ )
                if( ta.Altar.PoleObjList[ i ].AltarBonusType == EAltarBonusType.NONE )
                {
                    ls.Add( ta.Altar.PoleObjList[ i ] );
                    aid.Add( i );
                }
            }
            if( ls.Count > 0 )
            {
                int id = Random.Range( 0, ls.Count );
                ls[ id ].Copy( bn );
                ls[ id ].AltarBonusFactor = 1;                                                    // if more than one is to be hanged, hangs just one object x multiple times
                if( ls[ id ].AltarBonusType == EAltarBonusType.Blade )
                    ls[ id ].ShieldTimer = 10;
                ls[ id ].HangTimer = ls[ id ].TotalHangTimer;
                break;
            }
        }
        HangItem( bn, --amt, altar );                                                             // recursive                             
    }
    public static List<AltarBonusStruct> GetPoleObjList( EAltarBonusType type, Unit altar = null )
    {
        List<Unit> al = GetAltars();
        if( altar )
        {
            al = new List<Unit>();
            al.Add( altar );
        }
        List<AltarBonusStruct> ls = new List<AltarBonusStruct>();
        for( int a = 0; a < al.Count; a++ )
        {
            Unit ta = al[ a ];
            for( int i = 0; i < ta.Altar.PoleObjList.Count; i++ )
            if( ta.Altar.PoleObjList[ i ].AltarBonusType == type )
                ls.Add( ta.Altar.PoleObjList[ i ] );
        }
        return ls;
    }
    public void InitAltarBonuses()
    {
        Body.Level = 1;
        AltarBonusList = new List<AltarBonusStruct>();
        NextAltarBonus = new AltarBonusStruct();
        NextAltarBonus.Reset();
        Unit.RightText.gameObject.SetActive( false );

        for( int i = 0; i < 8; i++ )
        {
            AltarBonusStruct al = new AltarBonusStruct();
            al.ID = i;
            AltarBonusList.Add( al );
        }

        PoleObjList = new List<AltarBonusStruct>();
        for( int i = 0; i < 8; i++ )
        {
            AltarBonusStruct bn = new AltarBonusStruct();
            if( Unit.Md )
            if( Unit.Md.PoleObjList != null && i < Unit.Md.PoleObjList.Count )
                bn.Copy( Unit.Md.PoleObjList[ i ] );
            PoleObjList.Add( bn );
        }   
    }
    public void SetPoleDir( Vector3 dir )
    {
        Unit.Spr.transform.eulerAngles = dir;
        Body.Sprite5.transform.rotation = Quaternion.Euler( -dir );
    }
    public static List<Unit> GetAltars()
    {
        List<Unit> al = new List<Unit>();
        for( int y = ( int ) G.HS.Area.yMin - 1; y < G.HS.Area.yMax + 1; y++ )
        for( int x = ( int ) G.HS.Area.xMin - 1; x < G.HS.Area.xMax + 1; x++ )
            {
                Unit altar = Map.I.GetUnit( ETileType.ALTAR, new Vector2( x, y ) );
                if( altar && altar.Control.Resting == false ) al.Add( altar );
            }
        return al;
    }   
}