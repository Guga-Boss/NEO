//----------------------------------------------
//
//         Copyright © 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace HeavyDutyInspector
{

	public class KeywordsConfig : ScriptableObject
	{

		public List<KeywordCategory> keyWordCategories = new List<KeywordCategory>();

	}

	[System.Serializable]
	public class KeywordCategory : System.Object
	{
		public string name;

		[NonSerialized]
		public bool expanded;

		public List<string> keywords = new List<string>();

		public KeywordCategory()
		{
			name = "";
		}

		public KeywordCategory(string name)
		{
			this.name = name;
		}
	}

}
