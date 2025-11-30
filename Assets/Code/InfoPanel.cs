using UnityEngine;
using System.Collections;

public class InfoPanel : MonoBehaviour {

	public tk2dTextMesh TitleTxt, LevelTxt, UpgradeCostTxt, HPText, AttackTxt, RangeTxt;
	public tk2dSprite Image;
	//public UnitInfo UnitData;
	public tk2dSprite[] PerkIconSprite, PerkIconBackSprite;
	public Vector2 Selection;

	public void Start()
	{
		Selection = new Vector2( -1, -1 );
		gameObject.SetActive( false );
	}

	public void UpdateIt () 
	{
//		gameObject.SetActive( false );
//		if( Selection.x == -1 ) return;
//
//	  bool res = UpdateUnitData ( art );
//	  if( res == false ) return;
//
//	  float lev = UnitData.Level;
//	  UpgradeCostTxt.text = "";
//	  HPText.text = "HP: " + UnitData.Hp.ToString("0") + " / " + UnitData.TotHp.ToString("0");
//
//		for( int i = 0; i < PerkIconSprite.Length; i++ )
//		{
//			PerkIconSprite[i].gameObject.SetActive( false );
//			PerkIconBackSprite[i].gameObject.SetActive( false );
//		}
//
//      if( art != null )
//		{
//			lev = art.Level;
//			UpgradeCostTxt.text = "Lvl UP: " + art.GetUpgradeCost( 1 ).ToString("0");
//			HPText.text = "HP: " + UnitData.TotHp.ToString("0");
//		}
//
//	  gameObject.SetActive( true );
//	  TitleTxt.text = UnitData.Name;
//	  Image.SetSprite( Manager.I.GetUnitSpriteID( UnitData.Type, UnitData.Ply ) );
//	  LevelTxt.text = "Level: " + lev;
//	  AttackTxt.text = "Attack: " + UnitData.Attack.ToString("0") + " + " + UnitData.AttackBonus.ToString("0");
//	  RangeTxt.text = "Range: " + UnitData.Range.ToString("0");
//
//		for( int i = 0; i < UnitData.PerkList.Count; i++ )
//		if ( UnitData.PerkList[i].Type != EPerkType.NONE )
//		{ 
//			PerkIconSprite[i].gameObject.SetActive( true );
//			PerkIconBackSprite[i].gameObject.SetActive( true );
//			PerkIconSprite[i].spriteId = UnitData.PerkList[i].IconSprite.spriteId;
//			PerkIconBackSprite[i].spriteId = UnitData.PerkList[i].IconBackSprite.spriteId;
//		   }				 
	}
	public bool UpdateUnitData () 
	{
//		UnitInfo ui = new UnitInfo();
//		if( art != null )   // Artifact info
//		   {
//			UnitData.Copy ( Manager.I.UnitDefaultData[ ( int ) art.UnitType ]);
//			UnitData.CopyArtifactPerkToUnit( art );
//		    UnitData.Level = art.Level;
//		    Player pl = Manager.I.GlobalP1;
//			if( art.Eply == EPlayer.P2 ) pl = Manager.I.EnemyPlayer;
//		    UnitData.Ply = art.Eply;
//			UnitData.SetUnitLevelData( false, pl );
//		    }
//		else       // Map unit Info
//		    {
//			if( Selection.x != -1 )
//				if( LevelMap.I.CurMap.UInfo[ (int)Selection.x, (int)Selection.y ] != null )
//					UnitData.Copy( LevelMap.I.CurMap.UInfo[ (int)Selection.x, (int)Selection.y ] );
//			else return false;
//		    }
		return true;
	}

	public void Select( Vector2 _sel )
	{
		if ( Selection.x != -1 )
		{
		     if ( Selection != _sel ) Selection = _sel; 
	         else Selection = new Vector2( -1, -1 );
		     return;
		}

	  if ( Selection.x == -1 ) Selection = _sel;
	  else Selection = new Vector2( -1, -1 );
	}
}
