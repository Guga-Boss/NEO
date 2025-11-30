//----------------------------------------------
//
//      Copyright © 2013 - 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

public class ExampleComment : MonoBehaviour {

	[Comment("Quickly add comments to your variables.", CommentType.Info)]
	public bool isCommented = true;
}
