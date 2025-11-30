using UnityEngine;
using System.Collections;

public class SceneChangerWidget : MonoBehaviour 
{
	#if NGUI_DLL

	#region Private Variables
	
	[SerializeField]
	GameObject m_buttonObject = null;
	[SerializeField]
	GameObject m_labelObject = null;
	[SerializeField]
	GameObject m_lockedObject = null;
	[SerializeField]
	GameObject m_unlockedObject = null;
	[SerializeField]
	bool m_isLocked = true;
	[SerializeField]
	int m_maxAmountOfStars = 0;

	int m_currentAmountOfStars = 0;
	
	#endregion
	
	#region Public Properties

	public GameObject ButtonObject{get {return m_buttonObject;} set {m_buttonObject = value;}}
	public GameObject LabelObject{get {return m_labelObject;} set {m_labelObject = value;}}
	public GameObject LockedObject{get {return m_lockedObject;} set {m_lockedObject = value;}}
	public GameObject UnlockedObject{get {return m_unlockedObject;} set {m_lockedObject = value;}}

	public bool IsLocked 
	{
		get 
		{
			return m_isLocked;
		} 
		set 
		{
			if (m_isLocked == value) 
				return; 
			m_isLocked = value;
			ShowStars();
		}
	}

	public int CurrentAmountOfStars{get {return m_currentAmountOfStars;} set {m_currentAmountOfStars = value > MaxAmountOfStars? MaxAmountOfStars : value;}}
	public int MaxAmountOfStars{get {return m_maxAmountOfStars;} 
		set 
		{
			m_maxAmountOfStars = value;
		}
	}

	#endregion

	#region Unity Functions

	protected virtual void Awake()
	{
		ShowStars();
	}

	#endregion

	#region Helper Functions

	public virtual void ShowStars()
	{
		if(LabelObject != null)
			LabelObject.SetActive(true);
		if(!IsLocked)
		{
			if(UnlockedObject != null)
				UnlockedObject.SetActive(true);
			if(LockedObject != null)
				LockedObject.SetActive(false);
		}
		else
		{
			if(UnlockedObject != null)
				UnlockedObject.SetActive(false);
			if(LockedObject != null)
				LockedObject.SetActive(true);
		}
	}

	#endregion

	#region Gets and Sets

	public virtual UILabel GetLabel()
	{
		if(m_labelObject != null)
		{
			UILabel v_label = m_labelObject.GetComponent<UILabel>();
			return v_label;
		}
		return null;
	}
	
	public virtual string GetLabelText()
	{
		UILabel v_label = GetLabel();
		if(v_label != null)
		{
			return v_label.text;
		}
		return "";
	}
	
	public virtual void SetLabelText(string p_text)
	{
		UILabel v_label = GetLabel();
		if(v_label != null)
		{
			v_label.text = p_text;
		}
	}

	public virtual string GetLevelToLoadOnClick()
	{
		if(m_buttonObject != null)
		{
			ChangeSceneInteraction v_button = m_buttonObject.GetComponent<ChangeSceneInteraction>();
			if(v_button != null)
				return v_button.StageToLoadOnClick;
			#if KILT_SHOPHELPER
			ChangeSceneWithItemInteraction v_buttonWithItem = m_buttonObject.GetComponent<ChangeSceneWithItemInteraction>();
			if(v_buttonWithItem != null)
				return v_buttonWithItem.StageToLoadOnClick;
			#endif
		}
		return "";
	}

	public virtual void SetLevelToLoadOnClick(string p_levelName)
	{
		if(m_buttonObject != null)
		{
			ChangeSceneInteraction v_button = m_buttonObject.GetComponent<ChangeSceneInteraction>();
			if(v_button != null)
				v_button.StageToLoadOnClick = p_levelName;
			#if KILT_SHOPHELPER
			ChangeSceneWithItemInteraction v_buttonWithItem = m_buttonObject.GetComponent<ChangeSceneWithItemInteraction>();
			if(v_buttonWithItem != null)
				v_buttonWithItem.StageToLoadOnClick = p_levelName;
			#endif
		}
	}

	#endregion

	#endif
}
