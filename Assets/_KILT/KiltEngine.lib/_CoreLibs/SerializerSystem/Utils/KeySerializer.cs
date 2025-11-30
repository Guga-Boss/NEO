using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class KeySerializer
{
	#region Events
	
	public static event Delegates.EventHandler OnLoad;

	#endregion

	#region Static Properties

	static string m_baseSaveFileName = "baseKeySer.txt";
	static string m_saveFileName = "keySer.sav";
	static KeySaveStruct m_baseSaveStruct = null;
	static KeySaveStruct m_saveStruct = null;

	static KeySaveStruct BaseSaveStruct
	{
		get
		{
			if(m_baseSaveStruct == null)
				LoadFromBase();
			return m_baseSaveStruct;
		}
		set
		{
			if(m_baseSaveStruct == value)
				return;
			m_baseSaveStruct = value;
		}
	}

	static KeySaveStruct SaveStruct
	{
		get
		{
			if(m_saveStruct == null)
				Load();
			return m_saveStruct;
		}
		set
		{
			if(m_saveStruct == value)
				return;
			m_saveStruct = value;
		}
	}

	#endregion

	#region Static Functions

	#region Safe Casting Functions

	private static bool TryCastObject<T>(object p_object, out T p_out)
	{
		p_out = default(T);
		bool v_converted = false;
		if(p_object is T)
		{
			try
			{
				p_out = (T)p_object;
				v_converted = true;
			}
			catch
			{
				p_out = default(T);
				v_converted = false;
			}
		}
		if(!v_converted && p_object is Newtonsoft.Json.Linq.JArray)
		{
			try
			{
				p_out = ((Newtonsoft.Json.Linq.JArray)p_object).ToObject<T>();
				v_converted = true;
			}
			catch
			{
				p_out = default(T);
				v_converted = false;
			}
		}
		if(!v_converted && p_object is Newtonsoft.Json.Linq.JContainer)
		{
			try
			{
				p_out = ((Newtonsoft.Json.Linq.JContainer)p_object).ToObject<T>();
				v_converted = true;
			}
			catch
			{
				p_out = default(T);
				v_converted = false;
			}
		}
		return v_converted;
	}

	#endregion

	#region Save Struct

	#region Get All
	
	public static AOTDictionaryKV<string, float> GetAllFloatKeys()
	{
		return SaveStruct.FloatKeys.CloneDict() as AOTDictionaryKV<string, float>;
	}
	
	public static AOTDictionaryKV<string, int> GetAllIntKeys()
	{
		return SaveStruct.IntKeys.CloneDict() as AOTDictionaryKV<string, int>;
	}
	
	public static AOTDictionaryKV<string, bool> GetAllBoolKeys()
	{
		return SaveStruct.BoolKeys.CloneDict() as AOTDictionaryKV<string, bool>;
	}

	public static AOTDictionaryKV<string,string> GetAllStringKeys()
	{
		return SaveStruct.StringKeys.CloneDict() as AOTDictionaryKV<string,string>;
	}
	
	public static AOTDictionaryKV<string, T> GetAllObjectKeys<T>()
	{
		AOTDictionaryKV<string, T> v_newTypeDict = new AOTDictionaryKV<string, T>();
		foreach(AOTKeyValuePair<string, object> v_pair in SaveStruct.ObjectKeys)
		{
			if(v_pair != null)
			{
				T v_convertedObject = default(T);
				bool v_converted = TryCastObject<T>(v_pair.Value, out v_convertedObject);
				if(v_converted)
					v_newTypeDict.Add(v_pair.Key, v_convertedObject);
			}
		}
		return v_newTypeDict;
	}

	#endregion

	#region Get/Set

	public static void SetFloat(string p_key, float p_value)
	{
		SaveStruct.FloatKeys.AddReplacing(p_key, p_value);
		CallSave();
	}

	public static float GetFloat(string p_key)
	{
		float v_founded = SaveStruct.FloatKeys.GetValueByKey(p_key);
		return v_founded;
	}

	public static void SetInt(string p_key, int p_value)
	{
		SaveStruct.IntKeys.AddReplacing(p_key, p_value);
		CallSave();
	}
	
	public static int GetInt(string p_key)
	{
		int v_founded = SaveStruct.IntKeys.GetValueByKey(p_key);
		return v_founded;
	}

	public static void SetBool(string p_key, bool p_value)
	{
		SaveStruct.BoolKeys.AddReplacing(p_key, p_value);
		CallSave();
	}
	
	public static bool GetBool(string p_key)
	{
		bool v_founded = SaveStruct.BoolKeys.GetValueByKey(p_key);
		return v_founded;
	}

	public static void SetString(string p_key, string p_value)
	{
		SaveStruct.StringKeys.AddReplacing(p_key, p_value);
		CallSave();
	}
	
	public static string GetString(string p_key)
	{
		string v_founded = SaveStruct.StringKeys.GetValueByKey(p_key);
		return v_founded;
	}

	public static void SetObject<T>(string p_key, T p_value)
	{
		SaveStruct.ObjectKeys.AddReplacing(p_key, p_value);
		CallSave();
	}
	
	public static T GetObject<T>(string p_key)
	{
		object v_founded = SaveStruct.ObjectKeys.GetValueByKey(p_key);
		T v_returnObject = default(T);
		TryCastObject<T>(v_founded, out v_returnObject);
		
		return v_returnObject;
	}

	#endregion

	#region Contains

	public static bool ContainsFloatKey(string p_key)
	{
		return SaveStruct.FloatKeys.ContainsKey(p_key);
	}

	public static bool ContainsIntKey(string p_key)
	{
		return SaveStruct.IntKeys.ContainsKey(p_key);
	}

	public static bool ContainsBoolKey(string p_key)
	{
		return SaveStruct.BoolKeys.ContainsKey(p_key);
	}

	public static bool ContainsStringKey(string p_key)
	{
		return SaveStruct.StringKeys.ContainsKey(p_key);
	}

	public static bool ContainsObjectKey(string p_key)
	{
		return SaveStruct.ObjectKeys.ContainsKey(p_key);
	}

	#endregion

	#region Remove

	public static bool RemoveFloatKey(string p_key)
	{
		return SaveStruct.FloatKeys.RemoveByKey(p_key);
	}

	public static bool RemoveIntKey(string p_key)
	{
		return SaveStruct.IntKeys.RemoveByKey(p_key);
	}

	public static bool RemoveBoolKey(string p_key)
	{
		return SaveStruct.BoolKeys.RemoveByKey(p_key);
	}

	public static bool RemoveStringKey(string p_key)
	{
		return SaveStruct.StringKeys.RemoveByKey(p_key);
	}

	public static bool RemoveObjectKey(string p_key)
	{
		return SaveStruct.ObjectKeys.RemoveByKey(p_key);
	}
	
	#endregion

	#region Save/Load

	public static void Load()
	{
		LoadFromBase(); // We Must Load From Base First
		KeySaveStruct v_saveStruct = SerializerHelper.Deserialize<KeySaveStruct>("", m_saveFileName, true) as KeySaveStruct;
		KeySaveStruct v_finalStruct = new KeySaveStruct();

		v_finalStruct.MergeReplacing(BaseSaveStruct);
		v_finalStruct.MergeReplacing(v_saveStruct);

		m_saveStruct = v_finalStruct;
		if(OnLoad != null)
			OnLoad();
	}

	public static void Save()
	{
		Save(SerializationTypeEnum.AutoDetect);
	}

	public static void Save(SerializationTypeEnum p_serializationType)
	{
		SerializerHelper.Serialize<KeySaveStruct>(SaveStruct, "", m_saveFileName, true, p_serializationType);
	}

	public static void CallSave()
	{
		if(GlobalScheduler.InstanceExists() && Application.isPlaying)
			GlobalScheduler.CallFunction(GlobalScheduler.Instance.gameObject, new Delegates.FunctionPointer(Save), 0.2f, false);
		else
			Save();
	}

	#endregion

	#endregion

	#region Base Save Struct

	#region Get All
	
	public static AOTDictionaryKV<string, float> GetAllBaseFloatKeys()
	{
		return BaseSaveStruct.FloatKeys.CloneDict() as AOTDictionaryKV<string, float>;
	}
	
	public static AOTDictionaryKV<string, int> GetAllBaseIntKeys()
	{
		return BaseSaveStruct.IntKeys.CloneDict() as AOTDictionaryKV<string, int>;
	}
	
	public static AOTDictionaryKV<string,bool> GetAllBaseBoolKeys()
	{
		return BaseSaveStruct.BoolKeys.CloneDict() as AOTDictionaryKV<string, bool>;
	}
	
	public static AOTDictionaryKV<string,string> GetAllBaseStringKeys()
	{
		return BaseSaveStruct.StringKeys.CloneDict() as AOTDictionaryKV<string,string>;
	}
	
	public static AOTDictionaryKV<string, T> GetAllBaseObjectKeys<T>()
	{
		AOTDictionaryKV<string, T> v_newTypeDict = new AOTDictionaryKV<string, T>();
		foreach(AOTKeyValuePair<string, object> v_pair in BaseSaveStruct.ObjectKeys)
		{
			if(v_pair != null)
			{
				T v_convertedObject = default(T);
				bool v_converted = TryCastObject<T>(v_pair.Value, out v_convertedObject);
				if(v_converted)
					v_newTypeDict.Add(v_pair.Key, v_convertedObject);
			}
		}
		return v_newTypeDict;
	}
	
	#endregion
	
	#region Get/Set
	
	public static void SetBaseFloat(string p_key, float p_value)
	{
		BaseSaveStruct.FloatKeys.AddReplacing(p_key, p_value);
		CallSaveToBase();
	}
	
	public static float GetBaseFloat(string p_key)
	{
		float v_founded = BaseSaveStruct.FloatKeys.GetValueByKey(p_key);
		return v_founded;
	}
	
	public static void SetBaseInt(string p_key, int p_value)
	{
		BaseSaveStruct.IntKeys.AddReplacing(p_key, p_value);
		CallSaveToBase();
	}
	
	public static int GetBaseInt(string p_key)
	{
		int v_founded = BaseSaveStruct.IntKeys.GetValueByKey(p_key);
		return v_founded;
	}
	
	public static void SetBaseBool(string p_key, bool p_value)
	{
		BaseSaveStruct.BoolKeys.AddReplacing(p_key, p_value);
		CallSaveToBase();
	}
	
	public static bool GetBaseBool(string p_key)
	{
		bool v_founded = BaseSaveStruct.BoolKeys.GetValueByKey(p_key);
		return v_founded;
	}
	
	public static void SetBaseString(string p_key, string p_value)
	{
		BaseSaveStruct.StringKeys.AddReplacing(p_key, p_value);
		CallSaveToBase();
	}
	
	public static string GetBaseString(string p_key)
	{
		string v_founded = BaseSaveStruct.StringKeys.GetValueByKey(p_key);
		return v_founded;
	}
	
	public static void SetBaseObject<T>(string p_key, T p_value)
	{
		BaseSaveStruct.ObjectKeys.AddReplacing(p_key, p_value);
		CallSaveToBase();
	}
	
	public static T GetBaseObject<T>(string p_key)
	{
		object v_founded = BaseSaveStruct.ObjectKeys.GetValueByKey(p_key);
		T v_returnObject = default(T);
		TryCastObject<T>(v_founded, out v_returnObject);

		return v_returnObject;
	}
	
	#endregion
	
	#region Contains
	
	public static bool ContainsBaseFloatKey(string p_key)
	{
		return BaseSaveStruct.FloatKeys.ContainsKey(p_key);
	}
	
	public static bool ContainsBaseIntKey(string p_key)
	{
		return BaseSaveStruct.IntKeys.ContainsKey(p_key);
	}
	
	public static bool ContainsBaseBoolKey(string p_key)
	{
		return BaseSaveStruct.BoolKeys.ContainsKey(p_key);
	}
	
	public static bool ContainsBaseStringKey(string p_key)
	{
		return BaseSaveStruct.StringKeys.ContainsKey(p_key);
	}
	
	public static bool ContainsBaseObjectKey(string p_key)
	{
		return BaseSaveStruct.ObjectKeys.ContainsKey(p_key);
	}
	
	#endregion
	
	#region Remove
	
	public static bool RemoveBaseFloatKey(string p_key)
	{
		return BaseSaveStruct.FloatKeys.RemoveByKey(p_key);
	}
	
	public static bool RemoveBaseIntKey(string p_key)
	{
		return BaseSaveStruct.IntKeys.RemoveByKey(p_key);
	}
	
	public static bool RemoveBaseBoolKey(string p_key)
	{
		return BaseSaveStruct.BoolKeys.RemoveByKey(p_key);
	}
	
	public static bool RemoveBaseStringKey(string p_key)
	{
		return BaseSaveStruct.StringKeys.RemoveByKey(p_key);
	}
	
	public static bool RemoveBaseObjectKey(string p_key)
	{
		return BaseSaveStruct.ObjectKeys.RemoveByKey(p_key);
	}
	
	
	#endregion
	
	#region Save/Load
	
	public static void LoadFromBase()
	{
		m_baseSaveStruct = SerializerHelper.Deserialize<KeySaveStruct>("", m_baseSaveFileName, true) as KeySaveStruct;
		if(m_baseSaveStruct == null)
			m_baseSaveStruct = new KeySaveStruct();
	}

	public static void SaveToBase()
	{
		SaveToBase(SerializationTypeEnum.AutoDetect);
	}

	public static void SaveToBase(SerializationTypeEnum p_serializationType)
	{
		SerializerHelper.Serialize<KeySaveStruct>(BaseSaveStruct, "", m_baseSaveFileName, true, p_serializationType);
	}

	public static void CallSaveToBase()
	{
		if(GlobalScheduler.InstanceExists() && Application.isPlaying)
			GlobalScheduler.CallFunction(GlobalScheduler.Instance.gameObject, new Delegates.FunctionPointer(SaveToBase), 0.2f, false);
		else
			SaveToBase();
	}
	
	#endregion
	
	#endregion

	#endregion
}

public class KeySaveStruct
{
	#region Private Variables

	AOTDictionaryKV<string, float> m_floatKeys = new AOTDictionaryKV<string, float>();
	AOTDictionaryKV<string, int> m_intKeys = new AOTDictionaryKV<string, int>();
	AOTDictionaryKV<string, bool> m_boolKeys = new AOTDictionaryKV<string, bool>();
	AOTDictionaryKV<string, string> m_stringKeys = new AOTDictionaryKV<string, string>();
	AOTDictionaryKV<string, object> m_objectKeys = new AOTDictionaryKV<string, object>();

	#endregion

	#region Public Properties

	public AOTDictionaryKV<string, float> FloatKeys 
	{
		get
		{
			if(m_floatKeys == null)
				m_floatKeys = new AOTDictionaryKV<string, float>();
			return m_floatKeys;
		}
		set
		{
			if(m_floatKeys == value)
				return;
			m_floatKeys= value;
		}
	}

	public AOTDictionaryKV<string, int> IntKeys 
	{
		get
		{
			if(m_intKeys == null)
				m_intKeys = new AOTDictionaryKV<string, int>();
			return m_intKeys;
		}
		set
		{
			if(m_intKeys == value)
				return;
			m_intKeys= value;
		}
	}

	public AOTDictionaryKV<string, bool> BoolKeys 
	{
		get
		{
			if(m_boolKeys == null)
				m_boolKeys = new AOTDictionaryKV<string, bool>();
			return m_boolKeys;
		}
		set
		{
			if(m_boolKeys == value)
				return;
			m_boolKeys= value;
		}
	}

	public AOTDictionaryKV<string, string> StringKeys 
	{
		get
		{
			if(m_stringKeys == null)
				m_stringKeys = new AOTDictionaryKV<string, string>();
			return m_stringKeys;
		}
		set
		{
			if(m_stringKeys == value)
				return;
			m_stringKeys= value;
		}
	}

	public AOTDictionaryKV<string, object> ObjectKeys 
	{
		get
		{
			if(m_objectKeys == null)
				m_objectKeys = new AOTDictionaryKV<string, object>();
			return m_objectKeys;
		}
		set
		{
			if(m_objectKeys == value)
				return;
			m_objectKeys= value;
		}
	}

	#endregion

	#region Public Functions

	public void MergeReplacing(KeySaveStruct p_saveStructToMerge)
	{
		if(p_saveStructToMerge != null)
		{
			FloatKeys.MergeReplacing(p_saveStructToMerge.FloatKeys);
			IntKeys.MergeReplacing(p_saveStructToMerge.IntKeys);
			BoolKeys.MergeReplacing(p_saveStructToMerge.BoolKeys);
			StringKeys.MergeReplacing(p_saveStructToMerge.StringKeys);
			ObjectKeys.MergeReplacing(p_saveStructToMerge.ObjectKeys);
		}
	}

	#endregion
}
