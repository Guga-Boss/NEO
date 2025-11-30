using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ETrophyType
{
    NONE = 0, BRONZE, SILVER, GOLD, DIAMOND, ADAMANTIUM, GENIUS
}

public enum ERecordType
{
    NONE = 0, TRIGGER_EFFECT, TIME_TRIAL
}

public class RandomMapGoal : MonoBehaviour
{
    #region Variables
    [TabGroup( "Link" )]
    public GoalUpgradeInfo[ ] GoalUpgradeInfoList = null;
    [TabGroup( "Link" )]
    public RandomMapObjectivePanel Panel;
    [TabGroup( "Main" )]
    public int ID = -1;
    [TabGroup( "Main" )]
    public int Adventure = -1;
    [TabGroup( "Main" )]
    public bool Conquered = false;
    [TabGroup( "Main" )]
    public bool BonusGiven = false;
    [TabGroup( "Main" )]
    public bool MessageShown = false;
    [Space( 10 )]
    [TabGroup( "Bonus" )]
    public ETrophyType TrophyType = ETrophyType.NONE;
    [TabGroup( "Main" )]
    public bool TimeisUP;
    [TabGroup( "Main" )]
    public bool Available;
    [TabGroup( "Main" )]
    public int InitialLevel = 1;
    [TabGroup( "Main" )]
    public int LevelsPurchased = 0;
    [TabGroup( "Main" )]
    public int TotalAwarded = 0;
    [Space( 10 )]
    [TabGroup( "Bonus" )]
    public int BonusesAwarded = 0;
    [TabGroup( "Main" )]
    public int ConquestCount = 0;
    [TabGroup( "Bonus" )]
    public int BonusesGiven = 0;
    [TabGroup( "Chance" )]
    public float InitialBonusChance = 100;
    [TabGroup( "Chance" )]
    public float[ ] BonusChanceList = null;
    [TabGroup( "Chance" )]
    public float BonusChanceInflation = 0;
    [TabGroup( "Chance" )]
    public float MinBonusChance = 0;
    [TabGroup( "Chance" )]
    public float MaxBonusChance = 100;
    [TabGroup( "Req" )]
    public ItemType ConditionItem = ItemType.NONE;
    [TabGroup( "Req" )]
    public float ConditionItemAmount = 0;
    [TabGroup( "Bonus" )]
    [Space( 10 )]
    public ItemType BonusItem = ItemType.NONE;
    [TabGroup( "Bonus" )]
    public float BonusItemValue = 0;
    [TabGroup( "Bonus" )]
    [Space( 10 )]
    public ItemType BonusItem2 = ItemType.NONE;
    [TabGroup( "Bonus" )]
    public float BonusItem2Value = 0;
    [Space( 10 )]
    [TabGroup( "Bonus" )]
    public Blueprint TargetBluePrint = null;
    [TabGroup( "Bonus" )]
    //[HideInInspector]
    public string TargetBluePrintID = "";     
    [TabGroup( "Bonus" )]
    public float BluePrintBonusPlants = 1;
    [TabGroup( "Bonus" )]
    public float BluePrintBonusUses = 2;
    [Space( 10 )]
    [TabGroup( "Bonus" )]
    public Recipe TargetRecipe = null;
    //[HideInInspector]
    [TabGroup( "Bonus" )]
    public string TargetRecipeBuildingID = "";
    //[HideInInspector]
    [TabGroup( "Bonus" )]
    public string TargetRecipeID = "";
    [TabGroup( "Bonus" )]
    public int RecipeBonusAmount = 1;
    [Space( 10 )]
    [TabGroup( "Bonus" )]
    public bool ShowAdvanced = false;
    [Space( 10 )]
    [TabGroup( "Bonus" )]
    [ShowIf( "ShowAdvanced" )]
    public float[ ] BonusItemValueList = null;
    [TabGroup( "Bonus" )]
    [ShowIf( "ShowAdvanced" )]
    public float ExtraBonusPerDifficulty = 0;
    [TabGroup( "Bonus" )]
    [ShowIf( "ShowAdvanced" )]
    public int ExclusiveToAdventure = -1;
    [TabGroup( "Refresh" )]
    public float RefreshTime = 0;        // Time to wait after conquering it
    [TabGroup( "Refresh" )]
    public int MaxRefreshBonus = 1;      // maximmum refresh bonuses that can be accumulated
    [TabGroup( "Refresh" )]
    public int RefreshBonusAmount = 0;   // refresh Bonuses granted
    [TabGroup( "Refresh" )]
    public int  RefreshBonusAdd = 1;     // refresh Bonuses granted every time cunter expires
    [TabGroup( "Dif" )]
    public float MinimumDifficulty = 0;  // 0 == Disabled
    [TabGroup( "Dif" )]
    public float DifficultyUnlocked = 0;
    [TabGroup( "Record" )]
    public float TargetTime = 0;
    [TabGroup( "Record" )]
    public ERecordType RecordType = ERecordType.TRIGGER_EFFECT;
    [TabGroup( "Record" )]
    public float ConquestTime = 0;
    [TabGroup( "Record" )]
    public float ConquestAmount = 0;
    [TabGroup( "Record" )]
    public float LastObjectiveAmount = 0;
    [TabGroup( "Record" )]
    public System.DateTime LastConquestTime = System.DateTime.MinValue;
    [TabGroup( "Record" )]
    public static int MaxScoreNumber = 10;
    [TabGroup( "Record" )]
    public List<float> BestScoreList = new List<float>( MaxScoreNumber );
    [TabGroup( "Record" )]
    public List<System.DateTime> BestScoreDateList = new List<System.DateTime>( MaxScoreNumber );
    [TabGroup( "Record" )]
    public List<int> BestScoreTrialNumber = new List<int>( MaxScoreNumber );
    [TabGroup( "Record" )]
    public int ConquestTrial = -1;
    [TabGroup( "Record" )]
    public float TimeTryingUntilConquest = 0;
    [TabGroup( "Record" )]
    public static int NewRecordID = -1;
    [TabGroup( "Record" )]
    public int CurrentWinnigStreak = 0;
    [TabGroup( "Record" )]
    public int BestWinnigStreak = 0;
    [TabGroup( "Link" )]
    public MyTrigger Trig;
    public static int CurrentChosenGoal = 0;
    public static int GoalUpgradesPurchaseable = 0;
    public static int AvailableGoalUpgrades = 0;

    #endregion
    #region Buttons
    #if UNITY_EDITOR
    [HorizontalGroup( "Split", 0.5f )]
    [Button( "Edit Tilemap...", ButtonSizes.Large ), GUIColor( 1f, 0.52f, 0.1f )]
    public void EditQuestButton()
    {
        MapSaver ms = GameObject.Find( "Areas Template Tilemap" ).GetComponent<MapSaver>();
        ms.EditQuestDataCallBack();
    }
    
    [HorizontalGroup( "Split", 0.5f )]
    [Button( "Prev", ButtonSizes.Large ), GUIColor( 1, 1f, 0 )]
    public void PrevButton()
    {
        RandomMapData rm = transform.parent.transform.parent.GetComponent<RandomMapData>();
        if( --CurrentChosenGoal < 0 )
            CurrentChosenGoal = rm.GoalList.Length - 1;
        Selection.activeGameObject = rm.GoalList[ CurrentChosenGoal ].gameObject;
    }

    [HorizontalGroup( "Split", 0.5f )]
    [Button( "Next", ButtonSizes.Large ), GUIColor( 1, 1f, 0 )]
    public void NextButton()
    {
        RandomMapData rm = transform.parent.transform.parent.GetComponent<RandomMapData>();
        if( ++CurrentChosenGoal == rm.GoalList.Length )
            CurrentChosenGoal = 0;
        Selection.activeGameObject = rm.GoalList[ CurrentChosenGoal ].gameObject;
    }

    [Button( "Add Header", ButtonSizes.Medium ), GUIColor( 1f, 0.52f, 0.1f )]
    public void AddHeader()
    {
        GameObject ob = GameObject.Find( "Farm" );
        Farm f = ob.GetComponent<Farm>();
        f.UpdateListsCallBack();
        string txt = gameObject.name.Substring( 5 );
        if( ID + 1 < 10 )
            gameObject.name = txt.Insert( 0, "00" + ( ID + 1 ) + "  " );
        else
        if( ID + 1 < 100 )
            gameObject.name = txt.Insert( 0, "0" + ( ID + 1 ) + "  " );
    }
    #endif
    #endregion
    public void Init()
    {
        Conquered = false;
        BonusGiven = false;
        TimeisUP = false;
        MessageShown = false;
        ConquestTime = 0;
        ConquestAmount = 0;
        LastObjectiveAmount = 0;
        LastConquestTime = System.DateTime.MinValue;
        //ConquestTrial = -1;
        //TimeTryingUntilConquest = 0; 
        UpdateScoreListCreation();
    }
    public void UpdateScoreListCreation()
    {
        if( BestScoreList != null )
            if( BestScoreList.Count == MaxScoreNumber ) return;
        BestScoreList = new List<float>( MaxScoreNumber );
        BestScoreDateList = new List<System.DateTime>( MaxScoreNumber );
        BestScoreTrialNumber = new List<int>( MaxScoreNumber );
        for( int i = 0; i < MaxScoreNumber; i++ )                                         
        {
            BestScoreList.Add( -1 );
            System.DateTime date = new System.DateTime( 1, 1, 1, 1, 1, 1, 1 );
            BestScoreDateList.Add( date );
            BestScoreTrialNumber.Add( -1 );
        }
    }

    public void Conquer()
    {
        if( G.HS == null ) return;
        if( G.HS.Type != Sector.ESectorType.GATES )                               // Hero needs to leave cube to win goal prizes
        {
            ShowMessage();                                                        // Otherwise, just shows message and quit
            return;
        }

        if( ConquestTime == 0 )
            ConquestTime = GetTimeRemaining();
        if( RecordType != ERecordType.TIME_TRIAL )
        if( TimeisUP ) return;
        if( Available == false ) return;
        Item.IgnoreMessage = true;

        if( Panel.CheckBoxSprite.gameObject )
            Panel.CheckBoxSprite.gameObject.SetActive( true );

        Conquered = true;                                                                                       // set conquered var state to true

        if( ConquestCount == 0 )                                                                                // Conquest for the first time, Store trial #
        {
            ConquestTrial = ( int ) Item.GetNum( ItemType.Quest_Trial_Count );
        }

        ConquestCount++;

        CurrentWinnigStreak++;                                                                                  // Winning streak
        if( CurrentWinnigStreak > BestWinnigStreak )
            BestWinnigStreak = CurrentWinnigStreak;

        string ob = GetGoalDescriptionText();
        ob = ob.Replace( "xx", "" + Trig.ConditionVal1 );

        BonusGiven = true;

        float chance = GetBonusChance();

        if( Util.Chance( chance ) == false )                                                                    // Sort Failed, No Bonus
        {
            BonusGiven = false;
            Vector2 pos = Map.I.Hero.Pos + new Vector2( Random.Range( -2, 2 ), Random.Range( -2, 2 ) );
            if( Helper.I.ReleaseVersion )
                Message.CreateMessage( ETileType.NONE, "Objective Reached!\nBonus Sort Failed! ("
                + chance + "%)\n" + ob, pos, new Color( 0, 1, 0, 1 ), true, false, 12, 1, .1f );
            return;
        }
        
        string rs = "";
        string bn = "";
        bool res = false;
        ItemType it = ItemType.NONE;

        if( BonusItem != ItemType.NONE )                                                                             // Item
        {
            float bonus = GetStat( EGoalUpgradeType.ITEM );
            bn += "\n--Bonus:--";
            res = Item.AddItem( Inventory.IType.Inventory, BonusItem,
                               bonus, true, ExclusiveToAdventure );
            if( res ) bn += "\n" + Item.GetName( BonusItem ) + " +" + bonus.ToString( "0.#" );
        }

        if( TargetRecipe != null )                                                                                    // Recipe
        {
            int bonus = ( int ) GetStat( EGoalUpgradeType.RECIPE_BONUS );
            if( bonus >= 1 )
                Item.AddItem( Inventory.IType.Inventory,
                ItemType.Recipe_Image_1, bonus );

            bn = "\n--Bonus:--";
            if( TargetRecipe.RecipesAvailable == -1 )
                TargetRecipe.RecipesAvailable = bonus;
            else
                TargetRecipe.RecipesAvailable += bonus;
            bn += "\nRecipe: +" + bonus;
            Blueprint.SaveAll();                                                                                     // recipes available  needs to be saved in the blueprint file since its global
        }

        if( TargetBluePrint != null )                                                                                // Blueprint
        {
            float plants = GetStat( EGoalUpgradeType.BLUEPRINT_PLANTS );
            float uses = GetStat( EGoalUpgradeType.BLUEPRINT_USES );
            if( plants >= 1 )
                Item.AddItem( Inventory.IType.Inventory,
                ItemType.BluePrint_Image_1, plants );

            bn = "\n--Bonus:--";
            TargetBluePrint.FreePlants += plants;
            TargetBluePrint.AvailableUses += uses;
            if( TargetBluePrint.AvailableUses > 100 )
                TargetBluePrint.AvailableUses = 100;
            if( plants > 0 )
                bn += "\nBlueprint Plants: +" + plants;
            if( uses > 0 )
                bn += "\nBlueprint Uses: +" + uses;
            Blueprint.SaveAll();
        }
        else                                                                                                        // Item 2 prize wont be given if theres a blueprint as prize
        if( BonusItem2 != ItemType.NONE )                                                                           // Item 2
        {
            float bonus = GetStat( EGoalUpgradeType.ITEM2 );
            bn = "\n--Bonus:--";
            res = Item.AddItem( Inventory.IType.Inventory, BonusItem2,
                               bonus, true, ExclusiveToAdventure );
            if( res ) bn += "\n" + Item.GetName( BonusItem2 ) + " +" + bonus.ToString( "0.#" );
        }

        if( TrophyType == ETrophyType.BRONZE )                                                                       // Trophy
        {
            bn = "\nBronze Trophy!";
            Item.AddItem( Inventory.IType.Inventory, ItemType.Bronze_Trophy, 1, true, ExclusiveToAdventure );
            it = ItemType.Bronze_Trophy;
        }
        if( TrophyType == ETrophyType.SILVER )
        {
            bn = "\nSilver Trophy!";
            Item.AddItem( Inventory.IType.Inventory, ItemType.Silver_Trophy, 1, true, ExclusiveToAdventure );
            it = ItemType.Silver_Trophy;
        }
        if( TrophyType == ETrophyType.GOLD )
        {
            bn = "\nGold Trophy!";
            Item.AddItem( Inventory.IType.Inventory, ItemType.Gold_Trophy, 1, true, ExclusiveToAdventure );
            it = ItemType.Gold_Trophy;
        }
        if( TrophyType == ETrophyType.DIAMOND )
        {
            bn = "\nDiamond Trophy!";
            Item.AddItem( Inventory.IType.Inventory, ItemType.Diamond_Trophy, 1, true, ExclusiveToAdventure );
            it = ItemType.Diamond_Trophy;
        }
        if( TrophyType == ETrophyType.ADAMANTIUM )
        {
            bn = "\nAdamantium Trophy!";
            Item.AddItem( Inventory.IType.Inventory, ItemType.Adamantium_Trophy, 1, true, ExclusiveToAdventure );
            it = ItemType.Adamantium_Trophy;
        } 
        if( TrophyType == ETrophyType.GENIUS )
        {
            bn = "\nGenius Trophy!";
            Item.AddItem( Inventory.IType.Inventory, ItemType.Genius_Trophy, 1, true, ExclusiveToAdventure );
            it = ItemType.Genius_Trophy;
        }
        
        //Helper.I.FloatVal += cog;
        //Debug.Log( cog + "  " + perc + "  " + dif +  " FV " + Helper.I.FloatVal );

        string msg = "Objective Reached!\n";
        if( RecordType == ERecordType.TIME_TRIAL )
        {
            Panel.DescriptionLabel.text = GetGoalDescriptionText();                                            // Time trial msg
            Panel.DescriptionLabel.text = Panel.DescriptionLabel.text.Replace( "xx", "MOST" );
            msg = "Time Trial: Time´s up!!\n" +
            Panel.DescriptionLabel.text + " in " + Util.ToSTime( TargetTime ) +
            "\nAmount: " + ( int ) Trig.GetVarAmount( G.Hero );
            ob = "";
        }
        //if( Helper.I.ReleaseVersion )
            Message.CreateMessage( ETileType.NONE, it, msg + ob + rs + bn,                                    // Displays Success Message
            Map.I.Hero.Pos + Util.GetRandVector2( 3 ), new Color( 0, 1, 0, 1 ), 
       Util.Chance( 50 ), Util.Chance( 50 ), 12, .1f, .1f );
       BonusesGiven++;

       LastConquestTime = System.DateTime.Now;                                                                // Store conquest time

       RefreshBonusAmount--;

       UpdateScoreList();                                                                                     // Update Score List     

       if( G.HS ) G.HS.GoalConquered = true;                     

       Item.IgnoreMessage = false;
       UI.I.UpdGoalText = true;
    }
    private void ShowMessage()
    {
        if( MessageShown ) return;
        string ob = GetGoalDescriptionText();
        ob = ob.Replace( "xx", "" + Trig.ConditionVal1 );
        Message.GreenMessage( "Goal Conquered!\n" + ob );
        MessageShown = true;
    }
    public void UpdateScoreList()
    {
        int max = 0;
        bool ok = false;
        UpdateScoreListCreation();
        float score = Map.I.RecordTime;                                                                       // Score based on time
        //float score = Map.I.SessionTime - Map.I.FirstCubeDiscoveredTime;
        if( RecordType == ERecordType.TIME_TRIAL )                                                            // Is Time trial, so score is based in trigger ID amount
        {
            score = ( int ) Trig.GetVarAmount( G.Hero );
        } 

        for( int i = 0; i < MaxScoreNumber; i++ )                                                             // Insert score in the Best Score List
        {
            if( BestScoreList[ i ] == -1 ||
              ( RecordType == ERecordType.TRIGGER_EFFECT && score < BestScoreList[ i ] ) ||                   // less time
              ( RecordType == ERecordType.TIME_TRIAL     && score > BestScoreList[ i ] ) )                    // or higher score
            {
                BestScoreList.Insert( i, score );
                BestScoreDateList.Insert( i, System.DateTime.Now );
                int trial = ( int ) Item.GetNum( ItemType.Quest_Trial_Count );
                BestScoreTrialNumber.Insert( i, trial );
                max = i + 1;
                NewRecordID = i;
                ok = true;
                if( ConquestCount > 1 )
                    Message.GreenMessage( "New Record!!!" );
                break;
            }
        }

        if( ok )                                                                                                    // remove last elment in the list
        {
            BestScoreList.RemoveAt( MaxScoreNumber );
            BestScoreDateList.RemoveAt( MaxScoreNumber );
            BestScoreTrialNumber.RemoveAt( MaxScoreNumber );
        }

        for( int i = 1; i < max; i++ )                                                                              // Sorts score list        
        for( int j = 0; j < max - i; j++ )
        {
            if( ( RecordType == ERecordType.TRIGGER_EFFECT && BestScoreList[ j ] > BestScoreList[ j + 1 ] ) ||      // less time
                ( RecordType == ERecordType.TIME_TRIAL     && BestScoreList[ j ] < BestScoreList[ j + 1 ] ) )       // or higher score
            {
                System.DateTime temp = BestScoreDateList[ j ];
                BestScoreDateList[ j ] = BestScoreDateList[ j + 1 ];
                BestScoreDateList[ j + 1 ] = temp;
                float temp1 = BestScoreList[ j ];
                BestScoreList[ j ] = BestScoreList[ j + 1 ];
                BestScoreList[ j + 1 ] = temp1;
                int temp2 = BestScoreTrialNumber[ j ];
                BestScoreTrialNumber[ j ] = BestScoreTrialNumber[ j + 1 ];
                BestScoreTrialNumber[ j + 1 ] = temp2;
            }
        }        
    }

    public float GetBonusChance()
    {
        float res = InitialBonusChance + ( BonusChanceInflation * BonusesGiven );

        if( BonusChanceList != null && BonusChanceList.Length > 0 )                                                 // If theres a BonusChanceList, ignore the rest
        {
            int id = BonusesGiven;
            if( id > BonusChanceList.Length - 1 ) id = BonusChanceList.Length - 1;
            res = BonusChanceList[ id ];
        }

        res = Mathf.Clamp( res, MinBonusChance, MaxBonusChance );
        return res;
    }

    public void Save( string nm = "" )
    {
        if( Manager.I.SaveOnEndGame == false ) return;
        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Goal" + nm + ".NEO";                                    // Provides filename
        else file += "/Goal.NEO";

        RandomMapData rm = Map.I.RM.RMList[ Map.I.RM.CurrentAdventure ];
        string key = rm.QuestHelper.Signature + " " + name.Substring( 0, 4 ) + " ";

        G.Txt = "";
        G.CL = 0;
        int GoalFileVersion = 1;

        G.Txt += "" + GoalFileVersion + " : file ver\n";
        G.Txt += "" + G.Farm.UniqueID + " : Player ID\n";
        G.Txt += G.SB( Conquered )  + " : Conquered\n";
        G.Txt += "" + ConquestCount + " : conquest count\n";
        G.Txt += "" + BonusesGiven + " : bonuses given\n";
        G.Txt += "" + LevelsPurchased + " : levels purchased\n";
        G.Txt += LastConquestTime.ToBinary() + " : time binary\n";                                          // Save Last Conquest Time
        G.Txt += "" + RefreshBonusAmount + " : refresh bn amount\n";
        G.Txt += "" + ConquestTrial + " : conquest trial\n";
        G.Txt += "" + TimeTryingUntilConquest + " : time trying\n";
        G.Txt += "" + CurrentWinnigStreak + " : current winning streak\n";
        G.Txt += "" + BestWinnigStreak + " : best winning streak\n";

        UpdateScoreListCreation();

        G.Txt += "\n";
        G.Txt += "" + BestScoreList.Count + " : total score -------\n";
        G.Txt += "\n";

        for( int i = 0; i < BestScoreList.Count; i++ )                                          // Save Best Score List
        {
            G.Txt += BestScoreList[ i ] + " : best score " + i + "\n";
            G.Txt += BestScoreDateList[ i ].ToBinary() + " : best score date " + i + "\n";
            G.Txt += BestScoreTrialNumber[ i ] + " : best score trial number " + i + "\n";
            G.Txt += "\n";
        }

        ES2.Save( G.Txt, file + "?tag=Goal Info " + key );                                       // Save to file
    }

    public void Load( string nm = "" )
    {
        //Debug.Log( Map.I.RM.CurrentAdventure );                                               // Enable this to reset information about this goal. bugs may be caused if quests are moved or relocated, etc
        //Init();
        //Save( false );
        //return;
        
        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Goal" + nm + ".NEO";                                  // Provides filename
        else file += "/Goal.NEO";
 
        if( Adventure == -1 ) return;
        UpdateScoreListCreation();
        if( ES2.Exists( file ) )
        {
            RandomMapData rm = Map.I.RM.RMList[ Map.I.RM.CurrentAdventure ];
            string key = rm.QuestHelper.Signature + " " + name.Substring( 0, 4 ) + " ";
            string txt = "";
            if( ES2.Exists( file + "?tag=Goal Info " + key ) )
                txt = ES2.Load<string>( file + "?tag=Goal Info " + key );

            G.LineSplit = Regex.Split( txt, "\r\n|\r|\n" );
            G.CL = 0;

            if( G.LineSplit.Length >= 10 )
            {
                int filever = G.GI();
                G.CL++;                                                                             // Read Player unique id here
                bool conq = G.GB();
                if( nm != "" )
                    Conquered = conq;                                                               // Only load Conquered if its not a cube save
                ConquestCount = G.GI();
                BonusesGiven = G.GI();
                LevelsPurchased = G.GI();
                LastConquestTime = System.DateTime.FromBinary( G.GL() );
                if( ConquestCount <= 0 ) 
                    LastConquestTime = System.DateTime.MinValue;                                   // loaded a goal without conquests: set null time
                RefreshBonusAmount = G.GI();
                ConquestTrial = G.GI();
                float trying = G.GF();
                if( nm == "" )
                    TimeTryingUntilConquest = trying;                                              // TimeTryingUntilConquest is cummulative, do not load while cube loading
                CurrentWinnigStreak = G.GI();
                BestWinnigStreak = G.GI();

                G.CL++;
                int sz = G.GI();
                G.CL++;

                BestScoreDateList = new List<System.DateTime>();
                BestScoreList = new List<float>();
                BestScoreTrialNumber = new List<int>();

                for( int i = 0; i < sz; i++ )
                {
                    BestScoreList.Add( G.GF() );
                    BestScoreDateList.Add( System.DateTime.FromBinary( G.GL() ) );
                    BestScoreTrialNumber.Add( G.GI() );
                    G.CL++;
                }
            }
            else
            {
                ConquestCount = 0;
                BonusesGiven = 0;
                RefreshBonusAmount = MaxRefreshBonus;
                LastConquestTime = System.DateTime.MinValue;
                BestScoreList = new List<float>( MaxScoreNumber );
                BestScoreDateList = new List<System.DateTime>( MaxScoreNumber );
                BestScoreTrialNumber = new List<int>( MaxScoreNumber );
                ConquestTrial = -1;
                TimeTryingUntilConquest = 0;
                CurrentWinnigStreak = 0;
                BestWinnigStreak = 0;
            }
        }
    }

    public float GetBonusAmount( float bonus )
    {
        if( BonusItemValueList != null && BonusItemValueList.Length > 0 )                                                 // If theres a BonusItemValueList, ignore the rest
        {
            int id = BonusesGiven;
            if( id > BonusItemValueList.Length - 1 ) id = BonusItemValueList.Length - 1;
            bonus = BonusItemValueList[ id ];
        }
        
        if( ExtraBonusPerDifficulty != 0 )                                                                                // Extra Bonus per Dificulty Level
        {
            float extradif = Map.I.RM.DungeonDialog.Dificulty - MinimumDifficulty;
            float extra = extradif * ExtraBonusPerDifficulty;
            bonus += extra;
        }

        if( bonus < 0 ) bonus = 0;
        return bonus;
    }

    public void CheckConquest()
    {
        if( TargetTime > 0 )                                                                       // Timed Goal
        {
            float remaining = GetTimeRemaining();
            if( remaining <= 0 && BonusGiven == false )
            {
                if( TimeisUP == false )
                    ConquestAmount = LastObjectiveAmount;
                TimeisUP = true;                                                                   // Out of Time: No prize               
            }

            if( remaining > 0 && BonusGiven )
                ConquestAmount = LastObjectiveAmount;
        }

        if( Conquered == false )
        {
            if( GetGoalLevel() >= 1 )
            {
                if( RecordType == ERecordType.TIME_TRIAL )                                         // Time Trial Condition Check
                {
                    float time = Map.I.SessionTime - Map.I.FirstCubeDiscoveredTime;
                    if( time >= TargetTime  )
                        Conquer();
                }
                else                  
                {
                    bool req = true;
                    if( ConditionItem != ItemType.NONE )                                          // Item Requirement Check
                    {
                        float amt = Item.GetNum( ConditionItem );
                        if( amt < ConditionItemAmount ) req = false;
                    }

                    if( Trig.CheckConditionOperation() && req )                                   // Regular Goal Condition Check
                        Conquer();
                }
            }
        }        
    }

    public float GetTimeRemaining()
    {
        return TargetTime - Map.I.SessionTime + Map.I.FirstCubeDiscoveredTime;
    }

    public void Upgrade( bool purchase )
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf == false ) return;
        Panel.UpgradeButton.gameObject.SetActive( true );

        if( GoalUpgradeInfoList == null ) return;
         int max = GoalUpgradeInfoList.Length + InitialLevel;

        int level = GetGoalLevel();
        if( purchase ) level += 1;
        int id = level;                                                                                                                 
        if( purchase ) id -= 1;
        id -= InitialLevel;
        if( id >= GoalUpgradeInfoList.Length ) id = GoalUpgradeInfoList.Length - 1;

        int shoplevel = ( int ) AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.UPGRADE_SHOP );
        if( id >= max || shoplevel < 1 || GoalUpgradeInfoList.Length <= 0 )
        {
            Panel.UpgradeButton.gameObject.gameObject.SetActive( false );
            return;
        }    

        GoalUpgradeInfo gu = GoalUpgradeInfoList[ id ];                
        if( purchase == false && level >= max ||
            purchase == true && level >= max + 1 )
        {
            Panel.UpgradeButton.gameObject.gameObject.SetActive( false );
            return;
        }
        else
        {
            Panel.UpgradeBonusIcon.gameObject.SetActive( true );
            Panel.UpgradeBonusAmountLabel.gameObject.SetActive( true );
            Panel.UpgradeBonusLabel.gameObject.SetActive( true );

            Panel.UpgradePriceAmountLabel.text = "x" + gu.UpgradeCostAmount;     
            if( gu.UpgradeCostType !=  ItemType.NONE )
                Panel.UpgradePriceIcon.sprite2D = G.GIT( gu.UpgradeCostType ).Sprite.sprite2D;
            Panel.UpgradeBonusAmountLabel.text = "+" + gu.UpgradeBonusAmount;

            if( BonusItem != ItemType.NONE )
            if( gu.UpgradeType == EGoalUpgradeType.ITEM )
                Panel.UpgradeBonusIcon.sprite2D = 
                G.GIT( BonusItem ).Sprite.sprite2D;
            if( BonusItem2 != ItemType.NONE )
            if( gu.UpgradeType == EGoalUpgradeType.ITEM2 )
                Panel.UpgradeBonusIcon.sprite2D = 
                G.GIT( BonusItem2 ).Sprite.sprite2D;
            if( gu.UpgradeType == EGoalUpgradeType.RECIPE_BONUS )
                Panel.UpgradeBonusIcon.sprite2D = 
                G.GIT( ItemType.Recipe_Image_1 ).Sprite.sprite2D;
            if( gu.UpgradeType == EGoalUpgradeType.BLUEPRINT_PLANTS ||
                gu.UpgradeType == EGoalUpgradeType.BLUEPRINT_USES )
                Panel.UpgradeBonusIcon.sprite2D = 
                G.GIT( ItemType.BluePrint_Image_1 ).Sprite.sprite2D;

            if( level < 1 )
            {
                Panel.UpgradeBonusIcon.gameObject.SetActive( false );
                Panel.UpgradeBonusAmountLabel.gameObject.SetActive( false );
                Panel.UpgradeBonusLabel.gameObject.SetActive( false );
            }
        }

        int amt = ( int ) Item.GetNum( Inventory.IType.Inventory, gu.UpgradeCostType, Map.I.RM.CurrentAdventure );                 // Are resources enough?
        int needed = ( int ) gu.UpgradeCostAmount;
        bool enough = true;
        if( amt < needed ) enough = false;
        int need = needed - amt;

        if( purchase )
        {
            if( gu.ShopLevelRequired > 0 )
            if( shoplevel < gu.ShopLevelRequired )
            {
                Map.I.RM.DungeonDialog.SetMsg( "Upgrade Shop Level! +" + ( gu.ShopLevelRequired - shoplevel ), Color.red );
                return;
            }
             
            if( enough == false )                                                                                                       // Not enough resources
            {
                Map.I.RM.DungeonDialog.SetMsg( "ERROR: Not Enough Resources! Needs: " + need + " More.", Color.red );
                return;
            }

   
            Item.AddItem( Inventory.IType.Inventory, gu.UpgradeCostType, -gu.UpgradeCostAmount, true );                       // Add level and Charge resource
            MasterAudio.PlaySound3DAtVector3( "Cashier", transform.position );

            string cost = " " + needed + " " + gu.UpgradeCostType + "!";
            if( level <= 1 )
                Map.I.RM.DungeonDialog.SetMsg( "Goal Unlocked for " + cost, Color.green );
            else
                Map.I.RM.DungeonDialog.SetMsg( "Goal Upgraded for " + cost, Color.green );

            LevelsPurchased++;
            Save();
        }
        else
        {                                                                                                                              // Only Updates Unlock Cost Color and Label
            if( level <= 0 )
                Panel.UpgradeButtonLabel.text = "Unlock for: ";
            else
                Panel.UpgradeButtonLabel.text = "Upgrade to L" + ( level + 1 ) + ":";

            if( enough == false )
                Panel.UpgradePriceAmountLabel.color = Color.red;
            else
            {
                Panel.UpgradePriceAmountLabel.color = Color.green;
                GoalUpgradesPurchaseable++;
            }
            AvailableGoalUpgrades++;
            Panel.UpgradeBonusLabel.color = Color.white;
            Panel.UpgradeBonusLabel.text = "Item Bonus: ";
            if( gu.UpgradeType == EGoalUpgradeType.RECIPE_BONUS )
                Panel.UpgradeBonusLabel.text = "Recipe: ";
            if( gu.UpgradeType == EGoalUpgradeType.BLUEPRINT_PLANTS )
                Panel.UpgradeBonusLabel.text = "BP Plants: ";
            if( gu.UpgradeType == EGoalUpgradeType.BLUEPRINT_USES )
                Panel.UpgradeBonusLabel.text = "BP Uses: ";

            if( gu.ShopLevelRequired > 0 )
            if( shoplevel < gu.ShopLevelRequired )
            {
                Panel.UpgradeBonusLabel.text = "Shop +" + ( gu.ShopLevelRequired - shoplevel );
                Panel.UpgradeBonusLabel.color = Color.red;
            }
        }

        if( Map.I.RM.DungeonDialog.WindowType == EWindowType.Normal )
            Panel.UpgradeButton.gameObject.SetActive( false );
    }

    public int GetGoalLevel()
    {
        return InitialLevel + LevelsPurchased;
    }

    public void UpdateAvailability()
    {
        Panel.gameObject.SetActive( false );

        if( Map.I.RM.DungeonDialog.WindowType == EWindowType.Training )
        {
            Panel.gameObject.SetActive( true );
            Panel.UpgradeButton.gameObject.SetActive( true );
        }
        else
        {
            int level = GetGoalLevel();

            if( level >= 1 )
                Panel.gameObject.SetActive( true );

            Panel.UpgradeButton.gameObject.SetActive( false );
        }
    }

    public float GetStat( EGoalUpgradeType type )
    {
        float val = 0;
        int level = GetGoalLevel();
        level -= InitialLevel;

        switch( type )
            {
                case EGoalUpgradeType.ITEM:
                val = GetBonusAmount( BonusItemValue );
                for( int i = 0; i < level; i++ )
                if ( GoalUpgradeInfoList[ i ].UpgradeType == EGoalUpgradeType.ITEM )
                    val += GoalUpgradeInfoList[ i ].UpgradeBonusAmount;
                break;
                case EGoalUpgradeType.ITEM2:
                val = GetBonusAmount( BonusItem2Value );
                for( int i = 0; i < level; i++ )
                if ( GoalUpgradeInfoList[ i ].UpgradeType == EGoalUpgradeType.ITEM2 )
                    val += GoalUpgradeInfoList[ i ].UpgradeBonusAmount;
                break;
                case EGoalUpgradeType.RECIPE_BONUS:
                val = GetBonusAmount( RecipeBonusAmount );
                for( int i = 0; i < level; i++ )
                if ( GoalUpgradeInfoList[ i ].UpgradeType == EGoalUpgradeType.RECIPE_BONUS )
                    val += GoalUpgradeInfoList[ i ].UpgradeBonusAmount;
                break;
                case EGoalUpgradeType.BLUEPRINT_PLANTS:
                val = GetBonusAmount( BluePrintBonusPlants );
                for( int i = 0; i < level; i++ )
                if ( GoalUpgradeInfoList[ i ].UpgradeType == EGoalUpgradeType.BLUEPRINT_PLANTS )
                    val += GoalUpgradeInfoList[ i ].UpgradeBonusAmount;
                break;
                case EGoalUpgradeType.BLUEPRINT_USES:
                val = GetBonusAmount( BluePrintBonusUses );
                for( int i = 0; i < level; i++ )
                if ( GoalUpgradeInfoList[ i ].UpgradeType == EGoalUpgradeType.BLUEPRINT_USES )
                    val += GoalUpgradeInfoList[ i ].UpgradeBonusAmount;
                break;
            }
        return val;
    }

    public float UpdateRefresh()
    {
        if( RefreshTime == 0 ) return 0;
        System.TimeSpan difference = System.DateTime.Now.Subtract( LastConquestTime );
        float sec = RefreshTime - ( float ) difference.TotalSeconds;

        if( RefreshBonusAmount != MaxRefreshBonus )
        {
            if( sec <= 0 )
            {
                RefreshBonusAmount += RefreshBonusAdd;
                LastConquestTime = System.DateTime.MinValue;
                Save();
                Map.I.RM.DungeonDialog.UpdateGoalList = true;
            }
        }
        else
        {
            LastConquestTime = System.DateTime.MinValue;
        }

        return sec;
    }

    public string UpdateGoalDescription()
    {
        string key = name.Substring( 0, 4 );
        string tp = "";
        if( TrophyType == ETrophyType.BRONZE ) tp = "(BR)";
        if( TrophyType == ETrophyType.SILVER ) tp = "(SI)";
        if( TrophyType == ETrophyType.GOLD ) tp = "(GO)";
        if( TrophyType == ETrophyType.DIAMOND ) tp = "(DI)";
        if( TrophyType == ETrophyType.ADAMANTIUM ) tp = "(AD)";
        if( TrophyType == ETrophyType.GENIUS ) tp = "(GE)";

        string cond = "";
        if( Trig.ConditionVarID == ETriggerVarID.MONSTERSDEATHCOUNT )
            cond = "Monster Kill: " + Trig.ConditionVal1;
        if( Trig.ConditionVarID == ETriggerVarID.SCARABDEATHCOUNT )
            cond = "Scarab Kill: " + Trig.ConditionVal1;
        if( Trig.ConditionVarID == ETriggerVarID.BONFIRESLIT )
            cond = "Fire Lit: " + Trig.ConditionVal1;
        if( Trig.ConditionVarID == ETriggerVarID.UNIT_RESOURCECOLLECTED )
            cond = "Resouce Collected: " + Trig.ConditionVal1;
        if( Trig.ConditionVarID == ETriggerVarID.SECTORSCLEARED )
            cond = "Cubes Cleared: " + Trig.ConditionVal1;
        if( Trig.ConditionVarID == ETriggerVarID.UNIT_FISHINGBONUSREACHED )
            cond = "Fishing Bonus: " + Trig.ConditionVal1;
        if( Trig.ConditionVarID == ETriggerVarID.UNIT_CONQUEREDGOALS )
            cond = "Conquered Goals: " + Trig.ConditionVal1;

        string post = "";

        if( TargetBluePrint != null )
            post += " - " + TargetBluePrint.name + " Plants: " + BluePrintBonusPlants + " Uses: " + BluePrintBonusUses;

        if( TargetRecipe != null )
            post += " ---- " + TargetRecipe.name + " +" + RecipeBonusAmount;

        if( BonusItem != ItemType.NONE )
            post += " - " + BonusItem + " +" + BonusItemValue;

        if( BonusItem2 != ItemType.NONE )
            post += " - " + BonusItem2 + " +" + BonusItem2Value;

        if( TargetBluePrint != null && BonusItem2 != ItemType.NONE )
            G.Error( "Blueprint and item2 enabled bug." + this );

        if( ConditionItem != ItemType.NONE )
            post += "   <<Needs: " + ConditionItemAmount + " " + ConditionItem + ">>";

        if( RefreshTime > 0 )
            post += "  Refresh: " + Util.ToSTime( RefreshTime );

        if( BonusesAwarded > 0 )
            post += "     (Lim: " + BonusesAwarded + ")";

        return key + " " + tp + " " + cond + " " + post;
    }

    public static void InitAlternateStartingCube()
    {
        if( Map.I.RM.RMD.EnableAlternateStartingCube == false ) return;

        int scube = Map.I.RM.StartingCube;        
        if( Helper.I.StartingCube != -1 ) scube = Helper.I.StartingCube;

        for( int i = 0; i < Map.I.RM.RMD.GoalList.Length; i++ )
        {
            RandomMapGoal go = Map.I.RM.RMD.GoalList[ i ];
            if( go.Trig.ConditionVarID == ETriggerVarID.SECTORSCLEARED )
            {
                if( scube  > go.Trig.ConditionVal1 )                    // Mark goal as fake conquered
                {
                    go.Conquered = true;
                    go.ConquestTime = .000001f;
                    go.BonusGiven = true;
                }
            }
        }
    }

    public string GetGoalDescriptionText()
    {
        string txt = "";
        switch( Trig.ConditionVarID )
        {
            case ETriggerVarID.ROACHDEATHCOUNT: txt = "GOAL_ROACHDEATHCOUNT"; break;
            case ETriggerVarID.SCARABDEATHCOUNT: txt = "GOAL_SCARABDEATHCOUNT"; break;
            case ETriggerVarID.BONFIRESLIT: txt = "GOAL_BONFIRESLIT"; break;
            case ETriggerVarID.DIRTYBONFIRESLIT: txt = "GOAL_DIRTYBONFIRESLIT"; break;
            case ETriggerVarID.MONSTERSDEATHCOUNT: txt = "GOAL_MONSTERDEATHCOUNT"; break;
            case ETriggerVarID.UNIT_RESOURCECOLLECTED: txt = "GOAL_RESOURCECOLLECTED"; break;
            case ETriggerVarID.UNIT_FISHINGBONUSREACHED: txt = "GOAL_FISHINGBONUSREACHED"; break;
            case ETriggerVarID.UNIT_CONQUEREDGOALS: txt = "GOAL_CONQUEREDGOALS"; break;
            case ETriggerVarID.SECTORSCLEARED:
            string aux = UI.I.OrdinalNumberList[ ( int ) Trig.ConditionVal1 - 1 ];    
                txt = "Clear " + aux + " Cube";
                return txt;
            break;
        }
        return Language.Get( txt );
    }

    public static void SaveAll( string nm = "" )
    {
        if( Manager.I.SaveOnEndGame == false ) return;
        for( int i = 0; i < Map.I.RM.RMD.GoalList.Length; i++ )
            Map.I.RM.RMD.GoalList[ i ].Save( nm );
    }

    public static void LoadAll( string nm = "" )
    {
        for( int i = 0; i < Map.I.RM.RMD.GoalList.Length; i++ )     
            Map.I.RM.RMD.GoalList[ i ].Load( nm );
    }
    internal void ResetPrefabData()
    {
        BestScoreDateList = new List<System.DateTime>();
        BestScoreList = new List<float>();
        BestScoreTrialNumber = new List<int>();
        ConquestTrial = -1;
        ConquestAmount = 0;
        TargetTime = 0;
        LastObjectiveAmount = 0;
        TimeTryingUntilConquest = 0;
        BestWinnigStreak = 0;
        CurrentWinnigStreak = 0;
    }
    internal static void ClearPrefabData( int id = -1 )
    {
        if( id == -1 ) id = Map.I.RM.CurrentAdventure;
        if( Util.VID( Map.I.RM.RMList, id ) == false ) return;
        for( int j = 0; j < Map.I.RM.RMList[ id ].GoalList.Length; j++ )                                        // reset prefab data to keep prefab data clear
            Map.I.RM.RMList[ id ].GoalList[ j ].ResetPrefabData();
    }

    public static void RestoreGoalPrefabReferences( RandomMapData rm )
    {
        for( int g = 0; g < rm.GoalList.Length; g++ )                                                                           // restores Blueprint reference
        {
            RandomMapGoal gl = rm.GoalList[ g ];
            if( gl.TargetBluePrintID != "" )
            {
                int id = -1;
                for( int i = 0; i < Map.I.Farm.BluePrintList.Count; i++ )
                {
                    if( gl.TargetBluePrintID == Map.I.Farm.BluePrintList[ i ].UniqueID )
                        id = i;
                }
                if( id == -1 ) 
                { 
                    Debug.Log( "Bad Blueprint UID " ); 
                    gl.TargetBluePrintID = "";  
                }
                else
                    gl.TargetBluePrint = Map.I.Farm.BluePrintList[ id ];
            }
            else
                gl.TargetBluePrint = null;

            if( gl.TargetRecipeBuildingID != "" && gl.TargetRecipeID != "" )
            {
                int bldid = -1;
                int rcpid = -1;

                for( int b = 0; b < Map.I.Farm.BuildingList.Count; b++ )
                {
                    var bld = Map.I.Farm.BuildingList[ b ];
                    if( bld == null ) continue;

                    if( bld.UniqueID == gl.TargetRecipeBuildingID )
                    {
                        for( int r = 0; r < bld.RecipeList.Count; r++ )
                        {
                            var rec = bld.RecipeList[ r ];
                            if( rec == null ) continue;

                            if( rec.UniqueID == gl.TargetRecipeID )
                            {
                                bldid = b;
                                rcpid = r;
                                break;
                            }
                        }
                        break;
                    }
                }

                if( bldid == -1 )
                {
                    Debug.Log( "Bad recipe building UID: " + gl.TargetRecipeBuildingID + " " + rm.name );
                    gl.TargetRecipe = null; gl.TargetRecipeBuildingID = "";
                }
                else if( rcpid == -1 )
                {
                    Debug.Log( "Bad recipe UID: " + gl.TargetRecipeID );
                    gl.TargetRecipe = null; gl.TargetRecipeID = "";
                }
                else
                {
                    gl.TargetRecipe = Map.I.Farm.BuildingList[ bldid ].RecipeList[ rcpid ];
                }
            }
            else
            {
                //gl.TargetRecipe = null;
            }
        }
    }
}
