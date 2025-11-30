using UnityEngine;
using System.Collections;

public enum EGoalUpgradeType
{
    NONE = -1, ITEM = 0, ITEM2, RECIPE_BONUS, UNLOCK_GOAL,
    BLUEPRINT_PLANTS,
    BLUEPRINT_USES,

}

public class GoalUpgradeInfo : MonoBehaviour 
{
    public EGoalUpgradeType UpgradeType = EGoalUpgradeType.ITEM;
    public float UpgradeBonusAmount = 1;
    public float ShopLevelRequired = 1;
    [Space( 10 )]
    public ItemType UpgradeCostType = ItemType.NONE;
    public float UpgradeCostAmount = 1;

    public void SortUniqueID()
    {
        ////*************************************************************************************************
        //if( UniqueID != "" ) return; //*************NEVER REMOVE THIS OR YOU WILL SCREW UP GOAL SAVE SYSTEM 
        ////*************************************************************************************************

        //const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789"; //add the characters you want

        //int charAmount = 4;
        //for( int i = 0; i < charAmount; i++ )
        //{
        //    UniqueID += glyphs[ Random.Range( 0, glyphs.Length ) ];
        //}
    }
}
