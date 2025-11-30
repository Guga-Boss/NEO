using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class AutomaticDefinesDetector
{

	#region Static Variables

	const string AUTOMATIC_DEFINE_SAVE = "AutomaticDefineDetectorSave.txt";

	static AOTDictionaryKV<string,string> s_defineByFolderName = new AOTDictionaryKV<string, string>();

	#endregion

	#region Static Properties

	public static AOTDictionaryKV<string,string> DefineByFolderName
	{
		get
		{
			Load();
			return s_defineByFolderName;
		}
		set
		{
			if(s_defineByFolderName == value)
				return;
			s_defineByFolderName = value;
			Save();
		}
	}

	#endregion

	#region Helper Functions

	public static void RemoveDefineChecking(string p_defineName, bool p_removeFromGlobalDefineToo = true, bool p_forceSave = false)
	{
		DefineByFolderName.Remove(p_defineName);
		if(p_removeFromGlobalDefineToo)
			GlobalDefinesManager.RemoveDefineChecking(p_defineName);
		if(p_forceSave)
			Save();
	}
	
	public static void AddDefineChecking(string p_defineName, string p_fileNeeded, bool p_forceSave = false)
	{
		AOTKeyValuePair<string, string> v_founded = null;
		foreach(AOTKeyValuePair<string,string> v_pair in DefineByFolderName)
		{
			if(v_pair != null && !string.IsNullOrEmpty(v_pair.Value) && v_pair.Value.Equals(p_defineName))
			{
				v_founded = v_pair;
				break;
			}
		}
		bool v_needAdd = false;
		if(v_founded == null)
		{
			v_needAdd = true;
			v_founded = new AOTKeyValuePair<string, string>();
		}
		v_founded.Value = p_defineName;
		v_founded.Key = p_fileNeeded;

		if(v_needAdd)
			DefineByFolderName.Add(v_founded);
		if(p_forceSave)
			Save();
	}

	public static void Load(bool p_force = false)
	{
		if(p_force || s_defineByFolderName == null)
		{
			string v_currentPath = KiltUtils.SearchFile(AUTOMATIC_DEFINE_SAVE);
			if(string.IsNullOrEmpty(v_currentPath))
				s_defineByFolderName = SerializerHelper.Deserialize<AOTDictionaryKV<string,string>>("Editor", AUTOMATIC_DEFINE_SAVE, true, SerializationTypeEnum.JSON);
			else
				s_defineByFolderName = SerializerHelper.Deserialize<AOTDictionaryKV<string,string>>(v_currentPath, SerializationTypeEnum.JSON);
			
			if(s_defineByFolderName == null)
				s_defineByFolderName = new AOTDictionaryKV<string,string>();
		}
	}
	
	public static void Save(bool p_checkDefines = false)
	{
		Load();
		string v_currentPath = KiltUtils.SearchFile(AUTOMATIC_DEFINE_SAVE);
		if(string.IsNullOrEmpty(v_currentPath))
			SerializerHelper.Serialize(s_defineByFolderName, "Editor", AUTOMATIC_DEFINE_SAVE, true, SerializationTypeEnum.JSON);
		else
			SerializerHelper.Serialize(s_defineByFolderName, v_currentPath, SerializationTypeEnum.JSON);
		AssetDatabase.Refresh();
		if(p_checkDefines)
			CheckDefines();
		GlobalDefinesManager.Save();
	}

	public static void CheckDefines()
	{
		foreach(AOTKeyValuePair<string,string> v_pair in DefineByFolderName)
		{
			if(v_pair != null)
			{
				if(KiltUtils.FileExists(v_pair.Key))
					GlobalDefinesManager.AddDefineChecking(v_pair.Value);
				else
					GlobalDefinesManager.RemoveDefineChecking(v_pair.Value);
			}
		}
	}

	#endregion

	#region Unity Editor Callbacks

	[UnityEditor.Callbacks.PostProcessBuild]
	private static void PostProcessBuildFunction(UnityEditor.BuildTarget p_target, string p_path)
	{
		CheckDefines();
	}
	
	[UnityEditor.Callbacks.DidReloadScripts]
	private static void PostProcessReloadScriptsFunctions()
	{
		CheckDefines();
	}

	#endregion
}
