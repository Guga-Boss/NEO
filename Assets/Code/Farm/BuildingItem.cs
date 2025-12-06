using UnityEngine;
using System.Collections;

public class BuildingItem : MonoBehaviour
{
    public int ID = -1;
    public ItemType ItemType = 0;
    public float ItemCount = 0;
    public float ShownAcessories = 0;
    public float MaxItemStack = 0;
    public float BaseMaxItemStack = 1;
    public float BaseTotalProductionTime = 3600;
    public float TotalProductionTime = 0;
    public float ProductionTimeCount = 0;
    public float TotalWitherTime = 0;
    public int ToolHitCount = 0;
    public float AdditivePlantingFactor = -1;
    public bool WorkIsDone = false;
    public Building Building = null;
    public int ProductionLimit = -1;                 // This limits the automatic production to a certain limit. Above limit adds only if collected ingame
    public void Copy( BuildingItem it )
    {
        ItemCount = it.ItemCount;
        MaxItemStack = it.MaxItemStack;
        BaseMaxItemStack = it.BaseMaxItemStack;
        ItemType = it.ItemType;
        BaseTotalProductionTime = it.BaseTotalProductionTime;
        TotalProductionTime = it.TotalProductionTime;
        ProductionTimeCount = it.ProductionTimeCount;
        ToolHitCount = it.ToolHitCount;
        TotalWitherTime = it.TotalWitherTime;
        WorkIsDone = it.WorkIsDone;
        ShownAcessories = it.ShownAcessories;
        AdditivePlantingFactor = it.AdditivePlantingFactor;
        ProductionLimit = it.ProductionLimit;
    }

    // theres a bug that will hapen in the future. if i use an iron ore that is being used by the forge:
    // outside the farm, production is halted
    // inside: thats the bug: 
    public void UpdateBuildingItemProduction( Building bl, double addTime, int itemID )
    {
        float mstack = Building.GetStat( EVarType.Maximum_Item_Stack, bl, itemID );

        if( ProductionLimit != -1 )                                                            // Max Production limited                                    
            mstack = ProductionLimit;       

        float prod = Building.GetStat( EVarType.Total_Building_Production_Time, bl, itemID );

        if( prod <= 0 ) return;
        if( ItemCount < mstack && mstack > 0 )    //new last part
            ProductionTimeCount += ( float ) ( Time.unscaledDeltaTime + addTime );
        else
        {
            ProductionTimeCount = 0;
            return;
        }

        if( ItemCount < 0 ) ItemCount = 0;

        string msg = "";
        float amt = 0;
        float discard = 0;
        float original = ItemCount;
        for( ; ; )
            if( ProductionTimeCount >= prod )
            {
                ItemCount++;
                amt++;

                if( ItemCount >= mstack )
                {
                    discard = ItemCount - mstack;
                    ItemCount = mstack;
                    ProductionTimeCount = 0;
                    break;
                }
                ProductionTimeCount -= prod;
            }
            else break;

        if( addTime > 0 )
        {
            msg = G.GIT( ItemType ).GetName() + " Production: " + original + " + " + amt;
            if( discard > 0 ) msg += "Discarded: " + discard;
            Message.GreenMessage( msg );
        }
    }

    public void UpdateBuildingToolUsage( Building bl, double addTime, int itemID )
    {
        float prod = Building.GetStat( EVarType.Total_Building_Production_Time, bl, itemID );

        if( prod <= 0 ) return;

        ProductionTimeCount += ( float ) ( Time.unscaledDeltaTime + addTime );
        bool done = false;

        if( ProductionTimeCount >= prod )
        {
            done = true;
            ProductionTimeCount = 0;
        }

        bl.Unit.HealthBar.SetValueMax( ( int ) prod );
        bl.Unit.HealthBar.SetValueMin( 0 );

        float hits = ( float ) ProductionTimeCount;
        bl.Unit.HealthBar.SetValueCurrent( ( int ) hits );

        if( done )                                                                           // Work is Done, Finish Tasks
        {
            Unit ga = Map.I.GetUnit( bl.Unit.Pos, ELayerType.GAIA );
            Unit ga2 = Map.I.GetUnit( bl.Unit.Pos, ELayerType.GAIA2 );

            switch( bl.Type )
            {
                case BuildingType.Work_Area:
                if( bl.ToolType == ItemType.WoodAxe )                                              // clear the forest
                {
                    int x = ( int ) bl.Unit.Pos.x;
                    int y = ( int ) bl.Unit.Pos.y;
                    TKUtil.ClearForest( bl.Unit.Pos );
                    Map.I.Tilemap.SetTile( x, y, ( int ) ELayerType.DECOR, -1 );
                    Map.I.Tilemap.SetTile( x, y, ( int ) ELayerType.DECOR2, -1 );
                    Map.I.Tilemap.ForceBuild();
                }
                 
                if( bl.ToolType == ItemType.Hoe )                                                  // dig soil
                {
                    if( ga == null ) TKUtil.PrepareSoil( bl.Unit.Pos );
                    else TKUtil.ClearLayer( bl.Unit.Pos, ELayerType.GAIA );
                }

                if( bl.ToolType == ItemType.Shovel )                                               // dig river
                {
                    if( ga == null || ga.TileID == ETileType.MUD ) 
                        TKUtil.DigRiver( bl.Unit.Pos );
                    else TKUtil.ClearLayer( bl.Unit.Pos, ELayerType.GAIA );
                }
                break;
            }
            bl.Unit.Kill();
        }
    }

    public void UpdatePlantItem( Building bl, double addTime, int itemID )
    {           
        float prod = Building.GetStat( EVarType.Total_Building_Production_Time, bl, itemID );
        if( prod <= 0 ) return;
        ProductionTimeCount += ( float ) ( Time.unscaledDeltaTime + addTime );
        float fact = ( float ) ( ProductionTimeCount / prod );
        if( fact > 1 ) fact = 1;
        float max = Building.GetStat( EVarType.Maximum_Item_Stack, bl, itemID );

        float spriteFact = fact;
        if( spriteFact < .1f ) spriteFact = .1f;

        if( ProductionTimeCount >= prod )                                                                         // Plant is Ripe
        {
            WorkIsDone = true;
        }

        if( WorkIsDone == false )
        {
            bl.Unit.Spr.transform.localScale = new Vector3( spriteFact, spriteFact, 0 );                         // Plant Grow Animation
            bl.Unit.HealthBar.SetValueMax( ( int ) prod );
            bl.Unit.HealthBar.SetValueMin( 0 );
            float hits = ( float ) ProductionTimeCount;
            bl.Unit.HealthBar.SetValueCurrent( ( int ) hits );
            ShownAcessories = MaxItemStack + ( fact * 5 );
            ShownAcessories = Mathf.Ceil( ShownAcessories );
            ShownAcessories = Mathf.Clamp( ShownAcessories, 0, max );
        }

        for( int i = 0; i < bl.AcessoryList.Count; i++ )                                                         // Acessories (fruits) Sprite Handling
        {
            bl.AcessoryList[ i ].gameObject.SetActive( false );
            if( fact > 0.3f )
            if( i < ItemCount )
                {
                    bl.AcessoryList[ i ].gameObject.SetActive( true );
                    bl.AcessoryList[ i ].transform.localScale = new Vector3( spriteFact, spriteFact, 0 );
                }
        }

        if( WorkIsDone )                                                                                                         // Plant has Grown
        {
            if( TotalWitherTime > 0 )                                                                                            // Plant Wither 
            {
                float wit = TotalWitherTime - ( ProductionTimeCount - prod );
                if( wit <= 0 )
                    bl.Unit.Kill();
            }
            Manager.I.Tutorial.BerryPlanted = true;
        }
    }
}
