using UnityEngine;
using System.Collections;

public class BlockWood : BlockMaterial {

	#region Unity Functions

	protected override void Awake()
	{
		base.Awake();
		AmountOfTouchsWhenClicked = 1;
		Defense = 0.7f;
		Attack = 1f;
	}

	#endregion
}
