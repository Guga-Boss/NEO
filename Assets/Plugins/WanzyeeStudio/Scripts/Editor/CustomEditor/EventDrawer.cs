
/*WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW*\     (   (     ) )
|/                                                      \|       )  )   _((_
||  (c) Wanzyee Studio  < wanzyeestudio.blogspot.com >  ||      ( (    |_ _ |=n
|\                                                      /|   _____))   | !  ] U
\.ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ./  (_(__(S)   |___*/

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WanzyeeStudio.Editrix.Drawer{

	/// <summary>
	/// <c>UnityEditor.CustomPropertyDrawer</c> for <c>UnityEngine.Events.UnityEvent</c>.
	/// </summary>
	/// 
	/// <remarks>
	/// Unity doesn't allow to reorder the event list by default with unknown reason.
	/// And not support to select between multiple components of the same type.
	/// This overrides the original drawer to make the <c>UnityEditorInternal.ReorderableList</c> draggable.
	/// And modifies the <c>UnityEditor.GenericMenu</c> items to identify each component.
	/// Note, this works by reflection, it might fail if Unity changes the code in the future.
	/// </remarks>
	/// 
	/*
	 * Since the type of UnityEventBase has been used by UnityEditorInternal.UnityEventDrawer.
	 * Declare multiple attributes to override, user may add or remove custom type manually in case.
	 */
	[CustomPropertyDrawer(typeof(UnityEvent), true)]
	[CustomPropertyDrawer(typeof(UnityEvent<bool>), true)]
	[CustomPropertyDrawer(typeof(UnityEvent<float>), true)]
	[CustomPropertyDrawer(typeof(UnityEvent<int>), true)]
	[CustomPropertyDrawer(typeof(UnityEvent<string>), true)]
	[CustomPropertyDrawer(typeof(UnityEvent<Object>), true)]
	[CustomPropertyDrawer(typeof(UnityEvent<Vector2>), true)]
	[CustomPropertyDrawer(typeof(UnityEvent<Vector3>), true)]
	[CustomPropertyDrawer(typeof(UnityEvent<Color>), true)]
	[CustomPropertyDrawer(typeof(UnityEvent<BaseEventData>), true)]
	internal class EventDrawer : UnityEventDrawer{

		#region Fields

		/// <summary>
		/// The stored property paths which has been initialized.
		/// </summary>
		private readonly List<string> _inited = new List<string>();

		/// <summary>
		/// The current drawing list, used to get element <c>UnityEditor.SerializedProperty</c>.
		/// </summary>
		private ReorderableList _current;

		#endregion


		#region Methods

		/// <summary>
		/// Initialize the list of the specified <c>UnityEditor.SerializedProperty</c>.
		/// Called when <c>GetPropertyHeight()</c> be invoked since it's the first GUI entry.
		/// Set it reorderable, and register new element drawing callback.
		/// </summary>
		/// <param name="property">Property.</param>
		private void Initialize(SerializedProperty property){

			_inited.Add(property.propertyPath);

			try{

				var _b = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

				var _s = typeof(UnityEventDrawer).GetMethod("GetState", _b).Invoke(this, new object[]{property});
				var _l = (ReorderableList)typeof(State).GetField("m_ReorderableList", _b).GetValue(_s);

				_l.draggable = true;

				var _c = _l.drawElementCallback;
				_l.drawElementCallback = (r, i, a, f) => _current = _l;
				_l.drawElementCallback += DrawElement;
				_l.drawElementCallback += _c;

			}catch{}

		}

		/// <summary>
		/// Draw the element of events, callback for reorderable list.
		/// Check if need to override the menu button and show the modified menu.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawElement(Rect rect, int index, bool isActive, bool isFocused){

			try{

				var _e = _current.serializedProperty.GetArrayElementAtIndex(index);

				var _t = _e.FindPropertyRelative("m_Target").objectReferenceValue;

				if(!(_t is GameObject || _t is Component)) return;

				var _r = Rect.MinMaxRect(rect.x + (rect.width * 0.3f) + 3f, rect.y, rect.xMax + 2f, rect.y + 21f);

				if(GUI.Button(_r, GUIContent.none, GUIStyle.none)) GetMenu(_t, _e).DropDown(_r);

			}catch{

				_current.drawElementCallback -= DrawElement;

			}

		}

		/// <summary>
		/// Get the original menu with items renamed to identify components.
		/// </summary>
		/// <returns>The menu.</returns>
		/// <param name="target">Target.</param>
		/// <param name="listener">Listener.</param>
		private GenericMenu GetMenu(Object target, SerializedProperty listener){
			
			var _b = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			var _m = typeof(UnityEventDrawer).GetMethod("BuildPopupList", _b);

			var _e = typeof(UnityEventDrawer).GetField("m_DummyEvent", _b).GetValue(this);

			var _result = _m.Invoke(null, new object[]{target, _e, listener}) as GenericMenu;

			RenameMenuItems(_result);

			return _result;

		}

		/// <summary>
		/// Rename the items of the specified menu with target component index.
		/// </summary>
		/// <param name="menu">Menu.</param>
		private void RenameMenuItems(GenericMenu menu){

			var _b = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			var _a = (typeof(GenericMenu).GetField("menuItems", _b).GetValue(menu) as ArrayList).Cast<object>();

			var _c = _a.Skip(2).Select(_v => _v.GetType().GetField("content", _b).GetValue(_v) as GUIContent);

			var _l = _c.GroupBy(_v => _v.text.Remove(_v.text.IndexOf('/'))).Select(_v => _v.First().text).ToList();

			var _i = -1;

			foreach(var _v in _c) _v.text = string.Format("[{0}] {1}", _l.Contains(_v.text) ? ++_i : _i, _v.text);

		}

		/// <summary>
		/// Get the height of the property by Inspector window width.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label){

			if(!_inited.Contains(property.propertyPath)) Initialize(property);

			return base.GetPropertyHeight(property, label);

		}

		#endregion

	}

}
