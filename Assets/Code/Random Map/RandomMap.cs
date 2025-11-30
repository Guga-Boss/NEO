using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using DarkTonic.MasterAudio;
using System.Text;
public class RandomMap : MonoBehaviour
{
    #region Variables
    public int [,] PuzzleArea;
    public int CurrentAdventure = -1;
    public int AreaPopulateTrialCount, PreferedWaypoint;
    public Vector2 MapCenter, LastDungeonHeroPos, LastLabHeroPos, LastGateTileStepped;
    public DungeonDialog DungeonDialog;
    public UIButton OKButton;
    public UIPopupList DungeonNumberPopup;
    public RandomMapData RMD, ORMD;
    public Vector2 LabCordOrigin, AreasTMOrigin;
    public Rect LabArea;
    public tk2dTileMap AreasTM;
    public Sector[,] RMSector;
    public List<RandomMapData> RMList;
    public Sector.ESectorType HeroSectorType;
    public Sector HeroSector, GateSector;
    public Vector2 HeroSectorPos;
    public int LastSectorJumpID, AreaCont, NumAreaGatesOpened, NumLabGatesOpened, NumGateKeyNeeded;
    public float JumpKeyTimer, AreaGateCost, LabGateCost, AutoGotoEnergyCost;
    public bool GameOver;
    public bool LockSectorJump, LastCubeReached, LockWayPointJump;
    public Artifact TempArtifact;
    public TextAsset AreaNamesTextFile;
    public int PuzzleCount;    
    public SectorDefinition SD;
    public List<Vector2> CreatedTilesPos, ToggleList, KeyListToDelete;
    public List<Unit> ModedUnitlList = new List<Unit>();
    public List<ELayerType> CreatedTilesLayer;
    public List<VI> Dl = new List<VI>(), Fl = new List<VI>();  // List of gate and forest sector Objects
    public bool FirstInitialization = true;
    public SectorHint[] SHList;
    public static bool bUpdateDynamicMonsterLeveling, bUpdateRandomResourceTimers;
    public static int RecentAreaClearedID = -1;
    public float BarricadeDestroyInflation, FirewoodNeededInflation;
    public int PhysicsTileCount = 0, StartingCube;
    public List<Vector2> LabStartPosList;
    public List<EDirection> LabStartDirList;
    public int SortedLabStartID, AvailableCubesForPlaying;
    public bool MoveToLabStart = false;

    #endregion
    public void Start()
    {
        DungeonDialog.gameObject.SetActive( false );
        Reset();
        CurrentAdventure = -1;

        if( Helper.I.ReleaseVersion == false )
            Map.I.RM.DungeonDialog.ChooseAdventure( Helper.I.StartingAdventure ); 
    }
    public void Reset()
    { 
        LastDungeonHeroPos = LastLabHeroPos = HeroSectorPos = LastGateTileStepped = new Vector2( -1, -1 );
        HeroSectorType = Sector.ESectorType.NONE;
        AutoGotoEnergyCost = 0;
        LastSectorJumpID = 0;
        JumpKeyTimer = 0;
        PuzzleCount = 0;
        LastCubeReached = false;
        NumAreaGatesOpened = NumLabGatesOpened = 0;
        GameOver = true;
        bUpdateDynamicMonsterLeveling = bUpdateRandomResourceTimers = false;
        RecentAreaClearedID = -1;
        BarricadeDestroyInflation = FirewoodNeededInflation = 0;
    }

    public bool UpdateIt()
    {
        if( Quest.CurrentLevel != -1 ) return false;
        if( Manager.I.GameType != EGameType.CUBES ) return false;
        if( GameOver ) return false;

        if ( UpdateDialogToggle() ) return false;

        UpdateCloverResort();                                                             // Resorts clover position after a certain time

        Sector.UpdateSectorChanging();                                                    // Update Sector changing

        DungeonDialog.UpdateObjectives( true );

        UpdateRandomMap();

        UpdateStatistics();

        UpdateAreasJump();

        UpdateCubeJump();

        if( GS.ForceLoad == false )
        if( Map.I.Hero.Body.Hp <= 0 )
        {
            DungeonDialog.gameObject.SetActive( true );          
        }

        UpdateInput();

        UpdateBarricadeStrenghtening();

        return DungeonDialog.gameObject.activeSelf;
    }

    //_____________________________________________________________________________________________________________________________________ CreateRandomMap

    public void StartCubeSession( bool play = true )
    {
        Manager.I.GameType = EGameType.CUBES;
        AreaDefinition.RM = this;
        Quest.CurrentLevel = -1;        
        Map.I.AreaID = new int[ Map.I.Tilemap.width, Map.I.Tilemap.height ];

        LockSectorJump = false;
        LockWayPointJump = false;
        if( play )
        {
            DungeonDialog.ShowEndgameStats = true;
        }
        Quest.I.Dungeon.Reset();
        Quest.I.CurLevel = Quest.I.Dungeon;
        Reset();
        Map.I.CurrentArea = -1;
        StartingCube = 1;
        SortedLabStartID = Random.Range( 0, 8 );
        MoveToLabStart = true;
        RandomMapGoal. NewRecordID = -1;
        GameOver = false;
        HeroData.I.StartGame();
        RMD.Copy( RMList[ ( int ) CurrentAdventure ] );
        RMD.Init();
        Map.I.StopAllLoopedSounds();
        DungeonDialog.InitObjectives();

        for( int i = 0; i < RMD.SDList.Length; i++ )
            RMD.SDList[ i ].Reset();

        AreasTM.gameObject.SetActive( false );
        UI.I.FarmUI.gameObject.SetActive( false );
        BluePrintWindow.I.gameObject.SetActive( false );
        
        tk2dTileMap tm = Quest.I.Dungeon.Tilemap;
        TKUtil.CreateBlankMap( ref tm );        
        Map.I.TransTilemapUpdateList = new List<VI>();
        
        MapCenter = new Vector2( ( int ) tm.width / 2, ( int ) tm.height / 2 );        

        PuzzleArea = new int[ tm.width, tm.height ];
        Map.I.AreaID = new int[ tm.width, tm.height ];

        if( FirstInitialization )                                                        // Attention:  List being created only one time for optimization, if you want o change gate size, refactor code comparing with a var: "old size"
        for( int y = 0; y < tm.width; y++ )                                              // Create forest walls
        for( int x = 0; x < tm.width; x++ )
            {
                Map.I.AreaID[ x, y ] = -1;
                PuzzleArea[ x, y ] = -1;
                if( Util.IsMultiple( x, Sector.TSX ) || Util.IsMultiple( y, Sector.TSY ) )
                {
                    Quest.I.CurLevel.Tilemap.SetTile( x, y, ( int ) ELayerType.GAIA, ( int ) ETileType.FOREST );
                    Fl.Add( new VI( x, y ) );
                }
            }
        
        Sector.CreateObjects();

        Sector.InitAll();

        InitSectorData();

        InitLab();

        //UpdateSectorHint();

        InitArtifacts( null );

        Manager.I.ExitLevel();

        Map.I.StartGame();

        RandomMapPostInitialization();
    }

    //_____________________________________________________________________________________________________________________________________ 

    public void InitSectorData()
    {
        for( int y = 0; y < Sector.NY; y++ )
        for( int x = 0; x < Sector.NX; x++ )
            {
                SortSectorHint( x, y );
                CreateSectorGates( x, y );
                RMSector[ x, y ].AreaList = new List<Area>();                
            }
    }  
    
    public void InitLab()
    {
        if( RMD.PreferedLabPosition.x == -1 )
        LabCordOrigin = new Vector2( ( int ) Random.Range( 0, Sector.NX-1 ), ( int ) Random.Range( 1, Sector.NY ) );
        else
            LabCordOrigin = RMD.PreferedLabPosition;

        RMSector[ ( int ) LabCordOrigin.x, ( int ) LabCordOrigin.y ].Type = Sector.ESectorType.LAB;                             // Assign lab to sector types
        RMSector[ ( int ) LabCordOrigin.x + 1, ( int ) LabCordOrigin.y ].Type = Sector.ESectorType.LAB;
        RMSector[ ( int ) LabCordOrigin.x, ( int ) LabCordOrigin.y - 1 ].Type = Sector.ESectorType.LAB;
        RMSector[ ( int ) LabCordOrigin.x + 1, ( int ) LabCordOrigin.y - 1 ].Type = Sector.ESectorType.LAB;

        RMSector[ ( int ) LabCordOrigin.x, ( int ) LabCordOrigin.y ].Discovered = true;                                        // Set Discovered
        RMSector[ ( int ) LabCordOrigin.x + 1, ( int ) LabCordOrigin.y ].Discovered = true;
        RMSector[ ( int ) LabCordOrigin.x, ( int ) LabCordOrigin.y - 1 ].Discovered = true;
        RMSector[ ( int ) LabCordOrigin.x + 1, ( int ) LabCordOrigin.y - 1 ].Discovered = true; 

        LabArea = new Rect( RMSector[ ( int ) LabCordOrigin.x, ( int ) LabCordOrigin.y ].Area.x,
                            RMSector[ ( int ) LabCordOrigin.x, ( int ) LabCordOrigin.y ].Area.yMin -
                              Sector.TSY, ( Sector.TSX * 2 ) - 1, ( Sector.TSY * 2 ) - 1 );

        Map.I.CopyTilemap( false, Quest.I.LabList[ Quest.CurrentDungeon ].Tilemap, ref Quest.I.Dungeon.Tilemap,               // Copy Lab Tiles
                           LabArea.position, new Vector2( 0, 0 ),             
                                  Quest.I.LabList[ Quest.CurrentDungeon ].Tilemap.width, 
                                  Quest.I.LabList[ Quest.CurrentDungeon ].Tilemap.height, false, false );

        //for( int y = ( int ) LabArea.yMin; y < LabArea.yMax; y++ )                                                            // Apply Lab modifications
        //for( int x = ( int ) LabArea.xMin; x < LabArea.xMax; x++ )
        //if ( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
        //{
        //    ApplyMod( x, y, RMD.LabModAction, RMD.LabModValue );
        //}  
    }    

    public void CreateGate( bool horiz, Vector2 pos, int size )
    {
        int tile = ( int ) ETileType.CLOSEDDOOR;
        int layer = ( int ) ELayerType.GAIA;

        for( int i = 0; i <= size; i++ )
        {
            if( horiz )
            {
                int x = ( int ) pos.x - ( size / 2 ) + i;
                int y = ( int ) pos.y;
                Quest.I.Dungeon.Tilemap.SetTile( x, y, layer, tile );
                Dl.Add( new VI( x, y ) );
                Fl.Remove( new VI( x, y ) );
            }
            if(!horiz )
            {
                int x = ( int ) pos.x;
                int y = ( int ) pos.y - ( size / 2 ) + i;
                Quest.I.Dungeon.Tilemap.SetTile( x, y, layer, tile );
                Dl.Add( new VI( x, y ) );
                Fl.Remove( new VI( x, y ) );
            }
        }
    }

    public void CreateSectorGates( int x, int y )
    {
        if( FirstInitialization == false ) return;                                                                                                         // Attention:  List being created only one time for optimization, if you want o change gate size, refactor code comparing with a var: "old size"
        float c = RMD.GateCreationChance;
        bool[] create = { Util.Chance( c ), Util.Chance( c ), Util.Chance( c ), Util.Chance( c ) };

        int mid = Sector.TSX / 2;
        int size = RMD.GateSize;
        if( x > 0 && create[ 0 ] )
            CreateGate( false, new Vector2( ( int ) RMSector[ x, y ].Area.center.x - mid, ( int ) RMSector[ x, y ].Area.center.y ), size );
        if( x < Sector.NX - 1 && create[ 1 ] )
            CreateGate( false, new Vector2( ( int ) RMSector[ x, y ].Area.center.x + mid, ( int ) RMSector[ x, y ].Area.center.y ), size );
        if( y > 0 && create[ 2 ] )
            CreateGate( true, new Vector2( ( int ) RMSector[ x, y ].Area.center.x, ( int ) RMSector[ x, y ].Area.center.y - mid ), size );
        if( y < Sector.NY - 1 && create[ 3 ] )
            CreateGate( true, new Vector2( ( int ) RMSector[ x, y ].Area.center.x, ( int ) RMSector[ x, y ].Area.center.y + mid ), size );
    }
                 
    public Rect GetValidArea()
    {
        for( int i = 0; i < 99; i++ )
        {
            Rect r = new Rect();
            for( int t = 0; t < 99999; t++ )
            {
                r = new Rect( ( int ) Random.Range( MapCenter.x - 30, MapCenter.x + 30 ),
                              ( int ) Random.Range( MapCenter.y - 30, MapCenter.y + 30 ), Random.Range( 6, 10 ), Random.Range( 6, 10 ) );

                Vector2 p1 = r.position;
                Vector2 p2 = new Vector2( r.position.x + r.size.x - 1, r.position.y - r.size.y + 1);                

                bool oktile = true;
                for( int y = ( int ) p2.y; y <= p1.y; y++ )
                for( int x = ( int ) p1.x; x <= p2.x; x++ )
                    {
                        //Debug.Log( x + " " + y );
                        if( Map.I.AreaID[ x, y ] != -1 ) oktile = false;

                        Rect shop = new Rect( ( int ) MapCenter.x - 10, ( int ) MapCenter.y + 10, 20, 20 );

                        if( shop.Contains( new Vector2( x, y ) ) ) oktile = false;
                    }
                if( oktile == false ) {} else break;
                if( t == 99998 ) Debug.Log("gg");
            }

            bool ok = true;
            for( int a = 0; a < Quest.I.Dungeon.AreaList.Count; a++ )
            {
                Area ar = Quest.I.Dungeon.AreaList[ a ];

                Rect r2 = Quest.I.Dungeon.AreaList[ a ].AreaRect;

                Vector2 p1 = r.position;
                Vector2 p2 = new Vector2( r.position.x + r.width - 1, r.position.y - r.height + 1 );              

               // if( AreaIntersect( p1, p2 ) ) ok = false;

                if( Map.I.AreaID[ ( int ) MapCenter.x, ( int ) MapCenter.y ] != -1 ) ok = false;
            }
            if( ok )
            {
                r = new Rect( r.x + 1, r.y - 1, r.width - 2, r.height - 2 );
                return r;
            }
        }
        //Debug.LogError("Cannot create area");
        return new Rect( -1, -1, -1, -1 );
    }
    
    public int SortBarricadeSize( bool inarea, int orsize )
    {
        int size = 0;
        bool extra = true;
        if( orsize == 9 ) extra = false;
        if( inarea ) extra = false;

        if( inarea )                                                                                      // InArea
        {
            if( extra == false )
            {
                if( RMD.BarricadeChanceHeight != null &&                                                  // Random map data
                    RMD.BarricadeChanceHeight.Count == 10 )
                    size = Util.Sort( RMD.BarricadeChanceHeight );
                else Debug.LogError( "Bad Barricade Size." );
            }
        }
        else
        {                                                                                                  // Out Area
            if( extra == false )
            {
                if( RMD.OutBarricadeChanceHeight != null &&
                    RMD.OutBarricadeChanceHeight.Count == 10 )
                    size = Util.Sort( RMD.OutBarricadeChanceHeight );
                else Debug.LogError( "Bad Barricade Size." );
            }
            else
            {
                if( RMD.OutExtraBarricadeSize != null &&                                                    // Extra barricade Size
                    RMD.OutExtraBarricadeSize.Count > 0 )
                {
                    int id = Random.Range( 0, RMD.OutExtraBarricadeSize.Count );
                    size = orsize + ( int ) RMD.OutExtraBarricadeSize[ id ];
                    size = Mathf.Clamp( size, 0, Map.TotBarricade );
                }
                else size = orsize;
            }
        }
        return size;
    }
    
    
    public bool CheckMap( Area area )
    {
        tk2dTileMap tm = Quest.I.Dungeon.Tilemap;
        MyPathfind.I.CreateMap( Map.I.Tilemap.width, Map.I.Tilemap.height );
        MyPathfind.I.SeedForestMap( area );

        for( int y =  ( int ) area.P2.y; y <= area.P1.y; y++ )
        for( int x =  ( int ) area.P1.x; x <= area.P2.x; x++ )
        for( int yy = ( int ) area.P2.y; yy <= area.P1.y; yy++ )
        for( int xx = ( int ) area.P1.x; xx <= area.P2.x; xx++ )
        if( x != xx && y != yy )
        {
            ETileType mn1 = ( ETileType ) tm.GetTile( x, y, ( int ) ELayerType.MONSTER );
            ETileType mn2 = ( ETileType ) tm.GetTile( xx, yy, ( int ) ELayerType.MONSTER );

            if( mn1 == ETileType.ROACH || mn1 == ETileType.SCARAB )
                if( mn2 == ETileType.ROACH || mn2 == ETileType.SCARAB )
                {
                    MyPathfind.I.GetPath( new Vector2( x, y ), new Vector2( xx, yy ) );

                    if( MyPathfind.I.Path == null )                                                                                           // Monsters stuck, recursive recreation of area
                    {
                        AreaPopulateTrialCount++;                        
                        return false;
                    }
                }
        }

        return true;
    }
    public void PlaceArtifactsFromList( Sector s )
    {
        if( SD == null ) return;

        for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )
        for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
        {
            ETileType ga2 = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( xx, yy, ( int ) ELayerType.GAIA2 );

            if( ga2 == ETileType.ARTIFACT )
            {
                SectorDefinition sd = RMD.SDList[ RMD.SDID ];
                string art = "";

                #region old
                //if( sd.ArtifactCreationMethod == EArtifactCreationMethod.BY_ORDER )
                //    if( typeListCopy != null && typeListCopy.Count > 0 )                                                                               // Ordered Artifact placement
                //    {
                //        id = 0;
                //        art = typeListCopy[ id ].ToString();
                //        typeListCopy.RemoveAt( 0 );
                //        if( costListCopy != null && costListCopy.Count > 0 )
                //            costListCopy.RemoveAt( 0 );
                //    }
                //    else
                //        Debug.LogError( "Ordered Artifact Creation failed. Not enough artifacts." );

                //if( sd.ArtifactCreationMethod == EArtifactCreationMethod.RANDOM_FREE )
                //    if( typeListCopy != null && typeListCopy.Count > 0 )                                                                               // Sorts Artifact freely from The List (may repeat)
                //    {
                //        id = Random.Range( 0, typeListCopy.Count );
                //        art = typeListCopy[ id ].ToString();
                //    }

                //if( sd.ArtifactCreationMethod == EArtifactCreationMethod.RANDOM_NO_REPEAT )
                //    if( typeListCopy != null && typeListCopy.Count > 0 )                                                                               // Sorts Artifact frrom the list without repeating
                //    {
                //        id = Random.Range( 0, typeListCopy.Count );
                //        art = typeListCopy[ id ].ToString();
                //        typeListCopy.RemoveAt( id );
                //        if( costListCopy != null && costListCopy.Count > 0 )
                //            costListCopy.RemoveAt( id );
                //    }
                //    else
                //        Debug.LogError( "Random NO repeat Artifact Creation failed. Not enough artifacts." );
                #endregion

                int modn = ( int ) Mod.GetModInTile( new Vector2( xx, yy ) );
                int idd = -1;

                if( modn != -1 )
                {
                    for( int i = 0; i < Map.I.RM.SD.ModList.Length; i++ )
                    {
                        Mod md = Map.I.RM.SD.ModList[ i ];
                        if( ( int ) md.ModNumber == modn )
                        {
                            idd = i; break;
                        }
                    }
                }
                if( idd < 0 ) Debug.LogError( "MOD artifact type not set." );
                art = Map.I.RM.SD.ModList[ idd ].ArtifactType.ToString();

                if( art != "" )
                {
                    GameObject res = ( GameObject ) Resources.Load( "Artifacts/" + art );
                    Artifact ar = Quest.I.CreateArtifact( res, Quest.I.CurLevel, new Vector2( xx, yy ), s,
                                                        -1, s.Number, Quest.I.Dungeon.ArtifactFolder, true );
                    if( res )
                    {
                        Mod md = Map.I.RM.SD.ModList[ idd ];
                        ar.Copy( res.GetComponent<Artifact>(), false );                                                                                // Error message if cost notset
                        
                        ar.CostValue_1 = md.ArtifactCost;
                        ar.LifeTime = md.ArtifactLifetime;
                        ar.TargetHero = md.TargetHero;
                        ar.Multiplier = md.ArtifactMultiplier;
                        ar.TargetUnitName = GetComponent<tk2dTextMesh>();
                        Unit un = Map.I.GetUnit( ETileType.DOME, ar.Pos );
                        if( un == null )
                        {
                            un = CreateDome( new Vector2( xx, yy ) );                                                                                       // Auto create dome if not created
                            //Map.I.Tilemap.SetTile( xx, yy, ( int ) ELayerType.MONSTER, ( int ) ETileType.DOME );
                        }
                        if( un ) un.UpdateDomePrice();
                    }
                    else Debug.Log( "Artifacts/" + art + " Could not be found." );
                }
                else
                {
                    Debug.LogError( "Bad RandomArtifactTypeList ID" );
                }
            }
        }
    }
    public static Unit CreateDome( Vector2 tg )
    {
        Unit pf = Map.I.GetUnitPrefab( ETileType.DOME );                                                                           // Auto create dome if not created
        GameObject go = Map.I.CreateUnit( pf, tg );
        if( go )
        {
            Unit un = go.GetComponent<Unit>();
            un.Activate( true );
            un.gameObject.SetActive( true );
            return un;
        }
        return null;
    }

    public void InitArtifacts( Sector s, bool lab = true )
    {
        Rect area = LabArea;
        if( !lab ) area = s.Area;

        if( lab )
            Quest.I.CreateArtifacts( Quest.I.LabList[ Quest.CurrentDungeon ].Tilemap, Quest.I.Dungeon,                                        // Create Artifacts
                                   -1, -2, Quest.I.Dungeon.ArtifactFolder, LabArea.position, area, null );

        //else
        //    Quest.I.CreateArtifacts( AreasTM, Quest.I.CurLevel,                                                                               // Create Artifacts
        //                            -1, s.Number, Quest.I.Dungeon.ArtifactFolder, s.Area.position, area, s, true );

        if( !lab ) PlaceArtifactsFromList( s );        
        
        for( int a = 0; a < Quest.I.Dungeon.ArtifactList.Count; a++ )
        {
            Artifact ar = Quest.I.Dungeon.ArtifactList[ a ];
            TKUtil.Settile( ( int ) ar.Pos.x, ( int ) ar.Pos.y, ETileType.ARTIFACT );
        }
         
        if ( lab )
        for( int m = 0; m < Map.TotMod; m++ )                                                                                                 // Shuffles artifact position
        {
            List<Artifact> al = new List<Artifact>();

            for( int a = 0; a < Quest.I.Dungeon.ArtifactList.Count; a++ )
            {
                Artifact ar = Quest.I.Dungeon.ArtifactList[ a ];
                Vector2 pt = ar.Pos;

                ETileType mod = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( ( int ) ar.Pos.x,
                                                 ( int ) ar.Pos.y, ( int ) ELayerType.MODIFIER );

                //if( RMD.LabModAction != null )
                //if( RMD.LabModAction.Length > 0 && RMD.LabModAction[ 0 ] == EModAction.SwapArtifacts && m == 0 && mod == ETileType.MOD1 ||    // Deletes MODs
                //    RMD.LabModAction.Length > 1 && RMD.LabModAction[ 1 ] == EModAction.SwapArtifacts && m == 1 && mod == ETileType.MOD2 ||
                //    RMD.LabModAction.Length > 2 && RMD.LabModAction[ 2 ] == EModAction.SwapArtifacts && m == 2 && mod == ETileType.MOD3 ||
                //    RMD.LabModAction.Length > 3 && RMD.LabModAction[ 3 ] == EModAction.SwapArtifacts && m == 3 && mod == ETileType.MOD4 )
                //{
                //    al.Add( ar );
                //    //Quest.I.Dungeon.Tilemap.SetTile( ( int ) ar.Pos.x,
                //    //                                 ( int ) ar.Pos.y, ( int ) ELayerType.MODIFIER, -1 );   // Foi removido, verificar se nao deu bug
                //}
            }

            for( int i = 0; i < al.Count; i++ )                                                                                              // swap artifacts
            {
                int id = Random.Range( 0, al.Count );
                Map.I.RM.TempArtifact.Copy( al[ i ], false );
                al[ i ].Copy( al[ id ], false );
                al[ id ].Copy( Map.I.RM.TempArtifact, false );
            }
        }

        for( int a = 0; a < Quest.I.Dungeon.ArtifactList.Count; a++ )
        {
            Artifact ar = Quest.I.Dungeon.ArtifactList[ a ];

            if( ar.TargetUnitName != null )
            {
                ar.TargetUnitName.gameObject.SetActive( false );                                                              // Omit artifact text

                if( ar.Multiplier != Artifact.EMultiplier.x1 )                                                                // Show only artifact multiplier             
                {
                    ar.TargetUnitName.gameObject.SetActive( true );
                    ar.TargetUnitName.text = "            " + ar.Multiplier;
                }
            }
        }
    }

    public void RandomMapPostInitialization()
    {
        for( int i = 0; i < Quest.I.Dungeon.AreaList.Count; i++ )
        {
            Area ar = Quest.I.Dungeon.AreaList[ i ];
            ar.Spr.color = new Color( 0, 0, 0, 1 );
            ar.InfoText.color = new Color( 0, 0, 0, 1 );
        }

        Map.I.InvalidateInputTimer = .5f;
        Map.I.Hero.Control.ForceMove = EActionType.WAIT;

        if( Helper.I.ReleaseVersion == false )       // Updates items max stack not to clamp them and waste them all
            G.Inventory.UpdateIt();

        //if( RMD.RevealLabFog ) // hidden tiles has been removed
        //for( int a = 0; a < Quest.I.Dungeon.ArtifactList.Count; a++ )
        //{
        //    Artifact ar = Quest.I.Dungeon.ArtifactList[ a ];
        //    Map.I.RevealTile( false, ar.Pos );
        //}

        int range = 2;
        if ( RMD.AutoPickupNeighborArtifacts )
        for( int y = ( int ) Map.I.Hero.Pos.y - range; y <= Map.I.Hero.Pos.y + range; y++ )                                      // Auto Pickup starting neighbors artifacts
        for( int x = ( int ) Map.I.Hero.Pos.x - range; x <= Map.I.Hero.Pos.x + range; x++ )
            {
                Quest.I.UpdateArtifactStepping( new Vector2( x, y ) );
            }

        UpdateBriarCreation( RMSector[ ( int ) LabCordOrigin.x,     ( int ) LabCordOrigin.y     ], true,  false, false, true  );    // briar creation
        UpdateBriarCreation( RMSector[ ( int ) LabCordOrigin.x + 1, ( int ) LabCordOrigin.y     ], true,  true,  false, false );
        UpdateBriarCreation( RMSector[ ( int ) LabCordOrigin.x,     ( int ) LabCordOrigin.y - 1 ], false, false, true,  true  );
        UpdateBriarCreation( RMSector[ ( int ) LabCordOrigin.x + 1, ( int ) LabCordOrigin.y - 1 ], false, true,  true,  false );
        
        RandomMapGoal.InitAlternateStartingCube();
    }

    public void ChooseDungeonNumber()
    {
        Quest.CurrentDungeon = UIPopupList.current.items.IndexOf( UIPopupList.current.value );
    }

    public void StartCubes()
    {
        DungeonDialog.gameObject.SetActive( false );
        StartCubeSession();
    }

    public void CreateSectorAreas( Sector s )
    {
        int id = Random.Range( 0, SD.SourcePos.Count );

        int ts = ( Sector.TSX - 1 );
        //Vector2 cord = SD.SourcePos[ id ] * ts;
        Vector2 cord = new Vector2( 0, 0 );      

        MapSaver.I.CurrentSector = s;
        MapSaver.I.Load();

        s.FlipX = false;                                                                     // Flip sector Stuff
        if( SD.FlipX == EFlipSector.RAND )
        if( Util.Chance( 50 ) ) s.FlipX = true;
        if( SD.FlipX == EFlipSector.FORCE_YES )
            s.FlipX = true;

        Util.ToBool( Helper.I.ForceFlipX, ref  s.FlipX );

        s.FlipY = false;
        if( SD.FlipY == EFlipSector.RAND )
        if( Util.Chance( 50 ) ) s.FlipY = true;
        if( SD.FlipY == EFlipSector.FORCE_YES )
            s.FlipY = true;
        Util.ToBool( Helper.I.ForceFlipY, ref  s.FlipY );

        Vector2 toorigin = s.Area.position;
        if( s.Type == Sector.ESectorType.NORMAL )
        {                                                                                                                             // Copy tilemap
            Map.I.CopyTilemap( true, AreasTM, ref Quest.I.Dungeon.Tilemap, toorigin, cord, ts, ts, s.FlipX, s.FlipY );

            Map.I.CopyTilemap( true, AreasTM, ref Map.I.Tilemap, toorigin, cord, ts, ts, s.FlipX, s.FlipY, true,                      // Copy only Terrain tilemap
            false, false, false, false, false, false, false, false );
            AreasTMOrigin = cord;
        }

        UpdatePuzzleCopy( s );
        Map.I.CreateAreas( Quest.I.Dungeon.Tilemap, Quest.CurrentDungeon, Quest.I.Dungeon, s.Area, ref s.AreaList, s, SD );          // Create Areas
        s.AreasCreated = true;

        if( Map.I.LevelStats.NormalSectorsDiscovered >= RMD.MinimumMarkedAreaSector )                                                // Marked areaSort
        for( int a = 0; a < s.AreaList.Count; a++ )
        {
            s.AreaList[ a ].RandomMapSector = new Vector2( s.X, s.Y );
            if( Util.Chance( RMD.MarkedAreaChance ) ) s.AreaList[ a ].MarkedArea = true;        
        }

        for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )
        for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
        if( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
            {
                ETileType mod = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( xx, yy, ( int ) ELayerType.MODIFIER );

                if( mod == ETileType.LEVEL_ENTRANCE )                                                                      // Set sector enter pos for each sector for jump
                {
                    s.RescuePos = new Vector2( xx, yy );
                    //Quest.I.Dungeon.Tilemap.SetTile( xx, yy, ( int ) ELayerType.MODIFIER, -1 );
                }

                ApplyMod( xx, yy, SD.ModAction, SD.ModValue );                                                             // Apply Lab mod
            }

        if( s.RescuePos == new Vector2( -1, -1 ) ) 
            Debug.LogError("Sector Entrance not set " );
    }

    public void UpdatePuzzleCopy( Sector s )
    {
        //for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )
        //for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
        //if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
        //if ( PuzzleArea[ xx, yy ] == -1 )
        //    {
        //        ETileType mod = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( xx, yy, ( int ) ELayerType.MODIFIER );

        //        if( mod == ETileType.PUZZLE )
        //        {
        //            Vector2 sw = new Vector2( xx, yy );
        //            Vector2 se = new Vector2( -1, -1 );
        //            Vector2 ne = new Vector2( -1, -1 );
        //            Vector2 nw = new Vector2( -1, -1 );
        //            Vector2 sw2 = new Vector2( -1, -1 );

        //            int right  = Map.GetTileLineSize( Quest.I.Dungeon.Tilemap, new Vector2( xx, yy ), new Vector2( 1, 0 ), ETileType.PUZZLE, ref se );
        //            int top    = Map.GetTileLineSize( Quest.I.Dungeon.Tilemap, se, new Vector2( 0, 1 ), ETileType.PUZZLE, ref ne );
        //            int left   = Map.GetTileLineSize( Quest.I.Dungeon.Tilemap, ne, new Vector2( -1, 0 ), ETileType.PUZZLE, ref nw );
        //            int bottom = Map.GetTileLineSize( Quest.I.Dungeon.Tilemap, nw, new Vector2( 0, -1 ), ETileType.PUZZLE, ref sw2 );

        //            Debug.Log( right + " " + top + " " + left + "  " + bottom + "  " + sw + " " + se + "  " + ne + "  " + nw + "  " + sw2 );

        //            if( right == top && left == bottom )
        //            if( sw == sw2 )
        //                {
        //                    for( int py = ( int ) sw.y; py <= nw.y; py++ )
        //                    for( int px = ( int ) sw.x; px <= se.x; px++ )
        //                    if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( px, py ) ) )                            
        //                         PuzzleArea[ px, py ] = PuzzleCount;                           

        //                    MapSaver.I.LoadPuzzle();
        //                    bool flipx = false;
        //                    if( Util.Chance( 50 ) ) flipx = true;
        //                    bool flipy = false;
        //                    if( Util.Chance( 50 ) ) flipy = true;

        //                    Map.I.CopyTilemap( true, AreasTM, ref Quest.I.Dungeon.Tilemap, sw, new Vector2( 0, 0 ), right, top, flipx, flipy );
        //                    PuzzleCount++;
        //                }                    
        //        }
        //    }
    }

    public void ApplyMod( int xx, int yy, EModAction[] ma, float[] mv )
    {
        for( int i = 0; i < ma.Length; i++ )
        {
            ETileType mod = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( xx, yy, ( int ) ELayerType.MODIFIER );
            bool del = false;
            ETileType modnum = ETileType.MOD1 + i;        // atencao pois mudou o sistema depois de mudar para 128 x128, use EModType mod = GetModInTile( pt );
            if( mod == modnum )
            {
                float chance = -1;
                if( ma[ i ] == EModAction.KillUnitIfChance ) 
                {
                    chance = mv[ i ];                                                                                       // kill unit by chance
                    del = true;
                }

                if( chance != -1 )
                    if( Util.Chance( chance ) )
                    {
                        Quest.I.Dungeon.Tilemap.SetTile( xx, yy, ( int ) ELayerType.GAIA, -1 );
                        Quest.I.Dungeon.Tilemap.SetTile( xx, yy, ( int ) ELayerType.GAIA2, -1 );
                    }

                ETileType g2 = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( xx, yy, ( int ) ELayerType.GAIA2 );           // Rotation

                int num = ( int ) mv[ i ];
                if( num > 4 ) num = 4;

                int[] dirlist = new int[] { 0 };

                if( num == 1 ) dirlist = new int[] { 0, -1, 1 };
                if( num == 2 ) dirlist = new int[] { 0, -1, 1, -2, 2 };
                if( num == 3 ) dirlist = new int[] { 0, -1, 1, -2, 2, -3, 3 };
                if( num == 4 ) dirlist = new int[] { 0, -1, 1, -2, 2, -3, 3, 4 };

                int fact = dirlist[ Random.Range( 0, dirlist.Length ) ];

                if( ma[ i ] == EModAction.RotateUnitByRandomValue )
                {
                    TKUtil.Settile( xx, yy, Map.I.RotateTile( ( int ) g2, fact ) );
                    del = true;
                }
                //if( del ) Quest.I.Dungeon.Tilemap.SetTile( xx, yy, ( int ) ELayerType.MODIFIER, -1 ); // Removido, ver se nao deu bug
            }
        }
    }

    public void CreateSector( Vector2 spos )
    {
        RMD.Copy( RMList[ ( int ) CurrentAdventure ] );                                                                // Initializes RMD (Copy values)
  
        Sector s = RMSector[ ( int ) spos.x, ( int ) spos.y ];
        SectorHint.S = s;
        Sector.CS = s;
        Water.ResetVars( s );
        Mine.ResetVars( s );
        KeyListToDelete = new List<Vector2>();
        ModedUnitlList = new List<Unit>();
        ToggleList = new List<Vector2>();
        s.Fly = new List<Unit>();
        s.HintSortSuccess = SectorHint.SortHintChance( s.HintTypeText );
        RMSector[ ( int ) spos.x, ( int ) spos.y ].Discovered = true;
        Map.I.PondID = null;
        Map.I.CustomSpeedRaftPos = new List<Vector2>();
        Map.I.CustomSpeedFogPos = new List<Vector2>();
        Map.I.RestPos = new List<Vector2>();
        Map.I.RestID = new List<int>();
        s.GridInitialized = false;
        Map.I.ForceUpdateTrans = true;
        //Map.I.ProcessedTransCount = 0;

        Map.I.LevelStats.SectorsDiscovered++;                                                                          // Updates Sector Discovered stats 
        if( s.Type == Sector.ESectorType.NORMAL )
        {
            Map.I.LevelStats.NormalSectorsDiscovered++;
            Map.I.LevelStats.AreasDiscovered += s.AreaList.Count;
            s.Number = Map.I.LevelStats.NormalSectorsDiscovered;

            if( Map.I.RM.RMD.EnableAlternateStartingCube )                                                            // Alternate Starting Cube
            {
                s.Number = StartingCube + ( Map.I.LevelStats.NormalSectorsDiscovered - 1 );
                if( s.Number == 1)
                Message.CreateMessage( ETileType.NONE, "Alternate starting cube enabled in this quest!\n" +
                "Choose the desired Starting Cube in the dialog´s page.", 
                G.Hero.Pos, Color.green, true, true, 15, 0, -1 );
            }

            if( Helper.I.StartingCube != -1 )                                                                         // pre defined starting cube
            {
                s.Number = Helper.I.StartingCube;
            }

            if( s.Number > RMD.MaxCubes )                                                                             // Bounds check
            {
                s.Number = RMD.MaxCubes;
            }
        }

        List<float> hlist = new List<float>();
        for( int i = 0; i < RMD.SDList.Length; i++ )                                                                   // Sort Sector Data
        {
            float height = RMD.SDList[ i ].Height;

            if( s.Number < RMD.SDList[ i ].MinSectorNumber ||
                s.Number > RMD.SDList[ i ].MaxSectorNumber )
                height = 0;

            hlist.Add( height );
        }

        if( hlist.Count <= 0 ) Debug.LogError("Not enough Sectors To Sort");

        int sort = Util.Sort( hlist );
        //if( Helper.I.StartingCube != -1 )
        //    sort = Helper.I.StartingCube - 1;
        SD = RMD.SDList[ sort ];
        RMD.SDList[ sort ].UseCount++;
        RMD.SDID = sort;
        bUpdateDynamicMonsterLeveling = true;

        PhysicsTileCount = 0;
        Mod.StartIt();  
       
        if( SD.Script != "" )                                                                                        // Process Custom RMD Script
        {
            RMD.ProcessScript( SD.Script ); // Warning: you neead a RMD for each cube to use custom scripts
        }           

        CreateSectorAreas( s );                                                                                      // Create Sector areas

        AreaDefinition.Initialize( s );                                                                              // Init Area Definition        

        s.TotNormalMonsters = 0;
        s.TotFlyingMonsters = 0;
        int herbcount = 0;
        int scorpioncount = 0;

        int[] ll = { 3, 4, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        //int bld = ( int ) BuildingType.Plant_1 + ll[ Random.Range( 0, ll.Length ) ];

        for( int y = ( int ) s.Area.yMin; y < s.Area.yMax; y++ )                                                    // Create sector objects 
        for( int x = ( int ) s.Area.xMin; x < s.Area.xMax; x++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( x, y ) ) )
            {
                Map.I.UpdateTileGameObjectCreation( x, y, ELayerType.RAFT );
                if( Map.I.Unit[ x, y ] != null )
                if( Map.I.Unit[ x, y ].Control.IsFlyingUnit )                                                       // Inits Rafts
                    {
                        if( Map.I.Unit[ x, y ].TileID == ETileType.RAFT )
                        Map.I.Droplets.followTargets.Add( Map.I.Unit[ x, y ].Graphic.transform );  

                        s.AddFlying( Map.I.Unit[ x, y ] );
                        Mod.ApplyMods( x, y );                                                                      // Apply Mod 
                        Map.I.Unit[ x, y ] = null;
                    }

                Map.I.UpdateTileGameObjectCreation( x, y, ELayerType.GAIA );
                Map.I.UpdateTileGameObjectCreation( x, y, ELayerType.GAIA2 );
                Map.I.UpdateTileGameObjectCreation( x, y, ELayerType.MONSTER );
            
                if( Map.I.Gaia2[ x, y ] != null )
                if( Map.I.Gaia2[ x, y ].TileID == ETileType.MUD_SPLASH )
                    {
                        s.AddFlying( Map.I.Gaia2[ x, y ] );
                        Map.I.Gaia2[ x, y ] = null;                                                                // These are gaia2 but becomes monster layer (flying)
                     }

                if( Map.I.Unit[ x, y ] != null )
                if( Map.I.Unit[ x, y ].TileID ==  ETileType.FISH            ||                                     // Inits Fish, herbs and Blocker
                    Map.I.Unit[ x, y ].TileID == ETileType.HERB             ||
                    Map.I.Unit[ x, y ].ProjType( EProjectileType.FIREBALL ) ||                                 
                    Map.I.Unit[ x, y ].TileID == ETileType.BLOCKER          ||                           
                    Map.I.Unit[ x, y ].TileID == ETileType.ALGAE            ||
                    Map.I.Unit[ x, y ].TileID == ETileType.SPIKES           ||
                    Map.I.Unit[ x, y ].TileID == ETileType.FISHING_POLE     ||
                    Map.I.Unit[ x, y ].TileID == ETileType.IRON_BALL        ||
                    Map.I.Unit[ x, y ].TileID == ETileType.BOUNCING_BALL    ||
                    Map.I.Unit[ x, y ].TileID == ETileType.TRAIL )  
                    {
                        ETileType tl = Map.I.Unit[ x, y ].TileID;
                        if( Map.I.Unit[ x, y ].TileID == ETileType.HERB ) 
                            herbcount++;
                        if( Map.I.Unit[ x, y ].TileID == ETileType.FISHING_POLE )
                            PhysicsTileCount++;

                        if( Map.I.Unit[ x, y ].TileID == ETileType.FISH )
                        if( Map.I.Unit[ x, y ].Body.IsFish )
                            Map.I.Droplets.followTargets.Add( Map.I.Unit[ x, y ].transform );  

                        s.AddFlying( Map.I.Unit[ x, y ] );
                        Mod.ApplyMods( x, y );

                        if( tl == ETileType.SPIKES )
                        if( Map.I.Unit[ x, y ].Md )
                        if( Map.I.Unit[ x, y ].Md.AddAlgaeToSpike )                                             // Adds algae to spikes according to the rules defined in the spike mod 
                        {
                            Sector old = G.HS; G.HS = s;                                                        // updates G.HS to avoid bug and then restores value
                            Unit b = Map.I.SpawnFlyingUnit( new Vector2( x, y ), 
                            ELayerType.MONSTER, ETileType.ALGAE, null );
                            Mod.ApplyMods( x, y );
                            G.HS = old;                           
                        }
                        Map.I.Unit[ x, y ] = null;
                    }
            }

        List<Vector2> mList = new List<Vector2>(); 
        for( int y = ( int ) s.Area.yMin; y < s.Area.yMax; y++ )                                                    // Create sector objects 
        for( int x = ( int ) s.Area.xMin; x < s.Area.xMax; x++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( x, y ) ) )
            {
                ETileType mod = ( ETileType )
                Quest.I.Dungeon.Tilemap.GetTile( x, y, ( int ) ELayerType.MODIFIER );
                //Map.I.TransTilemapUpdateList.Add( new Vector2( x, y ) ); neo opt

                if( Map.I.Unit[ x, y ] != null )
                    {
                        if( Util.Chance( 100 ) )
                        {
                            Map.I.Unit[ x, y ].Control.RealtimeSpeedFactor = 100;                                   // Sort Realtime Speed
                        }
                        if( Map.I.Unit[ x, y ].TileID == ETileType.ALTAR )
                            s.AltarCount++;
                    }

                if( Map.I.Unit[ x, y ] != null )
                if( Map.I.Unit[ x, y ].ValidMonster ||
                    Map.I.Unit[ x, y ].TileID == ETileType.MINE ||
                    Map.I.Unit[ x, y ].TileID == ETileType.VINES )
                    {
                        if( Map.I.Unit[ x, y ].ValidMonster )
                            Statistics.AddStats( Statistics.EStatsType.MONSTERSDISCOVERED, 1 );

                        if( Map.I.Unit[ x, y ].TileID == ETileType.ROACH )                                          // Fills Up list for monsters leveling up
                        {
                            mList.Add( new Vector2( x, y ) );
                        }

                        if( Map.I.Unit[ x, y ].TileID == ETileType.SCORPION )                                         
                            scorpioncount++;

                        if( Map.I.Unit[ x, y ].Control.IsFlyingUnit )                                               // Inits Dragon
                        {
                            s.AddFlying( Map.I.Unit[ x, y ] );
                            Map.I.Unit[ x, y ].Control.FlightSpeedFactor = 100;
                            Map.I.Unit[ x, y ].Control.FlightSpeedFactorTimer = 5;

                            if( Map.I.Unit[ x, y ].TileID == ETileType.WASP         ||                              // Inits Wasps
                                Map.I.Unit[ x, y ].TileID == ETileType.MOTHERWASP   )   
                            {
                                Map.I.Unit[ x, y ].Control.Resting = false;
                                Map.I.Unit[ x, y ].Control.JumpTarget = new Vector2( x, y );
                            }

                            Mod.ApplyMods( x, y );                                                                  // Apply Mod    

                            if( Map.I.Unit[ x, y ].Control.Resting )
                            if( Map.I.Unit[ x, y ].Control.WakeUpGroup >= 0 )
                            {
                                Map.I.RestPos.Add( new Vector2( x, y ) );
                                Map.I.RestID.Add( Map.I.Unit[ x, y ].Control.WakeUpGroup );
                            }
                            Map.I.Unit[ x, y ] = null;  
                        }
                    }

                if( Map.I.Gaia[ x, y ] != null )
                {
                    if( Map.I.Gaia[ x, y ].TileID == ETileType.SNOW ||
                        Map.I.Gaia[ x, y ].TileID == ETileType.SAND )
                        PhysicsTileCount++;
                }
                                                                                     // Counts Physics tiles

                if( Map.I.Gaia2[ x, y ] != null )
                {
                    if( Map.I.Gaia2[ x, y ].TileID == ETileType.CAMERA )
                    {
                        int ori = ( int ) Quest.I.Dungeon.Tilemap.GetTile(                                          // camera obj hidden
                         x, y, ( int ) ELayerType.AREAS );
                        if( ( int ) mod == -1 && ori == -1 )
                            Map.I.Gaia2[ x, y ].gameObject.SetActive( true );
                        else
                            Map.I.Gaia2[ x, y ].gameObject.SetActive( false );
                    }
                }
            }

        InitArtifacts( s, false );

        //Map.I.RevealTile( true, s.Area.position );    

        if( SD.ExtraLevelPerUniqueMonster != null )                                                                             // Monster Extra levels
        if( SD.ExtraLevelPerUniqueMonster.Count > 0 )
        {
            Util.RandomizeList( ref mList );

            List<float> xtra = new List<float>();
            xtra.AddRange( SD.ExtraLevelPerUniqueMonster );
            
            for( int i = 0; i < mList.Count; i++ )
            {
                Unit un = Map.I.Unit[ ( int ) mList[ i ].x, ( int ) mList[ i ].y ];

                int id = Random.Range( 0, xtra.Count );
                un.Body.Level += xtra[ id ];
                un.UpdateLevelingData();
                xtra.RemoveAt( id );
                if( xtra.Count <= 0 ) break;
            }
        }

        UpdateSectorHint();

        Quest.I.UpdateArtifactData( ref Map.I.Hero );

        s.CheckpointList = new List<Vector2>();
        for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )
        for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
        {
            UpdateColliderCreation( new Vector2( xx, yy ) );                                                                 // Create colliders

            ETileType tile = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile(                                                  // Pre defined burning objects
                                      xx, yy, ( int ) ELayerType.MODIFIER );

            if( Map.I.Gaia2[ xx, yy ] )                                                                                      // Savegame List for jumping
            if( Map.I.Gaia2[ xx, yy ].TileID == ETileType.CHECKPOINT )
                s.CheckpointList.Add( Map.I.Gaia2[ xx, yy ].Pos );

            CopyDecorTiles( xx, yy );

            if( SD.AutoPickupArtifacts )                                                                                    // Auto Pickup Artifacts
                Quest.I.UpdateArtifactStepping( new Vector2( xx, yy ) );
            
            Mod.ApplyMods( xx, yy );                                                                                        // Apply Mod   

            if( Sector.GetPosSectorType( new Vector2( xx, yy ) ) == Sector.ESectorType.GATES )
                Map.I.Tilemap.SetTile( xx, yy, ( int ) ELayerType.TERRAIN, 1536 );                                         // creates grass tile behind gate tiles 

            if( Map.I.Unit[ xx, yy ] )
            if( Map.I.Unit[ xx, yy ].ValidMonster )
            if( Map.I.Unit[ xx, yy ].Body.OptionalMonster == false )                                                       // Calculates total monsters in cube
                {
                    if( Map.I.Unit[ xx, yy ].Control.IsFlyingUnit == false )
                        s.TotNormalMonsters++;                   
                }

            if( Map.I.Gaia2[ xx, yy ] )                                                                                   // Add chest extra bonuses
            if( Map.I.Gaia2[ xx, yy ].TileID == ETileType.ITEM )
            {
                Unit it = Map.I.Gaia2[ xx, yy ];
                if( it.Body.IsChest() )
                    Chests.Init( it );               
            }

            if( Map.I.Unit[ xx, yy ] )
            {
                if( Map.I.Unit[ xx, yy ].Control.Resting )                                                               // add resting lines
                if( Map.I.Unit[ xx, yy ].Control.WakeUpGroup >= 0 )
                {
                  Map.I.RestPos.Add( new Vector2( xx, yy ) );
                  Map.I.RestID.Add( Map.I.Unit[ xx, yy ].Control.WakeUpGroup );
                }
            }

            if( Map.I.Gaia2[ xx, yy ] )                                                                                   // Destroys key based mod key object
            if( Map.I.Unit[ xx, yy ]  ||                                               
                Map.I.FUnit[ xx, yy ] != null )
            {
                ETileType sp = Map.GetTileID( Map.I.Gaia2[ xx, yy ].TileID );
                if( sp == ETileType.DOOR_OPENER || sp == ETileType.DOOR_SWITCHER ||
                    sp == ETileType.DOOR_CLOSER || sp == ETileType.DOOR_KNOB )
                if( Map.I.GetUnit( ETileType.ORB, new Vector2( xx, yy ) ) == null )
                    Map.I.Gaia2[ xx, yy ].Kill();
            }

            Map.I.AddTrans( new VI( xx, yy ) );                                                                          // add trans tile to be updated                                                              
            Map.I.ClearTransTile( xx, yy, 0 );                                                                           // clear back trans tiles
            Map.I.ClearTransTile( xx, yy, 1 );                                                                           // optimize apenas gaia tiles (nao sei se da, optimize no GS.LOad tambem)
            Map.I.ClearTransTile( xx, yy, 2 );
            
            ETileType ga = ( ETileType ) Map.I.Tilemap.GetTile( xx, yy, 1 );                                             // clears gaia Back tiles so only traps will be shown
            if( ga == ETileType.WATER     || ga == ETileType.FOREST ||
                ga == ETileType.MUD       || ga == ETileType.CLOSEDDOOR ||
                ga == ETileType.OPENDOOR  || ga == ETileType.ROOMDOOR ||
                ga == ETileType.BLACKGATE || ga == ETileType.PIT ||
                ga == ETileType.SNOW      || ga == ETileType.SAND )
                Map.I.Tilemap.SetTile( xx, yy, ( int ) ELayerType.GAIA, -1 ); 
        }

        for( int i = 0; i < KeyListToDelete.Count; i++ )
        if ( Map.I.Gaia2[ ( int ) KeyListToDelete[ i ].x, ( int ) KeyListToDelete[ i ].y ] )
             Map.I.Gaia2[ ( int ) KeyListToDelete[ i ].x, ( int ) KeyListToDelete[ i ].y ].Kill();                       // kill these keys since they were used just for ori 5 purpose
        KeyListToDelete = new List<Vector2>();

        RestLine.Create();                                                                                              // Create Resting Lines     

        for( int i = 0; i < s.Fly.Count; i++ )
        {            
            if( s.Fly[ i ].ValidMonster )                                                                               // Calculates total flying monsters in cube
            if( s.Fly[ i ].Body.OptionalMonster == false )
                s.TotFlyingMonsters++;

            if( s.Fly[ i ].TileID == ETileType.VINES )                                                                 // default vine type
            {
            if( s.Fly[ i ].Control.VineList.Count == 0 )
                s.Fly[ i ].Control.VineList.Add( 0 );
            s.Fly[ i ].MeleeAttack.SpeedTimeCounter = -1;
            }
        }

        for(int i = 0; i < ToggleList.Count; i++)
        {
            Map.I.Tilemap.SetTile( ( int ) ToggleList[ i ].x, ( int ) ToggleList[ i ].y, ( int ) ELayerType.DECOR, -1 );                    // del bool togle tile decor 1  
            Map.I.Tilemap.SetTile( ( int ) ToggleList[ i ].x, ( int ) ToggleList[ i ].y, ( int ) ELayerType.DECOR2, -1 );                   // del bool togle tile decor 2  
        }

        s.SteppingMode = RMD.SteppingMode;                                         // Stepping mode as Default?
        if( scorpioncount > 0 && RMD.AutoScorpionSteppingMode )
            s.SteppingMode = true;                                                 // Scorpion present: stepping mode

        Map.I.SetMovementMode( s.SteppingMode );

        ModedUnitlList = new List<Unit>();
        Quest.I.Dungeon.Tilemap.gameObject.SetActive( false );

        Mod.PostModInitialization( s );                       

        Map.I.UpdateRoomDoorID();

        UpdateBriarCreation( s );

        CreateCubeLightningEffect( s, "Create Sector" );

        Map.I.UpdateGateAndRaftID( s );

        if( herbcount > 0 )
            Map.I.ShuffleAllHerbs( s );

        FirstInitialization = false; 

        Map.I.Tilemap.Build();

        Debug.Log( "Sector Created: "+ SD.name + " " + "Flip X " + s.FlipX + " Flip Y " + s.FlipY );        
    }

    public void CreateCubeLightningEffect( Sector s, string type )
    {
        //Map.I.CreateLightiningEffect( new Vector2( s.Area.xMin, s.Area.yMax ), new Vector2( s.Area.xMax, s.Area.yMax ), type );
        //Map.I.CreateLightiningEffect( new Vector2( s.Area.xMin, s.Area.yMin ), new Vector2( s.Area.xMax, s.Area.yMin ), type );
        //Map.I.CreateLightiningEffect( new Vector2( s.Area.xMax, s.Area.yMax ), new Vector2( s.Area.xMax, s.Area.yMin ), type );
        //Map.I.CreateLightiningEffect( new Vector2( s.Area.xMin, s.Area.yMax ), new Vector2( s.Area.xMin, s.Area.yMin ), type );
    }
    public void UpdateColliderCreation( Vector2 tg )
    {
        Unit un = Map.I.GetUnit( tg, ELayerType.MONSTER );
        bool createnew = false;
        //if( PhysicsTileCount <= 0 )                                                   // use graphics quality low on this one
        //{
        //    if( un && un.Collider)                                                    // deactivates monster collider
        //        un.Collider.enabled = false;
        //    return;
        //}
        bool active = true;
        if( un && un.Collider )                                                       // activates monster collider
            un.Collider.enabled = true;

        if( un && un.Collider )                                                       // new: no collider for monsters, kill hero if in the same tile
        if( un.ValidMonster )
        {
            un.Collider.enabled = false;
            return;
        }

        string col = "";

        if( un == null ) un = Map.I.GetUnit( tg, ELayerType.GAIA );

        if( un )
        {
            if( un.TileID == ETileType.FOREST )
            {
                createnew = true;
                col = "Forest Collider";
            }
            else
            if( un.TileID == ETileType.CLOSEDDOOR )
            {
                col = "Forest Collider";
            }
            else
            if( un.TileID == ETileType.OPENDOOR )
            {
                active = false;
            }
        }        

        if( col != "" )
        {
            un.SpawnedCollider = false;
            if( createnew )
            {
                Transform tr = PoolManager.Pools[ "Pool" ].Spawn( col );
                tr.position = tg;
                un.Collider = tr.gameObject.GetComponent<BoxCollider2D>();
                un.SpawnedCollider = true;
            }
        }
        if( un && un.Collider )
            un.Collider.enabled = active;
    }
    
    public void CopyDecorTiles( int x, int y )
    {
        ETileType tl = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( x, y, ( int ) ELayerType.DECOR );                  // copies decor tiles 
        Map.I.Tilemap.SetTile( x, y, ( int ) ( int ) ELayerType.DECOR, ( int ) tl );
        tl = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( x, y, ( int ) ELayerType.DECOR2 );
        Map.I.Tilemap.SetTile( x, y, ( int ) ( int ) ELayerType.DECOR2, ( int ) tl );

        tl = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( x, y, ( int ) ELayerType.GAIA );                             // copy decor gaia
        if( tl >= 0 && ( int ) tl < Map.I.Tilemap.data.tileInfo.Length )
        {
            var tileInfo = Map.I.Tilemap.data.tileInfo[ ( int ) tl ];
            if( tileInfo.Layer == ELayerType.GAIA )
            {
                Map.I.Tilemap.SetTile( x, y, ( int ) ( int ) ELayerType.GAIA, ( int ) tl );
                Map.I.UpdateTileGameObjectCreation( x, y, ELayerType.GAIA, ETileType.DECOR_GAIA );
            }
        }
    }

    public static bool CanCreateUnit( Vector2 tg, ELayerType layer, ETileType type )
    {
        if( Map.I.AreaID[ ( int ) tg.x, ( int ) tg.y ] < 0 ) return false;
        if( Map.I.RM.PuzzleArea[ ( int ) tg.x, ( int ) tg.y ] != -1 ) return false;

        ETileType gaia = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( ( int ) tg.x, ( int ) tg.y, ( int ) ELayerType.GAIA );
        ETileType gaia2 = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( ( int ) tg.x, ( int ) tg.y, ( int ) ELayerType.GAIA2 );
        ETileType monster = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( ( int ) tg.x, ( int ) tg.y, ( int ) ELayerType.MONSTER );
        ETileType mod = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( ( int ) tg.x, ( int ) tg.y, ( int ) ELayerType.MODIFIER );

        if( mod == ETileType.LEVEL_ENTRANCE ) return false;

        monster = Map.GetTileID( monster );

        if( layer == ELayerType.GAIA )
        {
            if( monster != ETileType.NONE ) return false;
            if( gaia != ETileType.NONE )
                if( gaia != ETileType.SNOW ) return false;
            if( gaia2 != ETileType.NONE ) return false;
        }
        else
        if( layer == ELayerType.MONSTER )
        {
            if( monster != ETileType.NONE ) return false;
            if( gaia != ETileType.NONE )
                if( gaia != ETileType.MUD ) return false;
            if( gaia2 != ETileType.NONE ) return false;
        }
        else
        if( layer == ELayerType.GAIA2 )
            {
                if( monster == ETileType.ROACH ||
                    monster == ETileType.SCARAB )
                    if( type == ETileType.ARROW )   // Monster over arrow and pusher
                        return true;

                if( monster == ETileType.BARRICADE )
                    if( type == ETileType.FIRE )
                        return true;

                if( gaia != ETileType.NONE )
                    if( gaia != ETileType.MUD ) return false;
                if( gaia2 != ETileType.NONE ) return false;
                if( monster != ETileType.NONE ) return false;
            }
        return true;
    }
    public void CopyRect( int x, int y, int tile, Vector2 sz )
    {
        for( int yy = 0; yy < sz.y; yy++ )
            for( int xx = 0; xx < sz.x; xx++ )
            {
                Vector2 tg = new Vector2( x, y ) + new Vector2( xx, -yy );
                int tl = tile;
                tl += xx;
                tl += ( ( yy ) * 128 );
                if( Map.PtOnMap( Map.I.Tilemap, tg ) )
                {
                    Map.I.Tilemap.SetTile( ( int ) tg.x, ( int ) tg.y, ( int ) ELayerType.DECOR, ( int ) tl );
                    Quest.I.Dungeon.Tilemap.SetTile( ( int ) tg.x, ( int ) tg.y, ( int ) ELayerType.DECOR, ( int ) tl );
                }
            }
    }

    public void CreateBriar( Vector2 tg, Sector s, bool vert = false )
    {
        Vector2 sz = new Vector2( 3, 4 );
        int or = 2821;
        if( vert )
        {
            sz = new Vector2( 4, 3 );
            or = 3333;
        }

        tg += new Vector2( s.Area.xMin, s.Area.yMin );
        CopyRect( ( int ) tg.x, ( int ) tg.y, or, sz );
    }

     public void UpdateBriarCreation( Sector s, bool top = true, bool right = true, bool bottom = true, bool left = true )
    {
        if( s.Type == Sector.ESectorType.NORMAL )
        {
            if( s.Y + 1 < Sector.NY && RMSector[ s.X, s.Y + 1 ].Discovered ) top    = false;     // top already created
            if( s.Y - 1 >= 0        && RMSector[ s.X, s.Y - 1 ].Discovered ) bottom = false;     // bottom already created
            if( s.X - 1 >= 0        && RMSector[ s.X -1,  s.Y ].Discovered ) left   = false;     // left already created
            if( s.X + 1 < Sector.NX && RMSector[ s.X + 1, s.Y ].Discovered ) right  = false;     // right already created                 
        }

        if( right )
        {
            CreateBriar( new Vector2( 28, 28 ), s );            // right
            CreateBriar( new Vector2( 28, 24 ), s );
            CreateBriar( new Vector2( 28, 20 ), s );

            CreateBriar( new Vector2( 28, 11 ), s );
            CreateBriar( new Vector2( 28, 7 ), s );
            CreateBriar( new Vector2( 28, 3 ), s );
        }
        if( left )
        {
            CreateBriar( new Vector2( -2, 28 ), s );           // left
            CreateBriar( new Vector2( -2, 24 ), s );
            CreateBriar( new Vector2( -2, 20 ), s );

            CreateBriar( new Vector2( -2, 11 ), s );
            CreateBriar( new Vector2( -2, 7 ), s );
            CreateBriar( new Vector2( -2, 3 ), s );
        }
        if( bottom )
        {
            CreateBriar( new Vector2( 0, 0 ), s, true );       // bottom
            CreateBriar( new Vector2( 4, 0 ), s, true );
            CreateBriar( new Vector2( 8, 0 ), s, true );

            CreateBriar( new Vector2( 17, 0 ), s, true );
            CreateBriar( new Vector2( 21, 0 ), s, true );
            CreateBriar( new Vector2( 25, 0 ), s, true );
        }
        if( top )
        {
            CreateBriar( new Vector2( 0, 30 ), s, true );      // top
            CreateBriar( new Vector2( 4, 30 ), s, true );
            CreateBriar( new Vector2( 8, 30 ), s, true );

            CreateBriar( new Vector2( 17, 30 ), s, true );
            CreateBriar( new Vector2( 21, 30 ), s, true );
            CreateBriar( new Vector2( 25, 30 ), s, true );
        }

        //Debug.Log( "Top: " + top + "   Bottom: " + bottom + "   Left: " + left + "    Right: " + right );
    }

    public void UpdateSectorCreation( Vector2 gpos )
    {
        Sector s = null;
        for( int d = 0; d < 8; d++ )
        {
            Vector2 sec = gpos + Manager.I.U.DirCord[ ( int ) d ];
            s = Sector.GetPosSector( sec );
            if( s != null && s.Type == Sector.ESectorType.NORMAL )
                if( s.AreasCreated == false )
                {
                    CreateSector( new Vector2( s.X, s.Y ) );
                }
        }
    }

    public void UpdateGatesCost( Vector2 gateOpenedPos )
    {
        if( Quest.CurrentLevel != -1 ) return;

        if( gateOpenedPos != new Vector2( -1, -1 ) )
        {
            UpdateSectorCreation( gateOpenedPos );
            Sector.ESectorType tp = Sector.GetPosSectorType( new Vector2( ( int ) gateOpenedPos.x, ( int ) gateOpenedPos.y ) );
            if( tp == Sector.ESectorType.GATES ) NumAreaGatesOpened++;
            else
            if( tp == Sector.ESectorType.LAB )   NumLabGatesOpened++;
        }

        AreaGateCost = RMD.InitialAreaGateCost + ( NumAreaGatesOpened * RMD.ExtraAreaGate );
        if( AreaGateCost > RMD.MaxAreaGateCost ) AreaGateCost = RMD.MaxAreaGateCost;
        LabGateCost = RMD.InitialLabGateCost;
    }

    public void SortSectorHint( int x, int y )
    {
        return;  // Added to avoid bugs. remove it if you want to use it.
        RMD.SectorHintFolder = RMList[ ( int ) CurrentAdventure ].SectorHintFolder;
        SHList = RMD.SectorHintFolder.gameObject.GetComponentsInChildren<SectorHint>();
        if( SHList.Length <= 0 ) return;

        List<float> hlist = new List<float>();
        for( int i = 0; i < SHList.Length; i++ )                                                                       // Sort Sector Hint Data
        {
            float height = SHList[ i ].Height;

            if( Map.I.LevelStats.NormalSectorsDiscovered < SHList[ i ].MinSectorNumber ||
                Map.I.LevelStats.NormalSectorsDiscovered > SHList[ i ].MaxSectorNumber )
                height = 0;
            hlist.Add( height );
        }
        
        int sort = Util.Sort( hlist );

        RMSector[ x, y ].HintTypeText = SHList[ sort ].name;

        SHList[ sort ].UseCount++;
        int strenght = Random.Range( 0, SHList[ sort ].StrenghtList.Length );
        RMSector[ x, y ].HintStrenght = SHList[ sort ].StrenghtList[ strenght ];

        RMSector[ x, y ].SectorHint = SHList[ sort ];

        int oper = Random.Range( 0, SHList[ sort ].OperationList.Length );
        RMSector[ x, y ].HintOperation = SHList[ sort ].OperationList[ oper ];

        StringBuilder builder = new StringBuilder( RMSector[ x, y ].HintText.text );
        for( int i = 0; i < builder.Length; i++ )         
            if( Random.Range( 0, 2 ) == 0 )
                if( builder[ i ] != ' ' )
                builder[ i ] = '-';

        RMSector[ x, y ].ShowHintFactor = Random.Range( 0, 100 );
         
        RMSector[ x, y ].HintText.text = builder.ToString();
        RMSector[ x, y ].HintDescription = SHList[ sort ].HintDescription;

        RMSector[ x, y ].AllHintText = RMSector[ x, y ].HintOperation + " " + 
        RMSector[ x, y ].HintStrenght.ToString() + " " + RMSector[ x, y ].HintTypeText;

        string str = RMSector[ x, y ].HintOperation.ToString();
        RMSector[ x, y ].HintOperatorFactor = new float[ str.Length ];                                                // Hint HintOperatorFactor: each letter has a range from 0 to 100
        for( int i = 0; i < str.Length; i++ )
            RMSector[ x, y ].HintOperatorFactor[ i ] = Random.Range( 0, 100 );

        RMSector[ x, y ].HintFactor = new float[ RMSector[ x, y ].HintTypeText.Length ];                              // Hint factor: Each letter has a range from 0 to 100
        for( int i = 0; i < RMSector[ x, y ].HintTypeText.Length; i++ )
            RMSector[ x, y ].HintFactor[ i ] = Random.Range( 0, 100 );

        str = RMSector[ x, y ].HintStrenght.ToString();
        RMSector[ x, y ].HintStrenghtFactor = new float[ str.Length ];                                                // Hint HintStrenghtFactor: each letter has a range from 0 to 100
        for( int i = 0; i < str.Length; i++ )
            RMSector[ x, y ].HintStrenghtFactor[ i ] = Random.Range( 0, 100 );
    }

    public void UpdateSectorHint()
    {
        if( Quest.CurrentLevel != -1 ) return;
        if( SHList.Length <= 0 ) return;
        int lev = ( int ) Map.I.Hero.Body.PsychicLevel;

        float showf = HeroData.I.PsychicShowHintFactor[ lev ];
        float lfactor = HeroData.I.PsychicShowLetterFactor[ lev ]; 

        bool[,] show = new bool[ Sector.NX, Sector.NY ];

        for( int y = 0; y < Sector.NY; y++ )
        for( int x = 0; x < Sector.NX; x++ )
            {
                string oper = RMSector[ x, y ].HintOperation.ToString();                                                                 // sort operator letters
                StringBuilder obuilder = new StringBuilder( oper );
                for( int i = 0; i < obuilder.Length; i++ )
                    if( RMSector[ x, y ].HintOperatorFactor[ i ] > lfactor ) obuilder[ i ] = '-';

                string hint = RMSector[ x, y ].HintTypeText;                                                                              // sort hint letters
                StringBuilder hbuilder = new StringBuilder( hint );
                for( int i = 0; i < hbuilder.Length; i++ )
                if ( RMSector[ x, y ].HintFactor[ i ] > lfactor ) hbuilder[ i ] = '-';

                string strenght = RMSector[ x, y ].HintStrenght.ToString();                                                               // sort strenght letters
                StringBuilder sbuilder = new StringBuilder( strenght );
                for( int i = 0; i < sbuilder.Length; i++ )
                if ( RMSector[ x, y ].HintStrenghtFactor[ i ] > lfactor ) sbuilder[ i ] = '-';

                RMSector[ x, y ].HintText.text = obuilder.ToString() +"\n" + sbuilder.ToString() + "\n" + hbuilder.ToString();

                RMSector[ x, y ].HintText.gameObject.SetActive( false );

                if( RMSector[ x, y ].ShowHintFactor < showf ) show[ x, y ] = true;
                if( RMSector[ x, y ].Type != Sector.ESectorType.NORMAL ) show[ x, y ] = false;
            }

        for( int y = 0; y < Sector.NY; y++ )
        for( int x = 0; x < Sector.NX; x++ )
            {
                int cont = 0;
                for( int d = 0; d < 8; d += 2 )
                {
                    Vector2 aux = new Vector2( x, y ) + Manager.I.U.DirCord[ ( int ) d ];
                    if( aux.x >= 0 && aux.x < 8 )
                    if( aux.y >= 0 && aux.y < 8 )
                        {
                            if( RMSector[ ( int ) aux.x, ( int ) aux.y ].Discovered ) cont++;
                        }
                }
                if( cont == 0 ) show[ x, y ] = false;
            }

        for( int y = 0; y < Sector.NY; y++ )
        for( int x = 0; x < Sector.NX; x++ )
            {
                if( show[ x, y ] )
                    RMSector[ x, y ].HintText.gameObject.SetActive( true );
                else
                    RMSector[ x, y ].HintText.gameObject.SetActive( false );
            }
    }

    public void UpdateAreasJump()
    {     
        if( RMD.QuickTravelEnabled == false ) return;
        if( HeroSectorType != Sector.ESectorType.NORMAL ) return;
        if( Map.I.CurrentArea != -1 ) return;
        if( G.Hero.Control.PathFinding.Path == null ) return;
        if( G.Hero.Control.PathFinding.Path.Count > 0 ) return;
        if( Map.I.FishingMode != EFishingPhase.NO_FISHING ) return;
        if( Map.I.FreeCamMode ) return;
        List<Vector2> poslist = HeroSector.CheckpointList;  
        if( Map.I.GetUnit( ETileType.WATER, G.Hero.Pos ) != null ) return;
        if( poslist.Count < 1 ) return;
        if( LockWayPointJump ) return;
        bool auto = false;

        if( Input.GetKeyDown( KeyCode.Backslash ) == false )
        {
            auto = Map.I.RM.DungeonDialog.AutoOpenGateCheck();
            if( auto == false )
            if( Helper.I.AutoWayPointJump == false ) return;
            if( Map.I.AutoOpenGateStep >= 6 ) return;
            if( Map.I.AutoOpenGateStep == 4 ) 
            {                 
                Map.I.AutoOpenGateStep++; 
            } 
        }

        float check = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.GOTO_CHECKPOINT );                        // Tech
        if( Helper.I.ReleaseVersion )
        if( check < 1 )
            {
                Message.RedMessage( "Unlock Find Puzzle Tech First in Studies Panel!\nOnly work in some cubes." );
                return;
            }

        if( auto == false ) HeroSector.LastAreaJumpID++;
        if( HeroSector.LastAreaJumpID >= poslist.Count )
            HeroSector.LastAreaJumpID = 0;

        if( Input.GetKeyDown( KeyCode.Backslash ) == true )
            PreferedWaypoint = HeroSector.LastAreaJumpID;
        else
        {
            if( auto )                                                                                              // prefered waypoint set by backslash to use with auto gate open
            if( PreferedWaypoint < poslist.Count )
                HeroSector.LastAreaJumpID = PreferedWaypoint;
                else
                PreferedWaypoint = 0;
        }

        if( Helper.I.PathfindMoveForWayPointJump == false || Map.I.RM.RMD.ForceJumpToCheckPoint )
        {
            MasterAudio.PlaySound3DAtVector3( "Puzzle Jump", G.Hero.Pos );
            if( HeroSector.GotoPuzzleUnlocked == false )
            {
                AutoGotoEnergyCost += RMD.GotoCheckPointResourceCost;
            }
            HeroSector.GotoPuzzleUnlocked = true;

            if( G.Hero.Pos != poslist[ HeroSector.LastAreaJumpID ] )
                G.Hero.Control.ApplyMove( G.Hero.Pos, poslist[ HeroSector.LastAreaJumpID ] );                       // Directly jump to waypoint
            return;
        }
 
        if( Item.GetNum( ItemType.Energy ) < Map.I.RM.RMD.GotoCheckPointResourceCost )
        {
            Message.RedMessage( "Not Enough Energy!" );
            return;
        }

        MyPathfind.I.CreateMap( Map.I.Tilemap.width, Map.I.Tilemap.height );                                         // Check Path
        MyPathfind.I.SeedJumpMap();
        MyPathfind.I.GetPath( Map.I.Hero.Pos, poslist[ HeroSector.LastAreaJumpID ] );
        if( MyPathfind.I.Path != null && MyPathfind.I.Path.Length > 1 )                                              // Fill move array
        {
            G.Hero.Control.PathFinding.Path.Clear();
            for( int i = 0; i < MyPathfind.I.Path.Length; i++ )
                G.Hero.Control.PathFinding.Path.Add( MyPathfind.I.Path[ i ] );
            G.Hero.Control.PathFinding.Path.Add( poslist[ HeroSector.LastAreaJumpID ] );
            G.Hero.Control.PathFinding.CurrentStep = 1;
            MasterAudio.PlaySound3DAtVector3( "Puzzle Jump", G.Hero.Pos );
            
            if( HeroSector.GotoPuzzleUnlocked == false )
            {
                AutoGotoEnergyCost += RMD.GotoCheckPointResourceCost;
            }

            HeroSector.GotoPuzzleUnlocked = true;
            return;
        }      
    }
    public void UpdateCubeJump()
    {
        return;
        if( RMD.QuickTravelEnabled == false ) return;
        if( HeroSectorType == Sector.ESectorType.LAB ) LastLabHeroPos = Map.I.Hero.Pos;
        else
            if( HeroSectorType == Sector.ESectorType.GATES )
            {
                if( LockSectorJump )
                    Message.GreenMessage( "Cube Jump Enabled!" );
                LastDungeonHeroPos = Map.I.Hero.Pos;
                LockSectorJump = false;
            }

        Unit save = Map.I.GetUnit( ETileType.CHECKPOINT, G.Hero.Pos );                                                     // over savegame restriction
        if( HeroSectorType == Sector.ESectorType.NORMAL )
        if( save == false ) return;

        Unit u = Map.I.GetUnit( Map.I.Hero.Pos, ELayerType.GAIA2 );                                                      //  Locks jump after hero step over arrow
        
        if( RMD.LockCubeJumpEnabled )
        if( HeroSectorType == Sector.ESectorType.LAB   ||
            HeroSectorType == Sector.ESectorType.NORMAL )
        if( u )
        if( Map.I.CurrentArea == -1 )
        if( u.TileID == ETileType.ARROW || u.TileID == ETileType.TRAP || u.TileID == ETileType.FREETRAP )            
        {
            //if( Map.I.AdvanceTurn && LockSectorJump == false )
            //    Message.RedMessage( "Cube Jump Disabled!\nStep on a Gate Tile to reenable it!" );
            LockSectorJump = true;
        }

        if( LastLabHeroPos.x == LabArea.x ||
            LastLabHeroPos.y == LabArea.y ||
            LastLabHeroPos.x == LabArea.x + LabArea.width  - 2 ||
            LastLabHeroPos.y == LabArea.x + LabArea.height - 2 )
        {
            LastLabHeroPos = Map.I.LevelEntrancePosition;
        }
            
        if( cInput.GetKey( "Wait" ) )
             JumpKeyTimer += Time.deltaTime;

        if( cInput.GetKeyUp( "Wait" ) )
        {
            JumpKeyTimer = 0; 
        }

        if( LockSectorJump ) return;

        if( Map.I.CurrentArea == -1 )
        {
            if( JumpKeyTimer > .4f )
            {
                Map.I.Hero.Control.ApplyMove( Map.I.Hero.Pos, LastLabHeroPos );
                Sector.EnterSector( HeroSector, LastLabHeroPos );
                JumpKeyTimer = 0;
                return;
            }
                        
            List<Sector> s = new List<Sector>();  // make a list of all sectors available
            for( int y = 0; y < Sector.NY; y++ )
            for( int x = 0; x < Sector.NX; x++ )
                {
                    Sector ss = RMSector[ x, y ];
                    int resCount = 0;
                    for( int yy = ( int ) ss.Area.yMin - 1; yy < ss.Area.yMax + 1; yy++ )
                    for( int xx = ( int ) ss.Area.xMin - 1; xx < ss.Area.xMax + 1; xx++ )
                    {
                        //if( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
                        //if( Map.I.Gaia2[ xx, yy ] )
                        //if( Map.I.Gaia2[ xx, yy ].IsResource( true ) ) resCount++;
                    }

                    if( RMSector[ x, y ].Type == Sector.ESectorType.NORMAL )
                    if( RMSector[ x, y ].Discovered )
                    if( RMSector[ x, y ].Cleared == false || resCount > 0 )
                        s.Add( RMSector[ x, y ] );
                }

            if( cInput.GetKeyDown( "Wait" ) )
            {
                LastSectorJumpID++;
                if( LastSectorJumpID >= s.Count ) LastSectorJumpID = 0;

                if( s.Count > 0 )
                if( s[ LastSectorJumpID ].LastHeroPos.x != -1 )
                {
                    Map.I.Hero.Control.ApplyMove( Map.I.Hero.Pos, s[ LastSectorJumpID ].LastHeroPos );
                    Sector.EnterSector( HeroSector, s[ LastSectorJumpID ].LastHeroPos );
                }
            }
        }         
    }

    public bool SectorOnMap( Vector2 tg )
    {
        if( tg.x < 0 ) return false;
        if( tg.y < 0 ) return false;
        if( tg.x > 7 ) return false;
        if( tg.y > 7 ) return false;
        return true;
    }
    

    public void UpdateRandomMap()
    {
        HeroSector.TimeSpentOnCube += Time.deltaTime;

        if( MoveToLabStart == false )
        if( Helper.I.AutoOpenGateAtStart || Map.I.RM.DungeonDialog.AutoOpenGateCheck() )                                                                       // Auto open gate
        if( Map.I.AutoOpenGateStep >= 0 && Map.I.AutoOpenGateStep <= 3 )
            {
                if( Map.I.AutoOpenGateStep == 0 && AutoGotoEnergyCost == 0 )
                {
                    AutoGotoEnergyCost += RMD.AutoOpenGateResourceCost;
                }
                EActionType ac = ( EActionType ) 
                Util.GetRotationToTarget( G.Hero.Pos, G.Hero.GetFront() );
                G.Hero.Control.ForceMove = ac;
            }        
        else 
        {
            if( Map.I.AutoOpenGateStep >= 5 && Map.I.AutoOpenGateStep <= 6 )
            {
                G.Hero.Control.ForceMove = EActionType.WAIT;
                Map.I.AutoOpenGateStep++;
              }
        }

        Vector2 pos = LabStartPosList[ SortedLabStartID ];
        if( MoveToLabStart )
        if( pos != Vector2.zero )                                                                                // Move hero to a predetermined pos after init                                                          
        if( Map.I.RM.HeroSector.Type == Sector.ESectorType.LAB )
        {
            Vector2 to = new Vector2( LabArea.xMin, LabArea.yMax ) + pos;
            if( RMD.DirectLabJump )
            {
                G.Hero.Control.ApplyMove( G.Hero.Pos, to );
            }
            else
            {
                MyPathfind.I.CreateMap( Map.I.Tilemap.width, Map.I.Tilemap.height );
                MyPathfind.I.SeedJumpMap();
                MyPathfind.I.GetPath( Map.I.Hero.Pos, to );
                G.Hero.Control.PathFinding.Path.Clear();
                for( int i = 0; i < MyPathfind.I.Path.Length; i++ )
                    G.Hero.Control.PathFinding.Path.Add( MyPathfind.I.Path[ i ] );
                G.Hero.Control.PathFinding.CurrentStep = 1;
            }
            MoveToLabStart = false;
            G.Hero.RotateTo( LabStartDirList[ SortedLabStartID ] );
        }

        if( Map.I.TurnFrameCount == 2 )                                                                          // only one check per turn
        {
            if( Map.I.CurrentArea == -1 )                                                                                       
            if( HeroSector.Type == Sector.ESectorType.NORMAL )
            {
                Unit save = Map.I.GetUnit( ETileType.CHECKPOINT, G.Hero.Pos );
                if( save )
                    HeroSector.LastHeroPos = G.Hero.Pos;
            }

            //if( HeroSector.Type == Sector.ESectorType.GATES )
            //    System.GC.Collect();

            UI.I.DebugLabel.text = "";

            if( Map.I.RM.DungeonDialog.gameObject.activeSelf == false )
            {
                Map.I.RM.DungeonDialog.UpdateIt();                                                                                  // On screen goal info
                if( DungeonDialog.InventoryGO.activeSelf )
                    DungeonDialog.InventoryGO.SetActive( false );            
            }

            int conquered = 0;
            if( UI.I.UpdGoalText )
            for( int i = 0; i < Map.I.RM.RMD.GoalList.Length; i++ )
            {
                RandomMapGoal go = Map.I.RM.RMD.GoalList[ i ];
                if( go.Conquered == false )
                {
                    //if( Map.I.AdvanceTurn )
                    //{
                    //    UI.I.GoalText = go.Panel.DescriptionLabel.text + ": "                                                // find next goal
                    //                + go.Panel.CurrentNumberLabel.text + " Done";
                    //    UI.I.GoalText = UI.I.GoalText.Replace( ".", ":" );

                    //}
                    break;
                }
                else conquered++;
            }

            if( conquered >= Map.I.RM.RMD.GoalList.Length )                                                                  // Final Msg
            {
                if( Helper.I.ReleaseVersion )
                    UI.I.SetBigMessage( "All Objectives Have Been Met!\n\n" +
                                        "Your Light is now Shining Brighter..\n\n" +
                                        "(Step through the Gate,\n\nthen Press Enter to Finalize Adventure)",
                                         Color.yellow, 99999 );
                UI.I.ArtifactsText.text = "All Objectives Completed!";
            }
            else
            {
                UI.I.QuestCompletitionText = "Goal " + ( conquered ) + "/" + Map.I.RM.RMD.GoalList.Length +                 // Current goal
                " Quest " + DungeonDialog.AdventureCompletion + "%";
            }

            if( DungeonDialog.gameObject.activeSelf ) UI.I.DebugLabel.text = "";
            if( Map.I.CurrentArea == -1 )                                                                                   // Sell wood for blue rune
            {
                int count = 0;
                for( int i = 0; i < Quest.I.CurLevel.ArtifactList.Count; i++ )                                                 // Check for Available artifacts
                {
                    Artifact ar = Quest.I.CurLevel.ArtifactList[ i ];
                    if( ar.Collected == Artifact.EStatus.NOT_COLLECTED )
                        if( Map.I.Revealed[ ( int ) ar.Pos.x, ( int ) ar.Pos.y ] )
                        {
                            if( Manager.I.GameType == EGameType.CUBES )
                            {
                                //if( HeroData.I.AddResource( false, ar.CostResource_1, -ar.CostValue_1 ) &&
                                //    HeroData.I.AddResource( false, ar.CostResource_2, -ar.CostValue_2 ) ) count++;
                            }
                        }
                }
                if( count > 0 )                
                     UI.I.ArtifactInfoLabel.text = "Perks: +" + count;                
                else UI.I.ArtifactInfoLabel.text = "Perks:";
            }
        }

        UpdateMudSteppingText();
        UpdateMapCarving();

        if( Map.I.AdvanceTurn ) 
        if( Map.I.CurrentArea != -1 )                                                                                         // Song of Peace Timer Decrease
        if( Map.I.CurArea.SongOfPeace > 0 )
            {
                Message.GreenMessage( "Song of Peace: " + Map.I.CurArea.SongOfPeace );
                Map.I.CurArea.SongOfPeace--;
            }

        UpdateAutoBuy();
        if( Map.I.AdvanceTurn )
        if( Map.I.LevelStats.NormalSectorsDiscovered >= 2 )
        if( Map.I.CurrentArea == -1 )
        if( Util.Chance( 15 ) )
        if( CheckGate( false, new Vector2( -1, -1 ) ) )
            Message.CreateMessage( ETileType.NONE, "Gate Key Available!", Map.I.Hero.Pos, new Color( 0, 1, 0, 1 ), true, true, 7 );

        UpdateResourceTimers( false );

        if ( Map.I.AdvanceTurn == false ) return;
        if ( Map.I.CurrentArea == -1 ) return;
        if ( Map.I.CurArea.AreaTurnCount <= 0 ) return;

        if( Map.I.CurrentArea != -1 )
        {
            float rune = 0, monst = 0;
            float totrunes = Map.I.CurArea.CalculateAreaClearedBonus( ref rune, ref monst, false, Map.I.RM.HeroSector );  
        }
    }

    public void UpdateMudSteppingText()
    {
        if( Map.I.GetUnit( ETileType.STONEPATH, G.Hero.Pos ) == null ) return;
        if( HeroSector.Type != Sector.ESectorType.NORMAL ) return;
        UI.I.DebugLabel.text = "Stonepath Effect:\n\n";
        if( CubeData.I.StonepathDiagonalMovement == false )
            UI.I.DebugLabel.text += "Hero Diagonal Movement Restricted.\n";
        if( CubeData.I.StonepathMovementMaxSpeed > 0 )
            UI.I.DebugLabel.text += "Hero Movement: " + CubeData.I.StonepathMovementMaxSpeed + " Steps per 10s\n";
        if( CubeData.I.StonepathRotationMaxSpeed > 0 )
            UI.I.DebugLabel.text += "Hero Rotation: " + CubeData.I.StonepathRotationMaxSpeed + " Steps per 10s\n";
    }

    public void UpdateMapCarving()
    {
        //return;
        //if( Map.I.AdvanceTurn ) return;
        //if( Map.I.CurrentArea != -1 ) return;
        //if( Map.I.RM.HeroSector == null ) return;
        //if( Map.I.RM.HeroSector.Carved ) return;
        //if( Map.I.RM.HeroSector.Type != Sector.ESectorType.NORMAL ) return;
        ////if( Input.GetKey( KeyCode.W ) == false ) return;

        //for( int a = 0; a < Map.I.RM.HeroSector.AreaList.Count; a++ )
        //    {
        //        Map.I.CreateMoveOrderList( Map.I.RM.HeroSector.AreaList[ a ], true );

        //        for( int i = 0; i < Map.I.MoveOrder.Count; i++ )
        //        for( int j = 0; j < Map.I.MoveOrder.Count; j++ )
        //        if ( i != j )
        //                {
        //                    MyPathfind.I.CreateMap( Map.I.Tilemap.width, Map.I.Tilemap.height );                        // optimize if slow
        //                    MyPathfind.I.SeedForestMap( Map.I.RM.HeroSector.AreaList[ a ] );

        //                    if( Map.I.MoveOrder[ i ].TileID == ETileType.ROACH ||
        //                        Map.I.MoveOrder[ i ].TileID == ETileType.SCARAB )
        //                    {
        //                        MyPathfind.I.Path = null;
        //                        MyPathfind.I.GetPath( Map.I.MoveOrder[ i ].Pos, Map.I.MoveOrder[ j ].Pos );

        //                        if( MyPathfind.I.Path == null )
        //                        {
        //                            MyPathfind.I.CreateMap( Map.I.Tilemap.width, Map.I.Tilemap.height );               // optimize if slow
        //                            MyPathfind.I.SeedClearedMap( Map.I.RM.HeroSector.AreaList[ a ] );
        //                            MyPathfind.I.GetPath( Map.I.MoveOrder[ i ].Pos, Map.I.MoveOrder[ j ].Pos );

        //                            if( MyPathfind.I.Path != null )
        //                            {
        //                                for( int p = 0; p < MyPathfind.I.Path.Length; p++ )
        //                                {
        //                                    TKUtil.SetTile( new Vector2( MyPathfind.I.Path[ p ].x,
        //                                    MyPathfind.I.Path[ p ].y ), ETileType.OPENBLACKGATE );
        //                                    Quest.I.Dungeon.Tilemap.SetTile( ( int ) MyPathfind.I.Path[ p ].x,
        //                                    ( int ) MyPathfind.I.Path[ p ].y, ( int ) ELayerType.GAIA, -1 );
        //                                 }
        //                            }
        //                        }
        //                    }
        //                }
        //    }

        //Map.I.RM.HeroSector.Carved = true;
        //Map.I.MoveOrder = new List<Unit>();
    }

    public void UpdateResourceTimers( bool areaExit, bool justmesh = false )
    {
        if( Quest.CurrentLevel != -1 ) return;
        if( Map.I.CurrentArea == -1 ) return;
        if( Map.I.CurArea.AreaTurnCount <= 0 ) return;

        if( G.Hero.Control.PerfectionistLevel >= 1 )                                             // Inarea Perfectionist rune decrease skip
        if( Area.LastAreaClearedWasPerfect ) return;

        for( int y = ( int ) Map.I.P2.y; y <= Map.I.P1.y; y++ )                                                    
        for( int x = ( int ) Map.I.P1.x; x <= Map.I.P2.x; x++ )
        if ( Map.I.GetPosArea( new Vector2( x, y ) ) != -1 )
		if ( Map.I.Gaia2[ x, y ] != null )
        if ( Map.I.Gaia2[ x, y ].LevelTxt )
        if ( Map.I.Gaia2[ x, y ].Body.TurnsToDie != -1 )
        {
            if( justmesh == false )
            if( Map.I.Gaia2[ x, y ].Body )
            {
                float wit = 1;
                if( Map.I.CurrentArea != -1 )                                                     // Time base decrease
                    if( Map.I.RM.RMD.RealtimeResourceCounterDecrease )
                        wit = Time.deltaTime;

                Map.I.Gaia2[ x, y ].Body.TurnsToDie -= wit;
                if( areaExit )
                    Map.I.Gaia2[ x, y ].Body.OriginalTurnsToDie -= wit;

                if( Map.I.Gaia2[ x, y ].Body.TurnsToDie <= 0 )
                {
                    if( Map.I.Hero.Body.ProspectorLevel >= 6 )
                        Map.I.Hero.Control.UpdateResourceCollecting( new Vector2( x, y ), true );
                    else
                        Map.I.Gaia2[ x, y ].Activate( false );
                }

                if( Map.I.Gaia2[ x, y ] )
                    Map.I.Gaia2[ x, y ].UpdateText();
            }
        }
    }

    public void UpdateStatistics()
    {
        if( Map.I.AdvanceTurn == false ) return;
        if( Quest.CurrentLevel != -1 ) return;
        if( Manager.I.GameType != EGameType.CUBES ) return;
        CheckPattern();
        Map.I.LevelStats.SectorsDiscovered = 0;
        Map.I.LevelStats.SectorsCleared = 0;
        if( Map.I.RM.StartingCube > 1 ) 
            Map.I.LevelStats.SectorsCleared += Map.I.RM.StartingCube - 1;
        Map.I.LevelStats.NumPerfectAreas = 0;
        Map.I.LevelStats.NumPerfectSectors = 0;
        Map.I.LevelStats.AccumulatedPoints = 0;
        Map.I.LevelStats.AccumulatedBonuses = 0;
        Map.I.LevelStats.MaxBonusReached = 0;

        HeroSector.FireIgnitionCount = 0;                                                                                  // Sector fire lit count
        for( int i = 0; i < HeroSector.AreaList.Count; i++ )
            HeroSector.FireIgnitionCount += HeroSector.AreaList[ i ].BonfiresLit;

        for( int y = 0; y < Sector.NY; y++ )                                                                              // Stats counted
        for( int x = 0; x < Sector.NX; x++ )
            {
                if( RMSector[ x, y ].Discovered )
                {
                    Map.I.LevelStats.SectorsDiscovered++;

                    RMSector[ x, y ].PerfectAreaCount = 0;
                    RMSector[ x, y ].AllBurntAreaCount = 0;
                    RMSector[ x, y ].MaximumWoundedUnits = 0;
 
                    for( int a = 0; a < RMSector[ x, y ].AreaList.Count; a++ )
                    {
                        if( RMSector[ x, y ].AreaList[ a ].Perfect && RMSector[ x, y ].AreaList[ a ].Cleared )
                        {
                            Map.I.LevelStats.NumPerfectAreas++;
                            RMSector[ x, y ].PerfectAreaCount++;

                            if( RMSector[ x, y ].AreaList[ a ].AllBurntAvailable )
                                RMSector[ x, y ].AllBurntAreaCount++;

                        }

                        if( RMSector[ x, y ].AreaList[ a ].Cleared || 
                            RMSector[ x, y ].AreaList[ a ].IsHeroArea() )
                        if( RMSector[ x, y ].AreaList[ a ].MaximumWoundedUnits >
                            RMSector[ x, y ].MaximumWoundedUnits )
                            RMSector[ x, y ].MaximumWoundedUnits =
                            RMSector[ x, y ].AreaList[ a ].MaximumWoundedUnits;

                        if( RMSector[ x, y ].PerfectAreaCount == RMSector[ x, y ].AreaList.Count )
                        {
                            RMSector[ x, y ].Perfect = true;
                            Map.I.LevelStats.NumPerfectSectors++;
                        }
                    }
                }

                if( RMSector[ x, y ].Cleared )
                    Map.I.LevelStats.SectorsCleared++;
            }
        
        for( int y = 0; y < Sector.NY; y++ )                                                                                                           // Updates all Sectors Scores
        for( int x = 0; x < Sector.NX; x++ )
            {
                if( RMSector[ x, y ].Discovered )
                    for( int a = 0; a < RMSector[ x, y ].AreaList.Count; a++ )
                    {
                        float points = 0, monst = 0;
                        RMSector[ x, y ].AreaList[ a ].CalculateAreaClearedBonus( ref points, 
                                                   ref monst, true, RMSector[ x, y ] );
                    }
            }

        for( int y = 0; y < Sector.NY; y++ )                                                                                                           
        for( int x = 0; x < Sector.NX; x++ )
            if( RMSector[ x, y ].Discovered )
            {
                Vector2[] tglist  = { new Vector2( 0,  1 ), new Vector2(  1, 0 ), 
                              new Vector2( 0, -1 ), new Vector2( -1, 0 ) };
                RMSector[ x, y ].NeighBorBonus = 0;
                for( int i = 0; i < 4; i++ )                                                                                                           // Neighboor sectors bonus
                {
                    Vector2 tg = new Vector2( x, y ) + tglist[ i ];

                    if( Map.I.RM.SectorOnMap( tg ) )
                    {
                        if( Map.I.RM.RMSector[ ( int ) tg.x, ( int ) tg.y ].Cleared )
                            if( Map.I.RM.RMSector[ ( int ) tg.x, ( int ) tg.y ].Perfect )
                                RMSector[ x, y ].NeighBorBonus += Map.I.RM.RMSector[ ( int ) tg.x, ( int ) tg.y ].TotalBonus;
                    }
                }
            }
       
        for( int y = 0; y < Sector.NY; y++ )
        for( int x = 0; x < Sector.NX; x++ )
            {
                if( RMSector[ x, y ].Discovered )
                {
                    for( int a = 0; a < RMSector[ x, y ].AreaList.Count; a++ )                                                                        // Updates all Score points text meshes
                    {            
                        float points = 0, monst = 0;
                        RMSector[ x, y ].AreaList[ a ].CalculateAreaClearedBonus( ref points,
                                                   ref monst, true, RMSector[ x, y ], true );

                        RMSector[ x, y ].AreaList[ a ].UpdateScoreMesh( RMSector[ x, y ].AreaList[a].Score );

                        if( RMSector[ x, y ].AreaList[ a ].Cleared )
                            Map.I.LevelStats.AccumulatedPoints += points;
                    }

                    float bn = RMSector[ x, y ].TotalBonus;

                    Map.I.LevelStats.AccumulatedBonuses += bn;

                    if( bn > Map.I.LevelStats.MaxBonusReached )
                        Map.I.LevelStats.MaxBonusReached = bn;
                }
            }
        }

    public bool UpdateDialogToggle()
    {
        bool QuickRestart = false;                                            
        if( Input.GetKeyDown( KeyCode.R ) )
        if( DungeonDialog.gameObject.activeSelf == false )
        if( GS.RestartAvailable || GS.SaveStepList.Count <= 0 )
        {
            if( Map.I.FishingMode != EFishingPhase.NO_FISHING ) return false;
            if( GS.CustomSaveExists ) return false;

            float avail = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.UPGRADE_AUTOOPENGATE_BUTTON );   // Quick restart 
            if( Helper.I.ReleaseVersion == false ) avail = 1;
            if( avail > 0 )
            QuickRestart = true;
        }

        if( Map.I.CubeDeath )
        if( Helper.I.AutoRestartAfterCubeDeath )                                                               // auto restart after death
        {
            if( GS.LastSavedCube == null && GS.CustomSaveExists == false )
            {
                QuickRestart = true;
                Manager.I.ForceRestart = true;
                Map.I.RM.DungeonDialog.NewButtonClicked = true;
                Map.I.RM.GameOver = true;
            }
            else
            {
                GS.ForceLoad = true;
                return false;
            }
        }

        if( Input.GetKeyDown( KeyCode.Return ) || QuickRestart ||          
            UI.I.RestartAreaButton.state == UIButtonColor.State.Pressed ) 
        {
            if( ( GameOver || Map.I.Hero.Body.Hp <= 0 ) && DungeonDialog.gameObject.activeSelf )
            {
                if( !GameOver )
                    DungeonDialog.SetMsg( "Game Over!\nCheck your goals and finalize Adventure.", Color.red );
            }
            else
                DungeonDialog.gameObject.SetActive( !DungeonDialog.gameObject.activeSelf );

            if( DungeonDialog.gameObject.activeSelf )
            {
                UI.I.ScrollText.gameObject.SetActive( false );
                UI.I.ScrollText.text = "";
            }
            if( UI.I.RestartAreaButton.state == UIButtonColor.State.Pressed )
                UI.I.RestartAreaButton.SetState( UIButtonColor.State.Normal, true );
        }
        return DungeonDialog.gameObject.activeSelf;
    }

    public void UpdateInput()
    {
        if( Map.I.InvalidateInputTimer > 0 ) return;

        if( Helper.I.DebugHotKey )
        {
            if( Input.GetKey( KeyCode.F7 ) )
            {
                for( int y = ( int ) LabArea.yMin; y < LabArea.yMax; y++ )
                for( int x = ( int ) LabArea.xMin; x < LabArea.xMax; x++ )
                if ( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )  // Watch for bugs ***
                {
                    Quest.I.UpdateArtifactStepping( new Vector2( x, y ) );
                }
            }
            if( Input.GetMouseButtonDown( 0 ) )
            {
                bool res = Map.I.Hero.CheckPurchase( true, Map.I.Hero.Pos, new Vector2( Map.I.Mtx, Map.I.Mty ) );
                if( res )
                {
                    UpdateGatesCost( new Vector2( Map.I.Mtx, Map.I.Mty ) );
                }

                Sector s = Sector.GetPosSector( new Vector2( Map.I.Mtx, Map.I.Mty ) );
                SectorHint.UpdateSectorHintColor( s );
            }
        }
    }

    public bool CheckGate( bool showMessage, Vector2 tg )
    {
        int cont = 0;
        NumGateKeyNeeded = -1;
        for( int i = 0; i < 8; i++ )                                                                                           // Check for Free Gate
        {
            Vector2 aux = tg + Manager.I.U.DirCord[ i ];
            if( Map.PtOnMap( Map.I.Tilemap, aux ) )
            {
                Sector s = Sector.GetPosSector( aux );

                if( s && s.Type == Sector.ESectorType.NORMAL && s.AreasCreated == false ) cont++;
            }
        }

        if( RMD.AllowFreeGateOpening )
        if( tg != new Vector2( -1, -1 ) )
        if( cont <= 0 ) return true;
         
        float ar = Map.I.LevelStats.AreasDiscovered - Map.I.LevelStats.AreasCleared;                                                         // Key Gate System: Maximum Dirty areas reached on dungeon
        ar -= Map.I.RM.RMD.PerfectAreaKeyWeight * Map.I.LevelStats.NumPerfectAreas;

        int discovered = Map.I.LevelStats.SectorsDiscovered - 4;                                                                             // Max Cubes Reached. Finalize Adventure
        if( Map.I.RM.RMD.EnableAlternateStartingCube )                                                        
            discovered += StartingCube - 1;        
        
        if( Map.I.RM.RMD.MaximumUnits != -1 )                                                                                               // Gate Key system: per Monster Alive
        if( Map.I.LevelStats.SectorsDiscovered > 4 )
        if( discovered < RMD.MaxCubes )
        {
            int rem = Map.I.RM.HeroSector.GetRemainingMonsters();
            if( rem > 0 )
            {
                if( showMessage )
                    Message.CreateMessage( ETileType.NONE, "Kill More Monsters!\n Required: " + rem,
                                           Map.I.Hero.Pos, new Color( 1, 0, 0, 1 ), true, true, 7 );
                return false;
            }
        }

        if( RMD.MaxCubes > 0 && discovered >= AvailableCubesForPlaying )                                                                    // Needs to unlock more cubes for playing
        if( AvailableCubesForPlaying != -1 )
        {
            LastCubeReached = true;
            if( showMessage )
                Message.GreenMessage( "This Adventure has in total: " + RMD.MaxCubes + " Cubes,\n" +
                "But you are only allowed to play: " + AvailableCubesForPlaying + " Cubes.\n\n" +
                "Finalize the Adventure, Unlock more cubes and\nthen start a new incursion to proceed.");
            return false;
        }

        if( RMD.MaxCubes > 0 && discovered >= RMD.MaxCubes )                                                                                
        {
            LastCubeReached = true;
            if( showMessage )
                Message.GreenMessage( "Maximum Cube Reached!\nConquer more Goals\nor finalize Adventure!" );
            return false;
        }

        return true;
    }

    public void EnterArea( Area area )
    {
        //MyPathfind.I.CreateMap( Map.I.Tilemap.width, Map.I.Tilemap.height );   // optimize if slow
        //MyPathfind.I.SeedForestMap( area );

        //for( int i = 0; i < Map.I.MoveOrder.Count; i++ )
        //    if( Map.I.MoveOrder[ i ].TileID == ETileType.ROACH || 
        //        Map.I.MoveOrder[ i ].TileID == ETileType.SCARAB )
        //    {
        //        MyPathfind.I.GetPath( Map.I.Hero.Pos, Map.I.MoveOrder[ i ].Pos );

        //        if( MyPathfind.I.Path == null )                                                   // Monsters stuck, give give free bomb
        //        {
        //            //Map.I.CurArea.FreeBomb = 1;
        //            Message.CreateMessage( ETileType.NONE, "Stuck Monster - Free Bomb Available.",
        //                                   Map.I.Hero.Pos, new Color( 0, 1, 0, 1 ), true, true, 7 );
        //            Quest.I.UpdateArtifactData( ref G.Hero );
        //            break;
        //        }
        //    }

        if( Map.I.Hero.Body.ProspectorLevel >= 5 )                                                                          // Front facing resource collecting
            Map.I.Hero.Control.UpdateResourceCollecting( Map.I.Hero.Pos + 
                Map.I.Hero.GetRelativePosition( EDirection.N, 1 ), false ); 
    }

    public void CheckPattern()
    {
        if( Map.I.CurrentArea == -1 ) return;
        if( Map.I.CurArea.MonsterFlankCount <= 0 )                                                                                          // Monster Flanked
        {
            Unit up = Map.I.GetUnit( new Vector2( Map.I.Hero.Pos.x, Map.I.Hero.Pos.y + 1 ), ELayerType.MONSTER );
            Unit dn = Map.I.GetUnit( new Vector2( Map.I.Hero.Pos.x, Map.I.Hero.Pos.y - 1 ), ELayerType.MONSTER );
            Unit lf = Map.I.GetUnit( new Vector2( Map.I.Hero.Pos.x - 1, Map.I.Hero.Pos.y ), ELayerType.MONSTER );
            Unit rt = Map.I.GetUnit( new Vector2( Map.I.Hero.Pos.x + 1, Map.I.Hero.Pos.y ), ELayerType.MONSTER );

            int old = Map.I.CurArea.MonsterFlankCount;

            if( up && dn )
            {
                if( up.ValidMonster && dn.ValidMonster )
                    Map.I.CurArea.MonsterFlankCount++;
            }

            if( rt && lf )
            {
                if( rt.ValidMonster && lf.ValidMonster )
                    Map.I.CurArea.MonsterFlankCount++;
            }
        }
    }
    
    public void UpdateAutoBuy()
    {
        if( RMD.AutoBuyMode == false ) return;
        if( HeroSector.Type != Sector.ESectorType.NORMAL ) return;
        if( Map.I.AdvanceTurn == false ) return;

        if( HeroData.I.BlueRunes < RMD.InitialLabGateCost ) return;

        List <Vector2> tgl = new List<Vector2>(); 

        for( int y = ( int ) LabArea.yMin; y < LabArea.yMax; y++ )
            for( int x = ( int ) LabArea.xMin; x < LabArea.xMax; x++ )
                if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
                    if( Quest.I.GetArtifactInPos( new Vector2( x, y ) ) )
                {
                    tgl.Add( new Vector2( x, y ) );
                }

        if( tgl.Count <= 0 ) return;

        int num = Random.Range( 0, tgl.Count );
        Quest.I.UpdateArtifactStepping( tgl[ num ] );

        Message.CreateMessage( ETileType.NONE, "Autobuy Artifact Bought!", Map.I.Hero.Pos, new Color( 0, 1, 0, 1 ), true, true, 7 );
    }

    public void Finalize()
    {
        if( Manager.I.Status == EGameStatus.LEVELINTRO ) return;
        Map.I.Lights.SetTempLight(); 
        GameOver = true;
        Map.I.SessionTime = 0;
        RandomMapGoal.NewRecordID = -1;
        Map.I.FirstCubeDiscoveredTime = 0;
        Map.I.StopAllLoopedSounds();

        if( HeroSector != null && Map.I.Gaia2 != null )
        {
            Map.I.ExitArea( Map.I.Hero.Pos, HeroSector.LastHeroPos );
            G.Hero.Control.ApplyMove( Map.I.Hero.Pos, HeroSector.LastHeroPos );
        }
        GameObject[ ] taggedObjects = GameObject.FindGameObjectsWithTag( "Lightining" );
        for( int i = 0; i < taggedObjects.Length; i++ )
            if( PoolManager.Pools[ "Pool" ].IsSpawned( taggedObjects[ i ].transform ) )
                PoolManager.Pools[ "Pool" ].Despawn( taggedObjects[ i ].transform );

        //for( int y = 0; y < Map.I.Tilemap.height + 1; y++ )     // Check for bugs +1 is to avoid color border
        //for( int x = 0; x < Map.I.Tilemap.width + 1; x++ )   
        //    Map.I.Tilemap.ColorChannel.SetColor( x, y, new Color( 0, 0, 0, 1 ) );
    }

    public void UpdateBarricadeStrenghtening()
    {
        //if( Map.I.AdvanceTurn == false ) return;
        //if( Map.I.CurrentArea != -1 ) return;
        //if( HeroSector.Type != Sector.ESectorType.NORMAL ) return;
        //if( HeroSector.AvailableNails <= 0 ) return;
        
        //List<Unit> ul = Util.GetUnitsInTheArea( HeroSector.Area, ETileType.BARRICADE );

        //float chance = RMD.NailWorkingChance;
        //if( AreaDefinition.Current.NailWorkingChance != -1 ) chance = AreaDefinition.Current.NailWorkingChance;
        
        //for( int i = 0; i < ul.Count; i++ )
        //{
        //    if( Map.I.Unit[ ( int ) ul[ i ].Pos.x, ( int ) ul[ i ].Pos.y ].Variation == 9 )
        //    {
        //        ul.RemoveAt( i );
        //        i--;
        //    }
        //}

        //if( ul.Count <= 0 ) return;

        //int rand = Random.Range( 0, ul.Count );

        //Unit un = Map.I.Unit[ ( int ) ul[ rand ].Pos.x, ( int ) ul[ rand ].Pos.y ];

        //if( Util.Chance( chance ) == false )
        //{
        //    HeroSector.AvailableNails--;
        //    Message.CreateMessage( ETileType.NONE, "Fail!  (" + chance + ")%", un.Pos, Color.red, true, true, 5, .2f, .12f );
        //    return;
        //}

        //if( un && un.TileID == ETileType.BARRICADE )
        //{
        //    HeroSector.AvailableNails--;
        //    un.SetVariation( un.Variation + 1 );
        //    Message.CreateMessage( ETileType.NONE, "+1", un.Pos, Color.green, true, true, 5, .2f, .12f );
        //}
    }

    public void UpdateCloverResort()
    {
        Map.I.CloverResortTimer -= Time.deltaTime;
        if( Map.I.AdvanceTurn == false )
        if( Map.I.CloverResortTimer <= 0 )
        {
            HeroSector.InitClovers();
            Map.I.CloverResortTimer = 10;
        }
    }
    public void FinishAutoGoto()
    {
        G.Hero.Control.PathFinding.Path.Clear();
        if( AutoGotoEnergyCost > 0 && Helper.I.ReleaseVersion )
        {
            string disc = "";
            float perc = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.DECREASE_ENERGY_COST );
            if( perc > 0 )
            {
                AutoGotoEnergyCost -= Util.Percent( perc, AutoGotoEnergyCost );
                disc = "\n" + perc + "% Off";
            }
            string msg = "-" + AutoGotoEnergyCost + "\nWaypoint: " + ( HeroSector.LastAreaJumpID + 1 ) + disc;
            Message.CreateMessage( ETileType.NONE, ItemType.Energy, msg, G.Hero.Pos, Color.red );
            Item.AddItem( ItemType.Energy, -AutoGotoEnergyCost );
            AutoGotoEnergyCost = 0;
        }
    }
}