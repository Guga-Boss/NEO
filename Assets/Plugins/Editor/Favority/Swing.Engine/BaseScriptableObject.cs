using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Swing.Engine
{
	public abstract partial class BaseScriptableObject : ScriptableObject
	{
		// This function is called when the object is loaded.
		protected virtual void OnEnable() { }
		// This function is called when the scriptable object goes out of scope.
		protected virtual void OnDisable() { }
		// This function is called when the scriptable object will be destroyed.
		protected virtual void OnDestroy() { }
	}
}
