//----------------------------------------------
//
//         Copyright © 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;

public class ExampleReorderableListNamedMonoBehaviour : MonoBehaviour {
#if !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
	[Comment("If your list contains objects extending NamedMonoBehaviour, you can alternatively choose to display them using the NamedMonoBehaviour drawer (instead of the default ComponentSelectionDrawer).", CommentType.Info)]
	[ReorderableList(false, true)]
	public List<FakeState> states;
#endif
}

