using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
#if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
using Kilt.RT.Reflection;
#else
using System.Reflection;
#endif

public static class EnumHelper 
{
	#region Long Enum Functions

	private static long SetLongFlags(long p_value, long p_flags, bool p_on)
	{
		try
		{
			long lValue = p_value;
			long lFlag = p_flags;
			if (p_on)
			{
				lValue |= lFlag;
			}
			else
			{
				lValue &= (~lFlag);
			}
			return lValue;
		}
		catch{}
		return p_on ? p_flags : p_value;
	}

	public static bool CheckIfIsEnum(System.Type p_type, bool withFlags)
	{
		if(p_type == null)
			return false;
		//METRO CHANGED ASSEMBLY see: https://msdn.microsoft.com/en-us/library/windows/apps/br230302(v=VS.85).aspx#reflection
		#if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
		if (!p_type.IsEnum())
			return false;
		#else
		if (!p_type.IsEnum)
			return false;
		#endif
		#if UNITY_WINRT && !UNITY_EDITOR
		//NOT SUPPORTED
		#else
		if (withFlags && !System.Attribute.IsDefined(p_type, typeof(System.FlagsAttribute)))
			return false;
		#endif
		return true;
	}

	public static bool ContainsLongFlag(long p_value, long p_flag, bool p_isFlagEnumType = true)
	{
		if(p_isFlagEnumType)
		{
			try
			{
				long lValue = p_value;
				long lFlag = p_flag;
				return (lValue & lFlag) != 0;
			}
			catch{}
		}
		else
			return p_value == p_flag;
		return false;
	}
	
	public static bool ContainsLongFlag(long p_value, long p_flag, System.Type p_enumType)
	{
		if(CheckIfIsEnum(p_enumType, true))
		{
			try
			{
				long lValue = p_value;
				long lFlag = p_flag;
				return (lValue & lFlag) != 0;
			}
			catch{}
		}
		else if (CheckIfIsEnum(p_enumType, false))
			return p_value == p_flag;
		return false;
	}

	public static long SetLongFlags(long p_value, long p_flags)
	{
		return SetLongFlags(p_value, p_flags, true);
	}
	
	public static long ClearLongFlags(long p_value, long p_flags)
	{
		return SetLongFlags(p_value, p_flags, false);
	}

	#endregion

	#region Strong-Typed Enums

	#if UNITY_EDITOR || (!UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_WINRT)
	private static T SetFlags<T>(this T value, T flags, bool on) where T : struct , System.IConvertible
	#else
	private static T SetFlags<T>(this T value, T flags, bool on) where T : struct
	#endif
	{
		if(CheckIfIsEnum<T>(true))
		{
			try
			{
				long lValue = System.Convert.ToInt64(value);
				long lFlag = System.Convert.ToInt64(flags);
				if (on)
				{
					lValue |= lFlag;
				}
				else
				{
					lValue &= (~lFlag);
				}
				return (T)System.Enum.ToObject(typeof(T), lValue);
			}
			catch{}
		}
		if(on)
		return flags;
		else
		return value;
	}

	#if UNITY_EDITOR || (!UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_WINRT)
	public static bool CheckIfIsEnum<T>(bool withFlags) where T : struct , System.IConvertible
	#else
	public static bool CheckIfIsEnum<T>(bool withFlags) where T : struct
	#endif
	{
        #if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
		if (!typeof(T).IsEnum())
			return false;
        #else
        if (!typeof(T).IsEnum)
			return false;
		#endif

		#if UNITY_WINRT && !UNITY_EDITOR
		//NOT SUPPORTED
		#else
		if (withFlags && !System.Attribute.IsDefined(typeof(T), typeof(System.FlagsAttribute)))
			return false;
		#endif
		return true;
	}

	#if UNITY_EDITOR || (!UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_WINRT)
	public static List<T> GetFlags<T>(this T value) where T : struct , System.IConvertible
	#else
	public static List<T> GetFlags<T>(this T value) where T : struct
	#endif
	{
		List<T> v_flags = new List<T>();
		if(CheckIfIsEnum<T>(true))
		{
			foreach (T flag in System.Enum.GetValues(typeof(T)))
			{
				if (value.ContainsFlag(flag))
					v_flags.AddChecking(flag);
			}
		}
		else if(CheckIfIsEnum<T>(false))
		{
			v_flags.AddChecking(value);
		}
		return v_flags;
	}

	#if UNITY_EDITOR || (!UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_WINRT)
	public static bool ContainsFlag<T>(this T value, T flag) where T : struct , System.IConvertible
	#else
	public static bool ContainsFlag<T>(this T value, T flag) where T : struct
	#endif
	{
		if(CheckIfIsEnum<T>(true))
		{
			try
			{
				long lValue = System.Convert.ToInt64(value);
				long lFlag = System.Convert.ToInt64(flag);
				return (lValue & lFlag) != 0;
			}
			catch{}
		}
		if(CheckIfIsEnum<T>(false))
		{
			if(EqualityComparer<T>.Default.Equals(value, flag))
				return true;
		}
		return false;
	}

	#if  UNITY_EDITOR || (!UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_WINRT)
	public static T SetFlags<T>(this T value, T flags) where T : struct , System.IConvertible
	#else
	public static T SetFlags<T>(this T value, T flags) where T : struct
	#endif
	{
		return value.SetFlags(flags, true);
	}

	#if UNITY_EDITOR || (!UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_WINRT)
	public static T ClearFlags<T>(this T value, T flags) where T : struct , System.IConvertible
	#else
	public static T ClearFlags<T>(this T value, T flags) where T : struct
	#endif
	{
		return value.SetFlags(flags, false);
	}

	/*#if UNITY_EDITOR || (!UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_WINRT)
	public static string GetDescription<T>(this T value) where T : struct , System.IConvertible
	#else
	public static string GetDescription<T>(this T value) where T : struct
	#endif
	{
		if(CheckIfIsEnum<T>(false))
		{
			string name = System.Enum.GetName(typeof(T), value);
			if (name != null)
			{
				FieldInfo field = typeof(T).GetField(name);
				if (field != null)
				{
					DescriptionAttribute attr = System.Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
					if (attr != null)
					{
						return attr.Description;
					}
				}
			}
		}
		return "";
	}*/

	#endregion
}
