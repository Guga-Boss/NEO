using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;
using DarkTonic.MasterAudio;

public class Quest : MonoBehaviour 
{
	public static Quest I;		
	public string QuestName;
	public static int CurrentLevel, CurrentDungeon;
	public Level[] LevelList;
    public Level[] LabList;
    public Level Dungeon, Farm;
    public Level CurLevel;

	public void start()
	{
		I = this;
	}

    public void CreateArtifacts( tk2dTileMap from, Level to, int lNum, int cube, GameObject folder, Vector2 origin, Rect area, Sector s, bool areastm = false )
    {
        tk2dTileMap tm = from;
        for( int i = 0; i < tm.GetTilePrefabsListCount(); i++ )                                                 // Find all artifacts prefabs and clone them
        {
            int x, y;
            int tempLayer = 0;
            GameObject prefabObject;
            tm.GetTilePrefabsListItem( i, out x, out y, out tempLayer, out prefabObject );
                 
            x+= ( int ) origin.x;
            y +=( int ) origin.y;
            
            if( prefabObject != null )
                if( prefabObject.tag == "Artifact" )
                    if( area.Contains( new Vector2( x, y ) ) )
                    {
                        CreateArtifact( prefabObject, to, new Vector2(x,y), s, lNum, cube, folder, areastm );
                    }
        }        
    }

    public Artifact CreateArtifact( GameObject prefab, Level to, Vector2 pos, Sector s, int lNum, int cube, GameObject folder, bool areastm )
    {
        //if( areastm )
        //{
        //    pos.x -= ( int ) Map.I.RM.AreasTMOrigin.x;
        //    pos.y -= ( int ) Map.I.RM.AreasTMOrigin.y;
        //}

        //if( s != null )
        //{
        //    Vector2 mirror = Sector.GetMirrorPos( pos, s );
        //    pos = new Vector( ( int ) mirror.x, ( int ) mirror.y );
        //}

        GameObject instance = ( GameObject ) GameObject.Instantiate( prefab );
        instance.transform.position = new Vector3( pos.x, pos.y, -1 );
        instance.name = "Artifact " + "L" + lNum + " " + pos.x + " " + pos.y;
        if( lNum == -1 ) instance.name = "Artifact " + "Dungeon " + pos.x + " " + pos.y;

        instance.gameObject.SetActive( true );
        instance.transform.parent = folder.transform;

        Artifact ar = instance.GetComponent<Artifact>();
        ar.Pos = new Vector2( pos.x, pos.y );
        ar.BelongingLevel = lNum;
        ar.BelongingCube = cube;

        if( ar.LifeTime == Artifact.EArtifactLifeTime.SESSION )
            instance.transform.localScale = new Vector3( 0.25f, 0.3f, 1f );

        if( ar.LifeTime == Artifact.EArtifactLifeTime.GAME )
            instance.transform.localScale = new Vector3( 0.35f, 0.4f, 1f );

        to.ArtifactList.Add( ar );
        return ar;
    }

    public void CreateGameObjects()
    {
        return; // old
        for( int l = 0; l < LevelList.Length; l++ )
        {
            bool old = LevelList[ l ].gameObject.activeSelf;                     // Initialize the AreaList array
            LevelList[ l ].gameObject.SetActive( true );            

            Area[] arl = Quest.I.LevelList[ l ].AreaFolder.transform.GetComponentsInChildren<Area>();
            for( int i = 0; i < arl.Length; i++ ) LevelList[ l ].AreaList.Add( arl[ i ] );
            
            LevelList[ l ].gameObject.SetActive( old );

            GameObject levelfolder = LevelList[ l ].gameObject;
            levelfolder.transform.parent = gameObject.transform;
            Rect r = new Rect( 0, 0, Map.I.Tilemap.width, Map.I.Tilemap.height );
            CreateArtifacts( LevelList[ l ].Tilemap, LevelList[ l ], l, -1, LevelList[ l ].ArtifactFolder, new Vector2( 0, 0 ), r, null );
        }
    }

	public void Init () 
	{
		I = this;
		CreateGameObjects();
	}

	//_____________________________________________________________________________________________________________________ Init Artifacts
		
	public void InitArtifacts()
	{
        //Artifact[] artifacts = Map.I.Tilemap.renderData.transform.GetComponentsInChildren<Artifact>();      // Deactivate map play artifacts

        //for( int i = 0; i < artifacts.Length; i++ ) 
        //{
        // if( artifacts[ i ].gameObject.tag == "Artifact" )     
        //     artifacts[ i ].gameObject.SetActive( false );               // be careful with bugs
        //}

        //for( int l = 0; l < LevelList.Length; l++ )
        //for( int a = 0; a < LevelList[ l ].ArtifactList.Count; a++ )
        //{
        //    LevelList[ l ].ArtifactList[ a ].Collected = Artifact.EStatus.NOT_COLLECTED;
        //    LevelList[ l ].ArtifactList[ a ].gameObject.SetActive( false );

        //    if( l < CurrentLevel ) 
        //    {
        //        if( LevelList[ l ].ArtifactList[ a ].LifeTime == Artifact.EArtifactLifeTime.GAME )
        //        LevelList[ l ].ArtifactList[ a ].Collected = Artifact.EStatus.COLLECTED; 
        //    }
        //    else
        //    if( l == CurrentLevel ) 
        //    {
        //        if( Map.I.Revealed[ ( int ) LevelList[ l ].ArtifactList[ a ].Pos.x,
        //                            ( int ) LevelList[ l ].ArtifactList[ a ].Pos.y ] )
        //        LevelList[ l ].ArtifactList[ a ].gameObject.SetActive( true );
        //    }
        //}
	}

    //_____________________________________________________________________________________________________________________ Update Artifact stepping

    public bool UpdateArtifactStepping( Vector2 pos )
    {
        if( Map.I.RM.GameOver ) return false;
        if( Map.PtOnMap( Map.I.Tilemap, pos ) == false ) return false;
        ETileType tile = ( ETileType ) CurLevel.Tilemap.GetTile( ( int ) pos.x,
                                                        ( int ) pos.y, ( int ) ELayerType.GAIA2 );

        if( tile != ETileType.ARTIFACT ) return false;
        if( Map.I.HeroIsDead ) return false;

        Artifact ar = GetArtifactInPos( pos );

        if( ar != null )
        {
            if( Map.I.Hero.CheckLevelLimits( ar.PerkType ) )
            {
                UI.I.MessageBoxTextLabel2.text = "Level Limit Reached!";
                return false;
            }
            HeroData.I.AddCollectedArtifact( pos, ar.LifeTime );
            if( Map.I.CurrentArea == -1 )
            {
                ar.Collected = Artifact.EStatus.COLLECTED;                                                                                              // Collect artifact
                ar.TimesBought++;
            }
            else ar.Collected = Artifact.EStatus.TEMPORARY_COLLECTED;

            Map.I.Hero.Control.PathFinding.Path.Clear();

            ar.gameObject.SetActive( false );                                                                                                           // Disable sprite

            UpdateArtifactData( ref Map.I.Hero, true );

            UI.I.SelectTempPerk( ar );

            UI.I.MessageBox.SetActive( true );
            if( Manager.I.GameType == EGameType.CUBES ) UI.I.MessageBox.SetActive( false );

            UI.I.MessageBoxTextLabel.text = "" + ar.ArtifactName +
                "\nAction: " + ar.ActionDescription + "\n\nLifetime: " + ar.LifeTime;                                                                  // Display Message
            if( ar.TargetUnitName ) UI.I.MessageBoxTextLabel.text += "\nTarget Hero: " + ar.TargetUnitName.text;

            UI.I.MessageBoxIcon.sprite2D = UI.I.PerkList[ ( int ) ar.PerkType ].Sprite[ 0 ].sprite2D;
            UI.I.ScrollText.gameObject.SetActive( false );
            UI.I.ScrollText.text = "";
            
            int levelFactor = -1;
            string basename = "";
            string addDesc = "";
            string iconText = "";
             
            Unit un = Map.I.Hero;
            if( ar.TargetHero != EHeroID.ALL_HEROES && Map.I.Hero.Variation != ( int ) ar.TargetHero )
            {
                un = UI.I.CompareHero;
                UI.I.CompareHero.Copy( Map.I.HeroList[ ( int ) ar.TargetHero ], false, false, false );
                Quest.I.UpdateArtifactData( ref un );
            }

            UI.I.GetPerkInfo( ar.PerkType, ref levelFactor, ref basename, ref addDesc, un, ref iconText, false );

            UI.I.MessageBoxTextLabel2.text  = "" + Manager.I.Translate( "&" + basename + "_" + levelFactor + "_TITLE", "Perks" ) + "\n\n";
            UI.I.MessageBoxTextLabel2.text += "" + Manager.I.Translate( "&" + basename + "_" + levelFactor + "_DESCRIPTION", "Perks" );
            UI.I.MessageBoxTextLabel2.text += "\n" + addDesc;
            UI.I.MessageBoxLevelLabel.text = "Level " + levelFactor;
            if( ar.PerkType == EPerkType.STAR )
                UI.I.MessageBoxLevelLabel.text = "Level " + Map.I.Hero.Body.Level;
            if( ar.PerkType == EPerkType.SCOUT )
                UI.I.MessageBoxLevelLabel.text = "Level " + Map.I.Hero.Control.ScoutLevel;

            int fact = levelFactor;
            if( fact < 0 ) fact = 0;
            int iconID = UI.I.GetIconID( fact, ar.PerkType );

            if( UI.I.PerkList[ ( int ) ar.PerkType ].Sprite[ iconID ] != null )
                UI.I.MessageBoxIcon2.sprite2D = UI.I.PerkList[ ( int ) ar.PerkType ].Sprite[ iconID ].sprite2D;

            MasterAudio.PlaySound3DAtVector3( "Artifact collected", transform.position );
            UI.I.TurnInfoLabel.text = "Artifact Found: " + ar.ArtifactName + " " + UI.I.MessageBoxLevelLabel.text + "\nBonus Granted: " + UI.I.MessageBoxTextLabel2.text;
            Map.I.RM.UpdateSectorHint();
            Controller.CreateMagicEffect( pos );
            Map.I.InvalidateInputTimer = .18f;  // To avoid double step bug
        }
        return true;
    }

    public void UpdateArtifactPrices( bool force = false )
    {
        if( force == false )
        {
            if( Map.I.AdvanceTurn == false ) return;
            if( Map.I.CurrentArea != -1 ) return;
            if( Map.I.AreaExitTurnCount > 2 ) return;
        }

        for( int a = 0; a < Quest.I.CurLevel.ArtifactList.Count; a++ )
        {
            Artifact ar = Quest.I.CurLevel.ArtifactList[ a ];
            if( ar && ar.Recurring )
            {
                UpdateArtifactPrice( ar );
            }           
        }
    }

    public void ReviveRecurringArtifacts()
    {
        if( Map.I.AdvanceTurn == false ) return;
        if( Map.I.CurrentArea != -1 ) return;
        
        for( int d = 0; d < 8; d++ )
        {
            Vector2 tg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) d ];
            if( Map.PtOnMap( Map.I.Tilemap, tg ) )
            {
                Artifact ar = GetArtifactInPos( tg, true );
                if( ar )
                if( ar.Collected == Artifact.EStatus.COLLECTED && ar.Recurring )
                {
                    ar.gameObject.SetActive( true );
                    ar.Collected = Artifact.EStatus.RECURRING;
                    UpdateArtifactPrice( ar );
                }
            }
        }
    }


    public void UpdateArtifactPrice( Artifact ar )
    {
        ETileType tile = ( ETileType ) CurLevel.Tilemap.GetTile( ( int ) ar.Pos.x,
                                   ( int ) ar.Pos.y, ( int ) ELayerType.MONSTER );

        if( tile == ETileType.DOME )
        {
            Map.I.UpdateTileGameObjectCreation( ( int ) ar.Pos.x, ( int ) ar.Pos.y, ELayerType.MONSTER );
            if( Map.I.Unit[ ( int ) ar.Pos.x, ( int ) ar.Pos.y ] )
            if( Map.I.Unit[ ( int ) ar.Pos.x, ( int ) ar.Pos.y ].TileID == ETileType.DOME )
                {
                    float price = ar.CalculatePrice();
                    Map.I.Unit[ ( int ) ar.Pos.x, ( int ) ar.Pos.y ].UpdateDomePrice( price );
                }
        }
    }

	//_____________________________________________________________________________________________________________________ Update Artifact Mouse Over Info

	public bool UpdateArtifactMouseOverInfo( Vector2 pos )
	{
		if( Manager.I.Status != EGameStatus.PLAYING ) return false;
        if( Map.I.RM.GameOver ) return false;
		if( Map.I == null ) return false;

        if( UI.I.MessageBox.activeSelf )
        {
            UI.I.ScrollText.gameObject.SetActive( false );
            UI.I.ScrollText.text = "";
            return false;
        }

        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
        {
            UI.I.ScrollText.gameObject.SetActive( false );
            UI.I.ScrollText.text = "";
            return false;
        }

        if( Map.PtOnMap( Map.I.Tilemap, pos ) == false ) return false;

        ETileType tile = ( ETileType ) CurLevel.Tilemap.GetTile( ( int ) pos.x,
														( int ) pos.y, ( int ) ELayerType.GAIA2 );

		if( !Map.I.Gaia2[ ( int ) Map.I.Hero.Pos.x, ( int ) Map.I.Hero.Pos.y ] ||
			 Map.I.Gaia2[ ( int ) Map.I.Hero.Pos.x, ( int ) Map.I.Hero.Pos.y ].TileID != ETileType.SCROLL )
             UI.I.ScrollText.gameObject.SetActive( false );
        
		if( tile != ETileType.ARTIFACT ) return false;

        int levelFactor = -1;
        string basename = "";
        string addDesc = "";
        string iconText = "";

        Unit un = Map.I.Hero;

        Artifact ar = GetArtifactInPos( pos );
        if( ar == null ) return false;

        UI.I.MouseArtifact = true;
        UI.I.GetPerkInfo( ar.PerkType, ref levelFactor, ref basename, ref addDesc, un, ref iconText, false );

        string hero = "\nTarget Hero: ";

        if( ar.TargetUnitName )
            hero += ar.TargetUnitName.text;

        if( Manager.I.GameType == EGameType.CUBES ) // Dungeon
        {
            hero = "\nMultiplier: " + ar.Multiplier;
            if( ar.Multiplier == Artifact.EMultiplier.x1 ) hero = "";
        }

        UI.I.ScrollText.gameObject.SetActive( true );
        UI.I.ScrollText.color = new Color( 1, 1, 1, 1 );
        UI.I.ScrollText.text = "Artifact: " + ar.ArtifactName + "\nAction: " + ar.ActionDescription + "\n\n\n\n\n\nLifetime: " + ar.LifeTime + hero;
        return true;
	}
    
    public Artifact GetArtifactInPos( Vector2 pos, bool collected = false )
    {
        for( int a = 0; a < CurLevel.ArtifactList.Count; a++ )
        {
            if( CurLevel.ArtifactList[ a ].Pos == pos )
                if( CurLevel.ArtifactList[ a ].BelongingLevel == CurrentLevel )
                    if( CurLevel.ArtifactList[ a ].Collected == Artifact.EStatus.NOT_COLLECTED || collected == true ||
                        CurLevel.ArtifactList[ a ].Collected == Artifact.EStatus.RECURRING )
                    {
                        return CurLevel.ArtifactList[ a ];
                    }
        }
        return null;
    }

	//_____________________________________________________________________________________________________________________ Update Artifact Data
	
	
	public void UpdateArtifactData( ref Unit _unit, bool justStepped = false )
	{
        float oldTotHP = _unit.Body.TotHp;

        _unit.Copy( Map.I.HeroList[ _unit.Variation ], false, false, false );

        _unit.Body.TotHp += Map.I.ExtraHeroHP;

        if( Manager.I.GameType != EGameType.FARM )
        {
            if( Quest.CurrentLevel != -1 )
            {
                for( int l = 0; l < LevelList.Length; l++ )
                    for( int a = 0; a < LevelList[ l ].ArtifactList.Count; a++ )
                    {
                        if( LevelList[ l ].ArtifactList[ a ].Collected == Artifact.EStatus.COLLECTED ||
                            LevelList[ l ].ArtifactList[ a ].Collected == Artifact.EStatus.TEMPORARY_COLLECTED ||
                            LevelList[ l ].ArtifactList[ a ].Collected == Artifact.EStatus.RECURRING )
                            LevelList[ l ].ArtifactList[ a ].Apply( ref _unit );
                    }
            }
            else
            {
                for( int a = 0; a < Quest.I.Dungeon.ArtifactList.Count; a++ )
                {
                    if( Quest.I.Dungeon.ArtifactList[ a ].LifeTime == Artifact.EArtifactLifeTime.CUBE &&               // Cube Restricted Lifetime Artifact
                        Quest.I.Dungeon.ArtifactList[ a ].BelongingCube != Map.I.RM.HeroSector.Number ) { }
                    else
                        if( Quest.I.Dungeon.ArtifactList[ a ].Collected == Artifact.EStatus.COLLECTED ||
                            Quest.I.Dungeon.ArtifactList[ a ].Collected == Artifact.EStatus.TEMPORARY_COLLECTED ||
                            Quest.I.Dungeon.ArtifactList[ a ].Collected == Artifact.EStatus.RECURRING )
                            Quest.I.Dungeon.ArtifactList[ a ].Apply( ref _unit );
                }
            }
        }
            
        _unit.Control.MovementLevel += Map.I.RM.RMD.BaseMovementLevel;
        _unit.Body.Stars += HeroData.I.InitialStars;
        _unit.Body.RangedAttackLevel += Map.I.RM.RMD.BaseHeroRangedAttackLevel;
        _unit.Body.MeleeAttackLevel += Map.I.RM.RMD.BaseHeroMeleeAttackLevel;
        _unit.Control.ArrowInLevel += Map.I.RM.RMD.BaseArrowInLevel;
        _unit.Control.ArrowOutLevel += Map.I.RM.RMD.BaseArrowOutLevel;
        _unit.Control.PlatformWalkingLevel += Map.I.RM.RMD.BasePlatformWalkingLevel;
        _unit.Control.PlatformSteps += Map.I.RM.RMD.BasePlatformSteps;
        _unit.Body.FireMasterLevel += Map.I.RM.RMD.BaseFireMasterLevel;
        _unit.Body.DestroyBarricadeLevel += Map.I.RM.RMD.BaseDestroyBarricadeLevel;
        _unit.Control.ScoutLevel += Map.I.RM.RMD.BaseScoutLevel;
        _unit.Body.MeleeShieldLevel += Map.I.RM.RMD.BaseHeroMeleeShieldLevel;
        _unit.Body.MissileShieldLevel += Map.I.RM.RMD.BaseHeroRangedShieldLevel;
        _unit.Body.MiningLevel += Map.I.RM.RMD.BaseMiningLevel;
		_unit.UpdateLevelingData();

        if( justStepped )
            _unit.Body.Hp += _unit.Body.TotHp - oldTotHP;  // refils hp in case of Tot Hp increase
	}

	//_____________________________________________________________________________________________________________________ InitArtifactsAfterLoading() - Called after Load

    public void InitArtifactsAfterLoading()
    {
        InitArtifacts();
        ClearArtifactsThatHaveBeenCollected();
		UpdateArtifactData( ref Map.I.Hero );
    }

	//_____________________________________________________________________________________________________________________ ClearArtifactsThatHaveBeenCollected
	
	public void ClearArtifactsThatHaveBeenCollected()
	{
        for( int a = 0; a < CurLevel.ArtifactList.Count; a++ )
        {
            for( int c = 0; c < HeroData.I.CollectedAreaArtifacts.Count; c++ )
                if( HeroData.I.CollectedAreaArtifacts[ c ] == CurLevel.ArtifactList[ a ].Pos )
                {
                    CurLevel.ArtifactList[ a ].gameObject.SetActive( false );
					CurLevel.ArtifactList[ a ].Collected = Artifact.EStatus.COLLECTED;
                }

            for( int c = 0; c < HeroData.I.CollectedLevelArtifacts.Count; c++ )
                if( HeroData.I.CollectedLevelArtifacts[ c ] == CurLevel.ArtifactList[ a ].Pos )
                {
                    CurLevel.ArtifactList[ a ].gameObject.SetActive( false );
					CurLevel.ArtifactList[ a ].Collected = Artifact.EStatus.COLLECTED;
                }

            for( int c = 0; c < HeroData.I.CollectedGameArtifacts.Count; c++ )
                if( HeroData.I.CollectedGameArtifacts[ c ] == CurLevel.ArtifactList[ a ].Pos )
                {
                    CurLevel.ArtifactList[ a ].gameObject.SetActive( false );
					CurLevel.ArtifactList[ a ].Collected = Artifact.EStatus.COLLECTED;
                }

            //UpdateArtifactData();
        }
	}
    public void SaveArtifacts()
    {
        string fname = "C://Users//Guga//Desktop//Artifacts.dat";

        for( int l = 0; l < LevelList.Length; l++ )
            for( int a = 0; a < LevelList[ l ].ArtifactList.Count; a++ )
            {
                ES2.Save( LevelList[ l ].ArtifactList[ a ].PrefabName, fname + "?tag=PrefabName " + l + "  " + a );
                ES2.Save( LevelList[ l ].ArtifactList[ a ].Pos,        fname + "?tag=Pos "        + l + "  " + a );
                ES2.Save( LevelList[ l ].ArtifactList[ a ].TargetHero, fname + "?tag=TargetHero " + l + "  " + a );
                ES2.Save( LevelList[ l ].ArtifactList[ a ].LifeTime,   fname + "?tag=LifeTime "   + l + "  " + a );
            }
        Debug.Log("Artifacts Saved");
    }

    public void LoadArtifacts()
    {
        string fname = "C://Users//Guga//Desktop//Artifacts.dat";

        for( int l = 0; l < LevelList.Length; l++ )
            for( int a = 0; a < LevelList[ l ].ArtifactList.Count; a++ )
            {
                GameObject tmo = GameObject.Find("Level " + l + " TileMap");
                
                if(tmo == null)
                {
                    Debug.Log( " Tilemap not found: " + l );
                    return;
                }

                tk2dTileMap tm = tmo.GetComponent<tk2dTileMap>();

                LevelList[ l ].ArtifactList[ a ].PrefabName = ES2.Load<string>( fname + "?tag=PrefabName " + l + "  " + a );

                GameObject res = ( GameObject ) Resources.Load( "Artifacts/" + LevelList[ l ].ArtifactList[ a ].PrefabName );
                Artifact from = res.GetComponent<Artifact>();

                if( res )
                {
                    Debug.Log( "Artifacts/" + LevelList[ l ].ArtifactList[ a ].PrefabName + " Loaded." );


                    bool ok = false;


                    for( int i = 0; i < tm.GetTilePrefabsListCount(); i++ )                                                 // Find all artifacts prefabs and clone them
                    {
                        int tempLayer = 0;
                        GameObject prefabObject;
                        int x, y;
                        tm.GetTilePrefabsListItem( i, out x, out y, out tempLayer, out prefabObject );
                        Vector2 pos = new Vector2( x, y );
                        if( prefabObject != null )
                            if( prefabObject.tag == "Artifact" )
                            {
                                Vector2 posi = ES2.Load<Vector2>( fname + "?tag=Pos " + l + "  " + a );

                                if( posi.x == pos.x )
                                if( posi.y == pos.y )
                                {
                                    Artifact ar2 = res.GetComponent<Artifact>();
                                    ar2.Copy( from );

                                    ar2.TargetHero = ES2.Load<EHeroID>( fname + "?tag=TargetHero " + l + "  " + a );
                                    ar2.LifeTime = ES2.Load<Artifact.EArtifactLifeTime>( fname + "?tag=LifeTime " + l + "  " + a );
                                    //LevelList[ l ].ArtifactList[ a ].TargetUnitName = LevelList[ l ].ArtifactList[ a ].transform.GetComponentInChildren<tk2dTextMesh>();
                                    ok = true;
                                }
                            }
                    }

                    if( !ok ) Debug.Log("Artifact not found L" + l  );

                }
                else Debug.Log( "Artifacts/" + LevelList[ l ].ArtifactList[ a ].PrefabName + " Could not be found." );

            }
        Debug.Log( "Artifacts Loaded" );
    }
}
