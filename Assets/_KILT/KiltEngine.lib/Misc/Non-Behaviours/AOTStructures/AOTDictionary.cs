using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Generic Dictionary Exists if you want to create your own serializable dict, so you can pass your own AOTKeyValuePair to dict
[System.Serializable]
public class AOTGenericDictionary<TAOTPair, TKey, TValue> : AOTList<TAOTPair>, ISerializationCallbackReceiver where TAOTPair: AOTKeyValuePair<TKey, TValue>, new()
{
	#region Serializable CallBack Variables
	[SerializeField]
	List<TValue> _values = new List<TValue>();
	[SerializeField]
	List<TKey> _keys = new List<TKey>();

	#endregion
	
	#region Serializable CallBacks Functions
	
	// save the dictionary to lists
	public virtual void OnBeforeSerialize()
	{
		if(Application.isPlaying)
		{
			_values.Clear();
			_keys.Clear();
			foreach(TAOTPair pair in this)
			{
				_values.Add(pair.Value);
				_keys.Add(pair.Key);
			}
		}
	}
	
	// load dictionary from lists
	public virtual void OnAfterDeserialize()
	{
		this.Clear();
		for(int i = 0; i < Mathf.Min(_values.Count, _keys.Count); i++)
		{
			TAOTPair v_pair = new TAOTPair();
			v_pair.Value = _values[i];
			v_pair.Key = _keys[i];
			this.AddWithoutCallEvents(v_pair);
		}
	}
	
	protected override void OnAdd(TAOTPair p_pair)
	{
		if(p_pair != null)
			_values.Add(p_pair.Value);
		if(p_pair != null)
			_keys.Add(p_pair.Key);
	}

	protected override void OnRemove(TAOTPair p_pair)
	{
		if(p_pair != null)
			_values.RemoveChecking(p_pair.Value, false);
		if(p_pair != null)
			_keys.RemoveChecking(p_pair.Key, false);
	}

	#endregion

	#region Dictionary Functions

	public TAOTPair GetPairByValue(TValue p_value)
	{
		foreach(TAOTPair v_pairValues in this)
		{
			try
			{
				if(AOTHelper.Equals(v_pairValues.Value, p_value))
					return v_pairValues;
			}
			catch{}
		}
		return default(TAOTPair);
	}
	
	public TAOTPair GetPairByKey(TKey p_key)
	{
		foreach(TAOTPair v_pairValues in this)
		{
			try
			{
				if(AOTHelper.Equals(v_pairValues.Key, p_key))
					//if(p_key.Equals(v_pairValues.Comparer))
					return v_pairValues;
			}
			catch{}
		}
		return default(TAOTPair);
	}
	
	public TKey GetKeyByValue(TValue p_value)
	{
		foreach(TAOTPair v_pairValues in this)
		{
			try
			{
				if(AOTHelper.Equals(v_pairValues.Value, p_value))
					//if(p_value.Equals(v_pairValues.Object))
					return v_pairValues.Key;
			}
			catch{}
		}
		return default(TKey);
	}
	
	public TValue GetValueByKey(TKey p_key)
	{
		foreach(TAOTPair v_pairValues in this)
		{
			try
			{
				if(AOTHelper.Equals(v_pairValues.Key, p_key))
					//if(p_key.Equals(v_pairValues.Comparer))
					return v_pairValues.Value;
			}
			catch{}
		}
		return default(TValue);
	}
	
	public AOTList<TValue> GetAllValueByKey(TKey p_key)
	{
		AOTList<TValue> v_values = new AOTList<TValue>();
		foreach(TAOTPair v_pairValues in this)
		{
			try
			{
				if(AOTHelper.Equals(v_pairValues.Key, p_key))
					//if(p_key.Equals(v_pairValues.Comparer))
					v_values.Add(v_pairValues.Value);
			}
			catch{}
		}
		return v_values;
	}
	
	public void RemovePairsWithNullValuesOrKeys()
	{
		RemoveNulls();
		RemovePairsWithNullValues();
		RemovePairsWithNullKeys();
	}
	
	public void RemovePairsWithNullValues()
	{
		AOTList<TAOTPair> v_newList =  new AOTList<TAOTPair>();
		foreach(TAOTPair v_pairValue in this)
		{
			try
			{
				if(!AOTHelper.IsNull(v_pairValue.Value))
					v_newList.Add(v_pairValue);
			}
			catch{}
		}
		this.Clear();
		foreach(TAOTPair v_pairValue in v_newList)
			this.AddWithoutCallEvents(v_pairValue);
	}
	
	public void RemovePairsWithNullKeys()
	{
		AOTList<TAOTPair> v_newList =  new AOTList<TAOTPair>();
		foreach(TAOTPair v_pairValue in this)
		{
			try
			{
				if(!AOTHelper.IsNull(v_pairValue.Key))
					v_newList.Add(v_pairValue);
			}
			catch{}
		}
		this.Clear();
		foreach(TAOTPair v_pairValue in v_newList)
			this.AddWithoutCallEvents(v_pairValue);
	}
	
	public bool ContainsKey(TKey p_key)
	{
		foreach(TAOTPair v_pairValue in this)
		{
			try
			{
				if(AOTHelper.Equals(v_pairValue.Key, p_key))
				{
					return true;
				}
			}
			catch{}
		}
		return false;
	}
	
	public bool ContainsValue(TValue p_value)
	{
		foreach(TAOTPair v_pairValue in this)
		{
			try
			{
				if(AOTHelper.Equals(v_pairValue.Value, p_value))
				{
					return true;
				}
			}
			catch{}
		}
		return false;
	}

	/*public void Add(TValue p_value, TKey p_key)
	{
		Add(p_key, p_value);
	}
	
	public void AddChecking(TValue p_value, TKey p_key)
	{
		AddChecking(p_key, p_value);
	}
	
	public void AddReplacing(TValue p_value, TKey p_key)
	{
		AddReplacing(p_key, p_value);
	}*/

	public void Add(TKey p_key, TValue p_value)
	{
		try
		{
			TAOTPair v_pair = new TAOTPair();
			v_pair.Value = p_value;
			v_pair.Key = p_key;
			base.Add(v_pair);
		}
		catch{}
	}
	
	public void AddChecking(TKey p_key, TValue p_value)
	{
		bool v_found = false;
		foreach(TAOTPair v_pair in this)
		{
			try
			{
				if(AOTHelper.Equals(v_pair.Value, p_value) && 
				   AOTHelper.Equals(v_pair.Key, p_key))
				{
					v_found = true;
					break;
				}
			}
			catch{}
		}
		if(!v_found)
			Add(p_key, p_value);
	}
	
	public void AddReplacing(TKey p_key, TValue p_value)
	{
		bool v_found = false;
		for(int i=0; i< this.Count; i++)
		{
			try
			{
				if(AOTHelper.Equals(this[i].Key, p_key))
				{
					v_found = true;
					TAOTPair v_pair = new TAOTPair();
					v_pair.Value = p_value;
					v_pair.Key = p_key;
					this[i] = v_pair;
					break;
				}
			}
			catch{}
		}
		if(!v_found)
			Add(p_key, p_value);
	}
	
	public void MergeReplacing<TParamPair>(AOTGenericDictionary<TParamPair, TKey, TValue> p_dictToMerge) where TParamPair : AOTKeyValuePair<TKey, TValue>, new()
	{
		if(p_dictToMerge != null)
		{
			foreach(TParamPair v_pair in p_dictToMerge)
			{
				try
				{
					TValue v_value = v_pair.Value;
					TKey v_key = v_pair.Key;
					AddReplacing(v_key, v_value);
				}
				catch{}
			}
		}
	}
	
	public bool Remove(TValue p_value)
	{
		TAOTPair v_removePair = default( TAOTPair );
		foreach(TAOTPair v_pairValue in this)
		{
			try
			{
				if(AOTHelper.Equals(v_pairValue.Value, p_value))	
				{
					v_removePair = v_pairValue;
					break;
				}
			}
			catch{}
		}
		return base.Remove(v_removePair);
	}
	
	public bool RemoveChecking(TValue p_value)
	{
		TAOTPair v_removePair = default( TAOTPair );
		foreach(TAOTPair v_pairValue in this)
		{
			try
			{
				if(AOTHelper.Equals(v_pairValue.Value, p_value))	
				{
					v_removePair = v_pairValue;
					break;
				}
			}
			catch{}
		}
		return this.RemoveChecking(v_removePair);
	}
	
	public bool RemoveByKey(TKey p_key)
	{
		TAOTPair v_removePair = default( TAOTPair );
		foreach(TAOTPair v_pairValue in this)
		{
			try
			{
				if(AOTHelper.Equals(v_pairValue.Key, p_key))	
				{
					v_removePair = v_pairValue;
					break;
				}
			}
			catch{}
		}
		return this.RemoveChecking(v_removePair);
	}
	
	public virtual AOTGenericDictionary<TAOTPair, TKey, TValue> CloneDict()
	{
		AOTGenericDictionary<TAOTPair, TKey, TValue> v_clonedDict = System.Activator.CreateInstance(GetType()) as AOTGenericDictionary<TAOTPair, TKey, TValue>;
		foreach(TAOTPair v_pair in this)
		{
			try
			{
				v_clonedDict.Add(v_pair);
			}
			catch{}
		}
		return v_clonedDict;
	}

	#endregion
}

//[System.Serializable, System.Obsolete("this class contain reversed 'Key and Values'. Use AOTDictionaryKV instead")]
public class AOTDictionary<TValue, TKey> : AOTDictionaryKV<TKey, TValue>
{
	public override AOTGenericDictionary<AOTKeyValuePair<TKey, TValue>, TKey, TValue> CloneDict()
	{
		AOTDictionary<TValue, TKey> v_clonedDict = new AOTDictionary<TValue, TKey>();
		foreach(AOTKeyValuePair<TKey, TValue> v_pair in this)
		{
			try
			{
				v_clonedDict.Add(v_pair);
			}
			catch{}
		}
		return v_clonedDict;
	}
}

[System.Serializable]
public class AOTDictionaryKV<TKey, TValue> : AOTGenericDictionary<AOTKeyValuePair<TKey,TValue>, TKey ,TValue>
{
	public override AOTGenericDictionary<AOTKeyValuePair<TKey,TValue>, TKey, TValue> CloneDict()
	{
		AOTDictionaryKV<TKey, TValue> v_clonedDict = new AOTDictionaryKV<TKey, TValue>();
		foreach(AOTKeyValuePair<TKey,TValue> v_pair in this)
		{
			try
			{
				v_clonedDict.Add(v_pair);
			}
			catch{}
		}
		return v_clonedDict;
	}
}

#region Helper Class

[System.Serializable]
public class AOTKeyValuePair<TKey, TValue> : FoldOutStruct
{
	#region Private Variables

	[SerializeField]
	TKey m_key;
	[SerializeField]
	TValue m_value;
	
	#endregion
	
	#region Public Properties

	//Key is the Comparer
	public TKey Key
	{
		get
		{
			return m_key;
		}
		set
		{
			m_key = value;
		}
	}
	
	//Value is the Object
	public TValue Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	[System.Obsolete("Use Key instead"), System.Xml.Serialization.XmlIgnoreAttribute]
	public TKey Comparer
	{
		protected get
		{
			return m_key;
		}
		set
		{
			m_key = value;
		}
	}

	[System.Obsolete("Use Value instead"), System.Xml.Serialization.XmlIgnoreAttribute]
	public TValue Object
	{
		protected get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	#endregion
	
	#region Constructors
	
	public AOTKeyValuePair()
	{
	}
	
	public AOTKeyValuePair(TKey p_key, TValue p_value)
	{
		m_key = p_key;
		m_value = p_value;
	}
	
	#endregion
	
	#region Helper Methods
	
	public override bool Equals(object p_value)
	{
		if(p_value == this)
			return true;
		if(!AOTHelper.IsNull(p_value))
		{
			try
			{
				AOTKeyValuePair<TKey, TValue> v_castedObject = p_value as AOTKeyValuePair<TKey, TValue>;
				if(v_castedObject != null)
				{
					if(((AOTHelper.IsNull(v_castedObject.Key) &&  AOTHelper.IsNull(this.Key)) || v_castedObject.Comparer.Equals(this.Key)) &&
					   ((AOTHelper.IsNull(v_castedObject.Value) &&  AOTHelper.IsNull(this.Value)) || v_castedObject.Value.Equals(this.Value)))
					{
						return true;
					}
				}
			}
			catch{}
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
	
	#endregion
}

//[System.Serializable, System.Obsolete("this class contain reversed 'Key and Values'. Use AOTKeyValuePair instead")]
public class AOTPair<TValue, TKey> : AOTKeyValuePair<TKey, TValue>
{
	#region Constructors
	
	public AOTPair()
	{
	}
	
	public AOTPair(TValue p_value, TKey p_key) : base(p_key, p_value)
	{
	}
	
	#endregion
}

#endregion