using UnityEngine;
using System.Collections;
using DarkTonic.MasterAudio;

public class RecipePanel : MonoBehaviour
{
    public UI2DSprite[] ReqSprites, GenSprites;
    public UI2DSprite UpgradeItemType;
    public UILabel TitleLabel, TimeLabel, LevelLabel, UpgradeItemCostLabel,
           UpgradeButtonLabel, ActivateButtonLabel, AvailableRecipesLabel, 
           ProductionBarLabel, QueueLabel;
    public UILabel[] ReqLabel, GenLabel, UpgradeHintLabel; 
    public UIButton UpgradeRecipeButton;
    public UISlider ProductionBar;
    public static bool IsUpgradeButtonHovered = false;
    public tk2dSprite CogSprite;

    public void UpdateIt( Recipe r, int id, Building bl )
    {
        if( bl != Recipe.SelectedBuilding.Building ) return;
        if( BluePrintWindow.I.gameObject.activeSelf )                                                               // BP Choosing is active
        {
            gameObject.SetActive( false );
            Map.I.Farm.BuildingUI.SetActive( false );
            return;
        }
        int ra = bl.GetRecipesAvailable( id );                                                                      // recipe not yet unlocked
        if( ra <= -1 ) return;
        int cicles = -1;
        if( bl )
            cicles = r.UpdateProductionData( id, bl );                                                              // get number of cicles

        if( ra < 1 ) cicles = 0;
        TitleLabel.text = "" + r.name + " (" + r.RecipeNumericalOrder + "/" + Recipe.TotRecipesEnabled + ")";
        float tot = r.GV( ERecipeUpgradeType.TIME );
        float rem = tot - r.TimeCount;
        TimeLabel.text = "Time: " + Util.ToSTime( rem ) + "   (Tot: " + Util.ToSTime( tot ) + ")";

        LevelLabel.text = "Level: " + ( r.Level + 1 );
        AvailableRecipesLabel.color = Color.white;                                                                  // recipes available info
        AvailableRecipesLabel.text = "x" + ra;
        if( ra < 1 ) AvailableRecipesLabel.color = Color.red;

        Item.SetAmt( ItemType.Recipe_Image_1, ra );

        TimeLabel.color = Color.white;
        LevelLabel.color = Color.white;
        ActivateButtonLabel.color = Color.green;
        ActivateButtonLabel.text = "On";
        if( r.Activated == false )
        {
            ActivateButtonLabel.text = "Off";
            ActivateButtonLabel.color = Color.red;
        }
        int max = r.GV( ERecipeUpgradeType.Queue );
        QueueLabel.text = "+" + r.QueueLenght + " (" + max + ")";                                         // Queue info

        UpdateUpgrade( bl );                                                                              // Updates upgrade stuff

        float per = ( int ) Util.GetPercent( r.TimeCount, tot );                                          // Production Bar update
        ProductionBar.value = ( per / 100f );
        ProductionBarLabel.text = "" + per + "%";
        float chc = 1;
        if( cicles < 1 )
        if( Util.Chance( 50 ) ) chc = -1;
        if( r.Activated )                                                                                 // Cog Animation
            CogSprite.transform.Rotate( 0, 0, (-25 * Time.deltaTime) * chc );
        Map.I.Farm.RecipePanel.transform.localPosition = new Vector3( -1362.2f, 384.1f, 0 );

        for( int i = 0; i < 4; i++ )
        {
            ReqSprites[ i ].transform.parent.gameObject.SetActive( false );
            GenSprites[ i ].transform.parent.gameObject.SetActive( false );
        }

        ProcessItemIcon( 0, ERecipeUpgradeType.Req_1_Amt, r.Required_Item_1, r, false );
        ProcessItemIcon( 1, ERecipeUpgradeType.Req_2_Amt, r.Required_Item_2, r, false );
        ProcessItemIcon( 2, ERecipeUpgradeType.Req_3_Amt, r.Required_Item_3, r, false );
        ProcessItemIcon( 3, ERecipeUpgradeType.Req_4_Amt, r.Required_Item_4, r, false );

        ProcessItemIcon( 0, ERecipeUpgradeType.Gen_1_Amt, r.Generated_Item_1, r, true );
        ProcessItemIcon( 1, ERecipeUpgradeType.Gen_2_Amt, r.Generated_Item_2, r, true );
        ProcessItemIcon( 2, ERecipeUpgradeType.Gen_3_Amt, r.Generated_Item_3, r, true );
        ProcessItemIcon( 3, ERecipeUpgradeType.Gen_4_Amt, r.Generated_Item_4, r, true );
    }

    public void OnEnable()
    {
        if( Map.I == null ) return;
        if( Manager.I == null ) return;
        Vector2 tg = Map.I.Hero.GetFront();
        Unit fbl = Map.I.GetUnit( ETileType.BUILDING, tg );
        if( fbl == null ) return;
        string sound = "";
        if( Recipe.SelectedBuilding == null ) return;
        if( Recipe.SelectedBuilding.Building.Type == BuildingType.Forge ) sound = "Mining";
        //if( Recipe.SelectedBuilding.Building.Type == BuildingType.Firepit ) sound = "Fire Ignite";
        if( sound != "" )
            MasterAudio.PlaySound3DAtVector3( sound, Map.I.Hero.Pos );
    }

    public void UpdateUpgrade( Building bl )
    {
        UpgradeRecipeButton.gameObject.SetActive( true );

        for( int i = 0; i < UpgradeHintLabel.Length; i++ )
            UpgradeHintLabel[ i ].gameObject.SetActive( false );
        
        int lev = bl.RecipeList[ ( int ) bl.SelectedRecipeID ].Level;
        if( lev >= bl.RecipeList[ ( int ) bl.SelectedRecipeID ].RecipeUpgradeInfoList.Length )                                                                                
        {
            UpgradeRecipeButton.gameObject.SetActive( false );                                                 // Max Level Reached
            return;
        }

        RecipeUpgradeInfo ui = bl.RecipeList[ ( int ) bl.SelectedRecipeID ].RecipeUpgradeInfoList[ lev ];
        UpgradeItemType.sprite2D = G.GIT( ui.UpgradeItemCostType ).Sprite.sprite2D;
        UpgradeItemCostLabel.text = "x" + ui.UpgradeItemCostAmount;
        bool res = bl.RecipeList[ ( int ) bl.SelectedRecipeID ].UpgradeRecipe( false, bl );

        if( res )                                                                                              // Upgrade button and stuff color
        {
            UpgradeButtonLabel.color = Color.green;
            UpgradeItemCostLabel.color = Color.white;
        }
        else
        {
            UpgradeButtonLabel.color = Color.red;
            UpgradeItemCostLabel.color = Color.red;
        }

        if( IsUpgradeButtonHovered )                                                                           // mouse over button upg effect
        {
            RecipeUpgradeInfo up = bl.RecipeList[ ( int ) 
            bl.SelectedRecipeID ].RecipeUpgradeInfoList[ lev ];
            UpdateUpgradeItemText( up.UpgradeType, up.UpgradeEffectAmount );
            UpdateUpgradeItemText( up.UpgradeType2, up.UpgradeEffectAmount2 );    
        }
    }

    public void UpdateUpgradeItemText( ERecipeUpgradeType type, float amt )
    {
        int id = -1;
        if( type == ERecipeUpgradeType.Req_1_Amt ) id = 0;
        if( type == ERecipeUpgradeType.Req_2_Amt ) id = 1;
        if( type == ERecipeUpgradeType.Req_3_Amt ) id = 2;
        if( type == ERecipeUpgradeType.Req_4_Amt ) id = 3;
        if( type == ERecipeUpgradeType.Gen_1_Amt ) id = 4;
        if( type == ERecipeUpgradeType.Gen_2_Amt ) id = 5;
        if( type == ERecipeUpgradeType.Gen_3_Amt ) id = 6;
        if( type == ERecipeUpgradeType.Gen_4_Amt ) id = 7;
        if( type == ERecipeUpgradeType.TIME ) id = 8;
        if( type == ERecipeUpgradeType.Queue ) id = 9;

        if( id != -1 )
        {
            UpgradeHintLabel[ id ].gameObject.SetActive( true );
            UpgradeHintLabel[ id ].text = "" + amt.ToString( "+#;-#;0" );

            if( id == 8 )
                UpgradeHintLabel[ id ].text = "" + Util.ToSTime( amt );
            if( id == 9 )
                UpgradeHintLabel[ id ].text = "" + "+" + amt;
        }     
    }

    public void ProcessItemIcon( int arrayId, ERecipeUpgradeType upt, ItemType itt, Recipe r, bool gen )
    {
        if( itt == ItemType.NONE ) return;
        int amt = r.GV( upt );
        if( amt > 0 )
        {
            if( gen == false )
            {
                ReqSprites[ arrayId ].sprite2D = G.GIT( itt ).Sprite.sprite2D;
                ReqLabel[ arrayId ].text = "x" + amt;
                ReqSprites[ arrayId ].transform.parent.gameObject.SetActive( true );
                ReqLabel[ arrayId ].color = Color.white;
                float avail = Item.GetNum( itt );                                                                  // label gets red if theres no req items available
                if( avail < amt )
                    ReqLabel[ arrayId ].color = Color.red;
            }
            else
            {
                GenSprites[ arrayId ].sprite2D = G.GIT( itt ).Sprite.sprite2D;
                GenLabel[ arrayId ].text = "x" + amt;
                GenSprites[ arrayId ].transform.parent.gameObject.SetActive( true );

                Building bl = Recipe.SelectedBuilding.Building;
                int itmid = Building.GetBuildingItemID( itt, bl );
                float tot = Building.GetStat( EVarType.Maximum_Item_Stack, bl, itmid );
                float avail = bl.Itm[ itmid ].ItemCount;
                float space = tot - avail;

                GenLabel[ arrayId ].color = Color.white;                                                           // label gets red if theres no enough space for gen item
                if( tot > 0 )
                if( amt > space )
                    GenLabel[ arrayId ].color = Color.red;
            }
        }
    }
    public void UpgradeRecipeButtonCallback()
    {
        Unit un = Recipe.SelectedBuilding;
        if( un == null ) return;
        Building bl = un.Building;
        bl.RecipeList[ ( int ) bl.SelectedRecipeID ].UpgradeRecipe( true, bl );
    }

    public void ChangeQueueCallback()
    {
        Unit un = Recipe.SelectedBuilding;
        if( un == null ) return;
        Building bl = un.Building;
        Recipe rp = bl.RecipeList[ ( int ) bl.SelectedRecipeID ];
        int max = rp.GV( ERecipeUpgradeType.Queue );
        if( ++rp.QueueLenght > max ) rp.QueueLenght = 1;
    }
    public void ActivateRecipeButtonCallback()
    {
        Unit un = Recipe.SelectedBuilding;
        if( un == null ) return;
        Building bl = un.Building;
        Recipe rp = bl.RecipeList[ ( int ) bl.SelectedRecipeID ];
        rp.Activated = !rp.Activated;
        int max = rp.GV( ERecipeUpgradeType.Queue );
        rp.QueueLenght = max;
    }
    public void NextRecipeButtonCallback()
    {
        Recipe.GetNextRecipe( +1, Recipe.SelectedBuilding.Building );
    }
}