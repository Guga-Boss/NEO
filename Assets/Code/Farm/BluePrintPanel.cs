using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class BluePrintPanel : MonoBehaviour
{
    public NSprite[ , ] Sprites;
    public NSprite  [ , ] BackSprites;
    public UIButton[,] Buttons;
    public NSprite[] BackSpritesAux;
    public TextMeshPro[ , ] Labels;
    public TextMeshPro BluePrintLabel,UsesLabel, Cost1Label, PowerLabel, BuyBPLabel, AutoApplyCostLabel;
    public UI2DSprite Cost2Item, Cost3Item;
    public NSprite Cost1Item, BuyBPItem, AutoApplyItem;
    public bool Initialized = false;
    public int PanelNumber;
    public Button AutoApplyButton, BuyPlantsButton;

    public void InitSprites( GameObject folder )                                                                               // Init all Blueprints sprite links
    {
        if( Initialized ) return;

        Sprites = new NSprite[ 5, 5 ];
        BackSprites = new NSprite[ 5, 5 ];
        Labels = new TextMeshPro[ 5, 5 ];

        Transform root = folder.transform;
        NSprite[] aux = new NSprite[ 25 ];
        TextMeshPro[] aux3 = new TextMeshPro[ 25 ];

        // Percorre os 25 objetos "BP Item" na pasta
        for( int i = 0; i < 25; i++ )
        {
            Transform bpItem = root.GetChild(i);

            // Pega o NSprite que está dentro do objeto filho chamado "Sprite"
            aux[ i ] = bpItem.Find( "Sprite" ).GetComponent<NSprite>();

            // Pega o TextMeshPro que está dentro do objeto filho chamado "Label"
            aux3[ i ] = bpItem.Find( "Label" ).GetComponent<TextMeshPro>();
        }

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

        Cost1Item.spriteId = G.GIT( bp.CostItem ).NSprite.TkSpriteId;                                // Attrib Sprite

        Cost1Label.text = "x" + bp.Cost.ToString("0.#");
        float suc = bp.BaseSuccessRate + bp.SuccessRate;

        for( int y = 0; y < 5; y++ )
        for( int x = 0; x < 5; x++ )
            {
                int id = ( int ) bp.ItemMatrix[ x, y ];
                TextMeshPro label = Labels[ x, y ];

                //if( Buttons[ x, y ] == null ) Buttons[ x, y ] = 
                //    BackSprites[ x, y ].gameObject.GetComponent<UIButton>();

                if( id != -1 )                                                                                              // Item exists
                {
                    if( id != -1 )
                    {
                        Sprites[ x, y ].spriteId = G.GIT( id ).NSprite.TkSpriteId;

                        //Sprites[ x, y ].spriteId = G.GIT( id ).NSprite.spriteId; 
                    }

                    //Buttons[ x, y ].enabled = false;
                    //BackSprites[ x, y ].spriteName = Map.I.Farm.BluePrintItemBackSpriteActive.spriteName;
                    //BackSprites[ x, y ].color = Map.I.Farm.BluePrintItemBackSpriteActive.color;
                    //ggggBackSprites[ x, y ].width = Map.I.Farm.BluePrintItemBackSpriteActive.width;
                    //BackSprites[ x, y ].height = Map.I.Farm.BluePrintItemBackSpriteActive.height;
                    //Buttons[ x, y ].hover = Color.black;
                    Blueprint.BPSortData sd = bp.BPSort[ bp.SortID ];

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
                            ( isCustom1 && Blueprint.IsBuilding( bp.ItemMatrix[ x, y ], sd.BPCustomIconType1 ) ) ||           // checa se é building apenas no tipo correspondente
                            ( isCustom2 && Blueprint.IsBuilding( bp.ItemMatrix[ x, y ], sd.BPCustomIconType2 ) );

                            if( !isBuilding || bp.ItemAmount[ x, y ] <= 1 )
                                label.text = "";                                                                              // Custom não-building: sempre esconde, ou building com quantidade <=1

                            bool isCustom = isCustom1 || isCustom2;
                            bool isCustomNonBuilding = isCustom && !isBuilding;
                            if( isCustomNonBuilding ) bp.ItemAmount[ x, y ] = 1;                                              // limits custom non building to 1 to prevent bugs and draw text correctly
                        }
                        //gggglabel.depth = 5;
                    }

                    if( bp.ItemMatrix[ x, y ] == ItemType.Tl_Blueprint_Icon_1 )                                                // Custom type 1, like forest or buildings
                    if( sd.BPCustomIconType1 != EBPIconType.NONE )
                        {
                            Sprites[ x, y ].sprite = Map.I.Farm.BluePrintCustomSprite[ (int) sd.BPCustomIconType1 ].sprite;
                            Sprites[ x, y ].UpdateVisuals(); // ADICIONE ESTA LINHA
                        }
                        else
                        SetSpriteEmpty( x, y, label );                                                                         // Sprite empty

                    if( bp.ItemMatrix[ x, y ] == ItemType.Tl_Blueprint_Icon_2 )                                                // Custom type 2, like forest or buildings
                    if( sd.BPCustomIconType2 != EBPIconType.NONE )
                        {
                            Sprites[ x, y ].sprite = Map.I.Farm.BluePrintCustomSprite[ (int) sd.BPCustomIconType2 ].sprite;
                            Sprites[ x, y ].UpdateVisuals(); // ADICIONE ESTA LINHA
                        }
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
            AutoApplyItem.spriteId = G.GIT( bp.AutoApplyCostItem ).NSprite.TkSpriteId;
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
                BuyBPItem.spriteId = G.GIT( bp.BuyBPCostItem ).NSprite.TkSpriteId;
                bool res = bp.BuyPlants( true );
                if( res ) BuyBPLabel.color = Color.white;
                else BuyBPLabel.color = Color.red;
            }
            else
            BuyPlantsButton.gameObject.SetActive( false );
    }

    void SetSpriteEmpty( int x, int y, TextMeshPro label )
    {
        //Sprites[ x, y ].depth = 4;
        label.text = "";
        //BackSprites[ x, y ].spriteName = Map.I.Farm.BluePrintItemBackSprite.spriteName;
        //BackSprites[ x, y ].color = Map.I.Farm.BluePrintItemBackSprite.color;
        //Buttons[ x, y ].enabled = false;
        //Buttons[ x, y ].hoverSprite = BackSprites[ x, y ].spriteName;
        //Buttons[ x, y ].pressedSprite = BackSprites[ x, y ].spriteName;
        //Buttons[ x, y ].hover = Color.black;
        //ggggBackSprites[ x, y ].depth = 3;
        //BackSprites[ x, y ].width = Map.I.Farm.BluePrintItemBackSprite.width;
        //BackSprites[ x, y ].height = Map.I.Farm.BluePrintItemBackSprite.height;

        BackSprites[ x, y ].color = new Color32( 51, 89, 14, 246 );
        Sprites[ x, y ].sprite = null;
        Sprites[ x, y ].spriteId = -1;       
        Sprites[ x, y ].TkSpriteId = -1;
        if( Sprites[ x, y ].Image )
            Sprites[ x, y ].Image.sprite = null;
        Sprites[ x, y ].UpdateVisuals();     
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
