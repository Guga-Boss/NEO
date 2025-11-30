using UnityEngine;
using System;
using Kilt.Core;

public class CameraFade : PersistentObject
{   
	#region Events
	
	public static Delegates.EventHandler OnFinishFade;

	#endregion

	#region Singleton

	private static CameraFade mInstance = null;
	
	public static CameraFade Instance
	{
		get
		{
			if( mInstance == null )
			{
				mInstance = GameObject.FindObjectOfType(typeof(CameraFade)) as CameraFade;
				
				if( mInstance == null )
				{
					CameraFade v_object = new GameObject("CameraFade").AddComponent<CameraFade>();
					//BoxCollider v_collider = v_object.gameObject.AddComponent<BoxCollider>();
					//v_collider.size = new Vector3(9999,9999,9999);
					mInstance = v_object;
				}
			}
			
			return mInstance;
		}
		protected set
		{
			mInstance = value;
		}
	}

	#endregion

	#region Public Variables
	
	public GUIStyle m_BackgroundStyle = new GUIStyle();						// Style for background tiling
	public Texture2D m_FadeTexture;											// 1x1 pixel texture used for fading
	public Color m_CurrentScreenOverlayColor = new Color(0,0,0,0);			// default starting color: black and fully transparrent
	public Color m_TargetScreenOverlayColor = new Color(0,0,0,0);			// default target color: black and fully transparrent
	public Color m_DeltaColor = new Color(0,0,0,0);							// the delta-color is basically the "speed / second" at which the current color should change
	public int m_FadeGUIDepth = -1000;										// make sure this texture is drawn on top of everything
	
	public float m_FadeDelay = 0;
	public Action m_OnFadeFinish = null;
	
	public bool m_canDestroy = false;
	public bool m_isFading = false;
	
	#endregion

	#region Unity Functions

	protected override void Awake()
	{
		if( mInstance == null )
		{
			base.Awake();
			mInstance = this as CameraFade;
			Instance.init();
		}
	}

	// Draw the texture and perform the fade:
	protected virtual void OnGUI()
	{   
		// If delay is over...
		if( Instance.m_FadeDelay <= 0)
		{
			// If the current color of the screen is not equal to the desired color: keep fading!
			if (Instance.m_CurrentScreenOverlayColor != Instance.m_TargetScreenOverlayColor)
			{			
				// If the difference between the current alpha and the desired alpha is smaller than delta-alpha * deltaTime, then we're pretty much done fading:
				if (Mathf.Abs(Instance.m_CurrentScreenOverlayColor.a - Instance.m_TargetScreenOverlayColor.a) < Mathf.Abs(Instance.m_DeltaColor.a) * GetDeltaTime())
				{
					FinishFade();
				}
				else
				{
					// Fade!
					SetScreenOverlayColor(Instance.m_CurrentScreenOverlayColor + Instance.m_DeltaColor * GetDeltaTime());
				}
			}
			else
			{
				FinishFade();
			}
		}
		// Only draw the texture when the alpha value is greater than 0:
		if (m_CurrentScreenOverlayColor.a > 0)
		{			
			GUI.depth = Instance.m_FadeGUIDepth;
			GUI.Label(new Rect(-10, -10, Screen.width + 10, Screen.height + 10), Instance.m_FadeTexture, Instance.m_BackgroundStyle);
		}
	}
	
	protected virtual void Update()
	{
		if( Instance.m_FadeDelay > 0)
		{
			Instance.m_FadeDelay -= GetDeltaTime();
		}
	}

	protected virtual void OnDestroy()
	{
		CallEvent();
	}

	protected virtual void OnApplicationQuit()
	{
		mInstance = null;
	}

	#endregion

	#region Helper Functions

	// Initialize the texture, background-style and initial color:
	public void init()
	{		
		Instance.m_FadeTexture = new Texture2D(1, 1);        
		Instance.m_BackgroundStyle.normal.background = Instance.m_FadeTexture;
	}

	private void FinishFade()
	{
		m_canDestroy = true;
		Instance.m_CurrentScreenOverlayColor = Instance.m_TargetScreenOverlayColor;
		SetScreenOverlayColor(Instance.m_CurrentScreenOverlayColor);
		Instance.m_DeltaColor = new Color( 0,0,0,0 );
		
		if( Instance.m_OnFadeFinish != null )
			Instance.m_OnFadeFinish();
		if(Instance.m_canDestroy)
		{
			Die();
		}
	}

	private float GetDeltaTime()
	{
		//return PauseManager.IsPaused()? Time.fixedDeltaTime : Time.deltaTime;
		return Time.unscaledDeltaTime; 
	}

	public static bool InstanceExists()
	{
		if(mInstance == null)
			return false;
		else
			return true;
	}

	/// <summary>
	/// Sets the color of the screen overlay instantly.  Useful to start a fade.
	/// </summary>
	/// <param name='newScreenOverlayColor'>
	/// New screen overlay color.
	/// </param>
	private static void SetScreenOverlayColor(Color newScreenOverlayColor)
	{
		Instance.m_CurrentScreenOverlayColor = newScreenOverlayColor;
		Instance.m_FadeTexture.SetPixel(0, 0, Instance.m_CurrentScreenOverlayColor);
		Instance.m_FadeTexture.Apply();
	}

	public static bool IsFading()
	{
		return InstanceExists()? Instance.m_isFading : false;
	}
	
	/// <summary>
	/// Starts the fade from color newScreenOverlayColor. If isFadeOut, start fully opaque, else start transparent.
	/// </summary>
	/// <param name='newScreenOverlayColor'>
	/// Target screen overlay Color.
	/// </param>
	/// <param name='fadeDuration'>
	/// Fade duration.
	/// </param>
	public static void StartAlphaFade(Color newScreenOverlayColor, bool isFadeOut, float fadeDuration )
	{
		Instance.m_isFading = !isFadeOut;
		Instance.m_canDestroy = false;
		if (fadeDuration <= 0.0f)		
		{
			SetScreenOverlayColor(newScreenOverlayColor);
		}
		else					
		{
			if( isFadeOut )
			{
				Instance.m_TargetScreenOverlayColor = new Color( newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0 );
				SetScreenOverlayColor( newScreenOverlayColor );
			} else {
				Instance.m_TargetScreenOverlayColor = newScreenOverlayColor;
				SetScreenOverlayColor( new Color( newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0 ) );
			}
			
			Instance.m_DeltaColor = (Instance.m_TargetScreenOverlayColor - Instance.m_CurrentScreenOverlayColor) / fadeDuration;	
		}
	}
	
	/// <summary>
	/// Starts the fade from color newScreenOverlayColor. If isFadeOut, start fully opaque, else start transparent, after a delay.
	/// </summary>
	/// <param name='newScreenOverlayColor'>
	/// New screen overlay color.
	/// </param>
	/// <param name='fadeDuration'>
	/// Fade duration.
	/// </param>
	/// <param name='fadeDelay'>
	/// Fade delay.
	/// </param>
	public static void StartAlphaFade(Color newScreenOverlayColor, bool isFadeOut, float fadeDuration, float fadeDelay )
	{
		Instance.m_isFading = !isFadeOut;
		Instance.m_canDestroy = false;
		if (fadeDuration <= 0.0f)		
		{
			SetScreenOverlayColor(newScreenOverlayColor);
		}
		else					
		{
			Instance.m_FadeDelay = fadeDelay;			
			
			if( isFadeOut )
			{
				Instance.m_TargetScreenOverlayColor = new Color( newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0 );
				SetScreenOverlayColor( newScreenOverlayColor );
			} else {
				Instance.m_TargetScreenOverlayColor = newScreenOverlayColor;
				SetScreenOverlayColor( new Color( newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0 ) );
			}
			
			Instance.m_DeltaColor = (Instance.m_TargetScreenOverlayColor - Instance.m_CurrentScreenOverlayColor) / fadeDuration;
		}
	}
	
	/// <summary>
	/// Starts the fade from color newScreenOverlayColor. If isFadeOut, start fully opaque, else start transparent, after a delay, with Action OnFadeFinish.
	/// </summary>
	/// <param name='newScreenOverlayColor'>
	/// New screen overlay color.
	/// </param>
	/// <param name='fadeDuration'>
	/// Fade duration.
	/// </param>
	/// <param name='fadeDelay'>
	/// Fade delay.
	/// </param>
	/// <param name='OnFadeFinish'>
	/// On fade finish, doWork().
	/// </param>
	public static void StartAlphaFade(Color newScreenOverlayColor, bool isFadeOut, float fadeDuration, float fadeDelay, Action OnFadeFinish )
	{
		Instance.m_isFading = !isFadeOut;
		Instance.m_canDestroy = false;
		if (fadeDuration <= 0.0f)		
		{
			SetScreenOverlayColor(newScreenOverlayColor);
		}
		else					
		{
			Instance.m_OnFadeFinish = OnFadeFinish;
			Instance.m_FadeDelay = fadeDelay;
			
			if( isFadeOut )
			{
				Instance.m_TargetScreenOverlayColor = new Color( newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0 );
				SetScreenOverlayColor( newScreenOverlayColor );
			} else {
				Instance.m_TargetScreenOverlayColor = newScreenOverlayColor;
				SetScreenOverlayColor( new Color( newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0 ) );
			}
			Instance.m_DeltaColor = (Instance.m_TargetScreenOverlayColor - Instance.m_CurrentScreenOverlayColor) / fadeDuration;
		}
	}
	
	public static void LoadSceneWithFade(Color newScreenOverlayColor, bool isFadeOut, float fadeDuration, float fadeDelay, int sceneToLoadIndex ,bool waitPreviousLoad)
	{
		if(waitPreviousLoad && IsFading())
			return;
		Instance.m_isFading = !isFadeOut;
		if(isFadeOut)
		{
			SceneManager.LoadLevel(sceneToLoadIndex, true);
			StartAlphaFade( newScreenOverlayColor, isFadeOut, fadeDuration, fadeDelay, null);
		}
		else
			StartAlphaFade( newScreenOverlayColor, isFadeOut, fadeDuration, fadeDelay, () => { SceneManager.LoadLevel(sceneToLoadIndex, true); } );
	}

	public static void LoadSceneWithFade(Color newScreenOverlayColor, bool isFadeOut, float fadeDuration, float fadeDelay, string sceneToLoad, bool waitPreviousLoad)
	{
		if(waitPreviousLoad && IsFading())
			return;
		Instance.m_isFading = true;
		if(isFadeOut)
		{
			SceneManager.LoadLevel(sceneToLoad, true);
			StartAlphaFade( newScreenOverlayColor, isFadeOut, fadeDuration, fadeDelay, null);
		}
		else
			StartAlphaFade( newScreenOverlayColor, isFadeOut, fadeDuration, fadeDelay, () => { SceneManager.LoadLevel(sceneToLoad, true); } );
	}

	public static void LoadSceneWithFadePingPong(Color newScreenOverlayColor, bool m_fadeOutFadeIn, float fadeDuration, float fadeOneDelay, float fadeTwoDelay, int sceneToLoadIndex )
	{
		if(!IsFading())
			StartAlphaFade(newScreenOverlayColor, !m_fadeOutFadeIn, fadeDuration, fadeOneDelay, () => LoadSceneWithFade(newScreenOverlayColor, m_fadeOutFadeIn, fadeDuration, fadeTwoDelay, sceneToLoadIndex, false) );
	}

	public static void LoadSceneWithFadePingPong(Color newScreenOverlayColor, bool m_fadeOutFadeIn, float fadeDuration, int sceneToLoadIndex )
	{
		LoadSceneWithFadePingPong(newScreenOverlayColor, m_fadeOutFadeIn, fadeDuration,0,0,sceneToLoadIndex);
	}

	public static void LoadSceneWithFadePingPong(Color newScreenOverlayColor, float fadeDuration, int sceneToLoadIndex )
	{
		LoadSceneWithFadePingPong(newScreenOverlayColor, true, fadeDuration, sceneToLoadIndex );
	}

	public static void LoadSceneWithFadePingPong(Color newScreenOverlayColor, bool m_fadeOutFadeIn, float fadeDuration, float fadeOneDelay, float fadeTwoDelay, string sceneToLoad )
	{
		if(!IsFading())
			StartAlphaFade(newScreenOverlayColor, !m_fadeOutFadeIn, fadeDuration, fadeOneDelay, () => LoadSceneWithFade(newScreenOverlayColor, m_fadeOutFadeIn, fadeDuration, fadeTwoDelay, sceneToLoad, false) );
	}

	public static void LoadSceneWithFadePingPong(Color newScreenOverlayColor, bool m_fadeOutFadeIn, float fadeDuration, string sceneToLoad )
	{
		LoadSceneWithFadePingPong(newScreenOverlayColor, m_fadeOutFadeIn, fadeDuration,0,0,sceneToLoad);
	}

	public static void LoadSceneWithFadePingPong(Color newScreenOverlayColor, float fadeDuration, string sceneToLoad )
	{
		LoadSceneWithFadePingPong(newScreenOverlayColor, true, fadeDuration, sceneToLoad );
	}
	
	protected void Die()
	{
		CallEvent();
		mInstance = null;
		Destroy(gameObject);
	}

	protected void CallEvent(bool p_force = false)
	{
		if(m_isFading || p_force)
		{
			m_isFading = false;
			if(OnFinishFade != null)
				OnFinishFade();
		}
	}

	#endregion
}