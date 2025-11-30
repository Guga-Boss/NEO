using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;

namespace Swing.Editor
{
	public abstract partial class BaseEditorWindow : EditorWindow, IHasCustomMenu
	{
		// Implement your own editor GUI here.
		protected virtual void OnGUI()
		{
			if (Event.current.type == EventType.ContextClick)
			{
				GenericMenu menu = new GenericMenu();
				fillContextMenu(menu);
				menu.ShowAsContext();

				Event.current.Use();
			}
		}

		// Called 100 times per second on all visible windows.
		protected virtual void Update() { }

		// Called at 10 frames per second to give the inspector a chance to update
		protected virtual void OnInspectorUpdate() { }


		// UNDOCUMENTED but seems to be called
		protected virtual void Awake() { }

		// Called when the object is loaded
		protected virtual void OnEnable()
		{
			Repaint();	// for recompilation
		}

		// Called when the scriptable object goes out of scope
		protected virtual void OnDisable() { }

		// Called when the EditorWindow is closed.
		protected virtual void OnDestroy() { }

		// Called when the window gets keyboard focus.
		protected virtual void OnFocus() { }

		// Called when the window loses keyboard focus.
		protected virtual void OnLostFocus() { }


		// Called whenever the selection has changed.
		protected virtual void OnSelectionChange() { }

		// Called whenever the scene hierarchy has changed.
		protected virtual void OnHierarchyChange() { }

		// Called whenever the project has changed.
		protected virtual void OnProjectChange() { }


		#region context menu

		protected virtual void fillContextMenu( GenericMenu _menu )
		{
			_menu.AddItem(new GUIContent("Edit EditorWindow Script"),
						  false,
						  delegate
						  {
							  MonoScript script = MonoScript.FromScriptableObject(this);
							  if (script)
							  {
								  AssetDatabase.OpenAsset(script);
							  }
						  });

			_menu.AddItem(new GUIContent("Select EditorWindow Script"),
						  false,
						  delegate
						  {
							  MonoScript script = MonoScript.FromScriptableObject(this);
							  if (script)
							  {
								  Selection.activeObject = script;
							  }
						  });
		}

		void IHasCustomMenu.AddItemsToMenu( GenericMenu _menu )
		{
			fillContextMenu(_menu);
		}

		#endregion
	}
}