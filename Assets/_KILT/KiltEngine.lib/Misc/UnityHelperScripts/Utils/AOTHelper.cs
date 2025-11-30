using UnityEngine;
using System.Collections;
#if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
using Kilt.RT.Reflection;
#endif

public static class AOTHelper {

	public static bool IsNullable(object obj)
	{
		if (obj == null) return true; // obvious
		System.Type type = obj.GetType();
        //METRO CHANGED ASSEMBLY see: https://msdn.microsoft.com/en-us/library/windows/apps/br230302(v=VS.85).aspx#reflection
        #if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
		if (!type.IsValueType()) return true; // ref-type
        #else
        if (!type.IsValueType) return true; // ref-type
		#endif
		if (System.Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
		return false; // value-type
	}
	
	public static bool IsNull(object p_object)
	{
		/*bool v_return = false;
		try
		{
			string v_dummyCheck = p_object.ToString();
			if(v_dummyCheck != null)
				v_return = false;
		}
		catch
		{
			v_return = true;
		}
		return v_return;*/
		return p_object == null;
	}

	public static new bool Equals(object p_object1, object p_object2)
	{
		if (IsNull(p_object1) && IsNull(p_object2))
			return true;
		else if (!IsNull(p_object1) && !IsNull(p_object2))
		{
			if (p_object1.Equals(p_object2)) 
				return true;
			else if (p_object1 is string && p_object2 is string) 
			{
				string v_string1 = p_object1.ToString();
				string v_string2 = p_object2.ToString();
				if (v_string1.Equals (v_string2))
					return true;
			}

			bool v_canCompare = false;
			double v_number1 = ObjectToDouble(p_object1, out v_canCompare);
			double v_number2 = ObjectToDouble(p_object2, out v_canCompare);
			if(v_canCompare)
			{
				if(v_number1 == v_number2)
					return true;
			}
		}
		return false;
	}

	static double ObjectToDouble(object p_object, out bool p_sucess)
	{
		p_sucess = false;
		double v_number = 0;
		if(p_object is long)
		{
			p_sucess = true;
			v_number += (long)p_object;
		}
		else if(p_object is double)
		{
			p_sucess = true;
			v_number += (double)p_object;
		}
		else if(p_object is int)
		{
			p_sucess = true;
			v_number += (int)p_object;
		}
		else if(p_object is float)
		{
			p_sucess = true;
			v_number += (float)p_object;
		}

		return v_number;
	}
}
