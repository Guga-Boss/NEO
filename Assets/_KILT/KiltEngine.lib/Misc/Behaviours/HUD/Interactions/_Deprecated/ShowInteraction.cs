using UnityEngine;
using System.Collections;

[System.Obsolete("Use ShowHideInteraction")]
public class ShowInteraction : InteractionBehaviour {
	
	#region Private Variables

	[SerializeField]
	GameObject m_targetToShow = null;
	[SerializeField]
	GameObject m_targetToHide = null;
	
	#endregion
	
	#region Public Properties
	
	public GameObject TargetToShow
	{ 
		get {return m_targetToShow;} 
		set {m_targetToShow = value;}
	}

	public GameObject TargetToHide
	{ 
		get {return m_targetToHide;} 
		set {m_targetToHide = value;}
	}
	
	#endregion
	
	public override void Send()
	{
		if(m_targetToShow != null)
		{
			KiltUtils.SetContainerVisibility(m_targetToShow, ShowObjectActionEnum.Show);
			/*ScheduledContainer v_panel = m_targetToShow.GetComponent<ScheduledPanel>();
			if(v_panel != null)
				v_panel.Show();
			else
				m_targetToShow.SetActive(true);*/
		}
		if(m_targetToHide != null)
		{
			KiltUtils.SetContainerVisibility(m_targetToHide, ShowObjectActionEnum.Hide);
			/*ScheduledPanel v_panel = m_targetToHide.GetComponent<ScheduledPanel>();
			if(v_panel != null)
				v_panel.Hide();
			else
				m_targetToHide.SetActive(false);*/
		}
	}
}
