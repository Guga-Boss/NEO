using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using PathologicalGames;

public partial class Unit : MonoBehaviour
{
    public void Init( EV vartype, bool val )
    {
        switch( vartype )
        {
            case EV.Resting:
            if( Control )
            {
                ToggleBool( ref val, "Resting" );
                Control.Resting = val;
                Control.OriginalResting = val;
            }
            break;
            case EV.FrontalRebound:
            if( Control )
                Control.FrontalRebound = val;
            break;
            case EV.FireON:
            Map.I.LightFire( ( int ) IniPos.x, ( int ) IniPos.y, val, true );
            break;
            case EV.IsTiny:
            Body.IsTiny = val;
            break;
            case EV.IsWorking:
            if( Body )
                Body.SetWorking( val );
            break;
            case EV.RandomizePositionOnRespawn:
            if( Control )
                Control.RandomizePositionOnRespawn = val;
            break;
            case EV.RandomizePositionOnPoleStep:
            if( Control )
            {
                Control.RandomizePositionOnPoleStep = val;
                if( TileID == ETileType.FISHING_POLE )
                    Body.Sprite2.gameObject.SetActive( val );
            }
            break;
            case EV.RandomizableHerb:
            if( Body )
                Body.RandomizableHerb = val;
            break;
            case EV.OptionalMonster:
            if( Body )
                Body.OptionalMonster = val;
            break;
            case EV.RestingDirectionIsSameAsHero:
            if( Control )
                Control.RestingDirectionIsSameAsHero = val;
            break;
            case EV.WaitAfterObstacleCollision:
            if( Control )
                Control.WaitAfterObstacleCollision = val;
            break;
        }
    }

    public void Init( EV vartype, EResourceOperation val )
    {
        switch( vartype )
        {
            case EV.ResourceOperation:
            Body.ResourceOperation = val;
            break;
        }
    }

    public void Init( EV vartype, EDynamicObjectSortingType val )
    {
        switch( vartype )
        {
            case EV.MovementSorting:
            if( Control )
                Control.MovementSorting = val;
            break;
            case EV.OrientationSorting:
            if( Control )
                Control.OrientationSorting = val;
            break;
        }
    }
    public void Init( EV vartype, string val )
    {
        switch( vartype )
        {
            case EV.PlayAnimation:
            if( Body )
            {
                Body.Animator = Spr.GetComponent<tk2dSpriteAnimator>();
                Body.Animator.Play( val );
                Spr.transform.Rotate( 0.0f, 0.0f, Random.Range( 0.0f, 360.0f ) );
            }
            break;
        }
    }
    public void Init( EV vartype, List<ItemType> val, Mod md = null )
    {
        switch( vartype )
        {
            case EV.BonusItemList:
            if( Body )
            {
                Body.BonusItemList = new List<ItemType>();
                Body.BonusItemList.AddRange( val );   
                Spr.spriteId = 89;                                   // Chooses chest as the sprite since there´s a list o bonuses 
            }
            break;
            case EV.MiningPrize:
            if( Body )
            {
                Body.MiningPrize = Md.DefaultMiningPrize;
                Body.MiningBonusAmount = 1;
                if( Util.Chance( Mod.MD.MiningBonusChance ) )
                {
                    int id = -1;
                    if( Mod.MD.MiningBonusFactor != null && Mod.MD.MiningBonusFactor.Count == Mod.MD.MiningPrizeList.Count )
                        id = Util.Sort( Mod.MD.MiningBonusFactor );    
                    else
                    if( Mod.MD.OrientatorEffects.Contains( EOrientatorEffect.Mine_Item_Type ) == false )
                        id = Random.Range( 0, Mod.MD.MiningPrizeList.Count );
                    if( id != -1 )
                    {
                        Body.MiningPrize = Mod.MD.MiningPrizeList[ id ];
                        if( Mod.MD.MiningBonusAmountList != null && Mod.MD.MiningBonusAmountList.Count == Mod.MD.MiningPrizeList.Count )
                            Body.MiningBonusAmount = Mod.MD.MiningBonusAmountList[ id ];
                    }
                }
                int mtt = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Mine_Item_Type );                    // Ori: Mine bonus item Type
                if( mtt >= 0 && mtt < Mod.MD.OrientatorItemTable1.Count ) 
                    Body.MiningPrize = Mod.MD.OrientatorItemTable1[ mtt ];
                if( Body.MiningPrize == ItemType.NONE ) Body.MiningBonusAmount = 0;
                float miam = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Mine_Item_Amount );               // Ori: Mine bonus item amount
                if( miam != -1 ) Body.MiningBonusAmount = miam;
            }
            break;
        }
    }

    public void Init( EV vartype, EMineType val )
    {
        switch( vartype )
        {
            case EV.MineType:
            if( TileID == ETileType.MINE )
            {
                int mt = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Mine_Type );                     // Ori: Mine Type
                if( Mod.MD.MineTypeList != null &&
                    mt >= 0 && mt < Mod.MD.MineTypeList.Count )                                                // Table value for second effect
                    Body.MineType = Mod.MD.MineTypeList[ mt ];                                                 // Ori: Resource Trigger

                Body.UPMineType = Md.UpMineType;
                mt = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.UpMine_Type );                       // Ori: UP Mine Type
                if( Mod.MD.MineTypeList != null &&
                    mt >= 0 && mt < Mod.MD.MineTypeList.Count )                                                // Table value for second effect
                    Body.UPMineType = Mod.MD.MineTypeList[ mt ]; 
            }

            if( Mine )
            {
                Mine.MineBonusEffType = Md.MineBonusEffType;
                Mine.MineBonusCnType = Md.MineBonusCnType;
                Mine.MineBonusVal1 = Md.MineBonusVal1;
                Mine.MineBonusVal2 = Md.MineBonusVal2;
                Mine.MineBonusVal3 = Md.MineBonusVal3;
                Mine.MineBonusVal4 = Md.MineBonusVal4;
                int id = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.MineBonusType );                 // Ori: Mine Bonus Eff type
                if( Mod.MD.MineBonusEffList != null &&
                    id >= 0 && id < Mod.MD.MineBonusEffList.Count )
                    Mine.MineBonusEffType = Mod.MD.MineBonusEffList[ id ];
                id = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.MineBonusCondition );                // Ori: Mine Bonus Cn type
                if( Mod.MD.MineBonusCnList != null &&
                    id >= 0 && id < Mod.MD.MineBonusCnList.Count )
                    Mine.MineBonusCnType = Mod.MD.MineBonusCnList[ id ];
                float sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.MineBonusVal1 );                       // Ori: Mine Bonus val 1
                if( sp != -1 ) Mine.MineBonusVal1 = sp;
                sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.MineBonusVal2 );                             // Ori: Mine Bonus val 2
                if( sp != -1 ) Mine.MineBonusVal2 = sp;
                sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.MineBonusVal3 );                             // Ori: Mine Bonus val 3
                if( sp != -1 ) Mine.MineBonusVal3 = sp;
                sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.MineBonusVal4 );                             // Ori: Mine Bonus val 4
                if( sp != -1 ) Mine.MineBonusVal4 = sp;
            }
            break;
        }
    }

    public void Init( EV vartype, ItemType val )
    {
        switch( vartype )
        {
            case EV.ResourceType:
            if( val != ItemType.NONE && ( TileID == ETileType.ITEM || TileID == ETileType.PLAGUE_MONSTER || TileID == ETileType.SAVEGAME || TileID == ETileType.SECRET ) )
            {
                Body.BonusItemList = new List<ItemType>();
                Body.BonusItemChanceList  = new List<float>();
                Body.BonusItemAmountList = new List<float>();

                ItemType tp = ( ItemType ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Item_Type );                        // Ori: Item type by table
                if( Md.OrientatorItemTable1 != null && Md.OrientatorItemTable1.Count > 0 &&
                    ( int ) tp >= 0 &&  ( int ) tp <= Mod.MD.OrientatorItemTable1.Count - 1 )
                {
                    tp = Md.OrientatorItemTable1[ ( int ) tp ];
                }

                if( tp != ItemType.NONE ) val = tp;
                Variation = ( int ) val;
                if( G.GIT( val ).TKSprite == null )
                    G.Error( "Create a TKSprite first. " + val );
                int id = G.GIT( val ).TKSprite.spriteId;
                if( TileID == ETileType.SAVEGAME )
                {
                    Body.Sprite2.gameObject.SetActive( true );
                    RightText.gameObject.SetActive( true );
                    Body.Sprite2.spriteId = id;
                    return;
                }

                if( TileID == ETileType.SECRET )
                {
                    Body.Sprite2.gameObject.SetActive( true );
                    Body.Sprite2.spriteId = id;
                    return;
                }

                Spr.spriteId = id;
                //if( TileID == ETileType.PLAGUE_MONSTER )
                //    InitPlagueMonster();

            }
            break;
            case EV.ResTrigger:
            {
                int id = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Res_Trigger );                   // Ori: Resource Trigger
                if( Mod.MD.ResourceTriggerTypeList != null &&
                    id >= 0 && id <= Mod.MD.ResourceTriggerTypeList.Count - 1 )                                // Table value for second effect
                    Body.ResTriggerType = Mod.MD.ResourceTriggerTypeList[ id ];                                // Ori: Resource Trigger
            }
            break;
        }
    }

    public void InitPlagueMonster()
    {
        Spr.Collection = Map.I.SpriteCollectionList[ ( int ) ESpriteCol.MONSTER_ANIM ];
        Body.Animator = Spr.GetComponent<tk2dSpriteAnimator>();
        var animMap = new Dictionary<int, string>
        {
        {(int)ItemType.Plague_Monster_Kickable, "Plague Monster 2"},
        {(int)ItemType.Plague_Monster_Slayer,   "Plague Monster 4"},
        {(int)ItemType.Plague_Monster_Grabber,  "Plague Monster 3"},
        {(int)ItemType.Plague_Monster_Blocker,  "Plague Monster 6"},
        {(int)ItemType.Plague_Monster_Cloner,   "Plague Monster 7"},
        {(int)ItemType.Plague_Monster_Spawner,  "Plague Monster 5"},
        {(int)ItemType.Plague_Monster_Swap,     "Plague Monster 1"}
       };

        string animName;
        if( animMap.TryGetValue( Variation, out animName ) )
        {
            var clip = Body.Animator.GetClipByName( animName );
            if( clip != null && clip.frames.Length > 0 )
            {
                int frame = Random.Range( 0, clip.frames.Length ); // random frame start
                float startTime = ( float ) frame / clip.fps;
                Body.Animator.Play( clip, startTime, 0f );  
            }
        }
        else Body.Animator.Stop();
        Body.GraphicsInitialized = true;
    }

    public void Init( EV vartype, MyBool val )
    {
        if( val == MyBool.DONT_CHANGE ) return;
        bool value = false;
        if( val == MyBool.TRUE ) value = true;
        switch( vartype )
        {
            case EV.IsBoss:
            Body.IsBoss = value;
            if( Body.IsBoss )                                                                   // Boss big sprite
            {
                Spr.scale = new Vector3( 2f, 2f, 1f );
            }
            break;
            case EV.TickBasedMovement:
            if( Control )
                Control.TickBasedMovement = value;
            break;
        }
    }


    public void Init( EV vartype, List<int> val )
    {
        switch( vartype )
        {
            case EV.TickMoveList:
            if( Control )
            {
                Control.TickMoveList = new List<int>();
                Control.TickMoveList.AddRange( val );                
                if( Mod.MD.Ori( EOrientatorEffect.Toggle_Tick_Movement ) >= 0 )
                for( int i = 0; i < 8; i++ )
                {
                    int tk = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Toggle_Tick_Movement, i );
                    if( tk >= 0 )
                        if( Control.TickMoveList.Contains( tk ) ) 
                            Control.TickMoveList.Remove( tk );
                        else
                        Control.TickMoveList.Add( tk );
                }
            }
            break;

            case EV.RandomMoveTurnList:
            if( Control )
            {
                Control.RandomMoveTurnList = new List<int>();
                Control.RandomMoveTurnList.AddRange( val );
            }
            break;

            case EV.RandomDirTurnList:
            if( Control )
            {
                Control.RandomDirTurnList = new List<int>();
                Control.RandomDirTurnList.AddRange( val );
            }
            break;
            case EV.Vine_Type:
            if( Control )
            {
                int id = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Vine_Type );                   // Ori: Vine Type
                if( id >= 0 )
                {
                    if( Control.VineList.Contains( id ) == false )
                        Control.VineList.Add( id );
                }
                
                int vid = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Vine_ID );                   // Ori: Vine ID - This is used to isolate or force conection of neighbor vines
                if( vid >= 0 )
                {
                    Control.MergingID = vid;
                }

                EDirection dr = ( EDirection ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Vine_Link );    // Ori: Add vine link
                if( dr != EDirection.NONE )
                {
                    Vector2 vnp = Pos + Manager.I.U.DirCord[ ( int ) dr ];
                    Unit vn = Map.GFU( ETileType.VINES, vnp );
                    if( vn )
                    {
                        if( id == -1 ) id = 0;                                                             // force connectino to a neighbor vine
                        if( vn.Control.VineList.Contains( id ) == false )
                            vn.Control.VineList.Add( id );
                        int inv = ( int ) Util.GetInvDir( dr );
                        vn.Control.ForceVineLink[ inv ] = id;
                    }
                    else
                    {
                        if( id == -1 ) id = 0;                                                            // force connection to any object even if its not vine
                        Control.ForceVineLink[ ( int ) dr ] = id;
                    }
                }
            }
            if( Body )
                Body.VineBurnTime = Md.VineBurnTime;
            break;
        }
    }

    public void Init( EV vartype, List<float> val, Mod md = null )
    {
        switch( vartype )
        {
            case EV.BonusItemAmountList:
            if( Body )
            {
                Body.BonusItemAmountList = new List<float>();
                Body.BonusItemAmountList.AddRange( val );
            }
            break;
            case EV.BonusItemChanceList:
            if( Body )
            {
                Body.BonusItemChanceList = new List<float>();
                Body.BonusItemChanceList.AddRange( val );
            }
            break;
        }
    }

    public void Init( EV vartype, List<EActionType> val )
    {
        switch( vartype )
        {
            case EV.DynamicObjectMoveList:
            if( Control )
            {
                Control.DynamicObjectMoveList = new List<EActionType>();
                Control.DynamicObjectMoveList.AddRange( val );
            }
            break;
        }
    }

    public void Init( EV vartype, List<Vector2> val )
    {
        switch( vartype )
        {
            case EV.DynamicObjectJumpList:
            if( Control )
            {
                Control.DynamicObjectJumpList = new List<Vector2>();
                Control.DynamicObjectJumpList.AddRange( val );
            }
            break;
        }
    }
    public void Init( EV vartype, List<EOrientation> val )
    {
        switch( vartype )
        {
            case EV.DynamicObjectOrientationList:
            if( Control )
            {
                Control.DynamicObjectOrientationList = new List<EOrientation>();
                Control.DynamicObjectOrientationList.AddRange( val );
            }
            break;
        }
    }

    public void Init( EV vartype, bool[ , ] val, Mod md )
    {
        switch( vartype )
        {
            case EV.RaftJointList:
            if( Control && TileID == ETileType.RAFT )
            {
                List<EDirection> edl = new List<EDirection>();
                if( val[ 0, 0 ] ) edl.Add( EDirection.NW );
                if( val[ 1, 0 ] ) edl.Add( EDirection.N );
                if( val[ 2, 0 ] ) edl.Add( EDirection.NE );
                if( val[ 0, 1 ] ) edl.Add( EDirection.W );
                if( val[ 2, 1 ] ) edl.Add( EDirection.E );
                if( val[ 0, 2 ] ) edl.Add( EDirection.SW );
                if( val[ 1, 2 ] ) edl.Add( EDirection.S );
                if( val[ 2, 2 ] ) edl.Add( EDirection.SE );

                int rj = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Set_Raft_Joint );       // ori: add a raft joint towards direction pointed. (place ori OVER raft)
                if( rj >= 0 )
                    edl.Add( ( EDirection ) rj );   

                for( int i = 0; i < edl.Count; i++ )
                {
                    Control.RaftJointList[ ( int ) edl[ i ] ].gameObject.SetActive( true );
                }
                Control.BrokenRaft = Md.BrokenRaft;
                ToggleBool( ref Control.BrokenRaft, "Broken Raft" );                                      // toogle bool broken raft
                Spr.spriteId = 74;
                if( Control.BrokenRaft )
                    Spr.spriteId = 79;
            }
            break;

            case EV.BabyList:
            int baby = Mod.MD.NumRandomBabies;
            int num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.AddRandomBabyNum );
            if( num > 0 )
                baby += Random.Range( 0, num );                                                   // orientator adds random num to number of babies

            num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.SetRandomBabyNum );        // orientator set num to number of babies
            if( num >= 0 )
                baby = num;
            Vector2 firstbabyCord = new Vector2( -1, -1 );                

            if( TileID == ETileType.ROACH  ||
                TileID == ETileType.SPIDER ||
                TileID == ETileType.ALTAR  ||
                TileID == ETileType.SCORPION )
            {
                if( TileID == ETileType.ALTAR )                                              // Create Altar bonuses
                    Altar.InitAltarBonuses();

                for( int i = 0; i < Body.Sp.Count; i++ )
                    Body.Sp[ i ].Type = ESpiderBabyType.NONE;

                for( int i = 0; i < Body.BabySprite.Count; i++ )
                if( Body.BabySprite[ i ] )
                {
                    Body.BabySprite[ i ].gameObject.SetActive( false );
                    if( i < 8 )
                        Body.HasBaby[ i ] = false;                    
                }
                if( val[ 1, 0 ] ) Body.HasBaby[ 0 ] = true;
                if( val[ 2, 0 ] ) Body.HasBaby[ 1 ] = true;
                if( val[ 2, 1 ] ) Body.HasBaby[ 2 ] = true;
                if( val[ 2, 2 ] ) Body.HasBaby[ 3 ] = true;
                if( val[ 1, 2 ] ) Body.HasBaby[ 4 ] = true;
                if( val[ 0, 2 ] ) Body.HasBaby[ 5 ] = true;
                if( val[ 0, 1 ] ) Body.HasBaby[ 6 ] = true;
                if( val[ 0, 0 ] ) Body.HasBaby[ 7 ] = true;   

                if( Mod.MD.SpiderBabyTypeList != null && Mod.MD.SpiderBabyTypeList.Count == 8 )
                for( int i = 0; i < 8; i++ )
                {
                    Body.Sp[ i ].Type = Mod.MD.SpiderBabyTypeList[ i ];
                }

                if( TileID == ETileType.SCORPION ||
                    TileID == ETileType.SPIDER )
                    Body.BabyVariation = new List<int>( new int[ ] { -1, -1, -1, -1, -1, -1, -1, -1 } );
            }

            if( Mod.MD.Ori( EOrientatorEffect.AddBabyToDir ) >= 0 )
            for( int i = 0; i < 8; i++ )
                {                    
                    EDirection dr = ( EDirection ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.AddBabyToDir, i );           // mod: add baby to dir

                    if( TileID == ETileType.ALTAR )
                    {
                        AltarBonusStruct bn = Altar.AltarBonusList[ i ];
                        if( Mod.MD.InitialAltarBonusList != null && Mod.MD.InitialAltarBonusList.Count > 0 )
                        {
                            if( ( int ) dr >= 0 && ( int ) dr < Mod.MD.InitialAltarBonusList.Count )
                                bn.Copy( Mod.MD.InitialAltarBonusList[ ( int ) dr ] );
                        }
                        dr = ( EDirection ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Set_Butcher_Baby_Item, i );         // mod: Set Butcher baby item type 
                        if( Md.OrientatorItemTable1 != null && Md.OrientatorItemTable1.Count > 0 &&
                           ( int ) dr >= 0 && ( int ) dr <= Mod.MD.OrientatorItemTable1.Count - 1 )
                        {
                            bn.AltarBonusItem = Md.OrientatorItemTable1[ ( int ) dr ];
                        }
                        dr = ( EDirection ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Set_Butcher_Scope, i );              // mod: Set Butcher Scope
                        if( ( int ) dr >= ( int ) EAltarBonusScope.Neighbors && ( int ) dr <= ( int ) EAltarBonusScope.Cube )
                        {
                            bn.Scope = ( EAltarBonusScope ) dr;
                        }
                        float vl =  Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Set_Butcher_Bonus_Factor, i );              // mod: Set Butcher Bonus Factor
                        if( vl >= 0 )
                        {                            
                            bn.AltarBonusFactor = ( float ) vl;
                        }

                        float f = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Set_Butcher_Bonus_Factor_Multiplier );        // mod: Set Butcher Bonus Factor Multiplier 

                        if( Altar.IsPassiveBonus( bn.AltarBonusType ) )                                                         // only aplies to passive bonuses
                        if( f >= 0 ) bn.AltarBonusFactor *= f;

                        Vector2 tg = Pos + Manager.I.U.DirCord[ i ];
                        ToggleBool( ref bn.Activated, "Butcher Activated", tg );                         // Toggle Bool: Butcher activated 
                    }                
                
                    if( Mod.PointDir == EOriPointDir.AWAY )
                    {
                        Body.HasBaby[ i ] = true;
                        if( TileID == ETileType.SPIDER || TileID == ETileType.SCORPION )
                        if( Mod.MD.PointAwaySpiderType != ESpiderBabyType.NONE )
                        {
                            Body.Sp[ i ].Type = Mod.MD.PointAwaySpiderType;  
                        }
                    }
                     
                    if( Mod.PointDir == EOriPointDir.ME )
                    {
                        Body.HasBaby[ i ] = true;
                        if( TileID == ETileType.SPIDER || TileID == ETileType.SCORPION )
                        {
                            if( Mod.MD.PointToMeSpiderType != ESpiderBabyType.NONE )
                            {
                                Body.Sp[ i ].Type = Mod.MD.PointToMeSpiderType;
                                int lm = Mod.GetLoneMod( IniPos + Manager.I.U.DirCord[ i ] );                  // lone mod around unit to choose item type baby type
                                if( lm != -1 && lm + 1 <= md.NeighborModItemList.Count - 1 )
                                {
                                    Body.BabyVariation[ i ] = ( int ) md.NeighborModItemList[ lm + 1 ];
                                    Body.Sp[ i ].Type = ESpiderBabyType.ITEM;                               // force item since theres a lone mod around
                                }
                            }

                            if( Md.NeighborSpellTypeList != null && md.NeighborSpellTypeList.Count > 0 )
                            {
                                int lo = Mod.GetLoneOri3( IniPos + Manager.I.U.DirCord[ i ] );                                       // lone mod around unit to choose item type baby type
                                if( lo == -1 ) Debug.LogError( "Bad Lone Ori3" );
                                
                                Body.BabyVariation.AddRange( new List<int>( new int[] { -1, -1, -1, -1, -1, -1, -1, -1 } ) );
                                Body.BabyVariation.AddRange( new List<int>( new int[] { -1, -1, -1, -1, -1, -1, -1, -1 } ) );
                                Body.Sp[ i ].Type = Md.NeighborSpellTypeList[ ( lo * 3 ) ];
                                Body.Sp[ i + 8 ].Type = Md.NeighborSpellTypeList[ ( lo * 3 ) + 1 ];
                                Body.Sp[ i + 16 ].Type = Md.NeighborSpellTypeList[ ( lo * 3 ) + 2 ];
                                if( Body.Sp[ i ].Type == ESpiderBabyType.ITEM )
                                    Body.BabyVariation[ i ] = ( int ) Md.NeighborSpellItemList[ ( lo * 3 ) ];
                                if( Body.Sp[ i + 8 ].Type == ESpiderBabyType.ITEM )
                                    Body.BabyVariation[ i + 8 ] = ( int ) Md.NeighborSpellItemList[ ( lo * 3 ) + 1 ];
                                if( Body.Sp[ i + 16 ].Type == ESpiderBabyType.ITEM )
                                    Body.BabyVariation[ i + 16 ] = ( int ) Md.NeighborSpellItemList[ ( lo * 3 ) + 2 ];
                            }
                        }
                    }
                }

            if( TileID == ETileType.ALGAE )                                                            // Algaes have babies in the middle tile, different from roaches and spider
            {
                for( int i = 0; i < Body.BabyDataList.Count; i++ )
                {
                    Body.BabyDataList[ i ].gameObject.SetActive( false );
                    Body.HasBaby[ i ] = false;
                }

                if( val[ 0, 0 ] ) Body.HasBaby[ 0 ] = true;
                if( val[ 1, 0 ] ) Body.HasBaby[ 1 ] = true;
                if( val[ 2, 0 ] ) Body.HasBaby[ 2 ] = true;
                if( val[ 0, 1 ] ) Body.HasBaby[ 3 ] = true;
                if( val[ 1, 1 ] ) Body.HasBaby[ 4 ] = true;
                if( val[ 2, 1 ] ) Body.HasBaby[ 5 ] = true;
                if( val[ 0, 2 ] ) Body.HasBaby[ 6 ] = true;
                if( val[ 1, 2 ] ) Body.HasBaby[ 7 ] = true;
                if( val[ 2, 2 ] ) Body.HasBaby[ 8 ] = true;

                List<int> idl = new List<int>();
                for( int i = 0; i < baby; i++ )
                {
                    for( int j = 0; j < 9999; j++ )
                    {
                        int id = Util.Sort( Mod.MD.RandomBabyFactorList );
                        Vector2 pt = Util.GetDirectionCord( Mod.MD.RandomBabyPosList[ id ] );
                        if( idl.Contains( id ) == false )
                        {
                            int ii = Util.GetCordId( pt );
                            idl.Add( id );
                            Body.HasBaby[ ii ] = true;
                            if( firstbabyCord.x == -1 ) firstbabyCord = pt;
                            break;
                        }
                    }
                }

                for( int i = 0; i < Body.BabySprite.Count; i++ )                   // Algae obj activation
                {
                    BabyData b = Body.BabyDataList[ i ];
                    b.Collider.isTrigger = true;
                    b.BabyTimeCounter = -1;
                    if( Body.HasBaby[ i ] )
                    {
                        b.gameObject.SetActive( true );
                        b.BabyTimeCounter = 0;
                        b.TimesDestroyed = 0;

                        EAlgaeBabyType type = EAlgaeBabyType.NETTLE;
                        if( Mod.MD.BabyTypeList != null && Mod.MD.BabyTypeFactorList != null && 
                            Mod.MD.BabyTypeList.Count == Mod.MD.BabyTypeFactorList.Count )
                            if( Mod.MD.BabyTypeList.Count > 0 )
                        {
                            int id = Util.Sort( Mod.MD.BabyTypeFactorList );
                            type = Mod.MD.BabyTypeList[ id ];

                            if( firstbabyCord == b.Cord )
                            if( md.ForceFirstBabyType != EAlgaeBabyType.NONE )                        // Use this to force one type of baby to alway be present, like rocks
                                type = md.ForceFirstBabyType;
                        }
                        
                        b.BabyType = type;
                        b.Sprite.spriteId = 466;
                        if( type == EAlgaeBabyType.FLOWER )
                        {
                            b.Sprite.spriteId = 468;
                        }
                        
                        // else
                        //if( Util.Chance( 6 ) )
                        //{
                        //    b.BabyType = EAlgaeBabyType.ROCK;
                        //    b.Sprite.spriteId = 467;
                        //    b.BoxCollider.isTrigger = false;
                        //}
                    }
                }
            }
            if( TileID == ETileType.ALTAR )                                                                                        // Create Altar bonuses
            {
                Body.RotationSpeed = Md.PoleRotationSpeed;

                float rs = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.RotationSpeed );                                        // ori: rotation speed
                if( rs != -1 ) Body.RotationSpeed = rs;

                List<AltarBonusStruct> bnl = new List<AltarBonusStruct>();
                float ang = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.PoleBonusAvailableAngle );                             // ori: pole bonus available angle for items with shield
                if( ang >= 0 )
                    Body.PoleBonusAvailableAngle = ang;

                if ( Mod.MD.Ori( EOrientatorEffect.AddBonusToPole ) >= 0 )
                for( int i = 0; i < 8; i++ )
                    {
                        EDirection dr = ( EDirection ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.AddBonusToPole, i );           // ori: Add bonus to pole
                        if ( Mod.PointDir != EOriPointDir.NONE )
                        for( int a = 0; a < 8; a++ )                        
                            {
                                if( ( int ) dr < Altar.PoleObjList.Count ) break;
                                AltarBonusStruct bn = new AltarBonusStruct();
                                bn.Reset();
                                Altar.PoleObjList.Add( bn );
                            }

                        if( Mod.MD.PointToMeAndAwayAltarBonus != null )
                        {
                            if( Mod.PointDir == EOriPointDir.ME )
                            if( Mod.MD.PointToMeAndAwayAltarBonus.Count >= 1 )
                            {
                                Altar.PoleObjList[ ( int ) dr ].Copy( Mod.MD.PointToMeAndAwayAltarBonus[ 0 ] );
                            }
                            if( Mod.PointDir == EOriPointDir.AWAY )
                            if( Mod.MD.PointToMeAndAwayAltarBonus.Count >= 2 )
                            {
                                Altar.PoleObjList[ ( int ) dr ].Copy( Mod.MD.PointToMeAndAwayAltarBonus[ 1 ] );
                            }
                        }

                        if( Util.LOk( Mod.MD.AltarPoleBonus, ( int ) dr ) )                                            // there are 2 ways to place items in the pole
                        {
                            if( ( int ) dr >= 0 && ( int ) dr < Mod.MD.AltarPoleBonus.Count )
                                Altar.PoleObjList[ i ].Copy( Mod.MD.AltarPoleBonus[ ( int ) dr ] );
                        }
                    }

                EDirection pdr = ( EDirection ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Set_Pole_Direction );  // Orientator Altar pole initial direction
                if( pdr != EDirection.NONE )
                    Altar.SetPoleDir( Util.GetRotationAngleVector( pdr ) );
                else        
                    Altar.SetPoleDir( new Vector3( 0, 0, Md.PoleStartRotation ) );
            }
            break;
        }
    }

    public void Init( EV vartype, List<EDirection> val )
    {
        switch( vartype )
        {
            case EV.DynamicObjectDirectionList:
            if( Control )
            {
                Control.DynamicObjectDirectionList = new List<EDirection>();
                Control.DynamicObjectDirectionList.AddRange( val );
            }
            break;
            case EV.RedRoachBabyList:
            if( Control )
            {
                Control.RedRoachBabyList = new List<EDirection>();
                Control.RedRoachBabyList.AddRange( val );
            }
            break;
        }
    }

    public void Init( EV vartype, EFishSwimType val )
    {
        switch( vartype )
        {
            case EV.FishSwimType:
            if( Control )
            {
                Control.FishSwimType = val;
                if( val == EFishSwimType.NONE )
                if( Body.FishType >= EFishType.FISH_1 && Body.FishType <= EFishType.FISH_FROG )
                    Control.FishSwimType = EFishSwimType.NORMAL;
            }
            break;
        }
    }

    public void Init( EV vartype, EFishType val )
    {
        switch( vartype )
        {
            case EV.WaterObjectType:
            if( Body )
            {
                Body.FishType = val;
                Body.IsFish = false;
                if( val >= EFishType.FISH_1 && val <= EFishType.FISH_FROG ) 
                    Body.IsFish = true;
                Map.I.InitFishGraphics( this );
                
                if( TileID == ETileType.TRAIL )
                {
                    ToggleBool( ref Body.TrailCoil, "Trail Coil" );
                    ToggleBool( ref Body.TrailRotator, "Trail Rotator" );
                    if( Body.TrailRotator ) RotateTo( EDirection.NONE );
                    UpdateAnimation();
                }
            }
            break;
        }
    }

    public void Init( EV vartype, EHerbColor val )
    {
        switch( vartype )
        {
            case EV.HerbColor:
            if( Body )
            {
                Body.HerbColor = val;
                Map.I.InitHerbGraphics( this );
            }
            break;
        }
    }
    public void Init( EV vartype, EHerbType val )
    {
        switch( vartype )
        {
            case EV.HerbType:
            if( Body )
            {
                Body.HerbType = val;
                Body.OriginalHerbType = val;
                Map.I.InitHerbGraphics( this );
            }
            break;
        }
    }
    public void Init( EV vartype, float val )
    {
        switch( vartype )
        {
            case EV.Variation:
            int num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Variation );                  // Ori: variation
            if( num >= 0 )
            {
                if( TileID == ETileType.BARRICADE ) num--;                                                 // -1 to adjust 0 1 bias
                val = num;
            }
            Variation = ( int ) val;

            if( TileID == ETileType.SPIKES )                                                               // toogle bool to set spike initial state
            {
                bool act = Util.IntToBool( Variation );
                ToggleBool( ref act, "Variation" );
                Variation = Util.BoolToInt( act );
                act = false;
                ToggleBool( ref act, "Always Active" ) ;
                if( act ) Variation = 3;
            }
            break;
            case EV.UnitBaseLevel:
            Body.BaseLevel = val;
            break;
            case EV.UnitLives:
            num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Unit_Lives );
            if( num >= 0 )
                val = num;
            if( val < 1 ) val = 1;
            Body.Lives = val;
            break;
            case EV.RealtimeSpeed:
            float sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Move_Speed );
            if( sp >= 0 )                                                                                  // Ori: Move speed from table 1
            {
                float fc = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Move_Speed_Factor );            // Ori: Move Speed Factor (for stair boulder cubes)
                if( fc <= 0 ) fc = 100;
                val = Util.Percent( fc, sp );
            }    

            if( val <= 0 ) val = 10;
        
            ETileType raft = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile(                                 
            ( int ) IniPos.x, ( int ) IniPos.y, ( int ) ELayerType.RAFT );

            if( raft == ETileType.RAFT )
            if( Mod.MD.RestrictToUnitType.Contains( ETileType.RAFT ) )
            if( Map.I.CustomSpeedRaftPos.Contains( IniPos ) == false )  
                Map.I.CustomSpeedRaftPos.Add( IniPos );                                                       // To set a custom speed for a raft group just set the speed of a sinle tile and set the mod to ONLY rafts
            if( raft == ETileType.FOG )
            if( Mod.MD.RestrictToUnitType.Contains( ETileType.FOG ) )                                      // same for custom fog speed
            if( Map.I.CustomSpeedFogPos.Contains( IniPos ) == false )
                Map.I.CustomSpeedFogPos.Add( IniPos );
            if( Control )
            {
                Control.RealtimeSpeed = val;
                float pr = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Move_Speed_Start_Time );
                if( pr >= 0 )
                {
                    float tottime = Control.GetRealtimeSpeedTime();
                    float time = Util.Percent( pr, tottime );                                              // Ori: Move start time percentage based
                    Control.SpeedTimeCounter = time;

                    if( Md.RealtimeSpeedList != null )                                                     // Custom speed list movement type alternate starting point
                    {
                        float tot = 0;
                        for( int i = 0; i < Md.RealtimeSpeedList.Count; i++ )
                            tot += Md.RealtimeSpeedList[ i ];

                        float left = Util.Percent( pr, tot ); 

                        for( int i = 0; i < Md.RealtimeSpeedList.Count; i++ )
                        {
                            if( left <= Md.RealtimeSpeedList[ i ] )
                            {
                                Control.SpeedTimeCounter = left;
                                Control.SpeedListID = i;
                                break;
                            }
                            left -= Md.RealtimeSpeedList[ i ];
                        }
                    }
                }

                Control.SpikeFreeStepTime = Md.SpikeFreeStepTime;
                pr = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Spike_Free_Step_Time );
                if( pr >= 0 )                                                                              // Ori: spike free step time in seconds
                    Control.SpikeFreeStepTime = pr;
            }     

            break;
            case EV.FlyingSpeed:
            if( Control )
            {
                if( val != -1 )
                    Control.FlyingSpeed = val;
                sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.FlyingSpeed );                         // Ori: Flying Speed
                if( sp >= 0 ) Control.FlyingSpeed = sp;
                Control.FlightSpeedFactor = Md.FlyingSpeedFactor;
                sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.FlyingSpeedFactor );                  // Ori: Flying Speed Factor
                if( sp >= 0 ) Control.FlightSpeedFactor = sp;

                if( Md.StartFlyingPhase != -1 )
                    Control.FlightJumpPhase = Md.StartFlyingPhase;
                if( Md.FlyingRotationSpeed >= 0 )
                    Control.FlyingRotationSpeed = Md.FlyingRotationSpeed;
            }

            if( Body )
            {
                Body.QQMissHeroDamage = Md.QQMissHeroDamage;
                sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.QQMissHeroDamage );                  // Ori:QQ Miss Hero Damage
                if( sp != -1 ) Body.QQMissHeroDamage = sp;
                Body.QQMinDamage = Md.QQMinDamage;
                sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.QQMinDamage );                       // Ori:QQ Min Damage
                if( sp != -1 ) Body.QQMinDamage = sp;
                Body.QQMaxDamage = Md.QQMaxDamage;
                sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.QQMaxDamage );                       // Ori:QQ max Damage
                if( sp != -1 ) Body.QQMaxDamage = sp;
            }

            break;
            case EV.WaitTime:
            if( Control )
                Control.WaitTime = val;
            break;
            case EV.ResourcePersist:
            if( Body )
            {
                num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Res_Persist_Times );          // Ori: Res Persist times
                if( num > 0 )
                    val = num;
                Body.ResourcePersistTotalSteps = ( int ) val;   
            }
            break;
            case EV.BaseWaspTotSpawnTimer:
            if( Control )
            {
                num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.WaspBreedSpeed );
                if( num > 0 )
                    val = num;                
                Control.BaseWaspTotSpawnTimer = val;
            }
            break;
            case EV.WaspSpawnInflationPerTile:
            if( Control )
                Control.WaspSpawnInflationPerTile = val;
            break;
            case EV.ExtraWaspSpawnTimerPerTile:
            if( Control )
                Control.ExtraWaspSpawnTimerPerTile = val;
            break;
            case EV.ExtraWaspSpawnInflationPerSec:
            if( Control )
                Control.ExtraWaspSpawnInflationPerSec = val;
            break;
            case EV.MaxSpawnCount:
            if( Control )
            {
                num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.BabyAmount );
                if( num >= 0 )
                    val = num;
                else
                    if( Control.MaxSpawnCount < 1 )
                        val = 0;
                Control.MaxSpawnCount = ( int ) val;
            }
            break;
            case EV.WaspDamage:
            if( Control )
                Control.WaspDamage = val;
            break;
            case EV.InitialForcedFrontalMovementFactor:
            if( Control )
            {
                Control.ForcedFrontalMovementFactor = ( int ) val;
                Control.ForcedFrontalMovementDir = Control.InitialDirection;
                RotateTo( Control.ForcedFrontalMovementDir );
            }
            break;
            case EV.MotherWaspRadius:
            if( Control )
                Control.MotherWaspRadius = ( int ) val;
            break;

            case EV.ShieldedWaspChance:
            if( Body )
            {
                Body.ShieldedWaspChance = val;
                Body.MaxShieldedWasps = Mod.MD.MaxShieldedWasps;
            }
            break;

            case EV.RestingDistance:
            num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Resting_Radius );
            if( num >= 0 )
                val = num;                                                                              // Orientator based radius
            if( Control )
                Control.BaseRestDistance = ( int ) val;

            num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Wake_Up_Group );
            if( num >= 0 )
                val = num;
            else
                val = Mod.MD.WakeUpGroup;
            if( Control )
                Control.WakeUpGroup = ( int ) val;

            num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.WakeUpTime );
            if( num >= 0 )
                val = num;
            else
                val = Mod.MD.WakeUpTime;
            if( Control )
                Control.WakeupTotalTime = ( int ) val;

            break;
            case EV.ResourceAmount:
            num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.ResourceAmount );
            if( num > 0 )                                                                                            // Ori resource amount
                val = num;

            if( Body )
            {
                Unit gaia = Map.I.GetUnit( ETileType.CLOSEDDOOR, IniPos );
                if( gaia )
                {
                    val += AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.GATE_PRICE, 
                        Mod.MD.ResourceType, false );                                                                // Gate price tech
                    if( val < 0 ) val = 0;
                }
                Body.StackAmount = ( int ) val;
                val = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Mini_Dome_Total_Time );
                if( val > 0 )                                                                                        // Ori mini dome total time
                    Body.MiniDomeTotalTime = val;

                val = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Resource_Waste_Time );
                if( val > 0 )                                                                                        // Res waste total time
                    Body.ResourceWasteTotalTime = val;
                else Body.ResourceWasteTotalTime = Mod.MD.ResourceWasteTime;

                if( TileID == ETileType.ITEM )
                {
                    val = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Resource_Slot );
                    if( val > 0 )                                                                                        // Res Slot type
                        Body.ResourceSlot = ( int ) val;
                    else Body.ResourceSlot = Mod.MD.ResourceSlot;
                    Body.Sprite5.gameObject.SetActive( false );
                    if( Spell.IsEquipable( ( ItemType ) Variation ) == false )
                        Body.ResourceSlot = 0;
                    else if( Body.ResourceSlot <= 0 )
                        Body.ResourceSlot = 1;
                    if( Body.ResourceSlot > 0 )
                    {
                        Body.Sprite5.gameObject.SetActive( true );
                        Body.Sprite5.spriteId = 53 + Body.ResourceSlot - 1;
                    }
                }
                else
                if( TileID == ETileType.SAVEGAME )
                {
                    RightText.text = "x" + GS.GetLoadCost( this ).ToString( "0.#" ); 
                    RightText.gameObject.SetActive( true );
                }
                else
                if( TileID == ETileType.SECRET )
                {
                    RightText.text = "x" + Body.StackAmount;
                    RightText.gameObject.SetActive( true );
                }
            }
            break;
            case EV.HitPointsRatio:
            Body.HitPointsRatio = val;
            break;
            case EV.BoulderPunchPower:
            if( Control )
            if( val != -1 )
                Control.BoulderPunchPower = val;
            if( Md.SpikedBoulder )
            {
                Body.SpikedBoulder = true;
                if( Body.BabySpriteFolder )
                    Body.BabySpriteFolder.gameObject.SetActive( true );
            }
            break;
            case EV.DynamicMaxSteps:
            if( Control )
                Control.DynamicMaxSteps = ( int ) val;
            break;
            case EV.TouchCount:
            Body.TouchCount = ( int ) val;
            break;
            case EV.TotHp:
            if( Body )
            {
                float hp = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Unit_HP );
                if( hp > 0 )
                    val = hp;
                if( Body.BigMonster == false ) val = 1;
                Body.BaseTotHP = val;
                Body.TotHp = val;
                Body.Hp = val;
            }
            break;
            case EV.MeleeAttack:
            if( MeleeAttack )
            {
                float att = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Melee_Attack_Base_Damage );
                if( att != -1 ) val = att;
                if( val > -1 )
                {
                    MeleeAttack.BaseDamage = val;
                    UpdateLevelingData();
                }
            }

            break;
            case EV.MeleeAttackTotalTime:
            if( MeleeAttack )
            {
                if( val > -1 )
                    MeleeAttack.TotalAttackTime = val;

                float tt = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Melee_Attack_Total_Time );
                if( tt >= 0 )                                                                                  // Ori: melee attack total time
                    MeleeAttack.TotalAttackTime = tt;

                float pr = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.MAttack_Speed_Start_Time );
                if( pr < 0 ) pr = 0;
                if( pr >= 0 )
                {                                                                                              // Ori: ranged start time percentage based
                    MeleeAttack.SpeedTimeCounter = Util.Percent( pr, MeleeAttack.GetRealtimeSpeedTime() );
                    
                    if( Util.HasDada( Md.MeleeAttackTotalTimeList ) )                                          // Custom speed list movement type alternate starting point
                    {
                        MeleeAttack.AttackTotalTimeList = new List<float>();
                        MeleeAttack.AttackTotalTimeList.AddRange( Md.MeleeAttackTotalTimeList );
                        if( MeleeAttack.TotalAttackTime > 0 )
                            MeleeAttack.AttackTotalTimeList[ 0 ] = MeleeAttack.TotalAttackTime;                // Use TotalAttackTime as a custom speed at first id

                        float tot = 0;
                        for( int i = 0; i < MeleeAttack.AttackTotalTimeList.Count; i++ )
                            tot += MeleeAttack.AttackTotalTimeList[ i ];

                        float left = Util.Percent( pr, tot );

                        for( int i = 0; i < MeleeAttack.AttackTotalTimeList.Count; i++ )
                        {
                            if( left <= MeleeAttack.AttackTotalTimeList[ i ] )
                            {
                                MeleeAttack.SpeedTimeCounter = left;
                                MeleeAttack.SpeedListID = i;
                                break;
                            }
                            left -= MeleeAttack.AttackTotalTimeList[ i ];
                        }

                        if( TileID ==  ETileType.SPIKES && Util.IsEven( MeleeAttack.SpeedListID ) == false )
                            MeleeAttack.FlipSpike( true );
                    }
                }
            }

            break;
            case EV.RangedAttackTotalTime:
            if( RangedAttack )
            {
                if( val > -1 )
                    RangedAttack.TotalAttackTime = val;

                float pr = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.RAttack_Speed_Start_Time );
                if( pr < 0 ) pr = 0;
                if( pr >= 0 )
                {
                    RangedAttack.SpeedTimeCounter = Util.Percent( pr, RangedAttack.GetRealtimeSpeedTime() );            // Ori: ranged start time percentage based

                    if( Md.RangedAttackTotalTimeList != null && Md.RangedAttackTotalTimeList.Count > 0 )                // Custom speed list movement type alternate starting point
                    {
                        RangedAttack.AttackTotalTimeList = new List<float>();
                        RangedAttack.AttackTotalTimeList.AddRange( Md.RangedAttackTotalTimeList );
                        if( RangedAttack.TotalAttackTime > 0 )
                            RangedAttack.AttackTotalTimeList[ 0 ] = RangedAttack.TotalAttackTime;                        // Use TotalAttackTime as a custom speed at first id

                        float tot = 0;
                        for( int i = 0; i < RangedAttack.AttackTotalTimeList.Count; i++ )
                            tot += RangedAttack.AttackTotalTimeList[ i ];

                        float left = Util.Percent( pr, tot );

                        for( int i = 0; i < RangedAttack.AttackTotalTimeList.Count; i++ )
                        {
                            if( left <= RangedAttack.AttackTotalTimeList[ i ] )
                            {
                                RangedAttack.SpeedTimeCounter = left;
                                RangedAttack.SpeedListID = i;
                                break;
                            }
                            left -= RangedAttack.AttackTotalTimeList[ i ];
                        }
                    }
                }
            }
            break;

            case EV.RangedAttack:
            if( RangedAttack )
                {               
                    if( val > -1 )
                    {
                        RangedAttack.BaseDamage = val;
                        UpdateLevelingData();
                    }
                }
            break;
            case EV.RangedAttackRange:
            if( RangedAttack )
            {
                if( TileID == ETileType.HUGGER )
                    if( val == -1 ) val = 3;
                if( val != -1 )
                {
                    RangedAttack.BaseRange = val;
                }
                UpdateLevelingData();
            }            
            break;
            case EV.BaseMeleeShield:
            if( Body )
                Body.BaseMeleeShield = val;
            UpdateLevelingData();
            break;
            case EV.InflictedDamageRate:
            if( Body )
                Body.InflictedDamageRate = val;
            break;
            case EV.BaseMissileShield:
            if( Body )
                Body.BaseMissileShield = val;
            UpdateLevelingData();
            break;

            case EV.RaftGroupID:
            if( Control )
            {
                Control.RaftGroupID = ( int ) val;
            }
            break;
            case EV.BaseMiningChance:
            if( Body )
                Body.BaseMiningChance = val;
            break;
            case EV.SortMiningChanceFactor:
            if( Body )
            {
                if( Util.Chance( val ) )
                {
                    if( Mod.MD.SortMiningChanceRandomFactorList != null && Mod.MD.SortMiningChanceRandomFactorList.Count == Mod.MD.SortMiningChanceRandomList.Count )
                    {
                        int id = Util.Sort( Mod.MD.SortMiningChanceRandomFactorList );
                        Body.BaseMiningChance = Mod.MD.SortMiningChanceRandomList[ id ];
                    }
                }
                int mt = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Mine_Chance );                       // Ori: Mine Chance
                if( mt >= 0 )
                    Body.BaseMiningChance = mt;
            }
            break;
            case EV.MineLeverChance:
            if( Body )
            {
                Body.LeverMine = false;
                if( Util.Chance( val ) ) Body.LeverMine = true;
            }
            break;

            case EV.MineBonuses:
            if( Mine )
            {
                if( Util.Chance( Md.RopeConnectedMineChance ) ) Mine.RopeMine = true; else
                if( Util.Chance( Md.HammerMineChance ) ) Mine.HammerMine = true; else
                if( Util.Chance( Md.ChiselMineChance ) ) Mine.ChiselMine = true;
                if( Util.Chance( Md.SpikedMineChance ) ) Mine.SpikedMine = true;
                if( Util.Chance( Md.StickyMineChance ) ) Mine.StickyMine = true;
                if( Util.Chance( Md.DynamiteMineChance ) ) Mine.DynamiteMine = true;
                if( Util.Chance( Md.CogMineChance ) ) Mine.CogMine = true;
                if( Util.Chance( Md.ArrowMineChance ) ) Mine.ArrowMine = true;
                if( Util.Chance( Md.HoleMineChance ) ) Mine.HoleMine = true;
                if( Util.Chance( Md.SwapperMineChance ) ) Mine.SwapperMine = true;
                if( Util.Chance( Md.HoleMineChance ) ) Mine.HoleMine = true;
                if( Util.Chance( Md.CannonMineChance ) ) Mine.CannonMine = true;
                if( Util.Chance( Md.MagnetMineChance ) ) Mine.MagnetMine = true;
                if( Util.Chance( Md.WheelMineChance ) ) Mine.WheelMine = true;
                if( Util.Chance( Md.JumperMineChance ) ) Mine.JumperMine = true;
                if( Util.Chance( Md.GloveMineChance ) ) Mine.GloveMine = true;
                if( Mine.WheelMine ) Mine.MineBonusDir = EDirection.NONE;
                num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.MineBonusDir );
                if( num >= 0 )
                {
                    Mine.MineBonusDir = ( EDirection ) num;
                    Mine.AnimateIconTimer = .1f;
                }
                //float chc = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Mine_Bonus_Chance );  ////////// attention: make a generic ori mine bonus chance and apply to the right bonus above
            }
            break;
            case EV.InfectedChance:
            if( Body )
            {
                Body.IsInfected = false;
                if( Util.Chance( val ) )
                {
                    Body.IsInfected = true;
                    Unit un = Map.I.GetUnit( ETileType.SCARAB, IniPos );
                    if( un )
                    {
                        num = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Infected_Radius );
                        if( num >= 1 )
                            val = num;                                                                              // Orientator: infected radius
                        else val = Md.InfectedRadius;
                        Body.InfectedRadius = ( int ) val;
                        Body.InfectedSprite.gameObject.SetActive( true );
                        //int rad = 1 + ( int ) ( ( Control.InfectedRadius ) * 2 );
                        //Body.InfectedSprite.transform.localScale = new Vector3( rad, rad, 1 );
                        un.Body.PerformPreMoveAttack = false;
                        un.Body.PerformPostMoveAttack = true;
                    }
                }
            }
            break;
            case EV.BaseTotalRespawnTime:
            if( Control )
                Control.BaseTotalRespawnTime = val;
            break;
            case EV.BaseTotalRespawnAmount:
            if( Control )
                Control.BaseTotalRespawnAmount = ( int ) val;
            break;
            case EV.FishingPowerFactor:
            if( Control )
                Control.FishingPowerFactor = val;
            break;
            case EV.FishRedLimit:
            if( Control )
                Control.FishRedLimit = val;
            break;
            case EV.FishGreenLimit:
            if( Control )
                Control.FishGreenLimit = val;
            break;
            case EV.FishingBonusAmount:
            if( Control )
                Control.FishingBonusAmount = val;
            break;
            case EV.MinFishSpeed:
            if( Control )
                Control.MinFishSpeed = val;
            break;
            case EV.MaxFishSpeed:
            if( Control )
            {
                Control.MaxFishSpeed = val;
            }
            break;
            case EV.OverFishSecondsNeededPerUnit:
            if( Control )
                Control.OverFishSecondsNeededPerUnit = val;
            break;
            case EV.FishingPole:
            if( Control )
            {
                 sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.Fishing_Pole_Level );
                 if( sp >= 0 )                                                                                  // Ori: Move Pole Extra level
                 {
                     Control.PoleExtraLevel = ( int ) sp;
                 }
            }

            if( Water )
            {
                Water.PoleBonusEffType = Md.PoleBonusEffType;
                Water.PoleBonusCnType = Md.PoleBonusCnType;
                Water.PoleBonusVal1 = Md.PoleBonusVal1;
                Water.PoleBonusVal2 = Md.PoleBonusVal2;
                int id = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.PoleBonusType );                 // Ori: Pole Bonus Eff type
                if( Mod.MD.PoleBonusEffList != null &&
                    id >= 0 && id < Mod.MD.PoleBonusEffList.Count )
                    Water.PoleBonusEffType = Mod.MD.PoleBonusEffList[ id ];
                id = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.PoleBonusCondition );                // Ori: Pole Bonus Cn type
                if( Mod.MD.PoleBonusCnList != null &&
                    id >= 0 && id < Mod.MD.PoleBonusCnList.Count )
                    Water.PoleBonusCnType = Mod.MD.PoleBonusCnList[ id ]; 
                sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.PoleBonusVal1 );                             // Ori: Pole Bonus val 1
                if( sp != -1 ) Water.PoleBonusVal1 = sp;
                sp = Mod.GetOrientatorNum( IniPos, EOrientatorEffect.PoleBonusVal2 );                             // Ori: Pole Bonus val 2
                if( sp != -1 ) Water.PoleBonusVal2 = sp;

                ToggleBool( ref Water.PoleBonusIsGlobal, "PoleBonusIsGlobal" );
            }

            break;
            case EV.RandomDirection:
            if( val <= 1 ) return;
            val = Mathf.Clamp( val, 0, 4 );
            int[ ] dirlist = new int[ ] { 0 };
            if( val == 1 ) dirlist = new int[ ] { 0, -1, 1 };
            if( val == 2 ) dirlist = new int[ ] { 0, -1, 1, -2, 2 };
            if( val == 3 ) dirlist = new int[ ] { 0, -1, 1, -2, 2, -3, 3 };
            if( val == 4 ) dirlist = new int[ ] { 0, -1, 1, -2, 2, -3, 3, 4 };
            int fact = dirlist[ Random.Range( 0, dirlist.Length ) ];
            int dir = ( int ) Util.RotateDir( ( int ) Dir, fact );
            dir = ( int ) Util.FlipDir( ( EDirection ) dir, Sector.CS.FlipX, Sector.CS.FlipY );
            RotateTo( ( EDirection ) dir );
            if( Control )
                Control.InitialDirection = Dir;
            break;

            case EV.OrientatorDirection:
            EDirection dr = ( EDirection ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.SetDirection );        // Orientator based direction
            if( dr != EDirection.NONE )
            {
                RotateTo( dr );
                if( Control )
                    Control.InitialDirection = dr;
            }

            dr = ( EDirection ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.UpMineDirection );                // Orientator based up mine direction
            if( dr != EDirection.NONE )
            if( Body )
                Body.UPMineDir = dr;
            break;

            case EV.HerbBonusAmount:
            if( Body )
                Body.HerbBonusAmount = ( int ) val;
            Map.I.InitHerbGraphics( this );
            break;

            case EV.AvailableFireHits:
            if( Body )
                Body.AvailableFireHits = ( int ) val;
            break;
            case EV.VsHeroRoachkAttackIncreasePerBaby:
            if( Body )
            {
                Body.VsHeroRoachAttackIncreasePerBaby = val;
            }
            break;
            case EV.VsRoachAttDecreasePerBaby:
            if( Body )
            {
                Body.VsRoachAttDecreasePerBaby = val;
            }
            break;
            case EV.MineType:
            if( Body )
            {
                if( Mod.MD.RandomMineType != null && Mod.MD.RandomMineType.Count >= 1 )
                    if( Mod.MD.RandomMineTypeFactor != null && Mod.MD.RandomMineType.Count == Mod.MD.RandomMineTypeFactor.Count )
                    {
                        int id = Util.Sort( Mod.MD.RandomMineTypeFactor );
                        val = ( int ) Mod.MD.RandomMineType[ id ];
                    }

                Body.MinePushSteps = 0;
                int st = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.HookPushSteps );
                if( st != -1 )                                                                               // Ori: Hook push steps init
                    Body.MinePushSteps = st;
                else
                    if( Mod.MD.HookPushStepList != null && Mod.MD.HookPushStepList.Count >= 1 )               
                    if( Mod.MD.HookPushStepListFactor != null &&
                        Mod.MD.HookPushStepList.Count == Mod.MD.HookPushStepListFactor.Count )
                        {
                            int idd = Util.Sort( Mod.MD.HookPushStepListFactor );                            // push steps from list
                            Body.MinePushSteps = ( int ) Mod.MD.HookPushStepList[ idd ];
                        }
                Body.MineType = ( EMineType ) val;
            }
            break;

            case EV.MineLeverChainTarget:
            if( Body && TileID == ETileType.MINE )
            {
                if( GS.IsLoading ) return;
                int forced = ( int ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.ChainConnect );
                if( forced < 0 )
                    if( Mod.MD.MineLeverChainTargetModList == null || Mod.MD.MineLeverChainTargetModList.Count < 1 ||    // no connection for chain
                        Mod.MD.MineLeverChainTargetModFactor == null ||
                        Mod.MD.MineLeverChainTargetModFactor.Count != Mod.MD.MineLeverChainTargetModList.Count )
                    {
                        //Body.Rope.gameObject.SetActive( false );
                        return;
                    }
                int max = 1;
                if( forced == -1 )
                {
                    int idd = Util.Sort( Mod.MD.MineLeverChainTargetModFactor );
                    val = ( int ) Mod.MD.MineLeverChainTargetModList[ idd ];
                    for( int i = 0; i < Map.I.RM.SD.ModList.Length; i++ )
                        if( Map.I.RM.SD.ModList[ i ].ModNumber == ( EModType ) val )
                        {
                            max = Map.I.RM.SD.ModList[ i ].MaxChainConnections;                        // calculates max chain connections
                            break;
                        }
                }
                Vector2 tg = new Vector2( -1, -1 );
                List<Unit> tglist = new List<Unit>();
                Sector hs = Sector.CS;
                if( val != ( int ) EModType.NONE )
                for( int y = ( int ) hs.Area.yMin - 1; y < hs.Area.yMax + 1; y++ )                                   
                for( int x = ( int ) hs.Area.xMin - 1; x < hs.Area.xMax + 1; x++ )
                if ( IniPos != new Vector2( x, y ) )
                {
                    Unit mn = GetConnectableUnit( new Vector2( x, y ) );
                    if( mn )
                        {
                            EModType mod = Mod.GetModInTile( new Vector2( x, y ) );                                       // mod number to connect chain to
                            if( forced >= 0 )
                            {
                                int targetnum = ( int ) Mod.GetOrientatorNum( 
                                new Vector2( x, y ), EOrientatorEffect.ChainConnect );                                // Chain connection made by orientator
                                if( targetnum == forced )
                                if( mn.Body.RopeConnectFather.Count < max )
                                    tglist.Add( mn );
                            }
                            else
                            if( val == ( int ) mod )
                            {
                                if( mn.Body.RopeConnectFather != null )                                               // Chain connection made by mod Number
                                if( mn.Body.RopeConnectFather.Count < max )
                                    tglist.Add( mn );
                            }
                        }
                }
                if( tglist.Count < 1 )
                    Debug.LogError( "Chain destination mod not set: " + 
                    ( EModType ) val + " " + Helper.I.GetCubeTile( IniPos ) + " " + IniPos );
                int id = Random.Range( 0, tglist.Count );
                tg = tglist[ id ].Pos;
                Body.RopeConnectSon = tglist[ id ];                                                                   // Updates son and fathers chained obj

                if( tglist[ id ].Body.RopeConnectFather.Contains( this ) == false )
                    tglist[ id ].Body.RopeConnectFather.Add( this );

                Body.ClockwiseChainPush = false;
                Body.MineLeverActivePuller = this;

                if( Util.Chance( Mod.MD.ClockwiseChainPushChance ) ) 
                    Body.ClockwiseChainPush = true;

                if( Mod.MD.Ori( EOrientatorEffect.SetLeverDirection ) >= 0 )
                for( int i = 0; i < 8; i++ )
                {
                    dr = ( EDirection ) Mod.GetOrientatorNum( IniPos, EOrientatorEffect.SetLeverDirection, i );
                    if( dr != EDirection.NONE )
                    {
                        if( Mod.PointDir == EOriPointDir.AWAY )                                                       // ORIENTATION pointing away Lever Mine: clockwise chain push
                        {
                            Body.MineLeverDir = Mod.OriDir;
                            Body.ClockwiseChainPush = true;
                        }
                        else
                        if( Mod.PointDir == EOriPointDir.ME )
                        {
                            EDirection inv = ( EDirection ) Manager.I.U.InvDir[ ( int ) Mod.OriDir ];
                            Body.MineLeverDir = inv;                                                     // ORIENTATION pointing to Lever Mine: clockwise chain push
                            Body.ClockwiseChainPush = false;
                        }
                    }
                }
                UpdateChainSizes( tg );
            }
            break;
        }
    }

    private void ToggleBool( ref bool val, string varname, Vector2 tg = default ( Vector2 ) )
    {
        if( tg == Vector2.zero ) tg = IniPos;
        ETileType d1 = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( ( int )                                             // Gets decor tile
        tg.x, ( int ) tg.y, ( int ) ELayerType.DECOR );
        ETileType d2 = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( ( int ) 
        tg.x, ( int ) tg.y, ( int ) ELayerType.DECOR2 );

        for( int id = 1; id <= 2; id++ )
        if( ( id == 1 && d1 == ETileType.BOOL_TOOGLE_1 ) ||                                                               // decor toogle tile present?
            ( id == 2 && d2 == ETileType.BOOL_TOOGLE_2 ) )
        {
            if( id == 1 )
            if( Md.BoolToogle1VarName == varname )                                                                        // Toogles var 1
              { val = !val; Map.I.RM.ToggleList.Add( tg ); }                                                              // Fill buffer 1 of decor tiles to delete
            if( id == 2 )
            if( Md.BoolToogle2VarName == varname )                                                                        // Toogles var 2
              { val = !val; Map.I.RM.ToggleList.Add( tg ); }                                                              // Fill buffer 2 of decor tiles to delete                                                                                             
        }    
    }
    public Unit GetConnectableUnit( Vector2 tg )
    {
        Unit un = Map.GFU( ETileType.MINE, tg );

        if( un == null )
            un = Map.I.GetUnit( tg, ELayerType.MONSTER );

        if( un == null && Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ] != null )
        if( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ].Count > 0 )
            un = Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ 0 ];
        if( un )
        {
            EModType mod = Mod.GetModInTile( tg );
            if( GS.IsLoading == false )
            if( Md.LimitChainConnectionModList != null &&
                Md.LimitChainConnectionModList.Count > 0 &&
                Md.LimitChainConnectionModList.Contains( mod ) == false )
                return null;                                                                                            // if this list is filled, only connect to objects to mods in this list

            if( un.TileID == ETileType.MINE ) return un;
            if( Md.CustomChainConnectionAvailability != null &&
                Md.CustomChainConnectionAvailability.Contains( un.TileID ) ) return un;
        }
        return null;
    }

    public void Init( EV vartype, EDirection val )
    {
        switch( vartype )
        {
            case EV.InitialDirection:
            if( Control )
                Control.InitialDirection = val;
            break;

            case EV.Direction:
            EDirection dr = Util.FlipDir( val, Sector.CS.FlipX, Sector.CS.FlipY );

            RotateTo( dr );
            if( Control )
                Control.InitialDirection = dr;
            break;

            case EV.FixedRestingDirection:
            EDirection rot = Util.FlipDir( val, Sector.CS.FlipX, Sector.CS.FlipY );
            RotateTo( rot );
            if( Control )
            {
                Control.InitialDirection = rot;
                Control.FixedRestingDirection = rot;
            }
            break;

            case EV.MineLeverDirection:
            val = Util.FlipDir( val, Sector.CS.FlipX, Sector.CS.FlipY );
            if( Body )
            {
                if( val == EDirection.NONE ) val = ( EDirection ) Util.GetRandomDir();
                Body.MineLeverDir = val;
            }
            break;
        }
    }

    public void Init( EV vartype, Vector2 val, Mod md = null )
    {
        switch( vartype )
        {
            case EV.WhiteGateGroup:
            if( Control )
            {
                Control.WhiteGateGroup = val;
            }
            break;            
        }
    }

    public void Init( EV vartype, Vector2 val, int id )
    {
        switch( vartype )
        {
            case EV.FlyingActionTarget:
            if( Control )
            {
                Control.FlyingActionTarget.Add( val );
            }
            break;
        }
    }

    public void Init( EV vartype, float val, int id, Mod md )
    {
        switch( vartype )
        {
            case EV.FlyingActionVal:
            if( Control )
            {
                Control.FlyingActionVal.Add( val );
            }
            break;
            case EV.FlyingActionLoopCount:
            if( Control )
            {
                Control.FlyingActionLoopCount.Add( ( int ) val );
            }
            break;
            case EV.FlyingActionTotalTime:
            if( Control )
            {
                Control.FlightActionTotalTime.Add( val );
            }
            break;
            case EV.FishAction:
            if( Control )
            {
                FishActionStruct fa = new FishActionStruct();              
                fa.ConditionType = Mod.MD.FishAction[ id ].ConditionType;
                fa.ConditionVal = Mod.MD.FishAction[ id ].ConditionVal;
                fa.EffFishSwimType = Mod.MD.FishAction[ id ].EffFishSwimType;
                fa.ActionTotalTime = Mod.MD.FishAction[ id ].ActionTotalTime;
                fa.TotalTimesApplied = Mod.MD.FishAction[ id ].TotalTimesApplied;
                fa.TimesApplied = 0;
                fa.EffectType = Mod.MD.FishAction[ id ].EffectType;
                fa.EffectVal = Mod.MD.FishAction[ id ].EffectVal;
                fa.EffectVal2 = Mod.MD.FishAction[ id ].EffectVal2;
                fa.WaitActionID = Mod.MD.FishAction[ id ].WaitActionID;
                fa.WaitTotalTime = Mod.MD.FishAction[ id ].WaitTotalTime;
                Control.FishAction.Add( fa );
            }
            break;
        }
    }

    public void Init( EV vartype, Color val )
    {
        switch( vartype )
        {
            case EV.SpriteColor:
            if( Body )
            {
                Body.SpriteColor = val;
            }
            break;
        }
    }
    public void CheckFireDamage()
    {
        //Unit fire = Map.I.GetUnit( ETileType.FIRE, Pos );
        //if( fire == null ) return; 
        //float tottime = Control.GetRealtimeSpeedTime();
        if( Body.TakeFireDamage() == false ) return;
        if( Control.IsFlyingUnit == false )
            Control.SpeedTimeCounter = 0;
         iTween.ShakePosition( Graphic.gameObject, new Vector3( .09f, .09f, 0 ), .2f );
        if( ValidMonster )
        {
            Body.CreateBloodSpilling( transform.position );                                         // Blood FX
            MasterAudio.PlaySound3DAtVector3( "Monster Falling", transform.position ); 
        }
    }
    public bool WakeMeUp()
    {
        Map.I.CountRecordTime = true;
        if( Control.WakeupTotalTime > 0 )                                                       // Wake up timer
        if( Control.WakeupTimeCounter == -1 )
        {
            Control.WakeupTimeCounter = Control.WakeupTotalTime;
            RestLine.RemoveIt( this );
            return true;
        }

        Control.SpeedTimeCounter = 0;
        Control.Resting = false;
        if( Control.RestingRadiusSprite )
            Control.RestingRadiusSprite.gameObject.SetActive( false );                    // Disables Radius sprite 
        Util.Punch( G.Hero.Pos, Pos, Graphic.gameObject, 0.6f, 0.6f );

        RestLine.RemoveIt( this );                                                        // Destroy line animation
 
        if( Control.WakeUpGroup < 0 )
            return false;
        return true;
    }
    public void RemoveChains( bool cubeclear = false )
    {
        if( Body.RopeConnectSon == null && Util.HasDada( Body.RopeConnectFather ) == null )          // normal non connected monster
            return;

        if( Body.OriginalHookType == EMineType.HOOK )
        if( Body.RopeConnectFather == null || Body.RopeConnectFather.Count < 1 ) return;
        
        if( GS.IsLoading )                                                                           // If its cube loading, only despawn chain objects instead of killing father list
        {
            for( int i = Body.RopeConnectFather.Count - 1; i >= 0; i-- )
            {
                if( Body.RopeConnectFather[ i ].Body.Rope )
                if( PoolManager.Pools[ "Pool" ].IsSpawned(
                    Body.RopeConnectFather[ i ].Body.Rope.transform.parent ) )
                {
                    PoolManager.Pools[ "Pool" ].Despawn(
                    Body.RopeConnectFather[ i ].Body.Rope.transform.parent );
                    Body.RopeConnectFather[ i ].Body.Rope = null;
                }
            }
            if( Body.Rope )
            {
                if( PoolManager.Pools[ "Pool" ].IsSpawned( Body.Rope.transform.parent ) )
                    PoolManager.Pools[ "Pool" ].Despawn( Body.Rope.transform.parent );
                Body.Rope = null;
            }
            return;
        }

        if( cubeclear )
        {
            if( Body.RopeConnectFather != null )
            for( int i = Body.RopeConnectFather.Count - 1; i >= 0; i-- )                              // cube is clear: kill all connections
            {
                Body.RopeConnectFather[ i ].Kill();
            }
            return;
        }

        if( Map.I.IsTileOnlyGrass( Pos ) && TileID != ETileType.MINE )
        {
            //if( Body.OriginalHookType == EMineType.HOOK )          
            //if( Body.RopeConnectSon == null ) return;
            //Unit prefabUnit = Map.I.GetUnitPrefab( ETileType.MINE );
            //GameObject go = Map.I.CreateUnit( prefabUnit, Pos );                                      // Creates the hook object after chained kill - removed may be implemented later
            //Unit res = go.GetComponent<Unit>();
            //res.Body.MineType = Body.OriginalHookType;
            //res.Body.MinePushSteps = 0;
            //for( int i = Body.RopeConnectFather.Count - 1; i >= 0; i-- )
            //{
            //    Body.RopeConnectFather[ i ].Body.RopeConnectSon = res;
            //    res.Body.RopeConnectFather.Add( Body.RopeConnectFather[ i ] );
            //    Body.RopeConnectFather[ i ].UpdateChainSizes( res.Pos );
            //}
        }
        else
        {
            if( Body.RopeConnectSon )
            if( Body.RopeConnectSon.Body.OriginalHookType == EMineType.SHACKLE )          
            {
                Body.ShackleDistance = -1;
                Body.RopeConnectSon.Body.RopeConnectFather.Remove( this ); 
                return;
            }

            if( Body.MineType != EMineType.HOOK )
            //if( Body.MineType != EMineType.SHACKLE )   // new to avoid hero step on shakle and de
            for( int i = Body.RopeConnectFather.Count - 1; i >= 0; i-- )
            {
                if( GS.IsLoading == false )
                    Body.RopeConnectFather[ i ].Kill();
            }
        }
        if( Body.Rope )                                                                // despawn chain object
        {
            if( PoolManager.Pools[ "Pool" ].IsSpawned( Body.Rope.transform.parent ) )
                PoolManager.Pools[ "Pool" ].Despawn( Body.Rope.transform.parent );
            Body.Rope = null;
        }
    }
    public void UpdateChainSizes( Vector2 tg )
    {
        if( Body.Rope == null )                                                            // Creates the chain object if theres no one yet created
        {
            Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Chain" );
            Body.Rope = tr.GetComponentInChildren<TackRope>();
            tr.transform.parent = Graphic.transform;
            tr.name = "Chain " + Random.Range(0,99);
            Body.Rope.name = "Rope";
            ChainLinks cl = tr.GetComponentInChildren<ChainLinks>();
            cl.transform.localPosition = Vector2.zero;
            cl.gameObject.SetActive( true );
            Body.Rope.gameObject.SetActive( true );
            Body.RopeOrigin = Body.Rope.ObjectA.GetComponent<Tack>();
            Body.RopeDestination = Body.Rope.ObjectB.GetComponent<Tack>();
            Body.RopeRotationHelper = cl.RotationHelper;
            Body.RopeOrigin.transform.localPosition = Vector2.zero;
            Body.RopeOrigin.gameObject.SetActive( true );
            Body.RopeDestination.gameObject.SetActive( true );
            Body.RopeDestination.transform.localPosition = new Vector3( 1, 1 );
            Body.RopeRotationHelper.gameObject.SetActive( true );
            Body.RopeRotationHelper.gameObject.SetActive( false );
            if( Body.MineHasLever() )
                Body.RopeRotationHelper.gameObject.SetActive( true );
        }

        //if( Body.MineType == EMineType.SPIKE_BALL && Body.ShackleDistance >= 1 ) return;   // new
        Body.Rope.gameObject.SetActive( true );        
        Vector3 add = Vector3.zero;
        Vector3 dist = new Vector3( tg.x, tg.y, 0 ) - transform.position;
        if( Body.RopeConnectSon && Body.RopeConnectSon.Body.MineType == EMineType.HOOK )
            add = new Vector3( 0, -0.27f, 0 );
        Body.Rope.transform.parent.transform.localScale = new Vector3( 1, 1, 1 );
        Body.RopeDestination.transform.localPosition = dist + add;
        float vl = -0.4f;
        if( Body.ClockwiseChainPush ) vl *= -1;
        Quaternion qn = Util.GetRotationToPoint( Pos, Body.RopeDestination.transform.position );
        Body.RopeRotationHelper.transform.rotation = Quaternion.RotateTowards(
        Body.RopeRotationHelper.transform.rotation, qn, 999f );
        if( Body.MineType == EMineType.SHACKLE ) vl = 0.2f;
        Body.RopeOrigin.transform.localPosition = Body.RopeRotationHelper.transform.right * vl;
        Body.Rope.AutoCalculateAmountOfNodes = true;
        if( Body.MineHasLever() )
        if( Util.Neighbor( G.Hero.Pos, Pos ) )
        {
            Body.EffectList[ 2 ].gameObject.SetActive( true );
            Body.EffectList[ 2 ].gameObject.transform.rotation = Quaternion.RotateTowards(
            Body.RopeRotationHelper.transform.rotation, qn, 999f );
        }
        Body.RopeOrigin.gameObject.SetActive( true );
        Body.RopeDestination.gameObject.SetActive( true );
    }

    public void UpdateSpikedBallChainSizes()
    {
        if( Body.ShackleDistance == -1 ) return;
        if( Body.RopeConnectSon == null ) return;                                                            // optimize: dont run always
        Body.RopeDestination.transform.position = Body.RopeConnectSon.Graphic.transform.position;
        Body.Rope.AutoCalculateAmountOfNodes = false;
        float dst = Vector2.Distance( Pos, Body.RopeConnectSon.Pos );
        Body.Rope.NonTensionedColor = Color.white;
        Body.Rope.AmountOfNodes = ( int ) ( Body.ShackleDistance * 2.5f );
        float dd = Util.Manhattan( Pos, Body.RopeConnectSon.Pos ) - 1;
        if( dd >= Body.ShackleDistance )
        {
            //Body.Rope.AmountOfNodes = ( int ) ( dst * 1.7f );
            Body.Rope.AutoCalculateAmountOfNodes = true;
            Body.Rope.NonTensionedColor = new Color32( 255, 144, 144, 255 );
            Body.Rope.RopeMaxSizeMultiplier = 1;
            Debug.Log("here");
        }
    }

    public bool LastMoveWasStill()
    {
        if( G.Hero.Control.LastMoveAction == EActionType.ROTATE_CCW ) return true;
        if( G.Hero.Control.LastMoveAction == EActionType.ROTATE_CW ) return true;
        if( G.Hero.Control.LastMoveAction == EActionType.BATTLE ) return true;
        if( G.Hero.Control.LastMoveAction == EActionType.WAIT ) return true;
        return false;
    }
    public void SetSpecialWasp( int type, Unit mother )
    {
        if( type == 1 )
        {
            Body.ShieldedWasp = true;
            Body.Sprite2.gameObject.SetActive( true );
            Control.ShieldedWaspCount++;
        }
        if( type == 2 )
        {
            Body.Sprite3.gameObject.SetActive( true );
            mother.Control.CocoonWaspCount++;
            Body.CocoonWasp = true;
        }
        if( type == 3 )
        {
            Body.EnragedWasp = true;
            mother.Control.EnragedWaspCount++;
            Body.Sprite4.gameObject.SetActive( true );
        }
    }
    public bool ProjType( EProjectileType type )
    {
        if( TileID == ETileType.PROJECTILE )
            if( type == Control.ProjectileType )
                return true;
        return false;
    }
}















