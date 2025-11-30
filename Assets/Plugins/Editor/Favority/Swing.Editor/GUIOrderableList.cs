using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;

namespace Swing.Editor
{
	public abstract class GUIOrderableList<T>
	{
		public IList<T> items;

		protected virtual void onStartDrag( int _itemIndex )
		{
		}
		protected virtual void onEndDrag( bool _hasDragChange )
		{
		}
		protected virtual void onSwapItem( int _itemIndexA, int _itemIndexB )
		{
			var itemA = items[_itemIndexA];
			items[_itemIndexA] = items[_itemIndexB];
			items[_itemIndexB] = itemA;
		}

		protected abstract Rect drawItem( int _itemIndex, bool _isDragged, Rect _dragRect );

		public bool isDragging
		{
			get { return dragIndex != -1; }
		}
		int clickIndex = -1;
		int dragIndex = -1;
		Rect dragRect;
		bool hasDragChange;

		void clearDrag()
		{
			clickIndex = -1;
			dragIndex = -1;
			hasDragChange = false;
		}

		public void draw( EditorWindow _window = null )
		{
			Vector2 mousePos = Event.current.mousePosition;
			EventType eventType = Event.current.type;

			switch (eventType)
			{
				case EventType.MouseDown:
					clearDrag();
					break;

				case EventType.MouseUp:
					onEndDrag(hasDragChange);
					clearDrag();
					break;

				case EventType.MouseDrag:
					var delta = Event.current.delta;
					dragRect.x += delta.x;
					dragRect.y += delta.y;
					break;
			}

			for (int i = 0; i < items.Count; i++)
			{
				var rect = drawItem(i, i == dragIndex, dragRect);

				switch (eventType)
				{
					case EventType.MouseDown:
						if (rect.Contains(mousePos))
						{
							// mark draggable but wait for MouseDrag to start dragging,
							// to avoid one-click-drags
							clickIndex = i;
						}
						break;

					case EventType.MouseDrag:
						if (rect.Contains(mousePos) &&
							clickIndex != -1) // discard if the drag started from outside
						{
							if (!isDragging)
							{
								// start dragging
								dragIndex = i;
								dragRect = rect;
								onStartDrag(i);
							}
							else if (i != dragIndex)
							{
								// drag over another item: swap them
								hasDragChange = true;
								onSwapItem(i, dragIndex);
								dragIndex = i;
							}
						}
						break;
				}
			}

			if (_window)
			{
				repaintOnDrag(_window);
			}
		}

		/// <summary>
		/// The containing window MUST set wantsMouseMove to true,
		/// and repaint on drag.
		/// </summary>
		public static void repaintOnDrag( EditorWindow _window )
		{
			switch (Event.current.type)
			{
				case EventType.MouseUp:
				case EventType.MouseDrag:
					_window.Repaint();
					break;
			}
		}
	}
}
