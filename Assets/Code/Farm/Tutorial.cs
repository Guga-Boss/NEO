using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;

public class Tutorial : MonoBehaviour
{
    #region Variables
    public bool ObjectiveCompleted = false;
    public int Phase = 1;
    public float PhaseTimer = 0;
    public int MessageType = 0;
    [HideInInspector]
    public int BluePrintSwitches = 0;
    [HideInInspector]
    public bool NewBluePrintChosen = false;
    [HideInInspector]
    public bool BluePrintAutomated = false;
    [HideInInspector]
    public bool BuildingMoved = false;
    [HideInInspector]
    public bool ScrollChecked = false;
    [HideInInspector]
    public bool ScrollCheckedNeeded = false;
    [HideInInspector]
    public bool RecipeUpgraded = false;
    [HideInInspector]
    public bool BerryPlanted = false;
    public int TutorialResetCount = 0;   // if you want to allow tutorial reset, this will avoid multiple prizes (already being saved)
    public ItemType[ ] GiftList;
    public int[ ] GiftAmountList;
    #endregion

    public bool CanSave()                                                      // This is used in the begining to avoid wastage of resources
    {
        if( Helper.I.ReleaseVersion == false ) return true;
        string file = Manager.I.GetProfileFolder() + "Farm" + ".NEO";        // Phase can only be saved after farm has been saved
        if( File.Exists( file ) == true ) return true;
        return false;
    }

    public void ProgressPhase( int _phase )
    {
        if( _phase == 6 )
        {
            BluePrintSwitches++;
        }
    }

    public void UpdateIt()
    {
        if( Helper.I.IgnoreTutorial ) SetPhase( 14 );

        UI.I.RestartAreaButton.gameObject.SetActive( true );                                     // Disable exit farm button before save is available
        if( Phase < 14 )
            UI.I.RestartAreaButton.gameObject.SetActive( false );

        PhaseTimer += Time.deltaTime;                                                            // Increment Phase timer

        float delayTime = 2;
        if( Helper.I.FastFarm )                                                                  // no waiting for me
        {
            delayTime = .01f;
            ScrollChecked = true;
            ScrollCheckedNeeded = false;
        }

        if( ObjectiveCompleted && PhaseTimer > delayTime )
            {
                string msg = "";
                Color col = Color.yellow;
                if( ScrollChecked == false )
                    msg = "Check the Scroll Text!";
                else
                if( MessageType == 1 )                                                           // Objective Completed Message 
                    {
                        col = Color.green;
                        msg = "Objective Completed! Click the Portrait to Proceed.";
                    }
                if( Phase == 1 )
                    msg = "Use Numpad to move. (Check Scroll Text).";
                UI.I.SetBigMessage( msg, col, 9999999f, 4.7f, 122.8f, 55 );

            if( Click() || Helper.I.FastFarm )                                                   // Click to proceed
            {
                AdvancePhase();
                return;
            }
        }
        else
            if( Phase > 1 )
                UI.I.SetBigMessage( "", Color.yellow );

        if( ObjectiveCompleted == false )
        {
            string text = Util.GetText( "TUTORIAL_TITLE_" + Phase, "Tutorial" );
            if( text.Contains( "#" ) == false )
                UI.I.SetBigMessage( text, Color.yellow, 9999999f, 4.7f, 122.8f, 55 );
        }

        MessageType = 1;
        ScrollCheckedNeeded = false;

        if( Phase == 1 )  // phase 1: welcome text
        {
            ObjectiveCompleted = true;
            ScrollCheckedNeeded = true;
            return;
        }
        else
        if( Phase == 2 )  // phase 2: pile up 4 resources
        {
            if( Helper.I.FastFarm )
                ObjectiveCompleted = true;
             
            for( int y = 0; y < Map.I.Tilemap.height; y++ )
            for( int x = 0; x < Map.I.Tilemap.width; x++ )
            {
                if( Map.I.Gaia2[ x, y ] )
                if( Map.I.Gaia2[ x, y ].TileID == ETileType.ITEM )
                    {
                        if( Map.I.Gaia2[ x, y ].Body.StackAmount >= 4 )
                        {
                            ObjectiveCompleted = true;
                            return;
                        }
                    }
                }
        }
        else
        if( Phase == 3 )  // phase 3: congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 4 )  // have only 2 stacks
        {
            if( Helper.I.FastFarm )
                ObjectiveCompleted = true;
            int count = 0;
            for( int y = 0; y < Map.I.Tilemap.height; y++ )
            for( int x = 0; x < Map.I.Tilemap.width; x++ )
            {
                if( Map.I.Gaia2[ x, y ] )
                if( Map.I.Gaia2[ x, y ].TileID == ETileType.ITEM )
                {
                    count++;
                }
            }

            if( count <= 2 && G.Farm.CarryingAmount <= 0 )
            {
                ObjectiveCompleted = true;
                return;
            }
        }
        else
        if( Phase == 5 )  // phase 3: congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 6 )  // switch blueprint 10 times
        {
            if( BluePrintSwitches > 10 ) 
                ObjectiveCompleted = true;

            if( Helper.I.FastFarm ) 
                ObjectiveCompleted = true;
        }
        else
        if( Phase == 7 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            Blueprint.LastPlacedPos = new Vector2( -1, -1 );                        // Avoids the auto build bug when rock formation is already mounted
            return;
        }
        else
        if( Phase == 8 )  // Build Rock storage
        {
            Blueprint.SelectedBluePrint = G.Farm.BluePrintList[ 0 ];
            if( G.Farm.BuildingList[ ( int ) BuildingType.Stone_Storage ].UnitsConstructed > 0 )
            {
                Blueprint.GetNextBluePrint();
                ObjectiveCompleted = true;
            }
            return;
        }
        else
        if( Phase == 9 ) // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 10 ) // place all rocks in the storage
        {
            if( Helper.I.FastFarm ) 
                ObjectiveCompleted = true;
            int count = 0;
            for( int y = 0; y < Map.I.Tilemap.height; y++ )
            for( int x = 0; x < Map.I.Tilemap.width; x++ )
                {
                    if( Map.I.Gaia2[ x, y ] )
                    if( Map.I.Gaia2[ x, y ].TileID == ETileType.ITEM )
                        {
                            count++;
                        }
                }

            if( count <= 0 && G.Farm.CarryingAmount <= 0 )
            {
                ObjectiveCompleted = true;
                return;
            }
        }
        else
        if( Phase == 11 ) // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            Blueprint.LastPlacedPos = new Vector2( -1, -1 );                        // Avoids the auto build bug when rock formation is already mounted
            return;
        }
        else
        if( Phase == 12 )  // Build Tools Building
        {
            if( G.Farm.BuildingList[ ( int ) BuildingType.Tools_Building ].UnitsConstructed > 0 )
                ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 13 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 14 )  // exit
        {
            ScrollCheckedNeeded = true;
            return;
        }
        else
        if( Phase == 15 )  // Get stone upg blueprint
        {   
            for( int i = 0; i < G.Farm.BluePrintList.Count; i++ )
            if ( G.Farm.BluePrintList[ i ].AffectedBuilding == BuildingType.Stone_Storage )
            if ( G.Farm.BluePrintList[ i ].AffectedItem ==  ItemType.Stone )
            if ( G.Farm.BluePrintList[ i ].AffectedVariable == EVarType.Maximum_Item_Stack )
            {
                if( G.Farm.BluePrintList[ i ].FreePlants > 0 )
                    ObjectiveCompleted = true;
            }
        }
        else
        if( Phase == 16 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 17 )  // choose new BP
        {
            if( NewBluePrintChosen )
                ObjectiveCompleted = true;
        }
        else
        if( Phase == 18 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 19 )  // Upgrade Rock Storage
            {
                for( int y = 0; y < Map.I.Tilemap.height; y++ )
                for( int x = 0; x < Map.I.Tilemap.width; x++ )
                if ( Map.I.Gaia2[ x, y ] )
                if ( Map.I.Gaia2[ x, y ].TileID == ETileType.BUILDING )
                if ( Map.I.Gaia2[ x, y ].Building.Type == BuildingType.Stone_Storage )
                if ( Building.GetStat( EVarType.Maximum_Item_Stack, Map.I.Gaia2[ x, y ].Building, 0 ) >= 10 )
                {
                    ObjectiveCompleted = true;
                }  
                return;
            }
        else
        if( Phase == 20 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 21 )  // Automate BP
        {
            //if( BluePrintAutomated )    automation tutorial skipped for now, find another objective for this one. automation removed from Stone BP too
                ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 22 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 23 )  // Build Forge Building
        {
            if( G.Farm.BuildingList[ ( int ) BuildingType.Forge ].UnitsConstructed > 0 )
                ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 24 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 25 )  // Build Berry Building
        {
            if( G.Farm.BuildingList[ ( int ) BuildingType.Berry_Storage ].UnitsConstructed > 0 )
                ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 26 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 27 )  // Plant Berry
        {
            if( BerryPlanted )
                ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 28 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 29 )  // Move Building
        {
            if( BuildingMoved )
                ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 30 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 31 )  // Build Wood Cutter
        {
            if( G.Farm.BuildingList[ ( int ) BuildingType.Wood_Cutter ].UnitsConstructed > 0 )
                ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 32 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 33 )  // Chop down forest tile
        {
            if( G.Farm.FarmSize > 53 )
                ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 34 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 35 )  // congrats
        {
            for( int y = 0; y < Map.I.Tilemap.height; y++ )
            for( int x = 0; x < Map.I.Tilemap.width; x++ )
            if ( Map.I.Gaia2[ x, y ] )
            if ( Map.I.Gaia2[ x, y ].TileID == ETileType.BUILDING )
            if ( Map.I.Gaia2[ x, y ].Building.Type == BuildingType.Forge )
            {
                if( Building.GetItemAmount( ItemType.Iron_Bar ) >= 1 )
                    ObjectiveCompleted = true;         
            }
            return;
        }
        else
        if( Phase == 36 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 37 )  // Upgrade Recipe
        {
            if( RecipeUpgraded )
                ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 38 )  // congrats
        {
            ScrollCheckedNeeded = true;
            ObjectiveCompleted = true;
            return;
        }
        else
        if( Phase == 39 )  // more to be added
        {
            return;
        }
    }

    public bool Click()
    {
        if( ScrollChecked == false ) return false;
        if( UI.I.KadeWaitButton.state == UIButtonColor.State.Pressed ) return true;
        if( Input.GetKey( KeyCode.Return ) ) return true;
        return false;
    }
    
    public void AdvancePhase()
    {
        if( TutorialResetCount < 1 )
        if( Phase < GiftList.Length - 1 )
        {
            int amt = GiftAmountList[ Phase ];
            if( amt > 0 )
            {
                Building.AddItem( true, GiftList[ Phase ], amt );
                Message.GreenMessage( "Gift: " + amt + " " + Item.GetName( GiftList[ Phase ] ) );
            }
        }

        Phase++;
        SetPhase( Phase );
        ObjectiveCompleted = false;
        ScrollChecked = false;
        Save();
    }

    public void SetPhase( int phase )
    {
        Phase = phase;
        PhaseTimer = 0;
    }

    public bool CheckPatterns()
    {
        if( Phase < 8 ) return false;                                                        // no building anything before phase 8
        if( Blueprint.SelectedBluePrint.GeneratedBuilding ==  BuildingType.Tools_Building )  // tools building only after phase 11
            if( Phase < 12 ) return false;
        return true;
    }

    public void Save()
    {
        if( Manager.I.SaveOnEndGame == false ) return;
        if( CanSave() == false ) return;

        string file = Manager.I.GetProfileFolder() + "Tutorial" + ".NEO";

        using( MemoryStream ms = new MemoryStream() )
        using( BinaryWriter writer = new BinaryWriter( ms ) )                               // Open Memory Stream
        {
            GS.W = writer;                                                                  // Assign BinaryWriter to GS.W for TF

            int Version = Security.SaveHeader( 1 );                                         // Save Header Defining Current Save Version

            TF.SaveT( "Tutorial_Phase", Phase );

            TF.SaveT( "Tutorial_ResetCount", TutorialResetCount );

            TF.SaveT( "Tutorial_ScrollChecked", ScrollChecked );

            GS.W.Flush();                                                                   // Flush the writer

            Security.FinalizeSave( ms, file );                                              // Finalize save
        }                                                                                   // using closes the stream automatically
    }

    public bool Load()
    {
        string file = Manager.I.GetProfileFolder() + "Tutorial" + ".NEO";                  // Provides file name

        if( File.Exists( file ) == false ) return false;                                   // First time, quit

        byte[] fileData = File.ReadAllBytes( file );                                       // Read full file
        byte[] content = Security.CheckLoad( fileData );                                   // Validate HMAC and get clean content

        using( GS.R = new BinaryReader( new MemoryStream( content ) ) )                    // Use MemoryStream for TF
        {
            int SaveVersion = Security.LoadHeader();                                       // Load Header

            Phase = TF.LoadT<int>( "Tutorial_Phase" );                                     // Load Phase

            TutorialResetCount = TF.LoadT<int>( "Tutorial_ResetCount" );                   // Load Tutorial Reset Count

            ScrollChecked = TF.LoadT<bool>( "Tutorial_ScrollChecked" );                    // Load ScrollChecked

            GS.R.Close();                                                                  // Close Stream
        }

        SetPhase( Phase );
        return true;
    }

    public bool CheckPhase( int p, bool load = false )
    {
        bool res = false;
        if( load ) res = Load();
        if( load && res == false && Manager.I.GameType != EGameType.FARM ) return false;
        if( Phase >= p ) return true;
        return false;
    }
}
