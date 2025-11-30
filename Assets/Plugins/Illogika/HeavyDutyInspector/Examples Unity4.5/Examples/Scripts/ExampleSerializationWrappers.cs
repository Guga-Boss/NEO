//----------------------------------------------
//
//         Copyright © 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;
using Object = UnityEngine.Object;

public class ExampleSerializationWrappers : MonoBehaviour {

	public int intMinValue = int.MinValue;
	public int intMaxValue = int.MaxValue;

	public Int16 shortExample;
	public Int64 longExample;

	public UInt16 ushortExample;
	public UInt32 uintExample;
	public UInt64 ulongExample;

	public Int16S serializableShortExample;
	public Int64S serializableLongExample;

	public UInt16S serializableUshortExample;
	public UInt32S serializableUintExample;
	public UInt64S serializableUlongExample;


}
