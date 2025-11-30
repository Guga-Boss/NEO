using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ChildrenTranckerAlgorithmEnum {Awake, CycleStart, FirstLateUpdate, EveryCycle}

public class AlphaScheduler : TimeScheduler {

	#region Private Variables

	[SerializeField]
	float m_from = 0f;
	[SerializeField]
	float m_to = 1f;
	[SerializeField]
	ChildrenTranckerAlgorithmEnum m_trackerAlgorithm = ChildrenTranckerAlgorithmEnum.FirstLateUpdate;
	[SerializeField, PropertyBackingFieldAttribute]
	bool m_propagateToChildrens = false; // used to Alpha Childrens
	[SerializeField]
	bool m_setInitialValueWhenStart = false; //Value returned by component, not by From or To

	#endregion

	#region Public Properties
	
	public float From { get {return m_from;} set {m_from = value;}}
	public float To { get {return m_to;} set {m_to = value;}}
	public bool PropagateToChildrens { get {return m_propagateToChildrens;} set {m_propagateToChildrens = value; TrackChildrenComponents();}}
	public bool SetInitialValueWhenStart { get {return m_setInitialValueWhenStart;} set {m_setInitialValueWhenStart = value;}}
	public ChildrenTranckerAlgorithmEnum TrackerAlgorithm { get {return m_trackerAlgorithm;} set {m_trackerAlgorithm = value;}}

	#endregion

	#region Protected Properties
	
	protected float Alpha
	{
		get
		{
			#if NGUI_DLL
			if (Widget != null) return Widget.alpha;
			if (Panel != null) return Panel.alpha;
			#endif
			if (SpriteRendererComponent != null) return SpriteRendererComponent.color.a;
			if (OwnerMaterial != null) return OwnerMaterial.color.a;
			if (CanvasGroupComponent) return CanvasGroupComponent.alpha;
			if (uiGraphics != null) return uiGraphics.color.a;
			return 0f;
		}
		set
		{
			#if NGUI_DLL
			if (Widget != null) Widget.alpha = value;
			if (Panel != null) Panel.alpha = value;
			#endif
			if(SpriteRendererComponent != null) 
			{ 
				Color v_tempColor = SpriteRendererComponent.color;
				v_tempColor.a = value;
				SpriteRendererComponent.color = v_tempColor;
			}
			if (OwnerMaterial != null) 
			{
				Color v_tempColor = OwnerMaterial.color;
				v_tempColor.a = value;
				OwnerMaterial.color = v_tempColor;
			}
			if (CanvasGroupComponent != null) 
			{
				CanvasGroupComponent.alpha = value;
			}
			else if (uiGraphics != null) 
			{
				Color v_tempColor = uiGraphics.color;
				v_tempColor.a = value;
				uiGraphics.color = v_tempColor;
			}
			SetAlphaInChildrens(value);
		}
	}

	SpriteRenderer _spriteRendererComponent = null;
	protected SpriteRenderer SpriteRendererComponent
	{
		get
		{
			if (Owner != null && _spriteRendererComponent == null)
				_spriteRendererComponent = Owner.GetComponent<SpriteRenderer>();
			return _spriteRendererComponent;
		}
	}

	#if NGUI_DLL
	UIWidget _uiWidget = null;
	protected UIWidget Widget
	{
		get
		{
			if (Owner != null && _uiWidget == null)
				_uiWidget = Owner.GetComponent<UIWidget>();
			return _uiWidget;
		}
	}
	
	UIPanel _uiPanel = null;
	protected UIPanel Panel
	{
		get
		{
			if (Owner != null && _uiPanel == null)
				_uiPanel = Owner.GetComponent<UIPanel>();
			return _uiPanel;
		}
	}
	#endif

	CanvasGroup _canvasGroupComponent = null;
	protected CanvasGroup CanvasGroupComponent
	{
		get
		{
			if (Owner != null && _canvasGroupComponent == null)
				_canvasGroupComponent = Owner.GetComponent<CanvasGroup>();
			return _canvasGroupComponent;
		}
	}

	MaskableGraphic _uiGraphics = null;
	protected MaskableGraphic uiGraphics
	{
		get
		{
			if (Owner != null && _uiGraphics == null)
				_uiGraphics = Owner.GetComponent<MaskableGraphic>();
			return _uiGraphics;
		}
	}
	
	Material _ownerMaterial = null;
	protected Material OwnerMaterial
	{
		get
		{
			if (Owner != null && Owner.GetComponent<Renderer>() != null && _ownerMaterial == null)
			{
				if (Owner.GetComponent<Renderer>().material.HasProperty("_Color"))
					_ownerMaterial = Owner.GetComponent<Renderer>().material;
			}
			return _ownerMaterial;
		}
	}

	List<CanvasGroup> _uiCanvasGroupChildrens = new List<CanvasGroup>();
	List<MaskableGraphic> _uiGraphicsChildrens = new List<MaskableGraphic>();
	List<SpriteRenderer> _spriteRendererChildrens = new List<SpriteRenderer>();
	List<Material> _materialChildrens = new List<Material>();
	#if NGUI_DLL
	List<UIWidget> _uiWidgetChildrens = new List<UIWidget>();
	List<UIPanel> _uiPanelChildrens = new List<UIPanel>();
	#endif
	
	#endregion

	#region Unity Functions

	public virtual void Awake()
	{
		_firstLateUpdate = true;
		if(TrackerAlgorithm == ChildrenTranckerAlgorithmEnum.Awake)
			TrackChildrenComponents();
	}

	bool _firstLateUpdate = true;
	public virtual void LateUpdate()
	{
		if(_firstLateUpdate)
		{
			_firstLateUpdate = false;
			if(TrackerAlgorithm == ChildrenTranckerAlgorithmEnum.FirstLateUpdate)
				TrackChildrenComponents();
		}
		if(TrackerAlgorithm == ChildrenTranckerAlgorithmEnum.EveryCycle)
			TrackChildrenComponents();
	}

	#endregion
	
	#region Constructor
	
	public AlphaScheduler()
	{
		OnCycleStartedExecution += HandleOnCycleStartedExecution;
	}

	#endregion

	#region Events Receivers

	void HandleOnCycleStartedExecution (CycleEventArgs e)
	{
		if(TrackerAlgorithm == ChildrenTranckerAlgorithmEnum.CycleStart)
			TrackChildrenComponents();
	}

	#endregion

	#region Helper Functions

	protected virtual void TrackChildrenComponents()
	{
		if(PropagateToChildrens)
		{
			if(Owner != null)
			{
				_spriteRendererChildrens = new List<SpriteRenderer>(Owner.GetComponentsInChildren<SpriteRenderer>());
				#if NGUI_DLL
				_uiWidgetChildrens = new List<UIWidget>(Owner.GetComponentsInChildren<UIWidget>());
				_uiPanelChildrens = new List<UIPanel>(Owner.GetComponentsInChildren<UIPanel>());
				#endif
				CanvasGroup[] v_canvasChildrens = Owner.GetComponentsInChildren<CanvasGroup>();
				_uiCanvasGroupChildrens.Clear();
				foreach(CanvasGroup v_group in v_canvasChildrens)
				{
					if(v_group != null && v_group.ignoreParentGroups)
					{
						_uiCanvasGroupChildrens.Add(v_group);
					}
				}

				MaskableGraphic[] v_maskableChildrens = Owner.GetComponentsInChildren<MaskableGraphic>();
				_uiGraphicsChildrens.Clear();
				foreach(MaskableGraphic v_maskable in v_maskableChildrens)
				{
					if(v_maskable != null)
					{
						bool v_isChild = false;
						foreach(CanvasGroup v_group in v_canvasChildrens)
						{
							if(v_group != null && KiltUtils.IsChildObject(v_group.gameObject, v_maskable.gameObject, true))
							{
								v_isChild = true;
								break;
							}
						}
						if(!v_isChild)
							_uiGraphicsChildrens.Add(v_maskable);
					}
				}

				Renderer[] v_rendererChildrens = Owner.GetComponentsInChildren<Renderer>();
				_materialChildrens.Clear();
				foreach(Renderer v_renderer in v_rendererChildrens)
				{
					if(v_renderer != null && v_renderer.material != null)
					{
						if (v_renderer.material.HasProperty("_Color"))
							_materialChildrens.Add(v_renderer.material);
					}
				}
			}
		}
		else
		{
			_uiCanvasGroupChildrens.Clear();
			_uiGraphicsChildrens.Clear();
			_spriteRendererChildrens.Clear();
			_materialChildrens.Clear();
			#if NGUI_DLL
			_uiWidgetChildrens.Clear();
			_uiPanelChildrens.Clear();
			#endif
		}
	}

	protected void SetAlphaInChildrens(float p_alphaValue)
	{
		foreach(SpriteRenderer v_renderer in _spriteRendererChildrens)
		{
			if(v_renderer != null)
			{
				Color v_tempColor = v_renderer.color;
				v_tempColor.a = p_alphaValue;
				v_renderer.color = v_tempColor;
			}
		}
		#if NGUI_DLL
		foreach(UIWidget v_widget in _uiWidgetChildrens)
		{
			if(v_widget != null)
			{
				v_widget.alpha = p_alphaValue;
			}
		}
		foreach(UIPanel v_panel in _uiPanelChildrens)
		{
			if(v_panel != null)
			{
				v_panel.alpha = p_alphaValue;
			}
		}
		#endif
		foreach(CanvasGroup v_group in _uiCanvasGroupChildrens)
		{
			if(v_group != null)
			{
				v_group.alpha = p_alphaValue;
			}
		}
		foreach(MaskableGraphic v_graphic in _uiGraphicsChildrens)
		{
			if(v_graphic != null)
			{
				Color v_tempColor = v_graphic.color;
				v_tempColor.a = p_alphaValue;
				v_graphic.color = v_tempColor;
			}
		}
		foreach(Material v_material in _materialChildrens)
		{
			if(v_material != null)
			{
				Color v_tempColor = v_material.color;
				v_tempColor.a = p_alphaValue;
				v_material.color = v_tempColor;
			}
		}
	}

	#endregion
	
	#region Overridden Functions

	float _initialValue = 0f;
	protected override void OnPingStart()
	{
		_initialValue = SetInitialValueWhenStart? Alpha : From;
		Alpha = Mathf.Lerp(_initialValue, To, GetTimeScale());
	}
	
	protected override void OnPongStart()
	{
		_initialValue = SetInitialValueWhenStart? Alpha : To;
		Alpha = Mathf.Lerp(From, _initialValue, GetTimeScale());
	}
	
	protected override void OnPingUpdate()
	{
		try
		{
			Alpha = Mathf.Lerp(_initialValue, To, GetTimeScale());
		}
		catch{}
	}
	
	protected override void OnPongUpdate()
	{
		try
		{
			Alpha = Mathf.Lerp(From, _initialValue, GetTimeScale());
		}
		catch{}
	}

	/*protected override void OnBeforePingFinish()
	{
		try
		{
			Alpha = Mathf.Lerp(From, To, GetTimeScale());
		}
		catch{}
	}
	
	protected override void OnBeforePongFinish()
	{
		try
		{
			Alpha = Mathf.Lerp(From, To, GetTimeScale());
		}
		catch{}
	}*/
	
	#endregion
}
