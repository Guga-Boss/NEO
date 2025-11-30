using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
using Kilt.RT.Reflection;
#else
using System.Reflection;
#endif

public enum PrefabSearchMethodEnum { Quick, AOS, Full }

public static class KiltUtils
{
#region NGUI/Renderer Helper

	public static int CalculateRendererNextDepth (GameObject p_gameObject)
	{
		int v_depth = -1;
		SpriteRenderer[] v_renderers = p_gameObject.GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0, imax = v_renderers.Length; i < imax; ++i) v_depth = Mathf.Max(v_depth, v_renderers[i].sortingOrder);
		return v_depth + 1;
	}

#if NGUI_DLL

	public static int CalculateNGUINextDepth (GameObject p_gameObject)
	{
		int v_depth = -1;
		UIWidget[] v_widgets = p_gameObject.GetComponentsInChildren<UIWidget>();
		for (int i = 0, imax = v_widgets.Length; i < imax; ++i) v_depth = Mathf.Max(v_depth, v_widgets[i].depth);
		return v_depth + 1;
	}

	public static void ConvertRendererToNGUIScale(SpriteRenderer p_renderer, UIWidget p_widget)
	{
		if(p_renderer != null && p_renderer.sprite != null && p_widget != null)
		{
			//Bounds v_bounds = p_widget.bounds;
			p_widget.transform.localScale = new Vector3(p_widget.transform.localScale.x * p_renderer.sprite.bounds.size.x, 
			                                            p_widget.transform.localScale.y * p_renderer.sprite.bounds.size.y, 
			                                            p_widget.transform.localScale.z * p_renderer.sprite.bounds.size.z);
		}
	}
	
	public static void ConvertNGUIToRendererScale(SpriteRenderer p_renderer, UIWidget p_widget)
	{
		if(p_renderer != null && p_renderer.sprite != null && p_widget != null)
		{
			p_widget.transform.localScale = new Vector3(p_renderer.sprite.bounds.size.x == 0? 0 : p_widget.transform.localScale.x/p_renderer.sprite.bounds.size.x, 
			                                            p_renderer.sprite.bounds.size.y == 0? 0 : p_widget.transform.localScale.y/p_renderer.sprite.bounds.size.y, 
			                                            p_renderer.sprite.bounds.size.z == 0? 0 : p_widget.transform.localScale.z/p_renderer.sprite.bounds.size.z);
		}
	}

#endif

#endregion

#region Path/File Helper
	
	public static List<string> SearchFiles(string p_dir, string p_pattern)
	{
		List <string> v_sceneNames = new List <string>();
		try
		{
#if UNITY_WINRT && !UNITY_EDITOR
			//NOT SUPPORTED
#elif !UNITY_WEBGL && !UNITY_WEBPLAYER && !UNITY_WP8 && !UNITY_WP_8_1
			foreach (string v_file in Directory.GetFiles(p_dir, p_pattern, SearchOption.AllDirectories))
			{
				v_sceneNames.Add (v_file);
			}
#else
			foreach (string v_file in Directory.GetFiles(p_dir, p_pattern))
			{
				v_sceneNames.Add (v_file);
			}
#endif
		}
		catch{}
		return v_sceneNames;
	}
	
	public static string SearchFile(string p_dir, string p_fileName)
	{
		try
		{
#if UNITY_WINRT && !UNITY_EDITOR
			//NOT SUPPORTED
#elif !UNITY_WEBGL && !UNITY_WEBPLAYER && !UNITY_WP8 && !UNITY_WP_8_1
			foreach (string v_file in Directory.GetFiles(p_dir, p_fileName, SearchOption.AllDirectories))
			{
				return v_file;
			}
#else
			foreach (string v_file in Directory.GetFiles(p_dir, p_fileName))
			{
				return v_file;
			}
#endif
		}
		catch{}
		return "";
	}
	
	public static string SearchFile(string p_fileName)
	{
		return SearchFile(Application.dataPath, p_fileName);
	}
	
	public static bool FileExists(string p_fileName)
	{
		string v_file = SearchFile(Application.dataPath, p_fileName);
		return !string.IsNullOrEmpty(v_file);
	}
	
	public static string GetCurrentDataPath()
	{
		//string v_string = System.IO.Directory.GetCurrentDirectory();
		string v_string = Application.dataPath;
		if(Application.isEditor)
			v_string = System.IO.Path.Combine(v_string, "Resources");
		else if(Application.platform == RuntimePlatform.Android)
			v_string = Application.persistentDataPath;
		else if(Application.platform == RuntimePlatform.IPhonePlayer)
			v_string = Application.persistentDataPath;
		else
			v_string = Application.persistentDataPath;
		v_string = KiltUtils.PathCorrection(v_string);
		
		return v_string;
	}
	
	public static string GetCompleteDataPath(string p_folder, string p_file, bool p_folderIsRelativePath = true, bool p_forceCreateDirectory = true)
	{
		string v_fullFolderPath = p_folderIsRelativePath? GetCurrentDataPath() : "";
#if !UNITY_WEBGL && !UNITY_WEBPLAYER
		if(!System.IO.Directory.Exists(v_fullFolderPath) && !string.IsNullOrEmpty(v_fullFolderPath) && p_forceCreateDirectory)
			System.IO.Directory.CreateDirectory(v_fullFolderPath);
		v_fullFolderPath = System.IO.Path.Combine(v_fullFolderPath, p_folder);
		if(!System.IO.Directory.Exists(v_fullFolderPath) && !string.IsNullOrEmpty(v_fullFolderPath) && p_forceCreateDirectory)
			System.IO.Directory.CreateDirectory(v_fullFolderPath);
#elif UNITY_EDITOR
		if(!CallEditorStaticFunctionWithReturn<bool>("SystemIOEditorWebPlayer", "DirectoryExists", v_fullFolderPath) && !string.IsNullOrEmpty(v_fullFolderPath) && p_forceCreateDirectory)
			CallEditorStaticFunction("SystemIOEditorWebPlayer", "CreateDirectory", v_fullFolderPath);
		v_fullFolderPath = CallEditorStaticFunctionWithReturn<string>("SystemIOEditorWebPlayer", "PathCombine", v_fullFolderPath, p_folder);
		if(!CallEditorStaticFunctionWithReturn<bool>("SystemIOEditorWebPlayer", "DirectoryExists", v_fullFolderPath) && !string.IsNullOrEmpty(v_fullFolderPath) && p_forceCreateDirectory)
			CallEditorStaticFunction("SystemIOEditorWebPlayer", "CreateDirectory", v_fullFolderPath);
#else
		v_fullFolderPath = string.IsNullOrEmpty(v_fullFolderPath)? p_folder : ("/" + p_folder);
#endif
		
		string v_resultPath = PathCombine(v_fullFolderPath, p_file);
		return v_resultPath;
	}
	
	public static string GetFileName(string p_fileWithPathOrFileName, bool p_includeFileExtension = false)
	{
		string v_fileName = "";
		if(!string.IsNullOrEmpty(p_fileWithPathOrFileName))
		{
			p_fileWithPathOrFileName = p_fileWithPathOrFileName.Replace("\\", "/");
			string[] v_splitedSlashString = p_fileWithPathOrFileName.Split('/');
			if(v_splitedSlashString.Length > 0)
			{
				string v_fileWithExtension = v_splitedSlashString[v_splitedSlashString.Length-1];
				if(!string.IsNullOrEmpty(v_fileWithExtension))
				{
					string[] v_splitedDotString = v_fileWithExtension.Split('.');
					if(v_splitedDotString.Length > 1)
					{
						string v_fileWithoutExtension = v_splitedDotString[v_splitedDotString.Length -2];
						string v_extension = v_splitedDotString[v_splitedDotString.Length -1];
						v_fileName = v_fileWithoutExtension;
						if(p_includeFileExtension)
							v_fileName += "." + v_extension;
					}
					else
						v_fileName = v_fileWithExtension;
				}
			}
		}
		return v_fileName;
	}
	
	public static string GetFileExtension(string p_fileWithPathOrFileName)
	{
		string v_extension = "";
		if(!string.IsNullOrEmpty(p_fileWithPathOrFileName))
		{
			string[] v_splitedDotString = p_fileWithPathOrFileName.Split('.');
			if(v_splitedDotString.Length > 1)
				v_extension = v_splitedDotString[v_splitedDotString.Length -1];
		}
		return v_extension;
	}
	
	public static string PathCombine(string p_path1, string p_path2)
	{
		try
		{
			if(p_path1 == null)
				p_path1 = "";
			if(p_path2 == null)
				p_path2 = "";
			p_path2 = PathCorrection(p_path2);
			if(p_path2.StartsWith("/") && !string.IsNullOrEmpty(p_path1)) //Fix bug in Path combine when second path starts with "/"
				p_path2 = p_path2.Remove(0,1);
			string v_resultPath = "";
#if !UNITY_WEBGL && !UNITY_WEBPLAYER
			v_resultPath = System.IO.Path.Combine(p_path1, p_path2);
#else
			v_resultPath = p_path1 + "/" + p_path2;
#endif
			v_resultPath = PathCorrection(v_resultPath);
			return v_resultPath;
		}
		catch{}
		return "";
	}
	
	//Used To Remove Incorrections in Path
	public static string PathCorrection(string p_path)
	{
		
		string v_resultPath = string.IsNullOrEmpty(p_path)? "" : p_path;
		try
		{
			v_resultPath = v_resultPath.Replace("\\", "/");
			int v_count = 0;
			int v_maxCount = 50;
			//Remove Double Path Bars
			while(v_resultPath.Contains("//") && v_count < v_maxCount)
			{
				v_resultPath = v_resultPath.Replace("//", "/");
				v_count++;
			}
		}
		catch{}
		return v_resultPath;
	}
	
	public static string PathUncombine(string p_path, string p_pathToUncombine)
	{
		try
		{
			if(p_path == null)
				p_path = "";
			if(p_pathToUncombine == null)
				p_pathToUncombine = "";
			string v_resultPath = PathCorrection(p_path);
			
			if(!string.IsNullOrEmpty(p_pathToUncombine))
			{
				string v_pathToUncombine = PathCorrection(p_pathToUncombine);
				if(!string.IsNullOrEmpty(v_pathToUncombine))
					v_resultPath = v_resultPath.Replace(v_pathToUncombine, "");
			}
			if(v_resultPath.IndexOf("/") == 0)
				v_resultPath = v_resultPath.Length > 1? v_resultPath.Substring(1) : "";
			if(v_resultPath != null && v_resultPath.LastIndexOf("/") == v_resultPath.Length -1 && v_resultPath.Length -1 >= 0)
				v_resultPath = v_resultPath.Length > 1? v_resultPath.Substring(0, v_resultPath.Length -1) : "";
			return PathCorrection(v_resultPath);
		}
		catch {}
		return PathCorrection(p_path);
	}
	
	/// <summary>
	/// Parses the subpath inside folder.
	/// ex: p_fullPath = "C:/Folder/Assets/Resources/Prefabs/Ball.prefab"    
	/// p_folder = "Resources"    
	/// return will be "Prefabs/Ball.prefab"
	/// </summary>
	/// <returns>The subpath inside folder.</returns>
	/// <param name="p_fullPath">P_full path.</param>
	/// <param name="p_folder">P_folder inside full path</param>
	public static string GetSubPathInsideFolder(string p_fullPath, string p_folder, bool p_includeFolder = false)
	{
		if(p_fullPath == null)
			p_fullPath = "";
		if(p_folder == null)
			p_folder = "";
		string v_resultPath = PathCorrection(p_fullPath);
		int v_index = v_resultPath.IndexOf(PathCorrection(p_folder).Trim('/'));
		try
		{
			v_resultPath = v_resultPath.Substring(v_index);
		}
		catch
		{
			v_resultPath = "";
		}
		v_resultPath = v_resultPath.Trim('/');
		if(!p_includeFolder)
			v_resultPath = v_resultPath.ReplaceFirst(p_folder, "").Trim('/');
		return v_resultPath;
	}
	
#endregion

#region Transform Methods

	public static void SetLossyScale(Transform p_prefabTransform, Vector3 p_prefabNewGlobalScale)
	{
		if(p_prefabTransform != null)
		{
			Vector3 v_newLocalScale = GetLocalScaleFromWorldScale(p_prefabTransform, p_prefabNewGlobalScale);
			p_prefabTransform.localScale = v_newLocalScale;
		}
	}

	public static Vector3 GetLocalScaleFromWorldScale(Transform p_prefabTransform, Vector3 p_prefabNewGlobalScale)
	{
		return GetLocalScaleFromWorldScale(p_prefabTransform.parent != null? p_prefabTransform.parent.lossyScale : new Vector3(1,1,1) , p_prefabNewGlobalScale);
	}

	public static Vector3 GetLocalScaleFromWorldScale(Vector3 p_parentLossyScale, Vector3 p_prefabGlobalScale)
	{
		float v_x = p_parentLossyScale.x == 0? 0 : p_prefabGlobalScale.x/p_parentLossyScale.x;
		float v_y = p_parentLossyScale.y == 0? 0 : p_prefabGlobalScale.y/p_parentLossyScale.y;
		float v_z = p_parentLossyScale.z == 0? 0 : p_prefabGlobalScale.z/p_parentLossyScale.z;
		return new Vector3(v_x, v_y, v_z);
	}

	public static void SetWorldEulerAngle(Transform p_prefabTransform, Vector3 p_prefabNewGlobalEulerAngle)
	{
		if(p_prefabTransform != null)
		{
			Vector3 v_newLocalEulerAngle = GetLocalEulerAngleFromWorldEulerAngle(GetWorldEulerAngle(p_prefabTransform.parent), p_prefabNewGlobalEulerAngle);
			p_prefabTransform.localEulerAngles = v_newLocalEulerAngle;
		}
	}

	public static Vector3 GetLocalEulerAngleFromWorldEulerAngle(Vector3 p_parentWorldEulerAngle, Vector3 p_prefabWorldEulerAngle)
	{
		float v_x = p_prefabWorldEulerAngle.x - p_parentWorldEulerAngle.x;
		float v_y = p_prefabWorldEulerAngle.y - p_parentWorldEulerAngle.y;
		float v_z = p_prefabWorldEulerAngle.z - p_parentWorldEulerAngle.z;
		return new Vector3(v_x, v_y, v_z);
	}

	public static Vector3 GetWorldEulerAngle(Transform p_prefabTransform)
	{
		Vector3 v_eulerAngle = new Vector3(0,0,0);
		Transform v_currentTransform = p_prefabTransform;
		while(v_currentTransform != null)
		{
			v_eulerAngle += v_currentTransform.transform.localEulerAngles;
			v_currentTransform = v_currentTransform.parent;
		}
		return v_eulerAngle;
	}

#endregion

#region Copy Methods

	public static T CopyComponent<T>(GameObject p_destination, T p_original, bool p_pasteAsValue = true, bool p_deleteOldOne = false) where T : Component
	{
		if(p_destination != null && p_original != null)
		{
			System.Type v_type = p_original.GetType();
			Component v_copy = null;
			if(p_deleteOldOne)
			{
				v_copy = p_destination.GetComponent(v_type);
				if(v_copy != null)
					Object.DestroyImmediate(v_copy);
			}
			if(p_pasteAsValue)
				v_copy = p_destination.GetComponent(v_type);
			if(v_copy == null)
				v_copy = p_destination.AddComponent(v_type);
            System.Reflection.FieldInfo[] v_fields = new System.Reflection.FieldInfo[0];
            while (v_type != null && v_type != typeof(Component) && v_type != typeof(Behaviour) && v_type != typeof(MonoBehaviour))
			{
                //All Privates and Publics in this Specific Type (Avoid BaseType Component, Behaviour and MonoBehaviour to prevent bugs)
                v_fields = v_type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Public);
                foreach (System.Reflection.FieldInfo v_field in v_fields)
				{
#if UNITY_WINRT && !UNITY_EDITOR
                    if (ReflectionHelper.GetAttribute<SerializeField>(v_field) != null)
                    {
                        object v_newValue = v_field.GetValue(p_original);
					    v_field.SetValue(v_copy, v_newValue);
                    }
#else
                    if (!v_field.IsNotSerialized && System.Attribute.IsDefined(v_field, typeof(SerializeField)))
                    {
                        object v_newValue = v_field.GetValue(p_original);
                        v_field.SetValue(v_copy, v_newValue);
                    }
#endif
				}
#if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
				v_type = v_type.BaseType();
#else
                v_type = v_type.BaseType;
#endif
			}
			
			if(v_copy is Behaviour && p_original is Behaviour)
				(v_copy as Behaviour).enabled = (p_original as Behaviour).enabled;

			
			return v_copy as T;
		}
		return null;
	}

#endregion

#region Finder Methods

	public static GameObject[] FindAllGameObjectsWithName(string p_name)
	{
		List<GameObject> v_objectsInScene = new List<GameObject>(FindAllGameObjects());
		List<GameObject> v_returnList = GetGameObjectsWithNameInList(v_objectsInScene, p_name);
		return v_returnList.ToArray();
	}

	public static GameObject[] FindAllGameObjects(bool p_includeInactive = true)
	{
		List<GameObject> v_finalList = new List<GameObject>();
		GameObject[] v_arrayReturn = v_finalList.ToArray();

		if(!p_includeInactive)
		{
			v_arrayReturn = Object.FindObjectsOfType<GameObject>();
		}
		else if (Application.isEditor)
		{
#if UNITY_EDITOR
			GameObject[] v_allObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
			
			foreach (GameObject v_object in v_allObjects)
			{	
				if (v_object.hideFlags == HideFlags.None)
				{
					string v_assetPath = AssetDatabase.GetAssetPath(v_object.transform.root.gameObject);
					if (!string.IsNullOrEmpty(v_assetPath))
					{
						continue;
					}
					v_finalList.Add(v_object);
				}
			}
			v_arrayReturn = v_finalList.ToArray();
#endif
		}
		else 
		{
			GameObject[] v_allActiveObjects = Object.FindObjectsOfType<GameObject>();
			List<GameObject> v_rootObjectsList = new List<GameObject>();
			foreach (GameObject v_object in v_allActiveObjects)
			{
				if(v_object.transform.parent == null)
				{
					v_rootObjectsList.Add(v_object);
				}
			}
			foreach (GameObject v_object in v_rootObjectsList)
			{
				if(!v_finalList.Contains(v_object))
					v_finalList.Add(v_object);
				GetAllGameObjects(v_object, ref v_finalList);
			}
			v_arrayReturn = v_finalList.ToArray();
		}
		
		return v_arrayReturn;
	}
	
	public static TypeBehaviour[] FindAllComponentsOfType<TypeBehaviour>() where TypeBehaviour : Component
	{
		return FindAllComponentsOfType<TypeBehaviour>(true);
	}
	
	public static TypeBehaviour[] FindAllComponentsOfType<TypeBehaviour>(bool p_includeInactive) where TypeBehaviour : Component
	{
		List<TypeBehaviour> v_finalList = new List<TypeBehaviour>();
		TypeBehaviour[] v_arrayReturn = v_finalList.ToArray();

		if(!p_includeInactive)
		{
			v_arrayReturn = Object.FindObjectsOfType<TypeBehaviour>();
		}
		/*else if (Application.isEditor)
		{
#if UNITY_EDITOR
			TypeBehaviour[] v_allObjects = (TypeBehaviour[])Resources.FindObjectsOfTypeAll(typeof(TypeBehaviour));

			foreach (TypeBehaviour v_behaviour in v_allObjects)
			{	
				if (v_behaviour.hideFlags == HideFlags.None)
				{
					string v_assetPath = AssetDatabase.GetAssetPath(v_behaviour.transform.root.gameObject);
					if (!string.IsNullOrEmpty(v_assetPath))
					{
						continue;
					}
					v_finalList.Add(v_behaviour);
				}
			}
			v_arrayReturn = v_finalList.ToArray();
#endif
		}*/
		else 
		{
			GameObject[] v_allActiveObjects = Object.FindObjectsOfType<GameObject>();
			List<GameObject> v_rootObjectsList = new List<GameObject>();
			foreach (GameObject v_object in v_allActiveObjects)
			{
				if(v_object.transform.parent == null)
				{
					v_rootObjectsList.Add(v_object);
				}
			}
			foreach (GameObject v_object in v_rootObjectsList)
			{
				v_finalList.AddRange(v_object.GetComponentsInChildren<TypeBehaviour>(true));
				TypeBehaviour v_behaviour = v_object.GetComponent<TypeBehaviour>();
				if(v_behaviour != null && !v_finalList.Contains(v_behaviour))
				{
					v_finalList.Add(v_behaviour);
				}
			}
			v_arrayReturn = v_finalList.ToArray();
		}

		return v_arrayReturn;
	}
	
	public static T[] FindAllAssets<T>() where T : Object
	{
		//Force Load All Resources at ResourcesFolder
		Resources.LoadAll<T>("");
		//Now Assets can find Objects in Resources
		T[] v_assets = (T[])Resources.FindObjectsOfTypeAll(typeof(T));
		return v_assets;
	}

	public static GameObject[] FindAllPrefabs()
	{
		List<GameObject> v_finalList = new List<GameObject>();
		GameObject[] v_arrayReturn = v_finalList.ToArray();
		if (Application.isEditor)
		{
#if UNITY_EDITOR
			GameObject[] v_allObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
			foreach (GameObject v_object in v_allObjects)
			{	
				if (v_object.hideFlags == HideFlags.None)
				{
					string v_assetPath = AssetDatabase.GetAssetPath(v_object.transform.root.gameObject);
					if (!string.IsNullOrEmpty(v_assetPath))
					{
						v_finalList.Add(v_object);
					}
				}
			}
			v_arrayReturn = v_finalList.ToArray();
#endif
		}
		else
		{
			List<GameObject> v_allObjectsList = new List<GameObject>((GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)));
			GameObject[] v_sceneObjects = FindAllGameObjects();
			foreach (GameObject v_object in v_sceneObjects)
			{
				if(!v_allObjectsList.Contains(v_object))
				{
					v_finalList.Add(v_object);
				}
			}
			v_arrayReturn = v_finalList.ToArray();
		}
		return v_arrayReturn;
	}

	public static TypeBehaviour[] FindAllResourcesComponentsOfType<TypeBehaviour>() where TypeBehaviour : Component
	{
		List<TypeBehaviour> v_finalList = new List<TypeBehaviour>();
		TypeBehaviour[] v_arrayReturn = v_finalList.ToArray();

		v_finalList = new List<TypeBehaviour>((TypeBehaviour[])Resources.FindObjectsOfTypeAll(typeof(TypeBehaviour)));
		TypeBehaviour[] v_sceneObjects = FindAllComponentsOfType<TypeBehaviour>();
		foreach (TypeBehaviour v_behaviour in v_sceneObjects)
		{
			v_finalList.RemoveChecking(v_behaviour);
		}
		v_arrayReturn = v_finalList.ToArray();

		return v_arrayReturn;
	}

	public static T[] FindAllAssetsAtFolder<T>(string p_folder = "") where T : Object
	{
		List<T> v_finalList = new List<T>();
		Object[] v_objects = Resources.LoadAll(p_folder);
		foreach(Object v_object in v_objects)
		{
			T v_asset = v_object as T;
			if(v_asset != null)
				v_finalList.AddChecking(v_asset);
		}
		return v_finalList.ToArray();
	}

	//Load All Files at Folder inside Resources Folder
	public static GameObject[] FindAllPrefabsAtFolder(string p_folder = "", string p_tagToAvoidObject = "AvoidByResourcesLoadAll")
	{
		List<GameObject> v_finalList = new List<GameObject>();
		Object[] v_objects = Resources.LoadAll(p_folder);
		foreach(Object v_object in v_objects)
		{
			GameObject v_gameObject = v_object as GameObject;
			try
			{
				if(v_gameObject != null && (string.IsNullOrEmpty(p_tagToAvoidObject) || string.IsNullOrEmpty(v_gameObject.tag) || !v_gameObject.tag.Contains(p_tagToAvoidObject)))
				{
					v_finalList.AddChecking(v_object as GameObject);
				}
			}
			catch{}
		}
		return v_finalList.ToArray();
	}

	public static TypeBehaviour[] FindAllResourcesComponentsAtFolder<TypeBehaviour>(string p_folder = "", string p_tagToAvoidObject = "AvoidByResourcesLoadAll") where TypeBehaviour : Component
	{
		List<GameObject> v_finalObjectList = new List<GameObject>(FindAllPrefabsAtFolder(p_folder, p_tagToAvoidObject));
		List<TypeBehaviour> v_finalComponents = GetListOfComponentsFromListOfGameObjects<TypeBehaviour>(v_finalObjectList);
		return v_finalComponents.ToArray();
	}

#endregion

#region Show Panel Methods

	public static bool IsChildObject(GameObject p_possibleParent, GameObject p_child, bool p_includeSelf = false)
	{
		bool v_isChild = false;
		if(p_child != null && p_possibleParent != null)
		{
			if(p_includeSelf && p_possibleParent == p_child)
				v_isChild = true;
			if(!v_isChild)
			{
				FocusContainer[] v_focusContainares = p_child.GetComponentsInParent<FocusContainer>();
				foreach(FocusContainer v_cont in v_focusContainares)
				{
					if(v_cont != null && v_cont.gameObject == p_possibleParent)
					{
						v_isChild = true;
						break;
					}
				}
			}
		}
		return v_isChild;
	}

	public static void SetContainerVisibility(GameObject p_object , ShowObjectActionEnum p_action, float p_time, bool p_canCreateNewGlobalSheduler = true)
	{
		if(p_object != null)
		{
			if(p_time > 0)
			{
				List<object> v_params = new List<object>();
				v_params.Add(p_object);
				v_params.Add(p_action);
				GlobalScheduler.CallStaticFunction(new Delegates.FunctionPointer<GameObject, ShowObjectActionEnum>(SetContainerVisibility),v_params, p_time, p_canCreateNewGlobalSheduler);
			}
			else
			{
				SetContainerVisibility(p_object , p_action);
			}
		}
	}
	
	public static void SetContainerVisibility(ScheduledContainer p_panel , ShowObjectActionEnum p_action, float p_time, bool p_canCreateNewGlobalSheduler = true)
	{
		if(p_panel != null)
		{
			if(p_time > 0)
			{
				List<object> v_params = new List<object>();
				v_params.Add(p_panel);
				v_params.Add(p_action);
				GlobalScheduler.CallStaticFunction(new Delegates.FunctionPointer<ScheduledContainer, ShowObjectActionEnum>(SetContainerVisibility),v_params, p_time,p_canCreateNewGlobalSheduler);
			}
			else
			{
				SetContainerVisibility(p_panel , p_action);
			}
		}
	}

	public static void SetContainerVisibility(GameObject p_object , ShowObjectActionEnum p_action)
	{
		if(p_object != null)
		{
			ScheduledContainer v_panel = p_object.GetComponent<ScheduledContainer>();
			if(v_panel != null)
			{
				SetContainerVisibility(v_panel , p_action);
			}
			else
			{
				if(p_action == ShowObjectActionEnum.Show || p_action == ShowObjectActionEnum.ShowFinish)
				{
					p_object.SetActive(true);
				}
				else
				{
					p_object.SetActive(false);
				}
			}
		}
	}

	public static void SetContainerVisibility(ScheduledContainer p_panel , ShowObjectActionEnum p_action)
	{
		if(p_panel != null)
		{
			if(p_action == ShowObjectActionEnum.Show)
			{
				p_panel.Show(false);
			}
			else if (p_action == ShowObjectActionEnum.ShowFinish)
			{
				p_panel.Show(true);
			}
			else if (p_action == ShowObjectActionEnum.Hide)
			{
				p_panel.Hide(false);
			}
			else 
			{
				p_panel.Hide(true);
			}
		}
	}

	public static PanelStateEnum GetContainerVisibilityInHierarchy(GameObject p_object)
	{
		if(p_object != null)
		{
			if(!p_object.activeInHierarchy)
				return PanelStateEnum.Closed;
			else
			{
				ScheduledContainer[] v_componentsInParent = p_object.GetComponentsInParent<ScheduledContainer>();
				PanelStateEnum v_return = PanelStateEnum.Opening;
				foreach(ScheduledContainer v_cont in v_componentsInParent)
				{
					if(v_cont != null && v_cont.enabled && (v_cont.PanelState == PanelStateEnum.Closed || v_cont.PanelState == PanelStateEnum.Closing))
						v_return = v_cont.PanelState;
				}
				if(v_return != PanelStateEnum.Closed && v_return != PanelStateEnum.Closing)
				   v_return = GetContainerVisibility(p_object);
				return v_return;
			}
		}
		return PanelStateEnum.Closed;
	}

	public static PanelStateEnum GetContainerVisibilityInHierarchy(ScheduledContainer p_panel)
	{
		if(p_panel != null)
		{
			return GetContainerVisibilityInHierarchy(p_panel.gameObject);
		}
		return PanelStateEnum.Closed;
	}

	public static PanelStateEnum GetContainerVisibility(GameObject p_object)
	{
		if(p_object != null)
		{
			ScheduledContainer v_panel = p_object.GetComponent<ScheduledContainer>();
			if(v_panel != null)
			{
				return GetContainerVisibility(v_panel);
			}
			else
			{
				return p_object.activeSelf? PanelStateEnum.Opened : PanelStateEnum.Closed;
			}
		}
		return PanelStateEnum.Closed;
	}

	public static PanelStateEnum GetContainerVisibility(ScheduledContainer p_panel)
	{
		if(p_panel != null)
		{
			return p_panel.PanelState;
		}
		return PanelStateEnum.Closed;
	}

#endregion

#region List Methods

	public static List<T> GetListOfComponentsFromListOfGameObjects<T>(List<GameObject> p_objectList) where T: Component
	{
		List<T> v_componentsList = new List<T>();
		if(p_objectList != null) 
		{
			foreach(GameObject v_object in p_objectList)
			{
				if(v_object != null)
				{
					T[] v_componentsArray = v_object.GetComponents<T>();
					v_componentsList.MergeList(new List<T>(v_componentsArray));
				}
			}
		}
		return v_componentsList;
	}

	public static List<GameObject> GetListOfGameObjectsFromListOfComponents<T>(List<T> p_componentsList) where T: Component
	{
		List<GameObject> v_objectList = new List<GameObject>();
		if(p_componentsList != null) 
		{
			foreach(T v_component in p_componentsList)
			{
				if(v_component != null)
					v_objectList.AddChecking(v_component.gameObject);
			}
		}
		return v_objectList;
	}

#endregion

#region RigidBody Methods

	public static float GetObjectMass(GameObject p_object, bool p_includeChildrens = true , bool p_multiplyByGravityScale = true)
	{
		float v_objectMass = 0;
		if(p_object != null)
		{
			List<Rigidbody2D> v_objectRigidBodysList = p_includeChildrens? new List<Rigidbody2D>(p_object.GetNonMarkedComponentsInChildren<Rigidbody2D>(false, false)) : new List<Rigidbody2D>();
			v_objectRigidBodysList.AddChecking(p_object.GetNonMarkedComponent<Rigidbody2D>());
			foreach(Rigidbody2D v_body in v_objectRigidBodysList)
			{
				if(v_body != null)
					v_objectMass += p_multiplyByGravityScale? (v_body.mass * v_body.gravityScale) : v_body.mass;
			}
		}
		return v_objectMass;
	}

	public static void ClearRigidBody2D(Rigidbody2D p_body)
	{
		if(p_body != null)
		{
			bool v_oldKinematic = p_body.isKinematic;
			p_body.velocity = Vector2.zero;
			p_body.angularVelocity = 0;
			p_body.isKinematic = true;
			p_body.isKinematic = v_oldKinematic;
		}
	}

#endregion

#region Private Functions

	private static void GetAllGameObjects(GameObject p_root, ref List<GameObject> p_outList)
	{
		if(p_root != null)
		{
			foreach(Transform v_transform in p_root.transform)
			{
				if(!p_outList.Contains(v_transform.gameObject))
					p_outList.Add(v_transform.gameObject);
				GetAllGameObjects(v_transform.gameObject, ref p_outList);
			}
		}
	}

	private static void LockObjectLogic(GameObject p_object)
	{
		if(p_object != null)
		{
			p_object.hideFlags |= HideFlags.NotEditable;
			foreach (Component v_comp in p_object.GetComponents(typeof(Component)))
			{
				if (!(v_comp is Transform))
				{
					v_comp.hideFlags |= HideFlags.NotEditable;
					v_comp.hideFlags |= HideFlags.HideInHierarchy;
				}
			}
		}
	}
	
	private static void UnlockObjectLogic(GameObject p_object)
	{
		if(p_object != null)
		{
			p_object.hideFlags &= ~HideFlags.NotEditable;
			foreach (Component v_comp in p_object.GetComponents(typeof(Component)))
			{
				if (!(v_comp is Transform))
				{
					// Don't check pref key; no harm in removing flags that aren't there
					v_comp.hideFlags &= ~HideFlags.NotEditable;
					v_comp.hideFlags &= ~HideFlags.HideInHierarchy;
				}
			}
		}
	}

	private static void ProcessChild<T>(Transform p_transform, ref List<T> p_list) where T : Component
	{
		if(p_transform != null)
		{
			T v_component = p_transform.GetComponent<T>();
			if (v_component != null)
				p_list.Add(v_component);
			foreach(Transform v_child in p_transform)
				ProcessChild<T>(v_child, ref p_list);
		}
	}

#endregion

#region Destroy Methods

	public static void Destroy(Object p_object, bool p_onlyDestroyInEditor = false)
	{
        //if( Application.isPlaying == false )
        if( Application.platform != RuntimePlatform.WindowsPlayer )
            return;  // guga add to avoid bug

		Destroy(p_object, 0f, true, p_onlyDestroyInEditor);
	}

	//Delayed Destroy in Editor
	public static void Destroy(Object p_object, float p_time, bool p_ignoreTimeScale = true, bool p_onlyDestroyInEditor = false)
	{
        //if( Application.isPlaying == false )
        if( Application.platform != RuntimePlatform.WindowsPlayer )
            return;  // guga add to avoid bug

		if(p_object != null)
		{
			MarkedToDestroy v_mark = MarkedToDestroy.GetMark(p_object);
			if(v_mark == null)
			{
				GameObject v_newObject = new GameObject();
				v_mark = v_newObject.AddComponent<MarkedToDestroy>();
				v_newObject.transform.SetAsFirstSibling();
			}
			v_mark.Target = p_object;
			v_mark.OnlyDestroyInEditor = p_onlyDestroyInEditor;
			v_mark.DestroyOnStart = false;
			v_mark.TimeToDestroy = p_time;
			v_mark.IgnoreTimeScale = p_ignoreTimeScale;
		}
	}

	//Destroy Faster Than Simple Destroy (Works in Editor too)
	public static void DestroyImmediate(Object p_object, bool p_onlyDestroyInEditor = false)
	{
        //if( Application.isPlaying == false )
        if( Application.platform != RuntimePlatform.WindowsPlayer )
            return;  // guga add to avoid bug

		if(p_object != null)
		{
			KiltUtils.Destroy(p_object, p_onlyDestroyInEditor);
			MarkedToDestroy v_mark = MarkedToDestroy.GetMark(p_object);
			if(v_mark != null)
				v_mark.DestroyOnStart = true;
		}
	}

#endregion

#region Lock Methods
	
	public static void LockObject(GameObject p_object, bool p_includeChildrens = true)
	{
		if(p_object != null)
		{
			Stack<GameObject> v_objectsToLock = new Stack<GameObject>();
			v_objectsToLock.Push(p_object);
			while (v_objectsToLock.Count > 0)
			{
				var v_objectInStack = v_objectsToLock.Pop();
				LockObjectLogic(v_objectInStack);
				if(p_includeChildrens)
				{
					foreach (Transform v_childTransform in v_objectInStack.transform)
						v_objectsToLock.Push(v_childTransform.gameObject);
				}
			}
		}
	}

	public static void UnlockObject(GameObject p_object, bool p_includeChildrens = true)
	{
		if(p_object != null)
		{
			Stack<GameObject> v_objectsToUnlock = new Stack<GameObject>();
			v_objectsToUnlock.Push(p_object);

			while (v_objectsToUnlock.Count > 0)
			{
				var v_objectInStack = v_objectsToUnlock.Pop();
				UnlockObjectLogic(v_objectInStack);
				if(p_includeChildrens)
				{
					foreach (Transform v_childTransform in v_objectInStack.transform)
						v_objectsToUnlock.Push(v_childTransform.gameObject);
				}
			}
		}
	}

#endregion

#region Prefab Utils

	public static string GetPossiblePrefabNameInFolder(string p_assetPath, string p_preferedName)
	{
		if(string.IsNullOrEmpty(p_assetPath))
			p_assetPath = "Assets/";

		string v_newName = p_preferedName;
		string v_fullAssetPath = PathCombine(Application.dataPath.Replace("Assets", ""), p_assetPath);
		string v_fullFilePath = PathCombine(v_fullAssetPath, v_newName + ".prefab");
		int v_counter = 1;
		while(System.IO.File.Exists(v_fullFilePath))
		{
			v_newName = p_preferedName + "(" + v_counter + ")";
			v_fullFilePath = PathCombine(v_fullAssetPath, v_newName + ".prefab");
			v_counter++;
		}
		return v_newName;
	}
	
	public static GameObject RenamePrefab(GameObject p_oldPrefab, string p_newNameWithoutExtension)
	{
		GameObject v_newPrefab = null;
#if UNITY_EDITOR
		try
		{
			if(!string.IsNullOrEmpty(p_newNameWithoutExtension) && KiltUtils.IsPrefab(p_oldPrefab, PrefabSearchMethodEnum.Full))
			{
				string v_assetPath = AssetDatabase.GetAssetPath(p_oldPrefab);
				string v_fileNameWithExtension = KiltUtils.GetFileName(v_assetPath, true);
				v_assetPath = v_assetPath.Replace(v_fileNameWithExtension, "");
				if(string.IsNullOrEmpty(v_assetPath))
				   v_assetPath = "Assets/";

				p_newNameWithoutExtension = GetPossiblePrefabNameInFolder(v_assetPath, p_newNameWithoutExtension);
				v_newPrefab = PrefabUtility.CreatePrefab(v_assetPath + p_newNameWithoutExtension + ".prefab", p_oldPrefab);

				//Destroy File
				string v_fullAssetPath = PathCombine(Application.dataPath.Replace("Assets", ""), v_assetPath);
				string v_oldFilePath = PathCombine(v_fullAssetPath, p_oldPrefab.name + ".prefab");
				string v_oldMetaFilePath = PathCombine(v_fullAssetPath, p_oldPrefab.name + ".prefab.meta");
				Object.DestroyImmediate(p_oldPrefab, true);
				EditorUtility.UnloadUnusedAssetsImmediate();
				if(System.IO.File.Exists(v_oldFilePath))
					System.IO.File.Delete(v_oldFilePath);
				if(System.IO.File.Exists(v_oldMetaFilePath))
					System.IO.File.Delete(v_oldMetaFilePath);
				AssetDatabase.Refresh();
				EditorApplication.RepaintProjectWindow();
			}
		}
		catch(System.Exception p_exception)
		{
			Debug.Log(p_exception.Message);
		}
#endif
		return v_newPrefab;
	}
	
	//Quick Search: fastest method that can return false positive values(this mode dont try find in hierarchy and can return false positives if object dont have parent)
	//AOS (Active-Only-Seach): Slow method that can return false positives when object is inactive (Very Small Chance)
	//Full: 100% chance to return real values but it is very slow
	public static bool IsPrefab(GameObject p_object, PrefabSearchMethodEnum p_searchMethod = PrefabSearchMethodEnum.Quick)
	{
		if(p_object != null)
		{
#if UNITY_EDITOR
			PrefabType v_prefabType = PrefabUtility.GetPrefabType(p_object);
			if(v_prefabType == PrefabType.Prefab || v_prefabType == PrefabType.ModelPrefab)
				return true;
#endif

			if(p_searchMethod != PrefabSearchMethodEnum.Quick)
			{
				bool v_includeInactives = p_searchMethod == PrefabSearchMethodEnum.AOS? false : true;
				List<GameObject> v_objectsInScene = new List<GameObject>(FindAllGameObjects(v_includeInactives));
				if(v_objectsInScene.Contains(p_object))
					return false;
			}
			if(Application.isPlaying)
			{
				//Try Solve By Sibling Solution(Creating a tempObject prevent to object be alone in tree and return false positives in simbling)
				bool p_returnSimbling = false;
				GameObject v_tempObject = new GameObject();
				v_tempObject.transform.SetAsFirstSibling();
				if (p_object.transform.root.GetSiblingIndex() == 0) 
					p_returnSimbling = true;
				Object.Destroy(v_tempObject);
				if(p_returnSimbling)
					return true;
			}
			else
			{
#if UNITY_EDITOR

				bool p_returnEditor = false;
				Object v_oldActiveObject = Selection.activeObject;
				Selection.activeTransform = p_object.transform;
				PrefabUtility.GetPrefabType(p_object.transform.root);
				if(PrefabUtility.GetPrefabParent(p_object.transform.root) == null && Selection.activeTransform == null)
					p_returnEditor = true;
				Selection.activeObject = v_oldActiveObject;
				if(p_returnEditor)
					return true;
#endif
			}
		}
		return false;
	}

#endregion

#region Type Utils

	public static bool IsSameOrSubclass(System.Type p_potentialDescendant , System.Type p_potentialBase)
	{
		if(p_potentialBase != null && p_potentialDescendant != null)
		{
			return p_potentialDescendant.IsSubclassOf(p_potentialBase)
				|| p_potentialDescendant == p_potentialBase;
		}
		return false;
	}

	public static bool IsSameOrSubClassOrImplementInterface(System.Type p_potentialDescendant , System.Type p_potentialBase)
	{
		if(p_potentialBase != null && p_potentialDescendant != null)
		{
			bool v_sucess = p_potentialBase.IsAssignableFrom(p_potentialDescendant) || (new List<System.Type>(p_potentialDescendant.GetInterfaces())).Contains(p_potentialBase);
			if(!v_sucess)
				v_sucess = IsSameOrSubclass(p_potentialDescendant , p_potentialBase);
			return v_sucess;
		}
		return false;
	}

#endregion

#region Game Object Utils

	//this Method Return True if Any of Part RendererBounds or NGUIBound is Out of Screen
	public static bool IsAnyPartOutOfScreen(GameObject p_object, bool p_rendererCheck = true, bool p_NGUICheck = true)
	{
		bool v_return = false;
		if(p_object != null)
		{
			Camera v_cameraThatDrawThisObject = CameraManager.GetCameraThatDrawLayer(p_object.layer);
			if(v_cameraThatDrawThisObject != null)
			{
				Rect v_screenRect = v_cameraThatDrawThisObject.rect;
				//Renderer
				if(p_rendererCheck)
				{
					List<SpriteRenderer> v_renderers = new List<SpriteRenderer>(p_object.GetComponents<SpriteRenderer>());
					v_renderers.MergeList(new List<SpriteRenderer>(p_object.GetComponentsInChildren<SpriteRenderer>()));
					foreach(SpriteRenderer v_renderer in v_renderers)
					{
						List<Vector2> v_boundsInPoints = new List<Vector2>();
						Vector2 v_topLeft = new Vector2(v_renderer.bounds.center.x - v_renderer.bounds.size.x/2, v_renderer.bounds.center.y - v_renderer.bounds.size.y/2);
						Vector2 v_topRight = new Vector2(v_topLeft.x + v_renderer.bounds.size.x, v_topLeft.y);
						Vector2 v_botLeft = new Vector2(v_topLeft.x, v_topLeft.y + v_renderer.bounds.size.y);
						Vector2 v_botRight = new Vector2(v_topLeft.x + v_renderer.bounds.size.x, v_topLeft.y + v_renderer.bounds.size.y);
						v_boundsInPoints.Add(v_topLeft);
						v_boundsInPoints.Add(v_topRight);
						v_boundsInPoints.Add(v_botLeft);
						v_boundsInPoints.Add(v_botRight);
						foreach(Vector2 v_point in v_boundsInPoints)
						{
							Vector2 v_viewPoint = v_cameraThatDrawThisObject.WorldToViewportPoint(v_point);
							//IsUnderView so return False
							if(!v_screenRect.Contains(v_viewPoint))
							{
								v_return = true;
								return v_return;
							}
						}
					}
				}
				//NGUI
#if NGUI_KILT_DLL
				if(p_NGUICheck)
				{
					List<UIWidget> v_widgets = new List<UIWidget>(p_object.GetComponents<UIWidget>());
					v_widgets.MergeList(new List<UIWidget>(p_object.GetComponentsInChildren<UIWidget>()));
					foreach(UIWidget v_widget in v_widgets)
					{
						List<Vector2> v_boundsInPoints = new List<Vector2>();
						Vector2 v_topLeft = new Vector2(v_widget.bounds.center.x - v_widget.bounds.size.x/2, v_widget.bounds.center.y - v_widget.bounds.size.y/2);
						Vector2 v_topRight = new Vector2(v_topLeft.x + v_widget.bounds.size.x, v_topLeft.y);
						Vector2 v_botLeft = new Vector2(v_topLeft.x, v_topLeft.y + v_widget.bounds.size.y);
						Vector2 v_botRight = new Vector2(v_topLeft.x + v_widget.bounds.size.x, v_topLeft.y + v_widget.bounds.size.y);
						v_boundsInPoints.Add(v_topLeft);
						v_boundsInPoints.Add(v_topRight);
						v_boundsInPoints.Add(v_botLeft);
						v_boundsInPoints.Add(v_botRight);
						foreach(Vector2 v_point in v_boundsInPoints)
						{
							Vector2 v_viewPoint = v_cameraThatDrawThisObject.WorldToViewportPoint(v_point);
							//IsUnderView so return False
							if(!v_screenRect.Contains(v_viewPoint))
							{
								v_return = true;
								return v_return;
							}
						}
					}
				}
#endif
			}
			else
				v_return = true; // Dont Have Camera so is out of Screen
		}
		return v_return;
	}

	//Return True if NGUI and Renderer bounds is completely out of bounds
	public static bool IsOutOfScreen(GameObject p_object, bool p_quickAlgorithm = false, bool p_rendererCheck = true, bool p_NGUICheck = true)
	{
		bool v_return = false;
		if(p_object != null)
		{
			Camera v_cameraThatDrawThisObject = CameraManager.GetCameraThatDrawLayer(p_object.layer);
			if(v_cameraThatDrawThisObject != null)
			{
				Vector2 v_positionInViewPort = v_cameraThatDrawThisObject.WorldToViewportPoint(p_object.transform.position);
				Rect v_screenRect = v_cameraThatDrawThisObject.rect;
				if(!v_screenRect.Contains(v_positionInViewPort))
				{
					if(p_quickAlgorithm)
						v_return = true;
					//Need More Complex Maths
					else
					{
						//Renderer Check
						if(p_rendererCheck)
						{
							List<SpriteRenderer> v_renderers = new List<SpriteRenderer>(p_object.GetComponents<SpriteRenderer>());
							v_renderers.MergeList(new List<SpriteRenderer>(p_object.GetComponentsInChildren<SpriteRenderer>()));
							foreach(SpriteRenderer v_renderer in v_renderers)
							{
								List<Vector2> v_boundsInPoints = new List<Vector2>();
								Vector2 v_topLeft = new Vector2(v_renderer.bounds.center.x - v_renderer.bounds.size.x/2, v_renderer.bounds.center.y - v_renderer.bounds.size.y/2);
								Vector2 v_topRight = new Vector2(v_topLeft.x + v_renderer.bounds.size.x, v_topLeft.y);
								Vector2 v_botLeft = new Vector2(v_topLeft.x, v_topLeft.y + v_renderer.bounds.size.y);
								Vector2 v_botRight = new Vector2(v_topLeft.x + v_renderer.bounds.size.x, v_topLeft.y + v_renderer.bounds.size.y);
								v_boundsInPoints.Add(v_topLeft);
								v_boundsInPoints.Add(v_topRight);
								v_boundsInPoints.Add(v_botLeft);
								v_boundsInPoints.Add(v_botRight);
								foreach(Vector2 v_point in v_boundsInPoints)
								{
									Vector2 v_viewPoint = v_cameraThatDrawThisObject.WorldToViewportPoint(v_point);
									//IsUnderView so return False
									if(v_screenRect.Contains(v_viewPoint))
									{
										v_return = false;
										return v_return;
									}
								}
							}
						}
						//NGUI Check
#if NGUI_KILT_DLL
						if(p_NGUICheck)
						{
							List<UIWidget> v_widgets = new List<UIWidget>(p_object.GetComponents<UIWidget>());
							v_widgets.MergeList(new List<UIWidget>(p_object.GetComponentsInChildren<UIWidget>()));
							foreach(UIWidget v_widget in v_widgets)
							{
								List<Vector2> v_boundsInPoints = new List<Vector2>();
								Vector2 v_topLeft = new Vector2(v_widget.bounds.center.x - v_widget.bounds.size.x/2, v_widget.bounds.center.y - v_widget.bounds.size.y/2);
								Vector2 v_topRight = new Vector2(v_topLeft.x + v_widget.bounds.size.x, v_topLeft.y);
								Vector2 v_botLeft = new Vector2(v_topLeft.x, v_topLeft.y + v_widget.bounds.size.y);
								Vector2 v_botRight = new Vector2(v_topLeft.x + v_widget.bounds.size.x, v_topLeft.y + v_widget.bounds.size.y);
								v_boundsInPoints.Add(v_topLeft);
								v_boundsInPoints.Add(v_topRight);
								v_boundsInPoints.Add(v_botLeft);
								v_boundsInPoints.Add(v_botRight);
								foreach(Vector2 v_point in v_boundsInPoints)
								{
									Vector2 v_viewPoint = v_cameraThatDrawThisObject.WorldToViewportPoint(v_point);
									//IsUnderView so return False
									if(v_screenRect.Contains(v_viewPoint))
									{
										v_return = false;
										return v_return;
									}
								}
							}
						}
#endif
						v_return = true;
					}
				}
			}
			else
				v_return = true; // Dont Have Camera so is out of Screen
		}
		return v_return;
	}

	public static void LookAt2DCoords(GameObject p_objectToRotate, GameObject p_objectToLook, float p_angleOffset)
	{
		if(p_objectToRotate != null && p_objectToLook != null)
		{
			Vector3 v_position1 = p_objectToLook.transform.position;
			Vector3 v_position2 = p_objectToRotate.transform.position;
			//Convert To ScreenPosition
			if(p_objectToLook.layer != p_objectToRotate.layer)
			{
				Camera v_camera1 = CameraManager.GetCameraThatDrawLayer(p_objectToLook.layer);
				Camera v_camera2 = CameraManager.GetCameraThatDrawLayer(p_objectToRotate.layer);
				if(v_camera1 != null && v_camera2 != null)
				{
					v_position1 = v_camera1.WorldToScreenPoint(v_position1);
					v_position2 = v_camera2.WorldToScreenPoint(v_position2);
				}
			}
			Vector2 v_dir = VectorHelper.Direction(v_position1, v_position2);
			float v_angle = Vector2.Angle(new Vector3(1,0), v_dir);
			if(v_position1.y > v_position2.y)
				v_angle *= -1;
			//Special Cases
			v_angle += p_angleOffset;
			v_angle = Mathf.RoundToInt(v_angle);
			p_objectToRotate.transform.rotation = Quaternion.AngleAxis(v_angle, new Vector3(0,0,1));
		}
	}

	//Work In Prefabs too
	public static T[] GetComponentsInChildren<T>(Transform p_transform) where T : Component
	{
		List<T> result = new List<T>();
		ProcessChild<T>(p_transform, ref result);
		return result.ToArray();
	}


	public static GameObject Instantiate(GameObject p_object, bool p_connectToPrefab = true)
	{
		GameObject v_return = null;
		if(p_object != null)
		{
			if(p_connectToPrefab)
			{
				if(Application.isEditor && !Application.isPlaying)
				{
#if UNITY_EDITOR
					if(IsPrefab(p_object) && p_object.transform.parent == null)// Dont accept prefab childrens
						v_return = UnityEditor.PrefabUtility.InstantiatePrefab(p_object) as GameObject;
#endif
				}
			}
			if(v_return == null)
				v_return = GameObject.Instantiate(p_object) as GameObject;
		}

		return v_return;
		 
	}

	public static List<List<GameObject>> GetLevelTreeFromRoot(GameObject p_root)
	{
		List<List<GameObject>> v_levelTree = new List<List<GameObject>>();
		if(p_root != null)
		{
			List<GameObject> v_currentList = new List<GameObject>();
			v_currentList.Add(p_root); // Add Self as first to check
			while(v_currentList.Count > 0)
			{
				v_levelTree.Add(v_currentList);
				List<GameObject> v_childrenList = new List<GameObject>();
				foreach(GameObject v_object in v_currentList)
					v_childrenList.MergeList(GetGameObjectDirectChildrens(v_object));
				v_currentList = v_childrenList;
			}
		}
		return v_levelTree;
	}

	public static List<GameObject> GetAllGameObjectsInRoot(GameObject p_root, bool p_includeSelf = true)
	{
		List<GameObject> v_interationList = new List<GameObject>();
		if(p_root != null)
		{
			v_interationList.Add(p_root); // Add Self as first to check
			for(int i=0; i<v_interationList.Count; i++)
				v_interationList.MergeList(GetGameObjectDirectChildrens(v_interationList[i]));
		}
		if(!p_includeSelf)
			v_interationList.RemoveChecking(p_root);
		return v_interationList;
	}

	public static List<T> GetComponentedOfTypeInRoot<T>(GameObject p_root, bool p_includeSelf = true) where T : Component
	{
		List<GameObject> v_objects = GetAllGameObjectsInRoot(p_root, p_includeSelf);
		List<T> v_return = new List<T>();
		foreach(GameObject v_object in v_objects)
		{
			if(v_object != null)
			{
				v_return.MergeList(new List<T>(v_object.GetComponents<T>()));
			}
		}
		return v_return;
	}
	
	public static List<GameObject> GetGameObjectDirectChildrens(GameObject p_root)
	{
		List<GameObject> v_childrens = new List<GameObject>();
		if(p_root != null)
		{
			foreach(Transform v_transformChildren in p_root.transform)
				v_childrens.AddChecking(v_transformChildren.gameObject);
		}
		return v_childrens;
	}

	public static GameObject[] GetChildrensWithName(GameObject p_root, string p_name)
	{
		List<GameObject> v_objectsInScene = GetAllGameObjectsInRoot(p_root, false);
		List<GameObject> v_returnList = GetGameObjectsWithNameInList(v_objectsInScene, p_name);
		return v_returnList.ToArray();
	}
	
	public static List<GameObject> GetGameObjectsWithNameInList(List<GameObject> v_interationList, string p_name)
	{
		List<GameObject> v_returnedObjects = new List<GameObject>();
		foreach(GameObject v_object in v_interationList)
		{
			if(v_object != null && v_object.name.Equals(p_name))
				v_returnedObjects.Add(v_object);
		}
		return v_returnedObjects;
	}
	
	public static bool ListContainsGameObjectWithName(List<GameObject> v_interationList, string p_name)
	{
		List<GameObject> v_listOfObjectsWithThisName = GetGameObjectsWithNameInList(v_interationList, p_name);
		if(v_listOfObjectsWithThisName.Count <= 0)
			return false;
		return true;
	}

#endregion

#region Component Utils

	public static Type GetComponentByIndex<Type>(GameObject p_object, int p_index) where Type : Component
	{
		Type v_foundedComponent = null;
		if(p_object != null)
		{
			Type[] v_components = p_object.GetComponents<Type>();
			if(v_components.Length > p_index)
				v_foundedComponent = v_components[p_index];
		}
		return v_foundedComponent;
	}
	
	public static int FindComponentIndex(Component p_component)
	{
		int v_index = -1;
		if(p_component != null && p_component.gameObject != null)
		{
			Component[] v_components = p_component.gameObject.GetComponents<Component>();
			for(int i=0; i<v_components.Length; i++)
			{
				if(v_components[i] == p_component)
				{
					v_index = i;
					break;
				}
			}
		}
		return v_index;
	}

#endregion

#region Message

	public static void SendMessage(Object p_object, string p_methodName, bool p_includeInactives, params object[] p_params)
	{
		if (p_object != null)
		{
			GameObject v_gameObject = p_object as GameObject;
			if(v_gameObject != null)
			{
				if(p_includeInactives || (v_gameObject.activeSelf && v_gameObject.activeInHierarchy))
				{
					List<Component> v_componentsToSend = ComponentsToNotify(p_object as GameObject);
					foreach(Component v_component in v_componentsToSend)
					{
						if(v_component != null)
						{
							MonoBehaviour v_behaviour = v_component as MonoBehaviour;
							if(v_behaviour == null || v_behaviour.enabled)
							{
								if(!KiltUtils.CallFunction(v_component, p_methodName, p_params))
									KiltUtils.CallFunction(v_component, p_methodName, new object[0]); //Try Call Method With No Parameters
							}
						}
					}
				}
			}
			else
				KiltUtils.CallFunction(p_object, p_methodName, p_params);
		}
	}

	static List<Component> ComponentsToNotify(GameObject go)
	{
		List<Component> v_behaviours = new List<Component>();
		if(go != null)
			v_behaviours.MergeList(new List<Component>(go.GetComponents<Component>()));
		return v_behaviours;
	}

	public static void BroadcastMessage(GameObject p_root, string p_methodName, bool p_includeInactives, params object[] p_params)
	{
		if(Application.isPlaying)
		{
			List<GameObject> v_objects = GetAllGameObjectsInRoot(p_root, true);
			foreach(GameObject v_object in v_objects)
			{
				SendMessage(v_object, p_methodName, p_includeInactives, p_params);
			}
		}
	}

	public static void BroadcastAllSceneObjects(string p_methodName, bool p_includeInactives, params object[] p_params)
	{
		if(Application.isPlaying)
		{
			GameObject[] v_objects = KiltUtils.FindAllGameObjects(p_includeInactives);
			foreach (GameObject v_object in v_objects) 
			{
				if (v_object != null && v_object.transform.parent == null) 
					BroadcastMessage(v_object, p_methodName, p_includeInactives, p_params);
			}
		}
	}

	[System.Obsolete("Use BroadcastAllSceneObjects instead")]
	public static void BroadcastAll(string p_methodName, object p_parameter = null, bool p_includeInactives = false)
	{
		if(Application.isPlaying)
		{
			GameObject[] v_objects = KiltUtils.FindAllGameObjects(p_includeInactives);
			foreach (GameObject v_object in v_objects) 
			{
				if (v_object != null && v_object.transform.parent == null) 
					v_object.BroadcastMessage(p_methodName, p_parameter, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

#endregion

#region Unique ID

	public static int GenerateUID()
	{
		int v_uid = (int)(Time.time*1000) + (int)KiltUtils.RandomRange (0,int.MaxValue/2);
		return v_uid;
	}

	const string UID_TARGET_DEVICE_KEYNAME = "UID_TARGET_DEVICE_KEYNAME";
	static string _targetDeviceUID = "";
	public static string GetTargetDeviceUID()
	{
		if(string.IsNullOrEmpty(_targetDeviceUID))
		{
			_targetDeviceUID = KeySerializer.GetString(UID_TARGET_DEVICE_KEYNAME);
			if(string.IsNullOrEmpty(_targetDeviceUID))
				_targetDeviceUID = GenerateTargetDeviceUID();
		}
		return _targetDeviceUID;
	}

	private static string GenerateTargetDeviceUID()
	{
		string v_uid = Application.platform.ToString()+ "_" + GenerateUID();
		KeySerializer.SetString(UID_TARGET_DEVICE_KEYNAME, v_uid);
		return v_uid;
	}

#endregion

#region Function Caller

	public static bool StaticFunctionExists(string p_typeName, string p_functionName)
	{
		try
		{
			return StaticFunctionExists(System.Type.GetType(p_typeName), p_functionName);
		}
		catch{}
		return false;
	}

	public static bool StaticFunctionExists(System.Type p_type, string p_functionName)
	{
		if(p_type != null)
		{
			try
			{
                System.Reflection.MethodInfo v_info = p_type.GetMethod(p_functionName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

				if(v_info != null)
				{
					return true;
				}
			}
			catch{}
		}
		return false;
	}

	public static bool FunctionExists(string p_typeName, string p_functionName)
	{
		try
		{
			return FunctionExists(System.Type.GetType(p_typeName), p_functionName);
		}
		catch{}
		return false;
	}
	
	public static bool FunctionExists(System.Type p_type, string p_functionName)
	{
		if(p_type != null)
		{
			try
			{
				System.Reflection.MethodInfo v_info = p_type.GetMethod(p_functionName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				if(v_info != null)
				{
					return true;
				}
			}
			catch{}
		}
		return false;
	}

	public static bool CallStaticFunction(string p_typeName, string p_functionName, params object[] p_param)
	{
		try
		{
			return CallStaticFunction(System.Type.GetType(p_typeName), p_functionName, p_param);
		}
		catch{}
		return false;
	}

	public static bool CallStaticFunction(System.Type p_type, string p_functionName, params object[] p_param)
	{
		if(p_type != null)
		{
			try
			{
                System.Reflection.MethodInfo v_info = p_type.GetMethod(p_functionName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

				if(v_info != null)
				{
					v_info.Invoke(null, p_param);
					return true;
				}
			}
			catch {}
		}
		return false;
	}

	public static T CallStaticFunctionWithReturn<T>(string p_typeName, string p_functionName, params object[] p_param)
	{
		return CallStaticFunctionWithReturn<T>(System.Type.GetType(p_typeName), p_functionName, p_param);
	}

	public static T CallStaticFunctionWithReturn<T>(System.Type p_type, string p_functionName, params object[] p_param)
	{
		bool p_sucess = false;
		T v_return = TryCallStaticFunctionWithReturn<T>(p_type, p_functionName, out p_sucess, p_param);
		return v_return;
	}
	
	public static T TryCallStaticFunctionWithReturn<T>(System.Type p_type, string p_functionName, out bool p_sucess, params object[] p_param)
	{
		T v_return = default(T);
		p_sucess = false;
		System.Type v_type = p_type;
		if(v_type != null)
		{
			try
			{
				System.Reflection.MethodInfo v_info = v_type.GetMethod(p_functionName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

				if(v_info != null)
				{
					v_return = (T)v_info.Invoke(null, p_param);
					p_sucess = true;
				}
			}
			catch{}
		}
		return v_return;
	}

	public static bool CallFunction(object p_instance, string p_functionName, params object[] p_param)
	{
		if(p_instance != null)
		{
			System.Type v_type = p_instance.GetType();
			return CallFunction(p_instance, v_type, p_functionName, p_param);
		}
		return false;
	}

	public static bool CallFunction(object p_instance, string p_typeName, string p_functionName, params object[] p_param)
	{
		if(p_instance != null)
		{
			System.Type v_type = System.Type.GetType(p_typeName);
			return CallFunction(p_instance, v_type, p_functionName, p_param);
		}
		return false;
	}

	public static bool CallFunction(object p_instance, System.Type p_type, string p_functionName, params object[] p_param)
	{
		if(p_instance != null)
		{
			System.Type v_type = p_type;
			if(v_type != null)
			{
				try
				{
                    System.Reflection.MethodInfo v_info = v_type.GetMethod(p_functionName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					if(v_info != null)
					{
						v_info.Invoke(p_instance, p_param);
						return true;
					}
				}
				catch{}
			}
		}
		return false;
	}

	public static bool CallEditorStaticFunction(string p_typeName, string p_functionName, params object[] p_param)
	{
#if UNITY_EDITOR
		System.Reflection.Assembly v_editorAssembly = GetEditorAssembly();
		if(v_editorAssembly != null)
		{
			p_typeName = string.IsNullOrEmpty(p_typeName)? "" : p_typeName;
			string v_fullTypeNameWithAssembly = p_typeName.Contains(v_editorAssembly.FullName)? p_typeName : p_typeName + ", " +v_editorAssembly.FullName;
			System.Type v_type = System.Type.GetType(v_fullTypeNameWithAssembly);
			return CallStaticFunction(v_type, p_functionName, p_param);
		}
#endif
		return false;
	}

	public static T CallEditorStaticFunctionWithReturn<T>(string p_typeName, string p_functionName, params object[] p_param)
	{
		bool p_sucess = false;
		T v_return = TryCallEditorStaticFunctionWithReturn<T>(p_typeName, p_functionName, out p_sucess, p_param);
		return v_return;
	}

	public static T TryCallEditorStaticFunctionWithReturn<T>(string p_typeName, string p_functionName, out bool p_sucess, params object[] p_param)
	{
		p_sucess = false;
		T v_return = default(T);
#if UNITY_EDITOR
		System.Reflection.Assembly v_editorAssembly = GetEditorAssembly();
		if(v_editorAssembly != null)
		{
			System.Type v_type = System.Type.GetType(p_typeName + ", " + v_editorAssembly.FullName);
			v_return = TryCallStaticFunctionWithReturn<T>(v_type, p_functionName, out p_sucess, p_param);
		}
#endif
		return v_return;
	}

	static System.Reflection.Assembly _editorAssembly = null;
	public static System.Reflection.Assembly GetEditorAssembly()
	{
#if UNITY_EDITOR
		if(_editorAssembly == null)
		{
			System.Reflection.Assembly[] v_assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			foreach(System.Reflection.Assembly v_assembly in v_assemblies)
			{
				if(v_assembly.FullName.Contains("Assembly-CSharp-Editor,"))
				{
					_editorAssembly = v_assembly;
					break;
				}
			}
		}
#endif
		return _editorAssembly;
	}

	static AOTDictionaryKV<System.Reflection.Assembly, System.Type[]> _assemblyTypesDict = null;
	public static AOTDictionaryKV<System.Reflection.Assembly, System.Type[]> GetAssemblyTypesDict(bool p_forceRefresh = false)
	{
		if(_assemblyTypesDict == null || p_forceRefresh)
		{
			_assemblyTypesDict = new AOTDictionaryKV<System.Reflection.Assembly, System.Type[]>();
#if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
			System.Reflection.Assembly[] v_assemblies = Kilt.RT.AppDomain.CurrentDomain.GetAssemblies();
#else
            System.Reflection.Assembly[] v_assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
#endif
			foreach(System.Reflection.Assembly v_assembly in v_assemblies)
			{
				if(v_assembly != null)
				{
#if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
					List<System.Type> v_typesList = new List<System.Type>();
					foreach(var v_typeInfo in v_assembly.DefinedTypes)
					{
						v_typesList.Add(v_typeInfo.GetType());
					}
					System.Type[] v_types = v_typesList.ToArray();
#else
                    System.Type[] v_types = v_assembly.GetTypes();
#endif
					_assemblyTypesDict.Add(v_assembly, v_types);
				}
			}
		}
		return _assemblyTypesDict;
	}

#endregion

#region Random
	
	public static int RandomRange(int p_min, int p_maxExcluding)
	{
		CheckIfNeedGenerateSeed();
		return Random.Range(p_min, p_maxExcluding);
	}

	public static float RandomRange(float p_min, float p_maxIncluding)
	{
		CheckIfNeedGenerateSeed();
		return Random.Range(p_min, p_maxIncluding);
	}

	public static string RandomString(string p_possibleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", int p_minLength = 8, int p_maxLength = 32)
	{
		string finalString = "";
		if(!string.IsNullOrEmpty(p_possibleChars))
		{
			int v_length = Mathf.Max(0, RandomRange(p_minLength, p_maxLength+1));
			char[] v_charArray = new char[v_length];
			
			for (int i = 0; i < v_charArray.Length; i++)
			{
				int v_index = RandomRange(0, p_possibleChars.Length);
				if(p_possibleChars.Length > v_index && v_index >= 0)
					v_charArray[i] = p_possibleChars[v_index];
			}
			finalString = new string(v_charArray);
		}
		return finalString;
	}

	public static string RandomString(int p_minLength, int p_maxLength)
	{
		return RandomString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", p_minLength, p_maxLength);
	}

	static bool _needGenerateSeed = true;
	public static void CheckIfNeedGenerateSeed(bool p_force = false)
	{
		if(_needGenerateSeed || p_force)
		{
			_needGenerateSeed = false;
			Random.seed = System.Environment.TickCount;
		}
	}
	
#endregion
	
#region PlayerSettings Helper
	
	static bool v_appIconNotFound = false;
	public static Texture2D GetAppIcon(string p_defaultResourcesFile = "default/drawable/app_icon")
	{
		Texture2D v_texture = null;
#if UNITY_EDITOR
		v_appIconNotFound = false;
#endif
		if(!v_appIconNotFound)
		{
#if UNITY_EDITOR
			Texture[] v_textures = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
			if(v_textures != null && v_textures.Length > 0 && v_textures[0] != null)
				v_texture = v_textures[0] as Texture2D;
#endif
			if(v_texture == null)
				v_texture = Resources.Load<Texture2D>(p_defaultResourcesFile);
			if(v_texture == null) //Try fallback to default icon
				v_texture = Resources.Load<Texture2D>("default/drawable/unity_icon");
			if(v_texture == null && Application.isPlaying && !Application.isEditor)
				v_appIconNotFound = true;
		}
		return v_texture;
	}

	const string APP_NAME_KEY = "K_APP_NAME_KEY";
	static bool v_appNameNotFound = false;
	public static string GetAppName(bool p_canSaveOrLoadFromKeySerializer = false)
	{
		string v_name = "";
#if UNITY_EDITOR
		v_appNameNotFound = false;
#endif
		if(!v_appNameNotFound)
		{
#if UNITY_EDITOR
			v_name = PlayerSettings.productName;
			if(!string.IsNullOrEmpty(v_name) && p_canSaveOrLoadFromKeySerializer)
				KeySerializer.SetBaseString(APP_NAME_KEY, v_name);
#endif
			if(p_canSaveOrLoadFromKeySerializer)
			{
				if(string.IsNullOrEmpty(v_name))
					v_name = KeySerializer.GetBaseString(APP_NAME_KEY);
			}
			if(string.IsNullOrEmpty(v_name) && Application.isPlaying && !Application.isEditor)
				v_appNameNotFound = true;
		}
		return v_name;
	}

#endregion

#region Security
	
	public static string GenerateMd5Hash(string p_string)
	{
#if !UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_WINRT
		System.Security.Cryptography.MD5 v_md5 = System.Security.Cryptography.MD5.Create();
		byte[] v_inputBytes = System.Text.Encoding.ASCII.GetBytes(p_string);
		byte[] v_hash = v_md5.ComputeHash(v_inputBytes);
		System.Text.StringBuilder v_stringBuilder = new System.Text.StringBuilder();
		for (int i = 0; i < v_hash.Length; i++) 
		{ 
			v_stringBuilder.Append(v_hash[i].ToString("X2")); 
		}
		return v_stringBuilder.ToString();
#else
		//MD5 is not available in WP8 so encode to UTF8 base64
		p_string = p_string == null? "" : p_string;
		string v_returnValue = p_string;
		try
		{
			byte[] v_toEncodeAsBytes = System.Text.Encoding.UTF8.GetBytes(p_string);
			v_returnValue = System.Convert.ToBase64String(v_toEncodeAsBytes);
		}
		catch{}
		return v_returnValue;
#endif
	}

#endregion
}
