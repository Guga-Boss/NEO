using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GlobalDefinesManager
{
	#region Static Properties

	static List<string> _removeList = new List<string>();

	static List<string> defines = null;
	public static List<string> Defines
	{
		get
		{
			if(defines == null)
				Load();
			return defines;
		}
		set
		{
			if(defines == value)
				return;
			defines = value;
		}
	}

	#endregion

	#region Static Functions

	public static void RemoveDefineChecking(string p_defineName)
	{
		_removeList.AddChecking(p_defineName);
		Defines.RemoveChecking(p_defineName);
	}
	
	public static void AddDefineChecking(string p_defineName)
	{
		#if UNITY_EDITOR

		List<string> v_stringList = Defines;

		if(string.IsNullOrEmpty(p_defineName))
			return;
		
		bool v_canAdd = true;
		foreach(string v_define in v_stringList)
		{
			if(p_defineName.Equals(v_define))
			{
				v_canAdd = false;
				break;
			}
		}
		if(v_canAdd)
			v_stringList.Add(p_defineName);

		#endif
	}

	public static void Load()
	{
		defines = ParseDefines();
		_removeList = new List<string>();
		AddDefinesInRemoveList();
	}

	public static void Save()
	{
		#if UNITY_EDITOR
		WriteInAllPrefs(Defines);
		AddDefinesInRemoveList();
		#endif
	}

	static void AddDefinesInRemoveList()
	{
		if(defines != null)
		{
			foreach(string v_define in defines)
			{
				_removeList.AddChecking(v_define);
			}
		}
	}

	static List<string> ParseDefines()
	{
		List<string> v_list = new List<string>();
		#if UNITY_EDITOR
		string v_defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
		v_list = new List<string>(v_defineSymbols.Split(';'));
		#endif
		return v_list;
	}

	static List<string> ParseDefines(string p_defines)
	{
		List<string> v_list = new List<string>();
		#if UNITY_EDITOR
		string v_defineSymbols = p_defines;
		v_list = new List<string>(v_defineSymbols.Split(';'));
		#endif
		return v_list;
	}

	#if UNITY_EDITOR
	
	static void WriteInAllPrefs(List<string> defs)
	{
		WriteInPrefs(defs, BuildTargetGroup.Android, false);
		WriteInPrefs(defs, BuildTargetGroup.iOS, false);
		WriteInPrefs(defs, BuildTargetGroup.WSA, false);
		WriteInPrefs(defs, BuildTargetGroup.PS3, false);
		WriteInPrefs(defs, BuildTargetGroup.PS4, false);
		WriteInPrefs(defs, BuildTargetGroup.PSM, false);
		WriteInPrefs(defs, BuildTargetGroup.PSP2, false);
		WriteInPrefs(defs, BuildTargetGroup.SamsungTV, false);
		WriteInPrefs(defs, BuildTargetGroup.Standalone, false);
		WriteInPrefs(defs, BuildTargetGroup.WebPlayer, false);
        WriteInPrefs(defs, BuildTargetGroup.WebGL, false);
        WriteInPrefs(defs, BuildTargetGroup.XBOX360, false);
		WriteInPrefs(defs, BuildTargetGroup.XboxOne, false);
		_removeList.Clear();
	}

	static void WriteInPrefs(List<string> defs, BuildTargetGroup p_target, bool p_forceRecompile = true)
	{

		string v_defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(p_target);
		List<string> v_currentDefines = ParseDefines(v_defineSymbols);
		v_defineSymbols = "";
		foreach(string v_currentDefine in v_currentDefines)
		{
			if(!_removeList.Contains(v_currentDefine))
			{
				if(!string.IsNullOrEmpty(v_defineSymbols))
					v_defineSymbols += ";";
				v_defineSymbols += v_currentDefine;
			}
		}
		
		for(int i = 0; i < defs.Count; i++)
		{
			if(!string.IsNullOrEmpty(defs[i]) && !v_defineSymbols.Contains(defs[i]))
			{
				if(!string.IsNullOrEmpty(v_defineSymbols))
				   v_defineSymbols += ";";
				v_defineSymbols += defs[i];
			}
		}

		PlayerSettings.SetScriptingDefineSymbolsForGroup(p_target, v_defineSymbols);
		if(p_forceRecompile)
		{
			_removeList.Clear();
		}
	}
	#endif
	                                                             
	#endregion
}
