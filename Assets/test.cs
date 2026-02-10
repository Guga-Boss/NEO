using UnityEngine;
using Sirenix.OdinInspector;

public class OdinTest: MonoBehaviour
{
    [Button( "Clique Aqui" )]
    void TestButton()
    {
        Debug.Log( "Odin f uncionando!" );
    }
}
