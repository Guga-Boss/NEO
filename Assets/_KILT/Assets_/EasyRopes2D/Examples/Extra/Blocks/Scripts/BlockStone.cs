using UnityEngine;
using System.Collections;

public class BlockStone : BlockMaterial {

	#region Unity Functions

	protected override void Awake()
	{
		base.Awake();
		AmountOfTouchsWhenClicked = 2;
		Defense = 0.8f;
		Attack = 0.7f;
	}

	#endregion
}
