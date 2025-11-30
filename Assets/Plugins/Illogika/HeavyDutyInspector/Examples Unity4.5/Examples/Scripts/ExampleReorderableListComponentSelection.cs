//----------------------------------------------
//
//         Copyright © 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;

public class ExampleReorderableListComponentSelection : MonoBehaviour {
#if !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
	[Comment("When applied to a list of objects extending the Component class, reorderable list will display these references with the ComponentSelection drawer.", CommentType.Info)]
	[ReorderableList]
	public List<FakeState> states;
#endif
}

