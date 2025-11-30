using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;
public class Chests : MonoBehaviour 
{
    public static Chests I;
    [System.Serializable]
    public class SortData 
    {
        [HorizontalGroup( "Data", Width = 0.5f )]
        [LabelWidth( 50 )] 
        public ItemType Prize = ItemType.NONE;              // Item type
        [HorizontalGroup( "Data", Width = 0.5f )]
        public float SortFactor = 100;
        [FoldoutGroup( "Advanced" )]
        public float PrizeAmount = 1;                       // Default Amount of item
        [FoldoutGroup( "Advanced" )]
        public List<float> PrizeAmountList = null;          // or sort amount if you want                
        [FoldoutGroup( "Advanced" )]
        public List<float> PrizeAmountListFactor = null;    // factor
    }
    public List<SortData> Level1ChestList;
    public List<SortData> Level2ChestList;
    public List<SortData> Level3ChestList;
    public List<SortData> Level4ChestList;
    void Start()
    {
        I = this;
    }
    public static void Init( Unit it )
    {
        AdventureUpgradeInfo.ChestBonusItem = new List<ItemType>();
        AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.CHEST_BONUS_CHANCE );                             // this is just for filling the ChestBonusItem list for item adding
        AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.CHEST_BONUS_AMOUNT );
        List<ItemType> bnl = new List<ItemType>();
        
        for( int i = 0; i < 4; i++ )
            I.UpdateSort( i, it );

        if( Util.HasDada( Map.I.RM.RMD.ChestBonusItemList ) )
            bnl.AddRange( Map.I.RM.RMD.ChestBonusItemList );
        else
            bnl.AddRange( it.Body.BonusItemList );

        for( int i = 0; i < AdventureUpgradeInfo.ChestBonusItem.Count; i++ )                                                // add a new tech item to the gameobject list
        if ( bnl.Contains( AdventureUpgradeInfo.ChestBonusItem[ i ] ) == false )
            {
                bnl.Add( AdventureUpgradeInfo.ChestBonusItem[ i ] );
                it.Body.BonusItemAmountList.Add( 0 );
                it.Body.BonusItemChanceList.Add( 0 );
                it.Body.BonusItemChestLevelList.Add( 0 );
            }

        for( int i = 0; i < bnl.Count; i++ )
        {
            float chc = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.CHEST_BONUS_CHANCE, bnl[ i ] );                 // update new tech item values
            float amt = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.CHEST_BONUS_AMOUNT, bnl[ i ] );

            if( it.Body.BonusItemAmountList[ i ] < 1 )                                                                      // minimum amount to 1
                it.Body.BonusItemAmountList[ i ] = 1;
            it.Body.BonusItemAmountList[ i ] += amt;
            it.Body.BonusItemChanceList[ i ] += chc;

            float basebn = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.CHEST_BASE_BONUS_CHANCE );                   // update new tech item values
            it.Body.BonusItemChanceList[ i ] += Util.Percent( basebn, it.Body.BonusItemChanceList[ i ] );
        }
        it.Body.BonusItemList = new List<ItemType>();
        it.Body.BonusItemList.AddRange( bnl );
        int ori = Mod.GetOriNumber( it.Pos );
        if( ori > 0 && ori <= 4 )                                                                             // Pre defined chest level
            it.Body.ChestLevel = ori;

        float upchc = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.INITIAL_UPGRADE_CHEST_CHANCE );     // upgrade chest
        if( Util.Chance( upchc ) )
            Chests.UpgradeChest( it, false );
    }
    private void UpdateSort( int id, Unit it )
    {
        List<SortData> list = Level1ChestList;                                     // choose wich list depending on level
        if( id == 1 ) list = Level2ChestList;
        if( id == 2 ) list = Level3ChestList;
        if( id == 3 ) list = Level4ChestList;
        List<float> fl = new List<float>();
        for(  int i = 0; i < list.Count; i++)
        {
            fl.Add( list[ i ].SortFactor );                                        // fill sort list
        }

        int res = Util.Sort( fl );                                                 // sort prize
        SortData sd = list[ res ];
        it.Body.BonusItemList[ id ] = Item.GetSortedItem( sd.Prize );
        it.Body.BonusItemAmountList[ id ] = sd.PrizeAmount;

        if( Util.HasDada( sd.PrizeAmountList ) )
        {
            int idd = Util.Sort( sd.PrizeAmountListFactor );
            it.Body.BonusItemAmountList[ id ] = sd.PrizeAmountList[ idd ];        // sort prize amount       
        }
    }

    public static void UpgradeChests( float chance, string type = "" )
    {
        if( chance <= 0 ) return;
        int amt = 0;
        for( int yy = ( int )  G.HS.Area.yMin - 1; yy < G.HS.Area.yMax + 1; yy++ )             // loop
        for( int xx = ( int ) G.HS.Area.xMin - 1;  xx < G.HS.Area.xMax + 1; xx++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
        {
            Unit it = Map.I.Gaia2[ xx, yy ];
            if( it && Map.I.Gaia2[ xx, yy ].TileID == ETileType.ITEM )                         // chest found
            {
                if( it.Body.IsChest() )
                if( Util.Chance( chance ) )
                if( UpgradeChest( it ) ) amt++;                                                // Upgrade chest
            }
        }
        if( amt > 0 ) 
            Message.GreenMessage( type + " Chest Upgraded! x" + amt );                         // message
    }

    public static bool UpgradeChest( Unit chest, bool fx = true )
    {
        if( chest.Body.ChestLevel >= 4 ) return false;
        chest.Body.ChestLevel++;
        if( fx ) 
            Controller.CreateMagicEffect( chest.Pos );                                     // Magic FX
        return true;
    }

    public static bool GetChestText( Unit un, ref string txt, ref string snd, Unit res, bool sort )
    {
        float inflation = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.CHEST_ITEM_CHANCE_INFLATION );
        
        bool success = false;
        for( int i = 0; i < res.Body.BonusItemList.Count; i++ )
        if ( res.Body.ChestLevel >= res.Body.BonusItemChestLevelList[ i ] + 1 )
            {
                int amtl = 1;
                float chance = 100;
                if( res.Body.BonusItemAmountList != null &&                                                              // Chest Amount calculation
                    res.Body.BonusItemAmountList.Count >= 1 &&
                    i < res.Body.BonusItemAmountList.Count )
                {
                    amtl = Util.FloatSort( res.Body.BonusItemAmountList[ i ] );
                }

                if( res.Body.BonusItemChanceList != null &&                                                              // Chest Chance Calculation
                    res.Body.BonusItemChanceList.Count >= 1 &&
                     i < res.Body.BonusItemChanceList.Count )
                {
                    int amt = Map.I.SessionChestsOpenCount;
                    chance = res.Body.BonusItemChanceList[ i ];
                    if( Map.I.RM.RMD.ChestInflationCummulative )
                        chance = Util.CompoundInterest( chance, ( int ) amt, inflation );                                // Tech: Chest inflation Cummulative
                    else
                        chance += Util.Percent( chance, amt * inflation );                                               // Tech: Chest inflation NON Cummulative
                }

                Item itt = G.GIT( res.Body.BonusItemList[ i ] );
                string ctxt = chance.ToString( "0." ) + "%)";
                if( chance < 5 ) ctxt = chance.ToString( "0.#" ) + "%)";
            
                string xtra = "";            
                if( i >= res.Body.ChestLevel )
                {
                    if( success == false )
                    if( sort )
                    {
                        chance = 0;                                                                                      // started sorting extra but first 4 sorts failed. extra chance should fail
                    }
                    xtra = "Extra: ";
                }

                txt += xtra + Item.GetName( itt.Type ) + " x" + amtl + "    (" + ctxt;                                   //"L" + ( i + 1 ) + " - "

                if( Util.Chance( chance ) || sort == false )                                                             // Sort successful
                {
                    if( sort )
                    {
                        bool trash = false;
                        Item.ForceMessage = true;
                        un.Control.GivePrize( itt, amtl, res, ref trash );
                        txt += " - SUCCESS!!";
                        snd = "Cashier";
                        success = true;
                    }
                    txt += "\n";
                }
                else
                {
                    string fail = "x" + amtl + " Fail.";                                                                 // Sort Failed
                    Vector2 pos = res.Pos + new Vector2(
                    Random.Range( -2f, 2f ), Random.Range( -2f, 2f ) );
                    txt += " - Fail!\n";
                }
            }

        if( sort )
        if( success )
            {
                int amt = 1;
                if( res.Body.ChestLevel == 2 ) amt = 2;
                if( res.Body.ChestLevel == 3 ) amt = 3;
                if( res.Body.ChestLevel == 4 ) amt = 5;
                Item.AddItem( ItemType.Chest_Points, amt );
                Map.I.SessionChestsOpenCount++;                                                                          // Increment Chest in the session var
            }
            else
            {
                Map.I.CreateExplosionFX( res.Pos );                                                                      // Explosion FX
            }
        return success;
    }

    public static void ChestStep( Unit un, ref string snd, ref bool destroy, Unit res )
    {
        string txt = "Chest Opened:\n";
        bool open = Chests.GetChestText( un, ref txt, ref snd, res, true );
        MasterAudio.PlaySound3DAtVector3( snd, res.Pos );
        Statistics.AddStats( Statistics.EStatsType.RESOURCECOLLECTED, 1f );
        destroy = true;
        float chc = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.CHEST_PERSIST_CHANCE );       // Chest persist
        if( chc > 0 )
        {
            txt += "Chest Persist Chance: " + chc.ToString( "0." ) + "%";
            if( Util.Chance( chc ) ) { destroy = false; txt += " OK!"; }
            else txt += " Failed!";
        }

        if( destroy )
            Controller.UpdateResourceDestruction( res );                                              // Destroy item obj

        UI.I.SetTurnInfoText( txt, 3, Color.white );
        Item.AddItem( ItemType.Quest_XP, Map.I.RM.RMD.QuestXPPerChest );

        float upchc = Map.I.RM.RMD.OpenChestUpgradeChestChance;                                      // upgrade chest after opening chest
        if( open )
            Chests.UpgradeChests( upchc );

        GlobalAltar.UpdateEvolution( "chest" );                                                      // Chest Evolve Random Altar                       
    }
}
