
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
	/// Helper to clear logs in Console window.
	/// </summary>
	/// 
	/// <remarks>
	/// Trigger from the menu "Window/Clear Console", or hotkey ALT-Shift-C.
	/// </remarks>
	/// 
	public static class ConsoleHelper{

		/// <summary>
		/// Clear the console logs, with hotkey Alt-Shift-C.
		/// </summary>
		/*
		 * Invoke twice to avoid only focusing the Console window in case.
		 */
		[MenuItem("Window/Clear Console &#c", false, 2200)]
		public static void ClearConsole(){

			if(!ClearConsoleValid()) throw new MissingMethodException("UnityEditorInternal.LogEntries", "Clear");

			_method.Invoke(null, null);
			_method.Invoke(null, null);

		}
		
		/// <summary>
		/// Check if <c>ClearConsole()</c> valid, reflection existing.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		[MenuItem("Window/Clear Console &#c", true)]
		private static bool ClearConsoleValid(){
			return (null != _method);
		}
		
		/// <summary>
		/// Method info to clear the console window.
		/// Reflect to <c>UnityEditorInternal.LogEntries.Clear()</c>.
		/// It's a public static method, without params, return <c>void</c>.
		/// </summary>
		/*
		 * http://answers.unity3d.com/questions/707636/
		 */
		private static readonly MethodInfo _method = new Func<MethodInfo>(() => {

			var _t = Type.GetType("UnityEditorInternal.LogEntries, UnityEditor");
			if(null == _t) return null;

			var _b = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			return _t.GetMethod("Clear", _b, null, new Type[0], null);

		})();

	}

}
