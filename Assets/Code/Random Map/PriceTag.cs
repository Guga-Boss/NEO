using UnityEngine;
using System.Collections;

public class PriceTag : MonoBehaviour {

    public tk2dTextMesh Price_1Text;
    public tk2dSprite Price_1Resource;
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
