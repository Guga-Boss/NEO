using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GlobalDefinesWizard : EditorWindow
{
	[MenuItem("KILT/Helper/Others/Edit Global Defines")]
	public static void OpenDefManager()
	{
		EditorWindow.GetWindow<GlobalDefinesWizard>(true, "Global Define Manager", true);
	}
	
	void OnEnable()
	{
		GlobalDefinesManager.Load();
	}
	
	List<string> defs;
	Vector2 scroll = Vector2.zero;
	void OnGUI()
	{
		Color oldColor = GUI.backgroundColor;
		defs = GlobalDefinesManager.Defines;
		
		scroll = GUILayout.BeginScrollView(scroll);

		GUI.backgroundColor = Color.cyan;
		if(GUILayout.Button("Add"))	
			defs.Add("NEW_DEFINE");
		GUI.backgroundColor = oldColor;

		GUILayout.Space(4);

		List<string> v_definesToRemove = new List<string>();
		for(int i = 0; i < defs.Count; i++)
		{
			GUILayout.BeginHorizontal();
			
			defs[i] = EditorGUILayout.TextField(defs[i]);
			
			GUI.backgroundColor = Color.red;
			if(GUILayout.Button("X", GUILayout.MaxWidth(24), GUILayout.MaxHeight(15)))
				v_definesToRemove.AddChecking(defs[i]);
			GUI.backgroundColor = oldColor;
			
			GUILayout.EndHorizontal();
			
		}

		foreach(string v_defineToRemove in v_definesToRemove)
		{
			GlobalDefinesManager.RemoveDefineChecking(v_defineToRemove);
		}
		v_definesToRemove.Clear();
		
		GUILayout.Space(4);

		GUILayout.EndScrollView();
		
		
		GUILayout.BeginHorizontal();
		GUI.backgroundColor = Color.green;
		if(GUILayout.Button("Apply") )
			GlobalDefinesManager.Save();
		GUI.backgroundColor = Color.magenta;
		if(GUILayout.Button("Refresh") )
			GlobalDefinesManager.Load();
		
		GUILayout.EndHorizontal();
		GUI.backgroundColor = oldColor;
	}
}
