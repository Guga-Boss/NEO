using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MyGrid: MonoBehaviour
{
    public enum Arrangement
    {
        Horizontal,
        Vertical,
        CellSnap
    }

    public enum ESorting
    {
        None,
        Alphabetic,
        Horizontal,
        Vertical,
        Custom
    }

    public enum EPivot
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }

    public Arrangement arrangement = Arrangement.Horizontal;
    public float CellWidth = 200f;
    public float CellHeight = 200f;
    public int ColumnLimit = 0;
    public ESorting Sorting = ESorting.None;
    public EPivot Pivot = EPivot.TopLeft;
    public bool HideInactive = false;
    public Action onReposition;
    public Comparison<Transform> CustomSort;

    [Button( "Reposition", ButtonSizes.Gigantic ), GUIColor( 1, 1f, 0 )]
    public void Reposition()
    {
        List<Transform> list = GetChildList();

        if( list.Count == 0 )
            return;

        int x = 0;
        int y = 0;

        int maxX = 0;
        int maxY = 0;

        for( int i = 0; i < list.Count; i++ )
        {
            Transform t = list[i];
            float depth = t.localPosition.z;

            Vector3 pos;

            if( arrangement == Arrangement.CellSnap )
            {
                pos = t.localPosition;
                if( CellWidth > 0 )
                    pos.x = Mathf.Round( pos.x / CellWidth ) * CellWidth;
                if( CellHeight > 0 )
                    pos.y = Mathf.Round( pos.y / CellHeight ) * CellHeight;
            }
            else
            {
                pos = ( arrangement == Arrangement.Horizontal )
                    ? new Vector3( CellWidth * x, -CellHeight * y, depth )
                    : new Vector3( CellWidth * y, -CellHeight * x, depth );
            }

            t.localPosition = pos;

            maxX = Mathf.Max( maxX, x );
            maxY = Mathf.Max( maxY, y );

            if( ++x >= ColumnLimit && ColumnLimit > 0 )
            {
                x = 0;
                y++;
            }
        }

        ApplyPivotOffset( list, maxX, maxY );

        onReposition?.Invoke();
    }

    void ApplyPivotOffset( List<Transform> list, int maxX, int maxY )
    {
        if( Pivot == EPivot.TopLeft )
            return;

        Vector2 pivotOffset = GetPivotOffset(Pivot);

        float width = (arrangement == Arrangement.Horizontal)
            ? maxX * CellWidth
            : maxY * CellWidth;

        float height = (arrangement == Arrangement.Horizontal)
            ? maxY * CellHeight
            : maxX * CellHeight;

        float offsetX = Mathf.Lerp(0f, width, pivotOffset.x);
        float offsetY = Mathf.Lerp(-height, 0f, pivotOffset.y);

        foreach( var t in list )
        {
            Vector3 pos = t.localPosition;
            pos.x -= offsetX;
            pos.y -= offsetY;
            t.localPosition = pos;
        }
    }

    Vector2 GetPivotOffset( EPivot p )
    {
        switch( p )
        {
        case EPivot.TopLeft: return new Vector2( 0f, 1f );
        case EPivot.Top: return new Vector2( 0.5f, 1f );
        case EPivot.TopRight: return new Vector2( 1f, 1f );
        case EPivot.Left: return new Vector2( 0f, 0.5f );
        case EPivot.Center: return new Vector2( 0.5f, 0.5f );
        case EPivot.Right: return new Vector2( 1f, 0.5f );
        case EPivot.BottomLeft: return new Vector2( 0f, 0f );
        case EPivot.Bottom: return new Vector2( 0.5f, 0f );
        case EPivot.BottomRight: return new Vector2( 1f, 0f );
        }
        return Vector2.zero;
    }

    List<Transform> GetChildList()
    {
        List<Transform> list = new List<Transform>();

        foreach( Transform t in transform )
        {
            if( !HideInactive || t.gameObject.activeSelf )
                list.Add( t );
        }

        if( Sorting != ESorting.None && arrangement != Arrangement.CellSnap )
        {
            switch( Sorting )
            {
            case ESorting.Alphabetic:
            list.Sort( ( a, b ) => string.Compare( a.name, b.name, StringComparison.Ordinal ) );
            break;

            case ESorting.Horizontal:
            list.Sort( ( a, b ) => a.localPosition.x.CompareTo( b.localPosition.x ) );
            break;

            case ESorting.Vertical:
            list.Sort( ( a, b ) => b.localPosition.y.CompareTo( a.localPosition.y ) );
            break;

            case ESorting.Custom:
            if( CustomSort != null )
                list.Sort( CustomSort );
            break;
            }
        }

        return list;
    }


    [Button( "Migrate ALL", ButtonSizes.Gigantic ), GUIColor( 1, 1f, 0 )]
    static void MigrateAll()
    {
        UIGrid[] grids = UnityEngine.Object.FindObjectsOfType<UIGrid>(true);

        foreach( var old in grids )
        {
            GameObject go = old.gameObject;

            MyGrid neo = go.AddComponent<MyGrid>();

            neo.arrangement = (MyGrid.Arrangement) old.arrangement;
            neo.Sorting = (MyGrid.ESorting) old.sorting;
            neo.ColumnLimit = old.maxPerLine;
            neo.CellWidth = old.cellWidth;
            neo.CellHeight = old.cellHeight;
            neo.HideInactive = old.hideInactive;
            neo.Pivot = (MyGrid.EPivot) old.pivot;
            DestroyImmediate( old, true );

            EditorUtility.SetDirty( go );
        }

        Debug.Log( "Migration complete." );
    }

}