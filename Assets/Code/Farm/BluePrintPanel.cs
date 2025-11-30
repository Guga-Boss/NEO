using UnityEngine;
using System.Collections;

public class BluePrintPanel : MonoBehaviour
{
    public UI2DSprite[ , ] Sprites;
    public UISprite  [ , ] BackSprites;
    public UIButton[,] Buttons;
    public UISprite[] BackSpritesAux;
    public UILabel[ , ] Labels;
    public UILabel BluePrintLabel, UsesLabel, Cost1Label, Cost2Label, Cost3Label, AutoApplyCostLabel, PowerLabel, BuyBPLabel;
    public UI2DSprite Cost1Item, Cost2Item, Cost3Item, AutoApplyItem, BuyBPItem;
    public bool Initialized = false;
    public GameObject Selection;
    public int PanelNumber;
    public UIButton AutoApplyButton, BuyPlantsButton;

    public void InitSprites( GameObject folder )                                                                               // Init all Blueprints sprite links
    {
        if( Initialized ) return;
        UI2DSprite[] aux = folder.GetComponentsInChildren<UI2DSprite>();
        UILabel[ ] aux3 = folder.GetComponentsInChildren<UILabel>();

        Sprites = new UI2DSprite[ 5, 5 ];
        BackSprites = new UISprite[ 5, 5 ];
        Labels = new UILabel[ 5, 5 ];
        Buttons = new UIButton[ 5, 5 ];
        
        int id = 0;
        for( int y = 4; y >= 0; y-- )
        for( int x = 0; x < 5; x++ )
            {
                Sprites[ x, y ] = aux[ id ];
                BackSprites[ x, y ] = BackSpritesAux[ id ];
                Labels[ x, y ] = aux3[ id ];
                id++;
            }
        Initialized = true;
    }

    public void UpdateIt( Blueprint bp, string _title, int panel )
    {
        if( PanelNumber == 0 )
        {
            BluePrintLabel.color = Color.white;                                                      // Title
            if( bp.GeneratedBuilding != BuildingType.NONE )
                BluePrintLabel.color = Color.yellow;
            BluePrintLabel.text = _title;                                                            // Farm
            string pow = GetEffectPowerText( bp );
            PowerLabel.text = pow;            
            PowerLabel.transform.parent.gameObject.SetActive( false );
            if( pow != "" )
                PowerLabel.transform.parent.gameObject.SetActive( true );
            string mx = " (max:" + bp.MaxUses + ")";
            if( bp.MaxUses <= 0 ) mx = "";                                                           // Uses Text                                                         
                UsesLabel.text = "Uses: " + bp.UsesList.Count + "/" +
                ( bp.UsesList.Count + bp.AvailableUses ) + mx;    
        }

        Cost1Item.sprite2D = G.GIT( bp.CostItem ).Sprite.sprite2D;

        Cost1Label.text = "x" + bp.Cost.ToString("0.#");
        float suc = bp.BaseSuccessRate + bp.SuccessRate;
        Cost2Label.text = "" + suc.ToString( "0.#" ) + "%";

        for( int y = 0; y < 5; y++ )
        for( int x = 0; x < 5; x++ )
            {
                UI2DSprite sp = null;
                int id = ( int ) bp.ItemMatrix[ x, y ];
                UILabel label = Labels[ x, y ];

                if( Buttons[ x, y ] == null ) Buttons[ x, y ] = 
                    BackSprites[ x, y ].gameObject.GetComponent<UIButton>();

                if( id != -1 )                                                                                              // Item exists
                {
                    if( id != -1 )
                    {
                        sp = G.GIT( id ).Sprite;
                        Sprites[ x, y ].sprite2D = sp.sprite2D;
                    }

                    Buttons[ x, y ].enabled = false;
                    BackSprites[ x, y ].spriteName = Map.I.Farm.BluePrintItemBackSpriteActive.spriteName;
                    BackSprites[ x, y ].color = Map.I.Farm.BluePrintItemBackSpriteActive.color;
                    BackSprites[ x, y ].width = Map.I.Farm.BluePrintItemBackSpriteActive.width;
                    BackSprites[ x, y ].height = Map.I.Farm.BluePrintItemBackSpriteActive.height;
                    Buttons[ x, y ].hover = Color.black;
                    if( label )
                    {
                        label.text = "x" + bp.ItemAmount[ x, y ];

                        bool isCustom1 = bp.ItemMatrix[ x, y ] == ItemType.Tl_Blueprint_Icon_1;
                        bool isCustom2 = bp.ItemMatrix[ x, y ] == ItemType.Tl_Blueprint_Icon_2;
                        bool isItemNormal = !isCustom1 && !isCustom2;

                        if( isItemNormal )
                        {
                            if( bp.ItemAmount[ x, y ] <= 1 ) label.text = "";                                                 // Item normal: esconde se quantidade <= 1
                        }
                        else                                                                                                  // é Custom1 ou Custom2
                        {                           
                            bool isBuilding =
                            ( isCustom1 && Blueprint.IsBuilding( bp.ItemMatrix[ x, y ], bp.BPCustomIconType1 ) ) ||           // checa se é building apenas no tipo correspondente
                            ( isCustom2 && Blueprint.IsBuilding( bp.ItemMatrix[ x, y ], bp.BPCustomIconType2 ) );

                            if( !isBuilding || bp.ItemAmount[ x, y ] <= 1 )
                                label.text = "";                                                                              // Custom não-building: sempre esconde, ou building com quantidade <=1

                            bool isCustom = isCustom1 || isCustom2;
                            bool isCustomNonBuilding = isCustom && !isBuilding;
                            if( isCustomNonBuilding ) bp.ItemAmount[ x, y ] = 1;                                              // limits custom non building to 1 to prevent bugs and draw text correctly
                        }
                        label.depth = 5;
                    }

                    if( bp.ItemMatrix[ x, y ] == ItemType.Tl_Blueprint_Icon_1 )                                                // Custom type 1, like forest or buildings
                    if( bp.BPCustomIconType1 != EBPIconType.NONE )
                        Sprites[ x, y ].sprite2D = Map.I.Farm.BluePrintCustomSprite[ ( int ) bp.BPCustomIconType1 ].sprite2D;
                    else
                        SetSpriteEmpty( x, y, label );                                                                         // Sprite empty

                    if( bp.ItemMatrix[ x, y ] == ItemType.Tl_Blueprint_Icon_2 )                                                // Custom type 2, like forest or buildings
                    if( bp.BPCustomIconType2 != EBPIconType.NONE )
                        Sprites[ x, y ].sprite2D = Map.I.Farm.BluePrintCustomSprite[ ( int ) bp.BPCustomIconType2 ].sprite2D;
                    else
                        SetSpriteEmpty( x, y, label );                                                                         // Sprite empty
                }
                else                                                                                        // Empty slot
                {
                    SetSpriteEmpty( x, y, label );                                                          // Sprite empty
                }
           }

        if( panel == 0 )
        if( bp.AutoApplyCostItem != ItemType.NONE &&                                                        // Auto apply button update
            bp.AutoApplyCost > 0 )
        {
            AutoApplyButton.gameObject.SetActive( true );
            AutoApplyCostLabel.text = "x" + bp.AutoApplyCost;
            AutoApplyItem.sprite2D = G.GIT( bp.AutoApplyCostItem ).Sprite.sprite2D;
            bool res = bp.AutoApply( true );
            if( res ) AutoApplyCostLabel.color = Color.white;
            else AutoApplyCostLabel.color = Color.red;

            if( G.Tutorial.Phase < 21 )                                                                    // Only allows stone capacity upgrade if player has mannually upgraded at least a few time before
                AutoApplyButton.gameObject.SetActive( false );
        }
        else
            AutoApplyButton.gameObject.SetActive( false );

        if( panel == 0 )
        if( bp.BuyBPCostItem != ItemType.NONE &&                                                          // Buy BP button update
            bp.BuyBPCost > 0 )
            {
                BuyPlantsButton.gameObject.SetActive( true );
                BuyBPLabel.text = "x" + bp.BuyBPCost;
                BuyBPItem.sprite2D = G.GIT( bp.BuyBPCostItem ).Sprite.sprite2D;
                bool res = bp.BuyPlants( true );
                if( res ) BuyBPLabel.color = Color.white;
                else BuyBPLabel.color = Color.red;
            }
            else
            BuyPlantsButton.gameObject.SetActive( false );
    }

    void SetSpriteEmpty( int x, int y, UILabel label )
    {
        Sprites[ x, y ].sprite2D = null;
        Sprites[ x, y ].depth = 4;
        label.text = "";
        BackSprites[ x, y ].spriteName = Map.I.Farm.BluePrintItemBackSprite.spriteName;
        BackSprites[ x, y ].color = Map.I.Farm.BluePrintItemBackSprite.color;
        Buttons[ x, y ].enabled = false;
        Buttons[ x, y ].hoverSprite = BackSprites[ x, y ].spriteName;
        Buttons[ x, y ].pressedSprite = BackSprites[ x, y ].spriteName;
        Buttons[ x, y ].hover = Color.black;
        BackSprites[ x, y ].depth = 3;
        BackSprites[ x, y ].width = Map.I.Farm.BluePrintItemBackSprite.width;
        BackSprites[ x, y ].height = Map.I.Farm.BluePrintItemBackSprite.height;
    }

    public static string GetEffectPowerText( Blueprint bp )
    {
        string pow = "";
        float power = Blueprint.GetStat( EVarType.BluePrint_Effect_Amount, bp, 1 );
        if( power == 0 ) return "";
        if( bp.AffectedVariable == EVarType.Total_Building_Production_Time )                     // effect power
            pow += Util.ToSTime( power );
        else
            pow += power.ToString( "+#;-#;0" );
        return pow;
    }
}
