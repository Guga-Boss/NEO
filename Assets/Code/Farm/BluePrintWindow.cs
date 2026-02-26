using UnityEngine;
using System.Collections;
using TMPro;

public class BluePrintWindow : MonoBehaviour 
{
    public static BluePrintWindow I;
    public int SelectionID;
    public Blueprint TempBluePrint, TempBluePrint2;
    public GameObject WindowFolder1, WindowFolder2, FarmFolder;
    public BluePrintPanel BPPanel1, BPPanel2, FarmPanel;
    public Vector3 OriginalPosition;
    public TextMeshPro TitleLabel;
    public static bool BothBlueprintsAreNew = false;
        
    public void Start()
    {
        I = this;
    }

    public void OnEnable()
    {
        SetSelection( -1 );
    }

    public static void Init()
    {
        BluePrintWindow.I.FarmPanel.InitSprites( BluePrintWindow.I.FarmFolder    );
        BluePrintWindow.I.BPPanel1.InitSprites ( BluePrintWindow.I.WindowFolder1 );
        BluePrintWindow.I.BPPanel2.InitSprites ( BluePrintWindow.I.WindowFolder2 );
        BluePrintWindow.I.transform.localPosition = BluePrintWindow.I.OriginalPosition;
    }
    
    public static void UpdateIt()
    {
        Blueprint bp = Blueprint.SelectedBluePrint;
        if( BluePrintWindow.I.gameObject.activeSelf )
        {
            UI.I.SetTurnInfoText( "", 1, Color.white );
            G.Farm.OldBlueprintButtonText.text = "Old";
            if( BothBlueprintsAreNew )
            {                                                                                          // booth bp are new
                G.Farm.OldBlueprintButtonText.text = "New";
            }
            BluePrintWindow.I.BPPanel1.UpdateIt( BluePrintWindow.I.TempBluePrint2, bp.name, 1 );
            BluePrintWindow.I.BPPanel2.UpdateIt( BluePrintWindow.I.TempBluePrint,  bp.name, 2 );
        }

        if( Blueprint.AvailableBluePrints > 0 )                                                        // Updates Main BP Panel
        {
            if( Building.BuildingHover == false )            
                BluePrintWindow.I.FarmPanel.gameObject.SetActive( true );
            BluePrintWindow.I.FarmPanel.UpdateIt( bp, bp.name, 0 );                       
        }
        else
            BluePrintWindow.I.FarmPanel.gameObject.SetActive( false );

        BluePrintWindow.I.TitleLabel.text = "Choose " + bp.name + " BluePrint";   
    }

    public void OnOKButton()
    {
        if( SelectionID == -1 ) return;

        if( SelectionID == 1 )
        if( BothBlueprintsAreNew )                                                                                                // Booth new
        {
            Blueprint.SelectedBluePrint.Copy( BluePrintWindow.I.TempBluePrint2 );
        }

        if( SelectionID == 2 )
        {
            Blueprint.SelectedBluePrint.Copy( BluePrintWindow.I.TempBluePrint );
        }

        Map.I.InvalidateInputTimer = .5f;
        gameObject.SetActive( false );

        float chc = FPow.Get( FPowType.New_BP_Refund_Chance, false );                                                             // Firepower: Refund tool
        if( Util.Chance( chc ) )
        {
            Message.GreenMessage( ItemType.Fire_Token, "Blueprint Refunded!" );
            FPow.LastPow.Use( 1 );
        }
        else
        {
            Blueprint.SelectedBluePrint.FreePlants--;                                                                              // Free plant var decrement
            Item.AddItem( Inventory.IType.Inventory, ItemType.BluePrint_Image_1, -1 );
        }
    }

    public void OnChoosePlant()
    {
        StartDialog( Blueprint.SelectedBluePrint );
        G.Farm.NewPlantButtonTxt.transform.parent.gameObject.SetActive( false );
        Map.I.InvalidateInputTimer = .6f;
    }

    public void OnAutoApplyBlueprint()
    {
        Blueprint.SelectedBluePrint.AutoApply( false );
    }

    public void OnBuyBlueprint()
    {
        Blueprint.SelectedBluePrint.BuyPlants( false );
    }

    public static void StartDialog( Blueprint bp )
    {
        BluePrintWindow.I.gameObject.SetActive( true );
        BluePrintWindow.I.TempBluePrint.Copy( bp );
        BluePrintWindow.I.TempBluePrint.Sort( true );
        BluePrintWindow.I.TempBluePrint.SortEverything( true );
        BothBlueprintsAreNew = false;
        BluePrintWindow.I.TempBluePrint2.Copy( bp );
        if( Util.Chance( Blueprint.SelectedBluePrint.SortBoothBlueprintsChance ) )
        {
            BothBlueprintsAreNew = true;
            BluePrintWindow.I.TempBluePrint2.Sort( true );
            BluePrintWindow.I.TempBluePrint2.SortEverything( true );
        }
    }
    public void SetSelection1()
    {
        SetSelection( 1 );
    }

    public void SetSelection2()
    {
        SetSelection( 2 );
    }
    public void SetSelection( int id )
    {
        SelectionID = id;
        if( id == -1 ) return;
        Map.I.InvalidateInputTimer = .5f;
        Manager.I.Tutorial.NewBluePrintChosen = true;
        OnOKButton();
    }
}
