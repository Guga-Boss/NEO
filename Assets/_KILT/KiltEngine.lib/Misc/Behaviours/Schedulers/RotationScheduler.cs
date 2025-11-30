using UnityEngine;
using System.Collections;

[System.Flags]
public enum RotationTypeOptionEnum {RotateBy = 1, RotateToX = 4, RotateToY = 8, RotateToZ = 16, RotateToXY = 12, RotateToYZ = 24, RotateToXZ = 20, RotateToXYZ = 28 }

public class RotationScheduler : TimeScheduler {
	
	#region Private Variables
	
	//[SerializeField]
	Vector3 m_currentDistanceRotatedBy = Vector2.zero;
	Vector3 m_distanceToRotateBy = Vector2.zero;
	
	[SerializeField]
	Vector3 m_rotateVector = Vector2.zero;
	[SerializeField]
	Vector3 m_rotateBackVector = Vector2.zero;
	[SerializeField]
	RotationTypeOptionEnum m_rotationTypeOption = RotationTypeOptionEnum.RotateBy;
	[SerializeField]
	bool m_isLocalRotation = false;
	[SerializeField]
	bool m_useRotateBackVector = false;
	[SerializeField]
	bool m_useVectorsToClampInitialValues = false;
	
	#endregion
	
	#region Protected Properties
	
	protected Vector3 CurrentDistanceRotatedBy {get {return m_currentDistanceRotatedBy;} set {m_currentDistanceRotatedBy = value;}}
	protected Vector3 DistanceToRotateBy {get {return m_distanceToRotateBy;} set {m_distanceToRotateBy = value;}}
	
	#endregion
	
	#region Public Properties
	
	public Vector3 RotateVector {get {return m_rotateVector;} set {m_rotateVector = value;}}
	public Vector3 RotateBackVector {get {return m_rotateBackVector;} set {m_rotateBackVector = value;}}
	public RotationTypeOptionEnum RotationTypeOption {get {return m_rotationTypeOption;} set {m_rotationTypeOption = value;}}
	public bool IsLocalRotation {get {return m_isLocalRotation;} set {m_isLocalRotation = value;}}
	public bool UseRotateBackVector {get {return m_useRotateBackVector;} set {m_useRotateBackVector = value;}}
	public bool UseVectorsToClampInitialValues {get {return m_useVectorsToClampInitialValues;} set {m_useVectorsToClampInitialValues = value;}}
	
	#endregion
	
	#region Constructor
	
	public RotationScheduler() : base()
	{
		CancelOnClick = false;
	}
	
	#endregion
	
	#region Overridden Methods
	
	protected override void OnPingStart()
	{
		InitRotationToMoveBy(true);
	}
	
	protected override void OnPongStart()
	{
		InitRotationToMoveBy(false);
	}
	
	protected override void OnPingUpdate()
	{
		SetAmountToRotate(true, GetTimeScale());
	}
	
	protected override void OnPongUpdate()
	{
		SetAmountToRotate(false, 1 - GetTimeScale());
	}
	
	/*protected override void OnPingFinish()
	{
		SetAmountToRotate(true, 1f);
	}

	protected override void OnPongFinish()
	{
		SetAmountToRotate(false, 1f);
	}*/
	
	#endregion
	
	#region Other Methods
	
	private void CheckIfCanClampInitialValues(bool p_isPing)
	{
		if(Owner != null)
		{
			if(UseVectorsToClampInitialValues && RotationTypeOption != RotationTypeOptionEnum.RotateBy)
			{
				if(p_isPing)
				{
					if(IsLocalRotation)
					{
						Vector3 v_vectorLocal = new Vector3(Owner.transform.localEulerAngles.x, Owner.transform.localEulerAngles.y, Owner.transform.localEulerAngles.z);
						v_vectorLocal.x = EnumHelper.ContainsLongFlag((long)RotationTypeOption,(long)RotationTypeOptionEnum.RotateToX)? RotateBackVector.x : v_vectorLocal.x;
						v_vectorLocal.y = EnumHelper.ContainsLongFlag((long)RotationTypeOption,(long)RotationTypeOptionEnum.RotateToY)? RotateBackVector.y : v_vectorLocal.y;
						v_vectorLocal.z = EnumHelper.ContainsLongFlag((long)RotationTypeOption,(long)RotationTypeOptionEnum.RotateToZ)? RotateBackVector.z : v_vectorLocal.z;
						Owner.transform.localEulerAngles = v_vectorLocal;
					}
					else
					{
						Vector3 v_vectorGlobal = new Vector3(Owner.transform.eulerAngles.x, Owner.transform.eulerAngles.y, Owner.transform.eulerAngles.z);
						v_vectorGlobal.x = EnumHelper.ContainsLongFlag((long)RotationTypeOption,(long)RotationTypeOptionEnum.RotateToX)? RotateBackVector.x : v_vectorGlobal.x;
						v_vectorGlobal.y = EnumHelper.ContainsLongFlag((long)RotationTypeOption,(long)RotationTypeOptionEnum.RotateToY)? RotateBackVector.y : v_vectorGlobal.y;
						v_vectorGlobal.z = EnumHelper.ContainsLongFlag((long)RotationTypeOption,(long)RotationTypeOptionEnum.RotateToZ)? RotateBackVector.z : v_vectorGlobal.z;
						Owner.transform.eulerAngles = v_vectorGlobal;
					}
				}
				else if (UseRotateBackVector)
				{
					if(IsLocalRotation)
					{
						Owner.transform.localEulerAngles = RotateVector;
					}
					else
					{
						Owner.transform.eulerAngles = RotateVector;
					}
				}	
			}
		}
	}
	
	public void InitRotationToMoveBy(bool p_isPing)
	{
		if(Owner != null)
		{
			CheckIfCanClampInitialValues(p_isPing);
			CurrentDistanceRotatedBy = Vector3.zero;
			Vector3 v_rotateVector = !p_isPing && UseRotateBackVector? RotateBackVector : RotateVector;
			if(RotationTypeOption == RotationTypeOptionEnum.RotateBy)
			{
				DistanceToRotateBy = v_rotateVector;
			}
			else if(p_isPing || (!p_isPing && UseRotateBackVector))
			{
				Vector3 v_initialRotation = m_isLocalRotation? Owner.transform.localEulerAngles : Owner.transform.eulerAngles;
				Vector3 v_finalVector = new Vector3(v_rotateVector.x - v_initialRotation.x, v_rotateVector.y - v_initialRotation.y, v_rotateVector.z - v_initialRotation.z);
				
				if(RotationTypeOption == RotationTypeOptionEnum.RotateToX)
					v_finalVector = new Vector3(v_finalVector.x, 0,0);
				else if(RotationTypeOption == RotationTypeOptionEnum.RotateToY)
					v_finalVector = new Vector3(0, v_finalVector.y,0);
				else if(RotationTypeOption == RotationTypeOptionEnum.RotateToZ)
					v_finalVector = new Vector3(0, 0 ,v_finalVector.z);
				else if(RotationTypeOption == RotationTypeOptionEnum.RotateToXY)
					v_finalVector = new Vector3(v_finalVector.x, v_finalVector.y , 0);
				else if(RotationTypeOption == RotationTypeOptionEnum.RotateToXZ)
					v_finalVector = new Vector3(v_finalVector.x, 0 ,v_finalVector.z);
				else if(RotationTypeOption == RotationTypeOptionEnum.RotateToYZ)
					v_finalVector = new Vector3(0, v_finalVector.y ,v_finalVector.z);
				DistanceToRotateBy = v_finalVector;
			}
		}
	}
	
	#endregion
	
	#region Gets and Sets
	
	protected virtual void SetAmountToRotate(bool p_isPing, float p_timeScale)
	{
		if(Owner != null)
		{
			float v_timeScale = p_timeScale;
			Vector3 v_timeScaleVector = new Vector3(DistanceToRotateBy.x * v_timeScale, DistanceToRotateBy.y * v_timeScale, DistanceToRotateBy.z * v_timeScale);
			Vector3 v_vectorToAdd = new Vector3(v_timeScaleVector.x - CurrentDistanceRotatedBy.x, v_timeScaleVector.y - CurrentDistanceRotatedBy.y, v_timeScaleVector.z - CurrentDistanceRotatedBy.z);
			CurrentDistanceRotatedBy = v_timeScaleVector;
			if(!p_isPing && !UseRotateBackVector)
				v_vectorToAdd = new Vector3(-v_vectorToAdd.x, -v_vectorToAdd.y, -v_vectorToAdd.z);
			if(m_isLocalRotation)
				Owner.transform.localEulerAngles = new Vector3(Owner.transform.localEulerAngles.x + v_vectorToAdd.x , Owner.transform.localEulerAngles.y + v_vectorToAdd.y, Owner.transform.localEulerAngles.z + v_vectorToAdd.z);
			else
				Owner.transform.eulerAngles = new Vector3(Owner.transform.eulerAngles.x + v_vectorToAdd.x , Owner.transform.eulerAngles.y + v_vectorToAdd.y, Owner.transform.eulerAngles.z + v_vectorToAdd.z);
		}
	}
	
	#endregion
}
