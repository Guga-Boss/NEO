using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ColorEffectInteraction : InteractionBehaviour {

	#region Variables
	
	public GameObject tweenTarget;
	public Color hover = new Color(0.6f, 1f, 0.2f, 1f);
	public Color pressed = Color.grey;
	public float duration = 0.2f;
	
	protected Color mColor;
	
	#endregion

	#region Properties

	public Color defaultColor
	{
		get
		{
			Start();
			return mColor;
		}
		set
		{
			Start();
			mColor = value;
		}
	}

	#endregion
	
	#region Unity Functions
	
	protected override void Awake ()
	{
		Trigger = ButtonTrigger.None;
		PauseAction = PauseActionEnum.None;
		KeyToLauch = KeyCode.None;
	}
	
	protected override void Start ()
	{
		if (!Started)
		{
			Started = true;
			Init ();
		}
	}

	protected override void OnEnable () 
	{ 
		base.OnEnable();
		if (Started && Highlighted) OnHover(KiltUICamera.IsHighlighted(gameObject)); 
	}

	protected override void OnDisable ()
	{
		base.OnDisable();
		if (Started && tweenTarget != null)
		{
			#if NGUI_DLL
			TweenColor tc = tweenTarget.GetComponent<TweenColor>();
			
			if (tc != null)
			{
				tc.color = mColor;
				tc.enabled = false;
			}
			#else
			ColorScheduler tc = tweenTarget.GetComponent<ColorScheduler>();
			
			if (tc != null)
			{
				tc.From = mColor;
				tc.enabled = false;
			}
			#endif
		}
	}
	
	#endregion

	#region Helper Functions

	protected void Init ()
	{
		if (tweenTarget == null) tweenTarget = gameObject;
		#if NGUI_DLL
		UIWidget widget = tweenTarget.GetComponent<UIWidget>();
		
		if (widget != null)
		{
			mColor = widget.color;
		}
		else
		#endif
		{
			Renderer ren = tweenTarget.GetComponent<Renderer>();
			MaskableGraphic graphic = tweenTarget.GetComponent<MaskableGraphic>();
			if (ren != null)
			{
				mColor = ren.material.color;
			}
			if(graphic != null)
			{
				mColor = graphic.color;
			}
			else if (ren == null && graphic == null)
			{
				Light lt = tweenTarget.GetComponent<Light>();
				
				if (lt != null)
				{
					mColor = lt.color;
				}
				else
				{
					#if NGUI_DLL
					Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " has nothing for ColorButton to color", this);
					#endif
					enabled = false;
				}
			}
		}
		OnEnable();
	}

	protected virtual void InitTween(GameObject p_tweenTarget, float p_durantion, Color p_to)
	{
		if (!Started) Start();
		#if NGUI_DLL
		TweenColor.Begin(p_tweenTarget, p_durantion, p_to);
		#else
		ColorScheduler tc = tweenTarget.GetComponent<ColorScheduler>();
		if (tc == null)
			tc = tweenTarget.gameObject.AddComponent<ColorScheduler>();
		if (tc != null)
		{
			Renderer ren = tweenTarget.GetComponent<Renderer>();
			MaskableGraphic graphic = tweenTarget.GetComponent<MaskableGraphic>();
			tc.MaxTime = duration;
			tc.From = ren != null? ren.material.color : (graphic != null? graphic.color : mColor);
			tc.To = p_to;
			tc.StartTimer();
		}
		#endif
	}

	#endregion
	
	#region Event Receivers

	protected override void OnPress (bool isPressed)
	{
		if (enabled && !CameraFade.InstanceExists())
		{
			if (!Started) Start();
			InitTween(tweenTarget, duration, isPressed ? pressed : (KiltUICamera.IsHighlighted(gameObject) ? hover : mColor));
		}
	}
	
	protected override void OnHover (bool isOver)
	{
		if (enabled && !CameraFade.InstanceExists())
		{
			if (!Started) Start();
			InitTween(tweenTarget, duration, isOver ? hover : mColor);
			Highlighted = isOver;
		}
	}

	#endregion
}
