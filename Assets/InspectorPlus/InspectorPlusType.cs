#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class InspectorPlusType {
	static AppDomain app = AppDomain.CurrentDomain;
	
    public static Type Get(string name)
    {
		foreach(Assembly a in app.GetAssemblies())
		{		
			Type t = a.GetType(name);
			
			if (t != null)
				return t;
		}
		
		return null;
    }
}
#endif