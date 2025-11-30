using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class G
{
    public static Farm Farm = Map.I.Farm;
    public static Inventory Packmule = Manager.I.PackMule;
    public static Inventory Inventory = Manager.I.Inventory;
    public static Unit Hero = Map.I.Hero;
    public static Tutorial Tutorial = Manager.I.Tutorial;
    public static Sector HS = null;

    public static void Init()
    {
        Farm = Map.I.Farm;
        Packmule = Manager.I.PackMule;
        Inventory = Manager.I.Inventory;
        Hero = Map.I.Hero;
        Tutorial = Manager.I.Tutorial;
    }

    public static void Deb( object txt )
    {
        Debug.Log( txt );
    }

    public static void Error( object txt )
    {
        Debug.LogError( txt );
        Application.Quit();
    }
        
    public static string[ ] LineSplit = null;
    public static int CL = 0;
    public static string Txt = "";
    public static Vector2 MP;

    public static float GF()
    {
        CL++;
        string st = LineSplit[ CL - 1 ];
        var words = st.Split( ' ' );
        return float.Parse( words[ 0 ] );
    }

    public static int GI()
    {
        CL++;
        string st = LineSplit[ CL - 1 ];
        var words = st.Split( ' ' );
        return int.Parse( words[ 0 ] );
    }

    public static bool GB()
    {
        CL++;
        string st = LineSplit[ CL - 1 ];
        var words = st.Split( ' ' );
        if( int.Parse( words[ 0 ] ) == 1 ) return true;
        return false;
    }

    public static string SB( bool var )
    {
        if( var == true ) return "1";
        return "0";
    }
    public static List<float> GLF( int sz )
    {
        CL++;
        string st = LineSplit[ CL - 1 ];
        var words = st.Split( ' ' );
        List<float> il = new List<float>();
        for( int i = 0; i < sz; i++ )
        {
            il.Add( int.Parse( words[ i ] ) );
        }
        return il;
    }
    public static List<int> GLI( int sz )
    {
        CL++;
        string st = LineSplit[ CL - 1 ];
        var words = st.Split( ' ' );
        List<int> il = new List<int>();
        for( int i = 0; i < sz; i++ )
        {
            il.Add( int.Parse( words[ i ] ) );
        }
        return il;
    }

    public static int[ , ] GLF2d( Vector2 sz )
    {
        List<int> ls = new List<int>();
        int[ , ] res = new int[ ( int ) sz.x, ( int ) sz.y ];
        for( int y = 0; y < ( int ) sz.y; y++ )
        {
            ls = GLI( ( int ) sz.x );
            for( int x = 0; x < ( int ) sz.x; x++ )
                res[ x, y ] = ls[ x ];
        }
        return res;
    }
    public static Vector2 GV2()
    {
        CL++;
        string st = LineSplit[ CL - 1 ];
        var words = st.Split( ' ' );
        return new Vector2( int.Parse( words[ 0 ] ), int.Parse( words[ 1 ] ) );
    }

    public static long GL()
    {
        CL++;
        string st = LineSplit[ CL - 1 ];
        var words = st.Split( ' ' );
        return long.Parse( words[ 0 ] );
    }
    public static Vector3 GetHpos( float z = -5f )                          // fow sounds that are too low
    {
        return new Vector3( Hero.Pos.x, Hero.Pos.y, z );
    }

    internal static Item GIT( int id )
    {
        ItemType type = ( ItemType ) id;                                   // Converte int para enum
        return GIT( type );                                                // Chama a função original
    }
    internal static Item GIT( ItemType type )
    {
        if( Application.isPlaying == false )                               // to avoid in editor bug. Dictionary not built
        {
            for( int i = 0; i < Manager.I.Inventory.ItemList.Count; i++ )                         
            if( Manager.I.Inventory.ItemList[ i ] )
            if( type == Manager.I.Inventory.ItemList[ i ].Type )
            return Manager.I.Inventory.ItemList[ i ];     
        }
        
        if( Inventory.ItemList[ ( int ) type ] == null ) 
            return null;

        if( Inventory.Dic.ContainsKey( type ) )
        {
            int index = Inventory.Dic[ type ];                              // Pega o índice do item na lista mestre
            if( index >= 0 && index < Inventory.ItemList.Count )
                return Inventory.ItemList[ index ];                         // Retorna o item
        }
        Debug.LogError( "Bad Item ID" );
        return null;                                                        // Retorna nulo se não existir
    }
}
