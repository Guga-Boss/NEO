using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid: MonoBehaviour
{
    public enum Arrangement { Horizontal, Vertical, CellSnap }
    public enum ESorting { None, Alphabetic, Horizontal, Vertical, Custom }
    public enum EPivot { TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight }

    public Arrangement arrangement = Arrangement.Horizontal;
    public float CellWidth = 200f;
    public float CellHeight = 200f;
    public int ColumnLimit = 0;
    public ESorting Sorting = ESorting.None;
    public EPivot Pivot = EPivot.TopLeft;
    public bool HideInactive = false;
    public Action onReposition;
    public Comparison<Transform> CustomSort;

    // Cache list to avoid GC Alloc every reposition;
    private List<Transform> _cachedList = new List<Transform>(64);       // Pre-allocated capacity;

    [Button( "Reposition", ButtonSizes.Gigantic ), GUIColor( 1, 1f, 0 )]
    public void Reposition()
    {
        FillChildList();                                                 // Reuse cached list;

        int count = _cachedList.Count;                                   // Cache list count;
        if( count == 0 ) return;                                          // Early exit;

        int x = 0;
        int y = 0;
        int maxX = 0;
        int maxY = 0;

        for( int i = 0; i < count; i++ )
        {
            Transform t = _cachedList[i];                                // Local reference;
            Vector3 pos = t.localPosition;                               // Current pos;
            float depth = pos.z;                                         // Keep Z depth;

            if( arrangement == Arrangement.CellSnap )
            {
                if( CellWidth > 0 ) pos.x = Mathf.Round( pos.x / CellWidth ) * CellWidth;
                if( CellHeight > 0 ) pos.y = Mathf.Round( pos.y / CellHeight ) * CellHeight;
            }
            else
            {
                // Optimized ternary for position calculation;
                if( arrangement == Arrangement.Horizontal )
                {
                    pos.x = CellWidth * x;
                    pos.y = -CellHeight * y;
                }
                else
                {
                    pos.x = CellWidth * y;
                    pos.y = -CellHeight * x;
                }
            }

            t.localPosition = pos;                                       // Apply new pos;

            if( x > maxX ) maxX = x;                                      // Manual Max for speed;
            if( y > maxY ) maxY = y;

            if( ++x >= ColumnLimit && ColumnLimit > 0 )
            {
                x = 0;
                y++;
            }
        }

        ApplyPivotOffset( _cachedList, maxX, maxY );                       // Pass the cached list;
        onReposition?.Invoke();                                          // Trigger event;
    }

    void ApplyPivotOffset( List<Transform> list, int maxX, int maxY )
    {
        if( Pivot == EPivot.TopLeft ) return;                             // Default pivot, no work;

        Vector2 pivotOffset = GetPivotOffset(Pivot);
        bool isHorizontal = (arrangement == Arrangement.Horizontal);

        float width = isHorizontal ? maxX * CellWidth : maxY * CellWidth;
        float height = isHorizontal ? maxY * CellHeight : maxX * CellHeight;

        float offsetX = Mathf.Lerp(0f, width, pivotOffset.x);
        float offsetY = Mathf.Lerp(-height, 0f, pivotOffset.y);

        int count = list.Count;                                          // Cache list count;
        for( int i = 0; i < count; i++ )
        {
            Transform t = list[i];
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
        default: return Vector2.zero;
        }
    }

    void FillChildList()
    {
        _cachedList.Clear();                                             // Reset without deallocating;
        var trans = transform;                                           // Cache transform reference;

        int childCount = trans.childCount;                               // Get total children;
        for( int i = 0; i < childCount; i++ )
        {
            Transform t = trans.GetChild(i);                             // Faster than foreach on transform;
            if( !HideInactive || t.gameObject.activeSelf )
                _cachedList.Add( t );
        }

        if( Sorting != ESorting.None && arrangement != Arrangement.CellSnap )
        {
            switch( Sorting )
            {
            case ESorting.Alphabetic:
            // Ordinal comparison is faster than culture-aware;
            _cachedList.Sort( ( a, b ) => string.Compare( a.name, b.name, StringComparison.Ordinal ) );
            break;

            case ESorting.Horizontal:
            _cachedList.Sort( ( a, b ) => a.localPosition.x.CompareTo( b.localPosition.x ) );
            break;

            case ESorting.Vertical:
            _cachedList.Sort( ( a, b ) => b.localPosition.y.CompareTo( a.localPosition.y ) );
            break;

            case ESorting.Custom:
            if( CustomSort != null ) _cachedList.Sort( CustomSort );
            break;
            }
        }
    }
}