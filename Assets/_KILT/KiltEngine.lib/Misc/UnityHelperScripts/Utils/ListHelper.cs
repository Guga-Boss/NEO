using System.Collections;
using System.Collections.Generic;

public static class ListHelper {
	
	#region List Helper
	
	public static T GetFirst<T>(this List<T> p_list)
	{
		T v_first = default(T);
		if(p_list != null &&  p_list.Count > 0)
			v_first = p_list[0];
		return v_first;
	}
	
	public static T GetLast<T>(this List<T> p_list)
	{
		T v_first = default(T);
		if(p_list != null &&  p_list.Count > 0)
			v_first = p_list[p_list.Count -1];
		return v_first;
	}
	
	public static List<string> GetStringList<T>(this List<T> p_list)
	{
		List<string> v_stringList = new List<string>();
		if(p_list != null)
		{
			for(int i=0; i<p_list.Count; i++)
			{
				object v_object = p_list[i];
				string v_toString = "NULL";
				try
				{
					v_toString = v_object.ToString();
				}
				catch{
					v_toString = "NULL";
				}
				v_stringList.Add(v_toString);
			}
		}
		return v_stringList;
	}
	
	public static List<T> CloneList<T>(this List<T> p_list)
	{
		List<T> v_clonedList = new List<T>();
		if(p_list != null)
		{
			foreach(T v_object in p_list)
			{
				v_clonedList.Add(v_object);
			}
		}
		return v_clonedList;
	}
	
	public static bool ContainsNull<T>(this List<T> p_list) where T : class
	{
		if(p_list != null)
		{
			for(int i=0; i<p_list.Count; i++)
			{
				object v_object = p_list[i];
				if(v_object == null)
					return true;
			}
		}
		return false;
	}
	
	public static void RemoveNulls<T>(this List<T> p_list) where T : class
	{
		if(p_list != null)
		{
			List<T> v_newList =  new List<T>();
			
			for(int i=0; i<p_list.Count; i++)
			{
				T v_object = p_list[i];
				if(!AOTHelper.IsNull(v_object))
					v_newList.Add(v_object);
			}
			p_list.Clear();
			foreach(T v_object in v_newList)
				p_list.Add(v_object);
		}
	}
	
	public static bool RemoveChecking<T>(this List<T> p_list, T p_object, bool p_removeNulls = true)
	{
		bool v_sucess = false;
		if(p_list != null && !AOTHelper.IsNull(p_object))
		{
			List<T> v_newList =  new List<T>();
			for(int i=0; i<p_list.Count; i++)
			{
				try
				{
					T v_object = p_list[i];
					if(!p_removeNulls || !AOTHelper.IsNull(v_object))
					{
						if(!AOTHelper.Equals(p_object, v_object))
							v_newList.Add(v_object);
						else
							v_sucess = true;
					}
				}
				catch
				{
					UnityEngine.Debug.Log("An error occurred when trying to RemoveChecking");
					v_sucess = true;
				}
			}
			p_list.Clear();
			foreach(T v_object in v_newList)
				p_list.Add(v_object);
		}
		return v_sucess;
	}
	
	public static bool AddChecking<T>(this List<T> p_list, T p_object)
	{
		bool v_sucess = false;
		try
		{
			if(p_list != null && !AOTHelper.IsNull(p_object)
			   && !p_list.Contains(p_object))
			{
				p_list.Add(p_object);
				v_sucess = true;
			}
		}
		catch
		{
			UnityEngine.Debug.Log("An error occurred when trying to AddChecking");
		}
		return v_sucess;
	}
	
	public static void MergeList<T>(this List<T> p_list, List<T> p_otherList)
	{
		if(p_otherList != null)
		{
			foreach(T v_object in p_otherList)
			{
				p_list.AddChecking(v_object);
			}
		}
	}
	
	public static void UnmergeList<T>(this List<T> p_list, List<T> p_otherList)
	{
		if(p_otherList != null)
		{
			List<T> v_dummyList = new List<T>();
			for(int i=0; i<p_list.Count; i++)
				//foreach(T v_object in p_otherList)
			{
				T v_object = p_list[i];
				if(!p_otherList.Contains(v_object))
					v_dummyList.Add(v_object);
			}
			p_list.Clear();
			for(int i=0; i<v_dummyList.Count; i++)
			{
				try
				{
					T v_object = v_dummyList[i];
					if(!AOTHelper.IsNull(v_object))
					{
						p_list.Add(v_object);
					}
				}
				catch
				{
					UnityEngine.Debug.Log("An error occurred when trying to UnmergeList");
				}
			}
		}
	}

    private static System.Random rng = new System.Random();
	public static void Shuffle<T>(this List<T> p_list)  
	{  
		int n = p_list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = p_list[k];  
			p_list[k] = p_list[n];  
			p_list[n] = value;  
		}  
	}
	
	public static int ObjectIndex<T>(this List<T> p_list, T p_object)  
	{  
		for(int i=0; i<p_list.Count; i++)
		{
			if(AOTHelper.Equals(p_list[i], p_object))
				return i;
		}
		return -1;
	}

	public static System.Type GetContainerFilterType<T>(this IEnumerable<T> p_container)
	{
		return typeof(T);
	}
	
	#endregion
	
}

//Old ListHelper
/*using System.Collections;
using System.Collections.Generic;

public static class ListHelper {
	
	#region List Helper

	public static T GetFirst<T>(this IList<T> p_list)
	{
		T v_first = default(T);
		if(p_list != null &&  p_list.Count > 0)
			v_first = p_list[0];
		return v_first;
	}

	public static T GetLast<T>(this IList<T> p_list)
	{
		T v_first = default(T);
		if(p_list != null &&  p_list.Count > 0)
			v_first = p_list[p_list.Count -1];
		return v_first;
	}

	public static List<string> GetStringList<T>(this List<T> p_list)
	{
		List<string> v_stringList = new List<string>();
		if(p_list != null)
		{
			foreach(T v_object in p_list)
			{
				if(!(typeof(T) is object) || (!EqualityComparer<T>.Default.Equals(v_object, default(T))))
					v_stringList.Add(v_object.ToString());
				else
					v_stringList.Add("null");
			}
		}
		return v_stringList;
	}
	
	public static List<T> CloneList<T>(this IList<T> p_list)
	{
		List<T> v_clonedList = new List<T>();
		if(p_list != null)
		{
			foreach(T v_object in p_list)
			{
				v_clonedList.Add(v_object);
			}
		}
		return v_clonedList;
	}

	public static bool ContainsNull<T>(this IList<T> p_list) where T : class
	{
		if(p_list != null)
		{
			foreach(T v_object in p_list)
			{
				if(EqualityComparer<T>.Default.Equals(v_object, default(T)))
					return true;
			}
		}
		return false;
	}

	public static void RemoveNulls<T>(this IList<T> p_list) where T : class
	{
		if(p_list != null)
		{
			List<T> v_newList =  new List<T>();
			foreach(T v_object in p_list)
			{
				if(!EqualityComparer<T>.Default.Equals(v_object, default(T)))
					v_newList.Add(v_object);
			}
			p_list.Clear();
			foreach(T v_object in v_newList)
				p_list.Add(v_object);
		}
	}

	public static bool RemoveChecking<T>(this IList<T> p_list, T p_object, bool p_removeNulls = true)
	{
		bool v_sucess = false;
		if(p_list != null && p_object != null)
		{
			List<T> v_newList =  new List<T>();
			foreach(T v_object in p_list)
			{
				if(!p_removeNulls || !AOTHelper.IsNullable(p_object) ||(!EqualityComparer<T>.Default.Equals(p_object, default(T))) )
				{
					if(!EqualityComparer<T>.Default.Equals(p_object, v_object))
						v_newList.Add(v_object);
					else
						v_sucess = true;
				}
			}
			p_list.Clear();
			foreach(T v_object in v_newList)
				p_list.Add(v_object);
		}
		return v_sucess;
	}

	public static bool AddChecking<T>(this IList<T> p_list, T p_object)
	{
		bool v_sucess = false;
		if(p_list != null && (!( KiltUtils.IsSameOrSubclass(typeof(T), typeof(object)) ) || (!EqualityComparer<T>.Default.Equals(p_object, default(T))) ) && !p_list.Contains(p_object))
		{
			p_list.Add(p_object);
			v_sucess = true;
		}
		return v_sucess;
	}

	public static void MergeList<T>(this IList<T> p_list, IList<T> p_otherList)
	{
		if(p_otherList != null)
		{
			foreach(T v_object in p_otherList)
			{
				p_list.AddChecking<T>(v_object);
			}
		}
	}

	public static void UnmergeList<T>(this IList<T> p_list, IList<T> p_otherList)
	{
		if(p_otherList != null)
		{
			foreach(T v_object in p_otherList)
			{
				p_list.RemoveChecking<T>(v_object);
			}
		}
	}
	
	public static void Shuffle<T>(this IList<T> p_list)  
	{  
		System.Random rng = new System.Random();  
		int n = p_list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = p_list[k];  
			p_list[k] = p_list[n];  
			p_list[n] = value;  
		}  
	}

	public static int ObjectIndex<T>(this IList<T> p_list, T p_object)  
	{  
		for(int i=0; i<p_list.Count; i++)
		{
			if(EqualityComparer<T>.Default.Equals(p_list[i], p_object))
				return i;
		}
		return -1;
	}
	
	#endregion

}*/
