//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2014  Illogika
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(AssetPathAttribute))]
	public class AssetPathDrawer : IllogikaDrawer {

		AssetPathAttribute assetPathAttribute { get { return ((AssetPathAttribute)attribute); } }
		
		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
			return base.GetPropertyHeight(prop, label) * 2;
	    }
		
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			if(prop.propertyType != SerializedPropertyType.String)
			{
				WrongVariableTypeWarning("AssetPath", "strings");
				return;
			}

			int originalIndentLevel = EditorGUI.indentLevel;

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID (FocusType.Passive), label);
			EditorGUI.indentLevel = 0;

			position.height /= 2;

			if(prop.hasMultipleDifferentValues)
			{
				EditorGUI.BeginChangeCheck();

				Object temp = EditorGUI.ObjectField(position, Resources.Load("-"), assetPathAttribute.type, false);

				if(EditorGUI.EndChangeCheck())
				{
					assetPathAttribute.obj = temp;
					prop.stringValue = FormatString(AssetDatabase.GetAssetPath(temp));
				}
			}
			else
			{
				EditorGUI.BeginChangeCheck();

				if(assetPathAttribute.obj == null && !string.IsNullOrEmpty(prop.stringValue))
					SelectObject(prop.stringValue);

				assetPathAttribute.obj = EditorGUI.ObjectField(position, assetPathAttribute.obj, assetPathAttribute.type, false);
				string temp = AssetDatabase.GetAssetPath(assetPathAttribute.obj);

				if(EditorGUI.EndChangeCheck())
				{
					prop.stringValue = temp;
					prop.stringValue = FormatString(prop.stringValue);
				}

				position.y += base.GetPropertyHeight(prop, label);

				EditorGUI.SelectableLabel(position, prop.stringValue);
			}

			EditorGUI.indentLevel = originalIndentLevel;
			EditorGUI.EndProperty();
		}

		void SelectObject(string path)
		{
			switch (assetPathAttribute.pathOptions)
			{
				case PathOptions.RelativeToAssets:
					assetPathAttribute.obj = AssetDatabase.LoadAssetAtPath("Assets/" + path, assetPathAttribute.type);
					break;
				case PathOptions.RelativeToResources:
					assetPathAttribute.obj = Resources.Load(path);
					break;
				case PathOptions.FilenameOnly:
					string fullPath = (from p in AssetDatabase.GetAllAssetPaths() where Path.GetFileName(p).Equals(path) select p).FirstOrDefault();
					assetPathAttribute.obj = AssetDatabase.LoadAssetAtPath(fullPath, assetPathAttribute.type);
					break;
			}
		}

		string FormatString(string path)
		{
			switch (assetPathAttribute.pathOptions)
			{
				case PathOptions.RelativeToAssets:
					path = path.Substring(path.IndexOf("Assets/") + 7);
					break;
				case PathOptions.RelativeToResources:
					if (path.Contains("Resources/"))
						path = path.Substring(path.IndexOf("Resources/") + 10).Replace(Path.GetExtension(path), "");
					else
						Debug.LogWarning("Selected asset is not in a Resources folder");
					break;
				case PathOptions.FilenameOnly:
					path = Path.GetFileName(path);
					break;
			}

			return path;
		}
	}


#if !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3
	public static partial class EditorGUIEx
	{
		public static string AssetPath(Rect position, string label, string assetPath, PathOptions pathOptions)
		{
			return AssetPath(position, label, assetPath, typeof(System.Object), pathOptions);
		}

		public static string AssetPath(Rect position, string label, string assetPath, System.Type type, PathOptions pathOptions)
		{
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(label));
			EditorGUI.indentLevel = 0;

			position.height = EditorGUIUtility.singleLineHeight;

			EditorGUI.BeginChangeCheck();

			Object obj = SelectObject(assetPath, pathOptions, type);

			obj = EditorGUI.ObjectField(position, obj, type, false);
			string temp = AssetDatabase.GetAssetPath(obj);

			if (EditorGUI.EndChangeCheck())
			{
				temp = FormatString(temp, pathOptions);
			}

			position.y += EditorGUIUtility.singleLineHeight; ;

			EditorGUI.SelectableLabel(position, temp);

			return temp;
		}

		static Object SelectObject(string path, PathOptions pathOptions, System.Type type)
		{
			switch (pathOptions)
			{
				case PathOptions.RelativeToAssets:
					return AssetDatabase.LoadAssetAtPath("Assets/" + path, type);
				case PathOptions.RelativeToResources:
					return Resources.Load(path);
				case PathOptions.FilenameOnly:
					string fullPath = (from p in AssetDatabase.GetAllAssetPaths() where Path.GetFileName(p).Equals(path) select p).FirstOrDefault();
					return AssetDatabase.LoadAssetAtPath(fullPath, type);
			}
			return null;
		}

		static string FormatString(string path, PathOptions pathOptions)
		{
			switch (pathOptions)
			{
				case PathOptions.RelativeToAssets:
					path = path.Substring(path.IndexOf("Assets/") + 7);
					break;
				case PathOptions.RelativeToResources:
					if (path.Contains("Resources/"))
						path = path.Substring(path.IndexOf("Resources/") + 10).Replace(Path.GetExtension(path), "");
					else
						Debug.LogWarning("Selected asset is not in a Resources folder");
					break;
				case PathOptions.FilenameOnly:
					path = Path.GetFileName(path);
					break;
			}

			return path;
		}
	}

	public static partial class EditorGUILayoutEx
	{
		public static string AssetPath(string label, string assetPath, PathOptions pathOptions)
		{
			return AssetPath(label, assetPath, typeof(System.Object), pathOptions);
		}

		public static string AssetPath(string label, string assetPath, System.Type type, PathOptions pathOptions)
		{
			EditorGUILayout.LabelField("");
			Rect position = GUILayoutUtility.GetLastRect();
			EditorGUILayout.LabelField("");

			return EditorGUIEx.AssetPath(position, label, assetPath, type, pathOptions);
		}
	}

#endif

}


