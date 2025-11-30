//----------------------------------------------
//
//         Copyright © 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;

public class ExampleTagList : MonoBehaviour {
#if !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
	[Comment("Use Unity's tag popup box to select tags in a list of strings. Allows you to delete a tag from anywhere in the list.", CommentType.Info)]
	[TagList]
	public List<string> tagsToFind;
#endif
}	
