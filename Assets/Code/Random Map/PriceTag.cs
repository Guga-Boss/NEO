using UnityEngine;
using System.Collections;
using TMPro;

public class PriceTag : MonoBehaviour {

    public TextMeshPro Price_1Text;
    public NSprite Price_1Resource;
    //public EResourceType CostResource_1;
    public float CostValue_1;
    //public EResourceType CostResource_2;
    public float CostValue_2;

    public void Start()
    {
        //CostResource_1 = EResourceType.None;
        CostValue_1 = 0;
        //CostResource_2 = EResourceType.None;
        CostValue_2 = 0;
    }
}
