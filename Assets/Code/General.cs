using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PathologicalGames;

public class Util
{
    public Vector2[] DirCord;
    public VI[] DirCordI;
    public int[] InvDir;
    public static bool FloatSortOk;

    public Util()
    {
        DirCord = new Vector2[] { new Vector2(0, 1),  new Vector2( 1, 1),   new Vector2( 1, 0),   new Vector2( 1,-1),  
		  	                      new Vector2(0,-1),  new Vector2(-1,-1),   new Vector2(-1, 0),   new Vector2(-1, 1)};
        InvDir = new int[] { 4, 5, 6, 7, 0, 1, 2, 3 };

        DirCordI = new VI[] { new VI(0, 1),  new VI( 1, 1),   new VI( 1, 0),   new VI( 1,-1),  
		  	                  new VI(0,-1),  new VI(-1,-1),   new VI(-1, 0),   new VI(-1, 1)};
    }

    public static GameObject FObj( string name )
    {
        GameObject obj = GameObject.Find( name );
        if( obj == null ) Debug.LogError( "Object Not Found: " + name );
        return obj;
    }

    /*public 	void Lim( out float num, float min, float max )
    {
        if( num < min ) num = min;
        if( num > max ) num = max;
    }
	
    public 	void Lim( out int num, int min, int max )
    {
        if( num < min ) num = min;
        if( num > max ) num = max;
    }*/


    public static int Sort( List<float> ph )
    {
        List<float> hmax = new List<float>();
        List<float> hmin = new List<float>();
        float totHeight = 0;
        for( int j = 0; j < ph.Count; j++ )
        {
            hmin.Add( totHeight );
            totHeight += ph[ j ];
            hmax.Add( totHeight );
        }

        float sort = Random.Range( 0.0f, totHeight );
        int id = -1;
        for( int j = 0; j < ph.Count; j++ )
        {
            if( sort >= hmin[ j ] && sort <= hmax[ j ] )
            {
                id = j; break;
            }
        }
        return id;
    }

    public static void SortUnitListByScore( ref List<float> score, ref List<Unit> unl )
    {
        if( score == null ) return;
        if( score.Count < 2 ) return;
        int length = score.Count;
        float temp = score[ 0 ];
        Unit tempun = unl[ 0 ];
        for( int i = 0; i < length; i++ )
        {
            for( int j = i + 1; j < length; j++ )
            {
                if( score[ i ] < score[ j ] )
                {
                    temp = score[ i ];
                    tempun = unl[ i ];
                    score[ i ] = score[ j ];
                    unl[ i ] = unl[ j ];
                    score[ j ] = temp;
                    unl[ j ] = tempun;
                }
            }
        }
    }

    public static float Factorial( float num, float times )
    {
        float res = 0;
        for( int i = 0; i < times; i++ )
        {
            res += num * ( i + 1 );
        }
        return res;
    }

    public static bool Neighbor( Vector2 pt1, Vector2 pt2 )
    {
        if( pt1.x >= pt2.x - 1 )
            if( pt1.x <= pt2.x + 1 )
                if( pt1.y >= pt2.y - 1 )
                    if( pt1.y <= pt2.y + 1 ) return true;
        return false;
    }

    //you can use as follows:
    //List<int> myValues = CreateList( 1, 2, 3 );
    List<T> CreateList<T>( params T[] values )
    {
        return new List<T>( values );
    }

    public static float CompoundInterest( float num, int times, float rate, bool cummulative = false )
    {
        if( times < 1 ) return 0;
        float res = num;
        for( int i = 0; i < times - 1; i++ )
        {
            if( cummulative )
                res += num + Percent( rate, res );
            else
                res += num + Percent( rate, num );
        }
        return res;
    }
    public static bool Chance( float num )
    {
        if( num <= 0 ) return false;
        if( num >= 100 ) return true;
        if( Random.Range( 0, 100 ) <= num ) return true;
        return false;
    }
    public static bool Chc()
    {
        return Chance( 50 );
    }
    public static float Mod( float val )
    {
        if( val < 0 ) return -val;
        return val;
    }

    public static bool IsEven( float num )
    {
        if( num % 2 == 0 ) return true;
        return false;
    }

    float AngleBetweenPoints( Vector2 a, Vector2 b )
    {
        return Mathf.Atan2( a.y - b.y, a.x - b.x ) * Mathf.Rad2Deg;
    }
    public static bool IsMultiple( int x, int n )
    {
        return ( x % n ) == 0;
    }
    public static float Percent( float percent, float num )
    {
        return percent * num / 100;
    }

    public static string ToTime( double sec )
    {
        string res = "";
        string signal = "";

        if( sec < 0 )
        {
            signal = "-";
            sec *= -1;
        }

        System.TimeSpan t = System.TimeSpan.FromSeconds( sec );
        string day = " Day ";
        if( t.Days > 1 ) day = " Days ";
        if( t.Days <= 0 ) day = "";

        string hour = " Hour ";
        if( t.Hours > 1 ) hour = " Hours ";
        if( t.Hours <= 0 ) hour = "";

        string min = " Minute ";
        if( t.Minutes > 1 ) min = " Minutes ";
        if( t.Minutes <= 0 ) min = "";

        string secs = " Second ";
        if( t.Seconds > 1 ) secs = " Seconds ";
        if( t.Seconds <= 0 ) secs = "";

        string xsec = "" + t.Seconds;
        if( t.Seconds < 1 ) xsec = "";

        string xmin = "" + t.Minutes;
        if( t.Minutes < 1 ) xmin = "";

        string xhour = "" + t.Hours;
        if( t.Hours < 1 ) xhour = "";

        string xday = "" + t.Days;
        if( t.Days < 1 ) xday = "";

        res = xday + day + xhour + hour + xmin + min + xsec + secs;
        if( sec < 10 ) res = sec.ToString( "0.0" ) + "s";

        return signal + res;
    }

    public static string ToSTime( double sec, int dec = -1 )
    {
        string res = "";
        string signal = "";

        if( sec < 0 )
        {
            signal = "-";
            sec *= -1;
        }

        System.TimeSpan t = System.TimeSpan.FromSeconds( sec );
        string day = "D ";
        if( t.Days >= 1 ) day = " Day ";
        if( t.Days < 1 ) day = "";

        string hour = "H ";
        if( t.Hours > 1 ) hour = " Hr ";
        if( t.Hours <= 0 ) hour = "";

        string min = "M ";
        if( t.Minutes > 1 ) min = " Min ";
        if( t.Minutes <= 0 ) min = "";

        string secs = " Sec";
        if( t.Seconds > 1 ) secs = " Sec";
        if( t.Seconds <= 0 ) secs = "";

        string xsec = "" + t.Seconds;
        if( t.Seconds < 1 ) xsec = "";
        else
            if( dec == 3 ) xsec = t.Seconds.ToString( "0." ) + "." + t.Milliseconds.ToString( "000." );

        string xmin = "" + t.Minutes;
        if( t.Minutes < 1 ) xmin = "";

        string xhour = "" + t.Hours;
        if( t.Hours < 1 ) xhour = "";

        string xday = "" + t.Days;
        if( t.Days < 1 ) xday = "";

        res = xday + day + xhour + hour + xmin + min + xsec + secs;
        if( dec != 3 )
            if( sec < 10 ) res = sec.ToString( "0.#" ) + " sec";
        return signal + res;
    }

    public static List<string> GetWords( string txt )
    {
        List<string> lst = new List<string>();
        int lastbegin = 0;
        for( int i = 0; i < txt.Length; i++ )
        {
            if( txt[ i ] == ' ' || i == txt.Length - 1 )
            {
                int x = 0;
                if( i == txt.Length - 1 ) x = 1;
                lst.Add( txt.Substring( lastbegin, i - lastbegin + x ) );
                lst[ lst.Count - 1 ] = lst[ lst.Count - 1 ].Replace( " ", string.Empty );
                lastbegin = i;
            }
        }
        return lst;
    }

    public static int RoundToNearestPowerOfTwo( int n )
    {
        if( n <= 0 )
        {
            Debug.LogError( "Calling this function with a non-positive n doesn't make sense (you sent " + n + ")" );
        }
        else if( n == 1 || n == 2 )
        {
            return n;
        }
        else
        {
            return ( int ) Mathf.Pow( 2, Mathf.Ceil( Mathf.Log( n ) / Mathf.Log( 2 ) ) );
        }
        return -1;
    }

    public static Vector3 RotatePointAroundPivot( Vector3 point, Vector3 pivot, Vector3 angles )
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler( angles ) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    public static void SetLayerRecursively( GameObject go, int layerNumber )
    {
        if( go == null ) return;
        foreach( Transform trans in go.GetComponentsInChildren<Transform>( true ) )
        {
            trans.gameObject.layer = layerNumber;
        }
    }

    public static int GetRandomDir( EDirection except = EDirection.NONE )
    {
        int dir = -1;
        for( ; ; ) { dir = Random.Range( 0, 8 ); if( dir != ( int ) except ) break; }
        return dir;
    }

    public static Vector2 GetTargetUnitVector( Vector2 pos, Vector2 tg )
    {
        float amt = 1;
        if( pos.x == tg.x && pos.y > tg.y ) return new Vector2( 0, -amt );
        if( pos.x == tg.x && pos.y < tg.y ) return new Vector2( 0, +amt );
        if( pos.x > tg.x && pos.y == tg.y ) return new Vector2( -amt, 0 );
        if( pos.x < tg.x && pos.y == tg.y ) return new Vector2( +amt, 0 );
        if( pos.x < tg.x && pos.y < tg.y ) return new Vector2( +amt, amt );
        if( pos.x > tg.x && pos.y < tg.y ) return new Vector2( -amt, amt );
        if( pos.x < tg.x && pos.y > tg.y ) return new Vector2( +amt, -amt );
        if( pos.x > tg.x && pos.y > tg.y ) return new Vector2( -amt, -amt );
        return new Vector2( -1, -1 );
    }

    public static EDirection GetTargetUnitDir( Vector2 pos, Vector2 tg )
    {
        if( tg.x == pos.x && tg.y > pos.y ) return EDirection.N;
        if( tg.x == pos.x && tg.y < pos.y ) return EDirection.S;
        if( tg.x > pos.x && tg.y == pos.y ) return EDirection.E;
        if( tg.x < pos.x && tg.y == pos.y ) return EDirection.W;
        if( tg.x < pos.x && tg.y < pos.y ) return EDirection.SW;
        if( tg.x > pos.x && tg.y < pos.y ) return EDirection.SE;
        if( tg.x < pos.x && tg.y > pos.y ) return EDirection.NW;
        if( tg.x > pos.x && tg.y > pos.y ) return EDirection.NE;
        return EDirection.NONE;
    }
    public static EDirection RotateDir( int dir, int val )
    {
        val *= -1;
        for( ; ; )
        {
            if( val > 0 ) dir--;
            if( val < 0 ) dir++;
            if( dir < 0 ) dir = 7;
            if( dir > 7 ) dir = 0;
            if( val > 0 ) val--;
            if( val < 0 ) val++;
            if( val == 0 ) return ( EDirection ) dir;
        }
    }

    public static void SetActiveRecursively( GameObject go, bool active )
    {
        go.SetActive( active );
        foreach( Transform t in go.transform )
        {
            SetActiveRecursively( t.gameObject, active );
        }
    }
    public static string GetText( string text, string sheet = "" )
    {
        string txt = "";
        if( sheet == "" ) txt = Language.Get( text );
        else txt = Language.Get( text, sheet );
        return txt.Replace( "\\n", "\n" );
    }
    public static string GetTextBetween( string text, string start, string end )
    {
        int startIndex = text.IndexOf( start );
        if( startIndex == -1 ) return "";
        startIndex += start.Length;
        int endIndex = text.IndexOf( end, startIndex );
        if( endIndex == -1 ) return "";
        return text.Substring( startIndex, endIndex - startIndex ).Trim();
    }

    public static string GetName( string p )
    {
        return p.Replace( '_', ' ' );
    }

    public static float GetFactorInfo( float p, ref float baseb, ref float perc, ref bool success )
    {
        p /= 100;
        baseb = Mathf.Floor( p ); ;
        float res = baseb;
        perc = p - res;
        perc *= 100;
        success = false;
        if( Chance( perc ) )
        {
            res += 1;
            success = true;
        }
        return res;
    }

    public static float GetFactor( float p )
    {
        float num = 0, perc = 0; bool success = false;
        return GetFactorInfo( p, ref num, ref perc, ref success );
    }

    public static List<Unit> GetUnitsInTheArea( Rect r, ETileType tile )
    {
        List<Unit> ul = new List<Unit>();
        ELayerType l = Map.GetTileLayer( tile );

        for( int y = ( int ) r.yMin; y < r.yMax; y++ )
            for( int x = ( int ) r.xMin; x < r.xMax; x++ )
            {
                Unit un = Map.I.Unit[ x, y ];
                if( l == ELayerType.GAIA ) un = Map.I.Gaia[ x, y ];
                if( l == ELayerType.GAIA2 ) un = Map.I.Gaia2[ x, y ];

                if( un && un.TileID == tile )
                    ul.Add( Map.I.Unit[ x, y ] );
            }

        return ul;
    }

    public static float SumList( List<float> list )
    {
        float sum = 0;
        for( int i = 0; i < list.Count; i++ )
            sum += list[ i ];
        return sum;
    }

    public static bool CheckRepeated( List<float> tgl )
    {
        for( int i = 0; i < tgl.Count; i++ )
            for( int j = 0; j < tgl.Count; j++ )
                if( i != j )
                {
                    if( tgl[ i ] == tgl[ j ] ) return true;
                }
        return false;
    }
    public static bool CheckRepeated( List<string> tgl, ref string repeated )
    {
        for( int i = 0; i < tgl.Count; i++ )
            for( int j = 0; j < tgl.Count; j++ )
                if( i != j )
                {
                    if( tgl[ i ] == tgl[ j ] )
                    {
                        repeated = " " + tgl[ i ] + " == " + tgl[ j ] + " IDS: " + i + " == " + j;
                        return true;
                    }
                }
        return false;
    }

    public static void RandomizeList( ref List<Vector2> alpha )
    {
        for( int i = 0; i < alpha.Count; i++ )
        {
            Vector2 temp = alpha[ i ];
            int randomIndex = Random.Range( i, alpha.Count );
            alpha[ i ] = alpha[ randomIndex ];
            alpha[ randomIndex ] = temp;
        }
    }

    public static bool IsNeighbor( Vector2 v1, Vector2 v2, bool sameIsTrue = true )
    {
        if( v1 == v2 )
        {
            if( sameIsTrue ) return true;
            return false;
        }
        Vector2 dif = v1 - v2;
        if( dif.x > 1 ) return false;
        if( dif.x < -1 ) return false;
        if( dif.y > 1 ) return false;
        if( dif.y < -1 ) return false;
        return true;
    }

    public static EDirection GetActionDirection( EActionType ac )
    {
        if( ac == EActionType.MOVE_N ) return EDirection.N;
        if( ac == EActionType.MOVE_NE ) return EDirection.NE;
        if( ac == EActionType.MOVE_E ) return EDirection.E;
        if( ac == EActionType.MOVE_SE ) return EDirection.SE;
        if( ac == EActionType.MOVE_S ) return EDirection.S;
        if( ac == EActionType.MOVE_SW ) return EDirection.SW;
        if( ac == EActionType.MOVE_W ) return EDirection.W;
        if( ac == EActionType.MOVE_NW ) return EDirection.NW;

        return EDirection.NONE;
    }

    public static float MakeSmallerThan( float num, float total, ref int times )
    {
        times = 0;
        for( ; ; )
        {
            if( num < total ) return num;
            times++;
            num -= total;
        }
    }
    public static string Plural( int num )
    {
        if( num >= 2 ) return "s";
        return "";
    }

    public static int AddNew( ref List<Vector2> dest, List<Vector2> src )
    {
        int cont = 0;
        for( int i = 0; i < src.Count; i++ )
            if( dest.Contains( src[ i ] ) == false )
            {
                dest.Add( src[ i ] );
                cont++;
            }
        return cont;
    }

    public static bool IsInBox( Vector2 tg, Vector2 boxPos, int size )
    {
        Rect r = new Rect( boxPos.x - size, boxPos.y - size, 1 + ( size * 2 ), 1 + ( size * 2 ) );
        if( r.Contains( tg ) ) return true;
        return false;
    }

    public static Quaternion GetRotationToPoint( Vector3 origin, Vector3 target )
    {
        Vector3 vectorToTarget = target - origin;
        float fov = ( Mathf.Atan2( vectorToTarget.y, vectorToTarget.x ) * Mathf.Rad2Deg ) - 90;
        Quaternion qt = Quaternion.AngleAxis( fov, Vector3.forward );
        return qt;
    }

    public static EDirection GetRotationToTarget( Vector2 or, Vector2 tg )
    {
        EDirection dr = EDirection.NONE;
        if( tg.x < or.x && tg.y == or.y ) dr = EDirection.W;
        else
            if( tg.x > or.x && tg.y == or.y ) dr = EDirection.E;
            else
                if( tg.x > or.x && tg.y < or.y ) dr = EDirection.SE;
                else
                    if( tg.x < or.x && tg.y > or.y ) dr = EDirection.NW;
                    else
                        if( tg.x == or.x && tg.y < or.y ) dr = EDirection.S;
                        else
                            if( tg.x == or.x && tg.y > or.y ) dr = EDirection.N;
                            else
                                if( tg.x > or.x && tg.y > or.y ) dr = EDirection.NE;
                                else
                                    if( tg.x < or.x && tg.y < or.y ) dr = EDirection.SW;
        return dr;
    }
    public static List<Unit> GetNeighbors( Vector2 tg, ETileType tile )
    {
        List<Unit> nl = new List<Unit>();
        for( int d = 0; d < 8; d++ )
        {
            Vector2 aux = tg + Manager.I.U.DirCord[ d ];
            if( Map.PtOnMap( Map.I.Tilemap, aux ) )
            {
                Unit un = Map.I.GetUnit( tile, aux );
                if( un != null ) nl.Add( un );
            }
        }
        return nl;
    }

    public static List<Unit> GetFlyingNeighbors( Vector2 tg, ETileType tile )
    {
        List<Unit> nl = new List<Unit>();
        for( int d = 0; d < 8; d++ )
        {
            Vector2 aux = tg + Manager.I.U.DirCord[ d ];
            if( Map.PtOnMap( Map.I.Tilemap, aux ) )
                if( Map.I.FUnit[ ( int ) aux.x, ( int ) aux.y ] != null )
                    for( int f = 0; f < Map.I.FUnit[ ( int ) aux.x, ( int ) aux.y ].Count; f++ )
                        if( Map.I.FUnit[ ( int ) aux.x, ( int ) aux.y ][ f ].TileID == tile )
                            nl.Add( Map.I.FUnit[ ( int ) aux.x, ( int ) aux.y ][ f ] );
        }
        return nl;
    }

    public static List<Vector2> GetTileLine( Vector2 tg, EDirection dir, ETileType tile, int max = -1 )
    {
        if( max == -1 ) max = Sector.TSX;
        List<Vector2> nl = new List<Vector2>();
        for( int d = 0; d < max; d++ )
        {
            Vector2 aux = tg + Manager.I.U.DirCord[ ( int ) dir ] * d;
            if( Map.PtOnMap( Map.I.Tilemap, aux ) )
            {
                Unit un = Map.I.GetUnit( tile, aux );
                if( un != null ) nl.Add( aux );
                else return nl;
            }
            else return nl;
        }
        return nl;
    }

    public static string GetListText( List<Vector2> list )
    {
        string txt = "";
        for( int i = 0; i < list.Count; i++ )
            txt += "  ID: " + i + " (" + list[ i ].x + "  " + list[ i ].y + ")";
        return txt;
    }

    public static int Manhattan( Vector2 p1, Vector2 p2 )
    {
        Vector2 res = p1 - p2;
        res = new Vector2( Mathf.Abs( res.x ), Mathf.Abs( res.y ) );
        if( res.x > res.y ) return ( int ) res.x;
        else return ( int ) res.y;
    }

    public static Vector2 GetRandVector2( float dist )
    {
        Vector2 or = Vector2.zero;
        return new Vector2( Random.Range( or.x - dist, or.x + dist ), Random.Range( or.y - dist, or.y + dist ) );
    }
    public static bool IsInTheSameDiagonal( Vector2 pos1, Vector2 pos2 )
    {
        float distX = Mathf.Abs( pos1.x - pos2.x );
        float distY = Mathf.Abs( pos1.y - pos2.y );
        if( distX == distY ) return true;
        return false;
    }

    public static bool IsDiagonal( EDirection dir )
    {
        if( dir == EDirection.NE ) return true;
        if( dir == EDirection.NW ) return true;
        if( dir == EDirection.SE ) return true;
        if( dir == EDirection.SW ) return true;
        return false;
    }

    public static EDirection GetInvDir( EDirection dr )
    {
        if( dr == EDirection.NONE ) Debug.LogError( "Bad Direction on InvDir" );
        return ( EDirection ) Manager.I.U.InvDir[ ( int ) dr ];
    }

    public static float GetDistanceToLine( Vector2 line1, Vector2 line2, Vector2 tg )
    {
        return DistancePointLine( tg, line1, line2 );
    }

    public static float DistancePointLine( Vector3 point, Vector3 lineStart, Vector3 lineEnd )
    {
        return Vector3.Magnitude( ProjectPointLine( point, lineStart, lineEnd ) - point );
    }
    public static Vector3 ProjectPointLine( Vector3 point, Vector3 lineStart, Vector3 lineEnd )
    {
        Vector3 rhs = point - lineStart;
        Vector3 vector2 = lineEnd - lineStart;
        float magnitude = vector2.magnitude;
        Vector3 lhs = vector2;
        if( magnitude > 1E-06f )
        {
            lhs = ( Vector3 ) ( lhs / magnitude );
        }
        float num2 = Mathf.Clamp( Vector3.Dot( lhs, rhs ), 0f, magnitude );
        return ( lineStart + ( ( Vector3 ) ( lhs * num2 ) ) );
    }
    public static Vector2 GetRelativePosition( EDirection srcdir, EDirection tgdir )
    {
        int shift = ( int ) ( ( ( int ) srcdir ) + tgdir );
        if( shift >= 8 ) shift -= 8;
        return Manager.I.U.DirCord[ shift ];
    }
    public static bool LOk<T>( List<T> lista, int index, bool showmsg = false )
    {
        bool ok = lista != null && index >= 0 && index < lista.Count;
        if( showmsg )
            Debug.LogError( "Out of bounds: " + lista.ToString() + " id: " + index + " Size: " + lista.Count );
        return ok;
    }
    public static int FloatSort( float p )
    {
        FloatSortOk = false;
        int intf = ( int ) p;
        float fact = p - intf;
        int add = 0;
        bool chance = Chance( fact * 100 );
        if( chance )
        { add = 1; FloatSortOk = true; }
        return intf + add;
    }

    public static EDirection FlipDir( EDirection dir, bool flipx, bool flipy )
    {
        return dir;
        if( dir == EDirection.NONE ) return EDirection.NONE;
        EDirection res = dir;
        if( flipx == false && flipy == false ) return res;

        if( flipx && flipy )
        {
            if( dir == EDirection.N ) return EDirection.S;
            if( dir == EDirection.S ) return EDirection.N;
            if( dir == EDirection.E ) return EDirection.W;
            if( dir == EDirection.W ) return EDirection.E;
            if( dir == EDirection.NW ) return EDirection.SE;
            if( dir == EDirection.SE ) return EDirection.NW;
            if( dir == EDirection.SW ) return EDirection.NE;
            if( dir == EDirection.NE ) return EDirection.SW;
        }
        else
            if( flipx )
            {
                if( dir == EDirection.E ) return EDirection.W;
                if( dir == EDirection.W ) return EDirection.E;
                if( dir == EDirection.NW ) return EDirection.NE;
                if( dir == EDirection.NE ) return EDirection.NW;
                if( dir == EDirection.SW ) return EDirection.SE;
                if( dir == EDirection.SE ) return EDirection.SW;
            }
            else
                if( flipy )
                {
                    if( dir == EDirection.N ) return EDirection.S;
                    if( dir == EDirection.S ) return EDirection.N;
                    if( dir == EDirection.NE ) return EDirection.SE;
                    if( dir == EDirection.SE ) return EDirection.NE;
                    if( dir == EDirection.NW ) return EDirection.SW;
                    if( dir == EDirection.SW ) return EDirection.NW;
                }
        return res;
    }

    public static Vector2 FlipVector( Vector2 vect, bool flipx, bool flipy )
    {
        Vector2 res = vect;
        if( flipx == false && flipy == false ) return res;
        if( flipx ) vect.x *= -1;
        if( flipy ) vect.y *= -1;
        return res;
    }

    public static void Punch( Vector3 from, Vector3 to, GameObject gameObject, float amt, float time )
    {
        Vector3 pun = Util.GetTargetUnitVector( from, to );
        iTween.PunchPosition( gameObject, amt * pun, time );
    }

    public static void ToBool( MyBool myBool, ref bool val )
    {
        if( myBool == MyBool.FALSE ) val = false;
        if( myBool == MyBool.TRUE ) val = true;
    }

    public static bool IsWithinAngleRange( Transform target, float centerAngle, float margin )
    {
        float currentAngle = Mathf.Repeat( target.eulerAngles.z, 360f );
        float min = Mathf.Repeat( centerAngle - margin + 360f, 360f );
        float max = Mathf.Repeat( centerAngle + margin + 360f, 360f );

        if( min < max )
            return currentAngle >= min && currentAngle <= max;
        else
            return currentAngle >= min || currentAngle <= max;
    }
    
    public static bool InsideAngleArc( EDirection angle, EDirection tgAng, int step )                         // essa funcao checa se um angulo esta dentro do leque: steps definem arco
    {
        int a = ( int ) angle;
        int b = ( int ) tgAng;
        int diff = Mathf.Abs( a - b );
        if( diff > 4 ) diff = 8 - diff; // diferença circular (ex: N e NW = 1)
        // passos 1 = só centro, passos 2 = ±1, passos 3 = ±2...
        int maxDiff = step - 1;
        return diff <= maxDiff;
    }


    public static EDirection GetAngleDirection( float angle )
    {
        EDirection dr = EDirection.NONE;
        if( angle > 360 ) angle -= 360;
        if( angle < -360 ) angle += 360;

        float div = 22.5f;

        //if( angle <= div * 1 && angle >= div * -1 ) return EDirection.N;
        //if( angle >= div * 1 && angle <= div * 3 ) return EDirection.NW;
        //if( angle >= div * 3 && angle <= div * 5 ) return EDirection.W;
        //if( angle >= div * 5 && angle <= div * 7 ) return EDirection.SW;
        //if( angle <= div * -1 && angle >= -div * 3 ) return EDirection.NE;
        //if( angle <= div * -3 && angle >= -div * 5 ) return EDirection.E;
        //if( angle <= div * -5 && angle >= -div * 7 ) return EDirection.SE;
        //return EDirection.S;


        if( angle >= 0 - div && angle <= 0 + div ) return EDirection.N;
        if( angle >= 45 - div && angle <= 45 + div ) return EDirection.NW;
        if( angle >= 90 - div && angle <= 90 + div ) return EDirection.W;
        if( angle >= 135 - div && angle <= 135 + div ) return EDirection.SW;
        if( angle >= 180 - div && angle <= 180 + div ) return EDirection.S;
        if( angle >= 225 - div && angle <= 225 + div ) return EDirection.SE;
        if( angle >= 270 - div && angle <= 270 + div ) return EDirection.E;
        if( angle >= 315 - div && angle <= 315 + div ) return EDirection.NE;
        if( angle <= 360 ) return EDirection.N;
        if( dr == EDirection.NONE )
            Debug.LogError( "dr == EDirection.NONE" );
        return dr;
    }
    public static EDirection GetCardinalDirection( Vector2 velocity )
    {
        if( velocity == Vector2.zero )
            return EDirection.NONE;
        float angle = Mathf.Atan2( velocity.y, velocity.x ) * Mathf.Rad2Deg;
        // Ajuste para ângulos positivos
        if( angle < 0 )
            angle += 360f;
        // Define os setores dos 8 pontos cardeais (dividido em 45° cada)
        if( angle >= 337.5f || angle < 22.5f ) return EDirection.E;
        if( angle >= 22.5f && angle < 67.5f ) return EDirection.NE;
        if( angle >= 67.5f && angle < 112.5f ) return EDirection.N;
        if( angle >= 112.5f && angle < 157.5f ) return EDirection.NW;
        if( angle >= 157.5f && angle < 202.5f ) return EDirection.W;
        if( angle >= 202.5f && angle < 247.5f ) return EDirection.SW;
        if( angle >= 247.5f && angle < 292.5f ) return EDirection.S;
        if( angle >= 292.5f && angle < 337.5f ) return EDirection.SE;
        return EDirection.E;
    }
    public static string Between( string Text, string FirstString, string LastString )
    {
        string STR = Text;
        string STRFirst = FirstString;
        string STRLast = LastString;
        string FinalString;
        int Pos1 = STR.IndexOf( FirstString ) + FirstString.Length;
        int Pos2 = STR.IndexOf( LastString );
        FinalString = STR.Substring( Pos1, Pos2 - Pos1 );
        return FinalString;
    }

    static public GameObject getChildGameObject( GameObject fromGameObject, string withName )
    {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>( true );
        foreach( Transform t in ts ) if( t.gameObject.name == withName ) return t.gameObject;
        return null;
    }

    public static float GetPercent( float num, float total )
    {
        return num * 100 / total;
    }
    public static float GetPercent( float percent, float min, float max )
    {
        return Mathf.Lerp( min, max, percent / 100f );
    }
    public static List<ItemType> CondenseItems( List<ItemType> items, List<float> inputCosts, ref List<float> condensedCosts )
    {
        // Make a safe copy of the input costs to avoid in-place overwrite issues
        List<float> safeCosts = new List<float>( inputCosts );

        List<ItemType> result = new List<ItemType>();

        // Make sure the ref list is usable
        if( condensedCosts == null )
            condensedCosts = new List<float>();
        else
            condensedCosts.Clear();

        for( int i = 0; i < items.Count; i++ )
        {
            ItemType type = items[ i ];
            float cost = safeCosts[ i ];

            int index = result.IndexOf( type );
            if( index >= 0 )
            {
                condensedCosts[ index ] += cost;
            }
            else
            {
                result.Add( type );
                condensedCosts.Add( cost );
            }
        }

        return result;
    }



    public static bool WriteToDesktop( string data, string file = "" )
    {
        string fileName = "My Log.txt";
        if( file != "" ) fileName = file;
        bool retValue = false;
        string dataPath = System.Environment.GetFolderPath( System.Environment.SpecialFolder.DesktopDirectory ) + "\\";
        if( !Directory.Exists( dataPath ) )
        {
            Directory.CreateDirectory( dataPath );
        }
        dataPath += fileName;
        try
        {
            System.IO.File.WriteAllText( dataPath, data );
            retValue = true;
        }
        catch( System.Exception ex )
        {
            string ErrorMessages = "File Write Error\n" + ex.Message;
            retValue = false;
            Debug.LogError( ErrorMessages );
        }
        return retValue;
    }

    public static bool IntToBool( int p )
    {
        if( p == 0 ) return false;
        return true;
    }
    public static int BoolToInt( bool b )
    {
        if( b ) return 1;
        return 0;
    }

    public static int GetOne( int br )
    {
        if( br > 0 ) return 1;
        return 0;
    }

    public static Vector2 GetDirectionCord( EDirection dir )
    {
        if( dir == EDirection.NW ) return new Vector2( 0, 0 );
        if( dir == EDirection.N ) return new Vector2( 1, 0 );
        if( dir == EDirection.NE ) return new Vector2( 2, 0 );
        if( dir == EDirection.E ) return new Vector2( 0, 1 );
        if( dir == EDirection.CENTER ) return new Vector2( 1, 1 );
        if( dir == EDirection.W ) return new Vector2( 2, 1 );
        if( dir == EDirection.SW ) return new Vector2( 0, 2 );
        if( dir == EDirection.S ) return new Vector2( 1, 2 );
        if( dir == EDirection.SE ) return new Vector2( 2, 2 );
        return new Vector2( -1, -1 );
    }

    internal static int GetCordId( Vector2 pt )
    {
        if( pt == new Vector2( 0, 0 ) ) return 0;
        if( pt == new Vector2( 1, 0 ) ) return 1;
        if( pt == new Vector2( 2, 0 ) ) return 2;
        if( pt == new Vector2( 0, 1 ) ) return 3;
        if( pt == new Vector2( 1, 1 ) ) return 4;
        if( pt == new Vector2( 2, 1 ) ) return 5;
        if( pt == new Vector2( 0, 2 ) ) return 6;
        if( pt == new Vector2( 1, 2 ) ) return 7;
        if( pt == new Vector2( 2, 2 ) ) return 8;
        return -1;
    }

    public static int GetBiggestValID( List<float> tlist )
    {
        int id = -1;
        float best = float.MinValue;
        for( int i = 0; i < tlist.Count; i++ )
        {
            if( tlist[ i ] > best )
            {
                id = i;
                best = tlist[ i ];
            }
        }
        return id;
    }
    public static Vector3 GetRotationAngleVector( EDirection dr )
    {
        Vector3 res = Vector3.zero;
        switch( dr )
        {
            case EDirection.N: res = new Vector3( 0.0f, 0.0f, 0.0f ); break;
            case EDirection.NE: res = new Vector3( 0.0f, 0.0f, 315.0f ); break;
            case EDirection.E: res = new Vector3( 0.0f, 0.0f, 270.0f ); break;
            case EDirection.SE: res = new Vector3( 0.0f, 0.0f, 225.0f ); break;
            case EDirection.S: res = new Vector3( 0.0f, 0.0f, 180.0f ); break;
            case EDirection.SW: res = new Vector3( 0.0f, 0.0f, 135.0f ); break;
            case EDirection.W: res = new Vector3( 0.0f, 0.0f, 90.0f ); break;
            case EDirection.NW: res = new Vector3( 0.0f, 0.0f, 45.0f ); break;
        }
        return res;
    }

    public static float GetRotationAngle( EDirection dr )
    {
        float res = -1;
        switch( dr )
        {
            case EDirection.N: res = 0.0f; break;
            case EDirection.NE: res = 315.0f; break;
            case EDirection.E: res = 270.0f; break;
            case EDirection.SE: res = 225.0f; break;
            case EDirection.S: res = 180.0f; break;
            case EDirection.SW: res = 135.0f; break;
            case EDirection.W: res = 90.0f; break;
            case EDirection.NW: res = 45.0f; break;
        }
        return res;
    }
    public static int CountTrueInBoolList( List<bool> list )
    {
        int count = 0;
        for( int i = 0; i < list.Count; i++ )
            if( list[ i ] ) count++;
        return count;
    }

    public static EMoveType GetMoveType( Unit un, Vector2 tg )
    {
        if( IsNeighbor( un.Pos, tg ) == false ) Debug.LogError( "No Neighbor." );
        int times = 0;
        for( int tm = 0; tm <= 4; tm++ )
        {
            times = tm;
            int dr = ( int ) Util.RotateDir( ( int ) un.Dir, tm );
            Vector2 frontright = un.Pos + Manager.I.U.DirCord[ dr ];
            dr = ( int ) Util.RotateDir( ( int ) un.Dir, -tm );
            Vector2 frontleft = un.Pos + Manager.I.U.DirCord[ dr ];
            if( tg == frontright || tg == frontleft ) break;
        }
        return ( EMoveType ) times;
    }
    public static EMoveType GetMoveType( Vector2 from, Vector2 to )
    {
        if( IsNeighbor( from, to ) == false ) Debug.LogError( "No Neighbor." );

        EDirection drr = GetTargetUnitDir( from, to );
        int times = 0;
        for( int tm = 0; tm <= 4; tm++ )
        {
            times = tm;
            int dr = ( int ) Util.RotateDir( ( int ) drr, tm );
            Vector2 frontright = from + Manager.I.U.DirCord[ dr ];
            dr = ( int ) Util.RotateDir( ( int ) drr, -tm );
            Vector2 frontleft = from + Manager.I.U.DirCord[ dr ];
            if( to == frontright || to == frontleft ) break;
        }
        return ( EMoveType ) times;
    }

    public static bool EqualPos( Vector3 v1, Vector2 v2 )   // excludes Z
    {
        if( v1.x == v2.x )
            if( v1.y == v2.y ) return true;
        return false;
    }
    public static Vector3 AttribPos( Vector2 vec, Vector3 tg )
    {
        return new Vector3( vec.x, vec.y, tg.z );
    }

    public static bool CheckBlock( Vector2 from, Vector2 to, Unit un, bool wall, bool gaia, bool arrow, bool monsterl, bool gaia2, bool fly, List<ETileType> except )
    {
        if( wall )
            if( Map.IsWall( to, true ) ) return true;

        if( gaia )
            if( Map.I.Gaia[ ( int ) to.x, ( int ) to.y ] != null )
                if( Map.I.Gaia[ ( int ) to.x, ( int ) to.y ].BlockMovement )
                    if( except == null ||
                        except.Contains( Map.I.Gaia[ ( int ) to.x, ( int ) to.y ].TileID ) == false )
                        return true;

        if( arrow )
            if( Map.I.CheckArrowBlockFromTo( from, to, un ) ) return true;
        if( monsterl )
            if( Map.I.Unit[ ( int ) to.x, ( int ) to.y ] != null )
                if( except == null ||
                    except.Contains( Map.I.Unit[ ( int ) to.x, ( int ) to.y ].TileID ) == false )
                    return true;
        if( gaia2 )
            if( Map.I.Gaia2[ ( int ) to.x, ( int ) to.y ] != null )
                if( except == null ||
                    except.Contains( Map.I.Gaia2[ ( int ) to.x, ( int ) to.y ].TileID ) == false )
                    return true;

        if( fly )
            if( Map.I.FUnit[ ( int ) to.x, ( int ) to.y ] != null )
                if( Map.I.FUnit[ ( int ) to.x, ( int ) to.y ].Count > 0 )
                    for( int i = 0; i < Map.I.FUnit[ ( int ) to.x, ( int ) to.y ].Count; i++ )
                        if( Map.I.FUnit[ ( int ) to.x, ( int ) to.y ][ i ].ValidMonster ) return true;
        return false;
    }

    public static EDirection GetRelativeDirection( int times, EDirection dr )
    {
        int shift = ( int ) ( dr + times );
        if( shift < 0 ) shift += 8;
        if( shift < 0 ) shift += 8;
        if( shift >= 8 ) shift -= 8;
        if( shift >= 8 ) shift -= 8;
        return ( EDirection ) ( shift );
    }

    public static List<Vector2> GetDirectionalSectorLoopCords( EDirection dir )
    {
        Sector s = Map.I.RM.HeroSector;
        List<Vector2> tgl = new List<Vector2>();
        if( dir == EDirection.S || dir == EDirection.SW || dir == EDirection.W )
            for( int y = ( int ) s.Area.yMin; y < s.Area.yMax; y++ )
                for( int x = ( int ) s.Area.xMin; x < s.Area.xMax; x++ )
                    if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
                        tgl.Add( new Vector2( x, y ) );

        if( dir == EDirection.N || dir == EDirection.NW )
            for( int y = ( int ) s.Area.yMax - 1; y >= ( int ) s.Area.yMin; y-- )
                for( int x = ( int ) s.Area.xMin; x < s.Area.xMax; x++ )
                    if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
                        tgl.Add( new Vector2( x, y ) );

        if( dir == EDirection.E || dir == EDirection.SE )
            for( int y = ( int ) s.Area.yMin; y < s.Area.yMax; y++ )
                for( int x = ( int ) s.Area.xMax - 1; x >= ( int ) s.Area.xMin; x-- )
                    if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
                        tgl.Add( new Vector2( x, y ) );

        if( dir == EDirection.NE )
            for( int y = ( int ) s.Area.yMax - 1; y >= ( int ) s.Area.yMin; y-- )
                for( int x = ( int ) s.Area.xMax - 1; x >= ( int ) s.Area.xMin; x-- )
                    if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
                        tgl.Add( new Vector2( x, y ) );

        return tgl;
    }

    public static List<Unit> GetCubeUnit( ETileType tl, ELayerType layer, bool addresting = false )
    {
        List<Unit> ul = new List<Unit>();
        Sector hs = Map.I.RM.HeroSector;
        for( int y = ( int ) hs.Area.yMin - 1; y < hs.Area.yMax + 1; y++ )
        for( int x = ( int ) hs.Area.xMin - 1; x < hs.Area.xMax + 1; x++ )
            {
                Unit un = Map.I.GetUnit( new Vector2( x, y ), layer );
                if( un ) 
                if( un.TileID == tl ) 
                if( un.Control.Resting == false ||
                    addresting == true ) ul.Add( un );
            }
        return ul;
    }
    public static List<Unit> GetCubeFUnit( ETileType tl, bool addresting = false )
    {
        List<Unit> ul = new List<Unit>();
        Sector hs = Map.I.RM.HeroSector;
        for( int y = ( int ) hs.Area.yMin - 1; y < hs.Area.yMax + 1; y++ )
        for( int x = ( int ) hs.Area.xMin - 1; x < hs.Area.xMax + 1; x++ )
        if( Map.PtOnMap( Map.I.Tilemap, new Vector2( x, y ) ) )
        if ( Map.I.FUnit[ x, y ] != null )
        {
            for( int i = 0; i < Map.I.FUnit[ x, y ].Count; i++ )
            {
                Unit un = Map.I.FUnit[ x, y ][ i ];
                if( un )
                if( un.TileID == tl )
                if( un.Control.Resting == false ||
                    addresting == true ) ul.Add( un );
            }
        }
        return ul;
    }

    internal static float Round( float p )
    {
        float fc = ( float ) Mathf.Round( p * 10000f ) / 10000f;
        return fc;
    }
    public static bool InRangeofArc( EDirection originalDir, EDirection tgAng, int arc )
    {
        for( int tm = 0; tm <= arc; tm++ )
        {
            int dr = ( int ) Util.RotateDir( ( int ) originalDir, tm );
            if( dr == ( int ) tgAng ) return true;
            dr = ( int ) Util.RotateDir( ( int ) originalDir, -tm );
            if( dr == ( int ) tgAng ) return true;
        }
        return false;
    }
    public static string GetPlural( int v )
    {
        if( v > 1 ) return "s";
        return "";
    }
    public static string GetEnabled( bool p )
    {
        if( p ) return " Enabled";
        return "Disabled";
    }

    internal static void SmoothRotate( Transform transform, EDirection dir, float fact )
    {
        Quaternion currentRot = transform.rotation;
        Quaternion targerRot = Quaternion.Euler( new Vector3( 0, 0, Util.GetRotationAngle( dir ) ) );
        Quaternion smoothRot = Quaternion.Slerp( currentRot, targerRot, Time.deltaTime * fact );
        transform.rotation = smoothRot;
    }

    public static void PlayParticleFX( string nm, Vector3 pos )
    {
        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( nm );
        tr.position = new Vector3( pos.x, pos.y, -6 );
        tr.eulerAngles = new Vector3( 0, 0, Random.Range( 0, 360 ) );
        ParticleSystem pr = tr.gameObject.GetComponent<ParticleSystem>();
        pr.Stop();
        pr.Play();
    }
    public static bool IsBoxOverlappingCircle( BoxCollider2D box, CircleCollider2D circle )
    {
        Bounds boxBounds = box.bounds;

        Vector2 circleCenter = circle.transform.TransformPoint( circle.offset );
        float circleRadius = circle.radius * circle.transform.lossyScale.x; // Assume escala uniforme

        // Encontra o ponto mais próximo do centro do círculo dentro do retângulo
        float closestX = Mathf.Clamp( circleCenter.x, boxBounds.min.x, boxBounds.max.x );
        float closestY = Mathf.Clamp( circleCenter.y, boxBounds.min.y, boxBounds.max.y );
        Vector2 closestPoint = new Vector2( closestX, closestY );

        // Calcula a distância até o centro do círculo
        float distanceSqr = ( closestPoint - circleCenter ).sqrMagnitude;

        return distanceSqr < circleRadius * circleRadius;
    }
    public static int Conv( int indiceAntigo )                          // convert tile from old tilemap brush size of 16 x 128 to 128 x 128
    {
        //return indiceAntigo;
        int larguraAntiga = 16;
        int larguraNova = 128;
        int linha = indiceAntigo / larguraAntiga;
        int coluna = indiceAntigo % larguraAntiga;
        int indiceNovo = linha * larguraNova + coluna;
        return indiceNovo;
    }

    public static Vector2 ReflectPosition( Vector2 oldPos, Vector2 newPos )
    {
        // Passo 1: Determinar os tiles de origem e destino (arredondados para o centro dos tiles)
        float oldTileX = Mathf.Round(oldPos.x);
        float oldTileY = Mathf.Round(oldPos.y);
        float blockedTileX = Mathf.Round(newPos.x);
        float blockedTileY = Mathf.Round(newPos.y);

        // Passo 2: Calcular a direção do movimento
        float dirX = blockedTileX - oldTileX;
        float dirY = blockedTileY - oldTileY;

        // Passo 3: Calcular as bordas do tile de origem na direção do movimento
        float edgeX = oldTileX + 0.5f * Mathf.Sign(dirX); // Borda em X
        float edgeY = oldTileY + 0.5f * Mathf.Sign(dirY); // Borda em Y

        // Passo 4: Calcular o excesso além da borda
        float excessX = (dirX != 0f) ? (newPos.x - edgeX) : 0f;
        float excessY = (dirY != 0f) ? (newPos.y - edgeY) : 0f;

        // Passo 5: Refletir o excesso para corrigir a posição
        float correctedX = (dirX != 0f) ? (edgeX - excessX) : newPos.x;
        float correctedY = (dirY != 0f) ? (edgeY - excessY) : newPos.y;

        return new Vector2( correctedX, correctedY );
    }
    
    public static int Animate( float time, int frames )  // animates using only time.delta time
    {
       return Mathf.FloorToInt( Time.time / time ) % frames;
    }
    public static float RandSig( float val )
    {
        if( Chance( 50 ) ) return val;
        return -1 * val;
    }
    public static bool ValidDir( EDirection dr )
    {
        if( dr < EDirection.N || dr > EDirection.NW ) return false;
        return true;
    }
   public static int ReturnNeighborID( int currentId, int direction )
    {
        return ( currentId + direction + 8 ) % 8;
    }
   public static void RemoveDuplicates<T>( List<T> list )
   {
       HashSet<T> seen = new HashSet<T>();
       int writeIndex = 0;

       for( int readIndex = 0; readIndex < list.Count; readIndex++ )
       {
           T item = list[ readIndex ];
           if( !seen.Contains( item ) )
           {
               seen.Add( item );
               list[ writeIndex ] = item;
               writeIndex++;
           }
       }

       // Remove the extra elements at the end
       if( writeIndex < list.Count )
       {
           list.RemoveRange( writeIndex, list.Count - writeIndex );
       }
   }
   public static bool IsPosInFront( Vector2 from, Vector2 to, Vector2 target )  // target in front of movement?
   {
       Vector2 dir = from - to;
       for( int i = 0; i < Sector.TSX; i++ )
       {
           Vector2 tg = from - dir * i;
           if( Sector.GetPosSectorType( tg ) == Sector.ESectorType.GATES ) return false;
           if( tg == target )
               return true;
       }
       return false;
   }
   public static EDirection GetVectorDir( Vector2 vc )
   {
       if( vc.x == 0 && vc.y == 1 ) return EDirection.N;        // norte
       if( vc.x == 0 && vc.y == -1 ) return EDirection.S;       // sul
       if( vc.x == 1 && vc.y == 0 ) return EDirection.E;        // leste
       if( vc.x == -1 && vc.y == 0 ) return EDirection.W;       // oeste
       if( vc.x == 1 && vc.y == 1 ) return EDirection.NE;       // nordeste
       if( vc.x == -1 && vc.y == 1 ) return EDirection.NW;      // noroeste
       if( vc.x == 1 && vc.y == -1 ) return EDirection.SE;      // sudeste
       if( vc.x == -1 && vc.y == -1 ) return EDirection.SW;     // sudoeste
       return EDirection.NONE;                                  // caso não seja nenhuma direção válida
   }

   public static bool HasDada<T>( List<T> list )                 // list populated?
   {
       return list != null && list.Count > 0;
   }
   public static bool HasDataArray<T>( T[] array )                    // array populado?
   {
       return array != null && array.Length > 0;
   }
   internal static object GetFathers( GameObject gameObject )
   {
       Transform t = gameObject.transform;
       string path = t.name;
       while( t.parent != null ) { t = t.parent; path = t.name + "/" + path; }
       return path;
   }
   public static bool VID<T>( List<T> list, int id )                // Valid ID on the list?
   {
       if( list == null || list.Count == 0 )
           return false;
       if( id < 0 || id > list.Count - 1 )
           return false;
       return true;
   }
   public static bool VID<T>( T[] array, int id )                  // Valid ID in the array?
   {
       if( array == null || array.Length == 0 )
           return false;
       if( id < 0 || id > array.Length - 1 )
           return false;
       return true;
   }
   public static void DebugList<T>( List<T> list, string label = "" )
   {
       if( list == null )
       {
           Debug.Log( label + " -> Lista nula" );
           return;
       }

       Debug.Log( label + " -> Lista contém " + list.Count + " itens:" );
       for( int i = 0; i < list.Count; i++ )
       {
           T item = list[ i ];
           if( item == null )
           {
               Debug.Log( "[" + i + "] -> null" );
           }
           else
           {
               // Usa ToString(), se for uma classe você pode sobrescrever ToString() para detalhar
               Debug.Log( "[" + i + "] -> " + item );
           }
       }
   }
   public static void ShuffleList<T>( List<T> list )
   {
       for( int i = 0; i < list.Count; i++ )
       {
           int j = Random.Range( i, list.Count );
           T tmp = list[ i ];
           list[ i ] = list[ j ];
           list[ j ] = tmp;
       }
   }
   public static float GetCurveVal( float id, float totPoints, float startValue, float endValue, float curve, float afterEndCurveMult = 1 )
   {
       // return base value if id is zero; prevents invalid zero output
       if( id <= 0f ) return startValue;

       // protection for invalid division; if total points too low, just return start value
       if( totPoints <= 1f ) return startValue;

       // handle curve multiplier when going beyond total points
       if( id > totPoints )
       {
           curve *= afterEndCurveMult; // apply external curve multiplier after end
           if( afterEndCurveMult == 0 ) return endValue; // if multiplier is zero, clamp to end
       }

       // safety: if curve is invalid (NaN or Infinity), reset to 1
       if( float.IsNaN( curve ) || float.IsInfinity( curve ) ) curve = 1f;

       // normalize range; id = 0 → t = 0, id = totPoints → t = 1
       float denom = totPoints; // use totPoints instead of totPoints - 1 to make level 1 already progress
       float t = Mathf.Clamp01( id / denom ); // ensure value stays in 0..1 range

       // apply curve; this makes the growth non-linear depending on curve exponent
       float curvedT = Mathf.Pow( t, curve );

       // interpolate between start and end values using curvedT
       return startValue + ( endValue - startValue ) * curvedT;
   }


   public static List<Vector2> CloneToVector2List( List<VI> source )
   {
       if( source == null ) return null;
       List<Vector2> result = new List<Vector2>( source.Count );
       for( int i = 0; i < source.Count; i++ )
       {
           result.Add( source[ i ].ToVector2() );
       }
       return result;
   }
}

[System.Serializable]
public class Timer {
	
	public float Target, Elapsed, Remaining;
	public bool bActive;
	public Timer() {}
	public void Reset() { Target = Elapsed = Remaining = 0; bActive = false; }
	
	public void Start( float target ) {
		Reset ();
		bActive = true; 
		Target = target;
		Remaining = target;
	} 
	
	public bool Done() {
		if( Elapsed >= Target ) return true; 
		return false;
	}

	public bool Update() {
		if( bActive )
		{
			Elapsed += Time.deltaTime;
			Remaining -= Time.deltaTime;
			if( Elapsed >= Target ) { return true; }
		}
		return false;
	}
}

