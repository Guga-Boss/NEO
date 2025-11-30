//----------------------------------------------
//
//      Copyright © 2013 - 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

public class ExampleComponentSelection43 : MonoBehaviour {
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
	[Comment("\n\nThis is the example for Unity 4.3. Please refer to the example for Unity 4.0 to 4.2 instead.\n\n", CommentType.Info, true)]
	public bool hidden1;
#else
	[Comment("Select components through a drop down menu that displays numbered components to easily identify which is which on overcharged GameObjects. Specify the name of a field belonging to this component to have its value displayed after the component's type and numbering, making it even easier to know which component you want to select.", CommentType.Info)]
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3
	public bool hidden;
#endif	
	[ComponentSelection("clip")]
	public AudioSource footstepsAudioSource;
	
	[Comment("Use the ComponentSelection Attribute to choose which component you want to select on another GameObject without having to open a second inspector.", CommentType.Info)]
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3
	public bool hidden2;
#endif	
	[ComponentSelection]
	public FakeState idleState;

	[Comment("New in Unity 4.3! Use the ComponentSelection Attribute with arrays or lists.", CommentType.Info)]
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3
	public bool hidden3;
#endif
	[ComponentSelection]
	public FakeStateAttack[] attackStates;
#endif
}
