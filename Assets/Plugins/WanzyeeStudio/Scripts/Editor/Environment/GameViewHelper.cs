
/*WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW*\     (   (     ) )
|/                                                      \|       )  )   _((_
||  (c) Wanzyee Studio  < wanzyeestudio.blogspot.com >  ||      ( (    |_ _ |=n
|\                                                      /|   _____))   | !  ] U
\.ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ./  (_(__(S)   |___*/

using UnityEditor;
using System;
using System.Reflection;

namespace WanzyeeStudio.Editrix.Toolkit{

	/// <summary>
	/// Helper to undock and fix current Game view size in pixel unit absolutely.
	/// </summary>
	/// 
	/// <remarks>
	/// Apply by clicking menu "Window/View/Fix Game View Size".
	/// The target window found by the order below:
	/// 	1. Game view with mouse over.
	/// 	2. Current focused Game view.
	/// 	3. The main Game view.
	/// </remarks>
	/// 
	/// <remarks>
	/// It's useful to preview the real size in Game view, pixel by pixel, not ratio scaled.
	/// Set window to selected size on the aspect drop-down menu, only for "Fixed Resolution".
	/// Use this to easily set size and save presets with the built-in feature.
	/// It might be incorrect if the size is too big to close even over the monitor.
	/// </remarks>
	/// 
	/// <remarks>
	/// Note, this works by reflection to access internal classes.
	/// We'd try to keep it up-to-date, but can't guarantee.
	/// </remarks>
	/// 
	/*
	 * http://answers.unity3d.com/questions/179775/
	 */
	public static class GameViewHelper{

		#region Menu
		
		/// <summary>
		/// Resize game view to selected fixed resolution.
		/// </summary>
		[MenuItem("Window/View/Fix Game View Size", false, 205)]
		public static void FixGameViewSize(){
			if(FixGameViewSizeValid()) FixSize();
			else throw new MissingMemberException("Some reflections invalid.");
		}
		
		/// <summary>
		/// Check if <c>FixGameViewSize()</c> valid, resizable window existing.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		[MenuItem("Window/View/Fix Game View Size", true)]
		private static bool FixGameViewSizeValid(){
			EditorWindow _w;
			object _s;
			return GetResizable(out _w, out _s);
		}

		#endregion


		#region Fields
		
		/// <summary>
		/// Type of Game view window.
		/// </summary>
		private static readonly Type _windowType = Type.GetType("UnityEditor.GameView, UnityEditor");
		
		/// <summary>
		/// Type of <c>UnityEditor.GameViewSize</c>.
		/// With custom setting info of game view resolution, not the displayed.
		/// </summary>
		private static readonly Type _sizeType = Type.GetType("UnityEditor.GameViewSize, UnityEditor");
		
		/// <summary>
		/// Method info to get main game view.
		/// Reflect to <c>UnityEditor.GameView.GetMainGameView()</c>.
		/// It's a private static method, without params, return <c>UnityEditor.GameView</c>.
		/// </summary>
		private static readonly MethodInfo _mainMehtod = new Func<MethodInfo>(() => {
			
			if(null == _windowType) return null;

			var _b = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

			var _m = _windowType.GetMethod("GetMainGameView", _b, null, new Type[0], null);
			
			return (_windowType == _m.ReturnType) ? _m : null;
			
		})();
		
		/// <summary>
		/// Property info to get current setting of game view size.
		/// Reflect to <c>UnityEditor.GameView.currentGameViewSize</c>.
		/// It's a private instance property, without indexer, return <c>UnityEditor.GameViewSize</c>.
		/// </summary>
		private static readonly PropertyInfo _sizeProp = (
			FindProperty(_windowType, "currentGameViewSize", _sizeType)
		);
		
		/// <summary>
		/// Property info to get type of game view size, 0 as AspectRatio, 1 as FixedResolution.
		/// Reflect to <c>UnityEditor.GameViewSize.sizeType</c>.
		/// It's a public instance property, without indexer, return <c>UnityEditor.GameViewSizeType</c>.
		/// </summary>
		private static readonly PropertyInfo _sizeTypeProp = FindProperty(
			_sizeType,
			"sizeType",
			Type.GetType("UnityEditor.GameViewSizeType, UnityEditor")
		);
		
		/// <summary>
		/// Property info to get width of game view size.
		/// Reflect to <c>UnityEditor.GameViewSize.width</c>.
		/// It's a public instance property, without indexer, return <c>int</c>.
		/// </summary>
		private static readonly PropertyInfo _widthProp = (
			FindProperty(_sizeType, "width", typeof(int))
		);
		
		/// <summary>
		/// Property info to get height of game view size.
		/// Reflect to <c>UnityEditor.GameViewSize.height</c>.
		/// It's a public instance property, without indexer, return <c>int</c>.
		/// </summary>
		private static readonly PropertyInfo _heightProp = (
			FindProperty(_sizeType, "height", typeof(int))
		);

		#endregion


		#region Methods
		
		/// <summary>
		/// Get the instance property of specific define type by name and return type.
		/// </summary>
		/// <returns>The game view size property.</returns>
		/// <param name="defineType">Define type.</param>
		/// <param name="name">Name.</param>
		/// <param name="returnType">Return type.</param>
		private static PropertyInfo FindProperty(Type defineType, string name, Type returnType){
			
			if(null == returnType || null == defineType) return null;

			var _b = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			return defineType.GetProperty(name, _b, null, returnType, new Type[0], null);
			
		}
		
		/// <summary>
		/// Undock and fix current game view size in pixel unit.
		/// Set window size as Aspect Drop-Down menu, only for fixed resolution.
		/// </summary>
		private static void FixSize(){
			
			EditorWindow _w;
			object _s;
			if(!GetResizable(out _w, out _s)) return;
			
			var _p = _w.position; //get origin
			
			_p.width = (int)_widthProp.GetValue(_s, null);
			_p.height = (int)_heightProp.GetValue(_s, null) + 17; //plus control bar
			
			_w.position = _p; //first set back, make window out of dock
			_w.position = _p; //double set to make sure resize
			
		}
		
		/// <summary>
		/// Get game view and check if able to fix size.
		/// Check reflections first, then try to get and check values used.
		/// Also valid for FixedResolution only.
		/// </summary>
		/// <returns><c>true</c>, if able to fix size.</returns>
		/// <param name="window">GameView Window.</param>
		/// <param name="size">GameViewSize.</param>
		private static bool GetResizable(out EditorWindow window, out object size){
			
			window = null;
			size = null;
			
			if(null == _mainMehtod || null == _sizeProp) return false;
			if(null == _sizeTypeProp || null == _widthProp || null == _heightProp) return false;
			
			window = GetCurrent();
			if(null == window) return false;
			
			size = _sizeProp.GetValue(window, null);
			return (null != size) && (1 == (int)_sizeTypeProp.GetValue(size, null));
			
		}
		
		/// <summary>
		/// Get the current game view.
		/// Window with mouse over first, then focused, otherwise find main.
		/// </summary>
		/// <returns>The current window.</returns>
		private static EditorWindow GetCurrent(){

			if(_windowType.IsInstanceOfType(EditorWindow.mouseOverWindow)) return EditorWindow.mouseOverWindow;

			else if(_windowType.IsInstanceOfType(EditorWindow.focusedWindow)) return EditorWindow.focusedWindow;
			
			else return _mainMehtod.Invoke(null, null) as EditorWindow;
			
		}
		
		#endregion

	}

}
