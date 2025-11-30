//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

public interface IAOTList
{
	int Count 
	{ 
		get;
	}

	bool FoldOut
	{
		get;
		set;
	}

	object this[int i]
	{
		get;
		set;
	}

	bool Add(object p_object);
	bool Remove(object p_item);
	void RemoveAt(int p_index);
}

#if UNITY_WINRT && !UNITY_EDITOR
[DataContract]
#else
[System.Serializable]
#endif
public class AOTList<T> : FoldOutStruct, IAOTList
{
#region Const

	const int MIN_BUFFER_SIZE = 16;

#endregion

#region Internal Variables

	[SerializeField]
	private T[] buffer;
	private System.Type type = typeof(T);
	private int _count = 0;

#endregion

#region  Public Properties

	[Newtonsoft.Json.JsonIgnore, System.Xml.Serialization.XmlIgnoreAttribute]
	public System.Type ContainerType 
	{
		get
		{
			return type;
		}
	}

	//Dont Change Buffer or Count. Set is present because we must desserialize Count and Buffer
	[Newtonsoft.Json.JsonIgnore, System.Xml.Serialization.XmlIgnoreAttribute]
	public int Count 
	{ 
		get 
		{
			//int v_bufferLength = buffer != null? buffer.Length : 0;
			//return v_bufferLength;
			_count = Mathf.Max(0, _count);
			return _count;
		}
		protected set
		{
			int v_value = Mathf.Max(0, value);
			if(_count == v_value)
				return;
			_count = v_value;
		}
	}

	public T[] Buffer 
	{ 
		get 
		{
			if(buffer == null)
			{
				buffer = new T[0];
			}
			return buffer;
		} 
		set {buffer = value;} 
	}

	/// <summary>
	/// Convenience function. I recommend using .buffer instead.
	/// </summary>
	
	public T this[int i]
	{
		get { return buffer[i]; }
		set { buffer[i] = value; }
	}

#endregion

#region IAOTList Implementations
	
	object IAOTList.this[int i]
	{
		get { return buffer[i]; }
		set 
		{
			try
			{
				if(value is T)
					buffer[i] = (T)value; 
			}
			catch{}
		}
	}
	
	bool IAOTList.Remove(object p_object)
	{
		try
		{
			if(p_object is T)
				return Remove((T) p_object);
		}
		catch{}
		return false;
	}
	
	bool IAOTList.Add(object p_object)
	{
		try
		{
			if(p_object is T)
			{
				Add((T) p_object);
				return true;
			}
		}
		catch{}
		return false;
	}
	
#endregion

#region Constructors

	public AOTList()
	{
		buffer = new T[0];
		_count = 0;
	}

	public AOTList(T[] p_array)
	{
		buffer = CloneArray(p_array);
		_count = p_array.Length;
	}

	public AOTList(List<T> p_list)
	{
		buffer = CloneArray(p_list != null? p_list.ToArray() : null);
		_count = p_list != null? p_list.Count: 0;
	}

	public AOTList(AOTList<T> p_aotList)
	{
		buffer = CloneArray(p_aotList != null? p_aotList.ToArray() : null);
		_count = p_aotList != null? p_aotList.Count: 0;
	}

#endregion

#region List Functions

	/// <summary>
	/// For 'foreach' functionality.
	/// </summary>
	
	public IEnumerator<T> GetEnumerator ()
	{
		if (buffer != null)
		{
			for (int i = 0; i < Count; ++i)
			{
				yield return buffer[i];
			}
		}
	}

	/// <summary>
	/// Helper function that expands the Count of the array, maintaining the content.
	/// </summary>
	/// 

	protected void AllocateMore ()
	{
		//T[] newList = new T[Count+1];
		int v_min = Mathf.Max(MIN_BUFFER_SIZE, Count);
		T[] newList = (buffer != null) ? new T[Mathf.Max(buffer.Length << 1, v_min)] : new T[v_min];
		if (buffer != null && Count > 0) buffer.CopyTo(newList, 0);
		buffer = newList;
	}
	
	/// <summary>
	/// Trim the unnecessary memory, resizing the buffer to be of 'Length' Count.
	/// Call this function only if you are sure that the buffer won't need to reCount anytime soon.
	/// </summary>
	protected void Trim ()
	{
		Trim (Count, false);
	}


	protected void Trim (int p_valueToTrim, bool p_updateCount = true)
	{
		if (buffer != null)
		{
			int v_valueToTrim = Mathf.Max(0,p_valueToTrim);
			if (p_valueToTrim < buffer.Length)
			{
				T[] newList = new T[v_valueToTrim];
				for (int i = 0; i < v_valueToTrim; ++i) newList[i] = buffer[i];
				buffer = newList;
			}
		}
		else buffer = new T[0];
		if(p_updateCount)
			Count = buffer.Length;
	}
	
	public void Clear () { buffer = null; Count = 0;}
	
	/// <summary>
	/// Clear the array and release the used memory.
	/// </summary>
	
	//public void Release () { Count = 0; buffer = null; }
	
	/// <summary>
	/// Add the specified item to the end of the list.
	/// </summary>
	
	public void Add (T item)
	{
		AddWithoutCallEvents (item);
		OnAdd(item);
	}

	protected void AddWithoutCallEvents (T item)
	{
		Count++;
		if (buffer == null || Count >= buffer.Length) AllocateMore();
		buffer[Count-1] = item;
	}
	
	/// <summary>
	/// Insert an item at the specified index, pushing the entries back.
	/// </summary>
	
	public void Insert (int index, T item)
	{
		index = Mathf.Max(0, index);
		if (index < Count)
		{
			Count++;
			if (buffer == null || Count >= buffer.Length) AllocateMore();
			for (int i = Count-1; i > index; --i)
			{
				buffer[i] = buffer[i - 1];
			}
			buffer[index] = item;
			OnAdd(item);
		}
		else Add(item);
	}
	
	/// <summary>
	/// Returns 'true' if the specified item is within the list.
	/// </summary>
	
	public bool Contains (T item)
	{
		if (buffer == null) return false;
		for (int i = 0; i < Count; ++i) 
		{
			if(
				(AOTHelper.IsNull(buffer[i]) && AOTHelper.IsNull(item)) || 
				(!AOTHelper.IsNull(buffer[i]) && buffer[i].Equals(item)) 
			  ) 
				return true;
		}
		return false;
	}
	
	/// <summary>
	/// Remove the specified item from the list. Note that RemoveAt() is faster and is advisable if you already know the index.
	/// </summary>
	
	public bool Remove (T item)
	{
		bool v_sucess = RemoveWithoutCallEvents (item);
		if(v_sucess)
			OnRemove(item);
		return v_sucess;
	}

	protected bool RemoveWithoutCallEvents (T item)
	{
		bool v_sucess = false;
		if (buffer != null)
		{
			EqualityComparer<T> comp = EqualityComparer<T>.Default;
			for (int i = 0; i < Count; ++i)
			{
				if (comp.Equals(buffer[i], item))
				{
					//--Count;
					buffer[i] = default(T);
					for (int b = i; b < Count; ++b) 
					{
						if(b+1 < Count)
							buffer[b] = buffer[b + 1];
						else
							buffer[b] = default(T);
					}
					v_sucess = true;
					break;
				}
			}
			if(v_sucess)
				Count = Mathf.Max(0,Count-1);
			if(Buffer.Length > (Count*3) && Buffer.Length > MIN_BUFFER_SIZE)
				Trim();
		}
		return v_sucess;
	}
	
	/// <summary>
	/// Remove an item at the specified index.
	/// </summary>
	
	public void RemoveAt (int index)
	{
		if (buffer != null && index < Count)
		{
			//--Count;
			OnRemove(buffer[index]);
			buffer[index] = default(T);
			for (int b = index; b < Count; ++b) 
			{
				if(b+1 < Count)
					buffer[b] = buffer[b + 1];
				else
					buffer[b] = default(T);
			}
			Count = Mathf.Max(0,Count-1);
			if(Buffer.Length > (Count*3) && Buffer.Length > MIN_BUFFER_SIZE)
				Trim();
		}
	}
	
	/// <summary>
	/// Mimic List's ToArray() functionality, except that in this case the list is reCountd to match the current Count.
	/// </summary>
	
	public T[] ToArray () 
	{ 
		Trim(); 
		T[] v_array = CloneArray(Buffer);
		return v_array;
	}
	
	/// <summary>
	/// List.Sort equivalent.
	/// </summary>
	
	public void Sort (System.Comparison<T> comparer)
	{
		bool changed = true;
		
		while (changed)
		{
			changed = false;
			
			for (int i = 1; i < Count; ++i)
			{
				if (comparer.Invoke(Buffer[i - 1], Buffer[i]) > 0)
				{
					T temp = Buffer[i];
					Buffer[i] = Buffer[i - 1];
					Buffer[i - 1] = temp;
					changed = true;
				}
			}
		}
	}

#endregion

#region New List Functions

	public T GetFirst()
	{
		T v_first = default(T);
		if(Count > 0)
			v_first = this[0];
		return v_first;
	}
	
	public T GetLast()
	{
		T v_first = default(T);
		if(Count > 0)
			v_first = this[Count -1];
		return v_first;
	}
	
	public AOTList<string> GetStringList()
	{
		AOTList<string> v_stringList = new AOTList<string>();
		for(int i=0; i<Count; i++)
		{
			object v_object = this[i];
			string v_toString = "null";
			try
			{
				v_toString = v_object.ToString();
			}
			catch{
				v_toString = "null";
			}
			v_stringList.Add(v_toString);
		}
		return v_stringList;
	}
	
	public AOTList<T> CloneList()
	{
		AOTList<T> v_clonedList = new AOTList<T>();
		foreach(T v_object in this)
		{
			v_clonedList.Add(v_object);
		}
		return v_clonedList;
	}

	public bool ContainsNull()
	{
		for(int i=0; i<this.Count; i++)
		{
			object v_object = this[i];
			if(AOTHelper.IsNull(v_object))
				return true;
		}
		return false;
	}
	
	public void RemoveNulls()
	{
		AOTList<T> v_newList =  new AOTList<T>();
		
		for(int i=0; i<this.Count; i++)
		{
			T v_object = this[i];
			if(!AOTHelper.IsNull(v_object))
				v_newList.Add(v_object);
		}
		this.Clear();
		foreach(T v_object in v_newList)
			this.AddWithoutCallEvents(v_object);
	}
	
	public bool RemoveChecking(T p_object, bool p_removeNulls = true)
	{
		bool v_sucess = false;
		if(!AOTHelper.IsNull(p_object))
		{
			if(p_removeNulls)
				RemoveNulls();
			AOTList<T> v_newList =  new AOTList<T>();
			for(int i=0; i<this.Count; i++)
			{
				try
				{
					T v_object = this[i];
					if(!AOTHelper.Equals(p_object, v_object))
						v_newList.Add(v_object);
					else
						v_sucess = true;
				}
				catch
				{
					UnityEngine.Debug.Log("An error occurred when trying to RemoveChecking");
					v_sucess = true;
				}
			}
			this.Clear();
			foreach(T v_object in v_newList)
				this.AddWithoutCallEvents(v_object);
		}
		if(v_sucess)
			OnRemove(p_object);
		return v_sucess;
	}
	
	public bool AddChecking(T p_object)
	{
		bool v_sucess = false;
		try
		{
			if(!AOTHelper.IsNull(p_object)
			   && !this.Contains(p_object))
			{
				this.Add(p_object);
				v_sucess = true;
			}
		}
		catch
		{
			UnityEngine.Debug.Log("An error occurred when trying to AddChecking");
		}
		return v_sucess;
	}
	
	public void MergeList(AOTList<T> p_otherList)
	{
		if(p_otherList != null)
		{
			foreach(T v_object in p_otherList)
			{
				this.AddChecking(v_object);
			}
		}
	}

	public void MergeList(T[] p_array)
	{
		MergeList(new AOTList<T>(p_array));
	}

	public void MergeList(List<T> p_otherList)
	{
		MergeList(new AOTList<T>(p_otherList));
	}
	
	public void UnmergeList(AOTList<T> p_otherList)
	{
		if(p_otherList != null)
		{
			AOTList<T> v_dummyList = new AOTList<T>();
			this.RemoveNulls();
			for(int i=0; i<this.Count; i++)
			{
				T v_object = this[i];
				if(!p_otherList.Contains(v_object))
					v_dummyList.Add(v_object);
				else
					OnRemove(v_object);
			}
			this.Clear();
			for(int i=0; i<v_dummyList.Count; i++)
			{
				try
				{
					T v_object = v_dummyList[i];
					this.AddWithoutCallEvents(v_object);
				}
				catch
				{
					UnityEngine.Debug.Log("An error occurred when trying to UnmergeList");
				}
			}
		}
	}

	public void UnmergeList(List<T> p_otherList)
	{
		UnmergeList(new AOTList<T>(p_otherList));
	}

	public void UnmergeList(T[] p_array)
	{
		UnmergeList(new AOTList<T>(p_array));
	}
	
	public void Shuffle()  
	{  
		System.Random rng = new System.Random();  
		int n = this.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = this[k];  
			this[k] = this[n];  
			this[n] = value;  
		}  
	}
	
	public int ObjectIndex(T p_object)  
	{  
		for(int i=0; i<this.Count; i++)
		{
			if(AOTHelper.Equals(this[i], p_object))
				return i;
		}
		return -1;
	}

	public static System.Type GetContainerFilterType()
	{
		return typeof(T);
	}

#endregion

#region Serialization CallBacks

	//We must trim Before Serialize
	[OnSerializing]
	private void OnSerializingMethod(StreamingContext context)
	{
		Trim();
	}

	//And set buffer after deserialize
	[OnDeserialized]
	private void OnDeserializedMethod(StreamingContext context)
	{
		Count = Buffer.Length;
	}

#endregion

#region Internal Events

	protected virtual void OnAdd(T p_item)
	{
	}

	protected virtual void OnRemove(T p_item)
	{
	}

#endregion

#region Static Functions

	public static T[] CloneArray(T[] p_buffer)
	{
		T[] v_newBuffer = (p_buffer != null) ? new T[p_buffer.Length] : new T[0];
		if (p_buffer != null && p_buffer.Length > 0) p_buffer.CopyTo(v_newBuffer, 0);
		return v_newBuffer;
	}

#endregion
}

/*
MethodInfo method = GetType().GetMethod("DoesEntityExist")
                             .MakeGenericMethod(new Type[] { t });
method.Invoke(this, new object[] { entityGuid, transaction });
 */