using UnityEngine;
using System.Collections;

public class DebugCircleRange : MonoBehaviour {
	
	#region Private Variables
	[SerializeField]
	float m_maxTime = 0.2f;
	[SerializeField]
	float m_radius = 0.06f;
	[SerializeField]
	Color m_color = new Color(1f,0,0,0.6f);
	[SerializeField]
	bool m_useUnscaledTime = true;

	float _currentTime = 0f;
	#endregion

	#region Public Properties

	public float MaxTime {get{return m_maxTime;} set{m_maxTime = value;}}
	public float Radius {get{return m_radius;} set{m_radius = value;}}
	public Color Color {get{return m_color;} set{m_color = value;}}
	public bool UseUnscaledTime {get{return m_useUnscaledTime;} set{m_useUnscaledTime = value;}}

	#endregion

	#region Unity Functions

	protected virtual void Start()
	{
		_currentTime = MaxTime;
	}
	
	protected virtual void Update () 
	{
		if(Application.isEditor)
			DebugDraw.DrawSphere(transform.position, Radius, Color, gameObject.layer);
		if(_currentTime > 0)
		{
			_currentTime = Mathf.Max(0,_currentTime - GetDeltaTime());
			if(_currentTime == 0)
				GameObject.Destroy(this.gameObject);
		}
	}

	private float GetDeltaTime()
	{
		if(UseUnscaledTime)
			return Time.unscaledDeltaTime;
		return Time.deltaTime;
	}

	#endregion
}
