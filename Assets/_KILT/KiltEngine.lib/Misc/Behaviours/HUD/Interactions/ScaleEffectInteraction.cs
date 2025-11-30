using UnityEngine;

public class ScaleEffectInteraction : InteractionBehaviour
{
	#region Variables

	public Transform tweenTarget;
	public Vector3 hover = new Vector3(1.1f, 1.1f, 1.1f);
	public Vector3 pressed = new Vector3(1.05f, 1.05f, 1.05f);
	public float duration = 0.2f;
	
	Vector3 _scale;

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
			if (tweenTarget == null) tweenTarget = transform;
			_scale = tweenTarget.localScale;
		}
	}
	
	protected override void OnDisable ()
	{
		base.OnDisable();
		if (Started && tweenTarget != null)
		{
			#if NGUI_DLL
			TweenScale tc = tweenTarget.GetComponent<TweenScale>();
			if (tc != null)
			{
				tc.scale = _scale;
				tc.enabled = false;
			}
			#else
			ScaleScheduler tc = tweenTarget.GetComponent<ScaleScheduler>();
			
			if (tc != null)
			{
				tc.ScaleVector = _scale;
				tc.enabled = false;
			}
			#endif
		}
	}

	protected virtual void InitTween(GameObject p_tweenTarget, float p_durantion, Vector3 p_to)
	{
		if (!Started) Start();
		#if NGUI_DLL
		TweenScale.Begin(tweenTarget.gameObject, duration, p_to).method = UITweener.Method.EaseInOut;
		#else
		ScaleScheduler tc = tweenTarget.GetComponent<ScaleScheduler>();
		if (tc == null)
			tc = tweenTarget.gameObject.AddComponent<ScaleScheduler>();
		if (tc != null)
		{
			tc.MaxTime = duration;
			tc.ScaleVector = p_to;
			tc.ScaleTypeOption = ScaleTypeOptionEnum.ScaleToXY;
			tc.UseScaleBackVector = false;
			tc.ForceFinishOnDisable = true;
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
			InitTween(tweenTarget.gameObject, duration, isPressed ? Vector3.Scale(_scale, pressed) :
			          (KiltUICamera.IsHighlighted(gameObject) ? Vector3.Scale(_scale, hover) : _scale));
		}
	}
	
	protected override void OnHover (bool isOver)
	{
		if (enabled && !CameraFade.InstanceExists())
		{
			if (!Started) Start();
			InitTween(tweenTarget.gameObject, duration, isOver ? Vector3.Scale(_scale, hover) : _scale);
			Highlighted = isOver;
		}
	}

	#endregion
}
