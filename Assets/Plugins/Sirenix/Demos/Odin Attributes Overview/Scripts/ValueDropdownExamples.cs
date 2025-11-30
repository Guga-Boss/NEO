using System;
using UnityEngine;
using System.Collections.Generic;
[Serializable]
public struct VI
{
    public int x;
    public int y;

    public VI( int x, int y ) { this.x = x; this.y = y; }

    // Operators
    public static VI operator +( VI a, VI b ) { return new VI( a.x + b.x, a.y + b.y ); }
    public static VI operator -( VI a, VI b ) { return new VI( a.x - b.x, a.y - b.y ); }
    public static VI operator *( VI a, int b ) { return new VI( a.x * b, a.y * b ); }
    public static VI operator /( VI a, int b ) { return new VI( a.x / b, a.y / b ); }
    public static bool operator ==( VI a, VI b ) { return a.x == b.x && a.y == b.y; }
    public static bool operator !=( VI a, VI b ) { return !( a == b ); }

    // Common functions
    public float Magnitude() { return Mathf.Sqrt( x * x + y * y ); }          // Euclidean length
    public int SqrMagnitude() { return x * x + y * y; }                     // squared length (faster)
    public float DistanceTo( VI other ) { return ( other - this ).Magnitude(); } // distance
    public int SqrDistanceTo( VI other ) { return ( other - this ).SqrMagnitude(); } // squared distance
    public VI Clamp( VI min, VI max ) { return new VI( Mathf.Clamp( x, min.x, max.x ), Mathf.Clamp( y, min.y, max.y ) ); }
    public VI Abs() { return new VI( Mathf.Abs( x ), Mathf.Abs( y ) ); }

    public VI Min( VI other ) { return new VI( Mathf.Min( x, other.x ), Mathf.Min( y, other.y ) ); }
    public VI Max( VI other ) { return new VI( Mathf.Max( x, other.x ), Mathf.Max( y, other.y ) ); }

    // Conversion to Vector2
    public Vector2 ToVector2() { return new Vector2( x, y ); }

    // Overrides
    public override bool Equals( object obj )
    {
        if( !( obj is VI ) ) return false;
        VI v = ( VI ) obj;
        return x == v.x && y == v.y;
    }

    public override int GetHashCode() { return x.GetHashCode() ^ y.GetHashCode(); }

    public override string ToString() { return "(" + x + "," + y + ")"; }
    public static implicit operator Vector2( VI v )
    {
        return new Vector2( v.x, v.y );
    }

    public static VI VTOVI( Vector2 Pos )
    {
        return new VI( ( int ) Pos.x, ( int ) Pos.y );
    }
}
