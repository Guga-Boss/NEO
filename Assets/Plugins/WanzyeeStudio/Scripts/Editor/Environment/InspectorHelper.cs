
/*WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW*\     (   (     ) )
|/                                                      \|       )  )   _((_
||  (c) Wanzyee Studio  < wanzyeestudio.blogspot.com >  ||      ( (    |_ _ |=n
|\                                                      /|   _____))   | !  ] U
\.ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ./  (_(__(S)   |___*/

using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

using Object = UnityEngine.Object;

namespace WanzyeeStudio.Editrix.Toolkit{

	/// <summary>
	/// Open new Inspector to edit specified object, and toggle Inspector states.
	/// </summary>
	/// 
	/// <remarks>
	/// Use <c>UnityEngine.Object</c> context menu "Inspect" to show single object in a new locked Inspector.
	/// Also able for <c>UnityEngine.Component</c>, useful to edit multiple on different <c>UnityEngine.GameObject</c>.
	/// And component context menu "Select" to select its <c>UnityEngine.GameObject</c>, useful in the indie Inspector.
	/// </remarks>
	/// 
	/// <remarks>
	/// Menu "Window/View/Inspect Selected", with hotkey Ctrl-I, to show selected objects in a new locked Inspector.
	/// Menu "Window/View/Toggle Inspector Mode", with hotkey Alt-I, to toggle debug mode of an Inspector.
	/// Menu "Window/View/Toggle Inspector Lock", with hotkey Ctrl-Shift-I, to toggle lock state of an Inspector.
	/// Toggle the one with mouse over, or focused, or the single one if multiple, otherwise do nothing.
	/// </remarks>
	/// 
	public static class InspectorHelper{

		#region Menu

		/// <summary>
		/// Open new locked Inspector window to show current component.
		/// </summary>
		/// <param name="command">Command.</param>
		[MenuItem("CONTEXT/Object/Inspect", false, 10000)]
		private static void ObjectInspect(MenuCommand command){
			Inspect(command.context);
		}

		/// <summary>
		/// Check if <c>ObjectInspect()</c> valid, reflection existing.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		/// <param name="command">Command.</param>
		[MenuItem("CONTEXT/Object/Inspect", true)]
		private static bool ObjectInspectValid(MenuCommand command){
			return null != _lockProp;
		}

		/// <summary>
		/// Ping and select the <c>UnityEngine.GameObject</c> of current component.
		/// </summary>
		/// <param name="command">Command.</param>
		[MenuItem("CONTEXT/Component/Select", false, 705)]
		private static void ComponentSelect(MenuCommand command){
			
			var _g = (command.context as Component).gameObject;

			EditorGUIUtility.PingObject(_g);

			Selection.activeObject = _g;

		}

		/// <summary>
		/// Open new locked Inspector window to show current selected objects, with hotkey Ctrl-I.
		/// </summary>
		[MenuItem("Window/View/Inspect Selected %i", false, 200)]
		public static void InspectSelected(){
			
			if(null == Selection.activeObject) throw new InvalidOperationException("Nothing is selected.");

			Inspect(Selection.objects);

		}

		/// <summary>
		/// Check if <c>InspectSelected()</c> valid, reflection existing and anything selected.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		[MenuItem("Window/View/Inspect Selected %i", true)]
		private static bool InspectSelectedValid(){
			return null != _lockProp && null != Selection.activeObject;
		}

		/// <summary>
		/// Toggle Inspector debug mode, with hotkey Alt-I.
		/// </summary>
		[MenuItem("Window/View/Toggle Inspector Mode &i", false, 200)]
		public static void ToggleInspectorMode(){

			var _i = GetInspector();

			SetMode(_i, (InspectorMode.Normal == GetMode(_i)) ? InspectorMode.Debug : InspectorMode.Normal);

		}

		/// <summary>
		/// Check if <c>ToggleInspectorMode()</c> valid, reflection valid and toggleable window found.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		[MenuItem("Window/View/Toggle Inspector Mode &i", true)]
		private static bool ToggleInspectorModeValid(){
			return null != _modeField && null != _modeMehtod && null != GetInspector(false);
		}

		/// <summary>
		/// Toggle Inspector lock state, with hotkey Ctrl-Shift-I.
		/// </summary>
		[MenuItem("Window/View/Toggle Inspector Lock %#i", false, 200)]
		public static void ToggleInspectorLock(){
			
			var _i = GetInspector();

			SetLocked(_i, !GetLocked(_i));

		}

		/// <summary>
		/// Check if <c>ToggleInspectorLock()</c> valid, reflection valid and toggleable window found.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		[MenuItem("Window/View/Toggle Inspector Lock %#i", true)]
		private static bool ToggleInspectorLockValid(){
			return null != _lockProp && null != GetInspector(false);
		}

		#endregion


		#region Fields

		/// <summary>
		/// Type of Inspector window.
		/// </summary>
		private static readonly Type _windowType = Type.GetType("UnityEditor.InspectorWindow, UnityEditor");

		/// <summary>
		/// Field info to get the mode of an Inspector.
		/// Reflect to <c>UnityEditor.InspectorWindow.m_InspectorMode</c>.
		/// It's a public instance field, return <c>UnityEditor.InspectorMode</c>.
		/// </summary>
		private static readonly FieldInfo _modeField = new Func<FieldInfo>(() => {

			if(null == _windowType) return null;

			var _b = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			var _f = _windowType.GetField("m_InspectorMode", _b);

			return (typeof(InspectorMode) == _f.FieldType) ? _f : null;

		})();

		/// <summary>
		/// Method info to set the mode of an Inspector.
		/// Reflect to <c>UnityEditor.InspectorWindow.SetMode()</c>.
		/// It's a private instance method, with a param <c>UnityEditor.InspectorMode</c>, return <c>void</c>.
		/// </summary>
		private static readonly MethodInfo _modeMehtod = new Func<MethodInfo>(() => {

			if(null == _windowType) return null;

			var _b = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			return _windowType.GetMethod("SetMode", _b, null, new []{typeof(InspectorMode)}, null);

		})();

		/// <summary>
		/// Property info to lock an Inspector.
		/// Reflect to <c>UnityEditor.InspectorWindow.isLocked</c>.
		/// It's a public instance property, without indexer, return <c>bool</c>.
		/// </summary>
		private static readonly PropertyInfo _lockProp = new Func<PropertyInfo>(() => {

			if(null == _windowType) return null;

			var _b = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			return _windowType.GetProperty("isLocked", _b, null, typeof(bool), new Type[0], null);

		})();

		#endregion


		#region Methods

		/// <summary>
		/// Get the current Inspector which is mouse hovered or focused, or the single instance.
		/// Otherwise optional to throw an exception or return <c>null</c>.
		/// </summary>
		/// <returns>The inspector.</returns>
		/// <param name="exception">Flag to throw an exception or return <c>null</c>.</param>
		private static EditorWindow GetInspector(bool exception = true){

			if(_windowType.IsInstanceOfType(EditorWindow.mouseOverWindow)) return EditorWindow.mouseOverWindow;
			if(_windowType.IsInstanceOfType(EditorWindow.focusedWindow)) return EditorWindow.focusedWindow;

			var _w = Resources.FindObjectsOfTypeAll(_windowType);

			if(1 == _w.Length) return _w[0] as EditorWindow;
			if(!exception) return null;

			if(0 == _w.Length) throw new InvalidOperationException("There's no Inspector window.");
			else throw new AmbiguousMatchException("There're multiple Inspector, please hover or focus one.");

		}

		/// <summary>
		/// Open new locked Inspector window to show specified objects.
		/// </summary>
		/// <returns>The new Inspector window.</returns>
		/// <param name="targets">Targets.</param>
		public static EditorWindow Inspect(params Object[] targets){

			if(null == _lockProp) throw new MissingMemberException("UnityEditor.InspectorWindow", "isLocked");
			if(null == targets) throw new ArgumentNullException("targets");

			var _t = targets.Distinct().Where(_v => null != _v);
			if(!_t.Any()) throw new ArgumentException("No target assigned.", "targets");

			var _result = ScriptableObject.CreateInstance(_windowType) as EditorWindow;
			_result.Show();

			var _s = Selection.objects;
			Selection.objects = _t.ToArray();

			SetLocked(_result, true);
			Selection.objects = _s;

			return _result;

		}

		/// <summary>
		/// Get the inspector mode of specified Inspector.
		/// </summary>
		/// <returns>The mode.</returns>
		/// <param name="inspector">Inspector.</param>
		public static InspectorMode GetMode(EditorWindow inspector){

			if(!_windowType.IsInstanceOfType(inspector)) throw new ArgumentException("The window isn't an Inspector.");

			if(null == _modeField) throw new MissingFieldException("UnityEditor.InspectorWindow", "m_InspectorMode");

			return (InspectorMode)_modeField.GetValue(inspector);

		}

		/// <summary>
		/// Sets the mode.
		/// </summary>
		/// <param name="inspector">Inspector.</param>
		/// <param name="mode">Mode.</param>
		public static void SetMode(EditorWindow inspector, InspectorMode mode){

			if(!_windowType.IsInstanceOfType(inspector)) throw new ArgumentException("The window isn't an Inspector.");

			if(null == _modeMehtod) throw new MissingMethodException("UnityEditor.InspectorWindow", "SetMode");

			_modeMehtod.Invoke(inspector, new object[]{mode});

		}

		/// <summary>
		/// Get the lock state of specified Inspector.
		/// </summary>
		/// <returns><c>true</c>, if locked, <c>false</c> otherwise.</returns>
		/// <param name="inspector">Inspector.</param>
		public static bool GetLocked(EditorWindow inspector){

			if(!_windowType.IsInstanceOfType(inspector)) throw new ArgumentException("The window isn't an Inspector.");

			if(null == _lockProp) throw new MissingMemberException("UnityEditor.InspectorWindow", "isLocked");

			return (bool)_lockProp.GetValue(inspector, null);

		}

		/// <summary>
		/// Set the lock state of specified Inspector.
		/// </summary>
		/// <param name="inspector">Inspector.</param>
		/// <param name="locked">If set to <c>true</c> locked.</param>
		public static void SetLocked(EditorWindow inspector, bool locked){

			if(!_windowType.IsInstanceOfType(inspector)) throw new ArgumentException("The window isn't an Inspector.");

			if(null == _lockProp) throw new MissingMemberException("UnityEditor.InspectorWindow", "isLocked");

			_lockProp.SetValue(inspector, locked, null);

		}

		#endregion

	}

}
