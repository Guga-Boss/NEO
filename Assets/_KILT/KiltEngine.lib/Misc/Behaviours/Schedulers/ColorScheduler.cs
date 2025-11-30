using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ColorScheduler : TimeScheduler {

	#region Private Variables
	
	[SerializeField]
	Color m_from = new Color(1,1,1,1);
	[SerializeField]
	Color m_to = new Color(0,1,0,1);
	[SerializeField]
	ChildrenTranckerAlgorithmEnum m_trackerAlgorithm = ChildrenTranckerAlgorithmEnum.FirstLateUpdate;
	[SerializeField, PropertyBackingFieldAttribute]
	bool m_propagateToChildrens = false; // used to Color Childrens
	[SerializeField]
	bool m_setInitialValueWhenStart = false;
	
	#endregion
	
	#region Public Properties
	
	public Color From { get {return m_from;} set {m_from = value;}}
	public Color To { get {return m_to;} set {m_to = value;}}
	public bool PropagateToChildrens { get {return m_propagateToChildrens;} set {m_propagateToChildrens = value; TrackChildrenComponents();}}
	public bool SetInitialValueWhenStart { get {return m_setInitialValueWhenStart;} set {m_setInitialValueWhenStart = value;}}
	public ChildrenTranckerAlgorithmEnum TrackerAlgorithm { get {return m_trackerAlgorithm;} set {m_trackerAlgorithm = value;}}
	
	#endregion
	
	#region Protected Properties
	
	protected Color Color
	{
		get
		{
			#if NGUI_DLL
			if (Widget != null) return Widget.color;
			if (SpriteRendererComponent != null) return SpriteRendererComponent.color;
			#endif
			if (OwnerMaterial != null) return OwnerMaterial.color;
			if (uiGraphics != null) return uiGraphics.color;
			return Color.white;
		}
		set
		{
			#if NGUI_DLL
			if (Widget != null) Widget.color = value;
			#endif
			if(SpriteRendererComponent != null) 
			{ 
				SpriteRendererComponent.color = value;
			}
			if (OwnerMaterial != null) 
			{
				OwnerMaterial.color = value;
			}
			if (uiGraphics != null) 
			{
				uiGraphics.color = value;
			}
			SetColorInChildrens(value);
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
	
	public ColorScheduler()
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
				_uiGraphicsChildrens = new List<MaskableGraphic>(Owner.GetComponentsInChildren<MaskableGraphic>());
				Renderer[] v_rendererChildrens = Owner.GetComponentsInChildren<Renderer>();
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
			_spriteRendererChildrens.Clear();
			_materialChildrens.Clear();
			_uiGraphicsChildrens.Clear();
			#if NGUI_DLL
			_uiWidgetChildrens.Clear();
			_uiPanelChildrens.Clear();
			#endif
		}
	}
	
	protected void SetColorInChildrens(Color p_colorValue)
	{
		foreach(SpriteRenderer v_renderer in _spriteRendererChildrens)
		{
			if(v_renderer != null)
			{
				Color v_tempColor = p_colorValue;
				v_renderer.color = v_tempColor;
			}
		}
		#if NGUI_DLL
		foreach(UIWidget v_widget in _uiWidgetChildrens)
		{
			if(v_widget != null)
			{
				v_widget.color = p_colorValue;
			}
		}
		#endif
		foreach(Material v_material in _materialChildrens)
		{
			if(v_material != null)
			{
				Color v_tempColor = p_colorValue;
				v_material.color = v_tempColor;
			}
		}
		foreach(MaskableGraphic v_graphic in _uiGraphicsChildrens)
		{
			if(v_graphic != null)
			{
				Color v_tempColor = p_colorValue;
				v_graphic.color = v_tempColor;
			}
		}
	}
	
	#endregion
	
	#region Overridden Functions
	
	Color _initialValue = Color.white;
	protected override void OnPingStart()
	{
		_initialValue = SetInitialValueWhenStart? Color : From;
		Color = Color.Lerp(_initialValue, To, GetTimeScale());
	}
	
	protected override void OnPongStart()
	{
		_initialValue = SetInitialValueWhenStart? Color : To;
		Color = Color.Lerp(From, _initialValue, GetTimeScale());
	}
	
	protected override void OnPingUpdate()
	{
		try
		{
			Color = Color.Lerp(_initialValue, To, GetTimeScale());
		}
		catch{}
	}
	
	protected override void OnPongUpdate()
	{
		try
		{
			Color = Color.Lerp(From, _initialValue, GetTimeScale());
		}
		catch{}
	}

	#endregion
}
