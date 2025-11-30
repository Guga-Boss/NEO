using UnityEngine;
using System.Collections;

public class NavigationMapBonus : MonoBehaviour
{
    public tk2dSprite BonusIcon;
    public tk2dTextMesh TextMesh;
    public ItemType BonusItem = ItemType.Stone;
    public float BonusAmount = 1;
    public Vector2 MapCord = new Vector2( -1, -1 );

    public void UpdateMeshCallBack()
    {
        GameObject ob = GameObject.Find( "Farm" );
        Farm f = ob.GetComponent<Farm>();
        f.UpdateListsCallBack();
    }

    public void Copy( NavigationMapBonus bn )
    {
        BonusIcon.spriteId = bn.BonusIcon.spriteId;
        BonusAmount = bn.BonusAmount;
        TextMesh.text = bn.TextMesh.text;
        BonusItem = bn.BonusItem;
        MapCord = bn.MapCord;
    }
}
