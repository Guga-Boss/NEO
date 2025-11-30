//----------------------------------------------
//
//         Copyright © 2014  Illogika
//----------------------------------------------

using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

public class ExampleHideConditional : MonoBehaviour {
#if !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2

	public enum TARGET_TYPE {
		Self,
		Position,
		Object
	}

	[Comment("Use the HiddenConditional attribute to hide some of your variables until a given boolean or enumeration condition is achieved.", CommentType.Info)]
	public bool hasTarget;

	[HideConditional("hasTarget", true)]
	public TARGET_TYPE targetType;

	[HideConditional("targetType", (int)TARGET_TYPE.Position)]
	public Vector3 positionTarget;

	[HideConditional("targetType", (int)TARGET_TYPE.Object)]
	public GameObject objectTarget;
#endif
}

