using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;
using System.IO;

namespace Swing.Editor
{
	public class FavorityWindow : BaseEditorWindow
	{
		const string k_title = "Favority";
		[MenuItem("Window/" + k_title)]
		static void createWindow()
		{
			var window = GetWindow<FavorityWindow>();
			window.title = "Favority";
		}

		string currentScene;

		protected override void OnEnable()
		{
			base.OnEnable();

			currentScene = EditorApplication.currentScene;

			// OnEnable happens after recompilation / window layout switching
			load();

			Repaint();
		}

		protected override void OnProjectChange()
		{
			base.OnProjectChange();

			//Debug.Log("OnProjectChange");
			Repaint();
		}
		protected override void OnHierarchyChange()
		{
			base.OnHierarchyChange();

			//Debug.Log("OnHierarchyChange");

			if (currentScene != EditorApplication.currentScene)
			{
				currentScene = EditorApplication.currentScene;
				onSceneLoaded();
			}

			Repaint();
		}

		void onSceneLoaded()
		{
			//Debug.Log("onSceneLoaded " + currentScene);
			if (tab == Tab.Scene)
			{
				load();
			}
		}

		#region save/load

		const string settingsPath = "UserSettings/Favority.asset";

		void save()
		{
			data.onPreSave();

			Utility.save(data, settingsPath);
		}
		void load()
		{
			var dataRead = Utility.load<FavorityData>(settingsPath);
			data = dataRead ? dataRead : CreateInstance<FavorityData>();
			data.hideFlags = HideFlags.HideAndDontSave;

			switch (tab)
			{
				case Tab.Project:
					items = data.projectItems;
					break;

				case Tab.Scene:
					var sceneData = data.getSceneData();
					if (sceneData != null)
					{
						items = sceneData.items;
						sceneData.onPostLoad();
					}
					else
					{
						items = null;
					}

					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		static class Utility
		{
			static readonly Object[] saveArray = new Object[1];
			public static void save( Object _object, string _path, bool _allowTextSerialization = true )
			{
				saveArray[0] = _object;
				ensureDirectory(_path);
				UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(saveArray, _path, _allowTextSerialization);
			}

			public static T load<T>( string _path )
				where T : Object
			{
				var objects = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(_path);
				return (objects.Length == 0) ? null : objects[0] as T;
			}

			static void ensureDirectory( string _path )
			{
				string dir = Path.GetDirectoryName(_path);
				if (dir != null)
				{
					Directory.CreateDirectory(dir);
				}
			}
		}

		#endregion


		FavorityData data;
		List<FavorityItem> items;

		void addItem( Object[] _unityObjects )
		{
			if (tab == Tab.Scene &&
				items == null)
			{
				items = data.createSceneData().items;
			}

			items.AddRange(
				_unityObjects.convertAll(o => new FavorityItem { unityObject = o }));

			save();
		}
		void removeItem( FavorityItem item )
		{
			items.Remove(item);
			highlightedItem = null;

			save();
		}
		void clearCurrentItems()
		{
			switch (tab)
			{
				case Tab.Project:
					data.projectItems.Clear();
					break;

				case Tab.Scene:
					var sceneData = data.getSceneData();
					if (sceneData != null)
					{
						sceneData.items.Clear();
					}
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			save();
			load();
		}


		static class Styles
		{
			static Styles()
			{
				dragAndDropHint = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleCenter
				};

				itemNormal = EditorStyles.label;
				itemHighlight = EditorStyles.whiteLabel;
				itemMissing = new GUIStyle(EditorStyles.whiteLabel)
				{
					normal = { textColor = Color.red },
				};

				itemDragged = new GUIStyle(GUI.skin.box)
				{
					alignment = TextAnchor.MiddleLeft,
					normal = {textColor = Color.white},
					wordWrap = false,
					padding = new RectOffset(6, 6, 4, 4),
				};
			}
			public static readonly GUIStyle dragAndDropHint;
			public static readonly GUIStyle itemNormal;
			public static readonly GUIStyle itemHighlight;
			public static readonly GUIStyle itemMissing;
			public static readonly GUIStyle itemDragged;
		}

		enum Tab
		{
			Project,
			Scene,
		}
		static readonly Tab[] tabValues = (Tab[])Enum.GetValues(typeof(Tab));
		static readonly string[] tabNames = Enum.GetNames(typeof(Tab));

		Event @event;
		protected override void OnGUI()
		{
			@event = Event.current;

			drawHeader();

			drawCenter();

			drawFooter();

			base.OnGUI();
		}

		[SerializeField]
		Tab tab;
		void drawHeader()
		{
			GUILayout.BeginHorizontal();
			{
				var newTab = EditorGUILayoutExt.miniRadioButtons(tab, tabValues, tabNames);
				if (newTab != tab)
				{
					tab = newTab;
					load();
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				{
					// open a new window
					var newWindow = CreateInstance<FavorityWindow>();
					newWindow.title = k_title;
					newWindow.Show();
				}
			}
			GUILayout.EndHorizontal();
		}

		void drawFooter()
		{
			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Save", EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(false)))
				{
					save();
				}

				if (GUILayout.Button("Load", EditorStyles.miniButtonMid, GUILayout.ExpandWidth(false)))
				{
					load();
				}

				if (GUILayout.Button("Clear", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
				{
					clearCurrentItems();
				}

				GUILayout.FlexibleSpace();

				iconSize = GUILayout.HorizontalSlider(iconSize, 16, 64, GUILayout.Width(50));

				GUILayout.Space(10);
			}
			GUILayout.EndHorizontal();
		}

		Rect centerRect;
		void drawCenter()
		{
			#region get a rect for "everything but header & footer"

			//Rect centerRect = GUILayoutUtility.GetRect(0, Screen.width, 0, Screen.height);

			Rect tmpCenterRect = GUILayoutUtility.GetRect(0, Screen.width, 0, Screen.height);
			switch (@event.type)
			{
				case EventType.Repaint:
					centerRect = tmpCenterRect;
					break;
				case EventType.Layout:
					break;
			}

			#endregion

			GUILayout.BeginArea(centerRect);
			{
				handleDragAndDrop(centerRect);

				drawItems();
			}
			GUILayout.EndArea();
		}

		Vector2 scroll;
		[SerializeField]
		float iconSize = 16;
		readonly GUIContent itemContent = new GUIContent();
		FavorityItem highlightedItem;
		FavorityItem editedItem;

		class ItemList : GUIOrderableList<FavorityItem>
		{
			public FavorityWindow owner;

			protected override Rect drawItem( int _itemIndex, bool _isDragged, Rect _dragRect )
			{
				return owner.drawItem(_itemIndex, _isDragged, _dragRect);
			}

			protected override void onEndDrag( bool _hasDragChange )
			{
				base.onEndDrag(_hasDragChange);

				if (_hasDragChange)
				{
					owner.save();
				}
			}
		}
		readonly ItemList guiList = new ItemList();

		void drawItems()
		{
			if (items.isNullOrEmpty())
			{
				return;
			}

			EditorGUIUtility.SetIconSize(new Vector2(iconSize, iconSize));

			scroll = EditorGUILayout.BeginScrollView(scroll);
			{
				guiList.owner = this;
				guiList.items = items;
				guiList.draw(this);
			}
			EditorGUILayout.EndScrollView();

			#region remove item on delete

			if (highlightedItem != null)
			{
				switch (@event.type)
				{
					case EventType.ValidateCommand:
						if (@event.commandName == "SoftDelete")
						{
							@event.Use();
						}
						break;

					case EventType.ExecuteCommand:
						if (@event.commandName == "SoftDelete")
						{
							removeItem(highlightedItem);
							@event.Use();
						}
						break;
				}
			}

			#endregion
		}
		Rect drawItem( int _itemIndex, bool _isDragged, Rect _dragRect )
		{
			var item = items[_itemIndex];

			GUIStyle style;
			if (item.unityObject)
			{
				itemContent.text = item.name;
				itemContent.image = item.icon;

				style = (item == highlightedItem) ? Styles.itemHighlight : Styles.itemNormal;
			}
			else
			{
				itemContent.text = "'{0}' (missing)".format(item.unityObjectName);
				itemContent.image = null;

				style = Styles.itemMissing;
			}

			// reserve the rect even if dragged
			var rect = GUILayoutUtility.GetRect(itemContent, style); // lazy but this gets the whole width for some reason, yay!

			if (_isDragged)
			{
				var size = Styles.itemDragged.CalcSize(itemContent);
				_dragRect = new Rect(_dragRect.x, _dragRect.y, size.x, size.y);
				GUI.Box(_dragRect, itemContent, Styles.itemDragged);

				return rect;
			}


			GUI.Label(rect, itemContent, style);

			switch (@event.type)
			{
				// highlight & select on click / open on double-click
				case EventType.MouseDown:
					{
						Rect labelRect = GUILayoutUtility.GetLastRect();
						if (labelRect.Contains(Event.current.mousePosition))
						{
							highlightedItem = item;

							switch (Event.current.clickCount)
							{
								case 1:
									select(item);
									Event.current.Use();
									break;

								case 2:
									open(item);
									Event.current.Use();
									break;
							}
						}
					}
					break;

				// context menu
				case EventType.ContextClick:
					{
						Rect labelRect = GUILayoutUtility.GetLastRect();
						if (labelRect.Contains(Event.current.mousePosition))
						{
							showContextMenu(item);
							Event.current.Use();
						}
					}
					break;
			}

			return rect;
		}

		void handleDragAndDrop( Rect _centerRect )
		{
			Rect dropArea = new Rect(0, 0, _centerRect.width, _centerRect.height);

			if (items.isNullOrEmpty())
			{
				GUI.Label(dropArea,
				          "Drag'n'drop assets{0:' or scene objects';''} here".format(tab == Tab.Scene ? 1 : -1),
				          Styles.dragAndDropHint);
			}

			switch (@event.type)
			{
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if (!dropArea.Contains(@event.mousePosition))
					{
						break;
					}

					// make sure we don't drag scene objects into the project tab
					if (tab == Tab.Project &&
					    DragAndDrop.objectReferences.exists(o => !AssetDatabase.Contains(o)))
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
						break;
					}

					DragAndDrop.visualMode = DragAndDropVisualMode.Link;

					if (@event.type == EventType.DragPerform)
					{
						DragAndDrop.AcceptDrag();
						addItem(DragAndDrop.objectReferences);
						@event.Use();
					}
					break;
			}
		}


		void select( FavorityItem item )
		{
			Selection.activeObject = item.unityObject;
			EditorGUIUtility.PingObject(item.unityObject);

			//Object[] windows = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
			//foreach (EditorWindow window in windows)
			//{
			//	Debug.Log(window.title + " - " + window.GetType().Name);
			//}

			/*if (AssetDatabaseExt.IsDirectory(item.unityObject))
			{
				Debug.Log("dir");
				AssetDatabase.OpenAsset(item.unityObject);
			}
			else
			{
				Selection.activeObject = item.unityObject;
			}*/
		}
		void open( FavorityItem item )
		{
			if (item.isAsset)
			{
				//AssetDatabase.OpenAsset(item.unityObject);

				// maybe less direct, but will also open folders in explorer... nice!
				string path = AssetDatabase.GetAssetPath(item.unityObject);
				EditorUtility.OpenWithDefaultApp(path);
			}
			else
			{
				Selection.activeObject = item.unityObject;
				SceneView.FrameLastActiveSceneView();
			}
		}

		void showContextMenu( FavorityItem item )
		{
			GenericMenu menu = new GenericMenu();

			menu.AddItem(new GUIContent("Rename shortcut"), false, onContextMenu_Rename, item);
			menu.AddItem(new GUIContent("Delete shortcut"), false, onContextMenu_Delete, item);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Move up"), false, onContextMenu_MoveUp, item);
			menu.AddItem(new GUIContent("Move down"), false, onContextMenu_MoveDown, item);
			menu.AddItem(new GUIContent("Move to top"), false, onContextMenu_MoveToTop, item);
			menu.AddItem(new GUIContent("Move to bottom"), false, onContextMenu_MoveToBottom, item);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Select"), false, onContextMenu_Select, item);
			menu.AddItem(new GUIContent("Open"), false, onContextMenu_Open, item);

			menu.ShowAsContext();
		}
		void onContextMenu_Select( object userData )
		{
			FavorityItem item = (FavorityItem)userData;
			select(item);
		}
		void onContextMenu_Open( object userData )
		{
			FavorityItem item = (FavorityItem)userData;
			open(item);
		}
		void onContextMenu_Rename( object userData )
		{
			FavorityItem item = (FavorityItem)userData;
			startRename(item);
		}
		void onContextMenu_Delete( object userData )
		{
			FavorityItem item = (FavorityItem)userData;
			removeItem(item);
		}
		void onContextMenu_MoveUp( object userData )
		{
			FavorityItem item = (FavorityItem)userData;

			int index = items.IndexOf(item);
			if (index > 0)
			{
				items.RemoveAt(index);
				items.Insert(index - 1, item);
			}
		}
		void onContextMenu_MoveDown( object userData )
		{
			FavorityItem item = (FavorityItem)userData;

			int index = items.IndexOf(item);
			if (index < items.Count - 1)
			{
				items.RemoveAt(index);
				items.Insert(index + 1, item);
			}
		}
		void onContextMenu_MoveToTop( object userData )
		{
			FavorityItem item = (FavorityItem)userData;

			items.Remove(item);
			items.Insert(0, item);
		}
		void onContextMenu_MoveToBottom( object userData )
		{
			FavorityItem item = (FavorityItem)userData;

			items.Remove(item);
			items.Add(item);
		}

		void startRename( FavorityItem item )
		{
			editedItem = item;
			TextInputDialog.show(item.name, onNameChanged);
		}
		void onNameChanged( string newName )
		{
			//Debug.Log("onNameChanged");

			if (editedItem != null)
			{
				editedItem.customName = newName;
				editedItem = null;

				save();
				Repaint();
			}
		}
	}

	public static class EditorGUILayoutExt
	{
		public static T miniRadioButtons<T>( T _selected, T[] _availableValues,
											 string[] _availableLabels, params GUILayoutOption[] _options )
		{
			int max = _availableValues.Length - 1;
			for (int i = 0; i <= max; i++)
			{
				var value = _availableValues[i];
				var label = _availableLabels[i];
				var style = (i < max) ? (i == 0) ? EditorStyles.miniButtonLeft : EditorStyles.miniButtonMid : EditorStyles.miniButtonRight;

				EditorGUI.BeginChangeCheck();
				GUILayout.Toggle(_selected.Equals(value), label, style, _options);
				if (EditorGUI.EndChangeCheck())
				{
					_selected = value;
				}
			}

			return _selected;
		}
	}
}
