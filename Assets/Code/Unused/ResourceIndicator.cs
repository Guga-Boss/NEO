using UnityEngine;
using System.Collections;

public class ResourceIndicator : MonoBehaviour 
{
    public ItemType ItemType;
    public UI2DSprite Sprite;
    public UILabel Label;
    public static bool UpdateGrid = false;
    public static int IndicatorCount = 0;
    public static int MonsterGatesCount = 0, FirepitPointsCount = 0;
    public static void DisableAll()
    {
        for( int i = 0; i < UI.I.ResourceIndicators.Count; i++ )
        {
            UI.I.ResourceIndicators[ i ].gameObject.SetActive( false );
        }
    }

    public static void UpdateIt()
    {        
        if( UpdateGrid == false ) return;
        DisableAll();
        IndicatorCount = 0;
        UpdateGrid = false;

        if( Manager.I.GameType == EGameType.FARM )
        {
            UpdateIndicator( ItemType.Res_Plague_Monster );
            return;
        }

        if( Map.I.RM.GameOver ) return;

        RandomMapData rm = Map.I.RM.RMD;
        UpdateIndicator( ItemType.Res_Bow_Arrow );
        UpdateIndicator( ItemType.Res_Melee_Attacks );
        UpdateIndicator( ItemType.Res_Boomerang );
        UpdateIndicator( ItemType.Res_Fire_Staff );
        UpdateIndicator( ItemType.Res_Throwing_Axe );
        UpdateIndicator( ItemType.Res_Spear );
        UpdateIndicator( ItemType.Res_Bamboo );
        UpdateIndicator( ItemType.Res_Hook_CW );             // Both Hooks only use CW forr data management
        UpdateIndicator( ItemType.Res_Mining_Points );
        UpdateIndicator( ItemType.Res_Mining_Bag );
        UpdateIndicator( ItemType.Res_SnowStep );
        UpdateIndicator( ItemType.Res_Stitches );
        UpdateIndicator( ItemType.Res_Oxygen );
        UpdateIndicator( ItemType.Res_Mask );
        UpdateIndicator( ItemType.Res_Bone );
        UpdateIndicator( ItemType.Res_Mushroom_Potion );
        UpdateIndicator( ItemType.Res_FireWood );
        UpdateIndicator( ItemType.Res_Bridge_Wood );
        UpdateIndicator( ItemType.Res_Fish_Yellow );
        UpdateIndicator( ItemType.Res_Fish_Red );
        UpdateIndicator( ItemType.Res_Fish_Blue );
        UpdateIndicator( ItemType.Res_Fish_Crab );
        UpdateIndicator( ItemType.Res_Fish_Manta_Ray );
        UpdateIndicator( ItemType.Res_Fish_Water_Snake );
        UpdateIndicator( ItemType.Res_Fish_Brown );
        UpdateIndicator( ItemType.Res_Fish_Frog );
        UpdateIndicator( ItemType.Res_Red_Herb );
        UpdateIndicator( ItemType.Res_Green_Herb );
        UpdateIndicator( ItemType.Res_Blue_Herb );
        UpdateIndicator( ItemType.Res_Yellow_Herb );
        UpdateIndicator( ItemType.Res_Black_Herb );
        UpdateIndicator( ItemType.Res_White_Herb );
        UpdateIndicator( ItemType.Res_BirdCorn );
        UpdateIndicator( ItemType.Res_Green_Mine_Crystal );
        UpdateIndicator( ItemType.Res_Cramps );
        UpdateIndicator( ItemType.Res_Water_Flower );
        UpdateIndicator( ItemType.Res_Hero_Shield );
        UpdateIndicator( ItemType.Res_Monster_Kill );
        UpdateIndicator( ItemType.Res_Plague_Monster );
        UpdateIndicator( ItemType.Res_Rudder );
        UpdateIndicator( ItemType.Res_Quiver );
        UpdateIndicator( ItemType.Res_Butcher_Coin );
        UpdateIndicator( ItemType.Res_Butcher_Prize );
        UpdateIndicator( ItemType.Res_Vine_Bale );
        UpdateIndicator( ItemType.Res_Fishing_Pole );
        UpdateIndicator( ItemType.Res_Harpoon );
        UpdateIndicator( ItemType.Res_Mining_Sieve );
        UpdateIndicator( ItemType.Res_Kick );
        UpdateIndicator( ItemType.Res_Exploding_Plant );
        UpdateIndicator( ItemType.Res_Torch );
        UpdateIndicator( ItemType.Res_Fire_Lits );
        UpdateIndicator( ItemType.Res_Double_Attack );
        UpdateIndicator( ItemType.Res_WebTrap );
        UpdateIndicator( ItemType.Res_Attack_Bonus );

        if( IndicatorCount <= 6 )
            UI.I.ResourcesGrid.transform.localPosition = new Vector3( -715.8f, -103.1f, 0 );
        else
            UI.I.ResourcesGrid.transform.localPosition = new Vector3( -715.8f, -61.38f, 0 );
        UI.I.ResourcesGrid.repositionNow = true;
    }

    public static void UpdateIndicator( ItemType itemType )
    {
        float amt = G.GIT( itemType ).GetCount();
        bool global = G.GIT( itemType ).IsGlobalGameplayResource;
        bool force = false;
        if( itemType == ItemType.Res_Bow_Arrow )
        {
            force = true;
            if( Map.I.RM.RMD.LimitedArrowPerCube == -1 )
                force = false;
        }

        if( itemType == ItemType.Res_Melee_Attacks )
        {
            force = true;
            if( Map.I.RM.RMD.LimitedMeleeAttacksPerCube == -1 ) 
                force = false;
        }

        if( itemType == ItemType.Res_Monster_Kill )                        // Show monster kill only if theres gate
        if( MonsterGatesCount <= 0 )
            return;
          
        if( itemType == ItemType.Res_Fire_Lits )                          // Show firepits points only if theres gate
        if( FirepitPointsCount <= 0 )
            return;
        
        if( amt >= 1 || force )
        {
            UI.I.ResourceIndicators[ IndicatorCount ].Label.color = Color.white;
            UI.I.ResourceIndicators[ IndicatorCount ].gameObject.SetActive( true );
            UI.I.ResourceIndicators[ IndicatorCount ].Label.text = "x" + amt.ToString( "0." );
            if( global )
            {
                //UI.I.ResourceIndicators[ IndicatorCount ].Label.text += " (G)";
                UI.I.ResourceIndicators[ IndicatorCount ].Label.color = Color.green;
            }
            UI.I.ResourceIndicators[ IndicatorCount ].Sprite.sprite2D =
            G.GIT( itemType ).Sprite.sprite2D;
            IndicatorCount++;

            if( IndicatorCount > UI.I.ResourceIndicators.Count - 1 )
            {
                IndicatorCount = UI.I.ResourceIndicators.Count - 1;
            }
        }
    }
}
