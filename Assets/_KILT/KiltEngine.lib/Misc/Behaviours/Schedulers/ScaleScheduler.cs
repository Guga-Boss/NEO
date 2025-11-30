using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Flags]
public enum ScaleTypeOptionEnum {ScaleBy = 1, ScalePercentBy = 2, ScaleToX = 4, ScaleToY = 8, ScaleToZ = 16, ScaleToXY = 12, ScaleToYZ = 24, ScaleToXZ = 20, ScaleToXYZ = 28}

public class ScaleScheduler : TimeScheduler {

	#region Private Variables
	
	//[SerializeField]
	Vector3 m_currentScaledValueBy = Vector2.zero;
	Vector3 m_scaleValueBy = Vector2.zero;
	
	[SerializeField]
	Vector3 m_scaleVector = Vector2.zero;
	[SerializeField]
	Vector3 m_scaleBackVector = Vector2.zero;
	[SerializeField]
	ScaleTypeOptionEnum m_scaleTypeOption = ScaleTypeOptionEnum.ScaleBy;
	[SerializeField]
	bool m_isLocalScale = true;
	[SerializeField]
	bool m_useScaleBackVector = false;
	[SerializeField]
	bool m_useVectorsToClampInitialValues = false;
	
	#endregion
	
	#region Protected Properties
	
	protected Vector3 CurrentScaledValueBy {get {return m_currentScaledValueBy;} set {m_currentScaledValueBy = value;}}
	protected Vector3 ScaleValueBy {get {return m_scaleValueBy;} set {m_scaleValueBy = value;}}
	
	#endregion
	
	#region Public Properties
	
	public Vector3 ScaleVector {get {return m_scaleVector;} set {m_scaleVector = value;}}
	public Vector3 ScaleBackVector {get {return m_scaleBackVector;} set {m_scaleBackVector = value;}}
	public ScaleTypeOptionEnum ScaleTypeOption {get {return m_scaleTypeOption;} set {m_scaleTypeOption = value;}}
	public bool IsLocalScale {get {return m_isLocalScale;} set {m_isLocalScale = value;}}
	public bool UseScaleBackVector {get {return m_useScaleBackVector;} set {m_useScaleBackVector = value;}}
	public bool UseVectorsToClampInitialValues {get {return m_useVectorsToClampInitialValues;} set {m_useVectorsToClampInitialValues = value;}}
	
	#endregion
	
	#region Constructor
	
	public ScaleScheduler() : base()
	{
		CancelOnClick = false;
	}
	
	#endregion
	
	#region Overridden Methods
	
	protected override void OnPingStart()
	{
		InitScaleValueBy(true);
	}
	
	protected override void OnPongStart()
	{
		InitScaleValueBy(false);
	}
	
	protected override void OnPingUpdate()
	{
		SetAmountToScale(true, GetTimeScale());
	}
	
	protected override void OnPongUpdate()
	{
		SetAmountToScale(false, 1 - GetTimeScale());
	}
	
	/*protected override void OnPingFinish()
	{
		SetAmountToScale(true, 1f);
	}
	
	protected override void OnPongFinish()
	{
		SetAmountToScale(false, 1f);
	}*/
	
	#endregion
	
	#region Other Methods

	private void CheckIfCanClampInitialValues(bool p_isPing)
	{
		if(Owner != null)
		{
			if(UseVectorsToClampInitialValues && ScaleTypeOption != ScaleTypeOptionEnum.ScaleBy && ScaleTypeOption != ScaleTypeOptionEnum.ScalePercentBy)
			{
				if(p_isPing)
				{
					if(IsLocalScale)
					{
						Vector3 v_vectorLocal = new Vector3(Owner.transform.localScale.x, Owner.transform.localScale.y, Owner.transform.localScale.z);
						v_vectorLocal.x = EnumHelper.ContainsLongFlag((long)ScaleTypeOption,(long)ScaleTypeOptionEnum.ScaleToX)? ScaleBackVector.x : v_vectorLocal.x;
						v_vectorLocal.y = EnumHelper.ContainsLongFlag((long)ScaleTypeOption,(long)ScaleTypeOptionEnum.ScaleToY)? ScaleBackVector.y : v_vectorLocal.y;
						v_vectorLocal.z = EnumHelper.ContainsLongFlag((long)ScaleTypeOption,(long)ScaleTypeOptionEnum.ScaleToZ)? ScaleBackVector.z : v_vectorLocal.z;
						Owner.transform.localScale = v_vectorLocal;
					}
					else
					{
						Vector3 v_vectorGlobal = new Vector3(Owner.transform.lossyScale.x, Owner.transform.lossyScale.y, Owner.transform.lossyScale.z);
						v_vectorGlobal.x = EnumHelper.ContainsLongFlag((long)ScaleTypeOption,(long)ScaleTypeOptionEnum.ScaleToX)? ScaleBackVector.x : v_vectorGlobal.x;
						v_vectorGlobal.y = EnumHelper.ContainsLongFlag((long)ScaleTypeOption,(long)ScaleTypeOptionEnum.ScaleToY)? ScaleBackVector.y : v_vectorGlobal.y;
						v_vectorGlobal.z = EnumHelper.ContainsLongFlag((long)ScaleTypeOption,(long)ScaleTypeOptionEnum.ScaleToZ)? ScaleBackVector.z : v_vectorGlobal.z;
						KiltUtils.SetLossyScale(Owner.transform, v_vectorGlobal);
					}
				}
				else if (UseScaleBackVector)
				{
					if(IsLocalScale)
					{
						Owner.transform.localScale = ScaleVector;
					}
					else
					{
						KiltUtils.SetLossyScale(Owner.transform, ScaleVector);
					}
				}
			}
		}
	}
	
	public void InitScaleValueBy(bool p_isPing)
	{
		if(Owner != null)
		{
			CheckIfCanClampInitialValues(p_isPing);
			CurrentScaledValueBy = Vector3.zero;
			Vector3 v_scaleVector = !p_isPing && UseScaleBackVector? ScaleBackVector : ScaleVector;
			if(ScaleTypeOption == ScaleTypeOptionEnum.ScaleBy)
			{
				ScaleValueBy = v_scaleVector;
			}
			else if(ScaleTypeOption == ScaleTypeOptionEnum.ScalePercentBy)
			{
				Vector3 v_transformScale = IsLocalScale? Owner.transform.localScale : Owner.transform.lossyScale;

				Vector3 v_finalScaleResult = !UseScaleBackVector && !p_isPing? new Vector3(v_scaleVector.x == 0? 0 : v_transformScale.x/v_scaleVector.x, v_scaleVector.y == 0? 0 : v_transformScale.y/v_scaleVector.y, v_scaleVector.z == 0? 0 : v_transformScale.z/v_scaleVector.z) : new Vector3(v_transformScale.x*v_scaleVector.x, v_transformScale.y*v_scaleVector.y, v_transformScale.z*v_scaleVector.z);
				Vector3 v_finalScaleValueBy = new Vector3(v_finalScaleResult.x - v_transformScale.x, v_finalScaleResult.y - v_transformScale.y, v_finalScaleResult.z - v_transformScale.z);
				ScaleValueBy = v_finalScaleValueBy;
			}
			else if(p_isPing || (!p_isPing && UseScaleBackVector))
			{
				Vector3 v_initialScale = m_isLocalScale? Owner.transform.localScale : Owner.transform.lossyScale;
				Vector3 v_finalVector = new Vector3(v_scaleVector.x - v_initialScale.x, v_scaleVector.y - v_initialScale.y, v_scaleVector.z - v_initialScale.z);
				
				if(ScaleTypeOption == ScaleTypeOptionEnum.ScaleToX)
					v_finalVector = new Vector3(v_finalVector.x, 0,0);
				else if(ScaleTypeOption == ScaleTypeOptionEnum.ScaleToY)
					v_finalVector = new Vector3(0, v_finalVector.y,0);
				else if(ScaleTypeOption == ScaleTypeOptionEnum.ScaleToZ)
					v_finalVector = new Vector3(0, 0 ,v_finalVector.z);
				else if(ScaleTypeOption == ScaleTypeOptionEnum.ScaleToXY)
					v_finalVector = new Vector3(v_finalVector.x, v_finalVector.y , 0);
				else if(ScaleTypeOption == ScaleTypeOptionEnum.ScaleToXZ)
					v_finalVector = new Vector3(v_finalVector.x, 0 ,v_finalVector.z);
				else if(ScaleTypeOption == ScaleTypeOptionEnum.ScaleToYZ)
					v_finalVector = new Vector3(0, v_finalVector.y ,v_finalVector.z);
				ScaleValueBy = v_finalVector;
			}
		}
	}
	
	#endregion
	
	#region Gets and Sets
	
	protected virtual void SetAmountToScale(bool p_isPing, float p_timeScale)
	{
		if(Owner != null)
		{
			float v_timeScale = p_timeScale;
			Vector3 v_timeScaleVector = new Vector3(ScaleValueBy.x * v_timeScale, ScaleValueBy.y * v_timeScale, ScaleValueBy.z * v_timeScale);
			Vector3 v_vectorToAdd = new Vector3(v_timeScaleVector.x - CurrentScaledValueBy.x, v_timeScaleVector.y - CurrentScaledValueBy.y, v_timeScaleVector.z - CurrentScaledValueBy.z);
			CurrentScaledValueBy = v_timeScaleVector;
			if(!p_isPing && !UseScaleBackVector && ScaleTypeOption != ScaleTypeOptionEnum.ScalePercentBy)
				v_vectorToAdd = new Vector3(-v_vectorToAdd.x, -v_vectorToAdd.y, -v_vectorToAdd.z);
			if(m_isLocalScale)
				Owner.transform.localScale = new Vector3(Owner.transform.localScale.x + v_vectorToAdd.x , Owner.transform.localScale.y + v_vectorToAdd.y, Owner.transform.localScale.z + v_vectorToAdd.z);
			else
			{
				Vector3 v_newLossyScale = new Vector3(Owner.transform.lossyScale.x + v_vectorToAdd.x , Owner.transform.lossyScale.y + v_vectorToAdd.y, Owner.transform.lossyScale.z + v_vectorToAdd.z);
				KiltUtils.SetLossyScale(this.transform, v_newLossyScale);
			}
		}
	}
	
	#endregion
}
