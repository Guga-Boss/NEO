//----------------------------------------------
//
//         Copyright © 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

public class ExampleChangeCheckCallback : NamedMonoBehaviour {

	[Comment("Use the ChangeCheckCallback attribute to get notified when a variable changes. Try it and change the Target variable.", CommentType.Info)]
	[HideVariable]
	public bool comment;

	[ChangeCheckCallback("UpdateName")]
	public GameObject target;

	void UpdateName()
	{
		scriptName = "Waypoint (" + target.ToString() + ")";
	}
}
