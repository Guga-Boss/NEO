using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnDataCollection
{
    public List<TurnData> TurnData;
}

public class TurnData 
{
	public EActionType Action;
	public Vector2 HeroPos;
	public EDirection HeroDir;
	public float ActionTime;
    public bool HeroDie;
    public int SaveCount;

	void Reset()
	{
		Action = EActionType.NONE;
		HeroPos = new Vector2( -1, -1 );
		HeroDir = EDirection.NONE;
		ActionTime = 0;
        HeroDie = false;
        SaveCount = 0;
	}

    public void Copy( TurnData td )
    {
        Action = td.Action;
        HeroPos = td.HeroPos;
        HeroDir = td.HeroDir;
        ActionTime = td.ActionTime;
        HeroDie = td.HeroDie;
        SaveCount = td.SaveCount;
    }
}
