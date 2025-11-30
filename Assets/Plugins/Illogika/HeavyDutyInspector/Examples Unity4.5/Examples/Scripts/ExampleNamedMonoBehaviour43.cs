//----------------------------------------------
//
//      Copyright © 2013 - 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

public class ExampleNamedMonoBehaviour43 : NamedMonoBehaviour {

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
	[Comment("\n\nThis is the example for Unity 4.3. Please refer to the example for Unity 4.0 to 4.2 instead.\n\n", CommentType.Info, true)]
	public bool hidden1;
	
#else
	[Comment("Set names on your scripts, and even the color the name will be displayed in", CommentType.Info)]
	[HideVariable]
	public bool hidden;

	[Comment("View your script references with more than just the GameObject's name and the script's type. Understand at a glance which script is referenced.", CommentType.Info)]
	[NamedMonoBehaviourAttribute]
	public FakeState attackState;
	
	[Comment("Also use the NamedMonoBehaviourAttribute to display NamedMonoBehaviours in arrays or lists.", CommentType.Info)]
	[NamedMonoBehaviourAttribute]
	public FakeState[] allStates;
#endif

}
