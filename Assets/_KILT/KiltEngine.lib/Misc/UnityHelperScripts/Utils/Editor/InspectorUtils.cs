using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

public class InspectorUtils 
{
	#region Public Draws

	public static SerializableType SerializableTypePopup(Rect p_rect, string p_label, SerializableType p_value, System.Type p_filterType, bool p_acceptGenericDefinition = false, bool p_acceptAbstractDefinition = false)
	{
		return SerializableTypePopupInternal(p_rect, p_label, p_value, p_filterType, false, p_acceptGenericDefinition);
	}

	public static SerializableType SerializableTypePopup(string p_label, SerializableType p_value, System.Type p_filterType, bool p_acceptGenericDefinition = false, bool p_acceptAbstractDefinition = false)
	{
		return SerializableTypePopupInternal(new Rect(), p_label, p_value, p_filterType, true, p_acceptGenericDefinition);
	}

	public static SerializableType SerializableTypePopupInternal(Rect p_rect, string p_label, SerializableType p_value, System.Type p_filterType, bool p_guiLayout, bool p_acceptGenericDefinition, bool p_acceptAbstractDefinition = false, bool p_acceptNull = true)
	{
		Color v_oldColor = GUI.backgroundColor;
		SerializableType v_return = new SerializableType();
		try
		{
			Vector2 v_offset = new Vector2(10, 16);
			Rect v_labelRect = new Rect(p_rect.x, p_rect.y, p_rect.width/3.0f, v_offset.y);
			Rect v_assemblyRect = new Rect(v_labelRect.x + v_labelRect.width, p_rect.y, 30, v_offset.y);
			Rect v_typeRect = new Rect(v_assemblyRect.x + v_assemblyRect.width, p_rect.y, Mathf.Max(0, p_rect.width - (v_labelRect.width +v_assemblyRect.width)), v_offset.y);

			AOTDictionaryKV<Assembly, System.Type[]> v_assemblyTypesDict = KiltUtils.GetAssemblyTypesDict(false);
			List<Assembly> v_assemblies = new List<Assembly>();
			Assembly v_currentAssembly = p_value == null || p_value.CastedType == null? null : p_value.CastedType.Assembly;

			//Filter Assemblies
			foreach(AOTKeyValuePair<Assembly, System.Type[]> v_pair in v_assemblyTypesDict)
			{
				if(v_pair != null && v_pair.Key != null)
				{
					if(AssemblyContainsFilteredType(v_pair.Key, p_filterType, p_acceptGenericDefinition, p_acceptAbstractDefinition))
						v_assemblies.Add(v_pair.Key);
					try
					{
						if(v_currentAssembly == null && p_value.StringType.Contains(v_pair.Key.FullName))
							v_currentAssembly = v_pair.Key;
					}
					catch{}
				}
			}
			if(p_acceptNull)
				v_assemblies.Insert(0, null);

			if(p_guiLayout)
				EditorGUILayout.BeginHorizontal();

			//Draw Label
			if(p_value.CastedType != null && p_value.CastedType.GetGenericArguments().Length > 0)
			{
				if(p_guiLayout)
					p_value.FoldOut = EditorGUILayout.Foldout(p_value.FoldOut, p_label);
				else
					p_value.FoldOut = EditorGUI.Foldout(v_labelRect, p_value.FoldOut, p_label);
			}
			else
			{
				if(p_guiLayout)
					GUILayout.Label(p_label);
				else
					GUI.Label(v_labelRect, p_label);
			}

			//Draw Popup Select Assembly
			int v_selectedIndex = v_assemblies.IndexOf(v_currentAssembly);

			GUI.backgroundColor = v_selectedIndex > 0 || (!p_acceptNull && v_selectedIndex >= 0)? new Color(0.2f, 1, 0.5f) : new Color(1, 0.2f, 0.5f);
			if(p_guiLayout)
				v_selectedIndex = EditorGUILayout.Popup(v_selectedIndex, v_assemblies.GetStringList().ToArray(), GUILayout.Width(30));
			else
				v_selectedIndex = EditorGUI.Popup(v_assemblyRect, v_selectedIndex, v_assemblies.GetStringList().ToArray());
			Assembly v_newAssembly = v_assemblies.Count > v_selectedIndex && v_selectedIndex >= 0? v_assemblies[v_selectedIndex] : null;
			bool v_assemblyChanged = false;
			if(v_newAssembly != v_currentAssembly)
			{
				v_currentAssembly = v_newAssembly;
				v_assemblyChanged = true;
			}
			GUI.backgroundColor = v_oldColor;

			//Filter Types in Assembly
			List<System.Type> v_assemblyTypes = new List<System.Type>();
			if(v_currentAssembly != null)
			{
				foreach(System.Type v_type in v_assemblyTypesDict.GetValueByKey(v_currentAssembly))
				{
					if(v_type != null 
					   && (!v_type.IsGenericTypeDefinition || p_acceptGenericDefinition) 
					   && (!v_type.IsAbstract || p_acceptAbstractDefinition) 
					   && (p_filterType == null || KiltUtils.IsSameOrSubClassOrImplementInterface(v_type, p_filterType)))
						v_assemblyTypes.Add(v_type);
				}
			}

			if(p_acceptNull)
				v_assemblyTypes.Insert(0, null);

			//Set Selected Index
			SetTypeSelectedIndex(ref v_assemblyTypes, ref p_value, ref v_selectedIndex, v_assemblyChanged, p_acceptNull);

			//Draw Popup Select Type
			GUI.backgroundColor = v_selectedIndex > 0 || (!p_acceptNull && v_selectedIndex >= 0)? new Color(0.2f, 0.8f, 0) : new Color(0.8f, 0.2f, 0);
			if(p_guiLayout)
				v_selectedIndex = EditorGUILayout.Popup(v_selectedIndex, v_assemblyTypes.GetStringList().ToArray());
			else
				v_selectedIndex = EditorGUI.Popup(v_typeRect, v_selectedIndex, v_assemblyTypes.GetStringList().ToArray());
			v_selectedIndex = EditorGUI.Popup(v_typeRect, v_selectedIndex, v_assemblyTypes.GetStringList().ToArray());
			System.Type v_currentType = v_assemblyTypes.Count > v_selectedIndex && v_selectedIndex >= 0? v_assemblyTypes[v_selectedIndex] : null;
			GUI.backgroundColor = v_oldColor;

			//Apply Values
			v_return.CastedType = p_value!= null && TypeImplementGenericTypeDefinition(p_value.CastedType, v_currentType)? p_value.CastedType : v_currentType;
			v_return.StringType = p_value!= null && TypeImplementGenericTypeDefinition(p_value.CastedType, v_currentType)? p_value.StringType : v_return.StringType;
			if(v_return.CastedType == null)
				v_return.StringType = v_currentAssembly != null? ", " + ( v_currentAssembly.FullName) : null;
			if(p_guiLayout)
				EditorGUILayout.EndHorizontal();

			//Generic Drawer Type Definition
			DrawGenericParametersSelector(ref v_currentAssembly, ref p_value, ref v_return, ref v_currentType, ref p_rect, v_offset, p_guiLayout, p_acceptAbstractDefinition);
			v_return.FoldOut = p_value.CastedType.GetGenericArguments().Length > 0? p_value.FoldOut : true;
		}
		catch{}
		return v_return;
	}

	static void SetTypeSelectedIndex(ref List<System.Type> p_assemblyTypes, ref SerializableType p_value, ref int p_selectedIndex, bool p_assemblyChanged, bool p_acceptNull)
	{
		if(p_assemblyChanged)
			p_selectedIndex = p_acceptNull? 1 : 0;
		else
		{
			if(p_value.CastedType != null && p_value.CastedType.GetGenericArguments().Length > 0)
			{
				p_selectedIndex = -1;
				string[] v_splittedValues = p_value.StringType.Split('`');
				string v_typeString = v_splittedValues.Length > 0? v_splittedValues[0] : (p_value.CastedType != null? p_value.CastedType.FullName : "");
				string v_genericArgString = v_splittedValues.Length > 1 && v_splittedValues[1].Length > 1? v_splittedValues[1][0] + "" : "";
				if(!string.IsNullOrEmpty(v_genericArgString))
					v_typeString += "`" + v_genericArgString;
				for(int i=0; i< p_assemblyTypes.Count; i++)
				{
					System.Type v_type = p_assemblyTypes[i];
					if(v_type != null && v_splittedValues.Length > 0 && v_type.FullName.Contains(v_typeString))
					{
						p_selectedIndex = i;
						break;
					}
				}
			}
			else
			{
				p_selectedIndex = p_value.CastedType != null ? p_assemblyTypes.IndexOf(p_value.CastedType) : -1;
			}
		}
	}

	static void DrawGenericParametersSelector(ref Assembly p_currentAssembly, ref SerializableType p_value, ref SerializableType p_return, ref System.Type p_currentType, ref Rect p_rect, Vector2 p_offset, bool p_guiLayout, bool p_acceptAbstractDefinition)
	{
		if(p_return.CastedType != null && p_return.CastedType.FullName.Contains("`")) 
		{
			System.Type[] v_genericArguments = p_currentType.GetGenericArguments();
			System.Type[] v_specializedArguments = p_return.CastedType.GetGenericArguments();
			string p_composedInternalType = "`" + v_genericArguments.Length+ "[";
			
			for(int i=0; i< v_genericArguments.Length; i++)
			{
				System.Type v_genericArgumentType = v_genericArguments[i];
				System.Type v_specializedArgumentType = v_specializedArguments.Length > i? v_specializedArguments[i] : null;
				if(v_specializedArgumentType != null && v_specializedArgumentType.IsGenericParameter)
					v_specializedArgumentType = null;
				if(v_genericArgumentType != null)
				{
					p_composedInternalType += (i > 0)? ",[" : "[";
					System.Type v_baseType = v_genericArgumentType.BaseType;
					if(v_baseType == null)
						v_baseType = typeof(object);
					if(v_specializedArgumentType == null)
						v_specializedArgumentType = v_baseType;
					
					SerializableType v_argument = new SerializableType(v_specializedArgumentType);
					Rect v_argumentRect = new Rect(p_rect.x + p_offset.x, p_rect.y + p_offset.y*(i+1), p_rect.width-(p_offset.x), p_offset.y);
					if(p_value.FoldOut)
					{
						if(p_guiLayout)
						{
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(p_offset.x);
						}
						v_argument = SerializableTypePopupInternal(v_argumentRect, "Param "+ (i+1) + ": ", v_argument, v_baseType, p_guiLayout, false, p_acceptAbstractDefinition, false);
						if(p_guiLayout)
							EditorGUILayout.EndHorizontal();
					}
					p_composedInternalType += v_argument.StringType;
					p_composedInternalType += "]";
					
				}
			}
			p_composedInternalType += "]";
			string[] v_splittedValue = p_return.StringType.Split('`');
			if(v_splittedValue.Length > 0)
			{
				string v_castedTypeString = v_splittedValue[0] + p_composedInternalType + ", "+ p_currentAssembly.FullName;
				System.Type v_type = System.Type.GetType(v_castedTypeString);
				if(v_type != null)
				{
					if(v_type != p_return.CastedType)
						p_return.CastedType = v_type;
				}
				else
					p_return.CastedType = p_currentType;
				p_return.StringType = v_castedTypeString;
			}
		}
	}

	static bool TypeImplementGenericTypeDefinition(System.Type p_type, System.Type p_genericType)
	{
		if(p_type != null && p_genericType != null && p_genericType.IsGenericTypeDefinition)
		{
			string[] v_typeStringSplitted = p_type.FullName.Split('`');
			string[] v_genericTypeStringSplitted = p_genericType.FullName.Split('`');

			string v_typeString = v_typeStringSplitted.Length >= 2 && v_typeStringSplitted[1].Length > 0? v_typeStringSplitted[0] + '`' + v_typeStringSplitted[1].Split('[') : "";
			string v_genericTypeString = v_genericTypeStringSplitted.Length >= 2 && v_genericTypeStringSplitted[1].Length > 0? v_genericTypeStringSplitted[0] + '`' + v_genericTypeStringSplitted[1].Split('[') : "";
			if(!string.IsNullOrEmpty(v_typeString) && !string.IsNullOrEmpty(v_genericTypeString) && string.Equals(v_typeString, v_genericTypeString))
			{
				return true;
			}
		}
		return false;
	}

	static bool AssemblyContainsFilteredType(Assembly p_assembly, System.Type p_filterType, bool p_acceptGenericDefinition, bool p_acceptAbstractDefinition)
	{
		if(p_filterType == null)
			return true;
		if(p_assembly != null)
		{
			foreach(System.Type v_type in p_assembly.GetTypes())
			{
				if(v_type != null && KiltUtils.IsSameOrSubClassOrImplementInterface(v_type, p_filterType) 
				   && (p_acceptGenericDefinition || !v_type.IsGenericTypeDefinition)
				   && (p_acceptAbstractDefinition || !v_type.IsAbstract))
					return true;
			}
		}
		return false;
	}

	public static bool DrawButton(string p_text, Color v_color, params GUILayoutOption[] p_layoutOption)
	{
		Color v_oldColor = GUI.backgroundColor;
		GUI.backgroundColor = v_color;
		bool v_sucess = GUILayout.Button(p_text, p_layoutOption);
		GUI.backgroundColor = v_oldColor;
		return v_sucess;
	}

	public static void DrawTitleText(string p_text, Color v_color)
	{
		Color v_oldColor = GUI.backgroundColor;
		GUI.backgroundColor = v_color;
		EditorGUILayout.HelpBox(p_text, MessageType.None);
		GUI.backgroundColor = v_oldColor;
	}

	public static System.IConvertible BitmapPopup(string p_label, System.IConvertible p_enum)
	{
		System.IConvertible p_return = p_enum;
		if(EnumHelper.CheckIfIsEnum(p_enum.GetType() ,true))
		{
			try
			{
				int v_value = (int)((System.IConvertible)p_enum);
				if(string.IsNullOrEmpty(p_label))
					v_value = EditorGUILayout.MaskField(v_value, System.Enum.GetNames(p_enum.GetType()));
				else
					v_value = EditorGUILayout.MaskField(p_label, v_value, System.Enum.GetNames(p_enum.GetType()));
				p_return = ((System.IConvertible)v_value);
			}
			catch{}
		}
		else if(EnumHelper.CheckIfIsEnum(p_enum.GetType() ,false))
		{
			try
			{
				if(string.IsNullOrEmpty(p_label))
					p_return = (System.Enum)EditorGUILayout.EnumPopup((System.Enum)p_enum);
				else
					p_return = (System.Enum)EditorGUILayout.EnumPopup(p_label, (System.Enum)p_enum);
			}
			catch{}
		}

		return p_return;
	}
	
	//Only works with serialized properties
	public static void DrawDefaultList(SerializedObject p_serializedObject , string p_propertyName)
	{
		try
		{
			if(string.IsNullOrEmpty(p_propertyName))
			   p_propertyName = "";
			if(p_serializedObject != null)
			{
				p_serializedObject.Update();

				SerializedProperty v_serializedProperty = p_serializedObject.FindProperty (p_propertyName);
				if(v_serializedProperty != null)
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(v_serializedProperty, true);
					if(EditorGUI.EndChangeCheck())
						p_serializedObject.ApplyModifiedProperties();
				}
				else
					EditorGUILayout.HelpBox("Cannot Find serialized property with name " + p_propertyName , MessageType.Warning);
			}
		}
		catch
		{
			EditorGUILayout.HelpBox("Cannot Find serialized property with name " + p_propertyName , MessageType.Warning);
		}
	}

	public static bool DrawArray<ObjectType>(string p_labelString, ref ObjectType[] p_array, bool p_isOpened)
	{
		List<ObjectType> v_list = p_array != null? new List<ObjectType>(p_array) : new List<ObjectType>();
		bool v_return = DrawList<ObjectType>(p_labelString, ref v_list, p_isOpened);
		p_array = v_list.ToArray();
		return v_return;
	}

	public static bool DrawList<ObjectType>(string p_labelString, ref List<ObjectType> p_list, bool p_isOpened)
	{
		IList v_genericList = p_list as IList;
		bool v_return = DrawListInternal<ObjectType>(p_labelString, ref v_genericList, p_isOpened);
		try
		{
			p_list = v_genericList as List<ObjectType>;
		}
		catch{}
		return v_return;
		
	}

	public static bool DrawDictionary<ComparerType, ObjectType>(string p_labelString, ref AOTDictionaryKV<ComparerType, ObjectType> p_dict, bool p_isOpened)
	{
		bool v_return = DrawDictionaryInternal<ComparerType, ObjectType>(p_labelString, ref p_dict, p_isOpened);
		return v_return;
	}

	public static object DrawType<ObjectType>(string p_labelString, object p_object)
	{
		object v_return = default(ObjectType);
		try
		{
			if(typeof(ObjectType) == typeof(float))
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)EditorGUILayout.FloatField((float)p_object);
				else
					v_return = (object)EditorGUILayout.FloatField(p_labelString, (float)p_object);
			}
			else if(typeof(ObjectType) == typeof(int))
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)EditorGUILayout.IntField((int)p_object);
				else
					v_return = (object)EditorGUILayout.IntField(p_labelString, (int)p_object);
			}
			else if(typeof(ObjectType) == typeof(Color))
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)EditorGUILayout.ColorField((Color)p_object);
				else
					v_return = (object)EditorGUILayout.ColorField(p_labelString, (Color)p_object);
			}
			else if(typeof(ObjectType).IsEnum)
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)BitmapPopup("",(System.Enum)p_object);
				else
					v_return = (object)BitmapPopup(p_labelString, (System.Enum)p_object);
			}
			else if(typeof(ObjectType) == typeof(string))
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)EditorGUILayout.TextField((string)p_object);
				else
					v_return = (object)EditorGUILayout.TextField(p_labelString, (string)p_object);
			}
			else if(typeof(ObjectType) == typeof(Vector4))
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)EditorGUILayout.Vector4Field("", (Vector4)p_object);
				else
					v_return = (object)EditorGUILayout.Vector4Field(p_labelString, (Vector4)p_object);
			}
			else if(typeof(ObjectType) == typeof(Vector3))
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)EditorGUILayout.Vector3Field("",(Vector3)p_object);
				else
					v_return = (object)EditorGUILayout.Vector3Field(p_labelString, (Vector3)p_object);
			}
			else if(typeof(ObjectType) == typeof(Vector2))
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)EditorGUILayout.Vector2Field("",(Vector2)p_object);
				else
					v_return = (object)EditorGUILayout.Vector2Field(p_labelString, (Vector2)p_object);
			}
			else if(typeof(ObjectType) == typeof(Rect))
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)EditorGUILayout.RectField("",(Rect)p_object);
				else
					v_return = (object)EditorGUILayout.RectField(p_labelString, (Rect)p_object);
			}
			else if(typeof(ObjectType) == typeof(bool))
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)EditorGUILayout.Toggle((bool)p_object);
				else
					v_return = (object)EditorGUILayout.Toggle(p_labelString, (bool)p_object);
			}
			else if(KiltUtils.IsSameOrSubclass(typeof(ObjectType), typeof(AnimationCurve)))
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)EditorGUILayout.CurveField((AnimationCurve)p_object);
				else
					v_return = (object)EditorGUILayout.CurveField(p_labelString, (AnimationCurve)p_object);
			}
			else if(KiltUtils.IsSameOrSubclass(typeof(ObjectType), typeof(object)))
			{
				if(string.IsNullOrEmpty(p_labelString))
					v_return = (object)EditorGUILayout.ObjectField((Object)p_object, typeof(ObjectType), true);
				else
					v_return = (object)EditorGUILayout.ObjectField(p_labelString, (Object)p_object, typeof(ObjectType), true);
			}
			else
				EditorGUILayout.HelpBox(typeof(ObjectType) +  " not supported." , MessageType.Warning);
		}
		catch
		{
			EditorGUILayout.HelpBox("Error drawing object.", MessageType.Warning);
		}

		return v_return;
	}

	#endregion

	#region Internal Draws

	private static bool DrawListInternal<ObjectType>(string p_labelString, ref IList p_list, bool p_isOpened)
	{
		p_isOpened = EditorGUILayout.Foldout(p_isOpened, p_labelString);
		try
		{
			//Try Fill Empty Lists
			if(p_list == null)
			{
				try
				{
					p_list = new List<ObjectType>();
				}
				catch{}
			}
			if(p_isOpened)
			{
				if(p_list != null)
				{
					//CheckDummy (This can return Exception, so check before draw anything)
					int v_index = p_list.Add(default(ObjectType));
					p_list.RemoveAt(v_index);

					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(10);
					int v_count = Mathf.Max(0, EditorGUILayout.IntField("Size",p_list.Count));
					EditorGUILayout.EndHorizontal();

					while(v_count != p_list.Count)
					{
						if(v_count > p_list.Count)
							p_list.Add(default(ObjectType));
						else
							p_list.RemoveAt(p_list.Count -1);
					}
					for(int i=0; i<p_list.Count; i++)
					{
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(10);
						object p_object = DrawType<ObjectType>("Element " + (i+1), p_list[i]);
						p_list[i] = p_object;
						EditorGUILayout.EndHorizontal();
					}
				}
				else
					EditorGUILayout.HelpBox("Array cannot be null" , MessageType.Warning);
			}
		}
		catch
		{
			EditorGUILayout.HelpBox("Array dont accept " + typeof(ObjectType) , MessageType.Warning);
		}
		return p_isOpened;
	}

	private static bool DrawDictionaryInternal<ComparerType, ObjectType>(string p_labelString, ref AOTDictionaryKV<ComparerType, ObjectType> p_dict, bool p_isOpened)
	{
		p_isOpened = EditorGUILayout.Foldout(p_isOpened, p_labelString);
		try
		{
			//Try Fill Empty Lists
			if(p_dict == null)
			{
				try
				{
					p_dict = new AOTDictionaryKV<ComparerType, ObjectType>();
				}
				catch{}
			}
			if(p_isOpened)
			{
				if(p_dict != null)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(10);
					int v_count = Mathf.Max(0, EditorGUILayout.IntField("Size",p_dict.Count));
					EditorGUILayout.EndHorizontal();
					
					while(v_count != p_dict.Count)
					{
						if(v_count > p_dict.Count)
							p_dict.Add(new AOTKeyValuePair<ComparerType, ObjectType>());
						else
							p_dict.RemoveAt(p_dict.Count -1);
					}
					for(int i=0; i<p_dict.Count; i++)
					{
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(10);
						object p_comparer = DrawType<ComparerType>("Element " + (i+1), p_dict[i].Key);
						object p_object = DrawType<ObjectType>("", p_dict[i].Value);
						p_dict[i] = new AOTKeyValuePair<ComparerType, ObjectType>((ComparerType)p_comparer, (ObjectType)p_object);
						EditorGUILayout.EndHorizontal();
					}
				}
				else
					EditorGUILayout.HelpBox("Dictionary cannot be null" , MessageType.Warning);
			}
		}
		catch
		{
			EditorGUILayout.HelpBox("Dictionary dont accept " + typeof(ObjectType) , MessageType.Warning);
		}
		return p_isOpened;
	}

	#endregion

	#region AOT List

	public static void DrawAOTList<AOTListType>
		(string p_labelString, 
		 AOTListType p_list, 
		 Delegates.FunctionPointer<AOTListType, int> p_onDrawElementFunction, 
		 Delegates.FunctionPointer<AOTListType> p_onAddElementFunctions
		 ) where AOTListType: IAOTList
	{
		DrawAOTList<AOTListType>(p_labelString, p_list, p_onDrawElementFunction, p_onAddElementFunctions,Vector2.zero,Vector2.zero, false);
	}

	public static Vector2 DrawAOTList<AOTListType>
		(string p_labelString, 
		 AOTListType p_list, 
		 Delegates.FunctionPointer<AOTListType, int> p_onDrawElementFunction, 
		 Delegates.FunctionPointer<AOTListType> p_onAddElementFunctions,
		 Vector2 p_scrollView,
		 bool p_useScroll = true) where AOTListType: IAOTList
	{
		return DrawAOTList<AOTListType>(p_labelString, p_list, p_onDrawElementFunction, p_onAddElementFunctions,p_scrollView,Vector2.zero, p_useScroll);
	}

	public static Vector2 DrawAOTList<AOTListType>
		(string p_labelString, 
		 AOTListType p_list, 
		 Delegates.FunctionPointer<AOTListType, int> p_onDrawElementFunction, 
		 Delegates.FunctionPointer<AOTListType> p_onAddElementFunctions,
		 Vector2 p_scrollView,
		 Vector2 p_initialOffset,
		 bool p_useScroll = true) where AOTListType: IAOTList
	{
		Vector2 v_scrollView = p_scrollView;
		if(p_list != null)
		{
			InspectorUtils.BeginContainer(p_initialOffset.x, p_initialOffset.y);
			p_list.FoldOut = EditorGUILayout.Foldout(p_list.FoldOut, p_labelString);
			int v_indexToRemove = -1;
			if(p_list.FoldOut)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(10);
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();
				GUI.enabled = false;
				GUILayout.Label("Length", GUILayout.Width(50));
				EditorGUILayout.IntField(p_list.Count, GUILayout.Width(80));
				GUI.enabled = true;
				//Add Functions Caller
				if(InspectorUtils.DrawButton("Add", Color.cyan, GUILayout.Width(50)))	
				{
					if(p_onAddElementFunctions != null)
						p_onAddElementFunctions(p_list);
				}
				EditorGUILayout.EndHorizontal();

				//Draw Opener
				InspectorUtils.DrawTitleText("{", new Color(0.5f, 0.5f, 0.5f));

				if(p_useScroll)
					v_scrollView = GUILayout.BeginScrollView(p_scrollView);
				for(int i=0; i<p_list.Count; i++)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(10);
					EditorGUILayout.BeginVertical();
					if(p_onDrawElementFunction != null)
						p_onDrawElementFunction(p_list, i);
					EditorGUILayout.EndVertical();
					if(InspectorUtils.DrawButton("X", Color.red, GUILayout.MaxWidth(24), GUILayout.MaxHeight(15)))
						v_indexToRemove = i;
					EditorGUILayout.EndHorizontal();
					
				}
				if(p_useScroll)
					GUILayout.EndScrollView();

				//Draw Closer
				InspectorUtils.DrawTitleText("}", new Color(0.5f, 0.5f, 0.5f));
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}
			//Remove Clicked
			if(v_indexToRemove >=0 && v_indexToRemove <= p_list.Count)
			{
				p_list.RemoveAt(v_indexToRemove);
				v_indexToRemove = -1;
			}
			InspectorUtils.EndContainer();
		}
		return v_scrollView;
	}

	#endregion

	#region Texture Generator

	static List<Texture2D> _dynamicGeneratedTextures = new List<Texture2D>();
	public static void BeginContainer(float p_initialOffsetX = 0, float p_initialOffsetY = 5)
	{
		BeginContainer(new Color(0.65f,0.65f, 0.65f, 0.3f), new Color(0.15f,0.15f, 0.15f, 0.3f), p_initialOffsetX, p_initialOffsetY);
	}

	public static void BeginContainer(Color p_personalColor, Color p_proColor, float p_initialOffsetX = 0, float p_initialOffsetY = 5)
	{
		GUILayout.Space(p_initialOffsetY);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(p_initialOffsetX);
		Color v_color = EditorGUIUtility.isProSkin? p_proColor : p_personalColor;
		GUIStyle v_containerGUIStyle = GetColoredGUIStyle(v_color);
		_dynamicGeneratedTextures.Add(v_containerGUIStyle.normal.background);
		EditorGUILayout.BeginVertical(v_containerGUIStyle);

	}

	public static void EndContainer(float p_finalOffsetY = 0)
	{
		GUILayout.Space(p_finalOffsetY);
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		if(_dynamicGeneratedTextures != null && _dynamicGeneratedTextures.Count > 0)
		{
			Texture2D v_textureToDestroy = _dynamicGeneratedTextures.GetLast();
			_dynamicGeneratedTextures.RemoveAt(_dynamicGeneratedTextures.Count -1);
			Object.DestroyImmediate(v_textureToDestroy);
			_dynamicGeneratedTextures.RemoveNulls();
		}
	}
	
	public static GUIStyle GetColoredGUIStyle(Color v_color)
	{
		GUIStyle v_containerGUIStyle = new GUIStyle();
		v_containerGUIStyle.normal.background = MakeTexture(1, 1, v_color);
		return v_containerGUIStyle;
	}

	public static Texture2D MakeTexture(int width, int height, Color col)
	{
		Color[] pix = new Color[width*height];
		
		for(int i = 0; i < pix.Length; i++)
			pix[i] = col;
		
		Texture2D result = new Texture2D(width, height);
		result.SetPixels(pix);
		result.Apply();
		
		return result;
	}

	#endregion

	[UnityEditor.Callbacks.DidReloadScripts]
	static void OnDidReloadScripts() 
	{
		KiltUtils.GetAssemblyTypesDict(true); //Force Refresh Dict
	}
}
