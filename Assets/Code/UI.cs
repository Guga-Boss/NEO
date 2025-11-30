using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;

public class UI : MonoBehaviour
{
    #region Variables
    public static UI I;
    public UILabel HeroHpText, LevelText, EnterAreaTxt, AreasText, ArtifactsText, GameLevelText, 
        TurnInfoLabel, MapLabel, UseButtonLabel, LosttHPLabel, BigTextHelpLabel, BigMessageText, QuestCompletitionLabel,
        ArtifactInfoLabel, NaviBronzeLabel, NaviSilverLabel, NaviGoldLabel, NavigationMapText;
    public int TurnInfoLabelCount = 0;
	public UISlider UnitHealthBar, QuestCompletitionBar;
    public UI2DSprite PerkInfoIcon, HorseIcon, MessageBoxIcon, MessageBoxIcon2, OverlaySprite, BackgroundUI;
    public GameObject PerkInfoIconBack, UIFolder;
	public UI2DSprite[] Stars;
    public UILabel PerkInfoTitleText, PerkInfoDescriptionText, PerkInfoTargetHeroText, ScrollText, FreeCamModeLabel, ErrorMessageLabel, 
                   DebugLabel, MessageBoxTextLabel, MessageBoxTextLabel2, MessageBoxLevelLabel, PerkInfoLevelLabel;
    public UIButton RepeatButton, ZoomButton, FreeCamButton, ArtifactSeekButton, AreaSeekButton, GridButton, CompareHeroButton, 
        KadeWaitButton, RestartAreaButton, BattleButton, UseAbilityButton, PerkInfoButton;
    public List<Perk> PerkList;
    public GameObject PerkModel, PerkInfoPanel, PerksFolder, PerksListFolder, SkullImage, Menu, HpPanel, PerksPanel, MessageBox, FarmUI, InventoryTarget, NavigationMapUI;
    public int NumGamePerk, NumLevelPerk, NumAreaPerk, LevelFactor;
    public Perk MushroomPerk, DestroyBarricadePerk, MonsterPushPerk, MonsterPressurePerk;
	public Unit SelUnit, CompareHero;
	public UIGrid Grid, ResourcesGrid;
    public UISprite ScrollBack;
	public UI2DSprite InfoIcon;
	public float TempSelectedPerkTimer, RKeyPressTimeCount;
	public EPerkType TempSelectedPerk, SelectedPerk;
	public Vector2 LockedTile;
	public UI2DSprite[] PortraitList;
	public int ActiveCompareHeroID, ArtifactLevelDifference;
    public tk2dTextMesh LoadingLevelText;
    public string AttackDescription, AttackType, AmbushInfo, CorneringInfo, SurplusInfo, SprinterInfo;
    public float ShieldReduction, Shield, AmbushBonus, SurplusBonus, ConeringBonus, SprinterBonus;
    public int MessageTurn, OriginalLevel, MouseArtifactMult;
    public bool ConsiderArtifactLevelDifference, ShowMainArtifacts, ShowSecondaryArtifacts, MouseArtifact, ForceUpdateUI;
    public MyPathfinding PathFind;
    public string[] OrdinalNumberList;
    public GridHelper GridHelper;
    public EPerkType CurrentPerkType;
    public List<ResourceIndicator> ResourceIndicators;
    public bool MouseOverUI = false;
    public float BigMessageTextTimeCounter = 0, BigMessageTextFadeTime = 3, OverlayFadeFactor = 1.5f;
    public int BigMessageTextTurnCounter = 0;
    public string QuestCompletitionText;
    public RandomMapObjectivePanel GoalPanel;
    public bool UpdBeastText, UpdGoalText;
    public EPerkType LastClickedPerk = EPerkType.MOVEMENTLEVEL;
    #endregion

    // Use this for initialization
    void Start()
    {
        I = this;
        SelectedPerk = EPerkType.MOVEMENTLEVEL;
        PerkModel.SetActive( false );
		NumGamePerk = NumLevelPerk = NumAreaPerk = 0; 
        TempSelectedPerkTimer = 0;
        MouseArtifactMult = 0;
		TempSelectedPerk = EPerkType.NONE;
		LockedTile = new Vector2( -1, -1 );
		ActiveCompareHeroID = ( int ) EHeroID.NONE;
        ShowMainArtifacts = true;
        ShowSecondaryArtifacts = true;
        BigMessageTextTimeCounter = 0;
        BigMessageTextTurnCounter = 0;
        MouseArtifact = false;
        UpdateAllTranslations();
    }
    public void InitGame()
    {
        PerkInfoIconBack.gameObject.SetActive( true );
        TurnInfoLabel.text = "";
        MessageTurn = 0;
        UpdBeastText = UpdGoalText = true;
        GoalPanel.gameObject.SetActive( true );
        EnableOverlay( new Color32( 114, 114, 255, 180 ), .25f );
        BigMessageText.gameObject.SetActive( false );
        BigMessageTextTimeCounter = 0;
    }
    public void UpdateIt()
    {
        ResourceIndicator.UpdateIt();

        if( Manager.I.Status != EGameStatus.PLAYING ) return;
        if( Map.I.Unit == null ) return;

        UpdateBigMessage();                                                                               // Big message update   
        UpdateMessages();

        if( ForceUpdateUI == false )
        if( Input.GetMouseButton( 0 ) == false ) 
        if( G.HS == null || G.HS.CubeFrameCount >= 3 )
        if( Map.I.TurnFrameCount != 3 ) return;                                     // Optimization Done: called once per turn, in the beginning or if forced or mouse clicked
        ForceUpdateUI = false;

        UpdateUnitUI();
        UpdateUI();
        UpdateInfoPanel();
        Map.I.NavigationMap.UpdateUI();
    }
    public int GetPerkLevelLimit( EPerkType pk, int level )
    {
        if( level >= UI.I.PerkList[ ( int ) pk ].Sprite.Length )
        {
            level =  UI.I.PerkList[ ( int ) pk ].Sprite.Length - 1;
        }
        return level;
    }

    public int GetPerkMaxLevel( EPerkType pk )
    {
        if( pk == EPerkType.NONE ) return 0;
            return UI.I.PerkList[ ( int ) pk ].Sprite.Length - 1;  
    }

    public float CheckLevel( float val )
    {
        if( MouseArtifact )                                                    // Mouse over artifact Level + 1
        {
            val += 1;
            int lev = ( int ) Mathf.Ceil( val );
            float lim = GetPerkLevelLimit( CurrentPerkType, lev );
            if( val > lim ) val = lim;
            return val;
        }
        
        if( val <= 0 ) return 0;
        float res = ( int ) val;

        if( ConsiderArtifactLevelDifference ) res -= ArtifactLevelDifference;

        if( res < 1 )
        {
            res = 1;
            if( ConsiderArtifactLevelDifference ) ArtifactLevelDifference = 0;
        }
        return res;
    }


    public void UpdateInfoPanel( bool force = false )
	{
        //if( force == false )
        //if( Map.I.AdvanceTurn == false )                       // optimization. check for bugs
        //if( Input.GetMouseButton( 0 ) == false )
        //    return;

        if( Map.I == null ) return;
        if( MessageBox.activeSelf )
            if( Input.GetKey( KeyCode.Return ) || Input.GetKey( KeyCode.X ) )
                MessageBox.SetActive( false );

        Unit un = Map.I.Hero;
		if( SelUnit != null ) un = SelUnit;

		int sel = -1;

		for( int i = 0; i < PerkList.Count; i++ )
        if( PerkList[ i ] )
		{
            if( PerkList[ i ].Button.state == UIButtonColor.State.Pressed )
			{
				SelectedPerk = ( EPerkType ) i;
                LastClickedPerk = SelectedPerk;
                PerkInfoIconBack.gameObject.SetActive( true );
                PerkInfoDescriptionText.transform.localPosition = new Vector3( -118, -113f, 0f );
                PerkInfoDescriptionText.width = 314;
                PerkInfoDescriptionText.width = 148;
                PerkInfoDescriptionText.color = Color.white;
                ArtifactLevelDifference = 0;
			}

			if( PerkList[ i ].Button.state == UIButtonColor.State.Hover   ) sel = i;
        }
        
		if( sel == -1 ) sel = ( int ) SelectedPerk;

		if( TempSelectedPerk != EPerkType.NONE )
		{
			TempSelectedPerkTimer -= Time.deltaTime;
			if( TempSelectedPerkTimer < 0 ) TempSelectedPerk = EPerkType.NONE;
            sel = ( int ) TempSelectedPerk;
		}

        LevelFactor = -1;
        string basename = "";
        string addDesc = "";
        string iconText = "";
        MouseArtifact = false;
        MouseArtifactMult = 1;

		for( int i = 0; i < PerkList.Count; i++ )
        if( PerkList[ i ] )
		{
            LevelFactor = -1;
            iconText = "";
            GetPerkInfo( ( EPerkType ) i, ref LevelFactor, ref basename, ref addDesc, un, ref iconText, false );

            if( PerkList[ i ].Category == Artifact.ECategory.NORMAL && ShowMainArtifacts == false ) LevelFactor = -1;
            if( PerkList[ i ].Category == Artifact.ECategory.SECONDARY && ShowSecondaryArtifacts == false ) LevelFactor = -1;

            PerkList[ i ].Label.text = iconText;

            int iconID = GetIconID( LevelFactor, ( EPerkType ) i );

            for( int s = 0; s < PerkList[ i ].Sprite.Length; s++ )
            {
                if( PerkList[ i ].Sprite[ s ] != null )                                                                      // Changes Perk Panel sprite
                    PerkList[ i ].Sprite[ s ].gameObject.SetActive( false );
            }

            if( LevelFactor != -1)
            if( PerkList[ i ].Sprite[ iconID ] != null )
            {
                PerkList[ i ].Sprite[ iconID ].gameObject.SetActive( true );                                                 // Changes info icon sprite
            }
            
            if( LevelFactor > 0 && PerkList[ i ].ActivateIcon )
                PerkList[ i ].gameObject.SetActive( true );                                                                  // Activates or deactivates icon
            else
                PerkList[ i ].gameObject.SetActive( false );
		}

        //PerkList[ ( int ) EPerkType.SCOUT ].gameObject.SetActive( false ); 

        Vector2 mp = new Vector2( Map.I.Mtx, Map.I.Mty );                                                                    // Mouse over artifact

        Unit arti = null;
        Map.I.GetUnit( ETileType.DOME, Map.I.Hero.GetFront() );
        if( arti != null ) mp = Map.I.Hero.GetFront();

        if( Map.PtOnMap( Map.I.Tilemap, mp ) )
        {
            ETileType tile = ETileType.NONE;
            if( Map.I.RM.DungeonDialog.gameObject.activeSelf == false )
                if( Manager.I.GameType == EGameType.CUBES )
                    tile = ( ETileType ) Quest.I.CurLevel.Tilemap.GetTile( ( int ) mp.x,
                                               ( int ) mp.y, ( int ) ELayerType.GAIA2 );

            if( tile == ETileType.ARTIFACT )
            {
                Artifact ar = Quest.I.GetArtifactInPos( mp );
                if( ar != null )
                {
                    SelectedPerk = ar.PerkType;
                    MouseArtifact = true;
                    MouseArtifactMult = ( int ) ar.Multiplier;
                }
            }
        }

        if( Map.I.SeekArtifact ) 
        {
            SelectedPerk = Map.I.SeekArtifact.PerkType;
            MouseArtifact = true;
        }

        addDesc = "";
        GetPerkInfo( SelectedPerk, ref LevelFactor, ref basename, ref addDesc, un, ref iconText, true );

        if( LevelFactor < 1 )
        {
            //SelectedPerk = EPerkType.MOVEMENTLEVEL;
            //if( SelUnit.TileID == ETileType.BARRICADE ) SelectedPerk = EPerkType.BARRICADE;
        }

        OriginalLevel = LevelFactor + ArtifactLevelDifference;                                                                         // Pg up and down to check other Perk Levels

        bool up = false;
        bool down = false;

        if( Input.GetKeyDown( KeyCode.PageUp   ) ) up   = true;
        if( Input.GetMouseButtonDown( 0        ) ) up   = true;
        if( Input.GetKeyDown( KeyCode.PageDown ) ) down = true;
        if( Input.GetMouseButtonDown( 1        ) ) down = true;


        float number = Input.GetAxis( "Mouse ScrollWheel" );

        if( number > 0 ) up = true;
        if( number < 0 ) down = true;

        if( MouseOverUI )
        {
            if( down )
            {
                if( ArtifactLevelDifference < OriginalLevel - 1 )
                    ArtifactLevelDifference++;
            }
            if( up ) ArtifactLevelDifference--;
        }

        if( ArtifactLevelDifference < 0 ) ArtifactLevelDifference = 0;

        if( SelectedPerk != EPerkType.NONE )
        {
            if( LevelFactor != -1 )
            {
                UpdateBigTextHelp( basename );
                int lim = GetPerkMaxLevel( CurrentPerkType );
                float perc = 100 * LevelFactor / lim;
                float id = Util.Percent( perc, 20 );
                int level = Mathf.RoundToInt( id );
                //string skill =  "";
                //if( PerkList[ ( int ) SelectedPerk ].ShowSkill )                                                        // old "apprentice" or "rookie" introduction 
                //    skill = Manager.I.Translate( "&SKILL_LEVEL_" + level, "Main" );  
                //PerkInfoTitleText.text = skill + " " + Manager.I.Translate("&" + basename + "_TITLE", "Perks");                      
                PerkInfoTitleText.text = Manager.I.Translate( "&" + basename + "_TITLE", "Perks" );                       // Load title text
                
                PerkInfoDescriptionText.text = Manager.I.Translate( "&" + basename + "_" + LevelFactor, "Perks" );                     // Load description text

                if( PerkList[ ( int ) SelectedPerk ].UseGoogle == false ) PerkInfoDescriptionText.text = ""; ///// ********* temp

                PerkInfoDescriptionText.text += addDesc;

                if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
                if( Map.I.RM.GameOver ) PerkInfoDescriptionText.text = "";

                string lev = "";
                if( ArtifactLevelDifference > 0 ) lev = "\n(-" + ArtifactLevelDifference + ")";

                PerkInfoLevelLabel.text = "Level " + LevelFactor;
                if( PerkList[ ( int ) SelectedPerk ].Type == EPerkType.STAR )
                    PerkInfoLevelLabel.text = "Level " + Map.I.Hero.Body.Level;
                if( PerkList[ ( int ) SelectedPerk ].Type == EPerkType.SCOUT )
                    PerkInfoLevelLabel.text = "Level " + Map.I.Hero.Control.ScoutLevel;
                if( PerkList[ ( int ) SelectedPerk ].Type == EPerkType.BARRICADE )
                    PerkInfoLevelLabel.text = "Level " + ( un.Variation + 1 );
                PerkInfoLevelLabel.text += lev;
                

                if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
                if( Map.I.RM.GameOver )
                    {
                        PerkInfoLevelLabel.text = "";
                        PerkInfoIconBack.gameObject.SetActive( false );
                        ArtifactInfoLabel.text = "";
                        PerkInfoTitleText.text = "";
                    }

                if( SelectedPerk == EPerkType.NONE ) PerkInfoIconBack.gameObject.SetActive( false );
                int iconID = GetIconID( LevelFactor, SelectedPerk );

                if( PerkList[ ( int ) SelectedPerk ].Sprite[ iconID ] != null )
                    InfoIcon.sprite2D = PerkList[ ( int ) SelectedPerk ].Sprite[ iconID ].sprite2D;

                if( PerkList[ ( int ) SelectedPerk ].Sprite[ iconID ] != null )
                {
                    PerkList[ ( int ) SelectedPerk ].Sprite[ iconID ].gameObject.SetActive( true );                            // Changes info icon sprite
                }
            }

            if( PerkList[ ( int ) SelectedPerk ].gameObject.activeSelf )                                                       // icon level
                if( PerkList[ ( int ) SelectedPerk ].Label.text == "" )
                    PerkList[ ( int ) SelectedPerk ].Label.text = "L" + LevelFactor;

            bool state = false;
            if( SelectedPerk == EPerkType.WALLDESTROYER )
            {
                state = true;
                UseButtonLabel.text = "Destroy!";
            } else
           if(  SelectedPerk == EPerkType.SNEAKINGLEVEL )
           {
               state = true;
               UseButtonLabel.text = "Scream!";
           }

            if( UseAbilityButton.gameObject.activeSelf != state ) 
                UseAbilityButton.gameObject.SetActive( state );
        }
    }

    public void UpdateBigTextHelp( string basename )
    {
        if( SelectedPerk == EPerkType.NONE ) return;
        if( PerkList[ ( int ) SelectedPerk ].HasBigHelpText == false ) return;
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
        if( Map.I.RM.GameOver ) return;

        if( BigTextHelpLabel.gameObject.activeSelf ) BigTextHelpLabel.gameObject.SetActive( false );
        if( PerkInfoButton.state != UIButtonColor.State.Hover ) return;

        BigTextHelpLabel.gameObject.SetActive( true );
        BigTextHelpLabel.text = Manager.I.Translate( "&" + basename , "Big Help" );                     
    }

    void FixedUpdate()
    {
       Grid.Reposition();
       AdaptPerkIconSize();
    }

	public void SetSelection( Vector2 tg )
	{
		if( tg == new Vector2( -1, -1 ) )
		{
			LockedTile = new Vector2( -1, -1 );
			if( Map.I.TileSelection != null )
			{
				Destroy( Map.I.TileSelection.gameObject );
				Map.I.TileSelection = null;
			}
		}
		else
		{
			LockedTile = tg;
			GameObject obj =  Manager.I.CreateObjInstance( "Tile Selection", "Tile Selection", EDirection.NONE, new Vector3( tg.x, tg.y, -4 ) );
			Map.I.TileSelection = obj.GetComponent<tk2dSprite>();
			obj.transform.parent = Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].Graphic.transform;
			SelectedPerk = EPerkType.NONE;  
		}
	}

    void UpdateUnitUI()
    {
		if( Manager.I.Status != EGameStatus.PLAYING ) return;
		if( Map.I == null || Map.I.Unit == null ) return;
		//if( Map.PtOnMap( Map.I.Tilemap, new Vector2( Map.I.Mtx, Map.I.Mty ) ) == false ) return;

        SelUnit = Map.I.Hero;
		if( SelUnit == false ) return;

        if( Map.I.Mtx != -1 )
            if( Map.PtOnMap( Map.I.Tilemap, new Vector2( Map.I.Mtx, Map.I.Mty ) ) )
				if( Map.I.Unit[ Map.I.Mtx, Map.I.Mty ] )
				{
                    if( Map.I.FreeCamMode && Input.GetMouseButtonDown( 0 ) )
					{
						if( LockedTile == new Vector2( -1, -1 ) )
						{
							SetSelection( new Vector2( ( int ) Map.I.Mtx, ( int ) Map.I.Mty ) );
						}
						else
						{
							SetSelection( new Vector2( -1, -1 ) );
						}
					}
				}

        //if( Map.I.Mtx != -1 )
        //    if( Map.PtOnMap( Map.I.Tilemap, new Vector2( Map.I.Mtx, Map.I.Mty ) ) )
        //        if( Map.I.Unit[ Map.I.Mtx, Map.I.Mty ] )
        //        {
        //            SelUnit = Map.I.Unit[ Map.I.Mtx, Map.I.Mty ];
        //        }

        //if( LockedTile != new Vector2( -1, -1 ) )
        //    SelUnit = Map.I.Unit[ ( int ) LockedTile.x, ( int ) LockedTile.y ];
        //else
        //if( Map.I.Mtx != -1 )
        //{            
        //    SelUnit = Map.I.Unit[ Map.I.Mtx, Map.I.Mty ];
        //    if( SelUnit && SelUnit.TileID == ETileType.DOME ) SelUnit = null;
        //}
        
		if( SelUnit == null ) SelUnit = Map.I.Hero;

		//UpdateCompareHeroFunction();

		if( Map.I.CurrentArea == -1 ) 
            Map.I.MonstersHpSum = Map.I.MonstersTotHpSum = 0;

        HeroHpText.text = "" + Map.I.Hero.Body.Hp.ToString( "0." ) + "/"                                                          // Hero HP Text
            + Map.I.Hero.Body.TotHp.ToString( "0." );
        //MonstersHpText.text = "" + Map.I.MonstersHpSum.ToString( "0." ) + "/" +                                                   // Monster HP Text
        //    Map.I.MonstersTotHpSum.ToString( "0." );

        //if( SelUnit.ValidMonster )
        //    MonstersHpText.text = "" + SelUnit.Body.Hp.ToString( "0." ) + "/" + 
        //        SelUnit.Body.TotHp.ToString( "0." );

        if( LosttHPLabel.gameObject.activeSelf )
            LosttHPLabel.gameObject.SetActive( false );

        if( Map.I.CurrentArea != -1 )                                                                                             // Lost HERO HP information
        {
            float hp = Map.I.HeroEnterAreaHP - G.Hero.Body.Hp;
            LosttHPLabel.text = "";
            if( hp > 0 )
            {
                LosttHPLabel.gameObject.SetActive( true );
                LosttHPLabel.text += "H " + hp.ToString( "0.#" ) + " HP";            
            }
             if( Map.I.CurArea.MonsterDamage > 0 )                                                                                 // Lost Monster HP information
            {
                LosttHPLabel.gameObject.SetActive( true );
                LosttHPLabel.text += "\nM " + Map.I.CurArea.MonsterDamage.ToString( "0.#" ) + " HP";
            }

            if( Map.I.CurArea.MonsterDamage > 0 )                                                                                 // HP ratio Information
            if( hp > 0 )
                {
                    float ratio = ( Map.I.CurArea.MonsterDamage / hp ) * 100;
                    LosttHPLabel.text += "\nR " + ratio.ToString( "0." ) + "%";
                }
        }
        
        int stars = ( int ) ( SelUnit.Body.Stars - ( ( SelUnit.Body.Level - 1 ) * 5 ) );

		for( int i = 0; i < 5; i++ )                       // Update Stars Sprites
		if( stars > i )
			Stars[ i ].gameObject.SetActive( true );
		else Stars[ i ].gameObject.SetActive( false );

		float perc = 100 * Map.I.Hero.Body.Hp / Map.I.Hero.Body.TotHp;
		UnitHealthBar.value = perc / 100;
		float perc2 = 100 * Map.I.MonstersHpSum / Map.I.MonstersTotHpSum;
    }
    public void UpdateUI()
	{
        DebugLabel.gameObject.SetActive( true );
        if( Manager.I.GameType == EGameType.FARM )
            DebugLabel.gameObject.SetActive( false );

        MapLabel.gameObject.SetActive( true );
        if( Manager.I.GameType == EGameType.FARM )
            MapLabel.gameObject.SetActive( false );

		if( Manager.I.Status != EGameStatus.PLAYING ) return;
              
        if( !AreasText.gameObject.activeSelf ) AreasText.gameObject.SetActive( true );
        if( !ArtifactsText.gameObject.activeSelf ) ArtifactsText.gameObject.SetActive( true );

        if( Map.I.FreeCamMode )
        {                                                         
            //AreasText.text = "Free Camera MODE";
            //GameLevelText.text = " Pos: (" + Map.I.Hero.Pos.x + ", " + Map.I.Hero.Pos.y + ")";
        }
        else
        {
            if( UpdBeastText )
            if( Map.I.RM.HeroSector ) 
            {
                if( Map.I.RM.HeroSector.Type == Sector.ESectorType.NORMAL )
                {
                    int killed = GetMonsterKilled();
                    if( killed < 0 ) killed = 0;
                    AreasText.text = "Beasts: " + ( killed ) +
                    "/" + ( GetTotMonsters() ); // + "   Awaken: " + Sector.TotalAwakenMonsters ); need to fix the TotalAwakenMonsters var before updating text
                }
                else AreasText.text = "";
                UpdBeastText = false;
            }

            //if( Map.I.RM.NumGateKeyNeeded > 0 )
            //{
            //    AreasText.text += "   Gate: +" + Map.I.RM.NumGateKeyNeeded;
            //    AreasText.color = Color.white;
            //}
            //else
            //    AreasText.color = Color.green;
        }

        //ArtifactsText.color = Color.yellow;
        //if( Map.I.RM.DungeonDialog.gameObject.activeSelf == false )
        //{
        //    ArtifactsText.text = "Artifacts: " + ( HowManyGameArtifactsCollectedOnLevel() + HeroData.I.CollectedLevelArtifacts.Count ) +
        //                                                         "/" + Quest.I.CurLevel.ArtifactList.Count;
        //    if( Quest.I.CurLevel.ArtifactList.Count <= 0 )
        //        ArtifactsText.text = "";
        //    if( Map.I.HeroAttackSpeedBonus > 0 )
        //    {
        //        ArtifactsText.color = Color.green;
        //        ArtifactsText.text = "Att Speed: +" + Map.I.HeroAttackSpeedBonus + "%";
        //    }
        //}
        //else
        //{
        //    ArtifactsText.text = "";
        //    AreasText.text = "";
        //}

        //if( G.Tutorial.Phase >= 20 )
        //{
        //    float val = Map.I.RM.DungeonDialog.AdventureCompletition;                                           // completition bar
        //    QuestCompletitionBar.value = val / 100;
        //    QuestCompletitionLabel.text = QuestCompletitionText;
        //    QuestCompletitionBar.gameObject.SetActive( true );
        //    if( Manager.I.GameType == EGameType.FARM )
        //        QuestCompletitionBar.gameObject.SetActive( false );
        //}

        if( Manager.I.GameType == EGameType.CUBES )
        if( Map.I.SessionTime > 1 )
        if( UpdGoalText )
        {
            Map.I.RM.DungeonDialog.UpdateObjectiveIcons( true );
        }

        if( Manager.I.GameType == EGameType.NAVIGATION )
        {
            ArtifactsText.text = "Navigation Map";
            AreasText.text = "";
        }
    
		if( EnterAreaTxt )
		{
			EnterAreaTxt.color = new Color( 1, 1, 1, EnterAreaTxt.color.a - Time.deltaTime * .5f );
			if( EnterAreaTxt.color.a <= 0 ) EnterAreaTxt.gameObject.SetActive( false );
		}
        
        //if( RestartAreaButton.state == UIButtonColor.State.Pressed ||
        //    Input.GetKey( KeyCode.R ) ) RKeyPressTimeCount += Time.deltaTime;
        //else RKeyPressTimeCount = 0;

        //if( RKeyPressTimeCount > 1.5f )
        //{
        //    Map.I.PreferedAreaSave = new Vector2( -1, -1 );
        //    Manager.I.ForceRestartFromBeginning = true;
        //    RKeyPressTimeCount = 0;
        //}

        if( Map.I.Hero.Body.ToolBoxLevel < 2 ) GridButton.gameObject.SetActive( false );
        else GridButton.gameObject.SetActive( true );

        if( Map.I.Hero.Body.ToolBoxLevel < 3 ) BattleButton.gameObject.SetActive( false );
        else BattleButton.gameObject.SetActive( true );

        if( Map.I.Hero.Body.ToolBoxLevel < 4 )
        {
            ArtifactSeekButton.gameObject.SetActive( false );
            AreaSeekButton.gameObject.SetActive( false );
        }
        else
        {
            ArtifactSeekButton.gameObject.SetActive( true );
            AreaSeekButton.gameObject.SetActive( true );
        }

        if( Map.I.Hero.Body.ToolBoxLevel < 5 ) CompareHeroButton.gameObject.SetActive( false );
        else CompareHeroButton.gameObject.SetActive( true );   
	}

    public void UpdateBigMessage()
    {
      if(  BigMessageTextTimeCounter > 0 )
          BigMessageText.gameObject.SetActive( true );
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf == true )
            BigMessageText.gameObject.SetActive( false );
       
        BigMessageTextTimeCounter -= Time.deltaTime;                                                                         // Big text message update
        if( BigMessageTextTurnCounter > 0 )
        {
            BigMessageTextTimeCounter = 100;
            if( Map.I.AdvanceTurn )
                BigMessageTextTurnCounter--;
        }

        float alpha = 1;
        if( BigMessageTextTimeCounter < BigMessageTextFadeTime )
            alpha = BigMessageTextTimeCounter / BigMessageTextFadeTime;
        if( alpha < 0 ) alpha = 0;
        BigMessageText.color = new Color( BigMessageText.color.r, BigMessageText.color.g, BigMessageText.color.b, alpha );
        if( BigMessageTextTimeCounter <= 0 || BigMessageTextTurnCounter == 0 )
        {
            BigMessageText.gameObject.SetActive( false );
            BigMessageTextTimeCounter = 0;
            BigMessageTextTurnCounter = 0;
        }
        //BigMessageText.gameObject.transform.position = new Vector3( ( int ) Helper.I.FloatVal1, ( int ) 
        //Helper.I.FloatVal2, BigMessageText.gameObject.transform.position.z );       
    }    

	public void SelectTempPerk( Artifact ar )
	{
		if( ( int ) ar.TargetHero != Map.I.Hero.Variation ) return;
		TempSelectedPerk = ar.PerkType;
		TempSelectedPerkTimer = 12;
	}
    public void SelectTempPerk( EPerkType type, float time )
    {
        TempSelectedPerk = type;
        TempSelectedPerkTimer = time;
    }

    public void ShowAreaEnterMessage()
    {
        string opt = "";
        if( Map.I.CurArea.Optional ) opt = "\n(Optional Area)";
        EnterAreaTxt.gameObject.SetActive( true );
        EnterAreaTxt.text = "Entering:\n" + '"' + Map.I.CurArea.AreaName + '"' + opt;
        EnterAreaTxt.color = new Color( 1, 1, 1, 1 );
    }

    public void ShowAreaClearMessage( int area )
    { 
        EnterAreaTxt.gameObject.SetActive( true );
        string perf = "";
        if( Manager.I.GameType == EGameType.CUBES && Map.I.CurArea.Perfect ) perf = "\nPerfect!";
        EnterAreaTxt.text = Quest.I.CurLevel.AreaList[ area ].AreaName + " Cleared!" + perf;
        EnterAreaTxt.color = new Color( 1, 1, 1, 1 );
    }
    public int AddPerk( ref Artifact art )
    {
        if( art.TargetHero != Map.I.SelectedHero )
        if( art.TargetHero != EHeroID.ALL_HEROES ) return -1;

        string oldtitle = art.Perk.TitleText;
        string oldmain  = art.Perk.MainText;

        GameObject instance = ( GameObject ) GameObject.Instantiate( PerkModel );
        instance.SetActive( true );

        int id = ( PerkList.Count );

        if( art.LifeTime == Artifact.EArtifactLifeTime.GAME )
        {
            instance.transform.position = new Vector3( PerkModel.transform.position.x + ( NumGamePerk * 1.15f ), PerkModel.transform.position.y, I.transform.position.z );
            NumGamePerk++;
        }
        if( art.LifeTime == Artifact.EArtifactLifeTime.SESSION )
        {
            I.transform.position = new Vector3( PerkModel.transform.position.x + ( NumLevelPerk * 1.15f ), PerkModel.transform.position.y - 1.15f, I.transform.position.z );
            NumLevelPerk++;
        }
        if( art.LifeTime == Artifact.EArtifactLifeTime.AREA  ) 
        {
            I.transform.position = new Vector3( PerkModel.transform.position.x + ( NumAreaPerk * 1.15f ), PerkModel.transform.position.y - 2.3f, I.transform.position.z );
            NumAreaPerk++;
        }

        I.transform.parent = PerksFolder.transform;
        I.name = "Perk " + id;
        Perk p = instance.GetComponent<Perk>();

        p.UiPanelPerkID = id;

        p.UIIconSprite.spriteId = art.Sprite.spriteId;
        p.UIIconBackSprite.spriteId = 0;
        p.Artifact = art;
        art.Perk.Artifact = art;
        art.Perk = p;
        art.Perk.TitleText = oldtitle;
        art.Perk.MainText = oldmain;
 
        PerkList.Add( art.Perk );
        return - 1;
    }
    
	public void CompareHeroFunction()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
        if( Map.I.RM.GameOver ) return;
        UpdateCompareHeroFunction( true );
    }

    public void BattleFunction()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
        if( Map.I.RM.GameOver ) return;
        if( Manager.I.GameType == EGameType.FARM ) return;
        Map.I.Hero.Control.ForceMove = EActionType.BATTLE;
    }

    public bool UpdateMessageBox()
    {
        if( MessageBox.activeSelf )
        {
            Quest.I.UpdateArtifactMouseOverInfo( new Vector2( -1, -1 ) );
            Map.I.UpdateWorldMapUnitsData();
            Map.I.UpdateCamera();
            return true;
        }
        return false;
    }
    
    public void UpdateCompareHeroFunction( bool force = false )
	{
        return;
		if( Input.GetKeyDown( KeyCode.F9 ) || force )
		{
			for( int i = 0; i < 20; i++ )
			{
				if( ActiveCompareHeroID == ( int ) EHeroID.NONE ) ActiveCompareHeroID = Map.I.Hero.Variation;

				ActiveCompareHeroID++;
				if( ActiveCompareHeroID == ( int ) ( EHeroID.HERO_4 + 1 ) )
					ActiveCompareHeroID = ( int ) EHeroID.KADE;

				if( Map.I.AvailableHeroesList[ ActiveCompareHeroID ] ) break;
			}
		}

		if( ActiveCompareHeroID != ( int ) EHeroID.NONE )
		{
            if( Map.I.Hero.Body.ToolBoxLevel < 5 )
            {
                Map.I.ShowMessage( Language.Get( "ERROR_TOOLBOX5" ) );
                return;
            }

			CompareHero.Copy( Map.I.HeroList[ ( int ) ActiveCompareHeroID ], false, false, false );
			Quest.I.UpdateArtifactData( ref CompareHero );
			UpdatePortrait( ( EHeroID ) ActiveCompareHeroID );
			SelUnit = CompareHero;
		}
	}

	public void UpdatePortrait( EHeroID hero )
	{
		for( int i = 0; i < PortraitList.Length; i++ )
			if( ( int ) hero == i )
				PortraitList[ i ].gameObject.SetActive( true );
			else
				PortraitList[ i ].gameObject.SetActive( false );
	}

    public void ToggleMenu()
    {
        bool state = Menu.activeSelf;
        Menu.SetActive( !state );
        PerkInfoPanel.gameObject.SetActive( state );
        HpPanel.gameObject.SetActive( state );
        //PerksPanel.gameObject.SetActive( state );
        ZoomButton.gameObject.SetActive( state );
    }

    public void ToggleFreeCamera()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
        if( Map.I.RM.GameOver ) return;
        SetFreeCamera( !Map.I.FreeCamMode );
    }

    public void SetZoomLevel()
    {
        int fz = ( int ) Item.GetNum( ItemType.Res_ForcedZoom );
        if( Manager.I.GameType != EGameType.CUBES ) fz = -1;

        if( CubeData.I.FixedZoomMode != -1 )                                              // Cubedata Forced zoom
            fz = CubeData.I.FixedZoomMode;

        if( Map.I.RM.RMD.LockedZoom || fz >= 0 )
        {
            Message.RedMessage("Locked Zoom!");                                           // Locked zoom message
            return;
        }
        if( Map.I.CurrentArea == -1 || Map.I.AreaCleared )
        {
            if( Manager.I.GameType == EGameType.NAVIGATION )
            {
                if( ++Map.I.ZoomMode == 3 ) Map.I.ZoomMode = 0;
                Map.I.NavigationMap.NavigationZoomMode = Map.I.ZoomMode;
            }
            else
            if( ++Map.I.ZoomMode == 4 ) Map.I.ZoomMode = 0;
        }
        else
        {
            if( ++Map.I.AreaZoomLevel == 2 ) Map.I.AreaZoomLevel = 0;
        }
    }

    public void SetFreeCamera( bool mode )
    {
        if( Manager.I.GameType != EGameType.CUBES )
        {
            UI.I.SetBigMessage( "Free Camera is only available at the cubes.", Color.red, 5f, 4.7f, 122.8f, 85, .001f );
            return;
        } 
        Map.I.FreeCamMode = mode;
        UI.I.UpdBeastText = true;

        if( Map.I.FreeCamMode )
        {
            UI.I.SetBigMessage( "Free Camera MODE\nMovement Keys Moves the Camera.\nlshift returns.", Color.green, 100f, 4.7f, 122.8f, 85, .001f );
        }
        else
        if( Map.I.FreeCamMode == false )
        {
            UI.I.SetBigMessage( "", Color.green, .1f );
            UI.I.BigMessageTextTimeCounter = .1f;
        }
    }

    public void UpdateSaveGameIconState( int x, int y, bool leavingArea = false )
    {
        if( Map.I.Gaia2[ x, y ] )
            if( Map.I.Gaia2[ x, y ].TileID == ETileType.CHECKPOINT )
            {
                float t = 0.7f;
                int area = Map.I.GetPosArea( new Vector2( x, y ) );

                string file = Manager.I.GetProfileFolder() + Manager.I.QuestName + "/" +
                             "Save Game L" + Quest.CurrentLevel + " " + x + " " + y + ".wq";
                bool exists = ES2.Exists( file + "?tag=SaveInfo" );
                
                Map.I.Gaia2[ x, y ].gameObject.SetActive( false );

                if( Map.I.Hero.Body.MemoryLevel < 1 )
                {
                    Map.I.Gaia2[ x, y ].gameObject.SetActive( false );
                }
                else
                    if( Map.I.Hero.Body.MemoryLevel < 2 )
                    {
                        if( area == -1 )
                        {
                            if( Map.I.Gaia2[ x, y ].Variation <= 0 )
                                Map.I.Gaia2[ x, y ].gameObject.SetActive( true );
                        }
                    }
                    else
                        if( Map.I.Hero.Body.MemoryLevel < 3 )
                        {
                            if( Map.I.Gaia2[ x, y ].Variation <= 0 )
                                Map.I.Gaia2[ x, y ].gameObject.SetActive( true );
                        }
                        else
                            if( Map.I.Hero.Body.MemoryLevel < 4 )
                            {
                                if( Map.I.Gaia2[ x, y ].Variation <= 1 )
                                    Map.I.Gaia2[ x, y ].gameObject.SetActive( true );
                            }
                            else
                                if( Map.I.Hero.Body.MemoryLevel < 5 )
                                {
                                    if( Map.I.Gaia2[ x, y ].Variation <= 2 )
                                        Map.I.Gaia2[ x, y ].gameObject.SetActive( true );
                                }
                                else
                                    if( Map.I.Hero.Body.MemoryLevel < 6 )
                                    {
                                        if( Map.I.Gaia2[ x, y ].Variation <= 3 )
                                            Map.I.Gaia2[ x, y ].gameObject.SetActive( true );
                                    }
                                    else
                                        if( Map.I.Hero.Body.MemoryLevel < 7 )
                                        {
                                            if( Map.I.Gaia2[ x, y ].Variation <= 4 )
                                                Map.I.Gaia2[ x, y ].gameObject.SetActive( true );
                                        }

                if( Map.I.FreeCamMode && exists ) Map.I.Gaia2[ x, y ].gameObject.SetActive( true );

                if( Map.I.Gaia2[ x, y ].gameObject.activeSelf )
                {
                    Map.I.Gaia2[ x, y ].Spr.color = new Color( 0, 0, 0, t );

                    if( area == -1 )
                    {
                        if( exists )
                        {
                            Map.I.Gaia2[ x, y ].LevelTxt.text = ES2.Load<string>( file + "?tag=SaveInfo" );

                            if( Map.I.FreeCamMode )
                            {
                                Map.I.Gaia2[ x, y ].LevelTxt.gameObject.SetActive( true );
                                Map.I.Gaia2[ x, y ].LevelTxt.color = new Color( 1, 1, 1, t );
                            }
                            else
                            {
                                Map.I.Gaia2[ x, y ].LevelTxt.gameObject.SetActive( false );
                            }
                            Map.I.Gaia2[ x, y ].Spr.color = new Color( 1, 1, 1, t );
                        }
                    }

                    if( area != -1 )
                    {
                        if( leavingArea ) { Map.I.Gaia2[ x, y ].Spr.color = new Color( 0, 0, 0, t ); return; }
                        for( int i = 0; i < Map.I.AreaSaveList.Count; i++ )
                        {
                            if( Map.I.AreaSaveList[ i ] == new Vector2( x, y ) )
                                Map.I.Gaia2[ x, y ].Spr.color = new Color( 1, 1, 1, t );
                        }
                    }
                }  
            }
    }

    public void CloseMessageBox()
    {
        MessageBox.gameObject.SetActive( false );
        Map.I.InvalidateInputTimer = .4f;
    }

    public int GetNumConqueredOptionalAreas()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf ) return 0;
        int count = 0;
        for( int i = 0; i < Quest.I.CurLevel.AreaList.Count; i++ )
        {
            if( Quest.I.CurLevel.AreaList[ i ].Optional )
                if( Quest.I.CurLevel.AreaList[ i ].Cleared ) count++;
        }
        return count;
    }

    public void ChooseArtifactnfoLevel()
    {
        //ArtifactLevelDifference++;
    }

    public int GetIconID( int levelFactor, EPerkType pk )
    {
        int id = levelFactor;
        if( levelFactor >= PerkList[ ( int ) pk ].Sprite.Length )
            id = PerkList[ ( int ) pk ].Sprite.Length - 1;
        return id;
    }

    public void TogglePrimaryFilter()
    {
        ShowMainArtifacts = !ShowMainArtifacts;
    }

    public void ToggleSecondaryFilter()
    {
        ShowSecondaryArtifacts = !ShowSecondaryArtifacts;
    }

    public void UpdateAllTranslations()
    {
        RestartAreaButton.GetComponentInChildren<UILabel>().text = Language.Get( "RESTART_BUTTON" );
        FreeCamButton.GetComponentInChildren<UILabel>().text = Language.Get( "FREECAMERA_BUTTON" );
        GridButton.GetComponentInChildren<UILabel>().text = Language.Get( "GRID_BUTTON" );
        ArtifactSeekButton.GetComponentInChildren<UILabel>().text = Language.Get( "ARTIFACTSEEK_BUTTON" );
        AreaSeekButton.GetComponentInChildren<UILabel>().text = Language.Get( "AREASEEK_BUTTON" );
        CompareHeroButton.GetComponentInChildren<UILabel>().text = Language.Get( "COMPAREHERO_BUTTON" );
        BattleButton.GetComponentInChildren<UILabel>().text = Language.Get( "BATTLEKEY_BUTTON" );
    }
    public void AdaptPerkIconSize()
    {
        //return;
        int count = 0;
        for( int i = 0; i < PerkList.Count; i++ )
        if ( PerkList[ i ] )
        if ( PerkList[ i ].ResizeIcon )
        if ( PerkList[ i ].gameObject.activeSelf ) count++;
        int id = count - 1;
        if( count < 1 && count >= PerkList.Count - 1 ) return;
        if( id > GridHelper.Position.Length - 1 ) return;
        if( id == -1 ) return;

        Grid.transform.localPosition = GridHelper.Position[ id ];
        Grid.cellWidth = GridHelper.CellWidth[ id ];
        Grid.cellHeight = GridHelper.CellHeight[ id ];
        Grid.maxPerLine = GridHelper.ColumnLimit[ id ];
        for( int i = 0; i < PerkList.Count; i++ )
        if( PerkList[ i ] )
        if ( PerkList[ i ].gameObject.activeSelf )
        if ( PerkList[ i ].ResizeIcon )
        {
            PerkList[ i ].gameObject.transform.localScale = GridHelper.Scale[ id ];
        }
    }
    public void GetPerkInfo( EPerkType type, ref int levelFactor, ref string basename, ref string addDesc, Unit un, ref string iconText, bool dif )
    {
        ConsiderArtifactLevelDifference = dif;
        CurrentPerkType = type;

        switch( ( EPerkType ) type )
        {
            case EPerkType.MONSTERPUSH: // Monster Push

            levelFactor = ( int ) CheckLevel( un.Control.MonsterPushLevel );
            basename = "MONSTERPUSH";

            break;

            case EPerkType.MONSTERCORNERING:  // Monster Press

            levelFactor = ( int ) CheckLevel( un.Control.MonsterCorneringLevel );
            basename = "MONSTERCORNERING";

            float perc = HeroData.I.MonsterCorneringBonus[ levelFactor ];
            addDesc = "\nAttack Bonus: " + perc + "%";           
            iconText = "+" + perc + "%";

            break;

            case EPerkType.DESTROYBARRICADE: // Destroy Barricade

            levelFactor = ( int ) CheckLevel( un.Body.DestroyBarricadeLevel );
            basename = "DESTROYBARRICADE";

            addDesc = "This Determines the size of barricade that can be destroyed by the hero.\n";
            addDesc += "Max size is the Destroy power plus the sum of neighbor touched barricades.\n";
            addDesc += "Destroy power: " + levelFactor;

            break;

            case EPerkType.BARRICADEFORRUNE: // Barricade For Rune

            levelFactor = ( int ) CheckLevel( un.Body.BarricadeForRune );
            basename = "BARRICADEFORRUNE";
            addDesc = "This Determines the amount of Barricades wood needed to be exchanged for Runes. (destroyed by Monsters only)";
            
            float req = Map.I.RM.RMD.BaseWoodRequiredForSale - ( levelFactor + MouseArtifactMult );
            float rune = Map.I.RM.RMD.WoodForRunePrize;
            addDesc += "\n\nWood for exchange: " + HeroData.I.BarricadeWood.ToString( "0." ) + " of " +            
            req + " Tot: " + Map.I.LevelStats.BarricadeWood + " (+" + rune + ")";
            break;

            case EPerkType.OUTAREABURNINGBARRICADEDESTROYBONUS: // Out Area Burning Barricade Destroy bonus

            levelFactor = ( int ) CheckLevel( un.Body.OutAreaBurningBarricadeDestroyBonus );
            basename = "OUTAREABURNINGBARRICADEDESTROYBONUS";

            addDesc = "This Determines The maximum size allowed for gaining a bonus for destroying an Outarea Burning Barricade.";
            addDesc += " To destroy a Burning barricade, its size also needs to be equal or lower to the amount of lit firepits in the area/Cube.";

            break;


            case EPerkType.HERONEIGHBORTOUCHADDER: // Hero Neighbor Touch Adder

            levelFactor = ( int ) CheckLevel( un.Body.HeroNeighborTouchAdder );
            basename = "HERONEIGHBORTOUCHADDER";

            addDesc = "If a barricade that has been touched is bigger or equivalent to the adder Size, it becomes an Adder (Green). An Adder Around the hero adds 1 to his Destruction Power.\n";
            //addDesc += "\nAdder: L" + ( Map.TotBarricade - levelFactor ) + " and UP";
            break;

            case EPerkType.LOOTER: // Looter
             
            levelFactor = ( int ) CheckLevel( un.Body.LooterLevel );
            basename = "LOOTER";

            //addDesc = "\n\nRandom Loot: +" + amt1 + " \n Random Loot Chance: " + (Map.I.RM.RMD.BaseRandomResourceChance + HeroData.I.RandomLootChance[ levelFactor ]) + "%";
            //if( levelFactor >= 3 )
            //{
            //addDesc += "\nMonster Loot: +" + ( Map.I.RM.RMD.BaseLootResourceAmount + HeroData.I.ExtraLootAmount[ levelFactor ] );
            //addDesc += "\nMonster Loot Turns: " + HeroData.I.ExtraMonsterLootTurns[ levelFactor ] + " to " + ( HeroData.I.ExtraMonsterLootTurns[ levelFactor ] + 4 );       
            //}
            break;

            case EPerkType.PROSPECTOR: // Stack Push
            
            levelFactor = ( int ) CheckLevel( un.Body.ProspectorLevel );
            basename = "PROSPECTOR";
            addDesc = "\n\nInterest: " + HeroData.I.ProspectorInterestRate[ levelFactor ] + "%";
            break;

            case EPerkType.EVASIONLEVEL: // Evasion

            levelFactor = ( int ) CheckLevel( un.Control.EvasionLevel );
            basename = "EVASION";

            float points = HeroData.I.EvasionStartingPoints[ levelFactor ];
            float totalp = HeroData.I.EvasionStartingPoints[ levelFactor ];
            if( Map.I.CurrentArea != -1 )
            {
                float deathpoints = 0;
                if( Map.I.Hero.Control.EvasionLevel >= 8 ) deathpoints += 2 * Map.I.CurArea.MonsterKillCount;
                int lev = ( int ) Mathf.Ceil( Map.I.Hero.Control.EvasionLevel );
                points = HeroData.I.EvasionStartingPoints[ lev ] - Map.I.CurArea.EvasionPointsUsed + deathpoints;
                totalp += deathpoints;
            }

            Map.I.Hero.Control.EvasionPoints = points;
            iconText = "L" + levelFactor + " - " + points + "p";
            addDesc = "\n\nEvasion Points: " + points + " of " + totalp;

            break;

            case EPerkType.PERFECTIONISTLEVEL: // Perfectionist

            levelFactor = ( int ) CheckLevel( un.Control.PerfectionistLevel );
            basename = "PERFECTIONIST";

            if( levelFactor < 2 )
            {
                addDesc = "\nInarea Runes timers remain untouched after clearing a Perfect Area";
            }
            else
            if( levelFactor < 3 )
            {
                addDesc = "\nOutarea Revealed Runes timers remain untouched after clearing a Perfect Area";
            }
            else
            if( levelFactor < 4 )
            {
                addDesc = "\nOutarea Hidden Runes timers decrease by one less unit after clearing a perfect area.";
            }
            break;

            case EPerkType.SCAVENGERLEVEL: // Scavenger

            levelFactor = ( int ) CheckLevel( un.Control.ScavengerLevel );
            basename = "SCAVENGER";
            break;
            

            case EPerkType.ECONOMY: // Economy

            levelFactor = ( int ) CheckLevel( un.Body.EconomyLevel );
            basename = "ECONOMIST";
            addDesc = "This Perk regulates Resource Gathering and Management.";
            break;

            case EPerkType.RESOURCEPERSISTANCE: // Resource Persistance

            levelFactor = ( int ) CheckLevel( un.Body.ResourcePersistance );
            basename = "RESOURCEPERSISTANCE";

            if( levelFactor < 2 )
            {
                addDesc = "This Perk regulates the How Long A bonus resource tile remains active after hero has entered the area.";
                addDesc += "\nResources can only be picked up immediatelly after entering area but not if Hero is being seen by any monster. (Hero has no LOS to any monster )";
            }
            else
            if( levelFactor < 3 )
            {
                addDesc = "One extra turn awarded until resources are gone.";
            }
            else
            if( levelFactor < 4 )
            {
                addDesc = "Unlimited Turns While Unseen by Monsters.";
            }
            else
            if( levelFactor < 5 )
            {
                addDesc = "Inarea Resources Remain if hero has been seen by no more than one Monster.";
            }
            else
            if( levelFactor < 6 )
            {
                addDesc = "Inarea Resources Remain if hero has been seen by no more than two Monsters.";
            }
            else
            if( levelFactor < 7 )
            {
                addDesc = "Inarea Resources Remain While the area is Virgin.";
            }
            else
            if( levelFactor < 8 )
            {
                addDesc = "Inarea Resources Remain if monsters have taken only one hit in the total sum.\n Hero needs to remain undamaged.";
            }
            else
            if( levelFactor < 9 )
            {
                addDesc = "Inarea Resources Remain if only one monster have been killed. (Hero Undamaged)";
            }
            else
            if( levelFactor < 10 )
            {
                addDesc = "Inarea Resources Remain if the Hero has Taken at least 2 Hits.";
            }
            else
            if( levelFactor < 11 )
            {
                addDesc = "Inarea Resources Remain Until it has been cleared.";
            }
            else
            if( levelFactor < 12 )
            {
                addDesc = "Inarea Resources Remain for two extra turns after the area has been cleared.";
            }
            else
            if( levelFactor < 13 )
            {
                addDesc = "Inarea Resources Remain Eternally. Even after Area Reentrance.";
            }

            break;

            case EPerkType.COLLECTOR: // Collector

            levelFactor = ( int ) CheckLevel( un.Body.CollectorLevel );
            basename = "COLLECTOR";

            if( levelFactor < 2 )
            {
                addDesc = "A second Rune is given if there are at least 6 free tiles around the bonus position.\nHint: Clear Barricades to get more free tiles.";
            }
            else
            if( levelFactor < 3 )
            {
                addDesc = "A second Rune is given if there are at least 5 free tiles around the bonus position.";
            }
            else
            if( levelFactor < 4 )
            {
                addDesc = "A second Rune is given if there are at least 4 free tiles around the bonus position.";
            }
            else
            if( levelFactor < 5 )
            {
                addDesc = "Every 3 free tiles around the rune position adds 1 to its bonus amount.";
            }
            else
            if( levelFactor < 6 )
            {
                addDesc = "The central rune position is also considered a free tile, and thus, is added to the bonus calculation.";
            }
            else
            if( levelFactor < 7 )
            {
                addDesc = "Every 2 free tiles around the rune position adds 1 to its bonus amount.";
            }
            else
            if( levelFactor < 8 )
            {
                addDesc = "Central tile is worth 2 free tiles.";
            }
            else
            if( levelFactor < 9 )
            {
                addDesc = "Every INAREA Touched Barricade Adds 15% to the final bonus Calculation.";
            }

            break;
  


            //addDesc = "Every3.";
            //addDesc = "Center.";

            case EPerkType.BARRICADEFIGHTERLEVEL: // Barricade Fighter

            levelFactor = ( int ) CheckLevel( un.Control.BarricadeFighterLevel );
            basename = "BARRICADEFIGHTER";
          
            int max = ( int ) G.Hero.Body.DestroyBarricadeLevel;
            if( max < 0 ) max = 0;

            string bar = "";          
            Vector2 fr = G.Hero.GetFront();
            int extra = Map.I.GetNeighborBarricadesTouchedCount( fr );
            if( extra > 0 ) bar = "+" + extra;

            addDesc += "\n\nDestroy power: " + max + bar;
            //addDesc += "\nAdder: L" + ( Map.TotBarricade - G.Hero.Body.HeroNeighborTouchAdder ) + " and UP";
            if( ( int ) un.Body.OutAreaBurningBarricadeDestroyBonus >= 1 )
                addDesc += "\nOutarea BB bonus: " + ( int ) un.Body.OutAreaBurningBarricadeDestroyBonus;

            string im = "";
            if( Map.I.CurrentArea != -1 && Map.I.CurArea.BarricadeWoodBark > 0 ) 
                im = "B" + Map.I.CurArea.BarricadeWoodBark + " ";

            iconText = im + "D" + max + bar;

            if( un.Body.BarricadeForRune >= 1 )
            {
                float wall = Map.I.RM.RMD.BaseWoodRequiredForSale - un.Body.BarricadeForRune;
                addDesc += "\nWood for exchange: " + HeroData.I.BarricadeWood.ToString( "0." ) + " of " +
                wall + " Tot: " + Map.I.LevelStats.BarricadeWood + " (+" + Map.I.RM.RMD.WoodForRunePrize + ")";
            }
            break;

            case EPerkType.ARROWRUSH: // Arrow Rush
            break;

            case EPerkType.INVULNERABILITY: // Invulnerability
            break;

            case EPerkType.DAMAGESURPLUS: // Damage Surplus

            levelFactor = ( int ) CheckLevel( un.Body.DamageSurplusLevel );
            basename = "DAMAGESURPLUS";

            iconText = "";
            if( un.MeleeAttack )
            {
                if( un.MeleeAttack.DamageSurplus > 0 )
                {
                    float tot = Util.Percent( HeroData.I.DamageSurplusBonus[ ( int ) levelFactor ], un.MeleeAttack.DamageSurplus );
                    addDesc = "\n\nStep: " + HeroData.I.DamageSurplusBonus[ ( int ) levelFactor ] + "% of " +
                    un.MeleeAttack.DamageSurplus.ToString( "0.#" ) + " Melee Damage = " + tot.ToString( "0.#" ) + "\n";
                    iconText = "M" + tot.ToString( "0.#" );
                }

                if( un.MeleeAttack.RTDamageSurplus > 0 )
                {
                    float tot = Util.Percent( HeroData.I.DamageSurplusBonus[ ( int ) levelFactor ], un.MeleeAttack.RTDamageSurplus );
                    addDesc += "\n\nRT: " + HeroData.I.DamageSurplusBonus[ ( int ) levelFactor ] + "% of " +
                    un.MeleeAttack.RTDamageSurplus.ToString( "0.#" ) + " Melee Damage = " + tot.ToString( "0.#" ) + "\n";
                }
            }

            if( un.RangedAttack )
            {
                if( un.RangedAttack.DamageSurplus > 0 )
                {
                    float tot = Util.Percent( HeroData.I.DamageSurplusBonus[ ( int ) levelFactor ], un.RangedAttack.DamageSurplus );
                    addDesc = "\n\nStep: " + HeroData.I.DamageSurplusBonus[ ( int ) levelFactor ] + "% of " +
                    un.RangedAttack.DamageSurplus.ToString( "0.#" ) + " Ranged Damage = " + tot.ToString( "0.#" ) + "\n";
                    iconText += " R" + tot.ToString( "0.#" );
                }

                if( un.RangedAttack.RTDamageSurplus > 0 )
                {
                    float tot = Util.Percent( HeroData.I.DamageSurplusBonus[ ( int ) levelFactor ], un.RangedAttack.RTDamageSurplus );
                    addDesc += "\n\nRT: " + HeroData.I.DamageSurplusBonus[ ( int ) levelFactor ] + "% of " +
                    un.RangedAttack.RTDamageSurplus.ToString( "0.#" ) + " Ranged Damage = " + tot.ToString( "0.#" ) + "\n";
                }               
            }

            break;

            case EPerkType.PLATFORMWALKING: // Platform Walking

            levelFactor = ( int ) CheckLevel( un.Control.PlatformWalkingLevel );
            basename = "PLATFORMWALKING";  

            if( levelFactor < 2 )
            {
                //addDesc = "\nHero can only exit Platforms by Moving to the same direction as he moved when he entered the Platform Group\n Example: Entered Moving North -> Needs to exit Moving north, too.";
            }
            else
            if( levelFactor < 3 )
            {
                //addDesc = "\nHero Receives Attack Bonus when attacking from Platforms.";
            }
            else
            if( levelFactor < 4 )
            {
                //addDesc = "\nHero Receives Platform Points when he drops all platforms in a group.\n Dirty Areas only!!";
                //addDesc += "\nLifetime Platform Points Collected gives the hero Attack Bonus Anywhere.";
            }
            else
            if( levelFactor < 5 )
            {
                //addDesc = "\nEvery single platform gives Bonus Points When Dropped Down";
                addDesc += "\nBonus: +1 Per Individual Platform";
            }
            else
            if( levelFactor < 6 )
                {
                    //addDesc = "\nFree exit Ability: \nHero can freely exit Platforms.";
                    addDesc += "\nCost: " + Map.I.RM.RMD.FreePlatformExitCost + " pp";
                }
            else
            if( levelFactor < 7 )
                {
                //addDesc = "\n+1 PP per Individual Platf. Also, Hero can Close platform groups immediatelly after clearing the area. (first area entrance time only - Only half pp gain)";
                    addDesc += "\nIndividual PP Bonus: +1";
                }
            else
                if( levelFactor < 8 )
                {
                    //addDesc = "\nOrientation Exit Ability: \nHero can freely exit Platforms if The orientation that he entered the group is the same as the exit\n";
                    //addDesc += "Example Diagonal entrance -> Free Diagonal Exit\n";
                    //addDesc += "Example Orthogonal entrance -> Free Orthogonal Exit";
                    addDesc += "\nCost: " + Map.I.RM.RMD.OrientationPlatformExitCost + " pp"; 
                }
           else
           if( levelFactor < 9 )
                {
                    //addDesc = "\nReversed exit Ability: \nHero can also exit Platforms moving to the oposite direction as he entered the group.";
                    addDesc += "\nCost: " + Map.I.RM.RMD.ReversedPlatformExitCost + " pp"; ;
                }
                else
                if( levelFactor < 10 )
                {
                    //addDesc = "Monster Exit Ability: \nHero can leave platforms to any monster neighbor tile";
                    addDesc += "\nCost: " + Map.I.RM.RMD.MonsterPlatformExitCost + " pp"; ;
                }
            else
            if( levelFactor < 11 )
            {
                //addDesc = "\nInfinite Platform Steps!";
            }

            // individual plat +1 points           
            int steps = 1 + ( int ) G.Hero.Control.PlatformSteps;

            if( steps <= 20 )
                addDesc += "\n\nTotal Steps: " + steps;
            else
                addDesc += "\n\nTotal Steps: Infinite!";
            if( levelFactor >= 3 )
            {
            addDesc += "\nPlatform Points: " + HeroData.I.PlatformPoints;
            addDesc += " Global: " + Map.I.LevelStats.PlatformPoints;
            }

            float global = Map.I.LevelStats.PlatformPoints * Map.I.RM.RMD.GlobalPlatformAttBonusPerPoint;
            if( levelFactor >= 2 )
            {
                addDesc += "\nAtt Bonus: (over) " + HeroData.I.PlatformAttackBonus[ ( int ) levelFactor ] + "%";
                if( levelFactor >= 3 )
                addDesc += " (Global) " + global + "%";
            }

            string stp = "Inf";
            if( steps <= 20 ) stp = "x" + steps;           
            iconText = stp;
            if( levelFactor >= 3 )
                iconText = HeroData.I.PlatformPoints.ToString("0.") + "p - " + stp;

            break;

            case EPerkType.PLATFORM_STEPS: // Platform Steps
 
            basename = "PLATFORMSTEPS";

            int lv = ( levelFactor + MouseArtifactMult );
            steps = 1 + lv;
            addDesc = "\nEach level of this perk increases the number of steps a hero can walk over platforms.";
            if( steps <= 20 )
                addDesc += "\n\nMaximum Platform Steps: " + steps;
            else
                addDesc += "\n\nMaximum Platform Steps: Infinite!";
            break;

            case EPerkType.BARRICADE: // Barricade

            if( un.TileID == ETileType.BARRICADE )
                levelFactor = 1;
            basename = "BARRICADE";
            break;

            case EPerkType.AMBUSHER: // Ambusher

            levelFactor = ( int ) CheckLevel( un.Body.AmbusherLevel );
            basename = "AMBUSHER";

            float bonus = HeroData.I.AmbusherBonusPerTile[ levelFactor ];
            addDesc = "\nBonus: " + bonus + "% Per Tile.";

            break;

            case EPerkType.MEMORY: // Memory
            levelFactor = ( int ) CheckLevel( un.Body.MemoryLevel );
            basename = "MEMORY";
            break;

            case EPerkType.TOOLBOX: // Toolbox
            levelFactor = ( int ) CheckLevel( un.Body.ToolBoxLevel );
            basename = "TOOLBOX";
            break;

            case EPerkType.STAR: // Star                

            if( un.UnitType == EUnitType.HERO )
                levelFactor = ( int ) CheckLevel( 1 );
            if( un.Body.Stars <= 0 ) levelFactor = 0;
            if( MouseArtifact == true ) levelFactor = 1;
            basename = "STARS";
            addDesc = "\nTotal Stars: " + un.Body.Stars;
            break;

            case EPerkType.SCOUT: // Scout    
            
            if( un.UnitType == EUnitType.HERO )
                levelFactor = ( int ) CheckLevel( 1 );
            if( un.Control.ScoutLevel <= 1 ) levelFactor = 0;
            if( MouseArtifact == true ) levelFactor = 1;
            basename = "SCOUT";
            addDesc = "\nScout Radius: " + un.Control.ScoutLevel + " Tiles.";

            addDesc += "\nOver Barricade: " + un.Control.OverBarricadeScoutLevel;
            addDesc += "\nShow Resource Chance: " + un.Control.ShowResourceChance + "%";
            addDesc += "\nShow Resource Neighbor: " + un.Control.ShowResourceNeighborsChance + "%";
            break;


            case EPerkType.OVERBARRICADESCOUT: // Over Barricade Scout    

            levelFactor = ( int ) CheckLevel( un.Control.OverBarricadeScoutLevel );
            basename = "OVERBARRICADESCOUT";
            if( MouseArtifact == true ) levelFactor = 1;
            addDesc = "\nEach level of this perk increases the ability of the hero to look over barricades by one unit size.";
            break;

            case EPerkType.SHOWRESOURCECHANCE: // Show Resource Chance

            levelFactor = ( int ) CheckLevel( un.Control.ShowResourceChance );
            basename = "SHOWRESOURCECHANCE";
            if( MouseArtifact == true ) levelFactor = 1;
            addDesc = "\nChance for a tile containing a bonus rune to be revealed.";
            break;

            case EPerkType.SHOWRESOURCENEIGHBORSCHANCE: // Show Neighbor Resource Chance

            levelFactor = ( int ) CheckLevel( un.Control.ShowResourceNeighborsChance );
            basename = "SHOWRESOURCENEIGHBORSCHANCE";
            if( MouseArtifact == true ) levelFactor = 1;
            addDesc = "\nShow Resource Neighbor Chance: This determines the chance for the neighbor tiles of a revealed rune to be revealed.";
            break;

            case EPerkType.MELEEATTACK: // Melee Attack

            levelFactor = ( int ) CheckLevel( un.Body.MeleeAttackLevel );
            basename = "MELEEATTACK";

            if( un.MeleeAttack )
            {
                float sp = HeroData.I.SecondaryAttackPower[ levelFactor ];
                float  sec = Util.Percent( sp, un.MeleeAttack.TotalDamage );
                addDesc = "\n\nDamage: " + un.MeleeAttack.TotalDamage;
                if( levelFactor >= 4 )
                    addDesc += "\nSecondary: " + sec.ToString("0.0") + " (" + sp + "%)";
                iconText = "" + un.MeleeAttack.TotalDamage.ToString( "0.#" );
                addDesc += "\nSpeed: " + un.MeleeAttack.GetRealtimeSpeed() + " Hits per 10s.";
            }

            break;

            case EPerkType.RTMELEEATTACKSPEED: // Melee Attack Realtime Speed

            levelFactor = ( int ) CheckLevel( un.Body.RealtimeMeleeAttSpeed );
            basename = "RTMELEEATTACKSPEED";
            addDesc = "Each Level of this perk Increases Hero Realtime Melee Attack Speed.";
            lv = ( levelFactor + MouseArtifactMult );
            addDesc += "\n\nSpeed: " + un.MeleeAttack.GetRealtimeSpeed( lv ) + " Hits per 10s.";

            break;

            case EPerkType.RANGEDATTACK: // Ranged Attack

            levelFactor = ( int ) CheckLevel( un.Body.RangedAttackLevel );
            basename = "RANGEDATTACK";

            int _range = 0;
            if( un.RangedAttack )
            {
                _range = Map.I.RM.RMD.BaseHeroRangedAttackRange + ( int ) un.RangedAttack.TotalRange;    
                iconText = "" + un.RangedAttack.TotalDamage.ToString( "0." ) + " (" + _range + ")";
                float sp = HeroData.I.SecondaryAttackPower[ levelFactor ];
                float  sec = Util.Percent( sp, un.RangedAttack.TotalDamage );
                addDesc = "\n\nDamage: " + un.RangedAttack.TotalDamage + " - Range: " + _range ;
                if( levelFactor >= 4 )
                    addDesc += "\nSecondary: " + sec.ToString( "0.0" ) + " (" + sp + "%)";
                iconText = "" + un.RangedAttack.TotalDamage.ToString( "0.#" );
                addDesc += "\n\nSpeed: " + un.RangedAttack.GetRealtimeSpeed() + " Hits per 10s.";
            }

            break;

            case EPerkType.RTRANGEDATTACKSPEED: // Ranged Attack Realtime Speed

            levelFactor = ( int ) CheckLevel( un.Body.RealtimeRangedAttSpeed );
            basename = "RTRANGEDATTACKSPEED";
            addDesc = "Each Level of this perk Increases Hero Realtime Ranged Attack Speed.";
            lv = ( levelFactor + MouseArtifactMult );
            addDesc += "\n\nSpeed: " + un.RangedAttack.GetRealtimeSpeed( lv ) + " Hits per 10s.";

            break;

            case EPerkType.MIRE: // Mire

            levelFactor = ( int ) CheckLevel( un.Control.MireLevel );
            basename = "MIRE";

            addDesc = "Each Level of this perk decreases Realtime Monsters Movement Speed.";
            lv = ( levelFactor + MouseArtifactMult );
            addDesc += "\n\nRT Monster Speed: " + un.Control.GetMonsterRTMovSpeed( lv ) + " per 10s.";

            break;

            case EPerkType.MINING: // Mining

            levelFactor = ( int ) CheckLevel( un.Body.MiningLevel );
            basename = "MINING";
            addDesc = "This perk Allows the Hero to mine for Ore.\n(Mining Points required).";
            lv = ( levelFactor + MouseArtifactMult );
            //addDesc += "\n\nRT Monster Speed: " + un.Control.GetMonsterRTMovSpeed( lv ) + " per 10s.";
            iconText = "MP:" + Item.GetNum( ItemType.Res_Mining_Points ).ToString( "0.#" );

            break;

            case EPerkType.FISHING: // Fishing

            levelFactor = ( int ) HeroData.I.GetVal( EHeroDataVal.FISHING_LEVEL );
            levelFactor = ( int ) CheckLevel( levelFactor );
            lv = ( levelFactor + MouseArtifactMult );
            basename = "FISHING";
            addDesc = "Each Level of this perk Increases Hero Fishing Ability.\n\n";

            float val = HeroData.I.GetVal( EHeroDataVal.FISHING_HOOK_ATTACK, lv );
            addDesc += "Hook Power: " + val.ToString( "0.0" ) + "\n";
            val = HeroData.I.GetVal( EHeroDataVal.FISHING_HOOK_SPEED, lv );
            addDesc += "Hook Speed: " + val.ToString( "0.00" ) + " m/s\n";
            val = HeroData.I.GetVal( EHeroDataVal.FISHING_HOOK_RADIUS, lv );
            addDesc += "Hook Radius: " + val.ToString( "0.000" ) + " mt\n";
            lv = ( levelFactor + MouseArtifactMult );
            //addDesc += "\n\nRT Monster Speed: " + un.Control.GetMonsterRTMovSpeed( lv ) + " per 10s.";
            //iconText = "MP:" + HeroData.I.fis.ToString( "0.#" );

            break;

            case EPerkType.RESTDISTANCE: // Rest Distance

            levelFactor = ( int ) CheckLevel( un.Control.RestingLevel );
            basename = "RESTDISTANCE";

            addDesc = "Each Level of this perk decreases Monsters Resting Distance.";
            lv = ( levelFactor + MouseArtifactMult );
            int rad =  ( int ) Map.I.RM.RMD.BaseRestingDistance - lv;
            if( rad < 1 ) rad = 1;
            addDesc += "\n\nRest Radius: " + rad + " Tiles";

            break;

            case EPerkType.MAGICATTACK: // Magic Attack
            break;

            case EPerkType.MOVEMENTLEVEL: // Movement Level

            levelFactor = ( int ) CheckLevel( un.Control.MovementLevel );

            if( Map.I.RM.SD && CubeData.I.StonepathDiagonalMovement == false )
            if( Map.I.GetMud( G.Hero.Pos ) )
            if( levelFactor > 3 ) levelFactor = 3;

                basename = "MOVEMENT";

                iconText = " ";

                break;

            case EPerkType.ARROWWALKING: // Arrow Walking Level

            levelFactor = ( int ) CheckLevel( un.Control.ArrowWalkingLevel );
            basename = "ARROWWALKING";

            if( un.Control.ArrowInLevel <= 0 && un.Control.ArrowOutLevel <= 0 )
            if( un.Control.ArrowWalkingLevel > 0 )
                {
                    un.Control.ArrowInLevel  = ( int ) un.Control.ArrowWalkingLevel;
                    un.Control.ArrowOutLevel = ( int ) un.Control.ArrowWalkingLevel;
                }

            int ad = 0;
            if( Map.I.CurrentArea != -1 ) ad = Map.I.CurArea.ArrowsDestroyed;
            int totHit = 1 + Map.I.NumMonstersOverArrows + ad;

            //addDesc = "Icon Numbers Indicate Arrow Movement Level (IN - OUT)" + totHit;
            addDesc = "This perk Allows you to walk over Arrows.";
            //addDesc += "\n\nTotal Hits: " + totHit;
            //iconText = un.Control.ArrowInLevel + " - " + un.Control.ArrowOutLevel + "   ";
            //iconText = "H" + totHit + " " + un.Control.ArrowInLevel + "/" + un.Control.ArrowOutLevel;

            iconText = " ";
            break;

            case EPerkType.ARROWINLEVEL: // Arrow In Walking Level

            levelFactor = ( int ) CheckLevel( un.Control.ArrowInLevel );
            basename = "ARROWIN";

            break;

            case EPerkType.ARROWOUTLEVEL: // Arrow Out Walking Level

            levelFactor = ( int ) CheckLevel( un.Control.ArrowOutLevel );
            basename = "ARROWOUT";

            break;

            case EPerkType.ARROWFIGHTERLEVEL: // Arrow Fighter Level

            levelFactor = ( int ) CheckLevel( un.Control.ArrowFighterLevel );
            basename = "ARROWFIGHTER";

           // if( levelFactor < 2 )
            {
                addDesc = "arrow fighter 1";
            }

            break;

            case EPerkType.DEXTERITY: // Dexterity

            levelFactor = ( int ) CheckLevel( un.Body.DexterityLevel );
            basename = "DEXTERITY";
            break;

            if( levelFactor < 2 )
            {
                addDesc = "Hero can craft Masks to scare Monsters. Move Towards a Monster to pose a threat to it. A Threatened Monster cannot move for 1 turn. Works Even behind obstacles. (No diagonal, Virgin Area Only)";
            }
            else
            if( levelFactor < 3 )
            {
                addDesc = "A Threatened Monster Tries to imitate Hero Movement.\nResources Collected by Threatened Monsters are worth 3x (No diagonal, Virgin Area Only)";
            }
            else
            if( levelFactor < 4 )
            {
                addDesc = "Diagonal Threat move allowed.";
            }
            else
            if( levelFactor < 5 )
            {
                addDesc = "Threat still works if only the hero has taken Damage. (Not Monsters)";
            }
            else
            if( levelFactor < 6 )
            {
                addDesc = "Threatened Monsters can Drop Down Platforms and Light firepits up.\n Firepits need at least 1 regular wood already placed.";
            }
            else
            if( levelFactor < 7 )
            {
                addDesc = "Any monster can be threatened if No big Monster has yet been attacked.";
            }
            else
            if( levelFactor < 8 )
            {
                addDesc = "Besides moving, A threatened Monster Cannot Attack either. (only to T1 and T2)";
            }
            else
            if( levelFactor < 9 )
            {
                addDesc = "Reversed Direction Threat.";
            }
            else
            if( levelFactor < 10 )
            {
                addDesc = "Any Virgin monster can be threatened.";
            }
            else
            if( levelFactor < 11 ) 
            {
                addDesc = "Threat available in the Hero 3 frontal Rows.";
            }
            else
            if( levelFactor < 12 ) 
            {
                addDesc = "Threat still works if the monster has taken only one Hit.";
            }
            else
            if( levelFactor < 13 ) 
            {
                addDesc = "Hero frontal tile need not to be clear for Scaried monster movement to work.";
            }

            int threat = G.Hero.Body.BaseThreatDuration + Map.I.RM.RMD.BaseThreatDuration;
            addDesc += "\n\nThreat Power: +" + threat + "T";

            if( un.Body.ScaryLevel >= 1 )
                addDesc += "\nScaried in the last " + un.Body.ScaryLevel + " Turns.";
            break;

            case EPerkType.BASETHREATDURATION: // Base Threat Level

            levelFactor = ( int ) CheckLevel( un.Body.BaseThreatDuration );
            basename = "BASETHREATDURATION";
            addDesc = "This Determines the Threat Duration in turns.";

            threat = G.Hero.Body.BaseThreatDuration + levelFactor;
            addDesc += "\n\nThreat Power: +" + threat + "T";

            if( un.Body.ScaryLevel >= 1 )
                addDesc += "\nScaried in the last " + un.Body.ScaryLevel + " Turns.";
            
            break;

            case EPerkType.SCARYLEVEL: // Scary Level

            levelFactor = ( int ) CheckLevel( un.Body.ScaryLevel );
            basename = "SCARYLEVEL";

            addDesc = "Collect Scary perks to improve your scary level. The scary perk automatically turns a threatened monster into a scaried monster.\n";
            addDesc += "A scaried monster can also be manipulated by hero swing moves (rotation) or wait movement. (Monster follow hero Direction if frontal tile is walkable). ";

            addDesc += "\nScaried in the last " + levelFactor + " Turns.";
            break;

            case EPerkType.BASEFREEEXITHPLIMIT: // Base Free Exit HP Limit

            levelFactor = ( int ) CheckLevel( un.Body.BaseFreeExitHPLimit );
            basename = "BASEFREEEXITHPLIMIT";

            break;

            case EPerkType.AGILITYLEVEL: // Agility
            
            levelFactor = ( int ) CheckLevel( un.Body.AgilityLevel );
            basename = "AGILITY";
            addDesc = "";

            if( levelFactor < 2 )
            {
                addDesc = "Agility Affects all Agility Abilities.";
                addDesc += "\nAs soon as a monster is killed, its Bonus attack is turned into Agility Points.\nAgility Points are automatically converted to runes.";
            }
            else
            if( levelFactor < 3 )
            {
                addDesc += "Increased Stats.";
            }
            else
            if( levelFactor < 4 )
            {
                addDesc += "Increased Stats.";
            }
            else
            if( levelFactor < 5 )
            {
                addDesc += "Increased Stats.";
            }
            else
            if( levelFactor < 6 )
            {
                addDesc += "Every Agility Ability Level Upgrade gives extra bonus Attack to all Agility abilities";
            }
            else
            if( levelFactor < 7 )
            {
                addDesc += "Increased Stats.";
            }
            else
            if( levelFactor < 8 )
            {
                addDesc += "Increased Stats.";
            }
            else
            if( levelFactor < 9 )
            {
                addDesc += "Increased Stats.";
            }
            else
            if( levelFactor < 10 )
            {
                addDesc += "Increased Stats.";
            }
            else
            if( levelFactor < 11 )
            {
                addDesc += "Increased Stats.";
            }

            float totA = HeroData.I.AgilityPointsToRune[ levelFactor ];
            float rn = HeroData.I.AgilityRunePrize[ levelFactor ];
            addDesc += "\nAgility Points: " + HeroData.I.AgilityPoints.ToString( "0." ) + " of " + totA + " (+" + rn + ")";

            int aglPerks = ( int ) G.Hero.Body.AgilityLevel    + ( int ) G.Hero.Body.FreshAttackLevel + ( int ) G.Hero.Body.RiskyAttackLevel +
                           ( int ) G.Hero.Body.MortalJumpLevel + ( int ) G.Hero.Body.OpenFieldAtttackLevel;

            float abn = HeroData.I.AgilityAttackBonusPerPerk[ levelFactor ] * aglPerks;

            if( levelFactor >= 5 )
                addDesc += "\nBonus Attack: " + HeroData.I.AgilityAttackBonusPerPerk[ levelFactor ] + " x " + aglPerks + " = " + abn + "%";

           break;

            case EPerkType.FRESHATTACKLEVEL: // Fresh Attack

           levelFactor = ( int ) CheckLevel( un.Body.FreshAttackLevel );
           basename = "FRESHATTACK";


           if( levelFactor < 2 )
            {
                addDesc = "Fresh 1";
            }
            else
            if( levelFactor < 3 )
            {
                addDesc = "Fresh 2";
            }

           lv = ( levelFactor + MouseArtifactMult );
           addDesc = "Fresh attack ability: Every Time a monster is attacked from a new position and direction, it receives increased damage in the next turns.\nMultiple Monsters Accumulate.";
           addDesc += "\nFresh Attack Bonus: " + HeroData.I.GetVal( EHeroDataVal.AGL_PABN, lv ).ToString( "0." ) + "%";

           break;

           case EPerkType.RISKYATTACKLEVEL: // Risky Attack

           levelFactor = ( int ) CheckLevel( un.Body.RiskyAttackLevel );
           basename = "RISKYATTACK";


           if( levelFactor < 2 )
           {
               addDesc = "Risk 1";
           }
           else
           if( levelFactor < 3 )
           {
               addDesc = "Risk 2";
           }

           addDesc = "Risky Attack Ability: Gives the Target Monster attack bonus if the hero has attracted a scarab to his neighbor tiles While Attacking.";
           addDesc += "\n\nAttack Bonus: " + HeroData.I.GetVal( EHeroDataVal.AGL_RABN, levelFactor ).ToString( "0." ) + "%"; 

           break;

            case EPerkType.MORTALJUMPLEVEL: // Mortal Jump Attack
            
            levelFactor = ( int ) CheckLevel( un.Body.MortalJumpLevel );
            basename = "MORTALJUMP";


           if( levelFactor < 2 )
            {
                addDesc = "Back Mortal Jump: Hero can jumb back over a monster if there's a monster on his back tile and any other monster surrounding the target tile. (Other monsters cannot attack)";
            }
           else
            if( levelFactor < 3 )
            {
                addDesc = "Outside area Mortal Jump Allowed.";
            }
            else
            if( levelFactor < 4 )
            {
                addDesc = "Jumped over Monster Bonus Attack Awarded.";
            }
            else
            if( levelFactor < 5 )
            {
                addDesc = "Jumped over Monster Cannot Attack after Mortal Jump.";
            }
            else
            if( levelFactor < 6 )
            {
                addDesc = "Other Monster Search range increased to 2";
            }
            else
            if( levelFactor < 7 )
            {
                addDesc = "Frontal Mortal Jump Allowed.";
            }
            else
            if( levelFactor < 8 )
            {
                addDesc = "Other Monsters Bonus Attack Awarded.";
            }

           if( levelFactor >= 3 )
               addDesc += "\nAttack Bonus: " + HeroData.I.GetVal( EHeroDataVal.AGL_MJBN, levelFactor ).ToString( "0." ) + "%";

           break;

            case EPerkType.OPENFIELDATTACKLEVEL: // Open Field Attack

            
            levelFactor = ( int ) CheckLevel( un.Body.OpenFieldAtttackLevel );
            basename = "OPENFIELDATTACK";

           addDesc = "Any monster attacked in an open field receives Attack Bonus.\nOpen Field: A tile surrounded only by grass tiles.\n Hero Tile doesn't count as open. (Move attack needed),";

           if( levelFactor < 2 )
            {
                addDesc += "";
            }
            else
            if( levelFactor < 3 )
            {
                addDesc += "";
            }
            else
            if( levelFactor < 4 )
            {
                addDesc += "\n Hero tile Counts as an Open tile.";
            }
            else
            if( levelFactor < 5 )
            {
                addDesc += "";
            }
            else
            if( levelFactor < 6 )
            {
                addDesc += "\nOne tile tolerance for Open Fields. \n(Only 50% Bonus Attack Granted in this case).";
            }

           addDesc += "\n\nAttack Bonus: " + HeroData.I.GetVal( EHeroDataVal.AGL_OFBN, levelFactor ).ToString( "0." ) + "%";

           break;

            case EPerkType.ORBSTRIKER: // Orb Striker

            levelFactor = ( int ) CheckLevel( un.Body.OrbStrikerLevel );
            basename = "ORBSTRIKER";

            break;

            case EPerkType.COOPERATION: // Cooperation

            levelFactor = ( int ) CheckLevel( un.Body.CooperationLevel );
            basename = "COOPERATION";

            break;

            case EPerkType.MELEESHIELD: // Melee Shield Level

            levelFactor = ( int ) CheckLevel( un.Body.MeleeShieldLevel );

            float total = levelFactor * Map.I.RM.RMD.HeroMeleeShieldBonusPerLevel; 

            basename = "MELEESHIELD";
            if( MouseArtifact == false )
            {
                addDesc = "\nBase Shield: " + un.Body.TotalMeleeShield;
                addDesc += "\nPower: " + total + "%";
                addDesc += "\nTotal: " + Util.Percent( total, un.Body.TotalMeleeShield );
            }

            iconText = "" + total.ToString( "0." );

            break;

            case EPerkType.MISSILESHIELD: // Missile Shield Level

            levelFactor = ( int ) CheckLevel( un.Body.MissileShieldLevel );

            total = levelFactor * Map.I.RM.RMD.HeroRangedShieldBonusPerLevel;

            basename = "MISSILESHIELD";
            if( MouseArtifact == false )
            {
                addDesc = "\nBase Shield: " + un.Body.TotalMissileShield;
                addDesc += "\nPower: " + total + "%";
                addDesc += "\nTotal: " + Util.Percent( total, un.Body.TotalMissileShield );
            }
            break;

            case EPerkType.MAGICSHIELD: // Magic Shield Level
            levelFactor = ( int ) CheckLevel( un.Body.MagicShieldLevel );
            basename = "MAGICSHIELD";
            break;

            case EPerkType.SPRINTERLEVEL: // Sprinter Level
            levelFactor = ( int ) CheckLevel( un.Control.SprinterLevel );
            basename = "SPRINTER";

            float bn =  Map.I.Hero.Body.CalculateSprinterAttackBonus( levelFactor, EDamageType.MELEE );

            addDesc = "\nAttack Bonus: " + bn + "% Initial: " + HeroData.I.SprinterAttackBonus[ levelFactor ] + "%";
            iconText = "" + bn.ToString( "0." ) + "%";

            break;

            case EPerkType.FIREMASTERLEVEL: // Fire Master Level
            levelFactor = ( int ) CheckLevel( un.Body.FireMasterLevel );
            basename = "FIREMASTER";

            float per = Map.I.RM.RMD.BaseFirePower + un.Body.FirePowerLevel;

            int fire = 0;
            if( Map.I.CurrentArea != -1 ) fire = Map.I.CurArea.BonfiresLit;                                                 // per firepit fire damage power inflation
            if( Map.I.RM.HeroSector )
                fire = Map.I.RM.HeroSector.FireIgnitionCount;
            per = Util.CompoundInterest( per, fire, Map.I.RM.RMD.FirePowerInflationPerBonfire );

            float dmg = Util.Percent( per,  Map.I.Hero.MeleeAttack.TotalDamage );            
            float fireattbn = HeroData.I.AttackBonusPerFireLit[ levelFactor ];
            fireattbn = Util.Percent( Map.I.RM.RMD.FireAttackBonusFactor, fireattbn );

            addDesc += "\nHero Attack Bonus: " + fireattbn + "% per bonfire";

            if( Map.I.CurrentArea != -1 )
            {
                int cont = Map.I.CurArea.BonfiresLit;
                if( Manager.I.GameType == EGameType.CUBES )
                    cont = Map.I.RM.HeroSector.FireIgnitionCount;
                float bntot = fireattbn * cont;
                addDesc += " = " + bntot.ToString( "0.0" ) + "%";
            }

            addDesc = "";
            if( levelFactor < 2 )
            {                
                addDesc += "Step Around Firepits to place Firewood around it, lightining them up.";
                //addDesc += "Hover mouse over Icon for more Help...";
                //addDesc += "\ngrowth Bonus: " + Map.I.RM.RMD.FirePowerInflationPerBonfire + "%";
            }
            else
            if( levelFactor < 3 )
            {
                addDesc += "Hero can use inarea Bark extracted from Barricades destroyed by himself to disguise among monsters. \nThis grants Him Immunity. (Monster Attacking invalidates it, Only works around firepits).";
            }
            else
            if( levelFactor < 4 )
            {
                addDesc += "Wood Placement still works if Only monsters have taken Damage.";
            }
            else
            if( levelFactor < 5 )
            {
                addDesc += "An Unlit Firepit receives one extra firewood if there's any Fire Lit with LOS to it.";
            }
            else
            if( levelFactor < 6 )
            {
                addDesc += "Unit can Jump over Firepits.";
            }
            else
            if( levelFactor < 7 )
            {
                addDesc += "One extra Firewood for Burning LOS firepits.";
            }
            else
            if( levelFactor < 8 )
            {
                addDesc += "Firepits Receives one extra firewood if NOT ANY ONE Firepit has LOS to the hero.";
            }
            else
            if( levelFactor < 9 )
            {
                addDesc += "Monsters suffer Immediate burn Damage after Stepping on Fire.";
            }            
            else
            if( levelFactor < 10 )
            {
                addDesc += "Unit can Jump over burning Barricades.";
            }
            else
            if( levelFactor < 11 )
            {
                addDesc += "Two Extra turns allowed for fire ignition after Clearing Area.";
            }
            else
            if( levelFactor < 12 )
            {
                addDesc += "Firepit receives one extra firewood if hero is facing it and no regular firewood has been added to it yet.";
            }
            else
            if( levelFactor < 13 )
            {
                addDesc += "Frontal Wood Placement Allowed.";
            }

            int woodneeded = Map.I.RM.RMD.BaseFireWoodNeeded - ( int ) un.Body.FireWoodNeeded;
            float pits = 0;
            if( Map.I.RM.HeroSector ) pits = Map.I.RM.HeroSector.FireIgnitionCount;
            int times = 0;
            woodneeded -= times;
            if( woodneeded < 1 ) woodneeded = 1;

            string spread = "None";
            if( un.Body.FireSpreadLevel >= 1 ) spread = "L" + un.Body.FireSpreadLevel;

            //addDesc += "\n\nPower: " + per.ToString( "0." ) + "% of Melee = " + dmg.ToString( "0.# " );
            //addDesc += "\nSpread: " + spread;
            //addDesc += "  Outarea: " + un.Body.OutsideFireWoodAllowed;
            addDesc += "\n\nWood needed: " + woodneeded;
            //addDesc += "  Pits: " + pits + " of " + pitsneeded;
            if( times > 0 ) addDesc += " (-" + times + ")";
            iconText = "W" + woodneeded;
                        
            break;

            #region Fire Sub Perks

            case EPerkType.FIREPOWERLEVEL: // Fire Power

            levelFactor = ( int ) CheckLevel( un.Body.FirePowerLevel );
            basename = "FIREPOWER";

            addDesc = "This Represents the Damage taken by monsters over burning firepits Measured by a percentage of melee attack.";
             
            if(!MouseArtifact ) MouseArtifactMult = 0;
            per = Map.I.RM.RMD.BaseFirePower + ( levelFactor + MouseArtifactMult );
            dmg = Util.Percent( per, Map.I.Hero.MeleeAttack.TotalDamage );      
            addDesc += "\n\nPower: " + per.ToString( "0." ) + "% of Melee = " + dmg.ToString( "0.# " );
            break;

            case EPerkType.FIRESPREADLEVEL: // Fire Spread

            levelFactor = ( int ) CheckLevel( un.Body.FireSpreadLevel );
            basename = "FIRESPREAD";

            
            spread = "None";
            if( un.Body.FireSpreadLevel >= 1 ) spread = "L" + levelFactor + " Barricades";
            addDesc = "Spread: This determines the maximum size a neighbor barricade needs to be for the fire to spread to it. \n\n";
            addDesc += "Fire Spreading: " + spread;

            break;

            case EPerkType.FIREWOODNEEDED: // Fire Wood Needed

            levelFactor = ( int ) CheckLevel( un.Body.FireWoodNeeded );
            basename = "FIREWOODNEEDED";
            woodneeded = Map.I.RM.RMD.BaseFireWoodNeeded - levelFactor;
            if( woodneeded < 1 ) woodneeded = 1;

            addDesc = "This Indicates the amount of wood that needs to be placed in a bonfire to light it.\n\n ";
            addDesc += "Wood: " + woodneeded;

            break;

            case EPerkType.OUTSIDEFIREWOODALLOWED: // Outside Firewood Allowed  

            levelFactor = ( int ) CheckLevel( un.Body.OutsideFireWoodAllowed );
            basename = "OUTSIDEFIREWOODALLOWED";

            addDesc = "This Indicates the Number of wood that can be placed from outside area. bump against a firepit to add/subtract outarea wood.\n\n";
            addDesc += "Outarea Wood: " + levelFactor + " Per Area.";            

            break;

            case EPerkType.FIRESTARBONUS: // Fire Star Bonus

            levelFactor = ( int ) CheckLevel( un.Body.FireStarBonus );
            basename = "FIRESTARBONUS";

            addDesc = "Each 10 of this Increase Hero Star Bonus by 1 per Fire lit.";

            if(!MouseArtifact ) MouseArtifactMult = 0;
            float bfb = ( float ) ( ( float ) ( levelFactor + MouseArtifactMult ) ) / 10;
            addDesc += "\n\n Star Bonus: " + bfb.ToString( "0.#" ) + " per bonfire.";

            break;

            #endregion

            case EPerkType.BERSERKLEVEL: // Berserk Level
            levelFactor = ( int ) CheckLevel( un.Body.BerserkLevel );
            basename = "BERSERK";
            
            iconText = "x" + un.Body.NumFreeAttacks;
            if( Map.I.CurrentArea == -1 )
                iconText = "x" + HeroData.I.ExtraAttacksperBersekerLevel[ levelFactor ];
            addDesc =  "\nAttacks: " + HeroData.I.ExtraAttacksperBersekerLevel[ levelFactor ];
            addDesc += "\nAttack Power: " + HeroData.I.BersekerAttackPower[ levelFactor ] + "%";
            break;

            case EPerkType.RICOCHETLEVEL: // Ricochet Level
            if( un.RangedAttack )
            levelFactor = ( int ) CheckLevel( un.RangedAttack.RicochetLevel );
            basename = "RICOCHET";
            break;

            case EPerkType.BEEHIVETHROWERLEVEL: // Beehive thrower Level
            levelFactor = ( int ) CheckLevel( un.Body.BeeHiveThrowerLevel );
            basename = "BEEHIVETHROWER";
            break;

            case EPerkType.PSYCHICLEVEL: // Psychic Level
            levelFactor = ( int ) CheckLevel( un.Body.PsychicLevel );
            basename = "PSYCHIC";

            addDesc  = "\nShow Hint Chance: " + HeroData.I.PsychicShowHintFactor[ levelFactor ] + "%"; 
            addDesc += "\nShow Letter Chance: " + HeroData.I.PsychicShowLetterFactor[ levelFactor ] + "%"; 
            break;

            case EPerkType.SNEAKINGLEVEL: // Sneaking Level
            levelFactor = ( int ) CheckLevel( un.Control.SneakingLevel );
            basename = "SNEAKING";

            int tt = ( ( int ) HeroData.I.SneakingWakeUpExtraTurns[ levelFactor ] + Map.I.RM.RMD.BaseWakeUpTurnAmount );
            addDesc = "\n\nBonus Turns: " + ( int ) HeroData.I.SneakingWakeUpExtraTurns[ levelFactor ];
            iconText = " ";

            if( Map.I.CurrentArea != -1 )
            {
                int left = ( tt - Map.I.CurArea.DirtyLifetimeSteps );
                if( left > 0 )
                {
                    addDesc += "\nWaking up in: " + left;
                    iconText = "" + left;
                }
                else iconText = " ";
            }
            break;

            case EPerkType.SLAYERLEVEL: // Slayer

            levelFactor = ( int ) CheckLevel( un.Control.SlayerLevel );
            basename = "SLAYERLEVEL";

            addDesc = "Dragon Kiling Perk.";

            float totdist = Map.I.RM.RMD.BaseMonsterTargetRadius + (
            levelFactor * Map.I.RM.RMD.ExtraMonsterTargetRadiusPerLevel );
            addDesc += "\n\nTargetting Radius: " + totdist;

            break;

            case EPerkType.FLYINGTARGETTING: // Flying Targeting

            levelFactor = ( int ) CheckLevel( un.Control.FlyingTargetting );
            basename = "FLYINGTARGETTING";
            addDesc = "Flying Monster Target Radius.\n\n";
            totdist = Map.I.RM.RMD.BaseMonsterTargetRadius + (
            levelFactor * Map.I.RM.RMD.ExtraMonsterTargetRadiusPerLevel );
            addDesc += "Targetting Radius: " + totdist;

            break;

            case EPerkType.SLAYERANGLE: // Slayer Angle

            levelFactor = ( int ) CheckLevel( un.Control.SlayerAngle );
            basename = "SLAYERANGLE";

            lv = ( levelFactor + MouseArtifactMult );
            float angle = Map.I.RM.RMD.BaseDragonSlayerAngle + lv;
            float dragonhp = Map.I.RM.RMD.BaseDragonSlayerMaxHP - G.Hero.Control.SlayerMaxHP;
            addDesc = "Hero Cannot be attacked by dragons while positioned in their back Visualization Cone. ";
            addDesc += "\nOnly Works until the dragon has taken a certain amount of damage.";
            addDesc += "\n\nCone Angle: " + angle + " Degrees.";
            addDesc += "\nMax Dragon HP: " + dragonhp + "%.";

            break;

            case EPerkType.SLAYERMAXHP: // Slayer Max HP

            levelFactor = ( int ) CheckLevel( un.Control.SlayerMaxHP );
            basename = "UNIT_SLAYERMAXHP";
            lv = ( levelFactor + MouseArtifactMult );
            dragonhp = Map.I.RM.RMD.BaseDragonSlayerMaxHP - lv;
            addDesc = "This perk determines the maximum percent of HP a monster needs to have for the back slayer ability to work. ";
            addDesc += "\nMax Dragon HP: " + dragonhp + "%.";
            break;

            case EPerkType.DISGUISE: // Dragon Disguise

            levelFactor = ( int ) CheckLevel( un.Control.DragonDisguiseLevel );
            basename = "DISGUISE";

                       
            if( levelFactor < 2 )
            {
                addDesc = "This Perk Allows the hero to move disguised among dragons without being attacked.";
                addDesc += " Hero needs to have 4 Forest tiles around him, and needs to stay in the back of the dragon. Void if Dragon is attacked.";
            }
            else
            if( levelFactor < 3 )
            {
                addDesc += "Angle improved";


                addDesc += "Hero remain undetected until he moves.";
                addDesc += "1 free second until monster start attacking.";
                addDesc += "Always immune if theres 4 tiles around.";
                addDesc += "max hp, ";
                addDesc += "under radius decreased.";
                addDesc += "back radius influnce.";
                addDesc += "base chance upgraded.";
                addDesc += "abiliy regained if bonus dropped";
            }
            else
            if( levelFactor < 4 )
            {
                addDesc += "Angle improved";
            }
            else
            if( levelFactor < 4 )
            {
                addDesc += "One Less Tile required.";
            }

            int tiles = ( int ) HeroData.I.SlayerForestTiles[ levelFactor ];
            int dragonangle = ( int ) HeroData.I.SlayerForestDragonAngle[ levelFactor ];
            addDesc += "\nForest Tiles: " + tiles;
            addDesc += " Angle: " + dragonangle;

            break;

            case EPerkType.DRAGONBONUSDROP: // Dragon Bonus Drop

            levelFactor = ( int ) CheckLevel( un.Control.DragonBonusDropLevel );
            basename = "DRAGONBONUSDROP";
            addDesc = "DRAGONBONUSDROP.";

            break;

            case EPerkType.DRAGONBARRICADEPROTECTION: // Dragon Barricade Protection

            levelFactor = ( int ) CheckLevel( un.Control.DragonBarricadeProtection );
            basename = "DRAGONBARRICADEPROTECTION";
            addDesc = "Each Level of this perk Increases the chance for a Dragon Shot to be blocked by a Touched barricade.\n\n";
            lv = ( levelFactor + MouseArtifactMult );
            addDesc += "Chance: " + lv + "% Per Barricade Log.";

            break;

            case EPerkType.WALLDESTROYER: // Wall Destroyer Level

            levelFactor = ( int ) CheckLevel( un.Body.WallDestroyerLevel );
            basename = "WALLDESTROYER";

            int wallc = Map.I.NumWallsDestroyed;
            int free = 0;
            if( Map.I.CurrentArea != -1 )
            {
                free = Map.I.CurArea.FreeBomb;
                wallc += Map.I.CurArea.BombsUsed - free;
            }
            int avail = ( int ) ( Map.I.LevelStats.AreasCleared / HeroData.I.WallDestroyerClearedAreasPerBomb[ levelFactor ] ) - wallc;
            avail += ( int ) HeroData.I.WallDestroyerBombsPerLevel[ levelFactor ];

            addDesc += "\n\nExtra Free Bomb every: " + HeroData.I.WallDestroyerClearedAreasPerBomb[ levelFactor ] + " Cleared Areas.";
            addDesc += "\nAvailable: " + avail + " Used: " + ( wallc + free );
            iconText = "" + avail.ToString( "0." );
            break;
        }
        if( iconText == "" ) iconText = "L" + levelFactor;
    }
    private void UpdateMessages()
    {
        if( Map.I.OxygenActive )                                                                               // Oxygen Msg
        {
            string txt = "Kade: I need to take a Breath...";
            SetBigMessage( txt, Color.cyan, .1f, 4.7f, 122.8f, 85, .001f );
        }

        float tm = ( int ) Item.GetNum( ItemType.Res_Mask );                                                   // Voodoo Mask Msg
        if( tm > 0 )
        {
            string txt = "Voodoo Mask: " + tm;
            if( tm >= 100 ) txt = "Voodoo Mask: Unlimited.";
            SetBigMessage( txt, Color.yellow, .1f, 4.7f, 122.8f, 85, .001f );
        }
    }

    public void SetBigMessage( string txt, Color col, float time = -1, float x = -1, float y = -1, int fontsize = -1, float fade = 3, int turn = -1 )
    {
        if( txt == "" )
        {
            BigMessageTextTimeCounter = 0;
            BigMessageText.gameObject.SetActive( false );
            return;
        }

        if( time == -1 ) time = 100;
        //BigMessageText.gameObject.transform.localPosition = new Vector3( 4.7f, 780, 0 );

        BigMessageTextTurnCounter = -1;
        if( turn > 0 )
            BigMessageTextTurnCounter = turn + 1;

        if( x != -1 && y != -1 )
            BigMessageText.gameObject.transform.localPosition = new Vector3( x, y, 0 );

        BigMessageText.fontSize = 125;
        if( fontsize != -1 )
            BigMessageText.fontSize = fontsize;

        BigMessageTextFadeTime = fade;
        BigMessageText.text = txt;
        BigMessageTextTimeCounter = time;
        BigMessageText.gameObject.SetActive( true );
        BigMessageText.color = col;
    }

    public int GetTotMonsters()
    {
        return Map.I.RM.HeroSector.TotFlyingMonsters + Map.I.RM.HeroSector.TotNormalMonsters;
    }
    public int GetMonsterKilled()
    {
        return ( Map.I.RM.HeroSector.TotNormalMonsters - Map.I.RM.HeroSector.AliveNormalMonsters ) +
               ( Map.I.RM.HeroSector.TotFlyingMonsters - Map.I.RM.HeroSector.AliveFlyingMonsters );
    }

    public void SetTurnInfoText( string txt, int turns, Color color )
    {
        TurnInfoLabel.text = txt;
        TurnInfoLabelCount = turns;
        TurnInfoLabel.color = color;
        TurnInfoLabel.gameObject.SetActive( true );
    }
    public void UpdateTurnInfoText()
    {
        if(--TurnInfoLabelCount < 0 )
        {
            TurnInfoLabel.gameObject.SetActive( false );
        }
    }
    public void UpdatePerkPanel( EPerkType type, Unit un )
    {
        int l = -1; string b = "", d = "", ic = "";
        UI.I.GetPerkInfo( type, ref l, ref b, ref d, un, ref ic, false );                                   // Updates Perk Panel
    }
    public void EnableOverlay( Color col, float fact )
    {
        OverlaySprite.color = col;
        OverlaySprite.gameObject.SetActive( true );
        OverlayFadeFactor = fact;
    }
}
