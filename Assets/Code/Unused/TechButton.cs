using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;

public class TechButton : MonoBehaviour
{
    #region Variables
    public GameObject Graphic, Connections;
    public AdventureUpgradeInfo[] UpgradeList;
    public UIButton ClickButton;
    public UILabel DescriptionLabel, UnlockCostLabel, RecurringLabel;
    public UI2DSprite DescriptionSprite, UnlockCostSprite;
    public UISprite BackSprite, LineUP, LineDOWN, LineLEFT, LineRIGHT;
    public List<UISprite> TechArrowListUP, TechArrowListDOWN, TechArrowListLEFT, TechArrowListRIGHT;
    public static TechButton[ , ] Button;
    public static int SX = 6;
    public static int SY = 4;
    public int X = -1, Y = -1;
    public int LeftNeighb, RightNeighb, UpNeighb, DownNeighb;
    public UIGrid UpGrid, DownGrid, LeftGrid, RightGrid;
    public bool Available = false;
    public bool NeighborsOK = false;
    public bool MaxLevelReached = false;
    public int CurTechID = 0;
    public static bool UpdateMatrix = true;
    public static bool PurchasingAvailable = true;
    public static int TechsAvailable = 0;
    public static int AffordableStudies = 0;
    public static int TotalStudies = 0;
    public static int StudiesCompleted = 0;
    public static int StudiesLevel = 0;
    public static float LastPurchaseTimeCount = 0;
    public static float LastPurchaseID = -1;
    public static string TempMessage = "";
    public static Vector2 ClickMouseHoverTechID, MouseHoverTechID;
    public static ETechEditorVarType TechVar = ETechEditorVarType.Upgrade_type;
    public static string TechEditorText;
    public static bool TechEditorActive = false;
    #endregion

    public static void StartIt()
    {
        PurchasingAvailable = true;
        ClickMouseHoverTechID = MouseHoverTechID = new Vector2( -1, -1 );
        TechVar = ETechEditorVarType.Upgrade_type;
        TechEditorActive = false;
    }
    public static void UpdateIt()
    {
        if( Map.I.RM.GameOver != true ) return; // new
        UpdateInfo();

        if( Button == null || UpdateMatrix )
        {
            CreateMatrix();                                                                      // Updates the Button Matrix
        }

        TechsAvailable = 0;
        AffordableStudies = 0;
        StudiesCompleted = 0;
        TotalStudies = 0;
        MouseHoverTechID = new Vector2( -1, -1 );
        LastPurchaseTimeCount += Time.deltaTime;
  
        for( int y = 0; y < SY; y++ )                                                            // Updates all buttons
        for( int x = 0; x < SX; x++ )
        if ( Button != null )
        if ( Button[ x, y ] != null )
        {
            Button[ x, y ].UpdateButton( x, y );            
        }

        UpdateDebug();                                                                           // Updates debug function
    }

    public static void UpdateInfo()
    {
        float perc = Util.GetPercent( StudiesCompleted, TotalStudies ); 
        string txt = "Studies Completed: " + TechButton.StudiesCompleted + " of "+
        TotalStudies + "   (" + perc.ToString( "0." ) + "%)" + "      Can Afford: " + AffordableStudies;
        Map.I.RM.DungeonDialog.StudiesCompletedLabel.text = txt;
    }

    public void UpdateButton( int x, int y )
    {
        LeftNeighb = 0;
        bool left = true;
        if( x == 0 ) left = false;
        else
        if( Button[ x - 1, y ] )
        if( Button[ x - 1, y ].UpgradeList == null ||                                         // Neighbor has upgrade lists?
            Button[ x - 1, y ].UpgradeList.Length <= 0 )
            left = false;
        else LeftNeighb = Button[ x - 1, y ].UpgradeList.Length;
        RightNeighb = 0;
        bool right = true;
        if( x == SX - 1 ) right = false;
        else
        if( Button[ x + 1, y ] )
        if( Button[ x + 1, y ].UpgradeList == null ||
            Button[ x + 1, y ].UpgradeList.Length <= 0 )
            right = false;
        else
        RightNeighb = Button[ x + 1, y ].UpgradeList.Length;
        UpNeighb = 0;
        bool up = true;
        if( y == 0 ) up = false;
        else
        if( Button[ x, y - 1 ] )
        if( Button[ x, y - 1 ].UpgradeList == null || 
            Button[ x, y - 1 ].UpgradeList.Length <= 0 ) 
            up = false;
        else
            UpNeighb = Button[ x, y - 1 ].UpgradeList.Length;
        DownNeighb = 0;
        bool down = true;
        if( y == SY - 1 ) down = false;
        else
        if( Button[ x, y + 1 ] )
        if( Button[ x, y + 1 ].UpgradeList == null || 
            Button[ x, y + 1 ].UpgradeList.Length <= 0 ) 
            down = false;
        else 
            DownNeighb = Button[ x, y + 1 ].UpgradeList.Length;

        int level = AdventureUpgradeInfo.GetTechLevel( x, y );
        AdventureUpgradeInfo au = null;
        if( Button[ x, y ] != null && Button[ x, y ].UpgradeList != null &&
            Button[ x, y ].UpgradeList.Length > 0 )
        {
            au = Button[ x, y ].UpgradeList[ 0 ];
        }

        int tid = 0;
        if( UpgradeList != null && UpgradeList.Length > 0 )
        {
            if( au && au.IsRecurring() )
            {
                TechsAvailable = au.UpgradeRecuringEffect.Count - level;                                  // recurring tech
                TotalStudies += au.UpgradeRecuringEffect.Count;
                Available = true;
                MaxLevelReached = false;
            }
            else
                if( Available && MaxLevelReached == false ) TechsAvailable++;
            StudiesCompleted += level;                                                                    // Calculate Studies completed number  

            if( UpgradeList[ tid ].LeftNeed <= 0 ) left = false;                                          // Neighbor needed
            if( UpgradeList[ tid ].RightNeed <= 0 ) right = false;
            if( UpgradeList[ tid ].UpNeed <= 0 ) up = false;
            if( UpgradeList[ tid ].DownNeed <= 0 ) down = false;

            Available = true;
            TotalStudies += UpgradeList.Length;
            MaxLevelReached = false;
            LineLEFT.color = Color.green;
            LineRIGHT.color = Color.green;
            LineUP.color = Color.green;
            LineDOWN.color = Color.green;

            for( int i = 0; i < 10; i++ )
            {
                int id = 10 - i;
                TechArrowListLEFT[ i ].gameObject.SetActive( true );                              // Disables or enables the tech Arrow gameobj
                if( left && id > UpgradeList[ tid ].LeftNeed )
                    TechArrowListLEFT[ i ].gameObject.SetActive( false );

                TechArrowListRIGHT[ i ].gameObject.SetActive( true );
                if( right && id > UpgradeList[ tid ].RightNeed )
                    TechArrowListRIGHT[ i ].gameObject.SetActive( false );

                TechArrowListUP[ i ].gameObject.SetActive( true );
                if( up && id > UpgradeList[ tid ].UpNeed )
                    TechArrowListUP[ i ].gameObject.SetActive( false );

                TechArrowListDOWN[ i ].gameObject.SetActive( true );
                if( down && id > UpgradeList[ tid ].DownNeed )
                    TechArrowListDOWN[ i ].gameObject.SetActive( false );

                int leftn = AdventureUpgradeInfo.GetTechLevel( x - 1, y );                         // Gets the amount of neighbor purchases
                int rightn = AdventureUpgradeInfo.GetTechLevel( x + 1, y );
                int upn = AdventureUpgradeInfo.GetTechLevel( x, y - 1 );
                int downn = AdventureUpgradeInfo.GetTechLevel( x, y + 1 );                

                TechArrowListLEFT[ i ].color = Color.green;                                        // Sets the Tech arrow color: Left
                if( left && leftn < id && TechArrowListLEFT[ i ].gameObject.activeSelf )
                {
                    TechArrowListLEFT[ i ].color = Color.red;
                    Available = false;
                    LineLEFT.color = Color.red;
                }

                TechArrowListRIGHT[ i ].color = Color.green;                                        // Sets the Tech arrow color: Right
                if( right && rightn < id && TechArrowListRIGHT[ i ].gameObject.activeSelf )
                {
                    TechArrowListRIGHT[ i ].color = Color.red;
                    Available = false;
                    LineRIGHT.color = Color.red;
                }

                TechArrowListUP[ i ].color = Color.green;                                            // Sets the Tech arrow color: Up
                if( up && upn < id && TechArrowListUP[ i ].gameObject.activeSelf )
                {
                    TechArrowListUP[ i ].color = Color.red;
                    Available = false;
                    LineUP.color = Color.red;
                }

                TechArrowListDOWN[ i ].color = Color.green;                                           // Sets the Tech arrow color: Down
                if( down && downn < id && TechArrowListDOWN[ i ].gameObject.activeSelf )
                {
                    TechArrowListDOWN[ i ].color = Color.red;
                    Available = false;
                    LineDOWN.color = Color.red;
                }

                if( left == false && right == false )                                                 // No side Dependencies, so its available
                if( up == false   && down == false  ) Available = true;
                NeighborsOK = Available;
            }
            //DownGrid.Reposition();
            //UpGrid.Reposition();
            //LeftGrid.Reposition();
            //RightGrid.Reposition();
        }

        LineLEFT.gameObject.SetActive( left );                                                         // Disables or enables connections
        TechArrowListLEFT[ 0 ].gameObject.transform.parent.gameObject.SetActive( left );
        LineRIGHT.gameObject.SetActive( right );
        TechArrowListRIGHT[ 0 ].gameObject.transform.parent.gameObject.SetActive( right );
        LineUP.gameObject.SetActive( up );
        TechArrowListUP[ 0 ].gameObject.transform.parent.gameObject.SetActive( up );
        LineDOWN.gameObject.SetActive( down );
        TechArrowListDOWN[ 0 ].gameObject.transform.parent.gameObject.SetActive( down );
        DescriptionLabel.text = "" + x + " " + y;

        UpdateAdventureUpgrade( x, y, false, ref DescriptionLabel, ref UnlockCostLabel,                // Updates Button info 
        ref ClickButton, ref UnlockCostSprite );
        UpdateGamePlayTechSetup( x, y );

        if( UpgradeList == null || UpgradeList.Length < 1 )                                            // No upgrades for this Button: Disable
            gameObject.SetActive( false );
        else gameObject.SetActive( true );

        BackSprite.color = Color.white;                                                                 // Sets Back sprite Color
        if( Available == false ) 
            BackSprite.color = new Color( 1, .4f, .4f, 1 );

        if( MaxLevelReached )                                                                           // Max level reached: blue back color
            BackSprite.color = new Color( .4f, .4f, 1f, 1 );      
    }
    public static void UpdateDebug()
    {
        if( Map.I.RM.GameOver == false ) return;
        if( Helper.I.TestTechTree == false ) return;
        if( TechEditorActive == false ) return;

        if( Input.GetKey( KeyCode.Delete ) )
        {
            //AdventureUpgradeInfo.AddTechLevel( -1, -1,-AdventureUpgradeInfo.GetTechLevel( -1, -1 ) );           // Reset Adventure level Debug
            for( int y = 0; y < SY; y++ )
            for( int x = 0; x < SX; x++ )
                {
                    AdventureUpgradeInfo.AddTechLevel( x, y,
                   -AdventureUpgradeInfo.GetTechLevel( x, y ) );                                                // Reset All techs Debug
                }
        }
    }

    public static void CreateMatrix()
    {
        Map.I.RM.DungeonDialog.StudiesGO.gameObject.SetActive( true );
        Map.I.RM.DungeonDialog.StudiesGO.transform.localPosition = new Vector3( 1015, -5, 0 );
        Button = new TechButton[ SX, SY ];
        for( int i = 0; i < Map.I.TB.Length; i++ )
        {
            int x = i % SX;    
            int y = i / SX;
            Button[ x, y ] = Map.I.TB[ i ];
            Button[ x, y ].X = x;
            Button[ x, y ].Y = y;
            Button[ x, y ].Available = false;
            Button[ x, y ].UpgradeList = null;
            Button[ x, y ].Connections.gameObject.SetActive( true );
            Button[ x, y ].Graphic.gameObject.SetActive( true );
            List<AdventureUpgradeInfo> ul = new List<AdventureUpgradeInfo>();
            if( Map.I.RM.CurrentAdventure != -1 )
            {
                RandomMapData rm = Map.I.RM.RMList[ Map.I.RM.CurrentAdventure ];
                if( rm.TechListFolder )
                {
                    GameObject fol = Util.getChildGameObject( rm.TechListFolder, "Tech " + x + " " + y );
                    if( fol )
                        Button[ x, y ].UpgradeList = fol.GetComponentsInChildren<AdventureUpgradeInfo>();
                }
            }
        }
        Map.I.RM.DungeonDialog.StudiesGO.gameObject.SetActive( false );
        UpdateMatrix = false;
    }

    public static void UpdateAdventureUpgrade( int techX, int techY, bool purchase, ref UILabel label, ref UILabel costlabel, ref UIButton btn, ref UI2DSprite spr )
    {
        DungeonDialog dd = Map.I.RM.DungeonDialog;
        costlabel.transform.parent.gameObject.SetActive( true );
        label.color = Color.red;
        int level = AdventureUpgradeInfo.GetTechLevel( techX, techY );                                // ADVENTURE UPGRADE
        if( purchase ) level += 1;
        int id = level;                                                                               // Are resources enough?
        if( purchase ) id -= 1;

        AdventureUpgradeInfo au = null;
        TechButton bt = null;
        int max = -1;
        if( techX != -1 )
        {
            bt = TechButton.Button[ techX, techY ];
           if(bt.RecurringLabel) bt.RecurringLabel.text = "";
            if( Util.HasDataArray( bt.UpgradeList ) )
            {
                AdventureUpgradeInfo upg = bt.UpgradeList[ 0 ];
                if( upg.IsRecurring() )                                                                      // recurring tech
                {
                    max = upg.UpgradeItem1RecuringCost.Count;
                    au = bt.UpgradeList[ 0 ];
                    if( purchase == false && level >= max ||                                                // Max Level reached for recurring tech
                        purchase == true && level >= max + 1 )
                    {
                        costlabel.transform.parent.gameObject.SetActive( false );
                        label.text = "Max Level: L" + level + "\n" +
                        AdventureUpgradeInfo.GetUpgradeMessage( au, false );
                        label.color = Color.grey;
                        bt.MaxLevelReached = true;
                        id = 0;
                    }
                }
            }
        }               

        if( techX == -1 )
        {
            max = Map.I.RM.RMD.AdventureUpgradeInfoList.Length;
            if( id == max ) id--;
            if( Util.VID( Map.I.RM.RMD.AdventureUpgradeInfoList, id ) )
                au = Map.I.RM.RMD.AdventureUpgradeInfoList[ id ];                                               // Dialog upgrade
            if( purchase == false && level >= max ||
                purchase == true && level >= max + 1 )
            {
                costlabel.transform.parent.gameObject.SetActive( false );
                label.text = "Max Quest Level Reached: L" +
                ( level + Map.I.RM.RMD.StartingAdventureLevel );                                            // Max quest level reached at the Dialog
                label.color = Color.grey;
                return;
            }
        }
         
        if( max == -1  )
        if( techX != -1 )                                                                                   // NON RECURRING TECH UPGRADE
        {
            if( bt.UpgradeList == null ) return;
            if( bt.UpgradeList.Length <= 0 ) return;

            max = bt.UpgradeList.Length;
            if( purchase == false && level >= max ||                                                       // Max Level reached
                purchase == true && level >= max + 1 )
            {
                costlabel.transform.parent.gameObject.SetActive( false );
                au = bt.UpgradeList[ max - 1 ];
                label.text = "Max Level: L" + level + "\n" + 
                AdventureUpgradeInfo.GetUpgradeMessage( au, false );
                label.color = Color.grey;
                bt.MaxLevelReached = true;
                id = bt.UpgradeList.Length - 1;
            }
            au = bt.UpgradeList[ id ];
            label.text = AdventureUpgradeInfo.GetUpgradeMessage( au );
        }

        if( bt )
        {
            bt.Graphic.SetActive( true );

            if( StudiesLevel < au.StudiesLevelToShow )                                                                           // hide tech if Stidies level is not enough
            {
                bt.Graphic.SetActive( false );
                return;
            }
            if( au.OnlyShowIfNeighborsArrowsPermit )                                                                             // Hide tech until all neighbor arrows are green
            {
                if( bt.NeighborsOK == false )
                    bt.Graphic.SetActive( false );
                bt.Connections.SetActive( true );
                if( bt.NeighborsOK == false ) return;
            }
        }

        int amt = ( int ) Item.GetNum( Inventory.IType.Inventory, au.UpgradeItem1Type,                                           // Gets amount in storage
        Map.I.RM.CurrentAdventure );

        if( au.TotalCollected ) amt = ( int ) Item.GetTotalGained( au.UpgradeItem1Type );                                        // or gets total amount of lifetime collects

        int needed = ( int ) au.UpgradeItem1Cost;
 
        if( au.IsRecurring() )
        {
            int purchased = level;                                                                                               // Calculates needed cost for recuring depending on next tech price
            if( purchase ) purchased -= 1;                                                                                       // to nulify the +1 added above that was causing bug
            if( purchased >= au.UpgradeItem1RecuringCost.Count )
                purchased = au.UpgradeItem1RecuringCost.Count - 1;
            needed = ( int ) au.UpgradeItem1RecuringCost[ purchased ];
        }

        bool enough = true;
        if( amt < needed ) enough = false;
        string itname = G.GIT( au.UpgradeItem1Type ).GetName();
        string price = " " + needed + " " + itname;
        string msg = "";
        if( bt ) bt.CurTechID = id;
        Color col = Color.red;
        bool trophy = dd.CheckTrophyRequirement();                                                                               // Check Trophy requirement

        if( btn.state == UIButtonColor.State.Hover )
        {
            float stock = Item.GetNum( Inventory.IType.Inventory, au.UpgradeItem1Type );                                         // items int stock
            if( au.TotalCollected ) stock = ( int ) Item.GetTotalGained( au.UpgradeItem1Type );                                  // Total items int stock

            msg = AdventureUpgradeInfo.GetUpgradeMessage( au, false, true );

            if( au.TotalCollected )
                msg += "\nReach a number of " + needed.ToString( "0.#" ) + " Lifetime " + itname +                               // Total collected msg
                " Collected to unlock for free.\n Lifetime " + itname + " Collected: " + stock.ToString( "0.#" );
            else
                msg += "\n Cost:" + price + "\n" + itname + " in Stock: " + stock.ToString( "0.#" );                             // normal price

            msg += "\nTech Scope: " + Util.GetName( au.TechScope.ToString() );
            if( au.TechScope == ETechScope.All_Quests )
                msg += "\nThis is a Global Tech! Valid for all Quests from now on!";                                             // Global quest
            if( techX != -1 )
            if( au.StudyXPBonus != 0 && au.TechTotalTime == 0 )
                msg += "\nStudy XP Bonus: " + au.StudyXPBonus.ToString( "+0;-#" );                                               // Study bonus

            if( Helper.I.ReleaseVersion == false && techX != -1 )
            {
                msg += "\nCord: " + techX + " " + techY;
                msg += "\nTech: " + ( id + 1 ) + " of " + bt.UpgradeList.Length;
            }
            if( Input.GetMouseButtonDown( 2 ) )
                ClickMouseHoverTechID = new Vector2( techX, techY );
            MouseHoverTechID = new Vector2( techX, techY );
            dd.SetMsg( msg, Color.yellow, .01f );
            dd.DungeonNameLabel.text = Language.Get( "TECH_" + au.UpgradeType.ToString(), "Main" );                       // Show tech help text

            if( au.TechTotalTime > 0 )
                dd.DungeonNameLabel.text += "\n\n" +Language.Get( "TIMED_TECH_MSG", "Main" );                             // timed  tech text

            if( au.PurchaseChance > 0 )
                dd.DungeonNameLabel.text += "\n\n" + Language.Get( "CHANCE_TECH_MSG", "Main" );                           // timed  tech text
            Map.I.RM.DungeonDialog.AutogateLev = -2;
        }

        float cost = -needed;
        if( au.UpgradeType == EAdventureUpgradeType.RECEIVE_GIFT )                                                        // gift inverted signal
        {
            enough = true;
            if( cost < 0 ) cost *= -1;
        }

        if( au.TechNeed > 0 )                                                                                             // Tech needed mode
        if( StudiesCompleted < au.TechNeed ) 
            bt.Available = false;

        if( bt && enough == false )
        {
            bt.BackSprite.color = new Color( 1, .4f, .4f, 1 );                                                           // Back sprite color
            bt.Available = false;
        }

        if( enough )
        {
            if( bt && bt.Available )
            if( bt.MaxLevelReached == false )
            {
                AffordableStudies++;                                                                                      // increment counter
            }
        }
        else
        {
            if( btn.state == UIButtonColor.State.Hover )
                dd.SetMsg( msg + "\nERROR: Not Enough Resources!", Color.yellow );                                       // no resource error message
        }

        if( purchase )
        {
            if( enough == false ) return;                                                                                // Not enough resources    
            if( trophy == false ) return;
            if( TechEditorActive ) return;
            if( PurchasingAvailable == false ) return;
            if( bt && bt.MaxLevelReached ) return;

            LastPurchaseID = au.TechID;
            LastPurchaseTimeCount = 0;
            if( au.TotalCollected == false )
                Item.AddItem( Inventory.IType.Inventory, au.UpgradeItem1Type, cost, true );
            if( au.PurchaseChance > 0 )                                                                                  // Purchase chance
            {
                bool sort = Util.Chance( au.PurchaseChance );
                if( sort == false )
                {
                    TempMessage = "Fail!";
                    MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );                                           // play error sound FX
                    return; 
                }
                TempMessage = "Success!";
            }
            if( techX != -1 )                                                                                            // Its a study, so add study xp
            if( au.TechTotalTime <= 0 )
                Item.AddItem( Inventory.IType.Inventory, ItemType.Study_XP, au.StudyXPBonus, true );
            AdventureUpgradeInfo.AddTechLevel( techX, techY, 1, au );                                                    // Add level and Charge resource
            MasterAudio.PlaySound3DAtVector3( "Cashier", G.Hero.Pos );

            if( au.TechTotalTime > 0 )
            {
                ItemType it = ItemType.TechPurchase_0_0 + techX + ( techY * TechButton.SX );                            // Reset Timed tech time counter (uses item.totalgained for data storage)
                int adv = ( int ) Map.I.RM.CurrentAdventure;
                G.GIT( it ).PerAdventureTotalCount[ adv ] = au.TechTotalTime;
            }

            if( techX != -1 )
                dd.SetMsg( "Study Completed for " + price, Color.green );                                         // custom messages
            else
                if( level <= 1 && Map.I.RM.RMD.StartingAdventureLevel == 0 )
                dd.SetMsg( "Quest Unlocked for " + price, Color.green );
            else
                dd.SetMsg( "Quest Upgraded for " + price, Color.green );

            if( au.UpgradeType == EAdventureUpgradeType.INCREASE_AVAILABLE_CUBES  )                               // Auto select max cube in the dialog           
                Map.I.RM.DungeonDialog.UpdateAlternateStartingCube( true );
            
            if( au.UpgradeType == EAdventureUpgradeType.TRADE )                                                   // give purchased item when buying
            {
                Item.AddItem( au.ItemAffected, au.UpgradeEffectAmount );
            }
            if( au.UpgradeType == EAdventureUpgradeType.UPGRADE_MAX_CAPACITY )                                    // Increase max item capacity
            {
                G.GIT( au.ItemAffected ).ExtraCapacity += au.UpgradeEffectAmount;
            }
            if( au.UpgradeType == EAdventureUpgradeType.ITEM_PRODUCTION_LIMIT )                                   // Increase max item capacity
            {
                G.GIT( au.ItemAffected ).ExtraProductionLimit += ( int ) au.UpgradeEffectAmount;
            }
            if( au.UpgradeType == EAdventureUpgradeType.ITEM_PRODUCTION_TOTAL_TIME )                              // item production time
            {
                G.GIT( au.ItemAffected ).ExtraProductionTotalTime += ( int ) au.UpgradeEffectAmount;
            }
            if( au.UpgradeType == EAdventureUpgradeType.ITEM_PRODUCTION_ACTIVATED )                               // item production activation
            {
                G.GIT( au.ItemAffected ).ProductionPurchased = ( int ) au.UpgradeEffectAmount;
            }
        }
        else
        {                                                                                                        // Only Updates Unlock Cost Color and Label
            if( level <= 0 && techX == -1 &&
                Map.I.RM.RMD.StartingAdventureLevel == 0 )
                label.text = "Unlock Quest for: ";
            else
            {
                //string lt = " L" + level;      //  removed the level text. see if it is important
                //if( level == 0 ) lt = "";
                //label.text = au.GetUpgradeText() + lt;
                label.text = au.GetUpgradeText();

                if( bt )
                {
                    UpgradeButtonIcon( au, bt );
                    label.color = Color.green;

                    if( au.TechNeed > 0 )                                                                     // Tech needed mode
                    if( StudiesCompleted < au.TechNeed )
                        {
                            label.text = "Need +" + ( au.TechNeed - StudiesCompleted ) + " More Studies";
                        }
                }
            }

            if( au.TotalCollected )
                costlabel.text = "TOT: " + needed;
            else
            {
                if( needed == 0 ) costlabel.text = "Free!";                                                               // free price!
                else
                    costlabel.text = "x" + needed;                                                                        // writes price
            }

            if( au.UpgradeType == EAdventureUpgradeType.RECEIVE_GIFT )                                                    // item label color
            {
                costlabel.text = "Get: " + needed;
                col = Color.blue;
            }
            if( enough && ( ( bt && bt.Available ) || techX == -1 ) ) 
                col = Color.green; 
            costlabel.color = col;

            if( au.UpgradeType == EAdventureUpgradeType.LEISURE )
                col = new Color( .5f, .5f, 1, 1 );

            if( bt && bt.MaxLevelReached )
                col = Color.grey;

            label.color = col;

            spr.sprite2D = G.GIT( au.UpgradeItem1Type ).Sprite.sprite2D;                                                // Updates Cost Sprite        

            if( techX != -1 )
            {
                bt.BackSprite.width = bt.BackSprite.height = 141;
                bt.BackSprite.spriteName = "Perk Dark Background";                                                      // Tech Back Sprite type
                if( au.TechScope == ETechScope.All_Quests )
                {
                    bt.BackSprite.spriteName = "Global Tech";
                    bt.BackSprite.width = bt.BackSprite.height = 155;
                }
            }

       bt.RecurringLabel.color = Color.yellow;
       if( au.PurchaseChance > 0 )
       {
           bt.RecurringLabel.text = "Chance: " + au.PurchaseChance.ToString( "0..#" ) + "%";                             // Purchase Chance
           if( LastPurchaseTimeCount < 2f )
           if( au.TechID == LastPurchaseID )
           {
               if( TempMessage == "Fail!" )
                   bt.RecurringLabel.color = Color.red;
               else
                   bt.RecurringLabel.color = Color.green;
               bt.RecurringLabel.text = TempMessage;
           }
       }
       else
       if( au.TechTotalTime > 0 )
       {
           bt.RecurringLabel.color = Color.yellow;
           int adv = ( int ) Map.I.RM.CurrentAdventure;
           ItemType it = ItemType.TechPurchase_0_0 + techX + ( techY * TechButton.SX );
           if( G.GIT( it ).PerAdventureCount[ adv ] <= 0 )
           {
               bt.RecurringLabel.text = "Tot Time: " + Util.ToSTime( au.TechTotalTime );
           }
           else
           {
               float time = G.GIT( it ).PerAdventureTotalCount[ adv ];
               bt.RecurringLabel.text = "Left: " + Util.ToSTime( time );
               bt.RecurringLabel.color = Color.green;
           }
       }
       else
           if( au.IsRecurring() )
           {
               ItemType it = ItemType.TechPurchase_0_0 + techX + ( techY * TechButton.SX );                            // updates recurring label text "X2"
               int num = ( int ) Item.GetNum( it );
               bt.RecurringLabel.text = "" + num + " of " + au.UpgradeItem1RecuringCost.Count;
           }
       else
       if( bt && bt.RecurringLabel )
           bt.RecurringLabel.text = "";

       if( Helper.I.ReleaseVersion == false )
           if( Input.GetKey( KeyCode.F1 ) )                                                                            // Only show tech number         
           {
               label.color = Color.green;
               label.text = "\nTech: " + ( id + 1 ) + " of " + bt.UpgradeList.Length;
               if( bt && bt.UpgradeList.Length > 1 ) label.color = new Color( .5f, .5f, 1, 1 );
           }
        }
    }

    public static void UpgradeButtonIcon( AdventureUpgradeInfo au, TechButton bt )
    {
        DungeonDialog dd = Map.I.RM.DungeonDialog;
        bt.DescriptionSprite.sprite2D = dd.SpriteList[ 0 ].sprite2D;
        switch( au.UpgradeType )
        {
            case EAdventureUpgradeType.UPGRADE_PACKMULE:
            bt.DescriptionSprite.sprite2D = dd.SpriteList[ 1 ].sprite2D;
            break;
        }
        int icon = -1;
        if( au.ItemAffected != ItemType.NONE )
            icon = ( int ) au.ItemAffected;
        if( au.UpgradeType == EAdventureUpgradeType.RECEIVE_GIFT )
            icon = ( int ) au.UpgradeItem1Type;
        if( au.UpgradeType == EAdventureUpgradeType.INITIAL_HP )
            icon = ( int ) ItemType.Res_HP;
        if( au.UpgradeType == EAdventureUpgradeType.CLOVER_CHANCE )
            icon = ( int ) ItemType.Clover;
        if( au.UpgradeType == EAdventureUpgradeType.UPGRADE_STUDIES )
            icon = ( int ) ItemType.Cog;
        if( au.UpgradeType == EAdventureUpgradeType.INITIAL_UPGRADE_CHEST_CHANCE ||
            au.UpgradeType == EAdventureUpgradeType.CLOVER_UPGRADE_CHEST_CHANCE  ||
            au.UpgradeType == EAdventureUpgradeType.CHEST_BASE_BONUS_CHANCE ||
            au.UpgradeType == EAdventureUpgradeType.CUBE_CLEAR_UPGRADE_CHEST_CHANCE ||
            au.UpgradeType == EAdventureUpgradeType.CHEST_PERSIST_CHANCE ||
            au.UpgradeType == EAdventureUpgradeType.CHEST_ITEM_CHANCE_INFLATION )
            icon = ( int ) ItemType.Chest_Points;
        if( au.UpgradeType == EAdventureUpgradeType.SPAWN_BUTCHER_CHANCE )
            icon = ( int ) ItemType.Butcher_Level;
        if( icon >= 0 ) 
            bt.DescriptionSprite.sprite2D = G.GIT( icon ).Sprite.sprite2D;
    }

    public void OnTechButtonPress()
    {
        if( X == -1 ) return;
        if( Available == false ) return;
        UpdateAdventureUpgrade( X, Y, true, ref DescriptionLabel, ref UnlockCostLabel,
        ref ClickButton, ref UnlockCostSprite );
    }


    public enum ETechEditorVarType
    {
        Upgrade_type, Upgrade_Effect_Amount, Cost_Item, Cost, Item_Affected,
        Study_XP
    }

    public void UpdateGamePlayTechSetup( int x, int y )
    {
        if( MouseHoverTechID == new Vector2( x, y ) )                                                         // purchase or de-purchase tech 
        {
            TechButton btt = TechButton.Button[ x, y ];
            if( Input.GetKeyDown( KeyCode.Comma ) )
                AdventureUpgradeInfo.AddTechLevel( x, y, -1 );
            if( Input.GetKeyDown( KeyCode.Period ) )
            if( btt.NeighborsOK )
                AdventureUpgradeInfo.AddTechLevel( x, y, +1 );
        }

        if( Input.GetKey( KeyCode.LeftControl ) )
        if( Input.GetKeyDown( KeyCode.C ) )
        if( MouseHoverTechID == new Vector2( x, y ) )                                                          // Copy tech with control + c
        {
            TechButton frb = TechButton.Button[ ( int ) MouseHoverTechID.x, ( int ) MouseHoverTechID.y ];
            AdventureUpgradeInfo frt = frb.UpgradeList[ frb.CurTechID ];
            TechButton tob = TechButton.Button[ ( int ) ClickMouseHoverTechID.x, ( int ) ClickMouseHoverTechID.y ];
            AdventureUpgradeInfo tot = tob.UpgradeList[ tob.CurTechID ];
            frt.UpgradeType = tot.UpgradeType;
            frt.UpgradeItem1Type = tot.UpgradeItem1Type;
            frt.UpgradeItem1Cost = tot.UpgradeItem1Cost;
            frt.UpgradeEffectAmount = tot.UpgradeEffectAmount;
            frt.ItemAffected = tot.ItemAffected;
        }

        if( ClickMouseHoverTechID != new Vector2( x, y ) ) return;                                              // Toggle Editor on or off
        if( Helper.I.ReleaseVersion ) return;
        if( Map.I.RM.GameOver == false ) return;

        if( Input.GetMouseButtonDown( 2 ) )
        {
            TechEditorActive = !TechEditorActive;
            if(!TechEditorActive )
                ClickMouseHoverTechID = new Vector2( -1, -1 );
            Map.I.RM.DungeonDialog.TechEditorFocus.gameObject.SetActive( TechEditorActive );
            TechVar = ETechEditorVarType.Upgrade_type;            
        }

        if( Input.GetMouseButtonDown( 1 ) )
        {
            int ttype = ( int ) TechVar;
            int ssz = System.Enum.GetValues( typeof( ETechEditorVarType ) ).Length;
            if( ++ttype >= ssz ) ttype = 0;
            TechVar = ( ETechEditorVarType ) ttype;
        }

        PurchasingAvailable = true;
        if( TechEditorActive == false ) { TechEditorText = ""; return; }
        PurchasingAvailable = false;

        TechButton bt = TechButton.Button[ x, y ];     
        AdventureUpgradeInfo au = bt.UpgradeList[ 0 ];
        if( au && au.IsRecurring() ) CurTechID = 0;

        if( CurTechID < 0 || CurTechID > bt.UpgradeList.Length )
        {
            Debug.LogError( "CurTechID out of bounds: " + CurTechID );
            return;
        }

        AdventureUpgradeInfo ad = bt.UpgradeList[ bt.CurTechID ];
        ad.UpdateGameObjectText(); 
        string txt = "Tech Editor:\n";
        if( TechVar == ETechEditorVarType.Upgrade_type ) txt += "-->>";
        txt += " Upgrade Type: " + ad.UpgradeType + "\n";
        if( TechVar == ETechEditorVarType.Upgrade_Effect_Amount ) txt += "-->>";
        txt += " Effect Amount: " + ad.UpgradeEffectAmount + "\n";
        if( TechVar == ETechEditorVarType.Cost_Item ) txt += "-->>";
        txt += "Cost Item: " + ad.UpgradeItem1Type + "\n";
        if( TechVar == ETechEditorVarType.Cost ) txt += "-->>";
        txt += "Cost: " + ad.UpgradeItem1Cost + "\n";
        if( TechVar == ETechEditorVarType.Item_Affected ) txt += "-->>";
        txt += "Item Affected: " + ad.ItemAffected + "\n";
        if( TechVar == ETechEditorVarType.Study_XP ) txt += "-->>";
        txt += "Study XP: " + ad.StudyXPBonus + "\n";

        TechEditorText = txt;

        #if UNITY_EDITOR 
        UnityEditor.Selection.activeGameObject = ad.gameObject;                                        // lock gameobject with midle
        #endif 

        Map.I.RM.DungeonDialog.TechEditorFocus.transform.position = bt.transform.position;

        if( Input.GetKeyDown( KeyCode.LeftArrow ) )
        if( ++ad.LeftNeed > LeftNeighb ) ad.LeftNeed = 0;
        if( Input.GetKeyDown( KeyCode.RightArrow ) )
        if( ++ad.RightNeed > RightNeighb ) ad.RightNeed = 0;
        if( Input.GetKeyDown( KeyCode.UpArrow ) )
        if( ++ad.UpNeed > UpNeighb ) ad.UpNeed = 0;
        if( Input.GetKeyDown( KeyCode.DownArrow ) )
        if( ++ad.DownNeed > DownNeighb ) ad.DownNeed = 0;

        int type = -1, sz = -1;
        if( TechVar == ETechEditorVarType.Upgrade_type )
        {
            type = ( int ) ad.UpgradeType;
            sz = System.Enum.GetValues( typeof( EAdventureUpgradeType ) ).Length - 1;
            ad.UpgradeType = ( EAdventureUpgradeType ) GetInput( type, sz );
        }
        if( TechVar == ETechEditorVarType.Cost_Item )
        {
            type = Map.I.TechEditorItemList.IndexOf( ad.UpgradeItem1Type );
            if( type == -1 ) { type = 0; Debug.Log( "item not in TechEditorItemList" ); }
            sz = Map.I.TechEditorItemList.Count;
            ad.UpgradeItem1Type = Map.I.TechEditorItemList[ GetInput( type, sz ) ];
        }
        if( TechVar == ETechEditorVarType.Item_Affected )
        {
            type = Map.I.TechEditorItemList.IndexOf( ad.ItemAffected );
            if( type == -1 ) { type = 0; Debug.Log( "item not in TechEditorItemList" ); }
            sz = Map.I.TechEditorItemList.Count;
            ad.ItemAffected = Map.I.TechEditorItemList[ GetInput( type, sz ) ];
        }
        if( TechVar == ETechEditorVarType.Cost )
        {
            type = ( int ) ad.UpgradeItem1Cost;
            sz = 51;
            ad.UpgradeItem1Cost = ( float ) GetInput( type, sz, 1 );
        }
        if( TechVar == ETechEditorVarType.Upgrade_Effect_Amount )
        {
            type = ( int ) ad.UpgradeEffectAmount;
            sz = 51;
            ad.UpgradeEffectAmount = ( float ) GetInput( type, sz, 1 );
        }
               
        if( TechVar == ETechEditorVarType.Study_XP )
        {
            type = ( int ) ad.StudyXPBonus;
            sz = 51;
            ad.StudyXPBonus = ( float ) GetInput( type, sz, 1 );
        }
    }

    public int GetInput( int type, int sz, int min = 0 )
    {
        if( Input.GetAxis( "Mouse ScrollWheel" ) > 0f )
        if( ++type >= sz ) type = min;
        if( Input.GetAxis( "Mouse ScrollWheel" ) < 0f )
        if( --type < min ) type = sz - 1;
        return type;
    }
    static float _tick = 0f;
    static float Wait = 1f;
    internal static void UpdateTimedTech()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf ) return;
        if( Manager.I.GameType != EGameType.CUBES ) return;

        _tick += Time.unscaledDeltaTime;

        if( _tick < Wait )
            return;
        if ( Button != null )
        for( int y = 0; y < SY; y++ )                                                          
        for( int x = 0; x < SX; x++ )
        if ( Button[ x, y ] != null )
        if ( Button[ x, y ].UpgradeList != null && Button[ x, y ].UpgradeList.Length > 0 )
        if ( Button[ x, y ].UpgradeList[ 0 ].TechTotalTime > 0 )
        {
            ItemType itt = ItemType.TechPurchase_0_0 + x + ( y * TechButton.SX );
            Item it = G.GIT( itt );  
            int adv = ( int ) Map.I.RM.CurrentAdventure;
            it.PerAdventureTotalCount[ adv ] -= Wait;                                               // Time Decrement
            if( it.PerAdventureTotalCount[ adv ] < 0 )
            {
                it.PerAdventureTotalCount[ adv ] = 0;                                                                 // Time is up!
                it.PerAdventureCount[ adv ] = 0;
                Map.I.RM.DungeonDialog.AutogateLev = -2;
            }
        }
        _tick -= Wait;
    }
}
