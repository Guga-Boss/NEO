//----------------------------------------------
//
//      Copyright © 2013 - 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;

public enum VegetableType { Bulb, Grain, Fruit, Plant, Squash };

[System.Serializable]
public class Vegetable : System.Object
{
#if !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
	public string name;

	public float plantHeight;
	public VegetableType vegetableType;
	public Texture2D image;

	[ComponentSelection]
	public Component component;

	[NamedMonoBehaviour]
	public FakeState namedMonoBeahviour;
#endif
}

public class ExampleReorderableListCustomSerializableObject : MonoBehaviour {
#if !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
	[Comment("Tired of dragging countless references all over again because you needed to insert an element in a List? Or worse, re-filling in every element manually because it was a list of custom serializable objects? With the ReorderableList attribute, you can move elements up and down and insert or delete new elements anywhere in the list, not just at the end.\n\nYou can even use ComponentSelection and NamedMonoBehaviour attributes in custom serializable objects inside your ReorderableList. Just add a flag to the constructor to have reorderable list calculate the right vertical spacing.", CommentType.Info)]
	[ReorderableList(true)]
	public List<Vegetable> vegetables;
#endif
}

