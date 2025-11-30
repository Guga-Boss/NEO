using UnityEngine;
using System.Collections;

public class AttackPanel : MonoBehaviour {

	public tk2dTextMesh TitleTxt, DamageText, RangeText;

	public void UpdateIt ( Attack at, bool apply )
    {
		//if( at == null || apply == false ) { gameObject.SetActive( false ); return; }
		//gameObject.SetActive( true );

		//if( at.DamageType == EDamageType.MELEE   ) TitleTxt.text = "Melee Att";
		//if( at.DamageType == EDamageType.MISSILE ) TitleTxt.text = "Missile Att";
		//if( at.DamageType == EDamageType.MAGIC   ) TitleTxt.text = "Magic Att";

		//DamageText.text =  "Damage:  " + at.TotalDamage.ToString("0.0");
		//float bn = Util.Percent( HeroData.I.DamageSurplusBonus[ ( int ) at.Unit.Body.DamageSurplusLevel ], at.DamageSurplus );
		//if( bn > 0 ) DamageText.text += " + " + bn.ToString("0.0");
		//if( at.DamageType == EDamageType.MISSILE )
		//{
		//	RangeText.gameObject.SetActive( true );
		//	RangeText.text = "Range: " + at.Range;
		//}
		//else RangeText.gameObject.SetActive( false );
	}
}
