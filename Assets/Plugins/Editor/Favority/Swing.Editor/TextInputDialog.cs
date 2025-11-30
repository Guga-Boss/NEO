using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;

namespace Swing.Editor
{
	public class TextInputDialog : BaseEditorWindow
	{
		public static void show( string text, Action<string> _onValidated )
		{
			if (_onValidated == null)
			{
				return;
			}

			TextInputDialog window = CreateInstance<TextInputDialog>();
			window.text = text;
			window.onValidated = _onValidated;

			float width = 200;
			float height = 60;
			window.position = new Rect(
				(Screen.currentResolution.width - width) * 0.5f,
				(Screen.currentResolution.height - height) * 0.5f,
				width, height);

			window.ShowPopup();
			window.Focus();

			//Rect rect = GUILayoutUtility.GetLastRect();
			//window.ShowAsDropDown(rect, new Vector2(200, 60));
			//window.Focus();
		}

		Action<string> onValidated;
		string text = "";
		bool focused;

		protected override void OnGUI()
		{
			base.OnGUI();

			GUI.SetNextControlName("text");
			text = GUILayout.TextField(text);

			if (Event.current.type == EventType.KeyUp &&
			    Event.current.keyCode == KeyCode.Return)
			{
				//Debug.Log("kb validate");
				validate();
			}

			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("OK"))
				{
					validate();
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Cancel"))
				{
					Close();
				}

				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			if (!focused)
			{
				focused = true;
				GUI.FocusControl("text");
			}
		}

		bool validated;
		void validate()
		{
			// make sure we don't do it twice what with GUI events shit...
			if (validated)
			{
				return;
			}
			validated = true;


			if (onValidated != null)
			{
				onValidated(text);
			}

			Close();
		}
	}
}
