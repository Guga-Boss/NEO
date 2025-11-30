using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//TODO: Build Inspector
[CustomEditor(typeof(MasterCallerScheduler))]
public class MasterCallerSchedulerInspector : Editor {

	MasterCallerScheduler m_controller;
	
	public override void OnInspectorGUI ()
	{
		m_controller = target as MasterCallerScheduler;

		//Do Logic Here
		
		if(GUI.changed)
			EditorUtility.SetDirty(m_controller);  
		
	}
}
