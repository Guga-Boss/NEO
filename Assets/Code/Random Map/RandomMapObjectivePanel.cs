using UnityEngine;
using System.Collections;

public class RandomMapObjectivePanel : MonoBehaviour 
{
    public UILabel DescriptionLabel, CurrentNumberLabel, Prize1Label, Prize2Label, 
                   UpgradePriceAmountLabel, UpgradeBonusAmountLabel, UpgradeButtonLabel, 
                   UpgradeBonusLabel, RequirementLabel;
    public UI2DSprite Prize1Icon, Prize2Icon, Prize2BackIcon, TrophyIcon, UpgradePriceIcon, 
    UpgradeBonusIcon, RequirementIcon;
    public UISprite CheckBoxSprite, CheckBoxBackSprite;
    public UIButton Button, UpgradeButton;
    public RandomMapGoal Goal;
    
    public void OnUpgradeObjectiveButtonPress()
    {
        Goal.Upgrade( true );
    }

    public void Copy( RandomMapObjectivePanel pan, RandomMapGoal go )
    {
        Prize1Label.text = pan.Prize1Label.text;
        Prize2Label.text = pan.Prize2Label.text;
        Prize1Icon.sprite2D = pan.Prize1Icon.sprite2D;
        Prize2Icon.sprite2D = pan.Prize2Icon.sprite2D;
        Prize2Icon.color = pan.Prize2Icon.color;
        Prize2BackIcon.sprite2D = pan.Prize2BackIcon.sprite2D;
        Prize1Label.gameObject.SetActive( pan.Prize1Label.gameObject.activeSelf );
        Prize2Label.gameObject.SetActive( pan.Prize2Label.gameObject.activeSelf );
        Prize1Icon.gameObject.SetActive( pan.Prize1Icon.gameObject.activeSelf );
        Prize2Icon.gameObject.SetActive( pan.Prize2Icon.gameObject.activeSelf );
        Prize2BackIcon.gameObject.SetActive( pan.Prize2BackIcon.gameObject.activeSelf );

        bool ok1 = true;
        if( go.BonusItem == ItemType.NONE ) ok1 = false;

        bool ok2 = true;
        if( go.BonusItem2 == ItemType.NONE )
        if( go.TargetBluePrint == null )
        if( go.TargetRecipe == null )
            ok2 = false;

        Prize1Label.transform.parent.gameObject.SetActive( ok1 );
        Prize2Label.transform.parent.gameObject.SetActive( ok2 );
    }
}
