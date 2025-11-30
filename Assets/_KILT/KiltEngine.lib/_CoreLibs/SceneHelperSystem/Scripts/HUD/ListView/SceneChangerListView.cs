using UnityEngine;
using System.Collections;

#if NGUI_DLL
public class SceneChangerListView : AutomaticListView 
{
	#region Private Variables
	
	[SerializeField]
	string m_scenePrefixName = "";
	[SerializeField]
	bool m_unlockFirstInList = true;
	[SerializeField]
	bool m_trackOldSceneToUpdateGroup = true;

	#endregion


	#region Public Properties

	public string ScenePrefixName 
	{
		get {return m_scenePrefixName; } 
		set {m_scenePrefixName = value;}
	}

	public bool UnlockFirstInList 
	{
		get {return m_unlockFirstInList; } 
		set {m_unlockFirstInList = value;}
	}

	public bool TrackOldSceneToUpdateGroup 
	{
		get {return m_trackOldSceneToUpdateGroup; } 
		set {m_trackOldSceneToUpdateGroup = value;}
	}

	#endregion

	#region Unity Functions
	
	// Use this for initialization
	protected override void Awake () 
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		UpdateGroupByLastSceneOrByLastUnlocked();
	}

	#endregion

	#region Helper Methods

	protected virtual void UpdateGroupByLastSceneOrByLastUnlocked()
	{
		bool p_updated = false;
		if(TrackOldSceneToUpdateGroup)
		{
			for(int i=0; i<ObjectsInList.Count; i++)
			{
				GameObject v_object = ObjectsInList[i];
				if(v_object != null)
				{
					SceneChangerWidget v_widget = v_object.GetComponent<SceneChangerWidget>();
					if(v_widget != null)
					{
						string v_levelToLoad = v_widget.GetLevelToLoadOnClick();
						if(SceneManager.OldLevelName.Equals(v_levelToLoad))
						{
							SetMovableContainerPositionByGroupToShow(GetGroupByIndex(i), true);
							p_updated = true;
							break;
						}
					}
				}
			}
		}
		if(!p_updated)
		{
			for(int i=ObjectsInList.Count-1; i>=0; i--)
			{
				GameObject v_object = ObjectsInList[i];
				if(v_object != null)
				{
					SceneChangerWidget v_widget = v_object.GetComponent<SceneChangerWidget>();
					if(v_widget != null && !v_widget.IsLocked)
					{
						SetMovableContainerPositionByGroupToShow(GetGroupByIndex(i));
						p_updated = true;
						break;
					}
				}
			}
		}
	}
	
	protected override void AfterWidgetAddedInList(GameObject v_object)
	{
		SceneChangerWidget v_widget = v_object.GetComponent<SceneChangerWidget>();
		if(v_widget != null)
		{
			v_widget.SetLabelText("" + (ObjectsInList.Count));
			v_widget.SetLevelToLoadOnClick(m_scenePrefixName+(ObjectsInList.Count));
			v_object.name = "" + (ObjectsInList.Count);
		}
	}
	
	protected override void AfterAllWidgetsAddedInList()
	{
		//Unlock First Member
		if(UnlockFirstInList && ObjectsInList.Count > 0)
		{
			GameObject v_object = ObjectsInList[0];
			if(v_object != null)
			{
				SceneChangerWidget v_widget = v_object.GetComponent<SceneChangerWidget>();
				if(v_widget != null)
					v_widget.IsLocked = false;
			}
		}
	}

	/*protected void CorrectNameAndSceneToClick()
	{
		for(int i =0; i<ObjectsInList.Count; i++)
		{
			GameObject v_object = ObjectsInList[i];
			if(v_object != null)
			{
				StageWidget v_stageWidget = v_object.GetComponent<StageWidget>();
				if(v_stageWidget != null)
				{
					v_stageWidget.SetLabelText("" + (i+1));
					v_stageWidget.SetLevelToLoadOnClick(m_scenePrefixName+(i+1));
					v_object.name = "" + (i+1);
				}
			}
		}
	}*/

	#endregion
}

#else
public class SceneChangerListView : MonoBehaviour 
{
}
#endif
