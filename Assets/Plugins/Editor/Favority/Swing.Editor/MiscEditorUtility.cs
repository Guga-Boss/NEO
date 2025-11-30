using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;
using System.Linq;
using System.Reflection;

namespace Swing.Editor
{
	public static class MiscEditorUtility
	{
		/// <summary>
		/// Will return ALL inactive objects.
		/// WARNING: slow!
		/// </summary>
		public static T findSceneObject<T>( bool _includeInactive = true )
			where T : Object
		{
			if (!_includeInactive)
			{
				return (T)Object.FindObjectOfType(typeof(T));
			}

			// [BF] adapted from http://docs.unity3d.com/Documentation/ScriptReference/Resources.FindObjectsOfTypeAll.html
			var objects = (T[])Resources.FindObjectsOfTypeAll(typeof(T));
			return objects.find(obj =>
			                    //// [BF] this seems arbitrary, removed for now
			                    //obj.hideFlags != HideFlags.NotEditable &&
			                    //obj.hideFlags != HideFlags.HideAndDontSave &&

			                    // filter out assets
			                    !AssetDatabase.Contains(obj)
				);
		}
		/// <summary>
		/// Will return ALL inactive objects.
		/// WARNING: slow!
		/// </summary>
		public static T[] findSceneObjects<T>( bool _includeInactive = true )
			where T : Object
		{
			if (!_includeInactive)
			{
				return (T[])Object.FindObjectsOfType(typeof(T));
			}

			// [BF] adapted from http://docs.unity3d.com/Documentation/ScriptReference/Resources.FindObjectsOfTypeAll.html
			var objects = (T[])Resources.FindObjectsOfTypeAll(typeof(T));
			return objects.findAll(obj =>
			                       //// [BF] this seems arbitrary, removed for now
			                       //obj.hideFlags != HideFlags.NotEditable &&
			                       //obj.hideFlags != HideFlags.HideAndDontSave &&

			                       // filter out assets
			                       !AssetDatabase.Contains(obj)
				);
		}

		/// <summary>
		/// Will return ALL inactive objects.
		/// WARNING: slow!
		/// </summary>
		public static IEnumerable<Transform> findSceneRoots()
		{
			return findSceneObjects<Transform>().convertAll(t => t.root).Distinct();
		}

		public static int getLocalIdentifierInFile( Object _sceneObject )
		{
			SerializedObject serializedObject = new SerializedObject(_sceneObject);

			//ReflectionUtility.SetProperty(serializedObject, "inspectorMode", InspectorMode.Debug);
			PropertyInfo inspectorMode = serializedObject.GetType().GetProperty("inspectorMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			inspectorMode.GetSetMethod(true).Invoke(serializedObject, new object[] { InspectorMode.Debug });

			var prop = serializedObject.FindProperty("m_LocalIdentfierInFile");
			return prop.intValue;
		}
	}
}
