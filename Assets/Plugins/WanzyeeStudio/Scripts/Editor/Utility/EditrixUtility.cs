
/*WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW*\     (   (     ) )
|/                                                      \|       )  )   _((_
||  (c) Wanzyee Studio  < wanzyeestudio.blogspot.com >  ||      ( (    |_ _ |=n
|\                                                      /|   _____))   | !  ] U
\.ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ./  (_(__(S)   |___*/

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

using Object = UnityEngine.Object;

namespace WanzyeeStudio.Editrix{

	/// <summary>
	/// Include some convenient methods for editor or asset operation.
	/// </summary>
	public static class EditrixUtility{

		/// <summary>
		/// Get all main assets in the project folder.
		/// </summary>
		/// <returns>The all assets.</returns>
		public static Object[] GetAllAssets(){

			return AssetDatabase.FindAssets("t:Object"

				).Select(_v => AssetDatabase.GUIDToAssetPath(_v)
				).OrderBy(_v => _v

				).Select(_v => AssetDatabase.LoadMainAssetAtPath(_v) //guid only for main
				).Where(_v => null != _v

			).Distinct().ToArray();

		}

		/// <summary>
		/// Get all asset labels used in project, or only find the ones used by assigned assets.
		/// </summary>
		/// <returns>The asset labels.</returns>
		/// <param name="assets">Assets.</param>
		public static string[] GetAllAssetLabels(params Object[] assets){

			if(null == assets || 0 == assets.Length) assets = GetAllAssets();

			return assets.Distinct(

				).Where(_v => (null != _v && AssetDatabase.Contains(_v))
				).SelectMany(_v => AssetDatabase.GetLabels(_v)

				).Distinct(
				).OrderBy(_v => _v

			).ToArray();

		}
		
		/// <summary>
		/// Get an order <c>string</c> of given object for sorting.
		/// </summary>
		/// 
		/// <remarks>
		/// It's asset path, append with sibling if relative to <c>UnityEngine.GameObject</c>.
		/// Optional to sort asset or hierarchy object first.
		/// </remarks>
		/// 
		/// <returns>The order.</returns>
		/// <param name="obj">Object.</param>
		/// <param name="assetFirst">If set to <c>true</c> asset first.</param>
		/// 
		public static string GetObjectOrder(Object obj, bool assetFirst = true){

			var _result = "p_";

			if(!AssetDatabase.Contains(obj)) _result = "h_";
			else if(assetFirst) _result = "a_";
			
			_result += AssetDatabase.GetAssetPath(obj);
			
			if(obj is Component) obj = ((Component)obj).transform;
			else if(obj is GameObject) obj = ((GameObject)obj).transform;
			else return _result;
			
			var _s = string.Empty;
			for(var t = (Transform)obj; null != t; t = t.parent) _s = "." + t.GetSiblingIndex() + _s;
			
			return _result + _s;
			
		}

		/// <summary>
		/// Determine if the path can be used to create a file or directory.
		/// </summary>
		/// 
		/// <remarks>
		/// Optional to throw an exception message or just return <c>false</c> if invalid.
		/// Check <c>IoUtility.CheckCreatable()</c> at the first.
		/// Then return <c>true</c> if the file doesn't exist yet or force to <c>overwrite</c>.
		/// Otherwise popup a dialog for the user to make the decision.
		/// </remarks>
		/// 
		/// <returns><c>true</c> if is creatable; otherwise, <c>false</c>.</returns>
		/// <param name="path">Path.</param>
		/// <param name="overwrite">Overwrite.</param>
		/// <param name="exception">Flag to throw an exception or return <c>false</c>.</param>
		/// 
		public static bool CheckIoCreatable(string path, bool overwrite = false, bool exception = false){

			if(!IoUtility.CheckCreatable(path, exception)) return false;
			if(overwrite || !File.Exists(path)) return true;

			var _m = Path.GetFileName(path) + " already exists.\nDo you want to replace it?";
			if(EditorUtility.DisplayDialog("Confirm Save As", _m, "Yes", "No")) return true;

			if(exception) throw new OperationCanceledException(path + " already exists.");
			else return false;

		}

	}

}
