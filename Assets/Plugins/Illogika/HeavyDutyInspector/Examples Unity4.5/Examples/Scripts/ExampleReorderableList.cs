//----------------------------------------------
//
//      Copyright © 2013 - 2014  Illogika
//----------------------------------------------
#if !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;

public class ExampleReorderableList : MonoBehaviour {
	
	[Comment("Tired of dragging countless references all over again because you needed to insert an element in a List? Or worse, re-filling in every element manually because it was a list of custom serializable objects? With the ReorderableList attribute, you can move elements up and down and insert or delete new elements anywhere in the list, not just at the end.", CommentType.Info)]
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3
	public bool hidden;
#endif
	[ReorderableList]
	public List<string> vegetables;
}
#endif
