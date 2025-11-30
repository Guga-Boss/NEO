using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;
using Swing.Engine;

namespace Swing.Editor
{
	[Serializable]
	public class FavorityItem
	{
		public Object unityObject;
		public int sceneObjectId; // last known ID

		public string unityObjectName; // last known name
		public string customName;
		public string name
		{
			get { return !string.IsNullOrEmpty(customName) ? customName : unityObject.name; }
		}

		public bool isAsset // TODO: cache this
		{
			get { return AssetDatabase.Contains(unityObject); }
		}

		public Texture icon // TODO: cache this?
		{
			get
			{
				Texture _icon = AssetPreview.GetMiniThumbnail(unityObject);
				//Texture _icon = AssetPreview.GetAssetPreview(item.unityObject);

				if (!_icon)
				{
					_icon = isAsset
						        ? AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(unityObject))
						        : AssetPreview.GetMiniTypeThumbnail(typeof(Transform));
				}

				return _icon;
			}
		}

		public void onPreSave()
		{
			if (!unityObject)
			{
				return;
			}

			sceneObjectId = AssetDatabase.Contains(unityObject)
								? -1
								: MiscEditorUtility.getLocalIdentifierInFile(unityObject);

			unityObjectName = unityObject.name;
		}
		public void onPostLoad( Dictionary<int, Object> _sceneGOs )
		{
			if (unityObject)
			{
				return;
			}

			if (sceneObjectId <= 0) // -1 : not a scene object; 0: not saved yet
			{
				return;
			}

			_sceneGOs.TryGetValue(sceneObjectId, out unityObject);
		}
	}

	[Serializable]
	public class FavorityScene
	{
		public string sceneGuid;
		public List<FavorityItem> items = new List<FavorityItem>();

		public void onPostLoad()
		{
			if (items.Count == 0)
			{
				return;
			}

			// find scene GameObjects and cache their id
			var sceneGOs = MiscEditorUtility.findSceneObjects<GameObject>();
			Dictionary<int, Object> sceneGoIds = new Dictionary<int, Object>(sceneGOs.Length);
			foreach (var sceneGO in sceneGOs)
			{
				int id = MiscEditorUtility.getLocalIdentifierInFile(sceneGO);
				if (id == 0)
				{
					continue; // this GO is not saved yet
				}
				sceneGoIds.Add(id, sceneGO);
			}

			// each item will try to find its GO by its id
			foreach (var item in items)
			{
				item.onPostLoad(sceneGoIds);
			}
		}
	}

	public class FavorityData : BaseScriptableObject
	{
		public List<FavorityItem> projectItems = new List<FavorityItem>();
		public List<FavorityScene> scenes = new List<FavorityScene>();

		public void onPreSave()
		{
			foreach (var item in projectItems)
			{
				item.onPreSave();
			}

			foreach (var scene in scenes)
			{
				foreach (var item in scene.items)
				{
					item.onPreSave();
				}
			}
		}

		static string getSceneGuid()
		{
			return AssetDatabase.AssetPathToGUID(EditorApplication.currentScene);
		}
		public FavorityScene getSceneData()
		{
			return getSceneData(getSceneGuid());
		}
		public FavorityScene getSceneData( string _sceneGuid )
		{
			return scenes.Find(s => s.sceneGuid == _sceneGuid);
		}
		public FavorityScene createSceneData()
		{
			string sceneGuid = getSceneGuid();

			var sceneData = getSceneData(sceneGuid);
			if (sceneData == null)
			{
				sceneData = new FavorityScene();
				sceneData.sceneGuid = sceneGuid;
				scenes.Add(sceneData);
			}

			return sceneData;
		}
	}
}
