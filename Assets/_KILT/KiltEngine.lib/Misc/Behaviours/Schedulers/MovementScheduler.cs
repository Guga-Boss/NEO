using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Flags]
public enum MoveTypeOptionEnum {MoveBy = 1, MoveToX = 4, MoveToY = 8, MoveToZ = 16, MoveToXY = 12, MoveToYZ = 24, MoveToXZ = 20, MoveToXYZ = 28 }

public class MovementScheduler : TimeScheduler {

	#region Private Variables

	//[SerializeField]
	Vector3 m_currentDistanceMovedBy = Vector2.zero;
	Vector3 m_distanceToMoveBy = Vector2.zero;

	[SerializeField]
	Vector3 m_moveVector = Vector2.zero;
	[SerializeField]
	Vector3 m_moveBackVector = Vector2.zero;
	[SerializeField]
	MoveTypeOptionEnum m_moveTypeOption = MoveTypeOptionEnum.MoveBy;
	[SerializeField]
	bool m_isLocalDistance = false;
	[SerializeField]
	bool m_useMoveBackVector = false;
	[SerializeField]
	bool m_useVectorsToClampInitialValues = false;

	#endregion

	#region Protected Properties

	protected Vector3 CurrentDistanceMovedBy {get {return m_currentDistanceMovedBy;} set {m_currentDistanceMovedBy = value;}}
	protected Vector3 DistanceToMoveBy {get {return m_distanceToMoveBy;} set {m_distanceToMoveBy = value;}}

	#endregion

	#region Public Properties

	public Vector3 MoveVector {get {return m_moveVector;} set {m_moveVector = value;}}
	public Vector3 MoveBackVector {get {return m_moveBackVector;} set {m_moveBackVector = value;}}
	public MoveTypeOptionEnum MoveTypeOption {get {return m_moveTypeOption;} set {m_moveTypeOption = value;}}
	public bool IsLocalDistance {get {return m_isLocalDistance;} set {m_isLocalDistance = value;}}
	public bool UseMoveBackVector {get {return m_useMoveBackVector;} set {m_useMoveBackVector = value;}}
	public bool UseVectorsToClampInitialValues {get {return m_useVectorsToClampInitialValues;} set {m_useVectorsToClampInitialValues = value;}}

	#endregion

	#region Constructor

	public MovementScheduler() : base()
	{
		CancelOnClick = false;
	}
	
	#endregion

	#region Overridden Methods

	protected override void OnPingStart()
	{
		InitDistanceToMoveBy(true);
	}

	protected override void OnPongStart()
	{
		InitDistanceToMoveBy(false);
	}
	
	protected override void OnPingUpdate()
	{
		SetAmountToMove(true, GetTimeScale());
	}

	protected override void OnPongUpdate()
	{
		SetAmountToMove(false, 1 - GetTimeScale());
	}

	/*protected override void OnPingFinish()
	{
		SetAmountToMove(true, 1f);
	}

	protected override void OnPongFinish()
	{
		SetAmountToMove(false, 1f);
	}*/

	#endregion

	#region Other Methods

	private void CheckIfCanClampInitialValues(bool p_isPing)
	{
		if(Owner != null)
		{
			if(UseVectorsToClampInitialValues && MoveTypeOption != MoveTypeOptionEnum.MoveBy)
			{
				if(p_isPing)
				{
					Vector3 v_vector = GetTransformVector();
					v_vector.x = EnumHelper.ContainsLongFlag((long)MoveTypeOption,(long)MoveTypeOptionEnum.MoveToX)? MoveBackVector.x : v_vector.x;
					v_vector.y = EnumHelper.ContainsLongFlag((long)MoveTypeOption,(long)MoveTypeOptionEnum.MoveToY)? MoveBackVector.y : v_vector.y;
					v_vector.z = EnumHelper.ContainsLongFlag((long)MoveTypeOption,(long)MoveTypeOptionEnum.MoveToZ)? MoveBackVector.z : v_vector.z;
					SetTransformVector(v_vector);
				}
				else if (UseMoveBackVector)
				{
					SetTransformVector(MoveVector);
				}	
			}
		}
	}

	protected virtual Vector3 GetTransformVector()
	{
		Vector3 v_vectorToReturn = Vector3.zero;
		if(IsLocalDistance)
		{
			v_vectorToReturn = (Owner.transform is RectTransform)? ((RectTransform)Owner.transform).anchoredPosition3D : Owner.transform.localPosition;
		}
		else
		{
			v_vectorToReturn = new Vector3(Owner.transform.position.x, Owner.transform.position.y, Owner.transform.position.z);
		}
		return v_vectorToReturn;
	}

	protected virtual void SetTransformVector(Vector3 p_vector)
	{
		if(IsLocalDistance)
		{
			if(Owner.transform is RectTransform)
				((RectTransform)Owner.transform).anchoredPosition3D = p_vector;
			else
				Owner.transform.localPosition = p_vector;
		}
		else
			Owner.transform.position = p_vector;
	}

	public void InitDistanceToMoveBy(bool p_isPing)
	{
		if(Owner != null)
		{
			CheckIfCanClampInitialValues(p_isPing);
			CurrentDistanceMovedBy = Vector3.zero;
			Vector3 v_moveVector = !p_isPing && UseMoveBackVector? MoveBackVector : MoveVector;
			if(MoveTypeOption == MoveTypeOptionEnum.MoveBy)
			{
				DistanceToMoveBy = v_moveVector;
			}
			else if(p_isPing || (!p_isPing && UseMoveBackVector))
			{
				Vector3 v_initialPosition = GetTransformVector();
				Vector3 v_finalVector = new Vector3(v_moveVector.x - v_initialPosition.x, v_moveVector.y - v_initialPosition.y, v_moveVector.z - v_initialPosition.z);
				
				if(MoveTypeOption == MoveTypeOptionEnum.MoveToX)
					v_finalVector = new Vector3(v_finalVector.x, 0,0);
				else if(MoveTypeOption == MoveTypeOptionEnum.MoveToY)
					v_finalVector = new Vector3(0, v_finalVector.y,0);
				else if(MoveTypeOption == MoveTypeOptionEnum.MoveToZ)
					v_finalVector = new Vector3(0, 0 ,v_finalVector.z);
				else if(MoveTypeOption == MoveTypeOptionEnum.MoveToXY)
					v_finalVector = new Vector3(v_finalVector.x, v_finalVector.y , 0);
				else if(MoveTypeOption == MoveTypeOptionEnum.MoveToXZ)
					v_finalVector = new Vector3(v_finalVector.x, 0 ,v_finalVector.z);
				else if(MoveTypeOption == MoveTypeOptionEnum.MoveToYZ)
					v_finalVector = new Vector3(0, v_finalVector.y ,v_finalVector.z);
				DistanceToMoveBy = v_finalVector;
			}
		}
	}

	#endregion

	#region Gets and Sets

	protected virtual void SetAmountToMove(bool p_isPing, float p_timeScale)
	{
		if(Owner != null)
		{
			float v_timeScale = p_timeScale;
			Vector3 v_timeScaleVector = new Vector3(DistanceToMoveBy.x * v_timeScale, DistanceToMoveBy.y * v_timeScale, DistanceToMoveBy.z * v_timeScale);
			Vector3 v_vectorToAdd = new Vector3(v_timeScaleVector.x - CurrentDistanceMovedBy.x, v_timeScaleVector.y - CurrentDistanceMovedBy.y, v_timeScaleVector.z - CurrentDistanceMovedBy.z);
			CurrentDistanceMovedBy = v_timeScaleVector;
			if(!p_isPing && !UseMoveBackVector)
				v_vectorToAdd = new Vector3(-v_vectorToAdd.x, -v_vectorToAdd.y, -v_vectorToAdd.z);
			Vector3 v_finalVector = GetTransformVector();
			v_finalVector.x += v_vectorToAdd.x;
			v_finalVector.y += v_vectorToAdd.y;
			v_finalVector.z += v_vectorToAdd.z;
			SetTransformVector(v_finalVector);
		}
	}

	#endregion
}
