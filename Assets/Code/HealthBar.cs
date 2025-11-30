using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour {
	
	public Rect Area;
	public bool  bDrawSecondColor;
	public string Name;
	public GameObject Obj, Back;
	public tk2dSprite Spr, BackSpr;
	public float Percent;
	public Vector3 OrigPos;

	public void Reset()
	{
		Rect Area = new Rect();
		bDrawSecondColor = true;
	    string Name = "";
		GameObject Obj = null;
		Back = null;
		Spr =  BackSpr = null;
		Percent = 100;
		Vector3 OrigPos = new Vector3( 0, 0, 0 );
	}
	
	public void Create( Vector3 pos, string name )
	{
		Name = name;
		Percent = 100;
		
		Back = Manager.I.CreateObjInstance ( "Bar", name + " Back", EDirection.NONE, pos );
		BackSpr = Back.GetComponent<tk2dSprite>();
		BackSpr.color = new Color( 1, 0, 0 );
		
		Obj = Manager.I.CreateObjInstance ( "Bar", name, EDirection.NONE, new Vector3( pos.x, pos.y,pos.z - 1 ) );	
		Spr = Obj.GetComponent<tk2dSprite>();
		Spr.color = new Color( 0, 1, 0 );
	}
	
	public void SetColor( Color col, Color bcol )
	{
		BackSpr.color = bcol;
		Spr.color = col;
	}
	
	public void UpdateIt( float percent )
	{
		if( Percent == percent ) return;
		if (Obj == null ) return;
		
		if( percent > 100 ) percent = 100;
		if( percent < 0   ) percent = 0;
		
		float dif = Percent - percent;
		float val = - ( dif * 0.01f ); 
		float tr = Util.Percent( dif,  3.2f);
		val = Util.Percent( percent, 1);

		tr  *= Back.transform.localScale.x;
		val *= Back.transform.localScale.x;
		Obj.transform.localScale = new Vector3( val, 1, 0 );	
		Percent = percent;
	}

	public void Destroy()
	{
		Destroy ( Back );
		Destroy ( BackSpr );
		Destroy ( Obj );
		Destroy ( Spr );
	} 
	
	public void Disable()
	{
		Obj.GetComponent<Renderer>().enabled  = false;
		Back.GetComponent<Renderer>().enabled = false;
	}
	
	public void Enable()
	{
		Obj.GetComponent<Renderer>().enabled  = true;
		Back.GetComponent<Renderer>().enabled = true;
	}
	
	public void MoveTo( Vector3 cord )
	{
		Obj.transform.position  = cord;		
		Back.transform.position = new Vector3( cord.x, cord.y, Obj.transform.position.z + 1 );	
	}
	
	public void Draw(float percent, bool bInverseColor)
	{
		/*
		Texture2D MyTexture = Resources.Load("WhiteBar") as Texture2D;
		GUI.color = new Color(1.0f , 0, 0);//Set color to red
		GUI.DrawTexture(new Rect(0, 0, 222, 222), MyTexture);
		GUI.color = Color.white;//Reset color to white
		
		
		DrawQuad (new Rect (0, 0, 200, 200), new Color ());
		if(percent < 0   ) percent =   0;
		if(percent > 100 ) percent = 100;
		Rect area1 = Area;
		float width = (float) (Area.right - Area.left);
		float mid   = (percent * width) / 100; 
		area1.right = (int)(area1.left + mid);		
		Area.right = area1.right;*/
	}
	
	public void DrawVert(int percent, bool bInverseColor)
	{
		/*	if(percent < 0   ) percent =   0;
		if(percent > 100 ) percent = 100;

		Rect area1 = Area;
		float height = Area.bottom - Area.top;
		int mid   = ((100 - percent) * height) / 100; 
		area1.top = area1.top + mid;
		
		Area.top = area1.top;*/
	}
}