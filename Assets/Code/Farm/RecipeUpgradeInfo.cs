using UnityEngine;
using System.Collections;

public enum ERecipeUpgradeType
{
    NONE = -1, Req_1_Amt, Req_2_Amt, Req_3_Amt, Req_4_Amt,
               Gen_1_Amt, Gen_2_Amt, Gen_3_Amt, Gen_4_Amt, TIME = 10, Queue
}

public class RecipeUpgradeInfo : MonoBehaviour
{
    public ERecipeUpgradeType UpgradeType = ERecipeUpgradeType.NONE;
    public float UpgradeEffectAmount = 0;
    public ERecipeUpgradeType UpgradeType2 = ERecipeUpgradeType.NONE;
    public float UpgradeEffectAmount2 = 0;
    public ItemType UpgradeItemCostType = ItemType.NONE;
    public int UpgradeItemCostAmount = 1;

    public string UpdateText( ERecipeUpgradeType type, float amt, Recipe rc, int id, int b, int phase )
    {
        string info = " " + Map.I.Farm.BuildingList[ b ] + " " + rc + " " + id;                      
        string head = "Lv " + ( id + 2 ) + " - ";
        if( phase > 1 )
            head = "";

        if( type == ERecipeUpgradeType.TIME )
        {
            if( phase == 1 )
                head += "Time: " + Util.ToSTime( amt );
            if( amt >= 0 )
                Debug.LogError( "ERecipeUpgrade time cant be positive " + info );
        }

        if( phase == 1 || phase == 2 )
        {
            if( type == ERecipeUpgradeType.Req_1_Amt )
                head += "req 1: " + rc.Required_Item_1 + " " + amt.ToString( "+#;-#;0" );
            if( type == ERecipeUpgradeType.Req_2_Amt )
                head += "req 2: " + rc.Required_Item_2 + " " + amt.ToString( "+#;-#;0" );
            if( type == ERecipeUpgradeType.Req_3_Amt )
                head += "req 3: " + rc.Required_Item_3 + " " + amt.ToString( "+#;-#;0" );
            if( type == ERecipeUpgradeType.Req_4_Amt )
                head += "req 4: " + rc.Required_Item_4 + " " + amt.ToString( "+#;-#;0" );

            if( type == ERecipeUpgradeType.Gen_1_Amt )
                head += "gen 1: " + rc.Generated_Item_1 + " " + amt.ToString( "+#;-#;0" );
            if( type == ERecipeUpgradeType.Gen_2_Amt )
                head += "gen 2: " + rc.Generated_Item_2 + " " + amt.ToString( "+#;-#;0" );
            if( type == ERecipeUpgradeType.Gen_3_Amt )
                head += "gen 3: " + rc.Generated_Item_3 + " " + amt.ToString( "+#;-#;0" );
            if( type == ERecipeUpgradeType.Gen_4_Amt )
                head += "gen 4: " + rc.Generated_Item_4 + " " + amt.ToString( "+#;-#;0" );
            if( type == ERecipeUpgradeType.Queue )
                head += "queue: " + amt.ToString( "+#;-#;0" ); 
        }

        if( phase == 3 )
        {
            head += "                (" + UpgradeItemCostType + ": " + UpgradeItemCostAmount.ToString( "+#;-#;0" ) + ")";
        }

        this.transform.parent.name = "Recipe Upgrade Info";
        return "" + head + " ";
    }
}
