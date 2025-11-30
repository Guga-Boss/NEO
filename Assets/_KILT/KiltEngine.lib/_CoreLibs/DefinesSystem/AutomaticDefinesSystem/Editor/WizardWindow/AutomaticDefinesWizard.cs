using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class AutomaticDefinesWizard : EditorWindow {

	[MenuItem("KILT/Helper/Others/Edit Automatic Defines")]
	public static void OpenDefManager()
	{
		EditorWindow.GetWindow<AutomaticDefinesWizard>(true, "Automatic Defines Detector", true);
	}
	
	void OnEnable()
	{
		AutomaticDefinesDetector.Load(true);
	}

	Vector2 scroll = Vector2.zero;
	void OnGUI()
	{
		Color oldColor = GUI.backgroundColor;
		
		scroll = GUILayout.BeginScrollView(scroll);

		GUI.backgroundColor = Color.cyan;
		if(GUILayout.Button("Add"))	
			AutomaticDefinesDetector.DefineByFolderName.Add("NEW_AUTOMATIC_DEFINE", "FileName.cs");
		GUI.backgroundColor = oldColor;

		GUILayout.Space(4);

		List<string> v_definesToRemove = new List<string>();
		for(int i = 0; i < AutomaticDefinesDetector.DefineByFolderName.Count; i++)
		{
			GUILayout.BeginHorizontal();
			
			string v_define = EditorGUILayout.TextField("Define Name", AutomaticDefinesDetector.DefineByFolderName[i].Value);
			if(v_define != null && !v_define.Equals(AutomaticDefinesDetector.DefineByFolderName[i].Value))
			{
				v_definesToRemove.AddChecking(AutomaticDefinesDetector.DefineByFolderName[i].Value);
				AutomaticDefinesDetector.DefineByFolderName[i].Value = v_define;
			}
			AutomaticDefinesDetector.DefineByFolderName[i].Key = EditorGUILayout.TextField("File Needed" , AutomaticDefinesDetector.DefineByFolderName[i].Key);
			GUI.backgroundColor = Color.red;
			if(GUILayout.Button("X", GUILayout.MaxWidth(24), GUILayout.MaxHeight(15)))
				v_definesToRemove.AddChecking(AutomaticDefinesDetector.DefineByFolderName[i].Value);
			GUI.backgroundColor = oldColor;
			
			GUILayout.EndHorizontal();
		}

		foreach(string v_defineToRemove in v_definesToRemove)
		{
			AutomaticDefinesDetector.RemoveDefineChecking(v_defineToRemove, true);
		}
		v_definesToRemove.Clear();
		
		GUILayout.Space(4);
		
		GUILayout.EndScrollView();
		
		
		GUILayout.BeginHorizontal();
		GUI.backgroundColor = Color.green;
		if( GUILayout.Button("Apply") )
			AutomaticDefinesDetector.Save(true);
		GUI.backgroundColor = Color.magenta;
		if( GUILayout.Button("Refresh") )
			AutomaticDefinesDetector.Load(true);
		
		GUILayout.EndHorizontal();
		GUI.backgroundColor = oldColor;
	}
}
