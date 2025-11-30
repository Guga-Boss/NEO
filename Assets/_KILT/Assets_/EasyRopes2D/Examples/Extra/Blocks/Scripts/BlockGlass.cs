using UnityEngine;
using System.Collections;

public class BlockGlass : BlockMaterial {

	#region Unity Functions
	
	protected override void Awake()
	{
		base.Awake();
		AmountOfTouchsWhenClicked = 1;
		Defense = 0.6f;
		Attack = 0.7f;
	}
	
	#endregion
}
