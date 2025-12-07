using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
#if UNITY_EDITOR  
using UnityEditor;
#endif

public enum EWindowType
{
    NONE = -1,
    Normal, Training, Studies
}

public class DungeonDialog : MonoBehaviour
{
    #region Variables
    public Unit Unit;
    public UI2DSprite[] SpriteList;
    public UI2DSprite[] Resources, TrophiesSprite;
    public UI2DSprite UnlockCost1Sprite, PackMuleEmptySlot, TechEditorFocus;
    public UISprite AutoGotoCheckMark;
    public UILabel MainTextLabel, DungeonNameLabel, DungeonNumberLabel, NewDungeonLabel, 
                   DificultyLabel, GoalInfoLabel, BackButtonLabel, UnlockCostLabel1, 
                   TrainningModeButtonLabel, StudyModeButtonLabel, StudiesCompletedLabel,
                   AvailableCubesLabel, GameCompletitionLabel;
    public UILabel[] TrophiesAmountLabel;
    public UISlider DificultySlider;
    public UIToggle AutoBuyModeToggle, BlindModeToggle;
    public UIPopupList PlayerType, StartingCubePopup;
    public GameObject BluePrintUI, QuestList, InventoryGO, PackmuleGO, 
    GoalsGO, StudiesGO, TechButtonsGO, BambooFrame, InventoryBack, DarkPerkBack, DecorFolder;
    public UITable ObjectivesTable;
    public UIButton BackButton, ProvisionsModeButton, StudiesModeButton, TrainingModeButton, NavigationMapButton, AutoGotoPuzzleButton;
    public float Dificulty = 100;
    public float DificultyLimit = 0;
    public string[] DificultyNames;
    public float ErrorMessageTimer;
    public bool PanelsButton;
    public EWindowType WindowType = EWindowType.Normal;
    public bool ShowEndgameStats = false;
    public int TotalActiveGoals, ConqueredGoals, LifeTimeConqueredGoals, TotalGoals, TotalOptionalGoals;
    public float AdventureCompletion;
    public bool NewIncursion = false;
    public bool NewButtonClicked = false;
    public bool FarmButtonClicked = false;
    public bool AutoGotoEnabled = true;
    public bool StartFarm = false;
    public bool GotoNavigationMap = false;
    public bool NavigationMapButtonClick = false;
    public bool QuickRestartAllowed = true;
    public bool QuickRestart = false;                         // Only for me: presseing r to restart immediately
    public RandomMapObjectivePanel[] PanelList;
    public List<int> AvailableAdventureList;
    public int TotBronze, TotSilver, TotGold, TotDiamond, TotAdamantium, TotGenius;
    public static bool UpdateStartingCube = true;
    public int SelectedGoal = -1, Bronze, Silver, Gold;
    public bool UpdateGoalList = false;
    public int AutogateLev = -2, AvailableCubes = -2;
    #endregion

    public void OnEnable()
    {
        if( Manager.I )
        {
            if( Blueprint.BluePrintsLoaded == false ) 
                Blueprint.LoadAll();
            BluePrintWindow.I.gameObject.SetActive( false );
            Manager.I.Inventory.gameObject.SetActive( true );
            UI.I.ScrollText.gameObject.SetActive( false );
            Manager.I.Inventory.transform.localPosition = new Vector3( -160, 243, 0 );
            UI.I.FarmUI.gameObject.SetActive( false );           
            ErrorMessageTimer = 0;
            FarmButtonClicked = false;
            StartFarm = false;
            AutogateLev = -2;
            AvailableCubes = -2;
            WindowType = EWindowType.Normal;
            for( int i = 0; i < PanelList.Length; i++ )           
                PanelList[ i ].gameObject.SetActive( false );
            UI.I.BackgroundUI.gameObject.SetActive( true );
            UI.I.UIFolder.gameObject.SetActive( true );
            if( Map.I.RM.GameOver )
                ChooseAdventure( Map.I.RM.CurrentAdventure );

            Map.I.Farm.BlueprintUI.gameObject.SetActive( false );
            Map.I.Farm.BuildingUI.gameObject.SetActive( false );
            InventoryBack.SetActive( false );
            DarkPerkBack.SetActive( true );
            UI.I.SetTurnInfoText( "", 1, Color.white );
            if( UI.I.BigMessageText.text != "" )
            if( UI.I.BigMessageText.text.Substring( 0, 3 ) == "All" )
                UI.I.BigMessageTextTimeCounter = 0f;
            System.GC.Collect();
            UpdateGoalList = false;
            QuickRestartAllowed = true;
            TechButton.UpdateMatrix = true;
            TechButton.UpdateIt();
        }
    }
     public void OnDisable()
    {
        Manager.I.Inventory.gameObject.SetActive( false );
        Map.I.InvalidateInputTimer = .5f;
        if( Map.I.HideVegetation )
        {
            UI.I.BackgroundUI.gameObject.SetActive( false );                                        // Do not show background on study mode
            UI.I.UIFolder.gameObject.SetActive( false );
        }
    }
    public void UpdatePanels()
    {
        PanelList = QuestList.gameObject.GetComponentsInChildren<RandomMapObjectivePanel>();

        //Debug.Log( PanelList.Length );

        //for( int i = 0; i < PanelList.Length; i++ )
        //{
        //    Debug.Log( PanelList[ i ] );
        //}
    }

    public void Update()
    {
        UpdateIt();
        G.Inventory.UpdateIt();
        G.Packmule.UpdateIt();
        UpdateHelp();
    }

	public void UpdateIt()
    {
        if( G.Tutorial.Phase == 14 ) G.Tutorial.AdvancePhase();                                                  // Tutorial: Advance Phase to 15           

        if( Manager.I.GameType == EGameType.NAVIGATION ) return;

        if( StartFarm )
        {
            Map.I.Farm.StartIt();
            return;
        }

        if( NewIncursion ) 
            StartQuest();
        if( GotoNavigationMap ) 
        { 
            Map.I.NavigationMap.StartIt(); 
            return; 
        }

        UpdateInput();

        if( Map.I.RM.CurrentAdventure == -1 )                                                                   // No quest chosen
        {
            UpdateEmptyAdventure();
            return;
        }

        UpdateAdventureUpgrade();

        UpdateShopping();

        UpdatePackMule();

        DungeonNameLabel.text = Map.I.RM.ORMD.gameObject.name;

        UpdateStudies();

        for( int i = 0; i < PanelList.Length; i++ )                                                              // Disables all Panels First
        {
            PanelList[ i ].gameObject.SetActive( false );
        }

        if( Inventory.HoverIcon == null && ErrorMessageTimer <= 0 )
        {
            string sig = Map.I.GetAdv().QuestHelper.Signature;
            MainTextLabel.text = Util.GetText( sig, "Adventure Description" );
            if( MainTextLabel.text.Contains( "##" ) )                                                           // default description text
                MainTextLabel.text = "Welcome to " + Map.I.GetAdv().QuestHelper.QuestName + "...";
            MainTextLabel.color = Color.white;
            if( WindowType == EWindowType.Studies )
                MainTextLabel.text = Util.GetText( "STUDIES_HELP", "Main" ); else
            if( WindowType == EWindowType.Training )
                MainTextLabel.text = Util.GetText( "TRAINING_HELP", "Main" );
        }
        if( gameObject.activeSelf ) Cursor.visible = true;   
        ConqueredGoals = 0;
        if( WindowType != EWindowType.Studies )
            TechEditorFocus.gameObject.SetActive( false );
        if( TechButton.TechEditorText != null && TechButton.TechEditorText != "" )
        {
            DungeonNameLabel.text = TechButton.TechEditorText;
            DungeonNameLabel.color = Color.red;
        }
        SelectedGoal = -1;
        DungeonNameLabel.color = Color.white;

        //BambooFrame.gameObject.SetActive( true );
        //if( Map.I.RM.GameOver )
            BambooFrame.gameObject.SetActive( false );

        UI.I.DebugLabel.text = "";
        bool goalhover = false;
        for( int i = 0; i < Map.I.RM.RMD.GoalList.Length; i++ )
        {
            RandomMapGoal go = Map.I.RM.RMD.GoalList[ i ];
            go.Panel.Goal = go;
            go.UpdateAvailability();

            go.Panel.CheckBoxSprite.gameObject.SetActive( false );
            go.Panel.DescriptionLabel.color = Color.white;
            go.Panel.CheckBoxBackSprite.color = Color.red;
            if( go.Panel.Goal.ConquestCount >= 1 )
                go.Panel.CheckBoxBackSprite.color = Color.white;

            if( go.Conquered )
            {
                go.Panel.CheckBoxSprite.gameObject.SetActive( true );
                go.Panel.DescriptionLabel.color = Color.green;
                if( go.BonusGiven == false ) go.Panel.DescriptionLabel.color = Color.red;
                go.Panel.CheckBoxBackSprite.color = Color.green;
                ConqueredGoals++;
            }

            go.Panel.DescriptionLabel.text = go.GetGoalDescriptionText();
            if( go.RecordType == ERecordType.TIME_TRIAL )
            {
                go.Panel.DescriptionLabel.text = go.Panel.DescriptionLabel.text.Replace( "xx", "MOST" );
            }
            else
                go.Panel.DescriptionLabel.text = go.Panel.DescriptionLabel.text.Replace( "xx", "" + go.Trig.ConditionVal1 );

            string post = "";
            float num = 0;
            switch( go.Trig.ConditionVarID )
            {
                case ETriggerVarID.MONSTERSDEATHCOUNT: num = ( int ) Map.I.LevelStats.MonstersDeathCount; break;
                case ETriggerVarID.ROACHDEATHCOUNT: num =  Map.I.LevelStats.RoachDeathCount; break;
                case ETriggerVarID.SCARABDEATHCOUNT: num =  Map.I.LevelStats.ScarabDeathCount; break;
                case ETriggerVarID.AREASCLEARED: num =  Map.I.LevelStats.AreasCleared; break;
                case ETriggerVarID.NORMALSECTORSDISCOVERED: num =  Map.I.LevelStats.NormalSectorsDiscovered; break;
                case ETriggerVarID.SECTORSCLEARED: num =  Map.I.LevelStats.SectorsCleared; break;
                case ETriggerVarID.PERFECTAREAS: num =  Map.I.LevelStats.NumPerfectAreas; break;
                case ETriggerVarID.PERFECTSECTORS: num =  Map.I.LevelStats.NumPerfectSectors; break;
                case ETriggerVarID.ACCUMULATEDPOINTS: num =  Map.I.LevelStats.AccumulatedPoints; break;
                case ETriggerVarID.BONFIRESLIT: num =  Map.I.LevelStats.BonfiresLit; break;
                case ETriggerVarID.DIRTYBONFIRESLIT: num =  Map.I.LevelStats.DirtyBonfiresLit; break;
                case ETriggerVarID.MAXBONUSREACHED: num =  Map.I.LevelStats.MaxBonusReached; break;
                case ETriggerVarID.ACCUMULATEDBONUS: num =  Map.I.LevelStats.AccumulatedBonuses; break;
                case ETriggerVarID.UNIT_RESOURCECOLLECTED: num = Map.I.LevelStats.ResourceCollected; break;
                case ETriggerVarID.UNIT_FISHINGBONUSREACHED: num = Map.I.LevelStats.FishingBonusReached; post = "%"; break;
                case ETriggerVarID.UNIT_CONQUEREDGOALS: num = Map.I.LevelStats.ConqueredGoals; break;
            }

            if( Map.I.RM.GameOver ) num = 0;
            go.Panel.CurrentNumberLabel.text = "" + num.ToString("0.") + post;
            go.LastObjectiveAmount = num;

            if( go.TargetTime > 0 )                                                                               // Timed Goal time text update
            {
                float remaining = go.GetTimeRemaining();
                
                if( go.ConquestTime == 0 )
                {
                    go.Panel.DescriptionLabel.text += " " + Util.ToSTime( remaining );
                }
                else
                {
                    if( go.RecordType == ERecordType.TIME_TRIAL )
                        go.Panel.DescriptionLabel.text += " " + Util.ToSTime( go.TargetTime );
                    else
                        go.Panel.DescriptionLabel.text += " " + Util.ToSTime( -go.ConquestTime );
                }

                if( go.TimeisUP || remaining < 0 )
                {
                    if( go.ConquestAmount != 0 )
                        go.Panel.CurrentNumberLabel.text = "" + go.ConquestAmount;
                }

                if( go.ConquestTime != 0 )                                                                        // Description Labes color
                if( go.ConquestTime >= 0 )
                    go.Panel.DescriptionLabel.color = Color.green;
                else
                    go.Panel.DescriptionLabel.color = Color.red;
                if( Map.I.RM.GameOver )
                {
                    go.Panel.DescriptionLabel.color = Color.white;
                }
                if( go.TargetTime != 0 && go.ConquestTime == 0 )  
                    go.Panel.DescriptionLabel.color = new Color( .6f, .6f, 1, 1 );

                if( go.RecordType == ERecordType.TIME_TRIAL )
                    go.Panel.DescriptionLabel.color = Color.cyan; // new Color( .6f, .6f, 1, 1 );
            }

            if( go.RefreshTime > 0 )                                                                               // Refreshing Goal 
            {
                go.Panel.DescriptionLabel.color = Color.yellow;
                float remaining = go.UpdateRefresh();
                if( go.RefreshBonusAmount < 1 && Map.I.RM.GameOver )
                {
                    go.Panel.DescriptionLabel.text += " ( " + Util.ToSTime( remaining )+" )";
                    go.Panel.DescriptionLabel.color = Color.red;
                }
            }

            if( go.Panel.Button.state == UIButtonColor.State.Hover )                                               // Mouse over Goal
            {
                if( Map.I.RM.RMD.EnableAlternateStartingCube )
                if( Input.GetMouseButtonDown( 0 ) )                                                                // Click Goal for popup choosing
                if( Map.I.RM.GameOver ) 
                if( go.Trig.ConditionVarID == ETriggerVarID.SECTORSCLEARED )
                if( go.ConquestCount > 0 )
                    SelectedGoal = ( int ) go.Trig.ConditionVal1;

                MainTextLabel.color = Color.white;
                string awa = "" + go.TotalAwarded;
                if( go.TotalAwarded <= 0 ) awa = "Unlimited";
                int level = go.GetGoalLevel();
                MainTextLabel.text  = "Goal Information:\n";
                if( go.RecordType == ERecordType.TIME_TRIAL )
                    MainTextLabel.text = "Time Trial Goal Information:\n\n";

                MainTextLabel.text += "Goal Level: " + level;
                if( level < 1 ) MainTextLabel.text = "Goal Locked!";

                MainTextLabel.text += "\nConquered: " + go.ConquestCount + " of " + 
                awa + " Available. (Bonus: " + go.BonusesGiven + ")";

                int trial = ( int ) Item.GetNum( ItemType.Quest_Trial_Count );
                if( go.ConquestCount > 0 )
                {
                    MainTextLabel.text += "\nFirst Conquered on Trial number " + ( go.ConquestTrial + 1 ) + " of a total of " +
                    ( trial + 1 ) + " trials.";
                    MainTextLabel.text += "\nTotal Time Taken: " + Util.ToSTime( ( int ) go.TimeTryingUntilConquest );
                }
                else
                MainTextLabel.text += "\nTotal Time Trying: " + Util.ToSTime( ( int ) go.TimeTryingUntilConquest );
                MainTextLabel.text += "\nWinning Streak: " +  go.CurrentWinnigStreak + " Best: " + go.BestWinnigStreak;    

                float time = Map.I.SessionTime - Map.I.FirstCubeDiscoveredTime;
                if( Map.I.FirstCubeDiscoveredTime == 0 ) time = 0;

                if( Map.I.RM.GameOver == false )
                if( go.RecordType == ERecordType.TIME_TRIAL )
                {
                    if( time < go.TargetTime )
                        MainTextLabel.text += "Current Time: " + Util.ToSTime( time, 3 ) + "\n";
                    else
                        MainTextLabel.text += "Current Time: Time´s Up.\n";
                }

                if( go.TargetTime > 0 )
                    MainTextLabel.text += "Target Time: " + Util.ToSTime( go.TargetTime ) + "\n";
                float ctime = go.TargetTime - go.ConquestTime;

                if( go.ConquestTime != 0 )
                {
                    if( go.RecordType == ERecordType.TIME_TRIAL )
                    {
                        float cscore = ( int ) go.Trig.GetVarAmount( G.Hero );
                        MainTextLabel.text += "Score Reached: " + cscore + "\n"; 
                    }
                    else                
                        if( go.ConquestCount >= 1 )
                            MainTextLabel.text += "\nConquest Time: " + Util.ToSTime( ctime, 3 ) + "\n"; 
                  
                }

                if( go.RefreshTime > 0 )
                {
                    float rem = go.UpdateRefresh();
                    if( go.RefreshBonusAmount == go.MaxRefreshBonus )
                        MainTextLabel.text += "\nRefresh Goal Available: " + go.RefreshBonusAmount + " of " + 
                        go.MaxRefreshBonus + " - Total Refresh time: " + Util.ToSTime( go.RefreshTime ) + "\n"; 
                    else
                        MainTextLabel.text += "\nRefresh Goal Available: " + go.RefreshBonusAmount + " of " + go.MaxRefreshBonus + 
                        " +"  + go.RefreshBonusAdd + " in: " + Util.ToSTime( rem ) + "";
                }

                float chance = go.GetBonusChance();
                if( chance < 100 )
                {
                    MainTextLabel.text += "Bonus Chance: " + chance + "%\n";

                    if( Map.I.RM.GameOver == false )
                    if( go.Conquered )
                    if( go.BonusGiven ) MainTextLabel.text += "Success!\n"; 
                    else MainTextLabel.text += "Bonus Failed!\n";                
                }

                if( go.TargetBluePrint != null )
                {
                    MainTextLabel.text += "\nBonus BluePrint:\n" + go.TargetBluePrint;
                    float plants = go.GetStat( EGoalUpgradeType.BLUEPRINT_PLANTS );
                    MainTextLabel.text += "\nBonus Plants: +" + plants + 
                    "   (Stock: " + go.TargetBluePrint.FreePlants + ")\n";
                    float uses = go.GetStat( EGoalUpgradeType.BLUEPRINT_USES );
                    MainTextLabel.text += " Bonus Uses: +" + uses +
                    "   (Stock: " + go.TargetBluePrint.AvailableUses + ")"; ;
                }

                if( go.TargetRecipe != null )
                {
                    float bonus = go.GetStat( EGoalUpgradeType.RECIPE_BONUS );
                    MainTextLabel.text += "\nBonus Recipe:\n" + go.TargetRecipe;
                    if( bonus >= 1 )
                        MainTextLabel.text += "\nBonus Amount: +" + bonus;
                    int tot = Recipe.GetRecipesAvailable( go.TargetRecipe );
                    MainTextLabel.text += "   (Stock: " + tot + ")";
                }

                if( go.BonusItem != ItemType.NONE )
                {
                    float bonus = go.GetStat( EGoalUpgradeType.ITEM );
                    MainTextLabel.text += "\n\nBonus: " + Util.GetName( 
                    go.BonusItem.ToString() ) + " +" + bonus.ToString( "0.#" );
                }

                if( go.BonusItem2 != ItemType.NONE )
                {
                    float bonus = go.GetStat( EGoalUpgradeType.ITEM2 );
                    MainTextLabel.text += ", " + Util.GetName( 
                    go.BonusItem2.ToString() ) + " +" + bonus.ToString( "0.#" );
                }

                if( go.ConditionItem != ItemType.NONE )                                                          // Required Item
                    MainTextLabel.text += "\n Needs: " + go.ConditionItemAmount + " " +
                    Item.GetName( go.ConditionItem );

                string score = "Best Score:\n\n";
                go.UpdateScoreListCreation();
                for( int g = 0; g < RandomMapGoal.MaxScoreNumber; g++ )                                          // Display Best Score List
                {
                    if( go.BestScoreList[ g ] != -1 )
                    if( g < go.BestScoreDateList.Count  )
                    {
                        string bestscore = Util.ToSTime( go.BestScoreList[ g ], 3 );
                        if( go.RecordType == ERecordType.TIME_TRIAL )
                            bestscore = "Score: " + ( int ) go.BestScoreList[ g ] + " ";
                        if( RandomMapGoal.NewRecordID == g ) score += "-New- ";
                        score += "#" + ( g + 1 ) + ": " + bestscore + " ";
                        score += go.BestScoreDateList[ g ].ToString() + " (";
                        score += ( go.BestScoreTrialNumber[ g ] + 1 ) + ")\n";
                    }
                    else score += "                       \n";
                }

                if( go.BestScoreList.Count > 0 )
                    DungeonNameLabel.text = score;

                if( go.MinimumDifficulty > 0 )
                    MainTextLabel.text += "\nMinimum Difficulty: " + go.MinimumDifficulty + "%";

                if( go.DifficultyUnlocked > 0 )
                    MainTextLabel.text += "\nUnlocks Difficulty: " + go.DifficultyUnlocked + "%";
                goalhover = true;

                if( Helper.I.DebugHotKey )                                                                                                     // Right Click Conquer Defor Debug
                if( Input.GetMouseButtonDown( 1 ) )
                    {
                        if( Input.GetKey( KeyCode.LeftControl ) )
                            G.GIT( ItemType.Bronze_Trophy ).
                            PerAdventureCount[ Map.I.RM.CurrentAdventure ] = 0;
                    else
                            go.Conquer();
                    }

                if( Map.I.RM.GameOver )                                                                                                         // Auto Goto Dificulty on Click
                if( Input.GetMouseButtonDown( 0 ) )
                if( go.MinimumDifficulty > 0 )
                {
                    float dif = GetDificultyRate( go.MinimumDifficulty );
                    SetDificulty( dif );
                }
            }
            go.Upgrade( false );                                                                                                                // Shopping Info           
        }

        if( LifeTimeConqueredGoals != Item.GetNum( ItemType.Goals_Completed ) )
            Item.SetAmt( ItemType.Goals_Completed, LifeTimeConqueredGoals,                                       // set goal complete item number
        Inventory.IType.Inventory, true, Map.I.RM.CurrentAdventure );

        int totg = TotalGoals - TotalOptionalGoals;

        if( totg > 0 )
            AdventureCompletion = 100 * LifeTimeConqueredGoals / totg;
        if( Map.I.RM.GameOver )
        {
            //float percn = Item.GetNum( Inventory.IType.Inventory,                                             // Completion Percentage
            //      ItemType.Adventure_Completion, Map.I.RM.CurrentAdventure );

            //if( AdventureCompletion != percn )                                                              // Updates Adventure Completition Item Value
            Item.SetAmt( ItemType.Adventure_Completion, AdventureCompletion,
                         Inventory.IType.Inventory, true, Map.I.RM.CurrentAdventure );        
            GoalInfoLabel.text = "Available: " + Map.I.RM.RMD.GoalList.Length 
            + " of " + totg + "     Quest: "+ AdventureCompletion + "% Complete." ;
            NewDungeonLabel.text = "New Incursion\n";
            DificultySlider.enabled = true;
        }
        else
        if( Map.I.RM.RMD.GoalList.Length > 0 )
        {
            float comp = 100 * ConqueredGoals / Map.I.RM.RMD.GoalList.Length;                                // Current gameplay completition percentage
            GoalInfoLabel.text = "Conquered: " + ConqueredGoals + " of " + Map.I.RM.RMD.GoalList.Length +
            "     Objectives: " + comp + "% Complete.";
            NewDungeonLabel.text = "Finalize!\n";

            if( Map.I.RM.LastCubeReached || G.Hero.Body.Hp <= 0 )
                NewDungeonLabel.text = "Finalize!\n";
            DificultySlider.enabled = false;
        }

        Map.I.LevelStats.ConqueredGoals = ConqueredGoals;                                                   // Conquered Goals Number for goal trigger

        if( ErrorMessageTimer > 0 )
        {
            ErrorMessageTimer -= Time.deltaTime;
        }
        else Inventory.HoverIcon = null;
        
        if( Dificulty > DificultyLimit )                                                                        // Update Difficulty Limit
        {
            float dif = GetDificultyRate( DificultyLimit );
            SetDificulty( dif );
        }

        UpdateAlternateStartingCube();
        
        UI.I.FreeCamModeLabel.gameObject.SetActive( false );
        UpdateObjectives();

        UpdateTrophiesPanel();

        UpdateAvailableAdventureList();
        
        UpdatePlayerProfileBackup( false, false );                                                              // Profile backup

        Item.ResourceAdded = false;

        AutoOpenGateCheck( true );
	}
    public void UpdateHelp()
    {
		if( Input.GetKey( KeyCode.F1 ) == false ) return;
        MainTextLabel.text = Language.Get( "HELPTEXT_DIALOG" );
        MainTextLabel.text = MainTextLabel.text.Replace( "\\n", "\n" );
        MainTextLabel.color = Color.green;
    }

    public bool AutoOpenGateCheck( bool updateobj = false )
    {
        bool res = false;
        if( AutogateLev == -2 )               // use this var to avoid function below every frame
            AutogateLev = ( int ) AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.UPGRADE_AUTOOPENGATE_BUTTON );
        if( AutogateLev > 0 ) res = true;
        if( updateobj )
            AutoGotoPuzzleButton.gameObject.SetActive( res );
        if( AutoGotoCheckMark.gameObject.activeSelf == false ) res = false;
        float sum = Map.I.RM.RMD.AutoOpenGateResourceCost + Map.I.RM.RMD.GotoCheckPointResourceCost;
        if( Item.GetNum( ItemType.Energy ) < sum ) 
            res = false;
        return res;
    }

    public void UpdateAlternateStartingCube( bool selectMax = false )
    {
        if( Map.I.RM.RMD.EnableAlternateStartingCube )                                                         // Gameobj activation or not
            StartingCubePopup.gameObject.SetActive( true );                                                    // Enable the popup UI; Activates alternate starting cube selection
        else
            StartingCubePopup.gameObject.SetActive( false );                                                   // Disable the popup UI; Hides the selection if not enabled
        StartingCubePopup.items = new List<string>();                                                          // Clear previous items; Reset the popup list

                                                                                                               // Available cubes update
        int tot = Map.I.RM.RMD.MaxCubes;                                                                       // Total number of cubes; Maximum cubes for this adventure
        if( Map.I.RM.RMD.InitiallyAvailableCubes <= -1 )
             AvailableCubes = tot;                                                                             // If no limit, all cubes are available; Default to total
        else
        {
            if( AvailableCubes == -2 )
                AvailableCubes = Map.I.RM.RMD.InitiallyAvailableCubes + ( int )                                // Calculate available cubes with upgrades; Include player upgrades
            AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.INCREASE_AVAILABLE_CUBES );
        }
        if( AvailableCubes > tot ) AvailableCubes = tot;                                                       // Clamp to total; Ensure available does not exceed max
        AvailableCubesLabel.text = "Total Cubes: " + tot + " Available: " + AvailableCubes;                    // Update UI label; Show total and available cubes
        int num = 1 + ( int ) Item.GetNum( ItemType.Starting_Cube,                                             // Get the number of starting cubes player owns; Current inventory count
        Inventory.IType.Inventory, Map.I.RM.CurrentAdventure );
        if( AvailableCubes < num ) num = AvailableCubes;                                                       // Clamp num to available; Prevent selecting more than allowed

        if( selectMax )
        {
            int conquered = ( int ) Item.GetNum( ItemType.Starting_Cube,                                       // Re-fetch current owned cubes; Check against available
            Inventory.IType.Inventory, Map.I.RM.CurrentAdventure );
            if( AvailableCubes <= conquered )
            {
                num = AvailableCubes;                                                                          // Set to max if conquered >= available; Force max selection
                if( AvailableCubes > conquered ) // new
                    Item.SetAmt( ItemType.Starting_Cube, AvailableCubes,
                    Inventory.IType.Inventory, true, Map.I.RM.CurrentAdventure );                              // Update inventory if needed; Sync to max
            }
        }

        if( Helper.I.FreePlay )
            num = tot;                                                                                         // In FreePlay mode, allow all cubes; Ignore limits

        for( int i = 0; i < num; i++ )
        {
            StartingCubePopup.AddItem( "Start at Cube #" + ( i + 1 ) + " " );                                  // Populate popup list; Add entries for selection
        }

        if( num == 0 )
            StartingCubePopup.value = "Start at Cube #" + 1 + " ";                                             // Default value if no cubes; Ensure popup has valid text

        if( UpdateStartingCube )                                                                               // Quest changed. choose max cube
        {
            selectMax = true;                                                                                  // Force select max; Update only once
            UpdateStartingCube = false;                                                                        // Reset flag; Prevent repeated update
        }

        int val = 0;
        if( SelectedGoal != -1 ) val = SelectedGoal;                                                           // Check if a goal is selected; Use it as initial value

        if( val != 0 && num >= val )
            StartingCubePopup.value = "Start at Cube #" + val + " ";                                           // Apply selected goal; Update popup value
        if( selectMax )
            StartingCubePopup.value = "Start at Cube #" + num + " ";                                           // Apply max selection; Update popup value

        if( StartingCubePopup.items.Contains(                                                                  // if Itemlist does not contain selected item, chooses the last one
           StartingCubePopup.value ) == false )
        {
            int last = StartingCubePopup.items.Count;
            if( last < 1 ) last = 1;
            StartingCubePopup.value = "Start at Cube #" + last + " ";                                          // Fallback to last item; Ensure valid selection exists
        }

        int current = 0;                                                                                       // Delta change based on input; Used for forced up/down adjustments
        if( Input.GetKeyDown( KeyCode.UpArrow ) ) current++;                                                   // Increase by 1; Up arrow increments
        if( Input.GetKeyDown( KeyCode.DownArrow ) ) current--;                                                 // Decrease by 1; Down arrow decrements
        if( Input.GetKeyDown( KeyCode.RightArrow ) ) current += 10;                                            // Increase by 10; Right arrow large step
        if( Input.GetKeyDown( KeyCode.LeftArrow ) ) current -= 10;                                             // Decrease by 10; Left arrow large step

        if( current != 0 )                                                                                     // Apply change only if key pressed; Skip if no input
        if( Map.I.RM.GameOver )
        {
            string valStr = StartingCubePopup.value;                                                           // Get current text; Extract number from it
            if( !string.IsNullOrEmpty( valStr ) )
            {
                int hashIndex = valStr.IndexOf( '#' );                                                         // Find '#' in string; Number follows this
                if( hashIndex != -1 )
                {
                    int cur = 0;
                    string numStr = valStr.Substring( hashIndex + 1 ).Trim();                                  // Extract number; After '#'
                    int.TryParse( numStr, out cur );                                                           // Parse number; Convert string to int
                    current = cur + current;                                                                   // Apply delta; Increment/decrement current
                }
            }
            if( current < 1 ) current = 1;                                                                     // Clamp minimum; Can't go below 1
            if( current > num ) current = num;                                                                 // Clamp maximum; Can't exceed available cubes
            StartingCubePopup.value = "Start at Cube #" + current + " ";                                       // Update popup text; Reflect new selection
        }
    }


    public void UpdateEmptyAdventure()
    {
        for( int i = 0; i < PanelList.Length; i++ )
        {
            PanelList[ i ].gameObject.SetActive( false );
        }
        DungeonNameLabel.text = "Click here to choose a Quest...\n(M Key)";
        PlayerType.gameObject.SetActive( false );
        StartingCubePopup.gameObject.SetActive( false );
        DificultySlider.gameObject.SetActive( false );
        for( int i = 0; i < TrophiesAmountLabel.Length; i++ )
        {
            TrophiesAmountLabel[ i ].text = "";
            TrophiesSprite[ i ].gameObject.SetActive( false );
        }
        BackButtonLabel.text = "No Quest Selected.";
        UnlockCostLabel1.transform.parent.gameObject.SetActive( false );
        if( NavigationMapButtonClick == false )
            MainTextLabel.text = "No quest has been selected.";
        GoalInfoLabel.text = "";
        DungeonNumberLabel.text = "";
        PackmuleGO.gameObject.SetActive( false );
        StudiesModeButton.gameObject.SetActive( false );
        TrainingModeButton.gameObject.SetActive( false );
        ProvisionsModeButton.gameObject.SetActive( false );
    }

    public void UpdateShopping()
    {
        if( gameObject.activeSelf == false ) return;
        ProvisionsModeButton.gameObject.SetActive( false );

        if( Map.I.RM.GameOver == false )
        {
            WindowType = EWindowType.Normal;
            StudiesModeButton.gameObject.SetActive( false );
            TrainingModeButton.gameObject.SetActive( false );
        }
        else
        {            
            StudiesModeButton.gameObject.SetActive( true );
            TrainingModeButton.gameObject.SetActive( true );
        }

        bool prov = false;
        if( AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.UPGRADE_SHOP ) < 1 )                                                  // Shopping not yet unlocked
            TrainingModeButton.gameObject.SetActive( false );
        else prov = true;

        TechButton.StudiesLevel = ( int ) AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.UPGRADE_STUDIES );
        if( TechButton.StudiesLevel < 1 )                                                                                            // Studies not yet unlocked
            StudiesModeButton.gameObject.SetActive( false );
        else prov = true; 
        ProvisionsModeButton.gameObject.SetActive( prov );

        if( WindowType == EWindowType.Training )                                                                                    // Activate Trainning Mode
        {
            InventoryGO.SetActive( false );
            PackmuleGO.SetActive( false );
            GoalsGO.SetActive( true );
            StudiesGO.SetActive( false );
            DecorFolder.SetActive( false );
        }
        else
        if( WindowType == EWindowType.Normal )                                                     // switch to normal dialog mode
        {
            InventoryGO.SetActive( true );
            PackmuleGO.SetActive( true );
            GoalsGO.SetActive( true );
            StudiesGO.SetActive( false );
            DecorFolder.SetActive( true );
        }

        TrainningModeButtonLabel.text = "Training";                                              // Training button text mesh
        TrainningModeButtonLabel.color = Color.white;
        if( RandomMapGoal.AvailableGoalUpgrades > 0 )
        {
            TrainningModeButtonLabel.color = Color.green;
            if( RandomMapGoal.GoalUpgradesPurchaseable > 0 )
                TrainningModeButtonLabel.text += " +" + 
                RandomMapGoal.GoalUpgradesPurchaseable + "";
        }
        RandomMapGoal.GoalUpgradesPurchaseable = 0;
        RandomMapGoal.AvailableGoalUpgrades = 0;
    }

    public void UpdatePackMule()
    {
        PackmuleGO.gameObject.SetActive( false );
        return;
        if( WindowType == EWindowType.Normal )
            if( AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.UPGRADE_PACKMULE ) >= 1 )  // optmize function only once
            PackmuleGO.gameObject.SetActive( true );
    }

    public void UpdateStudies()
    {
        StudiesGO.gameObject.SetActive( false );
        if( WindowType == EWindowType.Studies )
        {
            TechButton.StudiesLevel = ( int ) AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.UPGRADE_STUDIES );
            if( TechButton.StudiesLevel >= 1 )
                StudiesGO.gameObject.SetActive( true );

            InventoryGO.SetActive( false );
            PackmuleGO.SetActive( false );
            GoalsGO.SetActive( false );
            StudiesGO.SetActive( true );
            DecorFolder.SetActive( false );
            TechButton.UpdateIt();
        }
        
        if( Item.ResourceAdded )
            TechButton.UpdateIt();                      // Check if theres more techs available

        StudyModeButtonLabel.text = "Studies";
        StudyModeButtonLabel.color = Color.white;
        if( TechButton.TechsAvailable > 0 )
        {
            StudyModeButtonLabel.color = Color.green;
            if( StudiesModeButton.state == UIButtonColor.State.Hover )
            {
                float perc = Util.GetPercent( TechButton.StudiesCompleted, TechButton.TotalStudies );
                StudyModeButtonLabel.text += " " + perc.ToString( "0." ) + "%";
            }
            else
            {
                StudyModeButtonLabel.text += " +" + TechButton.AffordableStudies;
            }
        }
        if( TechButton.StudiesCompleted >= TechButton.TotalStudies )
            StudyModeButtonLabel.text = "Studies 100%";
    }

    public void UpdateAvailableAdventureList()
    {
        //AvailableAdventureList = new List<int>();
        //for( int i = 0; i < Map.I.RM.RMList.Length; i++ )                                                                                                                                                             
        //    {
        //        RandomMapData adv = Map.I.RM.RMList[ i ];
        //        bool add = true;
        //        if( adv.RequiredAdventureList != null )
        //            for( int a = 0; a < adv.RequiredAdventureList.Count; a++ )
        //            {
        //                int trophy = GetTrophyCount( adv.RequiredAdventureList[ a ] );
        //                if( Helper.I.RequireTrophiesToPlay )
        //                if( trophy < 1 ) add = false;
        //            }

        //        if( add ) AvailableAdventureList.Add( i );

        //    }
        //DungeonNumberLabel.text = "" + ( int ) ( Map.I.RM.ORMD.QuestID + 1 ) + "/" + AvailableAdventureList.Count;
    }
    
    public void OnBackButtonPress()
    {
        if( Map.I.RM.GameOver == false && Map.I.Hero.Body.Hp <= 0 )
        {
            SetMsg( "Game Over!", Color.red );
            return;
        }

        if( Map.I.RM.GameOver == false )
        {
            CloseDialog();
            return;
        }
        else
        {
            UpdateAdventureUpgrade( true );
        }
    }
    public void OnTrainningModeButtonPress()
    {
        WindowType = EWindowType.Training;
    }
    public void OnStudyModeButtonPress()
    {
        WindowType = EWindowType.Studies;
        TechButton.StartIt();
    }
    public void OnProvisionsModeButtonPress()
    {
        WindowType = EWindowType.Normal;
    }
    public void UpdateAdventureUpgrade( bool purchase = false )
    {        
        if( Map.I.RM.GameOver == false )                                                                                              // Adventure Active: Only Back Button Functionalitty
        {
            BackButtonLabel.text = "Back!";
            BackButtonLabel.color = new Color32( 18, 156, 74, 255 );
            UnlockCostLabel1.transform.parent.gameObject.SetActive( false );
            return;
        }

        TechButton.UpdateAdventureUpgrade( -1, -1, purchase, ref BackButtonLabel, ref UnlockCostLabel1, 
        ref BackButton, ref UnlockCost1Sprite );

        //UnlockCostLabel1.transform.parent.gameObject.SetActive( true );
        //int level = AdventureUpgradeInfo.GetAdventureLevel();
        //if( purchase ) level += 1;
        //int id = level;                                                                                                                 // Are resources enough?
        //if( purchase ) id -= 1;

        //int max = Map.I.RM.RMD.AdventureUpgradeInfoList.Length;                                                                         // Max Adventure Level Reached
        //if( purchase == false && level >= max     ||
        //    purchase == true  && level >= max + 1 )
        //{
        //    UnlockCostLabel1.transform.parent.gameObject.SetActive( false );
        //    BackButtonLabel.text = "Max Quest Level Reached: L" + level;
        //    return;
        //}
        
        //AdventureUpgradeInfo au = Map.I.RM.RMD.AdventureUpgradeInfoList[ id ];
        //int amt = ( int ) Item.GetNum( Inventory.IType.Inventory, au.UpgradeItem1Type, 
        //    Map.I.RM.CurrentAdventure );
        //int needed = ( int ) au.UpgradeItem1Cost;
        //bool enough = true; 
        //if( amt < needed ) enough = false;
        //string itname = G.GIT( au.UpgradeItem1Type ).GetName();
        //string price = " " + needed + " " + itname;

        //bool trophy = CheckTrophyRequirement();                                                                         // Check Trophy requirement
      
        //if( BackButton.state == UIButtonColor.State.Hover )
        //{
        //    string msg = AdventureUpgradeInfo.GetUpgradeMessage( au );       
        //    msg += "\n Cost:" + price + "\n\n" + itname + " in Stock: " +
        //    Item.GetNum( Inventory.IType.Inventory, au.UpgradeItem1Type ); 
        //    SetMsg( msg, Color.yellow, .01f );
        //}

        //if( purchase )
        //{
        //    if( enough == false )                                                                                                       // Not enough resources
        //    {                           
        //        SetMsg( "ERROR: Not Enough Resources!", Color.red );
        //        return;
        //    }

        //    if( trophy == false ) return;
        //    Item.AddItem( Inventory.IType.Inventory, ItemType.AdventureLevelPurchase, 1, true, Map.I.RM.CurrentAdventure );                     // Add level and Charge resource
        //    Item.AddItem( Inventory.IType.Inventory, au.UpgradeItem1Type, 
        //    -au.UpgradeItem1Cost, true );
        //    MasterAudio.PlaySound3DAtVector3( "Cashier", transform.position );

        //    if( level <= 1 )
        //        SetMsg( "Quest Unlocked for " + price, Color.green );
        //    else
        //        SetMsg( "Quest Upgraded for " + price, Color.green );
        //}
        //else
        //{                                                                                                                              // Only Updates Unlock Cost Color and Label
        //    if( level <= 0 )
        //        BackButtonLabel.text = "Unlock Quest for: ";
        //    else
        //        BackButtonLabel.text = "Quest Level: " + level + "\nUpgrade for:";

        //    UnlockCostLabel1.text = "x" + au.UpgradeItem1Cost;                                                          

        //    if( enough == false )
        //        UnlockCostLabel1.color = Color.red;
        //    else
        //        UnlockCostLabel1.color = Color.green;

        //    UnlockCost1Sprite.sprite2D = G.GIT( au.UpgradeItem1Type ).Sprite.sprite2D;
        //}        
    }

    public bool CheckTrophyRequirement()
    {
        if( Item.GetNum( Inventory.IType.Inventory, ItemType.AdventureLevelPurchase ) >= 1 ) return true;             // Already unlocked
        if( Map.I.RM.RMList[ Map.I.RM.CurrentAdventure ].StartingAdventureLevel >= 1 ) return true;

        //if( Helper.I.RequireTrophiesToPlay )                                                       // No trophy in the required Adventure Lists                                                                                                         
        {
            string msg = "";
            //if ( Map.I.RM.RMD.RequiredAdventureList != null )
            //for( int i = 0; i < Map.I.RM.RMD.RequiredAdventureList.Count - 1; i++ )
            //    {
            //        int trophy = GetTrophyCount( Map.I.RM.RMD.RequiredAdventureList[ i ] );
            //        if( trophy < 1 )
            //        {
            //            msg += "" + Map.I.RM.RMList[ ( int )
            //            Map.I.RM.RMD.RequiredAdventureList[ i ] ].name + " Adventure.\n";
            //        }
            //    }

            if( msg != "" )
            {
                SetMsg( "WARNING: To unlock this Quest you \nneed to Conquer " +
                        "at least one Trophy in the: \n\n" + msg, Color.red );
                return false;
            }

            int silver = 0;                                                                                   // Gets Trophies count
            int gold = 0;
            int bronze = 0;
            int diamond = 0;
            int genius = 0;
            string area = Map.I.RM.RMList[ Map.I.RM.CurrentAdventure ].RequiredTrophyArea;
            for( int i = 0; i < Map.I.RM.RMList.Count; i++ )
            {
                if( GetTrophyCount( Map.I.RM.RMList[ i ].QuestID, ETrophyType.BRONZE, area ) >= 1 ) bronze++;
                if( GetTrophyCount( Map.I.RM.RMList[ i ].QuestID, ETrophyType.SILVER, area ) >= 1 ) silver++;
                if( GetTrophyCount( Map.I.RM.RMList[ i ].QuestID, ETrophyType.GOLD,   area ) >= 1 ) gold++;
                if( GetTrophyCount( Map.I.RM.RMList[ i ].QuestID, ETrophyType.DIAMOND, area ) >= 1 ) diamond++;
                if( GetTrophyCount( Map.I.RM.RMList[ i ].QuestID, ETrophyType.GENIUS, area ) >= 1 ) genius++;
            }

            string add = " In Total.";
            if( area != "" ) add = " In this Area.";
            if( Map.I.RM.RMD.RequiredBronzeTrophies > 0 )                                                     // Required Bronze trophies
            if( bronze < Map.I.RM.RMD.RequiredBronzeTrophies )
                {
                    SetMsg( "WARNING: To unlock this Quest you need to Conquer " +
                        "at least: " + Map.I.RM.RMD.RequiredBronzeTrophies +
                        " Bronze Trophies" + add + "\n Conquered: " + bronze, Color.red );
                    return false;
                }

            if( Map.I.RM.RMD.RequiredSilverTrophies > 0 )                                                     // Required silver trophies
            if( silver < Map.I.RM.RMD.RequiredSilverTrophies )
                {
                    SetMsg( "WARNING: To unlock this Quest you need to Conquer " +
                        "at least: " + Map.I.RM.RMD.RequiredSilverTrophies +
                        " Silver Trophies" + add + "\n Conquered: " + silver, Color.red );
                    return false;
                }

            if( Map.I.RM.RMD.RequiredGoldTrophies > 0 )                                                      // Required Gold trophies
            if( gold < Map.I.RM.RMD.RequiredGoldTrophies )
                {
                    SetMsg( "WARNING: To unlock this Quest you need to Conquer " +
                        "at least: " + Map.I.RM.RMD.RequiredGoldTrophies +
                        " Gold Trophies" + add + "\n Conquered: " + gold, Color.red );
                    return false;
                }

             if( Map.I.RM.RMD.RequiredDiamondTrophies > 0 )                                                  // Required Diamond trophies
             if( diamond < Map.I.RM.RMD.RequiredDiamondTrophies )
                {
                    SetMsg( "WARNING: To unlock this Quest you need to Conquer " +
                        "at least: " + Map.I.RM.RMD.RequiredDiamondTrophies +
                        " Diamond Trophies" + add + "\n Conquered: " + diamond, Color.red );
                    return false;
                }

             if( Map.I.RM.RMD.RequiredGeniusTrophies > 0 )                                                  // Required Genius trophies
             if( genius < Map.I.RM.RMD.RequiredGeniusTrophies )
                 {
                     SetMsg( "WARNING: To unlock this Quest you need to Conquer " +
                         "at least: " + Map.I.RM.RMD.RequiredGeniusTrophies +
                         " Genius Trophies" + add + "\n Conquered: " + genius, Color.red );
                     return false;
                 }

            if( Map.I.RM.RMD.RequiredMaxCapacityItem != ItemType.NONE )                                      // Required Max Capacity
            if( G.GIT( Map.I.RM.RMD.RequiredMaxCapacityItem ).MaxStack <
                Map.I.RM.RMD.RequiredMaxCapacityAmount )                                     
                {
                    SetMsg( "WARNING: To unlock this Quest you need to Have a " +
                    "Maximum Capacity of at least: " + Map.I.RM.RMD.RequiredMaxCapacityAmount 
                    + " for " + Item.GetName( Map.I.RM.RMD.RequiredMaxCapacityItem ), Color.red );
                    return false;
                }
        }
        return true;
    }
    
    public void UpdateInput()
    {
        //if( Input.GetKeyDown( KeyCode.Escape ) && Map.I.RM.GameOver == false )
        //    NewButton( false );

        if( gameObject.activeSelf )
        if( Input.GetKeyDown( KeyCode.N ) )
            NewButton( false );

        if( Helper.I.ReleaseVersion == false ||
            gameObject.activeSelf )
        if( Input.GetKeyUp( KeyCode.R ) || QuickRestart )
            NewButton( true );

        if( NavigationMapButtonClick ) 
            GotoNavigationMap = true;

        if( NewButtonClicked )
            NewIncursion = true;

        if( FarmButtonClicked )
            StartFarm = true;

        if( Input.GetKeyDown( KeyCode.M ) )
        if( Map.I.RM.GameOver )
        {
            NavigationMapButtonClick = true;
            SetMsg( "Starting Navigation Map...", Color.green, 5, 55 );       
        }

        if( Input.GetKeyDown( KeyCode.F ) )
        if( Map.I.RM.GameOver )
        {
            OnFarmButtonClickedCallBack();      
        }

        UI.I.RestartAreaButton.gameObject.SetActive( true );
        if( Map.I.RM.GameOver )
            UI.I.RestartAreaButton.gameObject.SetActive( false );

        if( gameObject.activeSelf )
        {
            G.Hero.Control.PathFinding.Path.Clear();
            Time.timeScale = 1;
        }
    }

    public void OnFarmButtonClickedCallBack()
    {
        if( Map.I.RM.GameOver == false )
        {
            SetMsg( "ERROR: Finalize Current Quest First!", Color.red, 5, 48 );
            return;
        }
        FarmButtonClicked = true;
        SetMsg( "Going to the Farm...", Color.green, 5 , 50 );
    }

    public void OnAutoGotoPuzzleButtonCallBack()
    {
        AutoGotoEnabled = !AutoGotoEnabled;
        AutoGotoCheckMark.gameObject.SetActive( AutoGotoEnabled );
    }

    public void UpdateTrophiesPanel()
    {
        for( int i = 0; i < TrophiesAmountLabel.Length; i++ )
        {
            TrophiesAmountLabel[ i ].text = "";
            TrophiesSprite[ i ].gameObject.SetActive( false );
        }

        Bronze = 0; Silver = 0; Gold = 0;
        int diamond = 0; int adamantium = 0; int genius = 0;
        bool hover = false;
        if( NavigationMapButton.state == UIButtonColor.State.Hover )
            hover = true;
        bool adv = false;
        if( Input.GetKey( KeyCode.F1 ) )
            {
                hover = false;
                adv = true;
            }
        int available = 0;
        float totperc = 0;
        float totcomp = 0;
        int questcompleted = 0;
        for( int i = 0; i < Map.I.RM.RMList.Count; i++ )
        {
            RandomMapData rm = Map.I.RM.RMList[ i ];
            if( rm.Available )
            {
                if( hover ) i = Map.I.RM.CurrentAdventure;
                int br = ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Bronze_Trophy, i );
                int sv = ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Silver_Trophy, i );
                int go = ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Gold_Trophy, i );
                int di = ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Diamond_Trophy, i );
                int pl = ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Adamantium_Trophy, i );
                int ge = ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Genius_Trophy, i );

                if( hover )
                {
                    Bronze = br; Silver = sv; Gold = go; diamond = di; adamantium = pl; genius = ge;
                    break;
                }
                else
                {
                    Bronze += Util.GetOne( br ); Silver += Util.GetOne( sv ); Gold += Util.GetOne( go );
                    diamond += Util.GetOne( di ); adamantium += Util.GetOne( pl ); genius += Util.GetOne( ge );
                }

                available++;
                float complete = Item.GetNum( Inventory.IType.Inventory, ItemType.Adventure_Completion, i );
                if( complete >= 100 ) questcompleted++;
                complete = Util.Percent( rm.QuestRelevance, complete );
                totperc += complete;
                totcomp += Util.Percent( rm.QuestRelevance, 100 );
            }
            
        }
        float comp = Util.GetPercent( totperc, totcomp );

        if( hover )
            GameCompletitionLabel.text = "Quest " + AdventureCompletion + "% Complete.";                                         // Quest complete information
        else
        {
            GameCompletitionLabel.text = "Game " + comp.ToString( "0.#" ) +                                                      // Game Complete information               
            "% Complete      Quests Completed: " + questcompleted + "/" + available;

            if( Input.GetKey( KeyCode.F1 ) )                                                                                     // press F1 for Cube conquered information
            {
                int conq = 0;
                for( int i = 0; i < Map.I.RM.RMList.Count; i++ )                                                                 // counts cubes conquered
                if ( Map.I.RM.RMList[ i ].Available )
                     conq += ( int ) Item.GetNum( ItemType.Starting_Cube,
                     Inventory.IType.Inventory, i );
                float perc = Util.GetPercent(conq, Map.I.TotalCubeCount);
                GameCompletitionLabel.text = "Cubes Cleared: " + conq + " of " + 
                Map.I.TotalCubeCount + "  " + perc.ToString( "0.0" ) + "%";                                                      // display info
            }
        }

        if( Bronze > 0 )
        {
            TrophiesAmountLabel[ 0 ].text = "x" + Bronze;                                                                         // bronze info
            if( adv )
                TrophiesAmountLabel[ 0 ].text += "/" + TotBronze +
                "\n" + Util.GetPercent( Bronze, TotBronze ).ToString( "0.#" ) + "%";
            TrophiesSprite[ 0 ].gameObject.SetActive( true );
        }
        if( Silver > 0 )
        {
            TrophiesAmountLabel[ 1 ].text = "x" + Silver;                                                                         // silver info
            if( adv )
                TrophiesAmountLabel[ 1 ].text += "/" + TotSilver +
                "\n" + Util.GetPercent( Silver, TotSilver ).ToString( "0.#" ) + "%";
            TrophiesSprite[ 1 ].gameObject.SetActive( true );
        }
        if( Gold > 0 )
        {
            TrophiesAmountLabel[ 2 ].text = "x" + Gold;                                                                            // gold info
            if( adv )
                TrophiesAmountLabel[ 2 ].text += "/" + TotGold +
                "\n" + Util.GetPercent( Gold, TotGold ).ToString( "0.#" ) + "%";
            TrophiesSprite[ 2 ].gameObject.SetActive( true );
        }
        if( diamond > 0 )
        {
            TrophiesAmountLabel[ 3 ].text = "x" + diamond;                                                                         // diamond info
            if( adv )
                TrophiesAmountLabel[ 3 ].text += "/" + TotDiamond +
                "\n" + Util.GetPercent( diamond, TotDiamond ).ToString( "0.#" ) + "%";
            TrophiesSprite[ 3 ].gameObject.SetActive( true );
        }
        if( adamantium > 0 )
        {
            TrophiesAmountLabel[ 4 ].text = "x" + adamantium;                                                                       // adamantium info
            if( adv )
                TrophiesAmountLabel[ 4 ].text += "/" + TotAdamantium +
                "\n" + Util.GetPercent( adamantium, TotAdamantium ).ToString( "0.#" ) + "%";
            TrophiesSprite[ 4 ].gameObject.SetActive( true );
        }

        if(!hover )                                                                                      // quest secrets
        {
            int amt = ( int ) Item.GetNum( ItemType.Secrets_Found );                                     // secret is using einstein icon now
            TrophiesAmountLabel[ 5 ].text = amt + "/" + Map.I.RM.RMD.QuestSecrets;
            TrophiesSprite[ 5 ].gameObject.SetActive( true );
        }
        else                                                                                             // Lifetime secrets
        {
            int amt = ( int ) G.GIT( ItemType.Secrets_Found ).Count;                                   
            TrophiesAmountLabel[ 5 ].text = amt + "/" + Map.I.TotalSecrets;
            TrophiesSprite[ 5 ].gameObject.SetActive( true );
        }
    }

    public void NewButton( bool forceFinalize )
    {
        if( forceFinalize )                                                                            // Force Finalize for Quick Restarting
        {
            if( QuickRestartAllowed == false ) return;
            if( gameObject.activeSelf == false )
            if( Helper.I.ReleaseVersion ) return;                                                      // lock over save restart in the release
            FinalizeQuest();
            QuickRestartAllowed = false;
        }

        if( Map.I.RM.GameOver )
        {
            ShowEndgameStats = false;
            if( Helper.I.FreePlay == false )
            if( Map.I.RM.RMD.Available == false )                                                                        // Adventure Unavailable
            {
                SetMsg( "ERROR: Quest not yet Available!", Color.red );
                return;
            }

            int level = AdventureUpgradeInfo.GetTechLevel();                                                             // Adventure unlock requirement
            if( Helper.I.ReleaseVersion && Helper.I.FreePlay == false )
            if( level < 1 && Map.I.RM.RMD.StartingAdventureLevel == 0 )
                {
                    SetMsg( "ERROR: Unlock Quest First!", Color.red );
                    return;
                }

            if( Helper.I.FreePlay == false )
            if( Map.I.RM.RMD.RequiredItem != ItemType.NONE )
            {
                if( AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.UPGRADE_PACKMULE ) < 1 )
                {
                    SetMsg( "ERROR: Packmule Required on this quest!\n\nUnlock the Packmule First!", Color.red );        // Unlock packmule required
                    return;
                }
                //Item.MoveItem( Inventory.IType.Packmule, Map.I.RM.RMD.RequiredItem, 1 );                               // auto move required item

                int amount = ( int ) Item.GetNum( Inventory.IType.Packmule, 
                Map.I.RM.RMD.RequiredItem, Map.I.RM.RMD.RequiredItemAmount );
                int amountneeded = Map.I.RM.RMD.GetRequiredItemAmount();

                if( amount < amountneeded )
                {
                    SetMsg( "ERROR: Packmule Item Required!\n\n" + Map.I.RM.RMD.RequiredItem.ToString() +               // No Required Item
                        " x" + amountneeded + "\n\nClick Adventure Tab in the inventory and" + 
                        " then click the item to move it to the Packmule.", Color.red );
                    return;
                }
                else
                {
                    Manager.I.PackMule.ItemList[ ( int ) Map.I.RM.RMD.RequiredItem ].CountLifetime = true;             // Start Counting Lifetime
                }
            }

            SetMsg( "Preparing Backpack!", Color.green, 5, 47 );
            NewButtonClicked = true;
            QuickRestart = false;
            GetObjectivesList( Map.I.RM.CurrentAdventure, true );
        }
        else                                                                                           // Finalize Quest
        {
            FinalizeQuest();
        }
    }

    public void FinalizeQuest()
    {
        Map.I.RM.Finalize();

        for( int i = 0; i < Map.I.RM.RMD.GoalList.Length; i++ )                                       // Winning streak fail check
        {
            if( Map.I.RM.RMD.GoalList[ i ].Conquered == false )
                Map.I.RM.RMD.GoalList[ i ].CurrentWinnigStreak = 0;
        }

        RandomMapGoal.SaveAll();                                                                       // Save Goals Data

        Manager.I.Inventory.Save();                                                                    // Save inventory

        Map.I.Finalize( true );
        InitObjectives();
        SetDificulty();
        UpdatePlayerProfileBackup( true, false );
        UI.I.GameLevelText.text = "";
        Item.ResourceAdded = true;
        TechButton.UpdateIt();
        GetObjectivesList( Map.I.RM.CurrentAdventure, true );
    }

    private void StartQuest()
    {
        if( Map.I.RM.CurrentAdventure == -1 ) return;
        ChooseAdventure( Map.I.RM.CurrentAdventure );
        Map.I.RM.GameOver = false;
        Map.I.RM.StartCubes();
        NewIncursion = false;
        NewButtonClicked = false;
        Item.AddItem( ItemType.Quest_Trial_Count, +1 );
        G.Hero.Spr.spriteId = 256;
        GetObjectivesList( Map.I.RM.CurrentAdventure, true );
    }

    public void SetMsg( string msg, Color color, float timer = 5, int size = 31 )
    {
        MainTextLabel.text = msg;
        MainTextLabel.color = color;
        ErrorMessageTimer = timer;
        MainTextLabel.fontSize = size;
    }

    public void CloseDialog()
    {
        if( Map.I.RM.GameOver ) return;
        gameObject.SetActive( false );
        InventoryGO.SetActive( false );
        PackmuleGO.SetActive( false );
    }
    public void InitObjectives()
    {
        for( int i = 0; i < Map.I.RM.RMD.GoalList.Length; i++ )         
        {
            Map.I.RM.RMD.GoalList[ i ].Init();
        }
    }

    public void ChooseAdventureButton()
    {
        if( TechButton.TechEditorActive ) return;
        if( Map.I.RM.GameOver == false )
        {
            SetMsg( "ERROR: Finalize Current Quest First!", Color.red );
            return;
        }

        NavigationMapButtonClick = true;
        SetMsg( "Starting Navigation Map...", Color.green, 5, 43 );
    }
    
    public void ChooseAdventure( int adv = -1, bool btnClick = false )
    { 
        if( adv == -1 )
        {
            Map.I.RM.CurrentAdventure = adv; 
            return;
        }

        if( Map.I.RM.GameOver == false )
        {

            SetMsg( "ERROR: Finalize Current Adventure First!", Color.red );    
            if( Map.I.RM.LastCubeReached )
                    SetMsg( "ERROR: Finalize Current Adventure First!", Color.red );
            return;
        }
         
        ShowEndgameStats = false;
        Map.I.RM.CurrentAdventure = adv;
        Map.I.RM.ORMD = Map.I.RM.RMList[ ( int ) Map.I.RM.CurrentAdventure ];

        Map.I.RM.RMD.Copy( Map.I.RM.RMList[ ( int ) Map.I.RM.CurrentAdventure ] );
        Map.I.RM.RMD.SDList = Map.I.RM.ORMD.gameObject.GetComponentsInChildren<SectorDefinition>();
        WindowType = EWindowType.Normal;
        GetObjectivesList( Map.I.RM.CurrentAdventure);        
        LoadDificulty();
        SetDificulty();
        InitObjectives();
        UpdateObjectiveIcons();
        RandomMapGoal.RestoreGoalPrefabReferences( Map.I.RM.RMD );
        if( Map.I.RM.CurrentAdventure == -1 )
            UI.I.MapLabel.text = "";
        else
            UI.I.MapLabel.text = Map.I.RM.RMList[ ( int ) Map.I.RM.CurrentAdventure ].name;
    }

    public void UpdateObjectives( bool updatetimetrying = false )
    {
        UpdateObjectiveIcons();
        if( Map.I.RM.GameOver )
            return;

        if( Sector.GetPosSectorType( G.Hero.Pos ) != Sector.ESectorType.NORMAL ) return; // new
        if( G.HS.TimeSpentOnCube < 1 ) return;  // new

        for( int i = 0; i < Map.I.RM.RMD.GoalList.Length; i++ )
        {
            RandomMapGoal go = Map.I.RM.RMD.GoalList[ i ];
            go.Panel.Goal = go;
            go.Trig.Unit = Unit;
            go.Trig.UpdateIt();

            if( updatetimetrying )
            if( go.ConquestCount <= 0 )                                  // Increment Trime Trying
                go.TimeTryingUntilConquest += Time.deltaTime;

            go.CheckConquest();
        }
    }

    public void UpdateObjectiveIcons( bool force = false )
    {
        if( Map.I.RM.CurrentAdventure == -1 ) return;
        if( force == false )
        {
            if( gameObject.activeSelf == false ) return;
            if( ShowEndgameStats ) return;
        }
        bool panelOk = false;

        for( int i = 0; i < Map.I.RM.RMD.GoalList.Length; i++ )
        {
            RandomMapGoal go = Map.I.RM.RMD.GoalList[ i ];
            
            go.Panel.TrophyIcon.gameObject.SetActive( true );
            go.Panel.TrophyIcon.color = Color.white;
            if( go.TrophyType == ETrophyType.BRONZE )
                go.Panel.TrophyIcon.sprite2D = G.GIT( ItemType.Bronze_Trophy ).Sprite.sprite2D;
            else
            if( go.TrophyType == ETrophyType.SILVER )
                go.Panel.TrophyIcon.sprite2D = G.GIT( ItemType.Silver_Trophy ).Sprite.sprite2D;
            else
            if( go.TrophyType == ETrophyType.GOLD )
                go.Panel.TrophyIcon.sprite2D = G.GIT( ItemType.Gold_Trophy ).Sprite.sprite2D;
            else
            if( go.TrophyType == ETrophyType.DIAMOND )
                go.Panel.TrophyIcon.sprite2D = G.GIT( ItemType.Diamond_Trophy ).Sprite.sprite2D;
            else
            if( go.TrophyType == ETrophyType.ADAMANTIUM )
                go.Panel.TrophyIcon.sprite2D = G.GIT( ItemType.Adamantium_Trophy ).Sprite.sprite2D;
            else
            if( go.TrophyType == ETrophyType.GENIUS )
                go.Panel.TrophyIcon.sprite2D = G.GIT( ItemType.Genius_Trophy ).Sprite.sprite2D;
            else
            {
                go.Panel.TrophyIcon.sprite2D = G.GIT( ( int ) ItemType.Bronze_Trophy ).Sprite.sprite2D;
                go.Panel.TrophyIcon.color = new Color( .001f, .001f, .001f, .001f );  // Invisible trophy to avoid the trophy disaligned bug
            }

            int id = ( int ) go.BonusItem - 1;                                                                          // Item 1
            if( go.BonusItem != ItemType.NONE )
                go.Panel.Prize1Icon.sprite2D =
                G.GIT( go.BonusItem ).Sprite.sprite2D;
            float bonus = go.GetStat( EGoalUpgradeType.ITEM );
            go.Panel.Prize1Label.text = "x" + bonus;      

            bool icon = true;
            if( bonus == 0 ) icon = false;
            if( go.BonusItem == ItemType.NONE ) icon = false;
            go.Panel.Prize1Icon.gameObject.SetActive( icon );
            go.Panel.Prize1Label.gameObject.SetActive( icon );    

            id = ( int ) go.BonusItem2 - 1;                                                                              // Item 2
            if( go.BonusItem2 != ItemType.NONE )
                go.Panel.Prize2Icon.sprite2D =
                G.GIT( go.BonusItem2 ).Sprite.sprite2D;
            bonus = go.GetStat( EGoalUpgradeType.ITEM2 );
            go.Panel.Prize2Label.text = "x" + bonus;
            icon = true;
            if( bonus == 0 ) icon = false;
            if( go.BonusItem2 == ItemType.NONE ) icon = false;
            go.Panel.Prize2Icon.gameObject.SetActive( icon );
            go.Panel.Prize2Label.gameObject.SetActive( icon );         

            go.Panel.Prize2BackIcon.gameObject.SetActive( false );
            go.Panel.Prize2Icon.color = Color.white;
            if( go.TargetBluePrint != null )                                                                            // BluePrint Bonus
            {
                if( go.TargetBluePrint.AffectedItem != ItemType.NONE )
                {
                    go.Panel.Prize2BackIcon.sprite2D =
                    G.GIT( go.TargetBluePrint.AffectedItem ).Sprite.sprite2D;
                    go.Panel.Prize2BackIcon.gameObject.SetActive( true );
                }
                
                go.Panel.Prize2Icon.sprite2D =
                G.GIT( ItemType.BluePrint_Image_1 ).Sprite.sprite2D;

                if( go.TargetBluePrint.GeneratedBuilding != BuildingType.NONE )
                    go.Panel.Prize2Icon.color = Color.yellow;
                if( go.TargetBluePrint.AffectedVariable == EVarType.Total_Building_Production_Time )
                    go.Panel.Prize2Icon.color = Color.green;
                if( go.TargetBluePrint.AffectedVariable == EVarType.Carry_Capacity )
                    go.Panel.Prize2Icon.color = Color.cyan;

                float plants = go.GetStat( EGoalUpgradeType.BLUEPRINT_PLANTS );
                float uses = go.GetStat( EGoalUpgradeType.BLUEPRINT_USES );
                if( plants >= 1 )
                    go.Panel.Prize2Label.text = "x" + plants;
                else
                if( uses >= 1 )
                     go.Panel.Prize2Label.text = "x" + uses;
                icon = true;
                if( plants == 0 && uses == 0 ) icon = false;
                go.Panel.Prize2Icon.gameObject.SetActive( icon );
                go.Panel.Prize2Label.gameObject.SetActive( icon );
            }

            if( go.TargetRecipe != null )                                                                            // BluePrint Bonus
            {
                go.Panel.Prize2BackIcon.sprite2D =
                G.GIT( go.TargetRecipe.Generated_Item_1 ).Sprite.sprite2D;
                go.Panel.Prize2BackIcon.gameObject.SetActive( true );                
                go.Panel.Prize2Icon.color = Color.white;
                go.Panel.Prize2Icon.sprite2D =
                G.GIT( ItemType.Recipe_Image_1 ).Sprite.sprite2D;
                bonus = go.GetStat( EGoalUpgradeType.RECIPE_BONUS );
                go.Panel.Prize2Label.text = "x" + bonus;
                icon = true;
                if( bonus == 0 ) icon = false;
                go.Panel.Prize2Icon.gameObject.SetActive( icon );
                go.Panel.Prize2Label.gameObject.SetActive( icon );
            }

            icon = false;
            if( go.ConditionItem != ItemType.NONE )                                                                 // Item Requirement
            {
                icon = true;
                go.Panel.RequirementIcon.sprite2D =
                G.GIT( go.ConditionItem ).Sprite.sprite2D;
                go.Panel.RequirementLabel.text = "x" + go.ConditionItemAmount;
            }

            go.Panel.RequirementIcon.gameObject.SetActive( icon );                                                  
            go.Panel.RequirementLabel.gameObject.SetActive( icon );

            if( force && panelOk == false )                                                                        // UI panel goal update
            if( go.Conquered == false )
            {
                UI.I.GoalPanel.Copy( go.Panel, go );
                panelOk = true;
                UI.I.ArtifactsText.text = go.Panel.DescriptionLabel.text + ": "                                    // find next goal
                + go.Panel.CurrentNumberLabel.text + " Done";
                UI.I.ArtifactsText.text = UI.I.ArtifactsText.text.Replace( ".", ":" );
                UI.I.UpdGoalText = false;
            }
        }
    }
    
    public void LoadDificulty()
    {
        //string adv = " " + ( int ) Map.I.RM.RMD.QuestID + " ";
        //string file = Manager.I.GetProfileFolder() + "/Dialog Prefs.dat";
        //if( ES2.Exists( file + "?tag=DificultySlider.value " + adv ) )
        //    DificultySlider.value = ES2.Load<float>( file + "?tag=DificultySlider.value " + adv );
    }

    public float GetDificultyRate( float dif )
    {
        float rate = 0;
        for( float i = 0; i <= 1; i += 0.001f )
        {
            float res = ( int ) ( Map.I.RM.RMD.MinimumDifficulty + Util.Percent( i * 100,
                                  Map.I.RM.RMD.MaximumDifficulty - Map.I.RM.RMD.MinimumDifficulty ) );

            if( res == dif ) return i;            
        }
        return rate;
    }

    public void UpdateDificultySlider()
    {
        SetDificulty( -1 );
    }

    public void SetDificulty( float force = -1 )
    {
        return;
        float dif = force;
        if( force == -1 ) dif = DificultySlider.value;
        else DificultySlider.value = force;

        Dificulty = ( int ) ( Map.I.RM.RMD.MinimumDifficulty + Util.Percent( dif * 100, 
                              Map.I.RM.RMD.MaximumDifficulty - Map.I.RM.RMD.MinimumDifficulty ) );

        float id = DificultyNames.Length - 1;
        id *= dif;

        if( ShowEndgameStats )
        {
            ShowEndgameStats = false;
            InitObjectives(); // o problema e que esta funcao inicializa os goals fazendo com que invalide o fake conquered dos cubes menores do que o atual em InitAlternateStartingCube por isso adicionei return no comeco
                              // ideia: chamar funcao somente se for gameover
        }

        //string adv = " " + ( int ) Map.I.RM.RMD.QuestID + " ";                         // temp disabled to avoid the profile -1 folder creation bug
        //string file = Manager.I.GetProfileFolder() + "/Dialog Prefs.dat";
        //ES2.Save( dif, file + "?tag=DificultySlider.value " + adv );

        Map.I.RM.RMD.Init();
        GetObjectivesList( Map.I.RM.CurrentAdventure );
        DificultyLabel.text = "Dificulty: " + DificultyNames[ Mathf.RoundToInt( id ) ] + " (" + Dificulty + "%)" + "   Max: " + DificultyLimit + "%";
    }
    public void GetObjectivesList( int adv, bool starting = false )
    {
        if( adv == -1 ) return;
        RandomMapGoal[] rmg = Map.I.RM.RMList[ adv ].GoalList;
        List< RandomMapGoal> rl = new List<RandomMapGoal>( rmg );

        DificultyLimit = Map.I.RM.RMD.MinimumDifficulty;

        if( DificultyLimit < Map.I.RM.RMD.StartingDifficultyLimit )
            DificultyLimit = Map.I.RM.RMD.StartingDifficultyLimit;

        LifeTimeConqueredGoals = 0;

        for( int i = 0; i < rl.Count; i++ )                                                                    // Loads Goals Time Conquered
        {
            rl[ i ].Load();

            if( rl[ i ].ConquestCount > 0 )                                                                    // Difficulty Limit Calculation
            if( rl[ i ].DifficultyUnlocked > DificultyLimit ) 
                DificultyLimit = rl[ i ].DifficultyUnlocked;
            if( rl[ i ].ConquestCount > 0 ) 
                LifeTimeConqueredGoals++;
        }
        
        List< int> idl = new List<int>();

        TotalActiveGoals = rmg.Length;
        TotalGoals = rmg.Length;
        TotalOptionalGoals = 0;

        for( int i = 0; i < rl.Count; i++ )
        {
            rl[ i ].Adventure = adv;
            rl[ i ].Available = true;
            bool del = false;
            if( rl[ i ].MinimumDifficulty > 0 )
            if( rl[ i ].MinimumDifficulty > Dificulty ) del = true;

            if( rl[ i ].TotalAwarded > 0 )
            if( rl[ i ].ConquestCount >= rl[ i ].TotalAwarded )
            {
                del = true;
                TotalActiveGoals--;
            }

            if( starting )
            if( rl[ i ].RefreshTime > 0 )
                {
                    float refresh = rl[ i ].UpdateRefresh();                                         // Refreshed goals only appear in game over screen
                    if( rl[ i ].RefreshBonusAmount < 1 )
                    {
                        del = true;
                    }
                }

            if( rl[ i ].BonusesAwarded > 0 )
            if( rl[ i ].BonusesGiven >= rl[ i ].BonusesAwarded )
                {
                    del = true;
                    TotalActiveGoals--;
                }

            if( del )
            {
                //Debug.Log( rl[ i ] );
                idl.Add( i );
                rl[ i ].Available = false;
            }
        }
        
        for( int i = idl.Count - 1; i >= 0; i-- )
        {
            if( rl.Count <= 0 ) break;
            rl.RemoveAt( idl[ i ] );
        }

        Map.I.RM.RMD.GoalList = rl.ToArray();

        if( Map.I.RM.RMD.GoalList.Length > PanelList.Length)
            G.Error( "Map.I.RM.RMD.GoalList.Length > PanelList.Length - create more panels" );

        for( int i = 0; i < Map.I.RM.RMD.GoalList.Length; i++ )
        {
            Map.I.RM.RMD.GoalList[ i ].Panel = PanelList[ i ];
            if( Map.I.RM.RMD.GoalList[ i ].TrophyType == 
                ETrophyType.GENIUS ) TotalOptionalGoals++;
        }

        UpdateObjectiveIcons();
        UpdateGoalList = false;
    }

    public int GetTrophyCount( int adv, ETrophyType type = ETrophyType.NONE, string area = "" )
    {
        int num = 0;
        int id = Map.I.RM.RMList[ adv ].QuestID;
        if( area != "" )
        if( Map.I.RM.RMList[ adv ].QuestHelper.SubFolder != area ) return 0;
        if( type == ETrophyType.NONE || type == ETrophyType.BRONZE )
        num += ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Bronze_Trophy,  id );
        if( type == ETrophyType.NONE || type == ETrophyType.SILVER )
        num += ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Silver_Trophy,  id );
        if( type == ETrophyType.NONE || type == ETrophyType.GOLD )
        num += ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Gold_Trophy,    id );
        if( type == ETrophyType.NONE || type == ETrophyType.DIAMOND )
        num += ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Diamond_Trophy, id );
        if( type == ETrophyType.NONE || type == ETrophyType.ADAMANTIUM )
        num += ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Adamantium_Trophy, id );
        if( type == ETrophyType.NONE || type == ETrophyType.GENIUS )
        num += ( int ) Item.GetNum( Inventory.IType.Inventory, ItemType.Genius_Trophy,  id );
        return num;
    }

    public void UpdatePlayerProfileBackup( bool force, bool exit )
    {
        if( !exit )
        if( Map.I.RM.GameOver == false ) return;
        if( force == false )
        if( Input.GetKeyDown( KeyCode.B ) == false ) return;
        if( Application.platform != RuntimePlatform.WindowsPlayer ) return;
        string source = Manager.I.GetProfileFolder();
        string dest = System.Environment.GetFolderPath( System.Environment.SpecialFolder.MyDocuments );
        //string dest = Application.dataPath + "/Player Profile Backups/";  set dest to game root
        dest += "/My Games/NEO/Player Profile Backups/";
        if( !System.IO.Directory.Exists( dest ) )
        {
            System.IO.Directory.CreateDirectory( dest );
        }

        string exittxt = "";
        if( exit ) exittxt = "Game Exit - ";
        string subfolder = "";
        for( int i = 1; i < 99999; i++ )
        {
            string date = System.DateTime.Now.ToString( "dd-MM-yyyy_hh-mm-ss" );
            subfolder = dest + "Profile Backup - " + exittxt + date + "/Profile 0/";
            if( !System.IO.Directory.Exists( subfolder ) )
            {
                System.IO.Directory.CreateDirectory( subfolder );
                Debug.Log( subfolder );
                break;
            }
        }

        if( System.IO.Directory.Exists( subfolder ) )
        {
            string[ ] files = System.IO.Directory.GetFiles( source );
            // Copy the files and overwrite destination files if they already exist.
            foreach( string s in files )
            {                // Use static Path methods to extract only the file name from the path.
                string fileName = System.IO.Path.GetFileName( s );
                string fullpath = subfolder + fileName;
                System.IO.File.Copy( s, fullpath, true );
                //G.Deb( fullpath + " Copied. " );
            }
            G.Deb( "Profile Backuped. " + subfolder );
        }
        else
        {
            G.Error( "Source path does not exist!" );
        }
    }
}
