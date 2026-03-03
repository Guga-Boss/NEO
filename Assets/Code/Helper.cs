using UnityEngine;
using System;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Helper : MonoBehaviour
{
    public static Helper I;
    [TabGroup( "Main" )]
    public bool ReleaseVersion;
    [TabGroup( "Start" )]
    public int StartingAdventure = 0;
    [TabGroup( "Main" )]
    public bool FreePlay = false;
    [TabGroup( "Main" )]
    public bool PlayMusic = false;
    [TabGroup( "Start" )]
    public int StartingCube = -1;
    [TabGroup( "Start" )]
    public bool StartFromLastEditedCube = false;
    [TabGroup( "Main" )]
    public bool SaveDataOnExit = true;
    [TabGroup( "Main" )]
    public bool FastFarm = false;
    [TabGroup( "Debug" )]
    public bool ShowDebugText, ShowDebugHeaderText;
    [TabGroup( "Debug" )]
    public float FloatVal1;
    [TabGroup( "Debug" )]
    public float FloatVal2;
    [TabGroup( "Debug" )]
    public float FloatVal3;
    [TabGroup( "Debug" )]
    public float FloatVal4;
    [TabGroup( "Debug" )]
    public bool IgnoreTutorial;
    [TabGroup( "Debug" )]
    public bool AutoClickPlayButton;
    [TabGroup( "Start" )]
    public bool StartAtCubes;
    [TabGroup( "Start" )]
    public bool StartAtFarm;
    [TabGroup( "Debug" )]
    public bool DebugHotKey;
    [TabGroup( "Debug" )]
    public bool TestTechTree = false;
    [TabGroup( "Debug" )]
    public bool ShowCameraDebugText = false;
    [TabGroup( "Other" )]
    public bool InvunerableHero;
    [TabGroup( "Other" )]
    public bool EnablePathFindingMovement;
    [TabGroup( "Other" )]
    public bool FreeBarricadeDestroy = false;
    [TabGroup( "Other" )]
    public bool FreeGateOpen = false;
    [TabGroup( "Other" )]
    public bool AutoRestartAfterCubeDeath = false;
    [TabGroup( "Other" )]
    public bool ForceRealtime = false;
    [TabGroup( "Other" )]
    public bool ForceStepping = false;
    [TabGroup( "Main" )]
    public bool AutoOpenGateAtStart = false;
    [TabGroup( "Main" )]
    public bool AutoWayPointJump = false;
    [TabGroup( "Main" )]
    public bool PathfindMoveForWayPointJump = true;
    [TabGroup( "Main" )]
    public bool ShowGaiaGrid = false;
    [TabGroup( "Debug" )]
    public MyBool ForceFlipX = MyBool.DONT_CHANGE;
    [TabGroup( "Debug" )]
    public MyBool ForceFlipY = MyBool.DONT_CHANGE;
    [TabGroup( "Other" )]
    public BoxCollider2D Colliders;
    [TabGroup( "Other" )]
    [Range( 0, 10 )]
    public float RealtimeSpeedFactor = 1;
    [TabGroup( "Debug" )]
    public NSprite HelperMark1, HelperMark2, HelperMark3;
    [TabGroup( "Debug" )]
    public string TileMapCord;
    [TabGroup( "Debug" )]
    public Vector2 TileMapCordVector;
    [TabGroup( "Debug" )]
    public int FrameRateLimit = -1;
    [TabGroup( "Other" )]
    public string ModDescriptionList;
    [TabGroup( "Debug" )]
    public Color TestColor;
    public ItemType ResourceType = ItemType.NONE;
    public float ResourceToAddAmount = 10;
    public float ResourceToRemoveAmount = -1;
    [TabGroup( "Other" )]
    public GameObject[ ] ShortcutList;
    [TabGroup( "Debug" )]
    public ETileType DrawTile;

#if UNITY_EDITOR
    [InitializeOnLoadMethod]                                                                   // Editor only initialization for Singleton
    static void InitSingleton()
    {
        EditorApplication.delayCall += () =>
        {
            if( !Application.isPlaying )
            {
                GameObject go = GameObject.Find("Helper");
                if( go != null ) I = go.GetComponent<Helper>();
            }
        };
    }
#endif

    void Start () 
    {
        I = this;

        if( Application.platform == RuntimePlatform.WindowsPlayer ) ReleaseVersion = true;

        if( ReleaseVersion )
        {
            HelperMark1.gameObject.SetActive( false );
            HelperMark2.gameObject.SetActive( false );
            HelperMark3.gameObject.SetActive( false );
            StartingAdventure = -1;
            ShowDebugText = false;
            ShowDebugHeaderText = false;
            IgnoreTutorial = false;
            AutoClickPlayButton = false;
            PlayMusic = true;
            StartAtCubes = false;
            SaveDataOnExit = true;
            StartAtFarm = false;
            AutoRestartAfterCubeDeath = true;
            FreePlay = false;                                           
            StartFromLastEditedCube = false;
            DebugHotKey = false;
            AutoOpenGateAtStart = false;
            AutoWayPointJump = false;
            PathfindMoveForWayPointJump = true;
            TestTechTree = false;
            InvunerableHero = false;
            EnablePathFindingMovement = false;
            FreeBarricadeDestroy = false;
            FreeGateOpen = false;
            ForceRealtime = false;
            ForceStepping = false;
            StartingCube = -1;
            FrameRateLimit = -1;
            ForceFlipX = MyBool.DONT_CHANGE;
            ForceFlipY = MyBool.DONT_CHANGE;
            ShowCameraDebugText = false;
            FastFarm = false;
        }
        else
        {
            FreeBarricadeDestroy = false;
            FreeGateOpen = true; 
            //AutoOpenGateAtStart = true;
            //AutoWayPointJump = true;
            PathfindMoveForWayPointJump = false;
            AutoRestartAfterCubeDeath = true;
            FreePlay = true;
        }

        if( Application.platform == RuntimePlatform.WindowsEditor )
        {
            FastFarm = true;
            DebugHotKey = true;
        }

        if( AutoOpenGateAtStart == false )                    // to avoid bugs
            AutoWayPointJump = false;

        UI.I.DebugLabel.SetText( new string( 'X', 1023 ) );
        UI.I.DebugLabel.ForceMeshUpdate();
        UI.I.DebugLabel.SetText( "" );
    }

    // Static cache to avoid GC spikes;
    private static System.Text.StringBuilder sbDebug = new System.Text.StringBuilder(1024);
    private static System.Text.StringBuilder sbMonitor = new System.Text.StringBuilder(1024);

    // NOVO: O tubo direto para injetar na malha do TMPro (Zero GC absoluto)
    private static char[] debugCharBuffer = new char[1024];

    // Função auxiliar Zero GC
    private bool AreStringsDifferent( System.Text.StringBuilder a, System.Text.StringBuilder b )
    {
        if( a.Length != b.Length ) return true;
        for( int i = 0; i < a.Length; i++ )
        {
            if( a[ i ] != b[ i ] ) return true;
        }
        return false;
    }

    // A MÁGICA ACONTECE AQUI
    private void ApplyDebugText()
    {
        if( AreStringsDifferent( sbDebug, sbMonitor ) )
        {
            int len = sbDebug.Length;

            // 1. Copia do StringBuilder direto para a memória RAM (char array) - 0 Bytes GC!
            sbDebug.CopyTo( 0, debugCharBuffer, 0, len );

            // 2. Injeta o array de char direto no motor do TextMeshPro!
            UI.I.DebugLabel.SetCharArray( debugCharBuffer, 0, len );

            sbMonitor.Length = 0;
            sbMonitor.Append( sbDebug );
        }
    }

    static void AppendOneDecimal( System.Text.StringBuilder sb, float value )
    {
        int intPart = (int)value;
        int decimalPart = (int)((value - intPart) * 10f);

        if( decimalPart < 0 ) decimalPart = -decimalPart;

        sb.Append( intPart );
        sb.Append( '.' );
        sb.Append( decimalPart );                                                      // Manual 1 decimal float append; avoids float ToString GC
    }

    public void DrawDebugText()
    {
        var mapI = Map.I;                                                              // Cache Map instance;
        if( mapI.RM.HeroSector == null ) return;
        if( Cursor.visible == false ) return;                                          // ADDED TO AVOID GC. IMPROVE FUNCTION LATER IF NECESSARY. This is a common case where we don't need to update debug text at all, so we can skip the whole function and avoid GC from string operations inside it.

        if( mapI.Unit == null ) return;

        sbDebug.Length = 0;                                                            // Zero GC clear

        if( ShowDebugHeaderText )
        {
            if( !string.IsNullOrEmpty( mapI.Deb ) )                                    // Safer string check;
            {
                sbDebug.Append( "Debug: " ).Append( mapI.Deb ).Append( '\n' );         // Literal char append avoids hidden string ops;
                ApplyDebugText();                                                      // Aplica com segurança Zero GC
                return;
            }

            sbDebug.Append( "Press F2 to Show Debug Text.\nMouse Over Unit for more Info.\n\nFPS: " );
            sbDebug.Append( (int) mapI.FPS ).Append( " Av: " );

            mapI.FPSSum += (int) mapI.FPS;
            mapI.FPSSumCount++;
            mapI.AverageFPS = mapI.FPSSum / mapI.FPSSumCount;

            sbDebug.Append( (int) mapI.AverageFPS );

            Vector2 mt = GetCubeTile(Map.GM());
            int mtxInt = (int)mt.x;
            int mtyInt = (int)mt.y;

            if( mtxInt >= 0 && mtxInt < Brain.sizeX && mtyInt >= 0 && mtyInt < Brain.sizeY )
            {
                if( Brain.dist != null )
                    sbDebug.Append( "\nBrain: " ).Append( Brain.dist[ mtxInt, mtyInt ] );
            }

            if( Cursor.visible && mapI.Mtx != -1 && mapI.Mty != -1 )
            {
                sbDebug.Append( "\nMouse Tile " ).Append( mapI.Mtx ).Append( ' ' ).Append( mapI.Mty );

                if( Manager.I.GameType == EGameType.CUBES &&
                    mapI.RM.HeroSector.Type == Sector.ESectorType.NORMAL )
                {
                    Vector2 cube = GetCubeTile(Map.GM());                              // Avoid implicit ToString();
                    sbDebug.Append( "\nCube Tile: " )
                           .Append( (int) cube.x )
                           .Append( ' ' )
                           .Append( (int) cube.y );
                }
            }

            if( mapI.CamDataID >= 0 )
                sbDebug.Append( "\nCamera Data ID: " ).Append( mapI.CamDataID );
        }

        if( mapI.Mtx == -1 || mapI.Mty == -1 )
        {
            if( ShowDebugHeaderText ) ApplyDebugText();                                // Aplica com segurança Zero GC
            return;
        }

        Unit un = Controller.GetRaft(new Vector2(mapI.Mtx, mapI.Mty));
        if( !un ) un = mapI.Unit[ mapI.Mtx, mapI.Mty ];
        if( !un ) un = mapI.Gaia2[ mapI.Mtx, mapI.Mty ];
        if( !un ) un = mapI.Gaia[ mapI.Mtx, mapI.Mty ];
        if( !un )
        {
            if( ShowDebugHeaderText ) ApplyDebugText();                                // Aplica com segurança Zero GC
            return;
        }

        if( !ShowDebugHeaderText && !ShowDebugText ) return;
        if( Manager.I.Status != EGameStatus.PLAYING ) return;
        if( BluePrintWindow.I.gameObject.activeSelf ) return;

        sbDebug.Append( "\n\nUnit Name: " ).Append( un.TileID );                       // ALERTA: Se TileID for Enum, ele gera lixo. Use (int)un.TileID se quiser purismo extremo.
        sbDebug.Append( "\nUnit Type: " ).Append( un.UnitType ).Append( '\n' );        // O mesmo vale aqui se UnitType for Enum.

        if( un.Body && un.ValidMonster )
        {
            if( un.Body.TotHp > 0 )
            {
                sbDebug.Append( "\nHP: " );
                AppendOneDecimal( sbDebug, un.Body.Hp );
                sbDebug.Append( " of " );
                AppendOneDecimal( sbDebug, un.Body.TotHp );
                sbDebug.Append( '\n' );
            }

            sbDebug.Append( "Lives: " ).Append( un.Body.Lives ).Append( '\n' );

            if( un.Control.IsFlyingUnit )
            {
                sbDebug.Append( "Flight Speed Factor: " );
                AppendOneDecimal( sbDebug, un.Control.FlightSpeedFactor );
                sbDebug.Append( "%\n" );

                sbDebug.Append( "Flight Speed: " );
                AppendOneDecimal( sbDebug, un.Control.FlyingSpeed );
                sbDebug.Append( " Tiles/sec\n" );
            }
        }

        if( un.Control && un.ValidMonster )
        {
            if( !un.Control.IsFlyingUnit )
            {
                sbDebug.Append( "\nMovement Speed: " );
                AppendOneDecimal( sbDebug, un.Control.GetMonsterRTMovSpeed() );
                sbDebug.Append( " steps/10s\n" );
            }
        }

        if( un.MeleeAttack )
        {
            sbDebug.Append( "\nMelee Damage: " );
            AppendOneDecimal( sbDebug, un.MeleeAttack.TotalDamage );

            sbDebug.Append( "\nAtt Speed: " );
            AppendOneDecimal( sbDebug, un.MeleeAttack.GetRealtimeSpeed() );
            sbDebug.Append( " hits/10s\n" );
        }

        ApplyDebugText();                                                              // Aplica final Zero GC
    }

    public Vector2 GetCubeTile( Vector2 tg )
    {
        if( Manager.I.GameType == EGameType.CUBES )
            return new Vector2( tg.x - Map.I.RM.HeroSector.Area.xMin,
                                tg.y - Map.I.RM.HeroSector.Area.yMin );
        else return new Vector2( -1, -1 );
    }
    #region Buttons
    #if UNITY_EDITOR
    [ButtonGroup( "1", 1 )]
    // [HorizontalGroup( "Split", 0.5f )]
    [Button( "Edit Quest", ButtonSizes.Large ), GUIColor( 1f, 0.52f, 0.1f )]
    public void EditQuestCallBack()
    {       
        MapSaver.EditQuestDataCallBack();
    }

    [ButtonGroup( "2", 2 )]
    //[HorizontalGroup( "Split", 0.5f )]
    [Button( "Goto Resource", ButtonSizes.Large ), GUIColor( 1f, 1f, 0 )]
    public void GotoResourceCallBack()
    {
        if( ResourceType == ItemType.NONE )
        {
            Debug.Log( "Choose a resource first." );
            return;
        }
        GameObject inv = GameObject.Find( "Inventory" );
        Inventory inve = inv.GetComponent<Inventory>();
        if( inve == null ) Debug.LogError( "No inventory obj found" );

        Selection.activeGameObject =
        inve.ItemList[ ( int ) ResourceType ].gameObject;
    }

    [ButtonGroup( "2", 2 )]
    //[HorizontalGroup( "Split", 0.5f )]
    [Button( "Add Resource", ButtonSizes.Large ), GUIColor( 0, 1f, 0 )]
    public void AddResourceCallBack()
    {
        if( ResourceType == ItemType.NONE ) 
        {
            Debug.Log( "Choose a resource first." );
            return;
        }
        if( Application.isPlaying == false ) return;
        Item.AddItem( Inventory.IType.Inventory, ResourceType, ResourceToAddAmount );
    }

    [ButtonGroup( "2", 2 )]
    //[HorizontalGroup( "Split", 0.5f )]
    [Button( "Remove Resource", ButtonSizes.Large ), GUIColor( 1f, 0, 0 )]
    public void RemoveResourceCallBack()
    {
        if( ResourceType == ItemType.NONE )
        {
            Debug.Log( "Choose a resource first." );
            return;
        }
        if( Application.isPlaying == false ) return;
        Item.AddItem( Inventory.IType.Inventory, ResourceType, ResourceToRemoveAmount );
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Hero", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoHeroCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 0 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Map", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoMapCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 1 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Navigation Map", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoNavigationCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 2 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Manager", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoManagerCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 3 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Random Map", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoRandomMapCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 4 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Hero Data", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoHeroDataCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 5 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "BluePrints", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoBlueprintsCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 6 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Buildings", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoBuildingsCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 7 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Farm", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoFarmCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 8 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Dialog", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoDialogCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 9 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Tutorial", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoTutorialCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 10 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Quests Panel", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoQuestsCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 11 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Object Pooling", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoPoolingCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 12 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "UI", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoUICallBack()
    {
        Selection.activeGameObject = ShortcutList[ 13 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Quest", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoQuestCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 14 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Master Audio", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoStatisticsCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 15 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Inventory", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoInventoryCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 16 ];
    }

    [TabGroup( "Other" )]
    public int QuestCopySource = -1;
    [TabGroup( "Other" )]
    public int QuestCopyDestination = -1;


    //[ButtonGroup( "1", 1 )]
    [TabGroup( "Other" )]
    // [HorizontalGroup( "Split", 0.5f )]
    [Button( "Copy Quest", ButtonSizes.Large ), GUIColor( 1f, 0.52f, 0.1f )]
    public void CopyQuestCallBack()
    {
        if( QuestCopyDestination < 0 ) return;
        if( QuestCopySource < 0 ) return;
        Map.I.RM.RMList[ QuestCopyDestination ].Copy( Map.I.RM.RMList[ QuestCopySource ], false );
        Debug.Log( "Quest Copied " + " Source: #" + QuestCopySource + " " + Map.I.RM.RMList[ QuestCopySource ].name + 
        " Destination: #" + QuestCopyDestination + " " + Map.I.RM.RMList[ QuestCopyDestination ].name );
        QuestCopyDestination = -1;
        QuestCopySource = -1;
    }

    #endif
    #endregion

    public string GetModDescriptionText()
    {
        RandomMap rm = GameObject.Find( "----------------Random Map----------------" ).
        GetComponent<RandomMap>();
        MapSaver ms = GameObject.Find( "Areas Template Tilemap" ).
        GetComponent<MapSaver>();
        if( ms.CurrentAdventure < 0 || ms.CurrentAdventure >= rm.RMList.Count ) 
            return "Quest Unavailable";
        string ls = "";
        RandomMapData rmd = rm.RMList[ ms.CurrentAdventure ];
        SectorDefinition[ ] sd = rmd.gameObject.GetComponentsInChildren<SectorDefinition>();
        for( int s = 0; s < sd.Length; s++ )
        {
            for( int m = 0; m < sd[ s ].ModList.Length; m++ )
            {
                if( sd.Length > 1 )
                    ls += "(SD:" + s + ") ";
                ls += sd[ s ].ModList[ m ].name + "\n";
            }
        }
        return ls;
    }
    public string GetOriListText()
    {
        
        return "";// gg
        string str = "";
        MapSaver ms = MapSaver.Get();
        RandomMap rm = GameObject.Find( "----------------Random Map----------------" ).GetComponent<RandomMap>();
        RandomMapData rmd = rm.RMList[ ms.CurrentAdventure ];
        SectorDefinition[] sd = rmd.gameObject.GetComponentsInChildren<SectorDefinition>();
        str += "\n\n";
        for( int s = 0; s < sd.Length; s++ )
        {
            for( int m = 0; m < sd[ s ].ModList.Length; m++ )
            {
                Mod md = sd[ s ].ModList[ m ];
                str += "\nBEGIN_" + md.ModNumber + "\n";
                str += "" + md.name + "\n";
                str += "\n\n";
                if( md.OrientatorEffects != null )
                {
                    for( int i = 0; i < md.OrientatorEffects.Count; i++ )
                    {
                        if( i == 0 ) str += "Effect:\n\n";
                        str += "  " + ( i + 1 ) + "= " + md.OrientatorEffects[ i ] + ",";
                        if( i == 9 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.OrientatorTable1 != null )
                {
                    for( int i = 0; i < md.OrientatorTable1.Count; i++ )
                    {
                        if( i == 0 ) str += "Ori 1: -----" + md.OrientatorEffects[ 0 ] + "-----\n\n";
                        str += "  " + i + "=" + md.OrientatorTable1[ i ] + ",";
                        if( i == 9 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.OrientatorTable2 != null )
                {
                    for( int i = 0; i < md.OrientatorTable2.Count; i++ )
                    {
                        if( i == 0 ) str += "Ori2: -----" + md.OrientatorEffects[ 1 ] + "-----\n\n";
                        str += "  " + i + "=" + md.OrientatorTable2[ i ] + ",";
                        if( i == 9 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.OrientatorTable3 != null )
                {
                    for( int i = 0; i < md.OrientatorTable3.Count; i++ )
                    {
                        if( i == 0 ) str += "Ori3: -----" + md.OrientatorEffects[ 2 ] + "-----\n\n";
                        str += "  " + i + "=" + md.OrientatorTable3[ i ] + ",";
                        if( i == 9 || i == 19 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.OrientatorTable4 != null )
                {
                    for( int i = 0; i < md.OrientatorTable4.Count; i++ )
                    {
                        if( i == 0 ) str += "Ori4: -----" + md.OrientatorEffects[ 3 ] + "-----\n\n";
                        str += "  " + i + "=" + md.OrientatorTable4[ i ] + ",";
                        if( i == 9 || i == 19 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.OrientatorItemTable1 != null )
                {
                    for( int i = 0; i < md.OrientatorItemTable1.Count; i++ )
                    {
                        if( i == 0 ) str += "Item List:\n\n";
                        str += " " + i + "  " + md.OrientatorItemTable1[ i ] + ",";
                        if( i == 9 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.InitialAltarBonusList != null )
                {
                    for( int i = 0; i < md.InitialAltarBonusList.Count; i++ )
                    {
                        if( i == 0 ) str += "Altar Bonus List:\n\n";
                        str += " " + i + "  " + md.InitialAltarBonusList[ i ].AltarBonusType + ",";
                        if( i == 9 ) str += "\n";
                    }
                    str += "\n\n";
                    str += "Scope: ";
                    foreach( string name in Enum.GetNames( typeof( EAltarBonusScope ) ) )
                    {
                        int value = ( int ) Enum.Parse( typeof( EAltarBonusScope ), name );
                        str += "   " + value + "  " + name + "   ";
                    }
                }
                str += "END_" + md.ModNumber + "\n";
            }
        }
        return str;
    }


    // draw an x mark over a target for debug purposes
    public void DrawMark( Vector2 pt )
    {
        Message.CreateMessage( ETileType.NONE, "x", pt, Color.green, true, true, 15, 0, -1 );
    }
    internal void UpdateDebug()
    {
        if( Input.GetKey( KeyCode.LeftShift ) )                                                              // Update language cache
        if( Input.GetKey( KeyCode.F1 ) )
                Language.I.UpdateLanguage();

        if( Input.GetMouseButtonDown( 1 ) )
        if( Input.GetKey( KeyCode.Insert ) == false ) 
        {
            float time = 10;
            if( Input.GetKey( KeyCode.F1 ) )
                time = 1000000;
            Message.CreateMessage( ETileType.NONE, ItemType.NONE, "" + Map.I.Mtx + "," + 
            Map.I.Mty, new Vector2( Map.I.Mtx - .3f, Map.I.Mty ),                                            // right click to show cord
            Color.green, false, false, time, 0, -1, 70 );
        }
        
        //if( G.HS == null || G.HS.Type != Sector.ESectorType.NORMAL ) return;
        if( Manager.I.GugaVersion == false ) return;                                                         // Below this line, Just for me, baby!
        Vector2 tg = new Vector2( Map.I.Mtx, Map.I.Mty );

        if( Input.GetKeyDown( KeyCode.F12 ) )                                                                // F12: play random song 
            Manager.I.PlaylistController.PlayRandomSong();

        if( Input.GetKey( KeyCode.Insert ) )                                                                 // Mini editor
        {
            if( Input.GetMouseButton( 1 ) )
            {
                TKUtil.ClearLayer( tg, ELayerType.GAIA );
                Vector2 mc = GetCubeTile( tg );
                MapSaver.I.Tilemap.SetTile( ( int ) mc.x, ( int ) mc.y, ( int ) ELayerType.GAIA, ( int ) -1 );
                return;
            }

            if( Input.GetKeyDown( KeyCode.Insert ) )
            if( Input.GetMouseButton( 0 ) == false ) 
            {
                 Unit un = Map.I.GetUnit( tg, ELayerType.GAIA );           
                 if( un )
                 {
                     DrawTile = un.TileID;                                                                   // Select gaia tile with insert key
                     Message.CreateMessage( DrawTile, "", tg, Color.white );
                 }
            }

            if( Input.GetMouseButton( 0 ) ) 
            {
                Quest.I.CurLevel.Tilemap.SetTile( Map.I.Mtx, Map.I.Mty, ( int ) ELayerType.GAIA, ( int ) DrawTile );
                Vector2 mc = GetCubeTile( tg );
                MapSaver.I.Tilemap.SetTile( ( int ) mc.x, ( int ) mc.y, ( int ) ELayerType.GAIA, ( int ) DrawTile );
                Map.I.SetTile( Map.I.Mtx, Map.I.Mty, ELayerType.GAIA, DrawTile, true );
                for( int y = ( int ) tg.y - 1; y <= tg.y + 1; y++ )
                for( int x = ( int ) tg.x - 1; x <= tg.x + 1; x++ )
                    Map.I.TransTilemapUpdateList.Add( new VI( x, y ) );
            }

            if( Input.GetMouseButton( 2 ) )
            {
                MapSaver.I.SaveMap( MapSaver.I.LastLoadedFile, ref MapSaver.I.Tilemap );
                Debug.Log( "Saved: " + MapSaver.I.LastLoadedFile );
            }
            return;
        }

        if( Input.GetMouseButtonDown( 2 ) )
            G.Hero.Control.ApplyMove( new Vector2( -1, -1 ), tg );          // mouse click move hero

        if( Input.GetKey( KeyCode.Delete ) )
        {
            if( Input.GetMouseButton( 0 ) )
            {
                Unit un = Map.I.GetUnit( tg, ELayerType.MONSTER );          // localized kill: use mouse button + Delete
                if( un ) Map.Kill( un );
                Unit ga2 = Map.I.GetUnit( tg, ELayerType.GAIA2 );
                if( ga2 ) Map.Kill( ga2 );
                List<Unit> fl = Map.I.GetFUnit( tg );
                if( fl != null )
                for( int i = 0; i < fl.Count; i++ )
                    Map.Kill( fl[ i ] );
                return;
            }

            if( G.HS && G.HS.Type == Sector.ESectorType.NORMAL )
            if( Input.GetMouseButton( 2 ) )
            {
                for( int i = 0; i < G.HS.MoveOrder.Count; i++ )                                                   // Delete: Kill all debug
                if( G.HS.MoveOrder[ i ].ValidMonster )
                    G.HS.MoveOrder[ i ].Kill();
                for( int i = G.HS.Fly.Count - 1; i >= 0; i-- )
                    G.HS.Fly[ i ].Kill();  
            }
        }
    }
}
