using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class TechTable : SerializedMonoBehaviour
{
    /*      "  Explanation for this class: "
            "  This is a weighted chance table used to calculate random outcomes.\n\n" +
            "- Rows represent different stages (or lists) of chance distributions.\n" +
            "- Columns represent categories or possible outcomes.\n" +
            "- Each cell value is a weight (not required to sum to 100). Higher values mean higher probability.\n\n" +
            "Example: Row 0 defines the base distribution (List1). Row 1 can be an alternative distribution (List2), etc.\n\n" +
            "Arrange Limits:\n" +
            "- Level thresholds that define transitions between chance distributions in the matrix.\n" +
            "- The number of limits must be (number of rows - 1).\n" +
            "- For example, with 4 rows in the matrix, you need 3 limits.\n" +
            "- Each value defines the maximum level where the corresponding row applies.\n\n" +
            "Example: {25, 50, 100} →\n" +
            " • Levels 1–25 use Row 0\n" +
            " • Levels 26–50 interpolate towards Row 1\n" +
            " • Levels 51–100 interpolate towards Row 2\n" +
            " • Levels beyond 100 interpolate towards Row 3\n\n" +
            "Test Points:\n" +
            "- Test levels used to validate interpolation and distribution logic.\n" +
            "- The system will sample the chance matrix at these points to debug and verify that interpolations and level thresholds are working correctly.";*/

    [TabGroup( "Chances" )]
    [TableMatrix]
    [OdinSerialize]
    [LabelText( "Chance Matrix (NxM)" )]
    public float[ , ] chanceMatrix = new float[ 4, 8 ];
    public float Curve = 1;
    public float OverTableFactor = 0;
    public float OverTableCurve = 1;

    [TabGroup( "Chances" )]
    [OdinSerialize]
    [LabelText( "Arrange Limits" )]
    public List<int> arrangeLimits = new List<int>() { 25, 50, 100 };
    public int[] TestPoints = new int[] { 1, 5, 10, 20, 25, 50, 100, 120, 150 };
    public List<ItemType> BonusList = new List<ItemType>();


#if UNITY_EDITOR
    [Button( "Test", ButtonSizes.Large ), GUIColor( 1f, 0.52f, 0.1f )]
    public void EditQuestCallBack()
    {
        GlobalAltar.I = GameObject.Find( "Global Altar" ).GetComponent<GlobalAltar>();
        TechTable tb = GlobalAltar.I.SlotsTable;

#region Altar_Evolution      // use this code to test altar evolution curve
        /*      float totalBonus = 30;                                                                
                int maxCubes = 30;
                float curve = .8f;
                float totalWeight = 0f;
                for( int i = 1; i <= maxCubes; i++ )
                    totalWeight += Util.GetCurveVal( i, maxCubes, 0f, 1f, curve );

                float[] weights = new float[ maxCubes ];
                float sum = 0f;
                // Calcula pesos pela curva
                for( int i = 1; i <= maxCubes; i++ )
                {
                    // Agora, pega o peso do cubo atual e converte em bônus proporcional
                    float currentWeight = Util.GetCurveVal( i, maxCubes, 0f, 1f, curve );
                    float bonus = ( currentWeight / totalWeight ) * totalBonus;
                    sum += bonus;
                    float orig = bonus;
                    bonus = Util.FloatSort( bonus );
                    Debug.Log( "Cube " + i + " Bonus Altar Evolution: " + orig.ToString( "F2" ) + " Float Sort: " + bonus );
                }

                Debug.Log( "Total distributed: " + sum );
                return; */
#endregion

        if( name == "Bonus Quality Tech Table" )
                tb = GlobalAltar.I.BonusQualityTable;
        if( name == "Special Slot Available Tech Table" )
            tb = GlobalAltar.I.SpecialSlotsTable;
        GlobalAltar.I.TestSampleActiveSides( tb );
    }
#endif
}
