using UnityEngine;
using System.Collections;

public class OrientationSetterInteraction : InteractionBehaviour {

	#region Private Variables

	[SerializeField]
	ScreenOrientation m_orientation = ScreenOrientation.AutoRotation;

	#endregion

	#region Public Properties

	public ScreenOrientation Orientation
	{
		get
		{
			return m_orientation;
		}
		set
		{
			if(m_orientation == value)
				return;
			m_orientation = value;
		}
	}

	#endregion

	#region Overriden Functions

	public override void Send()
	{
		try
		{
			//Fix Orientation when coming from JavaSession
			if(Orientation == ScreenOrientation.AutoRotation)
			{
				if(Screen.autorotateToPortrait || Screen.autorotateToPortraitUpsideDown)
					Screen.orientation = ScreenOrientation.Portrait;
				else
					Screen.orientation = ScreenOrientation.Landscape;
			}
			Screen.orientation = Orientation;
		}
		catch{}
	}

	#endregion
}
