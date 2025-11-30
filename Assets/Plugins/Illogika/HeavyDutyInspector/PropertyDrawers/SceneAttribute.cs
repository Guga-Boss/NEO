//----------------------------------------------
//
//         Copyright © 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	public class SceneAttribute : PropertyAttribute {

		public string BasePath
		{
			get;
			private set;
		}

		public SceneAttribute(string basePath)
		{
			BasePath = basePath;
		}
	}
	
}
	