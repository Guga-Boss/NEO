using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(BlockSelector))]
public class BlockSelectorInspector : Editor {

	#region Private Variables

	BlockSelector m_component;
	bool m_firstCorrection = false;

	#endregion

	#region Unity Functions

	public override void OnInspectorGUI ()
	{
		m_component = target as BlockSelector;
		if(m_component != null)
		{
			m_component.ApplyShowHideComponent();
			m_component.FillObjectsInFolderList();
			if(!m_firstCorrection)
			{
				m_firstCorrection = true;
				m_component.CorrectSelectedPrefabIndex();
			}
			bool v_needRecreate = false;
			int v_folderIndex =  -1;

			EditorGUILayout.BeginHorizontal();

			bool v_buttonRefreshClicked = GUILayout.Button("Refresh");
			if(v_buttonRefreshClicked)
				m_component.FillObjectsInFolderList(true);
			bool v_buttonRecreateClicked = GUILayout.Button("Force Recreate");
			bool v_buttonResetLife = GUILayout.Button("Reset Block Life");
			if(v_buttonResetLife || v_buttonRecreateClicked)
				v_needRecreate = true;

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();
			
			bool  v_buttonFlipXClicked = GUILayout.Button("Flip X");
			bool  v_buttonFlipYClicked = GUILayout.Button("Flip Y");
			
			if(v_buttonFlipXClicked || v_buttonFlipYClicked)
				m_component.PerformFlip(v_buttonFlipXClicked, v_buttonFlipYClicked);
			
			EditorGUILayout.EndHorizontal();

			string [] v_stringFolderPath = m_component.FoldersPath.GetStringList().ToArray();
			v_folderIndex = EditorGUILayout.Popup("Folder", m_component.SelectedFolderIndex, v_stringFolderPath);
			if(v_folderIndex != m_component.SelectedFolderIndex)
			{
				v_needRecreate = true;
				m_component.SelectedFolderIndex = v_folderIndex;
			}

			FolderPrefabs v_selectedFolder = m_component.SelectedFolderPrefab;
			if(v_selectedFolder != null)
			{
				string [] v_stringPrefabsInFolder = GetListOfNames(v_selectedFolder.PrefabsInFolder).ToArray();
				int v_prefabIndex = EditorGUILayout.Popup("Prefab", m_component.SelectedPrefabInFolderIndex, v_stringPrefabsInFolder);
				if(v_prefabIndex != m_component.SelectedPrefabInFolderIndex)
				{
					v_needRecreate = true;
					m_component.SelectedPrefabInFolderIndex = v_prefabIndex;
				}
			}

			EditorGUILayout.Separator();

			bool v_hideOtherComponents = EditorGUILayout.Toggle("Hide Other Components", m_component.HideComponentsInInspector);
			if(m_component.HideComponentsInInspector != v_hideOtherComponents)
				m_component.HideComponentsInInspector = v_hideOtherComponents;

			if(GUI.changed)
				EditorUtility.SetDirty(m_component);
			if(v_needRecreate && m_component.SelectedObject != null)
			{
				GameObject v_newObject = m_component.InstantiateObjectByPrefab(m_component.SelectedObject,!v_buttonResetLife,true,true);
				Selection.activeObject = v_newObject;
			}
		}
	}

	#endregion

	#region Helper Functions

	private List<string> GetListOfNames(List<GameObject> p_list)
	{
		List<string> v_nameList = new List<string>();
		if(p_list != null)
		{
			foreach(GameObject v_object in p_list)
			{
				string v_name = v_object != null? v_object.name : "";
				v_nameList.Add(v_name);
			}
		}
		return v_nameList;
	}

	#endregion
}

