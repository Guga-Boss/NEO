//----------------------------------------------
//
//         Copyright © 2014  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace HeavyDutyInspector
{

	public static class CreateKeywords
	{
		[MenuItem("Assets/ScriptableObjects/Create New Keywords")]
		public static void CreateKeywordsConfig()
		{
			KeywordsConfig config = ScriptableObject.CreateInstance<KeywordsConfig>();

			if (!System.IO.Directory.Exists(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets/Resources/Config/")))
				System.IO.Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets/Resources/Config/"));

			AssetDatabase.CreateAsset(config, "Assets/Resources/Config/KeywordsConfig.asset");
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();
			Selection.activeObject = config;
		}
	}

}
