
/*WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW*\     (   (     ) )
|/                                                      \|       )  )   _((_
||  (c) Wanzyee Studio  < wanzyeestudio.blogspot.com >  ||      ( (    |_ _ |=n
|\                                                      /|   _____))   | !  ] U
\.ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ./  (_(__(S)   |___*/

using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

using Object = UnityEngine.Object;

namespace WanzyeeStudio.Editrix.Toolkit{

	/// <summary>
	/// Group or ungroup scene <c>UnityEngine.Transform</c> for better arrangement.
	/// </summary>
	/// 
	/// <remarks>
	/// Menu "GameObject/Group/Make Group" or hotkey Ctrl-G to group selected children.
	/// Menu "GameObject/Group/Ungroup Parent" or hotkey Ctrl-Shift-G to ungroup selected parents.
	/// Menu "GameObject/Group/Ungroup Children" or hotkey Ctrl-Alt-G to move selected children out from the group.
	/// </remarks>
	/// 
	public static class ObjectGrouper{

		#region Menu

		/// <summary>
		/// Make a group <c>UnityEngine.Transform</c> contains selected children in the scene, with hotkey Ctrl-G.
		/// </summary>
		[MenuItem("GameObject/Group/Make Group %g", false, 90)]
		public static void MakeGroup(){
			
			if(!MakeGroupValid()) throw new InvalidOperationException("No transform selected.");

			Selection.objects = new Object[]{MakeGroup(null, Selection.transforms).gameObject};

		}

		/// <summary>
		/// Check if <c>MakeGroup()</c> valid, any scene <c>UnityEngine.Transform</c> selected.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		[MenuItem("GameObject/Group/Make Group %g", true)]
		private static bool MakeGroupValid(){
			return Selection.transforms.Any();
		}

		/// <summary>
		/// Ungroup children <c>UnityEngine.Transform</c> from selected parent in the scene, with hotkey Ctrl-Shift-G.
		/// </summary>
		/// 
		/// <remarks>
		/// This'll destroy the parent if it has no other <c>UnityEngine.Component</c>.
		/// Works like <c>Transform.DetachChildren</c> but detach to upward parent instead of root.
		/// </remarks>
		/// 
		/*
		 * It causes crash when called with both destroying and selecting at once sometimes.
		 * It can be avoided by delay call, it may be an Unity internal execution order problem.
		 */
		[MenuItem("GameObject/Group/Ungroup Parent %#g", false, 90)]
		public static void UngroupParent(){
			
			if(!UngroupParentValid()) throw new InvalidOperationException("No parent transform selected.");

			EditorApplication.delayCall += () => {

				var _p = Selection.transforms.Where(_v => 0 < _v.childCount);

				var _c = _p.SelectMany(_v => UngroupParent(_v, 2 > _v.GetComponents<Component>().Length));

				Selection.objects = _c.Select(_v => _v.gameObject).ToArray();

			};

		}

		/// <summary>
		/// Check if <c>UngroupParent()</c> valid, any scene parent <c>UnityEngine.Transform</c> selected.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		[MenuItem("GameObject/Group/Ungroup Parent %#g", true)]
		private static bool UngroupParentValid(){
			return Selection.transforms.Any(_v => 0 < _v.childCount);
		}

		/// <summary>
		/// Ungroup selected children <c>UnityEngine.Transform</c> to upward parent, with hotkey Ctrl-Alt-G.
		/// </summary>
		[MenuItem("GameObject/Group/Ungroup Children %&g", false, 90)]
		public static void UngroupChildren(){

			if(!UngroupChildrenValid()) throw new InvalidOperationException("No child transform selected.");

			var _c = Selection.transforms.Where(_v => null != _v.parent).OrderBy(_v => -_v.GetSiblingIndex());

			foreach(var _v in _c) UngroupChild(_v);

		}

		/// <summary>
		/// Check if <c>UngroupParent()</c> valid, any scene child <c>UnityEngine.Transform</c> selected.
		/// </summary>
		/// <returns><c>true</c>, if valid.</returns>
		[MenuItem("GameObject/Group/Ungroup Children %&g", true)]
		private static bool UngroupChildrenValid(){
			return Selection.transforms.Any(_v => null != _v.parent);
		}

		#endregion


		#region Methods

		/// <summary>
		/// Filter the top level transforms, excluding prefabs, ordered by sibling index.
		/// </summary>
		/// <returns>The tops.</returns>
		/// <param name="transforms">Transforms.</param>
		public static Transform[] FilterTops(params Transform[] transforms){

			if(null == transforms) throw new ArgumentNullException("transforms");

			Func<Transform, string> _k = (o) => {
				var _s = string.Empty;
				for(var t = o; t != null; t = t.parent) _s = "." + t.GetSiblingIndex() + _s;
				return _s;
			};

			var _t = transforms.Distinct().Where(_v => null != _v && !EditorUtility.IsPersistent(_v)).ToArray();

			return _t.Where(_v => !_t.Any(_o => _o != _v && _v.IsChildOf(_o))).OrderBy(_k).ToArray();

		}

		/// <summary>
		/// Make a group <c>UnityEngine.Transform</c> contains specified children.
		/// </summary>
		/// <returns>The group parent.</returns>
		/// <param name="name">Name.</param>
		/// <param name="children">Children.</param>
		public static Transform MakeGroup(string name, params Transform[] children){

			if(null == children) throw new ArgumentNullException("children");

			children = FilterTops(children);
			if(!children.Any()) throw new ArgumentException("No children assigned.", "children");

			var _g = new GameObject(string.IsNullOrEmpty(name) ? "New Group" : name);
			var _result = children.All(_v => _v is RectTransform) ? _g.AddComponent<RectTransform>() : _g.transform;

			Undo.RegisterCreatedObjectUndo(_g, "Make Group");
			Undo.SetTransformParent(_result, children[0].parent, "Arrange Group");

			_result.SetSiblingIndex(children[0].GetSiblingIndex());
			foreach(var _v in children) Undo.SetTransformParent(_v, _result, "Join Group");

			return _result;

		}

		/// <summary>
		/// Ungroup all children <c>UnityEngine.Transform</c> from specified parent.
		/// </summary>
		/// 
		/// <remarks>
		/// Optional to destroy the original parent after done.
		/// Works like <c>UnityEngine.Transform.DetachChildren</c> but detach to upward parent instead of root.
		/// </remarks>
		/// 
		/// <returns>The children from the parent.</returns>
		/// <param name="parent">Parent.</param>
		/// <param name="destroy">If set to <c>true</c> destroy.</param>
		/// 
		public static Transform[] UngroupParent(Transform parent, bool destroy = false){

			if(null == parent) throw new ArgumentNullException("parent");

			var _result = parent.Cast<Transform>().Reverse().ToArray();
			if(!_result.Any()) throw new ArgumentException("No children included.", "parent");

			foreach(var _v in _result) UngroupChild(_v);

			if(destroy) Undo.DestroyObjectImmediate(parent.gameObject);

			return _result;

		}

		/// <summary>
		/// Ungroup a child <c>UnityEngine.Transform</c> from current parent to upward.
		/// </summary>
		/// <returns>The new parent.</returns>
		/// <param name="child">Child.</param>
		public static Transform UngroupChild(Transform child){

			if(null == child) throw new ArgumentNullException("child");

			var _p = child.parent;
			if(null == _p) throw new ArgumentException("Given child is a root.", "child");

			Undo.SetTransformParent(child, _p.parent, "Ungroup Child");

			child.SetAsLastSibling();
			child.SetSiblingIndex(_p.GetSiblingIndex() + 1);

			return child.parent;

		}

		#endregion

	}

}
