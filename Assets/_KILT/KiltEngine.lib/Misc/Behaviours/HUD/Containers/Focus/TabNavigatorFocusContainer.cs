using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TabNavigatorFocusContainer : FocusContainer 
{
	#region Unity Functions

	protected virtual void Update()
	{
		CheckInput();
	}

	#endregion

	#region Helper Functions

	public virtual void CheckInput(bool p_force = false)
	{
		if ((p_force || Input.GetKeyDown(KeyCode.Tab)) && FocusContainer.IsUnderFocus(this.gameObject))
		{
			StartCoroutine(ChangeInput());
		}
	}

	bool _checking = false;
	protected virtual IEnumerator ChangeInput()
	{
		if(!_checking)
		{
			_checking = true;
			EventSystem v_eventSystem = EventSystem.current;
			if(v_eventSystem != null)
			{
				bool v_moveBack = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
				GameObject v_currentSelectedGameObject = v_eventSystem.currentSelectedGameObject;
				Selectable v_currentSelectedComponent = v_currentSelectedGameObject != null? v_currentSelectedGameObject.GetComponent<Selectable>() : null;

				if(!(v_currentSelectedComponent is InputField) || !(((InputField)v_currentSelectedComponent).multiLine))
				{
					List<Selectable> v_currentSelectables = new List<Selectable>();
					for(int i=0; i<Selectable.allSelectables.Count; i++)
					{
						Selectable v_selectableComponent = Selectable.allSelectables[i];
						v_currentSelectables.RemoveChecking(v_selectableComponent);
						if(v_selectableComponent != null && v_selectableComponent.navigation.mode != Navigation.Mode.None  && FocusContainer.IsUnderFocus(v_selectableComponent.gameObject))
						{
							//Sort with SelectionUpDown Index
							int v_indexDown = v_currentSelectables.IndexOf(v_selectableComponent.navigation.selectOnDown);
							int v_indexUp = v_currentSelectables.IndexOf(v_selectableComponent.navigation.selectOnUp);
							bool v_insertingComplete = false;
							int v_indexToInsert = v_indexDown >=0 && v_indexDown < v_currentSelectables.Count? v_indexDown : (v_indexUp >=0 && v_indexUp < v_currentSelectables.Count? v_indexUp + 1 : -1);
							if(v_indexToInsert >=0 && v_indexToInsert < v_currentSelectables.Count+1)
							{
								try
								{
									v_currentSelectables.Insert(v_indexToInsert, v_selectableComponent);
									v_insertingComplete = true;
								}
								catch{}
							}
							if(!v_insertingComplete)
							{
								v_insertingComplete = true;
								v_currentSelectables.Add(v_selectableComponent);
							}
						}
					}

					if(v_currentSelectedComponent != null)
					{
						int v_index = v_currentSelectables.IndexOf(v_currentSelectedComponent);
						v_index = v_moveBack? v_index-1 : v_index+1;
						v_currentSelectedComponent = v_index >= 0 && v_index < v_currentSelectables.Count? v_currentSelectables[v_index] : null;
						v_currentSelectedGameObject = v_currentSelectedComponent != null? v_currentSelectedComponent.gameObject : null;
					}
					if(v_currentSelectedComponent == null)
					{
						v_currentSelectedComponent = v_moveBack? v_currentSelectables.GetLast() : v_currentSelectables.GetFirst();
						v_currentSelectedGameObject = v_currentSelectedComponent != null? v_currentSelectedComponent.gameObject : null;
					}

					v_eventSystem.SetSelectedGameObject(v_currentSelectedGameObject);
					if (v_currentSelectedComponent != null)
					{
						InputField v_inputfield = v_currentSelectedComponent.GetComponent<InputField>();
						if (v_inputfield != null)
							v_inputfield.OnPointerClick(new PointerEventData(v_eventSystem));
						yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(0.05f));
					}
				}
			}
			_checking = false;
		}
	}

	#endregion
}
