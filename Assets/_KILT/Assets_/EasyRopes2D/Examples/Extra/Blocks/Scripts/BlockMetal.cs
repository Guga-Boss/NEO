using UnityEngine;
using System.Collections;

public class BlockMetal : BlockMaterial 
{
	#region Unity Functions

	protected override void Awake()
	{
		AmountOfTouchsWhenClicked = 0;
		base.Awake();
		Defense = 0.05f; //TOUGH DEFENSE
		Attack = 0.8f;
	}

	#endregion

	#region Event Implementations
	
	public override void OnUserClick()
	{
	}
	
	#endregion


}
