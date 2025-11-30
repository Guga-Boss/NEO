using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlobalPressController : MonoBehaviour 
{
	#region Singleton
	
	private static GlobalPressController m_instance = null;
	
	public static GlobalPressController Instance
	{
		get
		{
			if( m_instance == null )
			{
				m_instance = GameObject.FindObjectOfType(typeof(GlobalPressController)) as GlobalPressController;
			}
			
			return m_instance;
		}
		protected set
		{
			m_instance = value;
		}
	}
	
	#endregion

	#region Events

	public static event Delegates.EventHandler<bool> OnGlobalPress;
	public static event Delegates.EventHandler<Vector2> OnGlobalDrag;
	public static event Delegates.EventHandler<GameObject> OnGlobalDrop;

	#endregion

	#region Private Functions

	[SerializeField]
	float m_pressRadius = 0.06f; // In Global Distance
	[SerializeField]
	GameObject m_clickEffect = null;
	[SerializeField]
	GameObject m_trailEffect = null;
	[SerializeField]
	string m_clickEffectCameraLayerName = "ClickEffectLayer";
	[SerializeField]
	bool m_showDebug = false;

	Camera _clickEffectCamera = null;	
	GameObject _effectInScene;
	GameObject _trailEffectInScene;
	#endregion

	#region Public Functions

	public float PressRadius {get {return m_pressRadius;} set {m_pressRadius= value;}}
	public GameObject ClickEffect {get {return m_clickEffect;} set {m_clickEffect= value;}}
	public GameObject TrailEffect {get {return m_trailEffect;} set {m_trailEffect= value;}}
	public bool ShowDebug {get {return m_showDebug;} set {m_showDebug= value;}}
	public string ClickEffectCameraLayerName {get {return m_clickEffectCameraLayerName;} set {m_clickEffectCameraLayerName= value;}}

	public Camera ClickEffectCamera 
	{
		get 
		{
			if(_clickEffectCamera == null)
			{
				_clickEffectCamera = GetComponent<Camera>();
				if(_clickEffectCamera == null)
				{
					_clickEffectCamera = GetComponentInChildren<Camera>();
					if(_clickEffectCamera == null)
					{
						GameObject v_clickEffectCameraObject = new GameObject(ClickEffectCameraLayerName);
						v_clickEffectCameraObject.layer = Mathf.Clamp(LayerMask.NameToLayer(ClickEffectCameraLayerName), 0 ,31);
						v_clickEffectCameraObject.transform.parent = this.transform;
						v_clickEffectCameraObject.transform.position = new Vector2(0,0);
						_clickEffectCamera = v_clickEffectCameraObject.AddComponent<Camera>();
					}
				}
				_clickEffectCamera.orthographic = true;
				_clickEffectCamera.clearFlags = CameraClearFlags.Depth;
				_clickEffectCamera.cullingMask = 1 << LayerMask.NameToLayer(ClickEffectCameraLayerName);
				_clickEffectCamera.depth = 999;
				_clickEffectCamera.orthographicSize = 1;
				_clickEffectCamera.nearClipPlane = -2;
				_clickEffectCamera.farClipPlane = 2;
			}
			return _clickEffectCamera;
		}
	}
	#endregion

	#region Unity Functions

	protected virtual void Awake()
	{
		RegistedEvents();
	}

	protected virtual void Start()
	{
		ShowTrailEffect(); //Force Create Trail Effect Before Any Click
	}

	protected virtual void OnDestroy()
	{
		UnregisterEvents();
	}
	
	protected virtual void OnLevelWasLoaded()
	{
		if(m_instance != this && m_instance != null)
			Object.Destroy(this.gameObject);
		else
		{
			if(m_instance == null)
				Instance = this;
		}
	}

	#endregion

	#region Helper Functions

	protected virtual void RegistedEvents()
	{
		UnregisterEvents();
		//#if NGUI_KILT_DLL
		KiltUICamera.OnGlobalPress += HandleOnGlobalPress;
		KiltUICamera.OnGlobalDrag += HandleOnGlobalDrag;
		KiltUICamera.OnGlobalDrop += HandleOnGlobalDrop;
		//#endif
	}

	protected virtual void UnregisterEvents()
	{
		//#if NGUI_KILT_DLL
		KiltUICamera.OnGlobalPress -= HandleOnGlobalPress;
		KiltUICamera.OnGlobalDrag -= HandleOnGlobalDrag;
		KiltUICamera.OnGlobalDrop -= HandleOnGlobalDrop;
		//#endif
	}

	protected virtual GameObject FindCloseObjectInRange(Vector2 p_globalPoint, float p_radius)
	{
		GameObject v_returnObject = null;
		if(PressRadius > 0)
		{
			float v_currentDistance = 9999999f;
			float v_angleStep = 10f;
			for(float j=0; j<360.0f; j+= v_angleStep)
			{
				Vector3 v_rotatedVector = VectorHelper.RotateZ(new Vector2(1,0),j);
				RaycastHit2D v_hit = Physics2D.Raycast(p_globalPoint, v_rotatedVector, p_radius);
				if(v_hit.transform != null && v_hit.transform.gameObject != null)
				{
					if(v_hit.distance < v_currentDistance)
					{
						v_returnObject = v_hit.transform.gameObject;
						v_currentDistance = v_hit.distance;
					}
				}
			}
		}
		return v_returnObject;
	}

	protected void ShowEffect()
	{
		int v_layer =  Mathf.Clamp(LayerMask.NameToLayer(ClickEffectCameraLayerName), 0 ,31);
		Vector2 v_globalPointInClickEffectCamera = ClickEffectCamera.ScreenToWorldPoint(KiltUICamera.currentTouch != null? KiltUICamera.currentTouch.pos : (Vector2)Input.mousePosition);
		//ShowEffect In ClickEffectCamera ScreenSpace
		ShowEffect(v_globalPointInClickEffectCamera, PressRadius, v_layer);
	}

	//In ClickCamera Space
	protected virtual void ShowEffect(Vector2 p_point, float p_radius, int p_layer)
	{
		float v_time = 0.2f;
		GameObject v_object = null;
		if(ClickEffect != null)
		{
			v_time = 0f; // Dont Call Destroy when have effect attached
			v_object = GameObject.Instantiate(ClickEffect) as GameObject;
		}
		if(v_object == null && ShowDebug)
			v_object = new GameObject();

		if(v_object != null)
		{
			v_object.name = "Click_Effect";
			v_object.transform.parent = ClickEffectCamera.transform;
			v_object.transform.position = p_point;
			v_object.layer = Mathf.Clamp(p_layer, 0 , 31);
			if(ShowDebug && Application.isEditor)
			{
				DebugCircleRange v_debugComponent = v_object.GetComponent<DebugCircleRange>();
				if(v_debugComponent == null)
					v_debugComponent = v_object.AddComponent<DebugCircleRange>();
				v_debugComponent.Radius = p_radius;
				v_debugComponent.MaxTime = v_time;
			}
		}
		if(_effectInScene != null)
		{
			Object.Destroy(_effectInScene);
			_effectInScene = v_object;
		}
	}

	protected void ShowTrailEffect()
	{
		if(TrailEffect != null && _trailEffectInScene == null)
		{
			int v_layer = Mathf.Clamp(LayerMask.NameToLayer(ClickEffectCameraLayerName), 0 , 31);
			Vector2 v_globalPointInClickEffectCamera = ClickEffectCamera.ScreenToWorldPoint(Input.mousePosition);
			//ShowEffect In ClickEffectCamera ScreenSpace
			ShowTrailEffect(v_globalPointInClickEffectCamera, v_layer);
		}
	}

	protected virtual void ShowTrailEffect(Vector2 p_point, int p_layer)
	{
		if(TrailEffect != null && _trailEffectInScene == null)
		{
			_trailEffectInScene = GameObject.Instantiate(TrailEffect) as GameObject;
			_trailEffectInScene.name = "Trail_Effect";
			_trailEffectInScene.transform.parent = ClickEffectCamera.transform;
			_trailEffectInScene.layer = p_layer;
			_trailEffectInScene.transform.position = new Vector3(p_point.x, p_point.y, -1);
		}
	}

	//Get Radius based in ClickEffect Camera distances
	protected virtual float GetNewRadius(Camera p_newCamera, Vector2 p_screenPoint)
	{
		float v_newRadius = PressRadius;
		if(p_newCamera != null)
		{
			//Get global Point in NewCamera Space and In ClickEffectCamera Space 
			Vector2 v_globalPoint = p_newCamera.ScreenToWorldPoint(p_screenPoint);
			Vector2 v_globalPointInClickEffectCamera = ClickEffectCamera.ScreenToWorldPoint(p_screenPoint);
			//Pick one point in PressRadius distance in Global ClickEffectCamera Space
			Vector2 v_radiusDistancePointInClickEffect = new Vector2(v_globalPointInClickEffectCamera.x - PressRadius, v_globalPointInClickEffectCamera.y);
			//Send This distance point to ScreenSpace
			Vector2 v_screenDistancePoint = ClickEffectCamera.WorldToScreenPoint(v_radiusDistancePointInClickEffect);
			//Send ScreenDistance Point to newCamera World Space and get final value
			v_newRadius = Vector2.Distance(p_newCamera.ScreenToWorldPoint(v_screenDistancePoint), v_globalPoint);
		}
		return v_newRadius;
	}

	protected virtual void TryUpdateCurrentTouch(Camera p_pressCamera)
	{
		if(p_pressCamera != null && p_pressCamera != ClickEffectCamera)
		{
			if(KiltUICamera.currentTouch != null && KiltUICamera.currentTouch.pressed == null)
			{
				Vector2 v_globalPoint = p_pressCamera.ScreenToWorldPoint(KiltUICamera.currentTouch.pos);
				float v_newRadius = GetNewRadius(p_pressCamera, KiltUICamera.currentTouch.pos);
				GameObject v_object = FindCloseObjectInRange(v_globalPoint, v_newRadius);
				//if(v_object == null && _trailEffectInScene != null)
				//	v_object = _trailEffectInScene;
				KiltUICamera.currentTouch.current = v_object;
				KiltUICamera.currentTouch.pressed = v_object;
				KiltUICamera.currentTouch.dragged = v_object;
			}
		}
	}

	#endregion

	#region Events Registration

	protected virtual void HandleOnGlobalPress (bool p_isPressed)
	{
		if(p_isPressed)
		{
			ShowTrailEffect();
			if(KiltUICamera.currentTouch != null)
			{
				ShowEffect();
				foreach(Camera v_camera in CameraManager.CamerasInScene)
				{
					TryUpdateCurrentTouch(v_camera);
					if(KiltUICamera.currentTouch.pressed != null)
						break;
				}
				if(KiltUICamera.currentTouch.pressedCam != null && KiltUICamera.currentTouch.pressed == null)
					TryUpdateCurrentTouch(KiltUICamera.currentTouch.pressedCam);
			}
		}
		if(OnGlobalPress != null)
			OnGlobalPress(p_isPressed);
	}

	protected virtual void HandleOnGlobalDrag (Vector2 p_delta)
	{
		if(OnGlobalDrag != null)
			OnGlobalDrag(p_delta);
	}

	protected virtual void HandleOnGlobalDrop (GameObject p_dropObject)
	{
		if(OnGlobalDrop != null)
			OnGlobalDrop(p_dropObject);
	}

	#endregion

}
