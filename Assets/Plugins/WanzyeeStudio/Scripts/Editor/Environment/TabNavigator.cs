
/*WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW*\     (   (     ) )
|/                                                      \|       )  )   _((_
||  (c) Wanzyee Studio  < wanzyeestudio.blogspot.com >  ||      ( (    |_ _ |=n
|\                                                      /|   _____))   | !  ] U
\.ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ./  (_(__(S)   |___*/

using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WanzyeeStudio.Editrix.Toolkit{

	/// <summary>
	/// Utility to switch or close a tab or window.
	/// </summary>
	/// 
	/// <remarks>
	/// Menu "Window/View/Next Tab", with hotkey Ctrl-`, to focus the next of hovered or focused tab.
	/// Menu "Window/View/Previous Tab", with hotkey Ctrl-Shift-`, to focus the previous of hovered or focused tab.
	/// Menu "Window/View/Close Tab", with hotkey Ctrl-W, to close the hovered and focused window tab.
	/// Menu "Window/View/Close Window", with hotkey Ctrl-Shift-W, to close the hovered and focused window.
	/// Easy thing causes easy mistake, be careful to use this, since closing is not undoable.
	/// </remarks>
	/// 
	public static class TabNavigator{

		#region Menu

		/// <summary>
		/// Switch to the next window tab from the focused one, with hotkey Ctrl-`.
		/// </summary>
		[MenuItem("Window/View/Next Tab %`", false, 100)]
		public static void NextTab(){
			var _t = GetTab();
			_t.Value[(_t.Value.IndexOf(_t.Key) + 1) % _t.Value.Count].Focus();
		}

		/// <summary>
		/// Switch to the previous window tab from the focused one, with hotkey Ctrl-Shift-`.
		/// </summary>
		[MenuItem("Window/View/Previous Tab %#`", false, 100)]
		public static void PreviousTab(){
			var _t = GetTab();
			_t.Value[(_t.Value.IndexOf(_t.Key) + _t.Value.Count - 1) % _t.Value.Count].Focus();
		}

		/// <summary>
		/// Check if <c>NextTab()</c> or <c>PreviousTab</c> valid, multiple tabs in the hovered or focused tab.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		[MenuItem("Window/View/Next Tab %`", true)]
		[MenuItem("Window/View/Previous Tab %#`", true)]
		private static bool NextPreviousTabValid(){
			return null != GetTab(false).Key;
		}

		/// <summary>
		/// Close the focused window tab, with hotkey Ctrl-W.
		/// </summary>
		[MenuItem("Window/View/Close Tab %w", false, 100)]
		public static void CloseTab(){
			if(!CloseTabValid()) throw new InvalidOperationException("No window tab focused and hovered.");
			EditorWindow.focusedWindow.Close();
		}

		/// <summary>
		/// Check if <c>CloseTab()</c> valid, focused window is hovered and existing.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		[MenuItem("Window/View/Close Tab %w", true)]
		private static bool CloseTabValid(){
			return EditorWindow.mouseOverWindow == EditorWindow.focusedWindow && null != EditorWindow.focusedWindow;
		}

		/// <summary>
		/// Close the whole window contains the focused tab, with hotkey Ctrl-Shift-W.
		/// </summary>
		[MenuItem("Window/View/Close Window %#w", false, 100)]
		public static void CloseWindow(){

			var _w = (EditorWindow.mouseOverWindow == EditorWindow.focusedWindow) ? EditorWindow.focusedWindow : null;
			if(null == _w) throw new InvalidOperationException("No focused window tab hovered");

			try{ _closeMethod.Invoke(GetRoot(_w), null); }
			catch{ throw new MissingMemberException("Some reflections invalid."); }

		}

		/// <summary>
		/// Check if <c>CloseWindow()</c> valid, focused tab is hovered in a closable window.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		[MenuItem("Window/View/Close Window %#w", true)]
		private static bool CloseWindowValid(){
			var _w = EditorWindow.focusedWindow;
			return EditorWindow.mouseOverWindow == _w && null != GetRoot(_w);
		}

		#endregion


		#region Type Fields

		/// <summary>
		/// Type of dock area.
		/// </summary>
		private static readonly Type _dockType = Type.GetType("UnityEditor.DockArea, UnityEditor");

		/// <summary>
		/// Type of main window.
		/// </summary>
		private static readonly Type _mainType = Type.GetType("UnityEditor.MainWindow, UnityEditor");

		/// <summary>
		/// Type of window view.
		/// </summary>
		private static readonly Type _viewType = Type.GetType("UnityEditor.View, UnityEditor");

		/// <summary>
		/// Type of container window.
		/// </summary>
		private static readonly Type _contType = Type.GetType("UnityEditor.ContainerWindow, UnityEditor");

		#endregion


		#region Reflection Fields

		/// <summary>
		/// Field info to get the editor windows of a dock area.
		/// Reflect to <c>UnityEditor.DockArea.m_Panes</c>.
		/// It's an internal instance field, return <c>List</c> of <c>UnityEditor.EditorWindow</c>.
		/// </summary>
		private static readonly FieldInfo _panesField = new Func<FieldInfo>(() => {

			if(null == _dockType) return null;

			var _b = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			var _f = _dockType.GetField("m_Panes", _b);

			return typeof(List<EditorWindow>).IsAssignableFrom(_f.FieldType) ? _f : null;

		})();

		/// <summary>
		/// Field info to get the host view of a window.
		/// Reflect to <c>UnityEditor.EditorWindow.m_Parent</c>.
		/// It's an internal instance field, return <c>UnityEditor.View</c>.
		/// </summary>
		private static readonly FieldInfo _parentField = new Func<FieldInfo>(() => {

			if(null == _viewType) return null;

			var _b = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			var _f = typeof(EditorWindow).GetField("m_Parent", _b);

			return _viewType.IsAssignableFrom(_f.FieldType) ? _f : null;

		})();

		/// <summary>
		/// Property info to get the parent of a view.
		/// Reflect to <c>UnityEditor.View.parent</c>.
		/// It's a public instance property, without indexer, return <c>UnityEditor.View</c>.
		/// </summary>
		private static readonly PropertyInfo _parentProp = new Func<PropertyInfo>(() => {

			if(null == _viewType) return null;

			var _b = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			return _viewType.GetProperty("parent", _b, null, _viewType, new Type[0], null);

		})();

		/// <summary>
		/// Property info to get the window container of a view.
		/// Reflect to <c>UnityEditor.View.window</c>.
		/// It's a public instance property, without indexer, return <c>UnityEditor.ContainerWindow</c>.
		/// </summary>
		private static readonly PropertyInfo _windowProp = new Func<PropertyInfo>(() => {

			if(null == _viewType) return null;

			var _b = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			return _viewType.GetProperty("window", _b, null, _contType, new Type[0], null);

		})();

		/// <summary>
		/// Method info to close a window container.
		/// Reflect to <c>UnityEditor.ContainerWindow.Close()</c>.
		/// It's a public instance method, without params, return <c>void</c>.
		/// </summary>
		private static readonly MethodInfo _closeMethod = new Func<MethodInfo>(() => {
			
			if(null == _contType) return null;

			var _b = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			return _contType.GetMethod("Close", _b, null, new Type[0], null);

		})();

		#endregion


		#region Methods

		/// <summary>
		/// Get the switchable tab, i.e., hovered or focused tab with multiple siblings.
		/// Optional to throw an exception or return nothing.
		/// </summary>
		/// <returns>The tab.</returns>
		/// <param name="exception">Flag to throw an exception or return nothing.</param>
		private static KeyValuePair<EditorWindow, List<EditorWindow>> GetTab(bool exception = true){

			try{

				var _w = new[]{EditorWindow.mouseOverWindow, EditorWindow.focusedWindow}.Where(_v => null != _v);
				if(!_w.Any()) throw new InvalidOperationException("No hovered or focused window tab.");

				var _s = _w.Distinct().ToDictionary(_v => _v, _v => GetSiblings(_v)).Where(_v => 1 < _v.Value.Count);
				if(!_s.Any()) throw new InvalidOperationException("No other tabs in hovered or focused window.");

				return _s.First();

			}catch(Exception e){

				if(exception) throw e;
				else return new KeyValuePair<EditorWindow, List<EditorWindow>>();

			}

		}

		/// <summary>
		/// Get the sibling tabs of specified window tab, include itself.
		/// </summary>
		/// <returns>The siblings.</returns>
		/// <param name="window">Window.</param>
		private static List<EditorWindow> GetSiblings(EditorWindow window){

			if(null == window) throw new ArgumentNullException("window");
			if(null == _dockType) throw new TypeLoadException("Could not load type 'UnityEditor.DockArea'.");

			if(null == _panesField) throw new MissingFieldException("UnityEditor.DockArea", "m_Panes");
			if(null == _parentField) throw new MissingFieldException("UnityEditor.EditorWindow", "m_Parent");

			var _d = _parentField.GetValue(window);
			if(!_dockType.IsInstanceOfType(_d)) throw new InvalidOperationException("The window isn't in a dock area.");

			return (List<EditorWindow>)_panesField.GetValue(_d);

		}

		/// <summary>
		/// Get the root container of specified window, exclude editor main window which isn't closable.
		/// </summary>
		/// <returns>The root.</returns>
		/// <param name="window">Window.</param>
		private static object GetRoot(EditorWindow window){
			
			if(null == _mainType || null == _parentField || null == _parentProp || null == _windowProp) return null;

			if(null == _closeMethod || null == window) return null;

			var _r = _parentField.GetValue(window);

			for(var p = _r; null != p; p = _parentProp.GetValue(p, null)) _r = p;

			return _mainType.IsInstanceOfType(_r) ? null : _windowProp.GetValue(_r, null);

		}

		#endregion

	}

}
