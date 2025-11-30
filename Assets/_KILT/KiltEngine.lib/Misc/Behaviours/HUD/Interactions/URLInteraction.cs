using UnityEngine;
using System.Collections;

public class URLInteraction : InteractionBehaviour 
{
	#region Private Variables

	[SerializeField]
 	string m_url = "http://google.com.br";

	#endregion

	#region Public Properties

	public string Url
	{
		get
		{
			return m_url;
		}
		set
		{
			if(m_url == value)
				return;
			m_url = value;
		}
	}

	#endregion

	#region Helper Functions

	public override void Send ()
	{
		if (string.IsNullOrEmpty(Url)) 
			return;

		Application.OpenURL(Url);
	}

	#endregion
}
