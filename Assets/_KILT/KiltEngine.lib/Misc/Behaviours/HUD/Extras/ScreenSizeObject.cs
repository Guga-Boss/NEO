using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScreenSizeObject : MonoBehaviour 
{
	#region Private Variables
	[SerializeField]
	bool m_checkEveryCycle = false;
	Camera m_cameraThatDrawThisObject = null;

	//Aux Variables
	bool _needRecalc = true;
	int _lastWidth = 0;
	int _lastHeight = 0;

	#endregion

	#region Public Properties

	public bool CheckEveryCycle
	{
		get
		{
			return m_checkEveryCycle;
		}
		set
		{
			if(m_checkEveryCycle == value)
				return;
			m_checkEveryCycle = value;
		}
	}

	public Camera CameraThatDrawThisObject
	{
		get
		{
			if(m_cameraThatDrawThisObject == null)
			{
				m_cameraThatDrawThisObject = CameraManager.GetCameraThatDrawLayer(gameObject.layer);
			}
			return m_cameraThatDrawThisObject;
		}
	}

	#endregion

	#region Unity Functions

	protected virtual void Start()
	{
		RecalcSize(true);
	}

	protected virtual void Update () 
	{
		CheckIfNeedRecalc();
	}

	protected virtual void LateUpdate () 
	{
		RecalcSize(CheckEveryCycle);
	}

	#endregion

	#region Helper Functions
	
	public virtual void MarkToRecalcSize()
	{
		_needRecalc = true;
	}

	protected virtual void CheckIfNeedRecalc()
	{
		if(_lastWidth != Screen.width || _lastHeight != Screen.height)
			MarkToRecalcSize();
	}

	protected virtual void RecalcSize(bool p_force = false)
	{
		if(p_force || _needRecalc)
		{
			_needRecalc = false;
			_lastWidth = Screen.width;
			_lastHeight = Screen.height;

			//Recalc
			if(CameraThatDrawThisObject != null)
			{
				Vector3 v_middlePoint = CameraThatDrawThisObject.ScreenToWorldPoint(new Vector2(_lastWidth/2.0f, _lastHeight/2.0f));
				Vector3 v_topLeftPoint = CameraThatDrawThisObject.ScreenToWorldPoint(new Vector2(0, 0));
				Vector3 v_scale = new Vector3(Mathf.Abs(v_middlePoint.x - v_topLeftPoint.x)*2.05f, Mathf.Abs(v_middlePoint.y - v_topLeftPoint.y)*2.05f, 1);
				KiltUtils.SetLossyScale(this.transform, v_scale);
				transform.position = new Vector3(v_middlePoint.x, v_middlePoint.y, transform.position.z);
			}
		}
	}

	#endregion
}
