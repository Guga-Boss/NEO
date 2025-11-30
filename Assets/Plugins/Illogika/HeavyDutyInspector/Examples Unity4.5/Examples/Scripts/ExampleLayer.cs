//----------------------------------------------
//
//      Copyright © 2013 - 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

public class ExampleLayer : MonoBehaviour {

	[Comment("Easily select a layer from Unity's layer popup and store it as an integer", CommentType.Info)]
	[Layer]
	public int affectedLayer;

}
