using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class BlockSelector : MonoBehaviour 
{
	public static List<FolderPrefabs> m_folders = new List<FolderPrefabs>();
	public static List<string> m_foldersPaths = new List<string>() {"Glass", "Metal", "Wood", "Stone", "Balloon"};
	public static string m_mainFolderPath = "Prefabs";

	#region Private Variables

	[SerializeField]
	int m_selectedFolderIndex = -1;
	[SerializeField]
	int m_selectedPrefabInFolderIndex = -1;
	[SerializeField]
	bool m_hideComponentsInInspector = true;
	//[SerializeField]
	//string m_selectedFolderName = ""; // Used To check If Prefab is the correct when refresh

	#endregion

	#region Protected Properties

	[SerializeField, HideInInspector]
	GameObject _cachedPrefab = null;

	protected GameObject CachedPrefab
	{
		get
		{
			#if UNITY_EDITOR
			if(_cachedPrefab == null)
				_cachedPrefab = PrefabUtility.GetPrefabParent(this.gameObject) as GameObject;
			#endif
			return _cachedPrefab;
		}
		set
		{
			if(_cachedPrefab == value)
				return;
			_cachedPrefab = value;
		}
	}

	#endregion

	#region Public Properties

	public string SelectedFolderName  
	{
		get 
		{
			return m_foldersPaths.Count > m_selectedFolderIndex && m_selectedFolderIndex >= 0? m_foldersPaths[m_selectedFolderIndex] : "";
		} 
	}

	public string SelectedPrefabName  
	{
		get 
		{
			string v_return = SelectedObject != null? SelectedObject.name : "";
			return v_return;
		} 
	}

	public bool HideComponentsInInspector  
	{
		get {return m_hideComponentsInInspector;} 
		set 
		{
			if(m_hideComponentsInInspector == value)
				return;
			m_hideComponentsInInspector = value;
			ApplyShowHideComponent();
		}
	}

	public string MainFolderPath  
	{
		get {return m_mainFolderPath;} 
		set {m_mainFolderPath = value;}
	}

	public List<string> FoldersPath
	{
		get 
		{
			if(m_foldersPaths == null)
				m_foldersPaths = new List<string>();
			return m_foldersPaths;
		} 
		set {m_foldersPaths = value;}
	}

	public List<FolderPrefabs> Folders  
	{
		get 
		{
			if(m_folders == null)
				m_folders = new List<FolderPrefabs>();
			return m_folders;
		} 
		set {m_folders = value;}
	}

	public FolderPrefabs SelectedFolderPrefab  
	{
		get 
		{
			if(SelectedFolderIndex >= 0 && SelectedFolderIndex < Folders.Count) 
				return Folders[SelectedFolderIndex];
			return null;
		} 
	}

	public int SelectedFolderIndex  
	{
		get {return m_selectedFolderIndex;} 
		set 
		{
			m_selectedFolderIndex = value;
		}
	}

	public int SelectedPrefabInFolderIndex  
	{
		get {return m_selectedPrefabInFolderIndex;} 
		set {m_selectedPrefabInFolderIndex = value;}
	}
	

	public GameObject SelectedObject  
	{
		get 
		{
			GameObject v_object = null;
			if(SelectedFolderPrefab != null 
			   && SelectedPrefabInFolderIndex >= 0 && SelectedPrefabInFolderIndex < SelectedFolderPrefab.PrefabsInFolder.Count)
				v_object = SelectedFolderPrefab.PrefabsInFolder[SelectedPrefabInFolderIndex];

			return v_object;
		} 
	}

	#endregion

	#region Unity Functions

	protected virtual void Awake()
	{
		FillObjectsInFolderList(false, false);
		MoveComponentToFirst(this);
	}

	protected virtual void Start()
	{
		ApplyShowHideComponent();
		CorrectSelectedPrefabIndex();
	}

	#endregion

	#region Helper Functions

	public void CorrectSelectedPrefabIndex(bool p_force = false, bool p_canRefillFolders = true)
	{
		if(Application.isEditor || p_force)
		{
			if(CachedPrefab != null)
			{
				for(int i=0; i < Folders.Count; i++)
				{
					FolderPrefabs v_foldPrefab = Folders[i];
					if(v_foldPrefab != null)
					{
						for(int j=0; j < v_foldPrefab.PrefabsInFolder.Count; j++)
						{
							GameObject v_prefab = v_foldPrefab.PrefabsInFolder[j];
							if(v_prefab == CachedPrefab)
							{
								if(m_selectedFolderIndex != i)
									m_selectedFolderIndex = i;
								if(m_selectedPrefabInFolderIndex != j)
									m_selectedPrefabInFolderIndex = j;
								if(p_canRefillFolders)
								{
									string v_cachedPrefabPath = KiltUtils.PathCorrection(GetPrefabResourcesFilePath(CachedPrefab));
									string v_pathByIndex = KiltUtils.PathCorrection("Resources/" + MainFolderPath + "/" + SelectedFolderName + "/" + SelectedPrefabName + ".prefab");
									if(!v_pathByIndex.Equals(v_cachedPrefabPath))
									{
										FillObjectsInFolderList(true);
										CorrectSelectedPrefabIndex(true, false); //After refill, call it again forcing find corret index but preventing to fill again
									}
								}
								return;
							}
						}
					}
				}
			}
			else
			{
				if(m_selectedFolderIndex != -1)
					m_selectedFolderIndex = -1;
				if(m_selectedPrefabInFolderIndex != -1)
					m_selectedPrefabInFolderIndex = -1;
			}
		}
	}

	protected string GetPrefabResourcesFilePath(GameObject p_object)
	{
		string v_assetPath = "";
		if(p_object != null)
		{
			#if UNITY_EDITOR
			v_assetPath = AssetDatabase.GetAssetPath(CachedPrefab.gameObject);
			if (!string.IsNullOrEmpty(v_assetPath))
			{
				string[] v_splitAsset = System.Text.RegularExpressions.Regex.Split(v_assetPath ,"Resources");
				if(v_splitAsset.Length == 2)
				{
					v_assetPath = "Resources" + v_splitAsset[1];
				}
			}
			#endif
		}
		return v_assetPath;
	}

	public void PerformFlip(bool p_flipX, bool p_flipY)
	{
		Vector3 v_transformScale = transform.localScale;
		if(p_flipX)
			v_transformScale.x = -v_transformScale.x;
		if(p_flipY)
			v_transformScale.y = -v_transformScale.y;
		transform.localScale = v_transformScale;
		#if UNITY_EDITOR
		EditorUtility.SetDirty(this);
		#endif
	}

	public void FillObjectsInFolderList(bool p_forceRefreshFolderList = false, bool p_onlyExecuteInEditMode = true)
	{
		if(Application.isEditor || !p_onlyExecuteInEditMode)
		{
			if(Folders == null || Folders.Count <= 0 || p_forceRefreshFolderList)
			{
				Folders.Clear();
				foreach(string v_folderPath in FoldersPath)
				{
					string v_finalFolder = MainFolderPath + "/" + v_folderPath;
					FolderPrefabs v_folderPrefabStruct = new FolderPrefabs(v_folderPath);
					v_folderPrefabStruct.PrefabsInFolder = new List<GameObject>(KiltUtils.FindAllPrefabsAtFolder(v_finalFolder));
					Folders.Add(v_folderPrefabStruct);
				}

				//Re-check Index Dont Calling Changes
				m_selectedFolderIndex = FindFolderIndexByNameInList(SelectedFolderName, Folders);
				m_selectedPrefabInFolderIndex = FindPrefabIndexByNameInList(SelectedPrefabName, SelectedFolderPrefab != null? SelectedFolderPrefab.PrefabsInFolder : null);
			}
		}
	}

	public int FindFolderIndexByNameInList(string p_name, List<FolderPrefabs> p_list)
	{
		int v_index = -1;
		if(p_list != null)
		{
			for(int i=0; i<p_list.Count; i++)
			{
				FolderPrefabs v_folder = p_list[i];
				if(v_folder != null && v_folder.Folder.Equals(p_name))
				{
					v_index = i;
					break;
				}
			}
		}
		return v_index;
	}

	public int FindPrefabIndexByNameInList(string p_name, List<GameObject> p_list)
	{
		int v_index = -1;
		if(p_list != null)
		{
			for(int i=0; i<p_list.Count; i++)
			{
				GameObject v_object = p_list[i];
				if(v_object != null && v_object.name.Equals(p_name))
				{
					v_index = i;
					break;
				}
			}
		}
		return v_index;
	}

	public GameObject InstantiateObjectByPrefab(GameObject p_prefab, bool p_keepLifePercert = false, bool p_destroyOldObject = true, bool p_keepEffectProperties = true)
	{
		if(p_prefab != null)
		{
			CachedPrefab = p_prefab;
			GameObject v_newObject = KiltUtils.Instantiate(p_prefab);
			v_newObject.name = p_prefab.name+"(Selected)";
			v_newObject.transform.parent = this.transform.parent;
			v_newObject.transform.position = this.transform.position;
			v_newObject.transform.rotation = this.transform.rotation;

			float v_signX = Mathf.Sign(this.transform.localScale.x);
			float v_signY = Mathf.Sign(this.transform.localScale.y);

			Vector3 v_localScale = new Vector3(p_prefab.transform.localScale.x*v_signX, p_prefab.transform.localScale.y*v_signY, p_prefab.transform.localScale.z);
			v_newObject.transform.localScale = v_localScale;
			if(p_keepLifePercert)
				CopyDamagePercentToNewObject(v_newObject);
			//MergeGameObjectWithThisObject(p_prefab);
			BlockSelector v_newComponent = CopyThisComponent(v_newObject);
			//Move New Component to first in new prefab
			MoveComponentToFirst(v_newComponent);
			//Plug Ropes To New Component
			AttachAllRopesToNewObject(v_newObject);
			//Select New Object in Editor Selection (if old object is previous selected and will be destroyed)
			if(p_destroyOldObject)
				CheckIfNeedSelect(v_newObject);
			//Apply Modifications in Cloned Component
			ApplyModificationsInEditMode(v_newComponent);
			ApplyShowHideComponent();

			if(p_destroyOldObject)
			{
				if(Application.isPlaying)
					GameObject.Destroy(this.gameObject);
				else
					KiltUtils.Destroy(this.gameObject);
			}

			return v_newObject;
		}
		return this.gameObject;
	}

	private void CopyDamagePercentToNewObject(GameObject p_newObject)
	{
		if(p_newObject != null)
		{
			Block v_newBlock = p_newObject.GetComponent<Block>();
			Block v_thisBlock = this.GetComponent<Block>();
			if(v_newBlock != null && v_thisBlock != null)
			{
				float v_percent = v_thisBlock.MaxLife != 0? Mathf.Clamp(v_thisBlock.CurrentLife/v_thisBlock.MaxLife, 0, 1) : 0;
				v_newBlock.CurrentLife = v_percent * v_newBlock.MaxLife;
			}
		}
	}

	private void AttachAllRopesToNewObject(GameObject p_newObject)
	{
		if(p_newObject != null && p_newObject.GetComponent<Rigidbody2D>() != null && this.GetComponent<Rigidbody2D>() != null)
		{
			if(Application.isPlaying)
			{
				RopesAttached v_attachedComponent = GetComponent<RopesAttached>();
				if(v_attachedComponent != null)
				{
					foreach(Rope2D v_rope in v_attachedComponent.PluggedRopes)
					{
						if(v_rope.GetRigidBody2DFromObject(v_rope.ObjectA) == this.GetComponent<Rigidbody2D>())
							v_rope.ObjectA = p_newObject;
						if(v_rope.GetRigidBody2DFromObject(v_rope.ObjectB) == this.GetComponent<Rigidbody2D>())
							v_rope.ObjectB = p_newObject;
					}
				}
			}
			else
			{
				Rope2D[] v_arrayOfRopes = KiltUtils.FindAllComponentsOfType<Rope2D>(true);
				foreach(Rope2D v_rope in v_arrayOfRopes)
				{
					bool v_changed = false;
					if(v_rope.GetRigidBody2DFromObject(v_rope.ObjectA) == this.GetComponent<Rigidbody2D>())
					{
						v_changed = true;
						v_rope.ObjectA = p_newObject;
					}
					if(v_rope.GetRigidBody2DFromObject(v_rope.ObjectB) == this.GetComponent<Rigidbody2D>())
					{
						v_changed = true;
						v_rope.ObjectB = p_newObject;
					}

					if(v_changed)
						ApplyModificationsInEditMode(v_rope);
				}
			}
		}
	}

	private void ApplyModificationsInEditMode(Object p_object)
	{
		#if UNITY_EDITOR
		if(p_object != null)
			UnityEditor.EditorUtility.SetDirty(p_object);
		#endif
	}

	private void CheckIfNeedSelect(GameObject p_object)
	{
		#if UNITY_EDITOR
		if(p_object != null && UnityEditor.Selection.activeTransform == this.transform)
			UnityEditor.Selection.activeTransform = p_object.transform;
		#endif
	}

	public virtual void ApplyShowHideComponent()
	{
		if(m_hideComponentsInInspector)
			HideAllComponentsInInspector();
		else
			ShowAllComponentsInInspector();
	}

	private void HideAllComponentsInInspector()
	{
		Component[] v_components = GetComponents<Component>();
		foreach(Component v_component in v_components)
		{
			if(v_component != null && v_component != this && v_component != this.transform)
			{
				//Dont Hide Special Properties
				if(v_component is BlockSpecialProperty)
				{
					v_component.hideFlags &= ~HideFlags.HideInInspector;
					v_component.hideFlags &= ~HideFlags.NotEditable;
				}
				else
				{
					v_component.hideFlags |= HideFlags.HideInInspector;
					v_component.hideFlags |= HideFlags.NotEditable;
				}
			}
		}
		foreach(Transform v_child in transform)
		{
			if(v_child != null)
				KiltUtils.LockObject(v_child.gameObject);
		}
	}

	private void ShowAllComponentsInInspector()
	{
		Component[] v_components = GetComponents<Component>();
		foreach(Component v_component in v_components)
		{
			if(v_component != null && v_component != this && v_component != this.transform)
			{
				v_component.hideFlags &= ~HideFlags.HideInInspector;
				v_component.hideFlags &= ~HideFlags.NotEditable;
			}
		}
		foreach(Transform v_child in transform)
		{
			if(v_child != null)
				KiltUtils.UnlockObject(v_child.gameObject);
		}
	}

	//Scripts must be in last Position before destroyimmediate object to not throw exception
	private void MoveComponentToFirst(Component p_component)
	{
		#if UNITY_EDITOR && !UNITY_5
		if(p_component != null)
		{
			bool v_canMoveDownAgain = true;
			while(v_canMoveDownAgain)
				v_canMoveDownAgain = UnityEditorInternal.ComponentUtility.MoveComponentUp(p_component);
		}
		
		#endif
	}
	
	private void MoveComponentToLast(Component p_component)
	{
		#if UNITY_EDITOR
		if(p_component != null)
		{
			bool v_canMoveDownAgain = true;
			while(v_canMoveDownAgain)
				v_canMoveDownAgain = UnityEditorInternal.ComponentUtility.MoveComponentDown(p_component);
		}

		#endif
	}

	private BlockSelector CopyThisComponent(GameObject p_target)
	{
		BlockSelector v_newComponent = null;
		if(p_target != null)
			v_newComponent = KiltUtils.CopyComponent<BlockSelector>(p_target, this);
		return v_newComponent;
	}

	
	#endregion

	#region Editor

	#if UNITY_EDITOR

	[MenuItem("KILT/Create/Block")]
	public static void CreateBlockMenuItem()
	{
		GameObject v_parentObject = GameObject.Find("BlocksContainer");
		GameObject v_selector = new GameObject("(Selected)");
		if(v_parentObject != null)
			v_selector.transform.parent = v_parentObject.transform;
		v_selector.transform.localScale = Vector2.one;
		v_selector.transform.position = Vector2.zero;
		v_selector.transform.localPosition = new Vector3(v_selector.transform.localPosition.x, v_selector.transform.localPosition.y, 0); // Prevent Rope Collider to be in front of this Object

		BlockSelector v_component = v_selector.AddComponent<BlockSelector>();
		v_component.FillObjectsInFolderList(true);
		v_component.m_selectedFolderIndex = 0;
		v_component.SelectedPrefabInFolderIndex = 0;
		v_selector = v_component.InstantiateObjectByPrefab(v_component.SelectedObject);
		Selection.activeTransform = v_selector.transform;
	}

	[MenuItem("KILT/Version Controller/Revert All (Block Selector)")]
	public static void RecreateAllBlockSelectors()
	{
		BlockSelector[] v_selectorsInScene = KiltUtils.FindAllComponentsOfType<BlockSelector>();
		foreach(BlockSelector v_selector in v_selectorsInScene)
		{
			if(v_selector != null)
			{
				v_selector.CorrectSelectedPrefabIndex();
				if(v_selector.SelectedObject != null)
					v_selector.InstantiateObjectByPrefab(v_selector.SelectedObject,true,true,true);
				else
					Debug.Log("Fail Update Object with name \'" + v_selector.name + "\'. Select this object and update by yourself.");
			}
		}
	}

	#endif

	#endregion
}

[SerializeField]
public class FolderPrefabs
{
	#region Private Variables

	[SerializeField]
	string m_folder = "";
	[SerializeField]
	List<GameObject> m_prefabsInFolder = new List<GameObject>();

	#endregion

	#region Public Variables

	public string Folder { get {return m_folder;} set {m_folder = value;}}
	public List<GameObject> PrefabsInFolder { get {return m_prefabsInFolder;} set {m_prefabsInFolder = value;}}

	#endregion

	#region Contructors

	public FolderPrefabs()
	{
	}

	public FolderPrefabs(string p_folder)
	{
		m_folder = p_folder;
	}

	#endregion
}
