using UnityEngine;
using System.Collections;

public class BlockMaterial : Block {

	#region Private Variables

	[SerializeField]
	bool m_canDestroyOnClick = true;
	//Use to Define how many touchs will be counted when user click the block
	[SerializeField]
	int m_amountOfTouchsWhenClicked = 1;
	
	#endregion

	#region Public Properties

	public bool CanDestroyOnClick
	{
		get {return m_canDestroyOnClick;}
		set {m_canDestroyOnClick = value;}
	}

	public int AmountOfTouchsWhenClicked
	{
		get {return m_amountOfTouchsWhenClicked;}
		set {m_amountOfTouchsWhenClicked = value;}
	}

	#endregion

	#region Event Implementations
	
	public override void OnUserClick()
	{
		if(!PauseManager.IsPaused() && !CameraFade.InstanceExists() && CanDestroyOnClick)
		{
			Destroy();
		}
	}

	#endregion
}
